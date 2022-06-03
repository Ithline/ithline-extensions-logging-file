namespace Ithline.Extensions.Logging.File;

/// <summary>
/// Provides configuration for <see cref="FileLoggerProvider"/>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// Gets or sets the path of the file to log to.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the interval at which the file should roll over.
    /// </summary>
    public FileRollingInterval RollingInterval { get; set; } = FileRollingInterval.None;

    /// <summary>
    /// Gets or sets a number of files to retain. Zero or negative value will disable removal of old files. Default value is 31.
    /// </summary>
    public int MaxRollingFiles { get; set; } = 31;
}
