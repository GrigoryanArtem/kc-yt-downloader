namespace kc_yt_downloader.Model.Processing;

public static class YtDlpCommands
{
    public static YtDlpCommand DumpJson(string url)
        => new($"--dump-json {url}");

    public static YtDlpCommand Version()
        => new($"--version");
}
