using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using kc_yt_downloader.Model.Enums;
using kc_yt_downloader.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using System.IO;
using System.Windows.Input;
using static kc_yt_downloader.GUI.ViewModel.LogViewModel;

namespace kc_yt_downloader.GUI.ViewModel;

using NavigationCommands = Model.NavigationCommands;

public partial class CutTaskViewModel : ObservableObject
{
    #region Constants

    private const string LOGS_DIRECTORY = "logs";

    #endregion

    private readonly ILogger<CutTaskViewModel> _logger;
    private readonly YtDlpProxy _ytDlpProxy;
    private readonly YtDlp _ytDlp;
    private readonly VideoPreview _video;
    private readonly TimeSpan _totalDuration;

    private readonly YtDlpStatusViewModel _ytDlpStatus;
    private readonly string _logsDirectory;

    private bool _canShowStatus = false;
    private CancellationTokenSource? _cancellationTokenSource;

    public CutTaskViewModel(DownloadVideoTask task, YtDlpProxy proxy)
    {
        var services = App.Current.Services;

        _ytDlpProxy = proxy;
        _ytDlp = services.GetRequiredService<YtDlp>();
        _logger = services.GetRequiredService<ILogger<CutTaskViewModel>>();

        Source = task;

        _video = _ytDlp.GetPreviewVideoByUrl(task.URL);
        _totalDuration = TimeSpan.FromSeconds(task.TimeRange?.GetDuration() ?? _video.Info.Duration);
        _ytDlpStatus = new(_totalDuration);
        _logsDirectory = Path.Combine(YtConfig.Global.DataDirectory, LOGS_DIRECTORY);

        DonePercent = task.Status == VideoTaskStatus.Completed ? 100 : 0;
        Status = new SimpleStatusViewModel(task.Status);

        if (_video is not null)
        {
            var tasks = services.GetRequiredService<TasksFactory>();
            var cutViewLoadingViewModel = tasks.CreateCutViewLoadingViewModel(_video!.Info.OriginalUrl, new(task));


            var store = services.GetRequiredService<NavigationStore>();
            var navigation = new NavigationService<CutViewLoadingViewModel>(store, () => cutViewLoadingViewModel);

            EditTaskCommand = new RelayCommand(navigation.Navigate);
        }

        var logNavigation = NavigationCommands.CreateModalNavigation<LogViewModelParameters, LogViewModel>(p => new LogViewModel(p));
        OpenLogCommand = new RelayCommand
           (
               execute: () => logNavigation.Navigate(new()
               {
                   Persister = _persister
               }),
               canExecute: () => _persister is not null
           );

        Persister = LogPersister.FromDirectory(_logsDirectory, task.Id)!;

        if (_ytDlpProxy.TryGetAvgFormatSpeed(task.FormatString, out var speed))
            EstimatedTime = FormatTimeSpan(_totalDuration / speed);
    }

    #region Properties

    #region Observable

    [ObservableProperty]    
    private bool _isRunning = false;

    [ObservableProperty]
    private double _donePercent;

    [ObservableProperty]
    private string _taskStatus;

