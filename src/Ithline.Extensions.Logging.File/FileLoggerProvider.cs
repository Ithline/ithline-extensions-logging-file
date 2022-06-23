using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ithline.Extensions.Logging.File;

/// <summary>
/// A provider of file loggers.
/// </summary>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers;
    private readonly ThreadingFileLoggerProcessor _processor;
    private readonly bool _includeScopes;
    private IExternalScopeProvider? _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> with the specified options.
    /// </summary>
    /// <param name="options">Options used to configure the provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><see cref="FileLoggerOptions.FilePath"/> is <see langword="null"/> or empty string.</exception>
    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrEmpty(options.Value.FilePath))
        {
            throw new ArgumentException("File name must be specified.", nameof(options));
        }

        _includeScopes = options.Value.IncludeScopes;
        _loggers = new ConcurrentDictionary<string, FileLogger>();
        _processor = new ThreadingFileLoggerProcessor(options.Value);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, (category, ctx) =>
        {
            return new FileLogger(category, ctx._includeScopes, ctx._processor)
            {
                ScopeProvider = ctx._scopeProvider,
            };
        }, this);
    }

    /// <inheritdoc/>
    public void SetScopeProvider(IExternalScopeProvider? scopeProvider)
    {
        _scopeProvider = scopeProvider;
        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _processor.Dispose();
    }
}
