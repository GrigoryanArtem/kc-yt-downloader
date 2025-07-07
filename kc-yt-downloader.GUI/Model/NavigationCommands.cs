using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Commands;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.Model;

public static class NavigationCommands
{
    private readonly static CloseModalNavigationService _closeModal;
    private static IServiceProvider Services => App.Current.Services;

    static NavigationCommands()
    {        
        _closeModal = Services.GetRequiredService<CloseModalNavigationService>();

        CloseModalCommand = new NavigateCommand(_closeModal);
    }

    public static ICommand NavigateBackCommand => NavigationHistory.NavigateBackCommand;
    public static ICommand CloseModalCommand { get; }

    public static void CloseModal()
        => _closeModal.Navigate();

    public static void NavigateBack()
        => NavigationHistory.Current.NavigateBack();

    public static INavigationService CreateNavigation<TViewModel>()
        where TViewModel : ObservableObject
    {
        var store = Services.GetService<NavigationStore>();
        return new NavigationService<TViewModel>(store!, Services.GetRequiredService<TViewModel>);
    }

    public static IParameterNavigationService<TParameter> CreateNavigation<TParameter, TViewModel>(Func<TParameter, TViewModel> viewModelBuilder)
        where TViewModel : ObservableObject
    {
        var store = Services.GetService<NavigationStore>();
        return new ParameterNavigationService<TParameter, TViewModel>(store!, p => viewModelBuilder(p));
    }

    public static IParameterNavigationService<TParameter> CreateModalNavigation<TParameter, TViewModel>(Func<TParameter, TViewModel> viewModelBuilder)
        where TViewModel : ObservableObject
    {
        var store = Services.GetService<ModalNavigationStore>();
        return new ParameterNavigationService<TParameter, TViewModel>(store!, p => viewModelBuilder(p));
    }

    public static INavigationService CreateModalNavigation<TViewModel>(TViewModel viewModel)
        where TViewModel : ObservableObject
    {
        var store = Services.GetService<ModalNavigationStore>();
        return new NavigationService<TViewModel>(store!, () => viewModel);
    }
}
