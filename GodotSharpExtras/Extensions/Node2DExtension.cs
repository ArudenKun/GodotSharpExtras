using System;
using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Extensions;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class Node2DExtension
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static Vector2 GetMouseDirection(this Node2D node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return (node.GetGlobalMousePosition() - node.GlobalPosition).Normalized();
    }
}
