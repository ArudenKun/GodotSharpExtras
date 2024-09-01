using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Abstractions;

public abstract class SourceGeneratorForDelegateWithAttribute<TAttribute>
    : SourceGeneratorForMemberWithAttribute<TAttribute, DelegateDeclarationSyntax>
    where TAttribute : Attribute
{
    protected abstract string GenerateCode(
        Compilation compilation,
        DelegateDeclarationSyntax node,
        INamedTypeSymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    );

    protected sealed override string GenerateCode(
        Compilation compilation,
        DelegateDeclarationSyntax node,
        ISymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    ) => GenerateCode(compilation, node, (INamedTypeSymbol)symbol, attribute, options);
}
