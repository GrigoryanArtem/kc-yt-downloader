using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using Microsoft.Win32;
using System.IO;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class FileNameControlViewModel : ObservableObject
{
    private static SelectedSettings Settings => YtConfig.Global.SelectedSettings;

    private FileNameControlViewModel()
    {
        CalculateFreeSpace();
    }

    public string? WorkingDirectory
    {
        get => Settings.WorkingDirectory;
        set
        {
            SetProperty(Settings.WorkingDirectory, value, nv => Settings.WorkingDirectory = nv);
            CalculateFreeSpace();
        }
    }

    [ObservableProperty]
    private string _fileName;
    [ObservableProperty]
    private string _availableFreeSpace;

    [RelayCommand]
    private Task ChooseWorkingDirectory()
    {
        var folderDialog = new OpenFolderDialog();

        if (folderDialog.ShowDialog() == true)
            WorkingDirectory = folderDialog.FolderName;

        return Task.CompletedTask;
    }

    public string GetFullPath()
        => String.IsNullOrWhiteSpace(WorkingDirectory) ? _fileName : Path.Combine(WorkingDirectory, _fileName);

    public static FileNameControlViewModel CreateFromName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars().Concat([' ']);
        var ch = name.Select(c => (invalidChars.Contains(c) ? '_' : c))
            .ToArray();

        return new()
        {
            FileName = new string(ch)
        };
    }

    public static FileNameControlViewModel CreateFromPath(string path) => new()
    {
        WorkingDirectory = Path.GetDirectoryName(path),
        FileName = Path.GetFileName(path)
    };

    private void CalculateFreeSpace()
    {        
        var path = Path.GetFullPath(WorkingDirectory ?? ".");
        var drive = Path.GetPathRoot(path)!;
        var driveInfo = new DriveInfo(drive);

        var (size, suffix) = SizeConverter.FormatSize(driveInfo.AvailableFreeSpace, decimalPlaces: 0);
        AvailableFreeSpace = SizeConverter.ToFriendlyString(size, suffix, decimalPlaces: 0);
    }
}
