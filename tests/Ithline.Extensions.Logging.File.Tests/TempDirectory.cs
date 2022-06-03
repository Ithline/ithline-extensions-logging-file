using System;
using System.IO;

namespace Ithline.Extensions.Logging.File;

public sealed class TempDirectory : IDisposable
{
    private bool _disposed;

    public TempDirectory()
    {
        DirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "Morpheus.Extensions.Logging.File",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    public string GetRandomFileName()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TempDirectory));
        }

        return Path.Combine(DirectoryPath, $"{Guid.NewGuid():N}.tmp");
    }

    public string GetFileName(string fileName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TempDirectory));
        }

        return Path.Combine(DirectoryPath, fileName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Directory.Delete(DirectoryPath, true);
            _disposed = true;
        }
    }
}
