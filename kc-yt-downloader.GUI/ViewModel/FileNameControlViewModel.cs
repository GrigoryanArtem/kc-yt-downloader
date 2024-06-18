using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NavigationMVVM;
using System.IO;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class FileNameControlViewModel : ObservableDisposableObject
    {
        public FileNameControlViewModel()
        {
            ChooseWorkingDirectoryCommand = new RelayCommand(async () => await OnChooseWorkingDirectory());
        }

        private string _workingDirectory;
        public string WorkingDirectory 
        {
            get => _workingDirectory;
            set => SetProperty(ref _workingDirectory, value); 
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
            => Path.Combine(WorkingDirectory, _fileName);
    }
}
