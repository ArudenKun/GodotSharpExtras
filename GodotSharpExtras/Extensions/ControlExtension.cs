using System;
using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Extensions;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class ControlExtension
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="control"></param>
    public static void CenterPivotOffset(this Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        control.PivotOffset = control.Size / 2f;
    }
}
