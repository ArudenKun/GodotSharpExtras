using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GodotSharpExtras.Helpers;

/// <summary>
/// 
/// </summary>
[PublicAPI]
// ReSharper disable once InconsistentNaming
public static class IOHelper
{
    /// <summary>
    /// The default buffer size to use
    /// </summary>
    public const int DefaultBufferSize = 4096;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static FileStream OpenRead(string path, int bufferSize = DefaultBufferSize) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static Task<FileStream> OpenReadAsync(string path, int bufferSize = DefaultBufferSize) =>
        Task.FromResult(OpenRead(path, bufferSize));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="overwrite"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static FileStream OpenWrite(
        string filePath,
        bool overwrite = true,
        int bufferSize = DefaultBufferSize
    ) =>
        new(
            filePath,
            overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            true
        );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="overwrite"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static Task<FileStream> OpenWriteAsync(
        string filePath,
        bool overwrite = true,
        int bufferSize = DefaultBufferSize
    )
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        return Task.FromResult(OpenWrite(filePath, overwrite, bufferSize));
    }
}