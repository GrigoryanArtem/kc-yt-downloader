﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class VideoInfoControlViewModel : ObservableObject
{
    private readonly VideoPreview _video;

    public VideoInfoControlViewModel(VideoPreview video)
    {
        var services = App.Current.Services;
        var ytDlpProxy = services.GetRequiredService<YtDlpProxy>();

        _video = video;

        Tasks = [.. ytDlpProxy.GetCachedTasks()
            .Where(t => t.Source.VideoId == video.Id)];

        ThumbnailUrl = _video.Info.Thumbnails
            .OrderByDescending(t => t.Width.HasValue ? 1d - (600d / t.Width) : double.NegativeInfinity)
            .ThenByDescending(t => t.Preference)
            .FirstOrDefault()?.Url;

        _video = video;
    }

    public string Title => _video.Info.Title ?? "Unknown Title";

    public ObservableCollection<CutTaskViewModel> Tasks { get; private set; }
    public string? ThumbnailUrl { get; private set; }

    public DateTime LastUpdatedTime => _video.ParseDate;
    public int Duration => _video.Info.Duration;
    public string UploadDate => _video.Info.UploadDate;
    public string OriginalUrl => _video.Info.OriginalUrl;
    public string[] UsedUrls => _video.AvailableURLs.Distinct().ToArray();


    public void OnOpen()
    {
        if (String.IsNullOrEmpty(_video?.Info?.OriginalUrl))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = _video.Info.OriginalUrl,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    public void DeleteVideo()
    {
        NavigationCommands.CloseModal();

        var proxy = App.Current.Services.GetRequiredService<YtDlpProxy>();
        proxy.DeleteVideo(_video);
    }

    [RelayCommand]
    public void DeleteTask(CutTaskViewModel cutTask)
    {
        var proxy = App.Current.Services.GetRequiredService<YtDlpProxy>();
        proxy.DeleteTask(cutTask.Source);

        Tasks.Remove(cutTask);
    }
}
