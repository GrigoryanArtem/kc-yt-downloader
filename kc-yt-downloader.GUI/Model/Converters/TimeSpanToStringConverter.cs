using System.Globalization;
using System.Windows.Data;

namespace kc_yt_downloader.GUI.Model.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TimeSpan)
                return value;

            TimeSpan valueTimeSpan = (TimeSpan)value;
            return valueTimeSpan switch
            {
                { TotalDays: > 1 } => $"{valueTimeSpan.TotalDays,6:f1}   d",
                { TotalHours: > 1 } => $"{valueTimeSpan.TotalHours,6:f1}   h",
                { TotalMinutes: > 1 } => $"{valueTimeSpan.TotalMinutes,6:f1} min",
                { TotalSeconds: > 1 } => $"{valueTimeSpan.TotalSeconds,6:f1} sec",

                { TotalMilliseconds: > .1 } => $"{valueTimeSpan.TotalMilliseconds,6:f1}  ms",
                _ => String.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
