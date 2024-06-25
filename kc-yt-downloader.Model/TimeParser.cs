namespace kc_yt_downloader.Model
{
    public static class TimeParser
    {
        public static TimeSpan ParseTime(string timeString)
        {
            var dotSplit = timeString.Split(".");

            var time = dotSplit[0];
            var milliseconds = dotSplit.Length > 1 ? Convert.ToInt32(dotSplit[1]) : 0;

            var tokens = time.Split(":").Reverse().ToArray();

            var seconds = Convert.ToInt32(tokens[0]);
            var minutes = tokens.Length > 1 ? Convert.ToInt32(tokens[1]) : 0;
            var hours = tokens.Length > 2 ? Convert.ToInt32(tokens[2]) : 0;

            return new TimeSpan(0, hours, minutes, seconds, milliseconds * 10);
        }
    }
}