    public string FileName => Path.GetFileName(Source.PredictedFilePath ?? Source.FilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TimeRangeString))]
    private DownloadVideoTask _source;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenLogCommand))]
    private LogPersister _persister;

    [ObservableProperty]
    private ObservableObject _status;

    #endregion

    public string? EstimatedTime { get; init; }

    public string? TimeRangeString
        => $"{Source?.TimeRange?.From:hh\\:mm\\:ss} - {Source?.TimeRange?.To:hh\\:mm\\:ss}";

    public bool HasEditMode => EditTaskCommand is not null;

    #endregion

    #region Commands
    
    public ICommand EditTaskCommand { get; }
    public RelayCommand OpenLogCommand { get; }

    #endregion

    [RelayCommand]
    public async Task Execute()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        App.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = true;

            Directory.CreateDirectory(_logsDirectory);

            Persister?.Dispose();
            Persister = new LogPersister(Path.Combine(_logsDirectory, $"{Source.Id}.{DateTime.Now:yyyyMMdd_HHmmss}.log"));
        });

        await TaskCycle();

        Persister.Stop();
        App.Current.Dispatcher.Invoke(() => IsRunning = false);
    }

    private void UpdateStatus(string str)
    {
        if (str is null)
            return;

        if (_ytDlpStatus.TryUpdate(str) && !_canShowStatus)
        {
            Status = _ytDlpStatus;
            _canShowStatus = true;
        }

        Persister.Write(LogPersister.LogLevel.Error, str);
        DonePercent = _ytDlpStatus.DonePercent;
    }


    [RelayCommand]
    private void OpenDirectory()
    {
        var filePath = Source.PredictedFilePath ?? Source.FilePath;
        var dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
        var fileName = Path.GetFileName(filePath);

        if (File.Exists(filePath))
        {
            ExplorerHelper.OpenFolderAndSelectItem(dir, fileName);
        }
        else
        {
            ExplorerHelper.OpenFolder(dir);
        }
    }

    [RelayCommand]
    private void DeleteTask()
    {
        var services = App.Current.Services;
        var ytDlp = services.GetRequiredService<YtDlpProxy>();
        ytDlp.DeleteTask(Source);
    }

    private async Task TaskCycle()
    {
        var commands = _ytDlp.CreateRunCommands(Source.Id);
        var status = VideoTaskStatus.Unknown;

        foreach (var (command, stage) in commands) 
        { 
            _logger.LogInformation("Running task {taskId} with command: {command}", Source.Id, command.ProcessCommand);
            _ytDlpStatus.UpdateStage(stage);

            try
            {
                _canShowStatus = false;

                Status = new LoadingViewModel();

                Source = Source with
                {
                    Status = VideoTaskStatus.Processing,
                };

                _ytDlp.UpdateTask(Source);
                _ytDlpProxy.Sync(YtDlpProxy.SyncType.Tasks);

                command.OnErrorUpdate = e => UpdateStatus(e.Data!);
                command.OnOutputUpdate = e => Persister.Write(LogPersister.LogLevel.Standard, e.Data!);

                await command.Run(_cancellationTokenSource!.Token);
            }
            catch (Exception exp)
            {
                GlobalSnackbarMessageQueue.WriteError("Error while running task", exp);
                _logger.LogError(exp, "Error while running task {taskId}", Source.Id);
            }

            status = command.ExitCode switch
            {
                ProcessExitCode.Success => VideoTaskStatus.Completed,
                ProcessExitCode.Error => VideoTaskStatus.Error,
                ProcessExitCode.Cancelled => VideoTaskStatus.Cancelled,

                _ => VideoTaskStatus.Unknown
            };

            _logger.LogInformation("Task {taskId} finished with exitCode: {exitCode} and status: {status}", Source.Id, command.ExitCode, status);

            if (status != VideoTaskStatus.Completed)
                break;
        }

        Source = Source with
        {
            Status = status,
            Completed = DateTime.Now,
            SpeedMedian = status == VideoTaskStatus.Completed ? _ytDlpStatus.GetSpeedMedian() : null, 
        };

        _ytDlp.UpdateTask(Source);
        _ytDlpProxy.Sync(YtDlpProxy.SyncType.Tasks);

        Status = new SimpleStatusViewModel(status);
    }

    [RelayCommand]
    public void Stop()
    {
        _logger.LogInformation("Stopping task {taskId}", Source.Id);
        _cancellationTokenSource?.Cancel();
    }

    private static string FormatTimeSpan(TimeSpan timeSpan) => string.Join(" ", new[]
    {
        FormatPart((int)timeSpan.TotalHours, "h."),
        FormatPart(timeSpan.Minutes, "min."),
        FormatPart(timeSpan.Seconds, "sec.")
    }.Where(p => p is not null));


    private static string? FormatPart(int quantity, string name)
        => quantity > 0 ? $"{quantity} {name}" : null;
}