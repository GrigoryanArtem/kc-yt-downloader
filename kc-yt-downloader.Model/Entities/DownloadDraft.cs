namespace kc_yt_downloader.Model.Entities;

public record DownloadDraft
{
    public required int Id { get; set; }
    public required DateTime Created { get; set; }
    public required string Title { get; set; }
    public required CutTaskRequest Request { get; set; }
}
