using CommunityToolkit.Mvvm.Input;
using NavigationMVVM;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class VideFormatSelectorViewModel : ObservableDisposableObject
    {
        public VideFormatSelectorViewModel(VideoFormatViewModel[] videoFormats, string targetId)
        {
            Formats = videoFormats;

            SelectFormatCommand = new((vf) => 
            {
                Array.ForEach(Formats, f => f.IsSelected = false);

                vf.IsSelected = true;
                SelectedFormat = vf;
            });

            SelectedFormat = Formats.FirstOrDefault(f => targetId is null || f.Id == targetId);
            if(SelectedFormat is not null)
                SelectedFormat .IsSelected = true;
        }

        public VideoFormatViewModel[] Formats { get; set; }
        public VideoFormatViewModel SelectedFormat { get; set; }

        public RelayCommand<VideoFormatViewModel> SelectFormatCommand { get; }
    }
}
