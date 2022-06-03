using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Ithline.Extensions.Logging.File;

internal partial class LogFile
{
    private sealed class Rolling : LogFile
    {
        private readonly Regex _fileMatcher;
        private readonly FileRollingInterval _rollingInterval;
        private readonly int _maxRollingFiles;
        private readonly string? _directoryPath;
        private readonly string _fileNamePrefix;
        private readonly string _fileNameSuffix;
        private readonly string _format;
        private StreamWriter? _writer;
        private DateTime? _checkpoint;

        public Rolling(string filePath, FileRollingInterval rollingInterval, int maxRollingFiles)
        {
            _format = ResolveFormat(rollingInterval);

            _rollingInterval = rollingInterval;
            _maxRollingFiles = maxRollingFiles;

            _directoryPath = Path.GetDirectoryName(filePath);
            _fileNamePrefix = Path.GetFileNameWithoutExtension(filePath);
            _fileNameSuffix = Path.GetExtension(filePath);

            var pattern = $"^{Regex.Escape(_fileNamePrefix)}(?<period>\\d{{{_format.Length}}}){Regex.Escape(_fileNameSuffix)}$";
            _fileMatcher = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        protected override TextWriter ResolveWriter(DateTime instant)
        {
            // if we have created writer and it is still valid, we just return
            if (_writer != null && _checkpoint > instant)
            {
                return _writer;
            }

            _writer?.Dispose();
            _writer = null;

            var fileName = $"{_fileNamePrefix}{instant.ToString(_format, CultureInfo.InvariantCulture)}{_fileNameSuffix}";
            _checkpoint = ResolveCheckpoint(_rollingInterval, instant);

            this.ApplyRetentionPolicy(fileName);
            var fs = new FileStream(Path.Combine(_directoryPath ?? string.Empty, fileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            return _writer = new StreamWriter(fs, encoding: _utf8);
        }

        private void ApplyRetentionPolicy(string currentFileName)
        {
            if (_maxRollingFiles < 1 || string.IsNullOrEmpty(_directoryPath))
            {
                return;
            }

            List<(string filePath, DateTime expiration)>? matchedFiles = null;
            foreach (var filePath in Directory.EnumerateFiles(_directoryPath))
            {
                var fileName = Path.GetFileName(filePath);
                var match = _fileMatcher.Match(fileName);
                if (match.Success)
                {
                    if (string.Equals(currentFileName, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (DateTime.TryParseExact(match.Groups[1].Value, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiration))
                    {
                        matchedFiles ??= new List<(string filePath, DateTime expiration)>();
                        matchedFiles.Add((filePath, expiration));
                    }
                }
            }

            // we have not found any files, nothing to delete
            if (matchedFiles is null)
            {
                return;
            }

            // calculate the number of files to remove, we subtract 1 from max number of files, because we always skip current file
            var removeCount = matchedFiles.Count - (_maxRollingFiles - 1);
            if (removeCount <= 0)
            {
                return;
            }

            matchedFiles.Sort((left, right) => left.expiration.CompareTo(right.expiration));
            for (var i = 0; i < removeCount; i++)
            {
                var (filePath, expiration) = matchedFiles[i];
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception)
                {
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer?.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ResolveFormat(FileRollingInterval rollingInterval)
        {
            return rollingInterval switch
            {
                FileRollingInterval.Year => "yyyy",
                FileRollingInterval.Month => "yyyyMM",
                FileRollingInterval.Day => "yyyyMMdd",
                FileRollingInterval.Hour => "yyyyMMddHH",
                _ => throw new ArgumentOutOfRangeException(nameof(rollingInterval)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime ResolveCheckpoint(FileRollingInterval rollingInterval, DateTime instant)
        {
            return rollingInterval switch
            {
                FileRollingInterval.Year => new DateTime(instant.Year, 1, 1, 0, 0, 0, instant.Kind).AddYears(1),
                FileRollingInterval.Month => new DateTime(instant.Year, instant.Month, 1, 0, 0, 0, instant.Kind).AddMonths(1),
                FileRollingInterval.Day => new DateTime(instant.Year, instant.Month, instant.Day, 0, 0, 0, instant.Kind).AddDays(1),
                FileRollingInterval.Hour => new DateTime(instant.Year, instant.Month, instant.Day, instant.Hour, 0, 0, instant.Kind).AddHours(1),
                _ => throw new ArgumentOutOfRangeException(nameof(rollingInterval)),
            };
        }
    }
}
