using System.Globalization;
using System.Windows.Data;
using static kc_yt_downloader.GUI.Model.LogPersister;

namespace kc_yt_downloader.GUI.Model.Converters
{
    public class LogLevelToPipeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LogLevel)
                return value;

            return (LogLevel)value switch
            {
                LogLevel.Standard => "pipe:1",
                LogLevel.Error => "pipe:2",

                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
