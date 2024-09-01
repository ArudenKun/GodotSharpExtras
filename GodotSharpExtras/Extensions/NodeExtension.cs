using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Extensions;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class NodeExtension
{
    /// <summary>
    /// Adds the Node to a group with a name equal to the Node's type name.
    /// </summary>
    /// <param name="node"></param>
    public static void AddToGroup(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.AddToGroup(node.GetType().Name);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <param name="idx"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetSibling<T>(this Node node, int idx)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return (T)node.GetParent().GetChild(idx);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetNode<T>(this Node node)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetNode<T>(typeof(T).Name);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetAutoLoadNode<T>(this Node node)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetNode<T>($"/root/{typeof(T).Name}");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetAutoLoadNode<T>(this Node node, string name)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetNode<T>($"/root/{name}");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetChildren<T>(this Node node)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        foreach (var child in node.GetChildren())
        {
            yield return (T)child;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetChildrenOfType<T>(this Node node)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetChildren().OfType<T>();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetFirstNodeOfType<T>(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var children = node.GetChildren();
        foreach (var child in children)
        {
            if (child is T t)
            {
                return t;
            }
        }

        return default;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetNodesOfType<T>(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var children = node.GetChildren();
        foreach (var child in children)
        {
            if (child is T t)
            {
                yield return t;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <param name="child"></param>
    public static void AddChildDeferred(this Node node, Node child)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.CallDeferred(Node.MethodName.AddChild, child);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodePath"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetNullableNodePath<T>(this Node node, NodePath? nodePath)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        return nodePath == null ? null : node.GetNodeOrNull<T>(nodePath);
    }

    /// <summary>
    /// Removes the node's children from the scene tree and then queues them for deletion.
    /// </summary>
    /// <param name="node"></param>
    public static void RemoveAndQueueFreeChildren(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        foreach (var child in node.GetChildren())
        {
            if (child == null)
                continue;
            node.RemoveChild(child);
            child.QueueFree();
        }
    }

    /// <summary>
    /// Queues all child nodes for deletion.
    /// </summary>
    /// <param name="node"></param>
    public static void QueueFreeChildren(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        foreach (var child in node.GetChildren())
        {
            child?.QueueFree();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetAncestor<T>(this Node node)
        where T : Node
    {
        ArgumentNullException.ThrowIfNull(node);
        var currentNode = node;
        while (currentNode != node.GetTree().Root && currentNode is not T)
        {
            currentNode = currentNode.GetParent();
        }

        return currentNode as T;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static Node? GetLastChild(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        var count = node.GetChildCount();
        return count == 0 ? null : node.GetChild(count - 1);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    public static void QueueFreeDeferred(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        node.CallDeferred("queue_free");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="nodes"></param>
    public static void QueueFreeAll(this IEnumerable<Node> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        foreach (var obj in nodes)
            obj.QueueFree();
    }

    /// <summary>
    /// Checks if the Node is the current game's scene. Useful for checking whether the scene was run using the "Run Current Scene" button.
    /// </summary>
    /// <returns></returns>
    public static bool IsCurrentScene(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetTree().CurrentScene.SceneFilePath == node.SceneFilePath;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static IEnumerable<Node> GetAllDescendants(this Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        // var result = new List<Node>();
        foreach (var child in node.GetChildren())
        {
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }

            yield return child;
        }
    }
}
