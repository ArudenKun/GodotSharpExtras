using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using JetBrains.Annotations;
using FileAccess = Godot.FileAccess;

namespace GodotSharpExtras.Helpers;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class FileSystemHelper
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static string GetOSDir(this string path) =>
        ProjectSettings.GlobalizePath(path).NormalizePath();

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string JoinPath(this string path, params string[] parts)
    {
        var paths = new List<string> { path };
        paths.AddRange(parts);
        return Path.Combine(paths.ToArray());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(this string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var newPath = (string)path.Clone();
        if (path.StartsWith("user://", StringComparison.InvariantCulture))
            newPath = ProjectSettings.GlobalizePath(newPath);
        if (path.StartsWith("res://", StringComparison.InvariantCulture))
            return path;
        newPath = Path.GetFullPath(newPath);
        newPath =
            OSHelper.GetPlatform() == PlatformType.Windows
                ? newPath.Replace("/", @"\", StringComparison.InvariantCulture)
                : newPath.Replace(@"\", "/", StringComparison.InvariantCulture);
        return newPath;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetParentFolder(this string path) =>
        path.NormalizePath().GetBaseDir().GetBaseDir();

    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsDirEmpty(this string path) =>
        !DirAccess.DirExistsAbsolute(path.NormalizePath())
        || DirAccess.GetFilesAt(path.NormalizePath()).Length == 0;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public static bool IsSelfContained()
    {
        if (OS.IsDebugBuild())
            return false;

        var baseDir = OS.GetExecutablePath().GetBaseDir();
        string[] filesAndDirsToCheck = ["._sc_", "._sc_.", "_sc_", "portable"];

        return filesAndDirsToCheck.Any(f =>
            File.Exists(baseDir.JoinPath(f)) || Directory.Exists(baseDir.JoinPath(f))
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string GetUserFolder(params string[] parts)
    {
        var path = IsSelfContained()
            ? OS.GetExecutablePath().GetBaseDir().JoinPath("data")
            : "user://".NormalizePath();

        if (!DirAccess.DirExistsAbsolute(path))
        {
            DirAccess.MakeDirRecursiveAbsolute(path);
        }

        return path.JoinPath(parts);
    }

    // public static void CopyTo(this string srcFile, string destFile)
    // {
    //     var file = new FileInfo(srcFile);
    //     if (!file.Exists)
    //         throw new FileNotFoundException($"Source file not found: {file.FullName}");
    //
    //     file.CopyTo(destFile);
    // }

    /// <summary>
    ///
    /// </summary>
    /// <param name="srcFile"></param>
    /// <param name="destFile"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void CopyTo(this string srcFile, string destFile)
    {
        srcFile = srcFile.NormalizePath();
        destFile = destFile.NormalizePath();

        var fileExists = FileAccess.FileExists(srcFile);

        if (!fileExists)
        {
            throw new FileNotFoundException($"Source file not found: {srcFile}");
        }

        DirAccess.CopyAbsolute(srcFile, destFile);
    }

    // public static void CopyDirectory(
    //     this string sourceDir,
    //     string destinationDir,
    //     bool recursive = false
    // )
    // {
    //     var dir = new DirectoryInfo(sourceDir);
    //
    //     if (!dir.Exists)
    //         throw new DirectoryNotFoundException($"Source Directory not found: {dir.FullName}");
    //
    //     var dirs = dir.GetDirectories();
    //
    //     Directory.CreateDirectory(destinationDir);
    //
    //     foreach (var file in dir.GetFiles())
    //     {
    //         var targetFilePath = Path.Combine(destinationDir, file.Name);
    //         file.CopyTo(targetFilePath);
    //     }
    //
    //     if (!recursive)
    //         return;
    //
    //     foreach (var subDir in dirs)
    //     {
    //         var targetDirPath = Path.Combine(destinationDir, subDir.Name);
    //         CopyDirectory(subDir.FullName, targetDirPath, true);
    //     }
    // }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destinationDir"></param>
    /// <param name="recursive"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(
        this string sourceDir,
        string destinationDir,
        bool recursive = false
    )
    {
        sourceDir = sourceDir.NormalizePath();
        destinationDir = destinationDir.NormalizePath();

        using var dirAccess = DirAccess.Open(sourceDir);

        if (dirAccess is null)
        {
            throw new DirectoryNotFoundException($"Source Directory not found: {sourceDir}");
        }

        dirAccess.MakeDirRecursive(destinationDir);

        foreach (var file in dirAccess.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file);
            file.CopyTo(targetFilePath);
        }

        if (!recursive)
            return;

        foreach (var subDir in dirAccess.GetDirectories())
        {
            var targetDirPath = Path.Combine(destinationDir, subDir);
            CopyDirectory(Path.Combine(sourceDir, subDir), targetDirPath, true);
        }
    }
}
