using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ithline.Extensions.Logging.File;

internal sealed class ThreadingFileLoggerProcessor : FileLoggerProcessor
{
    private const int MaxQueuedMessages = 1024;
    private readonly BlockingCollection<LogMessageEntry> _messageQueue;
    private readonly Thread _outputThread;
    private readonly LogFile _logFile;

    public ThreadingFileLoggerProcessor(FileLoggerOptions options)
    {
        _messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);

        _logFile = LogFile.Create(
            filePath: options.FilePath,
            rollingInterval: options.RollingInterval,
            retainFileCount: options.MaxRollingFiles);

        _outputThread = new Thread(this.ProcessLogQueue)
        {
            IsBackground = true,
            Name = "File logger queue processing thread"
        };
        _outputThread.Start();
    }

    public override void Enqueue(DateTime timestamp, string message)
    {
        if (_messageQueue.IsAddingCompleted)
        {
            return;
        }

        try
        {
            _messageQueue.Add(new LogMessageEntry(timestamp, message));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void ProcessLogQueue()
    {
        try
        {
            foreach (var entry in _messageQueue.GetConsumingEnumerable())
            {
                _logFile.WriteMessage(entry.Timestamp, entry.Message);
            }
        }
        catch
        {
            try
            {
                _messageQueue.CompleteAdding();
            }
            catch
            {
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageQueue.CompleteAdding();
            try
            {
                _outputThread.Join(1500);
            }
            catch (ThreadStateException)
            {
            }
            _logFile.Dispose();
        }

        base.Dispose(disposing);
    }
}
