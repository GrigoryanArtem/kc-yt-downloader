using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.Model;

public record CutViewModelParameters
{
    public CutVideoBatch Batch { get; set; }
    public Video Video { get; init; }
}
