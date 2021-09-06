namespace Ithline.Extensions.Logging.File
{
    internal readonly struct LogMessageEntry
    {
        public readonly string Message;

        public LogMessageEntry(string message)
        {
            Message = message;
        }
    }
}
