using System;
using System.Text;

namespace GodotSharpExtras.SourceGenerators.Models;

public class Tree<T> : TreeNode<T>
{
    public Tree(T value)
        : base(value, null) { }

    public void Traverse(Action<TreeNode<T>> action)
    {
        TraverseInternal(this);
        return;

        void TraverseInternal(TreeNode<T> node)
        {
            action(node);
            node.Children.ForEach(TraverseInternal);
        }
    }

    public void Traverse(Action<TreeNode<T>, int> action)
    {
        TraverseInternal(this, 0);
        return;

        void TraverseInternal(TreeNode<T> node, int depth)
        {
            action(node, depth++);
            node.Children.ForEach(x => TraverseInternal(x, depth));
        }
    }

    public override string ToString()
    {
        StringBuilder str = new();
        Traverse(PrintNode);
        return str.ToString();

        void PrintNode(TreeNode<T> node, int level)
        {
            var indent = new string(' ', level * 2);
            _ = str.AppendLine($"{indent}{node.Value}");
        }
    }
}
