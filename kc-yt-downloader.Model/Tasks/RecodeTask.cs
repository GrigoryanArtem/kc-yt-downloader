using kc_yt_downloader.Model.Enums;

namespace kc_yt_downloader.Model.Tasks;

public record RecodeTask : TaskBase
{    
    public required string InputFile { get; init; }
    public required string OutputFile { get; init; }
    public required string Format { get; init; }
    public required FFmpegPreset Preset { get; init; }

    public string ToCommandArgs()
        => $"-i {InputFile} -c:v libx264 -preset {Preset.ToString().ToLower()} {OutputFile}.{Format}";
}
