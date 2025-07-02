using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel;

using NavigationCommands = Model.NavigationCommands;

public class LogViewModel(LogViewModel.LogViewModelParameters parameters) : ObservableObject
{
    public record LogViewModelParameters
    {
        public LogPersister Persister { get; init; }
    }

    private readonly LogPersister _persister = parameters.Persister;

    public LogPersister Persister => _persister;
    public ICommand BackCommand => NavigationCommands.CloseModalCommand;
}
