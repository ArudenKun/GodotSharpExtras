using Godot;
using GodotSharpExtras.SourceGenerators.Attributes;

namespace Test;

public partial class Main : Node
{
    [Signal]
    public delegate void TestEventHandler();

    [Notify]
    private string _holy;

    [Notify]
    private int _fuck;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
