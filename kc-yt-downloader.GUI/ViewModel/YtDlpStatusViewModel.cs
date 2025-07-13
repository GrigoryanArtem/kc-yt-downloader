using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using System.Text.RegularExpressions;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class YtDlpStatusViewModel : ObservableObject
{
    private readonly  Dictionary<string, Action<string>> _updateFunctions;

    #region Properties


    [ObservableProperty]
    private int? _frame;

    [ObservableProperty]
    private TimeSpan? _time;

    [ObservableProperty]
    private double? _fps;

    [ObservableProperty]
    private string _totalSizeSuffix;

    [ObservableProperty]
    private decimal? _totalSize;

    [ObservableProperty]
    private double _speed;

    [ObservableProperty]
    private string _bitRate;

    [ObservableProperty]
    private decimal? _size;

    [ObservableProperty]
    private string _sizeSuffix;

    [ObservableProperty]
    private TimeSpan? _remaining;

    [ObservableProperty]
    private double _donePercent;

    private readonly TimeSpan _totalDuration;

    #endregion

    public YtDlpStatusViewModel(TimeSpan totalDuration)
    {
        _totalDuration = totalDuration;
        _updateFunctions = new()
        {
            { "time", s => Time = TimeSpan.Parse(s) }, // yt-dlp
            { "out_time", s => Time = TimeSpan.Parse(s) }, // ffmpeg

            { "size", s =>  (Size, SizeSuffix) = SizeConverter.ReformatString(s.Trim()) }, // yt-dlp
            { "total_size", s => (TotalSize, TotalSizeSuffix) = SizeConverter.FormatSize(Convert.ToInt64(s)) }, // ffmpeg

            { "speed", s => Speed = Double.TryParse(s.EndsWith("x") ? s[..^1] : s, out var val) ? val: .0 },
            { "bitrate", s => BitRate = s.EndsWith("kbits/s") ? s[..^7] : s },
            { "fps", s => Fps = ParseDouble(s) },
            { "frame", s => Frame = ParseInt(s) },
        };
    }


    public bool TryUpdate(string propertyString)
    {
        var matches = LogRegex().Matches(propertyString);

        foreach (Match match in matches)
        {
            var name = match.Groups["name"].Value;
            var value = match.Groups["value"].Value;

            if (!_updateFunctions.ContainsKey(name))
                continue;

            _updateFunctions[name](value);
        }

        if (matches.Count > 0)
            UpdateCalculatedProperties();

        return matches.Count != 0;
    }

    private void UpdateCalculatedProperties()
    {
        DonePercent = Time.HasValue ? (Time.Value.TotalSeconds / _totalDuration.TotalSeconds) * 100.0 : .0;

        if(Speed > 0)
            Remaining = Time.HasValue ? (_totalDuration - Time.Value) / Speed : null;
    }

    #region Private methods

    private static double? ParseDouble(string str)
        => Double.TryParse(str, out var d) ? d : null;

    private static int? ParseInt(string str)
        => Int32.TryParse(str, out var d) ? d : null;

    [GeneratedRegex(@"(?<name>\S+)=\s*(?<value>\S+)")]
    private static partial Regex LogRegex();

    #endregion
}
