namespace kc_yt_downloader.Model;

public record VideoPreview
{
    public string Id { get; init; }
    public string[] AvailableURLs { get; init; }
    public DateTime ParseDate { get; init; }
    public VideoPreviewInfo Info { get; init; }
}