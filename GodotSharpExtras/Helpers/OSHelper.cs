using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Helpers;

/// <summary>
///
/// </summary>
public enum PlatformType
{
    /// <summary>
    /// 
    /// </summary>
    Windows,
    /// <summary>
    /// 
    /// </summary>
    Linux,
    /// <summary>
    /// 
    /// </summary>
    Mac,
    /// <summary>
    /// 
    /// </summary>
    Unsupported
}

/// <summary>
///
/// </summary>
[PublicAPI]
// ReSharper disable once InconsistentNaming
public static class OSHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static PlatformType GetPlatform()
    {
        return OS.GetName() switch
        {
            "Windows" or "UWP" => PlatformType.Windows,
            "macOS" => PlatformType.Mac,
            "Linux" or "FreeBSD" or "NetBSD" or "OpenBSD" or "BSD" => PlatformType.Linux,
            _ => PlatformType.Unsupported
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static string GetName() => OS.GetName();

    /// <summary>
    ///
    /// </summary>
    public static bool Is64Bit => System.Environment.Is64BitOperatingSystem;
}
