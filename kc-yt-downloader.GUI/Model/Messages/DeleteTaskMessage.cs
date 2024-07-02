using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.Model.Messages
{
    public record DeleteTaskMessage
    {
        public CutVideoTask Task { get; init; }
    }
}
