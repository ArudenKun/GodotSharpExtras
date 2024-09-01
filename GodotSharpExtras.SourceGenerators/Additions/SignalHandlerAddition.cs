using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GodotSharpExtras.Attributes;
using GodotSharpExtras.SourceGenerators.Additions.Abstractions;
using GodotSharpExtras.SourceGenerators.Extensions;
using GodotSharpExtras.SourceGenerators.Extensions.CaseExtensions;
using GodotSharpExtras.SourceGenerators.Utilities;
using Microsoft.CodeAnalysis;

namespace GodotSharpExtras.SourceGenerators.Additions;

internal sealed class SignalHandlerAddition : PartialClassAddition
{
    private static readonly Dictionary<string, IEventSymbol> EventSymbolsCache = new();

    public string MethodName { get; }
    public Signal[] Signals { get; }

    public SignalHandlerAddition(
        IMethodSymbol methodSymbol,
        AttributeData[] attributeDatas,
        INamedTypeSymbol classSymbol
    )
        : base(classSymbol)
    {
        MethodName = methodSymbol.Name;

        var variables = classSymbol
            .GetMembers()
            .Where(x => x is IFieldSymbol or IPropertySymbol)
            .ToArray();

        var signals = new List<Signal>();
        foreach (var attributeData in attributeDatas)
        {
            var attribute = attributeData.MapToType<SignalHandlerAttribute>();
            var signalName = attribute.SignalName.ToPascalCase();
            var variableName = attribute.VariableName ?? string.Empty;
            var variableTypeSymbol = (
                variables.FirstOrDefault(x => x.Name == variableName) ?? classSymbol
            ) switch
            {
                IFieldSymbol fieldSymbol => fieldSymbol.Type,
                IPropertySymbol propertySymbol => propertySymbol.Type,
                ITypeSymbol typeSymbol => typeSymbol,
                _ => throw new InvalidCastException("Unknown signal type"),
            };

            if (!EventSymbolsCache.TryGetValue(signalName, out var eventSymbol))
            {
                var newEventSymbol = variableTypeSymbol
                    .GetAllMembers()
                    .OfType<IEventSymbol>()
                    .FirstOrDefault(x => x.Name == signalName);

                if (newEventSymbol is null)
                {
                    continue;
                }

                eventSymbol = newEventSymbol;
                EventSymbolsCache.Add(signalName, eventSymbol);
            }

            var eventTypeSymbol = eventSymbol.Type as INamedTypeSymbol;
            var eventParameters =
                eventTypeSymbol?.DelegateInvokeMethod?.Parameters
                ?? ImmutableArray<IParameterSymbol>.Empty;

            signals.Add(
                new Signal(signalName, variableName, variableTypeSymbol.Name, eventParameters)
            );
        }

        Signals = signals.ToArray();
    }

    public override Action<SourceStringBuilder> ReadyStatementWriter =>
        g =>
        {
            foreach (
                var (signalName, variableName, variableTypeName, eventParameterSymbols) in Signals
            )
            {
                var parameterTypesString = eventParameterSymbols.IsNotNullOrEmpty()
                    ? $"<{string.Join(
                        ", ",
                        eventParameterSymbols.Select(x => x.Type.ToDisplayString())
                    )}>"
                    : string.Empty;

                var paramDiscardsString =
                    eventParameterSymbols.Length == 1
                        ? "_ => "
                        : $"({string.Join(", ", eventParameterSymbols.Select(_ => "_"))}) => ";

                var isVariable = variableName.IsNotNullOrEmpty();
                var variable = isVariable ? $"{variableName}." : string.Empty;
                var variableType = isVariable ? $"{variableTypeName}." : string.Empty;

                g.Line(
                    variable,
                    "Connect(",
                    variableType,
                    "SignalName.",
                    signalName,
                    Comma,
                    Space,
                    "Callable.From",
                    parameterTypesString,
                    "(",
                    paramDiscardsString,
                    MethodName,
                    "()",
                    "));"
                );
            }
        };

    internal readonly record struct Signal(
        string Name,
        string VariableName,
        string VariableTypeName,
        ImmutableArray<IParameterSymbol> EventParameterSymbols
    );
}
