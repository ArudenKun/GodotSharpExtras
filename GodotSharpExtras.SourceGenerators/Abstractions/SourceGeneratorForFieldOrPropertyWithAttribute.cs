using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Abstractions;

public abstract class SourceGeneratorForFieldOrPropertyWithAttribute<TAttribute>
    : SourceGeneratorForMemberWithAttribute<TAttribute, VariableDeclarationSyntax>
    where TAttribute : Attribute
{
    protected abstract string GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        VariableSymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    );

    protected sealed override string GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        ITypeSymbol typeSymbol;
        ISymbol selfSymbol;
        if (symbol is IFieldSymbol fieldSymbol)
        {
            typeSymbol = fieldSymbol.Type;
            selfSymbol = fieldSymbol;
        }
        else
        {
            typeSymbol = ((IPropertySymbol)symbol).Type;
            selfSymbol = (IPropertySymbol)symbol;
        }

        return GenerateCode(
            compilation,
            node,
            new VariableSymbol(symbol.Name, typeSymbol, symbol.ContainingType, selfSymbol),
            attribute,
            options
        );
    }

    protected readonly record struct VariableSymbol(
        string Name,
        ITypeSymbol Type,
        INamedTypeSymbol ContainingType,
        ISymbol Self
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

    protected override SyntaxNode Node(VariableDeclarationSyntax node)
    {
        if (
            node.Parent is VariableDeclarationSyntax
            {
                Parent: FieldDeclarationSyntax fieldDeclarationSyntax
            }
        )
        {
            return fieldDeclarationSyntax.Declaration.Variables.Single();
        }

        return node;
    }
}
