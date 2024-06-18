using System.Windows.Controls;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.View.Controls.Base
{
    public class SmartScrollViewer : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
                base.OnMouseWheel(e);
        }
    }
}
