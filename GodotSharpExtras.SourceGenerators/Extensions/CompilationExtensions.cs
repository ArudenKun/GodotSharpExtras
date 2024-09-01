﻿using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Extensions;

internal static class CompilationExtensions
{
    public static string? GetFullName(this Compilation compilation, string type, string hint)
    {
        var symbols = compilation.GetSymbolsWithName(type, SymbolFilter.Type).ToArray();

        ResolveDuplicates();

        var symbol = symbols.FirstOrDefault();
        if (symbol is null)
            return null;

        var ns = symbol.NamespaceOrNull();
        return ns is null ? $"global::{type}" : $"{ns}.{type}";

        void ResolveDuplicates()
        {
            if (symbols.Skip(1).Any())
            {
                symbols = symbols // Ignore generics
                    .Where(x => x.MetadataName == type)
                    .ToArray();

                if (symbols.Skip(1).Any())
                {
                    // Differentiate by path
                    hint = string.Join(@"\", hint.Split('/'));
                    symbols = symbols
                        .Where(x =>
                            x.Locations.Select(location => location.GetLineSpan().Path)
                                .Any(s => s.EndsWith(hint))
                        )
                        .ToArray();
                }
            }
        }
    }

    public static string ValidateTypeIgnoreCase(
        this Compilation compilation,
        string assemblyName,
        string namespaceName,
        string type
    )
    {
        var assemblyRef = compilation
            .References.OfType<PortableExecutableReference>()
            .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.FilePath) == assemblyName);
        if (assemblyRef is null)
            return type;

        var assemblySymbol = (IAssemblySymbol?)compilation.GetAssemblyOrModuleSymbol(assemblyRef);
        if (assemblySymbol is null)
            return type;

        var namespaceSymbol = assemblySymbol
            .GlobalNamespace.GetNamespaceMembers()
            .FirstOrDefault(x => x.Name == namespaceName);
        if (namespaceSymbol is null)
            return type;

        var typeSymbol = namespaceSymbol
            .GetTypeMembers()
            .FirstOrDefault(x => CompareNameIgnoreCase(x.Name));
        return typeSymbol?.Name ?? type;

        bool CompareNameIgnoreCase(string name) =>
            string.Equals(type, name, StringComparison.OrdinalIgnoreCase);
    }
}
