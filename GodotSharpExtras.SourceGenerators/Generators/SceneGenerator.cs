using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using GodotSharpExtras.SourceGenerators.Additions;
using GodotSharpExtras.SourceGenerators.Additions.Abstractions;
using GodotSharpExtras.SourceGenerators.Attributes;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Models;
using GodotSharpExtras.SourceGenerators.Utilities;
using H;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotSharpExtras.SourceGenerators.Generators;

[Generator]
public sealed class SceneGenerator : IIncrementalGenerator
{
    private const string Id = "SG";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(
                $"{typeof(OnReadyGetAttribute).FullName}.g.cs",
                Resources.OnReadyGetAttribute_cs.AsGeneratedString()
            );
            ctx.AddSource(
                $"{typeof(SignalHandlerAttribute).FullName}.g.cs",
                Resources.SignalHandlerAttribute_cs.AsGeneratedString()
            );
        });

        context
            .SyntaxProvider.CreateSyntaxProvider(IsSyntaxTarget, TransformTarget)
            .Where(x => !x.IsDefault())
            .SelectAndReportExceptions(Generate, context, Id)
            .AddSource(context);
    }

    private FileWithName Generate(Root root, CancellationToken cancellationToken)
    {
        var additions = root.PartialClassAdditions;
        var source = new SourceStringBuilder(root.ClassSymbol);
        source.Line("using Godot;");
        source.Line("using System;");
        source.Line();

        if (root.Nullable)
        {
            source.Line("#nullable disable");
            source.Line();
        }

        source.PartialTypeBlockBrace(() =>
        {
            foreach (var addition in additions)
            {
                addition.DeclarationWriter?.Invoke(source);
            }

            source.Line();
            source.Constructor(() =>
            {
                source.Line("Ready += _InternalReady;");
                source.Line();
                foreach (var addition in additions)
                {
                    addition.ConstructorStatementWriter?.Invoke(source);
                }

                source.Line("Constructor();");
            });

            source.Line();
            source.Line("partial void Constructor();");
            source.Line();
            source.Line("private void _InternalReady()");
            source.BlockBrace(() =>
            {
                foreach (var addition in additions)
                {
                    addition.ReadyStatementWriter?.Invoke(source);
                }
            });

            foreach (var addition in additions)
            {
                addition.OutsideClassStatementWriter?.Invoke(source);
            }
        });

        return new FileWithName(GenerateFilename(root.ClassSymbol), source.ToString());
    }

    private static bool IsSyntaxTarget(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken
    ) => syntaxNode is ClassDeclarationSyntax;

    private static Root TransformTarget(
        GeneratorSyntaxContext generatorSyntaxContext,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var classDeclaration = (ClassDeclarationSyntax)generatorSyntaxContext.Node;
        var classSymbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration);
        var compilation = generatorSyntaxContext.SemanticModel.Compilation;
        var nodeSymbol = compilation.GetTypeByMetadataName("Godot.Node");
        var resourceSymbol = GetSymbolByName("Godot.Resource");
        var onReadyGetSymbol = GetSymbolByName(typeof(OnReadyGetAttribute).FullName);

        if (classSymbol is null || !classSymbol.IsOfBaseType(nodeSymbol))
        {
            return default;
        }

        var nullable =
            compilation is CSharpCompilation csc
            && csc.Options.NullableContextOptions != NullableContextOptions.Disable;

        var additions = ImmutableArray.CreateBuilder<PartialClassAddition>();

        foreach (var member in VariableSymbol.CreateAll(classSymbol).ToArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var attribute in member.Symbol.GetAttributes())
            {
                var site = new MemberAttributeSite(
                    member,
                    new AttributeSite(classSymbol, attribute)
                );

                if (!Equal(attribute.AttributeClass, onReadyGetSymbol))
                    continue;

                if (member.Type.IsOfBaseType(resourceSymbol))
                {
                    additions.Add(new OnReadyGetResourceAddition(site));
                }

                if (member.Type.IsOfBaseType(nodeSymbol))
                {
                    additions.Add(new OnReadyGetNodeAddition(compilation, site));
                }
            }
        }

        var members = classSymbol.GetMembers();

        foreach (var memberSymbol in members.OfType<INamedTypeSymbol>())
        {
            if (
                memberSymbol.HasAttribute("SignalAttribute")
                && memberSymbol.Name.EndsWith("EventHandler")
            )
            {
                additions.Add(
                    new CustomSignalAddition(
                        memberSymbol.Name,
                        memberSymbol.DelegateInvokeMethod!,
                        classSymbol
                    )
                );
            }
        }

        foreach (var memberSymbol in members.OfType<IMethodSymbol>())
        {
            if (memberSymbol.HasAttribute<SignalHandlerAttribute>())
            {
                additions.Add(
                    new SignalHandlerAddition(
                        memberSymbol,
                        memberSymbol.GetAttributes().Filter<SignalHandlerAttribute>().ToArray(),
                        classSymbol
                    )
                );
            }
        }

        return new Root(classSymbol, additions.ToImmutable(), nullable);

        INamedTypeSymbol GetSymbolByName(string? fullName) =>
            fullName is null
                ? throw new Exception($"{fullName} is not null")
                : compilation.GetTypeByMetadataName(fullName)
                    ?? throw new Exception($"Can't find {fullName}");
    }

    private static bool Equal(ISymbol? a, ISymbol? b) =>
        SymbolEqualityComparer.Default.Equals(a, b);

    private string GenerateFilename(ISymbol symbol)
    {
        const string ext = ".g.cs";
        const int maxFileLength = 255;
        var gn = $"{Format().SanitizeName()}{ext}";
        return gn;

        string Format() =>
            string.Join(
                    "_",
                    $"{symbol}.{GetType().Name.Replace("Generator", "")}".Split(
                        Path.GetInvalidPathChars()
                    )
                )
                .Truncate(maxFileLength - ext.Length);
    }

    private readonly record struct Root(
        INamedTypeSymbol ClassSymbol,
        ImmutableArray<PartialClassAddition> PartialClassAdditions,
        bool Nullable
    );
}
