﻿using System;
using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Extensions;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class PackedSceneExtension
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="scene"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? InstanceOrFree<T>(this PackedScene scene)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(scene);
        var node = scene.Instantiate();
        if (node is T t)
        {
            return t;
        }

        node.QueueFree();
        GD.PushWarning($"Could not instance PackedScene {scene} as {typeof(T).Name}");
        return null;
    }

    /// <summary>
    /// Instances the scene and passes it as an argument to the supplied action. Allows for "fetching" data from a packed scene without immediately using it.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public static void ExtractData<T>(this PackedScene scene, Action<T?> action)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(action);
        var node = scene.InstanceOrFree<T>();
        action(node);
        node?.QueueFree();
    }
}
