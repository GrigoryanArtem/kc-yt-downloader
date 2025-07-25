using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace kc_yt_downloader.GUI.Model
{
    public class LogPersister : IDisposable
    {
        public record LogMessage
        {
            public DateTime Time { get; init; }
            public TimeSpan Delta { get; init; }
            public LogLevel Level { get; init; }
            public string? Context { get; init; }
            public string? Message { get; init; }

            public string ToLogString()
                => $"{Time:yyyy-MM-dd HH:mm:ss.ffff} | {Delta:G} | {GetLevelString(Level)} | {Context} | {Message}";

            private string GetLevelString(LogLevel level) => level switch
            {
                LogLevel.Standard => "STD",
                LogLevel.Error => "ERR",

                _ => throw new NotSupportedException()
            };
        }

        public enum LogLevel
        {
            Error,
            Standard
        }

        private readonly ConcurrentQueue<LogMessage> _messagesQueue = new();
        private readonly CancellationTokenSource _source = new();
        private readonly CancellationToken _token;

        private readonly string _path;

        public LogPersister(string path)
        {
            _path = path;

            _token = _source.Token;
            Task.Run(WriteToFile, _token);
        }

        public ObservableCollection<LogMessage> Messages { get; } = new();

        public void Write(LogLevel level, string message)
        {
            string clearMessage = (message ?? String.Empty).Trim();
            string context = String.Empty;

            if (clearMessage.StartsWith('['))
            {
                var index = clearMessage.IndexOf(']');

                context = clearMessage[1..index];
                clearMessage = clearMessage[(index + 1)..].Trim();
            }

            _messagesQueue.Enqueue(new()
            {
                Level = level,
                Message = clearMessage.Trim(),
                Context = context,
                Time = DateTime.Now,
            });
        }

        public void Stop()
            => _source.Cancel();

        public void Dispose()
            => Stop();

        private async void WriteToFile()
        {
            DateTime? prevTime = null;
            while (!_token.IsCancellationRequested || !_messagesQueue.IsEmpty)
            {
                using StreamWriter writer = File.AppendText(_path);

                while (_messagesQueue.TryDequeue(out var message))
                {
                    var messageWithDelta = message with { Delta = message.Time - (prevTime ?? message.Time) };
                    var write = writer.WriteLineAsync(messageWithDelta.ToLogString());

                    prevTime = messageWithDelta.Time;
                    App.Current.Dispatcher.Invoke(() => Messages.Insert(0, messageWithDelta));

                    await write;
                }

                writer.Flush();
                await Task.Delay(100);
            }
        }

        public static LogPersister? FromDirectory(string dir, int id)
        {
            var lastLog = Directory.GetFiles(dir, $"{id}.*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f)
                .FirstOrDefault();

            if (lastLog is null)
                return null;

            var persister = new LogPersister(lastLog);
            persister.Stop();

            foreach (var line in File.ReadAllLines(lastLog).Reverse())
            {
                var tokens = line.Split('|', 5);

                if (!DateTime.TryParse(tokens[0].Trim(), out var time))
                    continue;

                if (!TimeSpan.TryParse(tokens[1].Trim(), CultureInfo.CurrentCulture, out var delta))
                    continue;

                persister.Messages.Add(new LogMessage
                {
                    Time = time,
                    Delta = delta,
                    Level = ParseLevel(tokens[2].Trim()),
                    Context = tokens[3].Trim(),
                    Message = tokens.Length > 4 ? tokens[4].Trim() : String.Empty
                });
            }

            return persister;
        }

        private static LogLevel ParseLevel(string level) => level switch
        {
            "STD" => LogLevel.Standard,
            "ERR" => LogLevel.Error,

            _ => throw new NotSupportedException()
        };
    }
}
