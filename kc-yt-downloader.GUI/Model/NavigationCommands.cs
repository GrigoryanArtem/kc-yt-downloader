using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Commands;
using NavigationMVVM.Services;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.Model;

public static class NavigationCommands
{
    private readonly static CloseModalNavigationService _closeModal;

    static NavigationCommands()
    {
        var services = App.Current.Services;
        _closeModal = services.GetRequiredService<CloseModalNavigationService>();

        CloseModalCommand = new NavigateCommand(_closeModal);
    }

    public static ICommand NavigateBackCommand => NavigationHistory.NavigateBackCommand;
    public static ICommand CloseModalCommand { get; }
    public static void CloseModal()
        => _closeModal.Navigate();
}
