using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;

        public ObservableObject? CurrentViewModel
            => _navigationStore.CurrentViewModel;
        public ObservableObject? CurrentModalViewModel
            => _modalNavigationStore.CurrentViewModel;
        public bool IsOpen => _modalNavigationStore.IsOpen;

        public MainWindowViewModel(NavigationStore navigationStore, ModalNavigationStore modalNavigationStore)
        {
            _navigationStore = navigationStore;
            _modalNavigationStore = modalNavigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            _modalNavigationStore.CurrentViewModelChanged += OnCurrentModalViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));        
            
            if(CurrentViewModel is not CutViewLoadingViewModel)
                NavigationHistory.Current.Navigate(CurrentViewModel!);
        }

        private void OnCurrentModalViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsOpen));
        }
    }
}