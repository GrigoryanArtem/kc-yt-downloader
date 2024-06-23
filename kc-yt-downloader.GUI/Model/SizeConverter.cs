namespace kc_yt_downloader.GUI.Model
{
    public static class SizeConverter
    {
        private static readonly string[] SIZE_SUFFIXES = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

        public static (decimal size, string suffix) FormatSize(long? value, int decimalPlaces = 1)
        {
            if (!value.HasValue)
                return (0, SIZE_SUFFIXES.First());

            if (value < 0)
            {
                var (size, suffix) = FormatSize(-value, decimalPlaces);
                return (-size, suffix);
            }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return (dValue, SIZE_SUFFIXES[i]);
        }

        public static string ToFriendlyString(decimal size, string suffix, int decimalPlaces = 1)
            => string.Format("{0:n" + decimalPlaces + "} {1}", size, suffix);

        public static string ToFriendlyString(long? value, int decimalPlaces = 1)
        {
            var (size, suffix) = FormatSize(value, decimalPlaces);
            return ToFriendlyString(size, suffix, decimalPlaces);
        }
    }
}
