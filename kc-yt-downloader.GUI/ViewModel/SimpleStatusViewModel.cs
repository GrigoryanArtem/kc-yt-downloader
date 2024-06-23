using kc_yt_downloader.Model;
using NavigationMVVM;
using System.Windows.Media;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class SimpleStatusViewModel : ObservableDisposableObject
    {
        public SimpleStatusViewModel(VideoTaskStatus status)
        {
            Status = status.ToString();
            StatusColor = status switch
            {
                VideoTaskStatus.Completed => Brushes.Green,
                VideoTaskStatus.Prepared => Brushes.SandyBrown,
                VideoTaskStatus.Error => Brushes.DarkRed,

                _ => Brushes.Black
            };
        }

        public string Status { get; }
        public Brush StatusColor { get; }
    }
}
