using kc_yt_downloader.Model;
using System.Globalization;
using System.Windows.Data;

namespace kc_yt_downloader.GUI.Model.Converters;

internal class TimeRangeRequestToDurationString : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeRangeRequest request)
            return value;

        var diff = TimeSpan.FromSeconds(request.End) - TimeSpan.FromSeconds(request.Start);
        return diff switch
        {
            { TotalDays: > 1 } => $"{diff.TotalDays,6:f1}   d",
            { TotalHours: > 1 } => $"{diff.TotalHours,6:f1}   h",
            { TotalMinutes: > 1 } => $"{diff.TotalMinutes,6:f1} min",
            { TotalSeconds: > 1 } => $"{diff.TotalSeconds,6:f1} sec",

            { TotalMilliseconds: > .1 } => $"{diff.TotalMilliseconds,6:f1}  ms",
            _ => String.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}