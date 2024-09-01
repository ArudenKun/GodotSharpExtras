using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Abstractions;

public abstract class SourceGeneratorForFieldOrPropertyWithAttribute<TAttribute>
    : SourceGeneratorForMemberWithAttribute<TAttribute, CSharpSyntaxNode>
    where TAttribute : Attribute
{
    protected abstract string GenerateCode(
        Compilation compilation,
        CSharpSyntaxNode node,
        VariableSymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    );

    protected sealed override string GenerateCode(
        Compilation compilation,
        CSharpSyntaxNode node,
        ISymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        node = node switch
        {
            VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax
                {
                    Parent: FieldDeclarationSyntax fieldDeclarationSyntax
                }
            } => fieldDeclarationSyntax,
            PropertyDeclarationSyntax propertyDeclarationSyntax => propertyDeclarationSyntax,
            _ => throw new InvalidCastException("Unexpected syntax node type"),
        };

        ITypeSymbol typeSymbol;
        ISymbol selfTypeSymbol;
        switch (symbol)
        {
            case IFieldSymbol fieldSymbol:
                typeSymbol = fieldSymbol.Type;
                selfTypeSymbol = fieldSymbol;
                break;
            case IPropertySymbol propertySymbol:
                typeSymbol = propertySymbol.Type;
                selfTypeSymbol = propertySymbol;
                break;
            default:
                throw new InvalidCastException(
                    $"Unsupported symbol type: {symbol.GetType().FullName}"
                );
        }

        return GenerateCode(
            compilation,
            node,
            new VariableSymbol(symbol.Name, typeSymbol, symbol.ContainingType, selfTypeSymbol),
            attribute,
            options
        );
    }

    protected readonly record struct VariableSymbol(
        string Name,
        ITypeSymbol Type,
        INamedTypeSymbol ContainingType,
        ISymbol SelfType
    );

    protected override bool IsSyntaxTarget(SyntaxNode node, CancellationToken _) =>
        node
            is VariableDeclaratorSyntax
                {
                    Parent: VariableDeclarationSyntax
                    {
                        Parent: FieldDeclarationSyntax { AttributeLists.Count: > 0 }
                    }
                }
                or PropertyDeclarationSyntax { AttributeLists.Count: > 0 };
}
