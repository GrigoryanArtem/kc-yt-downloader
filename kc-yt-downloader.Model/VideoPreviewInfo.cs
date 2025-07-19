namespace kc_yt_downloader.Model;

public record VideoPreviewInfo
{
    public string Title { get; init; }
    public IReadOnlyList<Thumbnail> Thumbnails { get; init; }
    public int Duration { get; init; }
    public string UploadDate { get; init; }
    public string OriginalUrl { get; init; }
}
