using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Extensions;

/// <summary>
/// SceneTree extension methods
/// </summary>
[PublicAPI]
public static class SceneTreeExtensions
{
    /// <summary>
    /// Gets the first Node as T in the group provided.
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <param name="group"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetFirstNodeInGroup<T>(this SceneTree sceneTree, string group)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(sceneTree);
        var node = sceneTree.GetFirstNodeInGroup(group);
        return (T)node;
    }

    /// <summary>
    /// Gets the first Node as T using T's typename as the group name.
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetFirstNodeInGroup<T>(this SceneTree sceneTree)
        where T : Node => sceneTree.GetFirstNodeInGroup<T>(typeof(T).Name);

    /// <summary>
    ///
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <param name="group"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree, string group)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(sceneTree);
        return sceneTree.GetNodesInGroup(group).Cast<T>();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sceneTree"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree)
        where T : Node
    {
        var name = typeof(T).Name;
        return GetNodesInGroup<T>(sceneTree, name);
    }

    /// <summary>
    /// Creates a Waiter for the Engine to complete an Process Frame.  Must use await to hold progression.
    /// In Godot 4.0, IdleFrame became ProcessFrame.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter IdleFrameAsync(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return ProcessFrameAsync(node);
    }

    /// <summary>
    /// Creates a Waiter for the Engine to complete an Process Frame.  Must use await to hold progression.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter ProcessFrameAsync(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.ToSignal(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
    }

    /// <summary>
    /// Creates a Waiter for the Engine to complete a Frame, before progressing to the next one.  Must use await to hold progression.
    /// *Note* This is the same as IdleFrame, just a convenience alias name.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter NextFrameAsync(this Node node) => ProcessFrameAsync(node);
}
