using System.Collections.Generic;
using System.Linq;

namespace GodotSharpExtras.SourceGenerators.Extensions;

internal static class LinqExtensions
{
    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) =>
        source.TryGetValue(key, out var value) ? value : default;

    public static IEnumerable<(int Index, T Value)> WithIndex<T>(this IEnumerable<T>? self) =>
        self?.Select((item, index) => (index, item)) ?? [];

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? self) => self is null || !self.Any();

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? self) => !IsNullOrEmpty(self);
}
