using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ithline.Extensions.Logging.File;

internal sealed class FileLogger : ILogger
{
    [ThreadStatic]
    private static StringBuilder? _stringBuilder;
    private readonly FileLoggerProcessor _processor;
    private readonly string _category;
    private readonly bool _includeScopes;

    public FileLogger(string category, bool includeScopes, FileLoggerProcessor processor)
    {
        _category = category;
        _includeScopes = includeScopes;
        _processor = processor;
    }

    internal IExternalScopeProvider? ScopeProvider { get; set; }

    public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!this.IsEnabled(logLevel))
        {
            return;
        }

        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);
        if (message is null && exception is null)
        {
            return;
        }

        _stringBuilder ??= new StringBuilder();

        // timestamp
        var timestamp = DateTime.Now;
        _stringBuilder.Append('[');
        _stringBuilder.Append(timestamp.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        _stringBuilder.Append(']');

        // log level
        _stringBuilder.Append('[');
        _stringBuilder.Append(logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        });
        _stringBuilder.Append(']');

        // category
        _stringBuilder.Append('[');
        _stringBuilder.Append(_category);
        _stringBuilder.Append('(');
        _stringBuilder.Append(eventId.ToString());
        _stringBuilder.Append(')');
        _stringBuilder.Append(']');

        // message
        _stringBuilder.Append(' ');
        _stringBuilder.Append(message ?? exception?.Message);

        // scopes
        if (_includeScopes)
        {
            ScopeProvider?.ForEachScope((scope, writer) =>
            {
                writer.AppendLine();
                writer.Append("=> ");
                writer.Append(scope);
            }, _stringBuilder);
        }

        if (exception is not null)
        {
            _stringBuilder.AppendLine();
            _stringBuilder.Append(exception.ToString());
        }

        var formattedLogEvent = _stringBuilder.ToString();

        _stringBuilder.Clear();
        if (_stringBuilder.Capacity > 1024)
        {
            _stringBuilder.Capacity = 1024;
        }

        _processor.Enqueue(timestamp, formattedLogEvent);
    }
}
