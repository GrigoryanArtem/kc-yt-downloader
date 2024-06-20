namespace kc_yt_downloader.Model
{
    public record Recode
    {
        public static string[] FORMATS { get; } = 
        [
            "avi", "flv", "gif", "mkv", "mov",
            "mp4", "webm", "aac", "aiff", "alac",
            "flac", "m4a", "mka", "mp3", "ogg",
            "opus", "vorbis", "wav"
        ];

        public string Format { get; init; }

        public string ToArgs()
            => $" --recode-video {Format}";
    }
}
