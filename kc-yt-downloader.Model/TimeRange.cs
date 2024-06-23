namespace kc_yt_downloader.Model
{
    public record TimeRange
    {
        public string From { get; init; }
        public string To { get; init; }

        public int GetDuration()
            => (int)(ParseTime(To) - ParseTime(From)).TotalSeconds;

        private static TimeSpan ParseTime(string timeString)
        {
            var dotSplit = timeString.Split(".");

            var time = dotSplit[0];
            var milliseconds = dotSplit.Length > 1 ? Convert.ToInt32(dotSplit[1]) : 0;

            var tokens = time.Split(":").Reverse().ToArray();

            var seconds = Convert.ToInt32(tokens[0]);
            var minutes = tokens.Length > 1 ? Convert.ToInt32(tokens[1]) : 0;
            var hours = tokens.Length > 2 ? Convert.ToInt32(tokens[1]) : 0;

            return new TimeSpan(0, hours, minutes, seconds, milliseconds * 10);
        }

        public string ToArgs()
            => $" --external-downloader ffmpeg --external-downloader-args \"ffmpeg_i:-loglevel error -progress pipe:2 -nostats -ss {From} -to {To}\"";
    }
}
