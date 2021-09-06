using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ithline.Extensions.Logging.File
{
    public sealed class FileLoggerTest : IClassFixture<TempDirectory>
    {
        private readonly TempDirectory _tmp;

        public FileLoggerTest(TempDirectory tmp)
        {
            _tmp = tmp ?? throw new ArgumentNullException(nameof(tmp));
        }

        [Fact]
        public void FileLoggerOptions_IsReadFromLoggingConfiguration()
        {
            // arrange
            var filePath = _tmp.GetRandomFileName();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("File:FilePath", filePath),
                    new KeyValuePair<string, string>("File:RollingInterval", "Month"),
                    new KeyValuePair<string, string>("File:MaxRollingFiles", "16"),
                })
                .Build();

            // act
            var loggerOptions = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration).AddFile();
                })
                .BuildServiceProvider()
                .GetRequiredService<IOptions<FileLoggerOptions>>()
                .Value;

            // assert
            loggerOptions.FilePath.Should().Be(filePath);
            loggerOptions.RollingInterval.Should().Be(FileRollingInterval.Month);
            loggerOptions.MaxRollingFiles.Should().Be(16);
        }

        [Fact]
        public void LogsWhenMessageIsNotProvided()
        {
            // arrange
            var processor = new TestFileLoggerProcessor();
            var exception = new InvalidOperationException("Invalid operation.");
            var logger = new FileLogger("NoMessage", processor);

            // act
            logger.LogCritical(eventId: 1, message: null, exception: exception);
            logger.LogError(eventId: 2, message: null, exception: exception);
            logger.LogWarning(eventId: 3, message: null, exception: exception);

            // assert
            processor.Messages.Should().HaveCount(3);

            // language=regex
            processor.Messages[0].Should().MatchRegex(@"^\[[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\]\[crit\]\[NoMessage\(1\)\] \[null\]\s+System\.InvalidOperationException: Invalid operation\.$");

            // language=regex
            processor.Messages[1].Should().MatchRegex(@"^\[[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\]\[fail\]\[NoMessage\(2\)\] \[null\]\s+System\.InvalidOperationException: Invalid operation\.$");

            // language=regex
            processor.Messages[2].Should().MatchRegex(@"^\[[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\]\[warn\]\[NoMessage\(3\)\] \[null\]\s+System\.InvalidOperationException: Invalid operation\.$");
        }
    }
}
