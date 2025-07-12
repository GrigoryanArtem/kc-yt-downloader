namespace kc_yt_downloader.Model.Utility;
public static class VideoFormatCombiner
{
    public static string Combine(string? videoFormatId, string? audioFormatId)
        => $"{videoFormatId ?? "bv"}+{audioFormatId ?? "ba"}";    
}
