namespace kc_yt_downloader.GUI.Model;

public static class SizeConverter
{
    private static readonly string[] SIZE_SUFFIXES = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

    public static (decimal size, string suffix) ReformatString(string sizeString, int decimalPlaces = 1)
        => FormatSize(ParseSizeString(sizeString), decimalPlaces);

    public static long ParseSizeString(string sizeString)
    {
        var trimmedSizeString = sizeString.ToUpper().Trim();
        var suffix = SIZE_SUFFIXES.Reverse()
            .FirstOrDefault(trimmedSizeString.EndsWith);

        int? index = suffix is not null ? Array.IndexOf(SIZE_SUFFIXES, suffix) : null;
        return index.HasValue 
            ? Convert.ToInt64(sizeString[..^suffix!.Length]) * (long)Math.Pow(1024, index.Value) 
            : Convert.ToInt64(sizeString);
    }

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
