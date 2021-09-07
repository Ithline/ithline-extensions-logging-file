using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ithline.Extensions.Logging.File
{
    internal sealed class FileLogger : ILogger
    {
        [ThreadStatic]
        private static StringBuilder? _stringBuilder;
        private readonly FileLoggerProcessor _processor;
        private readonly string _category;

        public FileLogger(string category, FileLoggerProcessor processor)
        {
            _category = category;
            _processor = processor;
        }

        internal IExternalScopeProvider? ScopeProvider { get; set; }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState?, Exception?, string> formatter)
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
            var timestamp = DateTimeOffset.Now;
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
            ScopeProvider?.ForEachScope((scope, writer) =>
            {
                if (IsPropertyBag(scope, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (string.IsNullOrEmpty(property.Key))
                        {
                            continue;
                        }

                        writer.AppendLine();
                        writer.Append(property.Key);
                        writer.Append(':');
                        writer.Append(property.Value?.ToString() ?? "null");
                    }
                }
                else if (scope is not null)
                {
                    writer.AppendLine();
                    writer.Append(scope.ToString());
                }
            }, _stringBuilder);

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

            _processor.Enqueue(formattedLogEvent);
        }

        private static bool IsPropertyBag(object scope, [NotNullWhen(true)] out IEnumerable<KeyValuePair<string, object?>>? properties)
        {
            if (scope is IReadOnlyList<KeyValuePair<string, object?>> list && list.Count > 0)
            {
                var item = list[list.Count - 1];
                if (item.Key != "{OriginalFormat}")
                {
                    properties = list;
                    return true;
                }
            }
            else if (scope is IEnumerable<KeyValuePair<string, object?>> enumerable)
            {
                properties = enumerable;
                return true;
            }

            properties = null;
            return false;
        }
    }
}
