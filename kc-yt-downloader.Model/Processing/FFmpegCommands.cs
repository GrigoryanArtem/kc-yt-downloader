namespace kc_yt_downloader.Model.Processing;

public static class FFmpegCommands
{
    public static FFmpegCommand Version()
        => new("-version");
}
