using System;
using GodotSharpExtras.SourceGenerators.Additions.Abstractions;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Models;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Additions;

internal class OnReadyGetNodeAddition : OnReadyGetAddition
{
    public OnReadyGetNodeAddition(Compilation compilation, MemberAttributeSite site)
        : base(site) { }

    private string NodePathName => PascalName + "Path";

    public override Action<SourceStringBuilder> DeclarationWriter =>
        g =>
        {
            var value = Path.IsNullOrEmpty() ? Path : UniqueName;
            g.Line("public NodePath ", NodePathName, " { get; }", " = ", $"\"{value}\";");
        };

    public override Action<SourceStringBuilder> ReadyStatementWriter =>
        g =>
        {
            g.Line("if (", NodePathName, " != null)");
            g.BlockBrace(() =>
            {
                {
                    g.Line(
                        Variable.Name,
                        " = GetNode" + (OrNull ? "OrNull" : "") + "<",
                        Variable.Type.ToFullDisplayString(),
                        ">" + "(",
                        NodePathName,
                        ");"
                    );
                }
            });

            if (!OrNull)
            {
                WriteMemberNullCheck(g, NodePathName);
            }
        };
}
