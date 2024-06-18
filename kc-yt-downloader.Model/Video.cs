using Newtonsoft.Json;

namespace kc_yt_downloader.Model
{
    public record Video
    {
        private readonly Lazy<VideoInfo?> _info;

        public Video(string json, VideoInfo? info, params string[] URLs) 
        {
            AvailableURLs = URLs ?? [];

            Id = info.Id;
            ParseDate = DateTime.Now;

            Json = json;

            _info = new Lazy<VideoInfo?>(() => info);
        }

        public Video()
            => _info = new Lazy<VideoInfo?>(ParseJson);            
        
        public string[] AvailableURLs { get; init; }
        public string Id { get; init; }
        public DateTime ParseDate { get; init; }
        public string Json { get; init; }

        public VideoInfo? Info => _info.Value;

        public static VideoInfo? ParseJson(string json)
            => json is not null ? JsonConvert.DeserializeObject<VideoInfo>(json) : null;

        private VideoInfo? ParseJson()
            => ParseJson(Json);
        
    }
}
