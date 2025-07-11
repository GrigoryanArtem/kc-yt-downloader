﻿using CommunityToolkit.Mvvm.ComponentModel;
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

    public static IParameterNavigationService<TParameter> CreateNavigation<TParameter, TViewModel>(Func<TParameter, TViewModel> viewModelBuilder)
        where TViewModel : ObservableObject
    {
        var store = Services.GetService<NavigationStore>();
        return new ParameterNavigationService<TParameter, TViewModel>(store!, p => viewModelBuilder(p));
    }
}
