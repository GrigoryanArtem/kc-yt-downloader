namespace kc_yt_downloader.GUI.Model;

public class SelectedSettings
{
    public string? WorkingDirectory { get; set; }
    public string? RecodeFormat { get; set; }

    public bool FullVideo { get; set; }
    public bool NeedRecode { get; set; }
}
