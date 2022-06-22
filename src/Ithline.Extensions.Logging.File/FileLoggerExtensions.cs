using System;
using System.Diagnostics.CodeAnalysis;
using Ithline.Extensions.Logging.File;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// Helper methods for <see cref="FileLoggerProvider"/> registration.
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds a file logger named 'File' to the factory.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
    /// <param name="configure">A delegate to configure the <see cref="FileLogger"/>.</param>
    /// <returns><paramref name="builder"/> for chaining.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions>? configure = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        return builder;
    }
}
