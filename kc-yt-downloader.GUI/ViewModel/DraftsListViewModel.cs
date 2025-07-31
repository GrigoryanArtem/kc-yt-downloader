using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using kc_yt_downloader.Model.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class DraftsListViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand), nameof(OpenCommand))]
    private DownloadDraft? _selectedDraft;

    public DraftsListViewModel()
    {
        var services = App.Current.Services;
        var ytDlp = services.GetRequiredService<YtDlp>();

        Drafts = [.. ytDlp.GetDrafts()];
    }

    public ObservableCollection<DownloadDraft> Drafts { get; set; }

    public bool IsDraftSelected => SelectedDraft is not null;
    
    [RelayCommand(CanExecute = nameof(IsDraftSelected))]    
    public void Open()
    {
        var services = App.Current.Services;
        var tasks = services.GetRequiredService<TasksFactory>();
        var cutViewLoadingViewModel = tasks.CreateCutViewLoadingViewModel(SelectedDraft!.Request.Id, new()
        {
            Segments = [.. SelectedDraft.Request.Parts.Select(p => new TimeRange
            {
                From = TimeSpan.FromSeconds(p.Start).ToString("hh\\:mm\\:ss"),
                To = TimeSpan.FromSeconds(p.End).ToString("hh\\:mm\\:ss")
            })],
        });


        var navigation = NavigationCommands.CreateNavigation(cutViewLoadingViewModel);
        navigation.Navigate();

        NavigationCommands.CloseModal();
    }

    [RelayCommand(CanExecute = nameof(IsDraftSelected))]
    public void Delete(DownloadDraft draft)
    {

    }
}
