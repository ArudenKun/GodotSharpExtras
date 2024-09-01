using System;
using System.Collections.Immutable;
using System.Linq;
using GodotSharpExtras.SourceGenerators.Additions.Abstractions;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Additions;

internal class CustomSignalAddition : PartialClassAddition
{
    public string Name { get; }
    public string[] MethodSignatures { get; }
    public ImmutableArray<IParameterSymbol> ParameterSymbols { get; }
    public string ParametersString { get; }
    public string ParameterTypesString { get; }
    public string PassParametersString { get; }

    public CustomSignalAddition(
        string name,
        IMethodSymbol delegateMethodSymbol,
        INamedTypeSymbol classSymbol
    )
        : base(classSymbol)
    {
        Name = name.Replace("EventHandler", string.Empty);
        MethodSignatures = classSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Select(x =>
                $"{x.Name}({string.Join(", ", x.Parameters.Select(p => p.Type.ToDisplayString()))})"
            )
            .ToArray();
        ParameterSymbols = delegateMethodSymbol.Parameters;
        ParametersString = string.Join(
            ", ",
            ParameterSymbols.Select(x => $"{x.Type.ToDisplayString()} {x.Name}")
        );
        ParameterTypesString = string.Join(
            ", ",
            ParameterSymbols.Select(x => x.Type.ToDisplayString())
        );
        PassParametersString = string.Join(", ", ParameterSymbols.Select(x => $"{x.Name}"));
    }

    public override Action<SourceStringBuilder> DeclarationWriter =>
        g =>
        {
            g.Line();
            g.Line($"partial void OnSignal{Name}({ParametersString});");
            g.Line($"public void EmitSignal{Name}({ParametersString})");
            g.BlockBrace(() =>
            {
                g.Line(
                    "EmitSignal(SignalName.",
                    Name,
                    ParameterSymbols.Length > 0 ? $", {PassParametersString}" : "",
                    ");"
                );
            });
            g.Line("public SignalAwaiter ToSignal", Name, "(GodotObject obj)");
            g.BlockBrace(() =>
            {
                g.Line("return obj.ToSignal(this, SignalName.", Name, ");");
            });
        };

    public override Action<SourceStringBuilder> ReadyStatementWriter =>
        g =>
        {
            var connectString =
                $"Connect(SignalName.{Name}, new Callable(this, MethodName.OnSignal{Name}));";

            if (ParameterSymbols.Length == 0)
            {
                if (!MethodSignatures.Contains($"OnSignal{Name}()"))
                    return;

                g.Line(connectString);
                return;
            }

            if (MethodSignatures.Contains($"OnSignal{Name}({ParameterTypesString})"))
            {
                g.Line(connectString);
            }
        };
}
