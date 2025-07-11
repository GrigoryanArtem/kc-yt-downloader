namespace kc_yt_downloader.Model;

public record TimeRange
{
    public string From { get; init; }
    public string To { get; init; }

    public int GetDuration()
        => (int)(TimeParser.ParseTime(To) - TimeParser.ParseTime(From)).TotalSeconds;

    public string ToArgs()
        => $" --download-sections *{From}-{To}";
}
