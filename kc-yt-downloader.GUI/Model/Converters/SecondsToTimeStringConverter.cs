using System.Globalization;
using System.Windows.Data;

namespace kc_yt_downloader.GUI.Model.Converters;

public class SecondsToTimeStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int intValue)
            return value;
        
        var time = TimeSpan.FromSeconds(intValue);
        return time.ToString("hh\\:mm\\:ss");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
