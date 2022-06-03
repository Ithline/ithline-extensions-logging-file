using System;

namespace Ithline.Extensions.Logging.File;

internal sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();

    private NullScope()
    {
    }

    public void Dispose()
    {
    }
}
