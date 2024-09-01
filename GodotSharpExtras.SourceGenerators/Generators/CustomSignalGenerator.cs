using System;
using System.Threading;
using Godot;
using GodotSharpExtras.SourceGenerators.Abstractions;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Generators;

[Generator]
public class CustomSignalGenerator : SourceGeneratorForDelegateWithAttribute<SignalAttribute>
{
    protected override string Id => nameof(CustomSignalGenerator);

    protected override string GenerateCode(
        Compilation compilation,
        DelegateDeclarationSyntax node,
        INamedTypeSymbol symbol,
        SignalAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        var source = new SourceStringBuilder(symbol.ContainingType);
        source.Line("// ", symbol.Name);
        return source.ToString();
    }

    protected override bool IsSyntaxTarget(SyntaxNode node, CancellationToken _) =>
        node is DelegateDeclarationSyntax delegateDeclarationSyntax
        && delegateDeclarationSyntax.Identifier.Text.EndsWith(
            "EventHandler",
            StringComparison.InvariantCulture
        );
}
