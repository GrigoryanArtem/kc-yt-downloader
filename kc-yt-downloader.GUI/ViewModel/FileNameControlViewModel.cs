using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using Microsoft.Win32;
using NavigationMVVM;
using System.IO;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class FileNameControlViewModel : ObservableDisposableObject
    {
        private static SelectedSettings Settings => YtConfig.Global.SelectedSettings;

        public FileNameControlViewModel(string name)
        {
            ChooseWorkingDirectoryCommand = new RelayCommand(async () => await OnChooseWorkingDirectory());

            var invalidChars = Path.GetInvalidFileNameChars().Concat([' ']);
            var ch = name.Select(c => (invalidChars.Contains(c) ? '_' : c))
                .ToArray();
            FileName =  new string(ch);
        }
        
        public string? WorkingDirectory 
        {
            get => Settings.WorkingDirectory;
            set => SetProperty(Settings.WorkingDirectory, value, nv => Settings.WorkingDirectory = nv); 
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public RelayCommand ChooseWorkingDirectoryCommand { get; }

        private async Task OnChooseWorkingDirectory()
        {
            var folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)            
                WorkingDirectory = folderDialog.FolderName;                            
        }

        public string GetFullPath()
            => String.IsNullOrWhiteSpace(WorkingDirectory) ? _fileName : Path.Combine(WorkingDirectory, _fileName);
    }
}
