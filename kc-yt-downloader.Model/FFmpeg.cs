using kc_yt_downloader.Model.Enums;
using kc_yt_downloader.Model.Exceptions;
using kc_yt_downloader.Model.Processing;

namespace kc_yt_downloader.Model;

public class FFmpeg
{
    public async Task<string> FFmpegVersion(CancellationToken cancellationToken)
    {
        var version = FFmpegCommands.Version();

        await version.Run(cancellationToken);

        if (version.ExitCode != ProcessExitCode.Success)
            throw new CommandException($"Failed to get yt-dlp version", version);

        var lines = version.Output.Split('\n');
        var versionToken = lines.FirstOrDefault()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2] ?? String.Empty;

        return versionToken;
    }
}
