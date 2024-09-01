using System;

// ReSharper disable once CheckNamespace
namespace GodotSharpExtras.Attributes;

/// <summary>
/// Generates code to initialize this property or field when the node is ready, and make the
/// initialization path configurable in the Godot editor. This attribute works on properties and
/// fields of types that subclass either Node or Resource.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class OnReadyGetAttribute : Attribute
{
    public string? Path { get; }

    /// <summary>
    /// Allows nulls and unexpected node types in the ready method without throwing exceptions.
    /// </summary>
    public bool OrNull { get; init; }

    /// <param name="path">
    /// The path that will be loaded when the node is ready. If not set a pascal case unique name will be set
    /// </param>
    public OnReadyGetAttribute(string? path = null)
    {
        Path = path;
    }
}
