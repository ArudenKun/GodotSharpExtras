using System;

namespace GodotSharpExtras.Attributes;

/// <summary>
/// Marks a function as a Event Handler for signals coming from Godot nodes.
/// </summary>
/// <remarks>
/// This is used to make code more readable, and easier to extend, without having to constantly add to the _Ready() function.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SignalHandlerAttribute : Attribute
{
    public string? VariableName { get; set; }

    public string SignalName { get; set; }

    /// <summary>
    /// Constructs a <see cref="SignalHandlerAttribute"/>
    /// </summary>
    /// <param name="signalName">The name of the signal you wish to connect to.</param>
    /// <param name="variableName">(Optional) The nameof() the Node variable you want to connect to.  When none is given, the instance (this) is used.</param>
    public SignalHandlerAttribute(string signalName, string? variableName = null)
    {
        SignalName = signalName;
        VariableName = variableName;
    }
}
