using kc_yt_downloader.GUI.Model;
using NavigationMVVM;
using NavigationMVVM.Commands;
using NavigationMVVM.Services;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class LogViewModel : ObservableDisposableObject
    {
        public record LogViewModelParameters
        {
            public LogPersister Persister { get; init; }
            public NavigationService<ObservableDisposableObject> BackNavigation { get; init; }
        }

        private readonly LogPersister _persister;

        public LogViewModel(LogViewModelParameters parameters)
        {
            _persister = parameters.Persister;

            BackCommand = new NavigateCommand(parameters.BackNavigation);
        }

        public LogPersister Persister => _persister;
        public ICommand BackCommand { get; }
    }
}
