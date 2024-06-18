using Newtonsoft.Json;

namespace kc_yt_downloader.Model
{
    public record VideoInfo
    (
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("title")] string Title,
        [property: JsonProperty("formats")] IReadOnlyList<VideoFormat> Formats,
        [property: JsonProperty("duration")] int Duration,
        [property: JsonProperty("duration_string")] string DurationString,
        [property: JsonProperty("format_id")] string FormatId,
        [property: JsonProperty("upload_date")] string UploadDate,
        [property: JsonProperty("thumbnails")] IReadOnlyList<Thumbnail> Thumbnails,
        [property: JsonProperty("original_url")] string OriginalUrl,
        [property: JsonProperty("webpage_url")] string WebPageUrl
    );
}
