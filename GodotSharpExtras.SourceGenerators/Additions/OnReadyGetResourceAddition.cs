using System;
using GodotSharpExtras.SourceGenerators.Additions.Abstractions;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Models;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis.CSharp;

namespace GodotSharpExtras.SourceGenerators.Additions;

internal class OnReadyGetResourceAddition : OnReadyGetAddition
{
    public OnReadyGetResourceAddition(MemberAttributeSite site)
        : base(site) { }

    private bool IsGeneratingAssignment => Path.IsNotNullOrEmpty();
    private string ResourceName => PascalName + "Resource";

    public override Action<SourceStringBuilder> DeclarationWriter =>
        g =>
        {
            g.Line("public ", Variable.Type.ToFullDisplayString(), " ", ResourceName);

            var setHasBeenSet = IsGeneratingAssignment
                ? "_hasBeenSet" + Variable.Name + " = true; "
                : "";

            g.BlockBrace(() =>
            {
                g.Line("get => ", Variable.Name, ";");
                g.Line("set { ", setHasBeenSet, Variable.Name, " = value; }");
            });

            if (IsGeneratingAssignment)
            {
                g.Line("private bool _hasBeenSet", Variable.Name, ";");
            }
        };

    public override Action<SourceStringBuilder>? ConstructorStatementWriter =>
        IsGeneratingAssignment
            ? g =>
            {
                g.Line("if (Engine.IsEditorHint())");
                g.BlockBrace(() =>
                {
                    WriteAssignment(g);
                });
                g.Line();
            }
            : null;

    public override Action<SourceStringBuilder>? ReadyStatementWriter =>
        IsGeneratingAssignment || !OrNull
            ? g =>
            {
                if (IsGeneratingAssignment)
                {
                    g.Line("if (!_hasBeenSet", Variable.Name, ")");
                    g.BlockBrace(() =>
                    {
                        WriteAssignment(g);
                    });
                }

                if (!OrNull)
                {
                    WriteMemberNullCheck(g, ResourceName);
                }
            }
            : null;

    private void WriteAssignment(SourceStringBuilder g)
    {
        g.Line(
            Variable.Name,
            " = GD.Load",
            "<",
            Variable.Type.ToFullDisplayString(),
            ">",
            "(",
            SyntaxFactory.Literal(Path ?? "").ToString(),
            ");"
        );
    }
}
