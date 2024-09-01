using System;
using Godot;
using JetBrains.Annotations;
using Timer = Godot.Timer;

namespace GodotSharpExtras.Extensions;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class TimerExtensions
{
    /// <summary>
    /// Creates a SceneTreeTimer, and waits for x amount of milliseconds before continuing execution.  Must await to halt progression.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="milliseconds"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter WaitTimer(this Node node, int milliseconds)
    {
        return node.WaitTimer(milliseconds / 1000.0f);
    }

    /// <summary>
    /// Creates a SceneTreeTimer, and waits for x amount of seconds before continuing execution.  Must await to halt progression.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="seconds"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter WaitTimer(this Node node, float seconds)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.ToSignal(node.GetTree().CreateTimer(seconds), Timer.SignalName.Timeout);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="node"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static SignalAwaiter WaitTimer(this Node node, TimeSpan timeSpan)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.ToSignal(
            node.GetTree().CreateTimer(timeSpan.TotalSeconds / 1000.0f),
            Timer.SignalName.Timeout
        );
    }

    /// <summary>
    /// Creates a SignalAwaiter for Timeout event on a Timer.  Can be used to await before continuing execution.
    /// </summary>
    /// <param name="timer"></param>
    /// <returns>SignalAwaiter</returns>
    public static SignalAwaiter Timeout(this Timer timer)
    {
        ArgumentNullException.ThrowIfNull(timer);
        return timer.ToSignal(timer, Timer.SignalName.Timeout);
    }

    /// <summary>
    /// Creates a SignalAwaiter for Timeout event on a SceneTreeTimer.  Can be used to await before continuing execution.
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public static SignalAwaiter Timeout(this SceneTreeTimer timer)
    {
        ArgumentNullException.ThrowIfNull(timer);
        return timer.ToSignal(timer, Timer.SignalName.Timeout);
    }
}
