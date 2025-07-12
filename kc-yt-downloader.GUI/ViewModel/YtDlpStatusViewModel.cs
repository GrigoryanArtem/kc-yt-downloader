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
    private string _speed;

    [ObservableProperty]
    private string _bitRate;

    [ObservableProperty]
    private decimal? _size;

    [ObservableProperty]
    private string _sizeSuffix;

    #endregion

    public YtDlpStatusViewModel()
    {
        _updateFunctions = new()
        {
            { "out_time", s => Time = TimeSpan.Parse(s) },
            { "total_size", s => (TotalSize, TotalSizeSuffix) = SizeConverter.FormatSize(Convert.ToInt64(s)) },
            { "speed", s => Speed = s.EndsWith("x") ? s[..^1] : s },
            { "bitrate", s => BitRate = s.EndsWith("kbits/s") ? s[..^7] : s },
            { "fps", s => Fps = ParseDouble(s) },
            { "frame", s => Frame = ParseInt(s) },
            { "size", s =>  (Size, SizeSuffix) = SizeConverter.ReformatString(s.Trim()) },
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

        return matches.Count != 0;
    }

    private double? ParseDouble(string str)
        => Double.TryParse(str, out var d) ? d : null;

    private int? ParseInt(string str)
        => Int32.TryParse(str, out var d) ? d : null;

    [GeneratedRegex(@"(?<name>\S+)=\s*(?<value>\S+)")]
    private static partial Regex LogRegex();
}
