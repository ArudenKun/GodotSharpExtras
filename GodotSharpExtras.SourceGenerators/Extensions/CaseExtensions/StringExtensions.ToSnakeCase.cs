using System;

namespace GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;

internal static partial class StringExtensions
{
    public static string ToSnakeCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '_',
            (s, disableFrontDelimeter) =>
            {
                if (disableFrontDelimeter)
                {
                    return [char.ToLowerInvariant(s)];
                }

                return ['_', char.ToLowerInvariant(s)];
            }
        );
    }
}
