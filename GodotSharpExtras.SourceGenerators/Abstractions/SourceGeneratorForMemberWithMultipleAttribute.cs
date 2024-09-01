using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using GodotSharpExtras.SourceGenerators.Extensions;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Abstractions;

public abstract class SourceGeneratorForMemberWithMultipleAttribute<TAttribute, TDeclarationSyntax>
    : IIncrementalGenerator
    where TAttribute : Attribute
    where TDeclarationSyntax : MemberDeclarationSyntax
{
    protected abstract string Id { get; }
    private static readonly string AttributeType = typeof(TAttribute).Name;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly string AttributeName = Regex.Replace(
        AttributeType,
        "Attribute$",
        "",
        RegexOptions.Compiled
    );

    protected virtual IEnumerable<FileWithName> StaticSources => [];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var (name, source) in StaticSources)
        {
            context.RegisterPostInitializationOutput(x => x.AddSource($"{name}.g.cs", source));
        }

        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            IsSyntaxTarget,
            GetSyntaxTarget
        );

        context
            .CompilationProvider.Combine(syntaxProvider.Collect())
            .Combine(context.AnalyzerConfigOptionsProvider)
            .SelectAndReportExceptions(
                (tuple, ct) => OnExecute(tuple.Left.Left, tuple.Left.Right, tuple.Right, ct),
                context,
                Id
            )
            .AddSource(context);
    }

    private static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
    {
        return node is TDeclarationSyntax type && HasAttributeType();

        bool HasAttributeType()
        {
            if (type.AttributeLists.Count is 0)
                return false;

            return type
                .AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                .Any(attribute => attribute.Name.ToString() == AttributeName);
        }
    }

    private static TDeclarationSyntax GetSyntaxTarget(
        GeneratorSyntaxContext context,
        CancellationToken _
    ) => (TDeclarationSyntax)context.Node;

    private IEnumerable<FileWithName> OnExecute(
        Compilation compilation,
        ImmutableArray<TDeclarationSyntax> nodes,
        AnalyzerConfigOptionsProvider options,
        CancellationToken cancellationToken
    )
    {
        foreach (var node in nodes.Distinct())
        {
            if (cancellationToken.IsCancellationRequested)
                continue;

            var model = compilation.GetSemanticModel(node.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(Node(node), cancellationToken: cancellationToken);
            var attribute = symbol
                ?.GetAttributes()
                .Filter<TAttribute>()
                .Select(x => x.MapToType<TAttribute>())
                .ToArray();

            if (attribute is null || symbol is null)
                continue;

            var generatedCode = _GenerateCode(
                compilation,
                node,
                symbol,
                attribute,
                options.GlobalOptions
            );

            if (generatedCode.IsNullOrEmpty())
            {
                continue;
            }

            yield return new FileWithName(GenerateFilename(symbol), generatedCode);
        }
    }

    protected abstract string GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        TAttribute[] attributes,
        AnalyzerConfigOptions options
    );

    private string _GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        TAttribute[] attributes,
        AnalyzerConfigOptions options
    )
    {
        try
        {
            return GenerateCode(compilation, node, symbol, attributes, options);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private const string Ext = ".g.cs";
    private const int MaxFileLength = 255;

    protected virtual string GenerateFilename(ISymbol symbol)
    {
        var gn = $"{Format()}{Ext}";
        return gn;

        string Format() =>
            string.Join(
                    "_",
                    $"{symbol}.{GetType().Name.Replace("Generator", string.Empty)}".Split(
                        Path.GetInvalidPathChars()
                    )
                )
                .Truncate(MaxFileLength - Ext.Length);
    }

    protected virtual SyntaxNode Node(TDeclarationSyntax node) => node;
}
