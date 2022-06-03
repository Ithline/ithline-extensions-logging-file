using System;

namespace Ithline.Extensions.Logging.File;

internal abstract class FileLoggerProcessor : IDisposable
{
    private bool _disposed;

    protected FileLoggerProcessor()
    {
    }

    public abstract void Enqueue(DateTime timestamp, string message);

    public void Dispose()
    {
        if (!_disposed)
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}
