using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace kc_yt_downloader.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum VideoTaskStatus
{
    Prepared,
    Completed,
    Error,
    Cancelled,
    Unknown,
    Processing,
}
