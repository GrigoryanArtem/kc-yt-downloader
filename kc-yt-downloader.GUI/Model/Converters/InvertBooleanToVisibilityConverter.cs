using MaterialDesignThemes.Wpf.Converters;
using System.Windows;

namespace kc_yt_downloader.GUI.Model.Converters;

public class InvertBooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public InvertBooleanToVisibilityConverter() : base(Visibility.Collapsed, Visibility.Visible) { }
}
