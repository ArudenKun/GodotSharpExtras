using System;

namespace GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;

internal static partial class StringExtensions
{
    public static string ToPascalCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(source, '\0', (s, _) => [char.ToUpperInvariant(s)]);
    }
}
