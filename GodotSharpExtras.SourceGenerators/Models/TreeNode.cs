﻿using System.Collections.Generic;

namespace GodotSharpExtras.SourceGenerators.Models;

public class TreeNode<T>
{
    public T Value { get; }
    public TreeNode<T>? Parent { get; }
    public List<TreeNode<T>> Children { get; } = [];

    public bool IsRoot => Parent is null;

    public TreeNode(T value, TreeNode<T>? parent)
    {
        Value = value;
        Parent = parent;
    }

    public TreeNode<T> Add(T value)
    {
        var node = new TreeNode<T>(value, this);
        Children.Add(node);
        return node;
    }
}
