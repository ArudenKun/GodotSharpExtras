using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Models;

internal record AttributeSite(INamedTypeSymbol ClassSymbol, AttributeData Attribute);
