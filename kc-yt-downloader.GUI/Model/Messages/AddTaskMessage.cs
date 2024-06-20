using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI.Model.Messages
{
    public record AddTaskMessage
    {
        public CutVideoTask Task { get; init; }
    }
}
