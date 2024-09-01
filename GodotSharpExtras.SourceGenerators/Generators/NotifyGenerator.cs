using System.Collections.Generic;
using GodotSharpExtras.SourceGenerators.Abstractions;
using GodotSharpExtras.SourceGenerators.Attributes;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Utilities;
using H;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Generators;

[Generator]
public sealed class NotifyGenerator
    : SourceGeneratorForFieldOrPropertyWithAttribute<NotifyAttribute>
{
    protected override string Id => nameof(NotifyGenerator);

    protected override IEnumerable<FileWithName> StaticSources { get; } =
        [new(typeof(NotifyAttribute).FullName!, Resources.NotifyAttribute_cs.AsGeneratedString())];

    // protected override string GenerateCode(
    //     Compilation compilation,
    //     SyntaxNode node,
    //     IPropertySymbol symbol,
    //     NotifyAttribute attribute,
    //     AnalyzerConfigOptions options
    // )
    // {
    //     var source = new SourceStringBuilder(symbol);
    //
    //     source.Line("// ", symbol.Name);
    //
    //     return source.ToString();
    // }
    protected override string GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        VariableSymbol symbol,
        NotifyAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        var source = new SourceStringBuilder(symbol.ContainingType);
        source.Line("// ", symbol.Name);
        return source.ToString();
    }
}
