using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using GodotSharpExtras.SourceGenerators.Extensions;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Abstractions;

public abstract class SourceGeneratorForMemberWithAttribute<TAttribute, TDeclarationSyntax>
    : IIncrementalGenerator
    where TAttribute : Attribute
    where TDeclarationSyntax : CSharpSyntaxNode
{
    protected abstract string Id { get; }

    protected virtual IEnumerable<FileWithName> StaticSources => [];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var (name, source) in StaticSources)
        {
            context.RegisterPostInitializationOutput(x => x.AddSource($"{name}.g.cs", source));
        }

        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(TAttribute).FullName!,
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

    protected virtual bool IsSyntaxTarget(SyntaxNode node, CancellationToken _) =>
        node is TDeclarationSyntax;

    private static GeneratorAttributeSyntaxContext GetSyntaxTarget(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _
    ) => context;

    private IEnumerable<FileWithName> OnExecute(
        Compilation compilation,
        ImmutableArray<GeneratorAttributeSyntaxContext> generatorAttributeSyntaxContexts,
        AnalyzerConfigOptionsProvider options,
        CancellationToken cancellationToken
    )
    {
        foreach (var generatorAttributeSyntaxContext in generatorAttributeSyntaxContexts)
        {
            if (cancellationToken.IsCancellationRequested)
                continue;

            var node = Node((TDeclarationSyntax)generatorAttributeSyntaxContext.TargetNode);
            var symbol = generatorAttributeSyntaxContext.TargetSymbol;
            var attribute = symbol.GetAttributes().First().MapToType<TAttribute>();

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
        TAttribute attribute,
        AnalyzerConfigOptions options
    );

    private string _GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        TAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        try
        {
            return GenerateCode(compilation, node, symbol, attribute, options);
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
