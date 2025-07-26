using kc_yt_downloader.Model.Utility;

namespace kc_yt_downloader.Model.Tasks;

public record DownloadVideoTask : TaskBase
{
    public string Name { get; init; }

    public DateTime Created { get; init; }

    public float? SpeedMedian { get; init; }
    public DateTime? Completed { get; init; }

    public VideoTaskStatus Status { get; init; }

    public string URL { get; init; }
    public TimeRange? TimeRange { get; init; }
    public Recode? Recode { get; init; }

    public string FilePath { get; init; }
    public string? PredictedExtension { get; init; }
    public string? PredictedFilePath =>
        PredictedExtension is not null ? string.Join('.', FilePath, PredictedExtension) : null;

    public string? VideoFormatId { get; init; }
    public string? AudioFormatId { get; init; }

    public string FormatString => VideoFormatCombiner.Combine(VideoFormatId, AudioFormatId);

    public string ToYtDlpArgs()
    {
        var timeRange = TimeRange?.ToArgs() ?? string.Empty;        

        return $"""-vU --verbose -f "{FormatString}"{timeRange} "{URL}" -o "{FilePath}.%(ext)s" """;
    }
}
