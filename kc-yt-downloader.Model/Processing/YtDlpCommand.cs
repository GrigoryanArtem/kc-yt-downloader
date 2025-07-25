using kc_yt_downloader.Model.Enums;

namespace kc_yt_downloader.Model.Processing;

public class YtDlpCommand(string arguments) : CommandBase("yt-dlp", arguments)
{
    protected override ProcessExitCode MapExitCode(int exitCode) => exitCode switch
    {
        0 => ProcessExitCode.Success,
        1 or 2 => ProcessExitCode.Error,
        101 => ProcessExitCode.Cancelled,

        _ => ProcessExitCode.Unknown
    };
}
