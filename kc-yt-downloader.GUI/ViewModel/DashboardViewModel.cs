using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Stores;

namespace kc_yt_downloader.GUI.ViewModel;

public class DashboardViewModel : ObservableObject
{
    private readonly YtDlp _ytDlp;

    public DashboardViewModel(NavigationStore store, YtDlp ytDlp)
    {
        var services = App.Current.Services;

        _ytDlp = ytDlp;
        DlpProxy = services.GetRequiredService<YtDlpProxy>();

        WeakReferenceMessenger.Default.Register<AddTaskMessage>(this, UpdateTasks);
        WeakReferenceMessenger.Default.Register<DeleteTaskMessage>(this, UpdateTasks);

        UpdateTasks();
    }
    
    public YtDlpProxy DlpProxy { get; set; }
    

    private CutTaskViewModel[] _tasks;
    public CutTaskViewModel[] Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    public UrlAddingViewModel UrlAddingViewModel { get; } = new();

    private void UpdateTasks(object sender, DeleteTaskMessage message)
    {
        if (message is not null)
            _ytDlp.DeleteTask(message.Task);

        UpdateTasks();
    }

    private void UpdateTasks(object sender, AddTaskMessage message)
    {
        if (message is not null)
            _ytDlp.AddTask(message.Task);

        UpdateTasks();
    }

    private void UpdateTasks()
        => Tasks = [.. _ytDlp.GetCachedTasks()
            .Where(t => t.Status != VideoTaskStatus.Completed)
            .Select(task => new CutTaskViewModel(task, _ytDlp))
            .OrderByDescending(video => video.Source.Created)];
}
