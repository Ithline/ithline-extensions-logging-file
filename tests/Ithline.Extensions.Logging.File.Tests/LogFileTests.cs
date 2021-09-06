using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Ithline.Extensions.Logging.File
{
    public sealed class LogFileTests : IClassFixture<TempDirectory>
    {
        private readonly TempDirectory _tmp;

        public LogFileTests(TempDirectory tmp)
        {
            _tmp = tmp;
        }

        [Theory]
        [InlineData(FileRollingInterval.None, 0)]
        [InlineData(FileRollingInterval.None, 1)]
        [InlineData(FileRollingInterval.None, 5)]
        [InlineData(FileRollingInterval.None, 10)]
        [InlineData(FileRollingInterval.Day, 0)]
        [InlineData(FileRollingInterval.Day, 1)]
        [InlineData(FileRollingInterval.Day, 5)]
        [InlineData(FileRollingInterval.Day, 10)]
        [InlineData(FileRollingInterval.Month, 0)]
        [InlineData(FileRollingInterval.Month, 1)]
        [InlineData(FileRollingInterval.Month, 5)]
        [InlineData(FileRollingInterval.Month, 10)]
        public void CreatingNewFile_WhenNotWriting_ShouldNotCreateFile(FileRollingInterval rollingInterval, int retainFileCount)
        {
            // arrange
            var filePath = _tmp.GetRandomFileName();

            // act
            using (var file = LogFile.Create(filePath, rollingInterval, retainFileCount))
            {
            }

            // assert
            System.IO.File.Exists(filePath).Should().BeFalse();
        }

        [Fact]
        public void WritingMessage_ToSimpleFile_ShouldWriteToOriginalFile()
        {
            // arrange
            var filePath = _tmp.GetRandomFileName();
            using (var logFile = LogFile.Create(filePath, FileRollingInterval.None, 0))
            {

                // act
                logFile.WriteMessage(new DateTime(2021, 01, 01), "message 1");
                logFile.WriteMessage(new DateTime(2021, 01, 02), "message 2");
                logFile.WriteMessage(new DateTime(2021, 01, 03), "message 3");
                logFile.WriteMessage(new DateTime(2021, 02, 01), "message 4");
                logFile.WriteMessage(new DateTime(2021, 02, 02), "message 5");
                logFile.WriteMessage(new DateTime(2021, 02, 11), "message 6");

                // assert
                AssertFileContent(filePath, @"message 1
message 2
message 3
message 4
message 5
message 6
");
            }
        }

        [Fact]
        public void WritingMessage_ToRollingFile_ShouldWriteMessageToCurrentFile()
        {
            // arrange
            var filePath = _tmp.GetFileName("rolling.txt");
            using (var logFile = LogFile.Create(filePath, FileRollingInterval.Day, 0))
            {

                // act
                logFile.WriteMessage(new DateTime(2021, 01, 01), "message 1");
                logFile.WriteMessage(new DateTime(2021, 01, 02), "message 2");
                logFile.WriteMessage(new DateTime(2021, 01, 03), "message 3");
                logFile.WriteMessage(new DateTime(2021, 02, 01), "message 4");
                logFile.WriteMessage(new DateTime(2021, 02, 02), "message 5");
                logFile.WriteMessage(new DateTime(2021, 02, 11), "message 6");

                // assert
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210101.txt"), $"message 1{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210102.txt"), $"message 2{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210103.txt"), $"message 3{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210201.txt"), $"message 4{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210202.txt"), $"message 5{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "rolling20210211.txt"), $"message 6{Environment.NewLine}");
            }
        }

        [Fact]
        public void WritingMessage_ToRollingFile_WithRetentionPolicy_ShouldRemoveOldFiles()
        {
            // arrange
            var filePath = _tmp.GetFileName("retention.txt");
            using (var logFile = LogFile.Create(filePath, FileRollingInterval.Day, 2))
            {

                // act
                logFile.WriteMessage(new DateTime(2021, 01, 01), "message 1");
                logFile.WriteMessage(new DateTime(2021, 01, 02), "message 2");
                logFile.WriteMessage(new DateTime(2021, 01, 03), "message 3");
                logFile.WriteMessage(new DateTime(2021, 02, 01), "message 4");
                logFile.WriteMessage(new DateTime(2021, 02, 02), "message 5");
                logFile.WriteMessage(new DateTime(2021, 02, 11), "message 6");

                // assert
                System.IO.File.Exists(Path.Combine(_tmp.DirectoryPath, "retention20210101.txt")).Should().BeFalse();
                System.IO.File.Exists(Path.Combine(_tmp.DirectoryPath, "retention20210102.txt")).Should().BeFalse();
                System.IO.File.Exists(Path.Combine(_tmp.DirectoryPath, "retention20210103.txt")).Should().BeFalse();
                System.IO.File.Exists(Path.Combine(_tmp.DirectoryPath, "retention20210201.txt")).Should().BeFalse();
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "retention20210202.txt"), $"message 5{Environment.NewLine}");
                AssertFileContent(Path.Combine(_tmp.DirectoryPath, "retention20210211.txt"), $"message 6{Environment.NewLine}");
            }
        }

        private static void AssertFileContent(string filePath, string expectation)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                var content = sr.ReadToEnd();
                content.Should().Be(expectation);
            }
        }
    }
}
