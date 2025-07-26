using kc_yt_downloader.Model.Enums;

namespace kc_yt_downloader.Model;

public record Recode
{
    public static string[] FORMATS { get; } =
    [
        "avi", "flv", "gif", "mkv", "mov",
        "mp4", "webm", "aac", "aiff", "alac",
        "flac", "m4a", "mka", "mp3", "ogg",
        "opus", "vorbis", "wav"
    ];

    public required string Format { get; init; }
    public required FFmpegPreset Preset { get; init; }
}
