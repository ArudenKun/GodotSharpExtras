using System;

namespace GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;

internal static partial class StringExtensions
{
    public static string ToCamelCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '\0',
            (s, disableFrontDelimeter) =>
            {
                if (disableFrontDelimeter)
                {
                    return [char.ToLowerInvariant(s)];
                }

                return [char.ToUpperInvariant(s)];
            }
        );
    }
}
