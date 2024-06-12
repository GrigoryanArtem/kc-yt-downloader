using Newtonsoft.Json;

namespace kc_yt_downloader.Model
{
    public record Thumbnail
    (
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("preference")] int Preference,
        [property: JsonProperty("url")] string Url,

        [property: JsonProperty("width")] int? Width,
        [property: JsonProperty("height")] int? Height,
        [property: JsonProperty("resolution")] string Resolution
    );
}
