using Newtonsoft.Json;

namespace kc_yt_downloader.Model
{
    public record VideoInfo
    (
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("title")] string Title,
        [property: JsonProperty("formats")] IReadOnlyList<VideoFormat> Formats,
        [property: JsonProperty("duration")] int Duration,
        [property: JsonProperty("upload_date")] string UploadDate,
        [property: JsonProperty("thumbnails")] IReadOnlyList<Thumbnail> Thumbnails
    );
}
