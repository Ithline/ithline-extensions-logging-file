using System;

namespace Ithline.Extensions.Logging.File;

internal readonly struct LogMessageEntry
{
    public LogMessageEntry(DateTime timestamp, string message)
    {
        Timestamp = timestamp;
        Message = message;
    }

    public DateTime Timestamp { get; }
    public string Message { get; }
}
