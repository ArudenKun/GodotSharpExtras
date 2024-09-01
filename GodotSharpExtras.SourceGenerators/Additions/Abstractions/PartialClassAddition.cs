using System;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Additions.Abstractions;

internal abstract class PartialClassAddition
{
    protected const string Space = " ";
    protected const string Comma = ",";
    protected const string Tab = "\t";

    protected PartialClassAddition(INamedTypeSymbol classSymbol)
    {
        ClassSymbol = classSymbol;
    }

    public INamedTypeSymbol ClassSymbol { get; }
    public virtual Action<SourceStringBuilder>? DeclarationWriter => null;
    public virtual Action<SourceStringBuilder>? ConstructorStatementWriter => null;
    public virtual Action<SourceStringBuilder>? ReadyStatementWriter => null;
    public virtual Action<SourceStringBuilder>? OutsideClassStatementWriter => null;
}
