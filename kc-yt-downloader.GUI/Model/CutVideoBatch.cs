using kc_yt_downloader.Model;
using kc_yt_downloader.Model.Tasks;

namespace kc_yt_downloader.GUI.Model;

public class CutVideoBatch
{
    public CutVideoBatch() { }

    public CutVideoBatch(DownloadVideoTask source)
    {
        Name = source.Name;
        VideoId = source.VideoId;
        URL = source.URL;
        Recode = source.Recode;
        FilePath = source.FilePath;

        VideoFormatId = source.VideoFormatId;
        AudioFormatId = source.AudioFormatId;

        Segments = source.TimeRange is null ? [] : [source.TimeRange];
    }

    public string Name { get; init; }
    public string VideoId { get; init; }

    public string URL { get; init; }
    public TimeRange[] Segments { get; init; } = [];
    public Recode? Recode { get; init; }

    public string FilePath { get; init; }

    public string? VideoFormatId { get; init; }
    public string? AudioFormatId { get; init; }
}
