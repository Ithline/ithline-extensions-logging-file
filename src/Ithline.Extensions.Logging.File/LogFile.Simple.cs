using System;
using System.IO;

namespace Ithline.Extensions.Logging.File;

internal partial class LogFile
{
    private sealed class Simple : LogFile
    {
        private readonly string _filePath;
        private StreamWriter? _writer;

        public Simple(string filePath)
        {
            _filePath = filePath;
        }

        protected override TextWriter ResolveWriter(DateTime instant)
        {
            if (_writer is not null)
            {
                return _writer;
            }

            var fs = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            return _writer = new StreamWriter(fs, encoding: _utf8);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer?.Dispose();
            }
        }
    }
}
