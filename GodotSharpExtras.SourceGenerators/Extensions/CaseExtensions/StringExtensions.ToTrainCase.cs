using System;

namespace GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;

internal static partial class StringExtensions
{
    public static string ToTrainCase(this string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return SymbolsPipe(
            source,
            '-',
            (s, disableFrontDelimeter) =>
            {
                if (disableFrontDelimeter)
                {
                    return [char.ToUpperInvariant(s)];
                }

                return ['-', char.ToUpperInvariant(s)];
            }
        );
    }
}
