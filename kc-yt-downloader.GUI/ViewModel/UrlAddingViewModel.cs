﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using Microsoft.Extensions.DependencyInjection;

namespace kc_yt_downloader.GUI.ViewModel;

public class UrlAddingViewModel : ObservableObject
{
    public UrlAddingViewModel()
        => AddUrlCommand = new RelayCommand(async () => await OnAddUrlAsync());


    private string? _url;
    public string? Url
    {
        get => _url;
        set
        {
            SetProperty(ref _url, value);
            OnPropertyChanged(nameof(IsAddButtonEnable));
        }
    }

    public bool IsAddButtonEnable => !IsProgress && !String.IsNullOrEmpty(Url);

    private bool _isProgress;
    public bool IsProgress
    {
        get => _isProgress;
        set
        {
            SetProperty(ref _isProgress, value);
            OnPropertyChanged(nameof(IsAddButtonEnable));
        }
    }
    public RelayCommand AddUrlCommand { get; }

    public Task OnAddUrlAsync()
        => Task.Run(OnAddUrl);

    public async Task OnAddUrl()
    {
        IsProgress = true;

        var url = Url;
        Url = String.Empty;

        var ytDlp = App.Current.Services.GetRequiredService<YtDlpProxy>();
        await ytDlp.GetVideo(url!, CancellationToken.None);

        IsProgress = false;
    }
}
