namespace kc_yt_downloader.Model;

public record TimeRangeRequest(int Start, int End);
public record CutTaskRequest(string Id, TimeRangeRequest[] Parts);
