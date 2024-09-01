using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Models;

internal record VariableSymbol(ITypeSymbol Type, string Name, ISymbol Symbol)
{
    public static VariableSymbol Create(IPropertySymbol member) =>
        new(member.Type, member.Name, member);

    public static VariableSymbol Create(IFieldSymbol member) =>
        new(member.Type, member.Name, member);

    public static IEnumerable<VariableSymbol> CreateAll(ITypeSymbol type) =>
        type.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(Create)
            .Concat(type.GetMembers().OfType<IFieldSymbol>().Select(Create));
}
