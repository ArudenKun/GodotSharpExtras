using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotSharpExtras.SourceGenerators.Extensions;

internal static class SyntaxNodeExtensions
{
    /// <summary>
    /// Finds the first node of type T by traversing the parent nodes.
    /// </summary>
    /// <typeparam name="T">the type of </typeparam>
    /// <param name="syntaxNode"></param>
    /// <returns>The first node of type T, otherwise null.</returns>
    internal static T? GetParent<T>(this SyntaxNode syntaxNode)
        where T : SyntaxNode
    {
        SyntaxNode? currentNode = syntaxNode.Parent;
        while (currentNode != null)
        {
            if (currentNode is T t)
                return t;

            currentNode = currentNode.Parent;
        }

        return null;
    }

    /// <summary>
    /// Finds the first attribute that matches the given name.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    internal static AttributeSyntax? GetAttribute(
        this MemberDeclarationSyntax member,
        string attributeName
    )
    {
        foreach (AttributeListSyntax attributeList in member.AttributeLists)
        foreach (AttributeSyntax attribute in attributeList.Attributes)
        {
            string identifier = attribute.Name switch
            {
                SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
                QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                _ => string.Empty,
            };
            if (identifier == attributeName)
                return attribute;

            const string attribute2 = "Attribute";
            if (identifier.Length == attributeName.Length + attribute2.Length)
            {
                ReadOnlySpan<char> identifierSpan = identifier.AsSpan();
                if (
                    identifierSpan[..^attribute2.Length].SequenceEqual(attributeName.AsSpan())
                    && identifierSpan[attributeName.Length..].SequenceEqual(attribute2.AsSpan())
                )
                    return attribute;
            }
        }

        return null;
    }

    /// <summary>
    /// Basically linq Contains method on a SyntaxTokenList
    /// </summary>
    /// <param name="modifiers"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    internal static bool Contains(this SyntaxTokenList modifiers, string token)
    {
        foreach (SyntaxToken modifier in modifiers)
            if (modifier.ValueText == token)
                return true;

        return false;
    }

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value.</para>
    /// <para>If not found, it returns null.</para>
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static TypedConstant? GetArgument(
        this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments,
        string name
    )
    {
        for (int i = 0; i < arguments.Length; i++)
            if (arguments[i].Key == name)
                return arguments[i].Value;

        return null;
    }

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value.</para>
    /// <para>If not found or value is not castable, it returns default.</para>
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static T? GetArgument<T>(
        this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments,
        string name
    ) =>
        GetArgument(arguments, name) switch
        {
            { Value: T value } => value,
            _ => default,
        };

    /// <summary>
    /// <para>Finds the argument with the given name and returns it's value as array.</para>
    /// <para>If not found or any value is not castable, it returns an empty array.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arguments"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    internal static T[] GetArgumentArray<T>(
        this ImmutableArray<KeyValuePair<string, TypedConstant>> arguments,
        string name
    )
    {
        if (arguments.GetArgument(name) is not { Kind: TypedConstantKind.Array } typeArray)
            return [];

        T[] result = new T[typeArray.Values.Length];
        for (int i = 0; i < result.Length; i++)
        {
            if (typeArray.Values[i].Value is not T value)
                return [];
            result[i] = value;
        }

        return result;
    }
}
