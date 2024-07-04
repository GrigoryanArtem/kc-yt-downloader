using System.Collections.Concurrent;
using System.IO;

namespace kc_yt_downloader.GUI.Model
{
    public class LogPersister : IDisposable
    {
        public record LogMessage
        {
            public DateTime Time { get; init; }
            public TimeSpan Delta { get; init; }
            public LogLevel Level{ get; init; }
            public string? SubLevel { get; init; }
            public string? Message { get; init; } 
        }

        public enum LogLevel
        {
            Error,
            Standard
        }

        private readonly ConcurrentQueue<string> _textQueue = new();
        private readonly CancellationTokenSource _source = new();
        private readonly CancellationToken _token;

        private readonly string _path;

        public LogPersister(string path)
        {
            _path = path;

            _token = _source.Token;
            Task.Run(WriteToFile, _token);
        }

        public void Write(LogLevel level, string message)
        {
            var str = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} | {GetLevelString(level)} | {message}";
            _textQueue.Enqueue(str);
        }

        public void Stop()
            => _source.Cancel();

        public void Dispose()
            => Stop();

        private async void WriteToFile()
        {
            while (!_token.IsCancellationRequested || !_textQueue.IsEmpty)
            {
                using StreamWriter w = File.AppendText(_path);

                while (_textQueue.TryDequeue(out var textLine))
                    await w.WriteLineAsync(textLine);

                w.Flush();
                await Task.Delay(100);
            }
        }

        private string GetLevelString(LogLevel level) => level switch
        { 
            LogLevel.Standard => "STD",
            LogLevel.Error => "ERR",

            _ => throw new NotSupportedException()
        };
    }
}
