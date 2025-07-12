using kc_yt_downloader.Model.Utility;

namespace kc_yt_downloader.Model;

public record CutVideoTask
{
    public int Id { get; init; }
    public string Name { get; init; }

    public DateTime Created { get; init; }
    public DateTime? Completed { get; init; }

    public VideoTaskStatus Status { get; init; }
    public string VideoId { get; init; }

    public string URL { get; init; }
    public TimeRange? TimeRange { get; init; }
    public Recode? Recode { get; init; }

    public string FilePath { get; init; }
    public string? PredictedExtension { get; init; }
    public string? PredictedFilePath => 
        PredictedExtension is not null ? String.Join('.', FilePath, PredictedExtension) : null;

    public string? VideoFormatId { get; init; }
    public string? AudioFormatId { get; init; }

    public string FormatString => VideoFormatCombiner.Combine(VideoFormatId, AudioFormatId);

    public string ToArgs()
    {
        var timeRange = TimeRange?.ToArgs() ?? String.Empty;
        var recode = Recode?.ToArgs() ?? String.Empty;

        return $"""-vU --verbose -f "{FormatString}"{timeRange}{recode} "{URL}" -o "{FilePath}.%(ext)s" """;
    }
}
