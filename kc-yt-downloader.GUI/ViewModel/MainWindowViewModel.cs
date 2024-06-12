using CommunityToolkit.Mvvm.ComponentModel;
using NavigationMVVM;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;

        public ObservableDisposableObject? CurrentViewModel
            => _navigationStore.CurrentViewModel;
        public ObservableDisposableObject? CurrentModalViewModel
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
        }

        private void OnCurrentModalViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsOpen));
        }
    }
}