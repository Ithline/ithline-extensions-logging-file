using System;
using System.IO;
using System.Text;

namespace Ithline.Extensions.Logging.File;

internal abstract partial class LogFile : IDisposable
{
    private static readonly Encoding _utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private bool _disposed;

    private LogFile()
    {
    }

    public static LogFile Create(string filePath, FileRollingInterval rollingInterval, int retainFileCount)
    {
        var directoryName = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        if (rollingInterval == FileRollingInterval.None)
        {
            return new Simple(filePath);
        }

        return new Rolling(filePath, rollingInterval, retainFileCount);
    }

    public void WriteMessage(DateTime instant, string message)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(this.GetType().Name);
        }

        var writer = this.ResolveWriter(instant);
        writer.WriteLine(message);
        writer.Flush();
    }

    protected abstract TextWriter ResolveWriter(DateTime instant);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        if (!_disposed)
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
