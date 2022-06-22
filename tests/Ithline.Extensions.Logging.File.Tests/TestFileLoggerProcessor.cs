using System;
using System.Collections.Generic;

namespace Ithline.Extensions.Logging.File;

internal sealed class TestFileLoggerProcessor : FileLoggerProcessor
{
    public TestFileLoggerProcessor()
    {
    }

    public List<string> Messages { get; } = new List<string>();

    public override void Enqueue(DateTime timestamp, string message) => Messages.Add(message);
}
