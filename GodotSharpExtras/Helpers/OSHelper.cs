using Godot;
using JetBrains.Annotations;

namespace GodotSharpExtras.Helpers;

/// <summary>
///
/// </summary>
public enum Platform
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
    // ReSharper disable once InconsistentNaming
    MacOS,

    /// <summary>
    /// 
    /// </summary>
    Android,

    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once InconsistentNaming
    IOS,

    /// <summary>
    /// 
    /// </summary>
    Web,

    /// <summary>
    /// 
    /// </summary>
    NotSupported
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
    public static Platform GetPlatform()
    {
        // return OS.GetName() switch
        // {
        //     "Windows" or "UWP" => Platform.Windows,
        //     "macOS" => Platform.Mac,
        //     "Linux" or "FreeBSD" or "NetBSD" or "OpenBSD" or "BSD" => Platform.Linux,
        //     _ => Platform.Unsupported
        // };

        switch (OS.GetName())
        {
            case "Windows":
            case "UWP":
                return Platform.Windows;
            case "macOS":
                GD.Print("Welcome to macOS!");
                return Platform.MacOS;
            case "Linux":
            case "FreeBSD":
            case "NetBSD":
            case "OpenBSD":
            case "BSD":
                return Platform.Linux;
            case "Android":
                return Platform.Android;
            case "iOS":
                return Platform.IOS;
            case "Web":
                return Platform.Web;
            default:
                return Platform.NotSupported;
        }
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