namespace kc_yt_downloader.Model.Tasks;

public abstract record TaskBase
{
    public long Id { get; init; }
    public required string VideoId { get; init; }
}
