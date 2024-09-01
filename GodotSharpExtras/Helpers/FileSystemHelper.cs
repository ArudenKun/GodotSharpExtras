using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using JetBrains.Annotations;

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
    /// <param name="fileNameOrPath"></param>
    public static void EnsureContainingDirectoryExists(string fileNameOrPath)
    {
        var fullPath = Path.GetFullPath(fileNameOrPath); // No matter if relative or absolute path is given to this.
        var dir = Path.GetDirectoryName(fullPath);
        EnsureDirectoryExists(dir);
    }

    /// <summary>
    ///     Makes sure that directory <paramref name="dir" /> is created if it does not exist.
    /// </summary>
    /// <remarks>Method does not throw exceptions unless provided directory path is invalid.</remarks>
    public static void EnsureDirectoryExists(string? dir)
    {
        // If root is given, then do not worry.
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="replacementChar"></param>
    /// <returns></returns>
    public static string SanitizeFileName(this string source, char replacementChar = '_')
    {
        ArgumentNullException.ThrowIfNull(source);
        var blackList = new HashSet<char>(Path.GetInvalidFileNameChars())
            { '"' }; // '"' not invalid in Linux, but causes problems
        var output = source.ToCharArray();
        for (int i = 0, ln = output.Length; i < ln; i++)
            if (blackList.Contains(output[i]))
                output[i] = replacementChar;

        return new string(output);
    }

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
        !Directory.Exists(path.NormalizePath())
        || Directory.GetFiles(path.NormalizePath()).Length == 0;

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

        if (!Directory.Exists(path))
        {
            EnsureDirectoryExists(path);
        }

        return path.JoinPath(parts);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="srcFile"></param>
    /// <param name="destFile"></param>
    /// <param name="overwrite"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void CopyTo(this string srcFile, string destFile, bool overwrite = true)
    {
        srcFile = srcFile.NormalizePath();
        destFile = destFile.NormalizePath();

        var file = new FileInfo(srcFile);
        if (!file.Exists)
            throw new FileNotFoundException($"Source file not found: {file.FullName}");

        file.CopyTo(destFile, overwrite);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destinationDir"></param>
    /// <param name="recursive"></param>
    /// <param name="overwrite"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(
        this string sourceDir,
        string destinationDir,
        bool recursive = false,
        bool overwrite = true
    )
    {
        sourceDir = sourceDir.NormalizePath();
        destinationDir = destinationDir.NormalizePath();

        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source Directory not found: {dir.FullName}");

        EnsureDirectoryExists(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, overwrite);
        }

        if (!recursive)
            return;

        foreach (var subDir in dir.GetDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirPath"></param>
    public static void DeleteDirectory(string dirPath)
    {
        dirPath = dirPath.NormalizePath();

        var dir = new DirectoryInfo(dirPath);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Directory not found: {dir.FullName}");
        }

        dir.Delete(true);
    }
}