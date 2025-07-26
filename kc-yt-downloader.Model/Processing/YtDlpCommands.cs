using kc_yt_downloader.Model.Tasks;

namespace kc_yt_downloader.Model.Processing;

public static class YtDlpCommands
{
    public static YtDlpCommand DumpJson(string url)
        => new($"--dump-json {url}");

    public static YtDlpCommand Version()
        => new($"--version");

    public static YtDlpCommand PredictExtension(string url, string formatString)
        => new($"""--skip-download --print ext -f "{formatString}" "{url}" """);

    public static YtDlpCommand Download(DownloadVideoTask task)
        => new(task.ToYtDlpArgs());
}
