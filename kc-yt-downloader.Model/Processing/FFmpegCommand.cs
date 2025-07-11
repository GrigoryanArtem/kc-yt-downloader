using kc_yt_downloader.Model.Enums;

namespace kc_yt_downloader.Model.Processing;

public class FFmpegCommand(string arguments) : CommandBase("ffmpeg", arguments)
{
    protected override ProcessExitCode MapExitCode(int exitCode) => exitCode switch
    {
        0 => ProcessExitCode.Success,
        _ => ProcessExitCode.Unknown
    };
}
