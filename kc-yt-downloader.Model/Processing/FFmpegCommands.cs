using kc_yt_downloader.Model.Tasks;

namespace kc_yt_downloader.Model.Processing;

public static class FFmpegCommands
{
    public static FFmpegCommand Version()
        => new("-version");

    public static FFmpegCommand Recode(RecodeTask task)
        => new(task.ToCommandArgs());
}
