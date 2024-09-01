using System.Collections.Generic;
using GodotSharpExtras.SourceGenerators.Abstractions;
using GodotSharpExtras.SourceGenerators.Attributes;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Utilities;
using H;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GodotSharpExtras.SourceGenerators.Generators;

[Generator]
public sealed class NotifyGenerator : SourceGeneratorForFieldWithAttribute<NotifyAttribute>
{
    protected override string Id => nameof(NotifyGenerator);

    protected override IEnumerable<FileWithName> StaticSources { get; } =
        [new(typeof(NotifyAttribute).FullName!, Resources.NotifyAttribute_cs.AsGeneratedString())];

    protected override string GenerateCode(
        Compilation compilation,
        FieldDeclarationSyntax node,
        IFieldSymbol symbol,
        NotifyAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        var source = new SourceStringBuilder(symbol.ContainingType);
        source.Line("// ", symbol.Name, Space, "TEST", Space, symbol.Type.ToDisplayString());
        return source.ToString();
    }
}
