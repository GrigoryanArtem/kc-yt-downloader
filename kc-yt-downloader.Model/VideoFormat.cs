using Newtonsoft.Json;

namespace kc_yt_downloader.Model
{
    public record VideoFormat
    (
        [property: JsonProperty("asr")] object Asr,
        [property: JsonProperty("filesize")] long? FileSize,
        [property: JsonProperty("format_id")] string FormatId,
        [property: JsonProperty("format_note")] string FormatNote,
        [property: JsonProperty("source_preference")] int? SourcePreference,
        [property: JsonProperty("fps")] double? Fps,
        [property: JsonProperty("audio_channels")] object? AudioChannels,
        [property: JsonProperty("height")] int? Height,
        [property: JsonProperty("quality")] double? Quality,
        [property: JsonProperty("has_drm")] bool? HasDrm,
        [property: JsonProperty("tbr")] double? Tbr,
        [property: JsonProperty("filesize_approx")] long? FileSizeApprox,
        [property: JsonProperty("url")] string Url,
        [property: JsonProperty("width")] int? Width,
        [property: JsonProperty("language")] object Language,
        [property: JsonProperty("language_preference")] int? LanguagePreference,
        [property: JsonProperty("preference")] object Preference,
        [property: JsonProperty("ext")] string Ext,
        [property: JsonProperty("vcodec")] string VCodec,
        [property: JsonProperty("acodec")] string ACodec,
        [property: JsonProperty("dynamic_range")] string DynamicRange,
        [property: JsonProperty("container")] string Container,
        [property: JsonProperty("protocol")] string Protocol,
        [property: JsonProperty("resolution")] string Resolution,
        [property: JsonProperty("aspect_ratio")] double? AspectRatio,
        [property: JsonProperty("video_ext")] string VideoExt,
        [property: JsonProperty("audio_ext")] string AudioExt,
        [property: JsonProperty("abr")] double? Abr,
        [property: JsonProperty("vbr")] double? Vbr,
        [property: JsonProperty("format")] string Format
    );
}
