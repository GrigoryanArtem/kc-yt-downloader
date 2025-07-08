using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.Model;

public class NavigationHistory : ObservableObject
{
    private static readonly Lazy<NavigationHistory> _lazy = new(() => new());
    private readonly Stack<ObservableObject> _viewModels = new();

    private NavigationHistory()
        => _navigateBackCommand = new RelayCommand(NavigateBack, () => _viewModels.Count > 0);

    public static NavigationHistory Current => _lazy.Value;

    public void Navigate(ObservableObject viewModel)
    {
        _viewModels.Push(viewModel);
        _navigateBackCommand.NotifyCanExecuteChanged();
    }


    private readonly RelayCommand _navigateBackCommand;
    public static ICommand NavigateBackCommand => Current._navigateBackCommand;

    public void NavigateBack()
    {
        var services = App.Current.Services;

        var alt = _viewModels.Pop();
        var viewModel = _viewModels.Count > 0 ? _viewModels.Pop() : alt;
        var navigation = new NavigationService<ObservableObject>(services.GetRequiredService<NavigationStore>(), () => viewModel);

        navigation.Navigate();
    }
}