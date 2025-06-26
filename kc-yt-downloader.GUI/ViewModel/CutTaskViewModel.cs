using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using NavigationMVVM.Services;
using NavigationMVVM.Stores;
using System.IO;
using System.Windows.Input;
using static kc_yt_downloader.GUI.ViewModel.LogViewModel;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class CutTaskViewModel : ObservableObject
{
    private const string LOGS_DIRECTORY = "logs";

    private YtDlp _ytDlp;
    private readonly VideoPreview _video;
    private readonly TimeSpan _totalDuration;

    private readonly YtDlpStatusViewModel _ytDlpStatus;
    private readonly string _logsDirectory;

    private bool _canShowStatus = false;

    public CutTaskViewModel(CutVideoTask task,
        YtDlp ytDlp)
    {
        _ytDlp = ytDlp;
        Source = task;

        _ytDlpStatus = new();

        _video = _ytDlp.GetPreviewVideoByUrl(task.URL);

        _totalDuration = TimeSpan.FromSeconds(task.TimeRange?.GetDuration() ?? _video.Info.Duration);
        _logsDirectory = Path.Combine(YtConfig.Global.CacheDirectory, LOGS_DIRECTORY);

        DonePercent = task.Status == VideoTaskStatus.Completed ? 100 : 0;

        Status = new SimpleStatusViewModel(task.Status);

        RunCommand = new RelayCommand(async () => await OnRun(), () => !IsRunning);
        OpenDirectoryCommand = new RelayCommand(OnOpenDirectory);

        var cutViewLoadingViewModel = new CutViewLoadingViewModel(() => Task.Run(() =>
        {
            {
                var video = _ytDlp.GetVideoByUrl(_video.Info.OriginalUrl);

                return new CutViewModelParameters()
                {
                    Batch = new(task),
                    Video = video
                };
            }
        }));

        var services = App.Current.Services;
        var store = services.GetRequiredService<NavigationStore>();
        var navigation = new NavigationService<CutViewLoadingViewModel>(store, () => cutViewLoadingViewModel);

        var logNavigation = new ParameterNavigationService<LogViewModelParameters, LogViewModel>(store, p => new LogViewModel(p));

        EditTaskCommand = new RelayCommand(navigation.Navigate);
        OpenLogCommand = new RelayCommand
        (
            execute: () => logNavigation.Navigate(new()
            {
                Persister = _persister
            }),
            canExecute: () => _persister is not null
        );
    }

    private bool _isRunning = false;
    private bool IsRunning
    {
        get => _isRunning;
        set
        {
            SetProperty(ref _isRunning, value);
            RunCommand.NotifyCanExecuteChanged();
        }
    }

    private double _donePercent;
    public double DonePercent
    {
        get => _donePercent;
        set => SetProperty(ref _donePercent, value);
    }

    private string _taskStatus;
    public string TaskStatus
    {
        get => _taskStatus;
        set => SetProperty(ref _taskStatus, value);
    }

    public string FileName => Path.GetFileName(Source.FilePath);

    private CutVideoTask _source;
    public CutVideoTask Source
    {
        get => _source;
        private set => SetProperty(ref _source, value);
    }


    private LogPersister _persister;
    private LogPersister Persister
    {
        get => _persister;
        set
        {
            _persister = value;
            OpenLogCommand.NotifyCanExecuteChanged();
        }
    }

    public string? TimeRangeString
        => FormatTimeSpan(TimeSpan.FromSeconds(Source?.TimeRange?.GetDuration() ?? 0));

    private ObservableObject _status;
    public ObservableObject Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public RelayCommand RunCommand { get; }
    public ICommand EditTaskCommand { get; }
    public ICommand OpenDirectoryCommand { get; }
    public RelayCommand OpenLogCommand { get; }

    private async Task OnRun()
    {
        IsRunning = true;

        Directory.CreateDirectory(_logsDirectory);

        Persister?.Dispose();
        Persister = new LogPersister(Path.Combine(_logsDirectory, $"{Source.Id}.{DateTime.Now:yyyyMMdd_HHmmss}.log"));

        await OnUpdate();

        Persister.Stop();

        IsRunning = false;
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

        DonePercent = _ytDlpStatus.Time.HasValue ?
            (_ytDlpStatus.Time.Value.TotalSeconds / _totalDuration.TotalSeconds) * 100.0 : 0;
    }


    private void OnOpenDirectory()
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(Source.FilePath));
        var ext = Source.Recode is not null ? $".{Source.Recode.Format}" : String.Empty;
        var fileName = Path.GetFileName(Source.FilePath) + ext;

        ExplorerHelper.OpenFolderAndSelectItem(dir, fileName);
    }

    [RelayCommand]
    private void DeleteTask()
        => WeakReferenceMessenger.Default.Send(new DeleteTaskMessage() { Task = Source });

    private async Task OnUpdate()
    {
        var proc = _ytDlp.RunTask(Source.Id);

        try
        {
            _canShowStatus = false;
            Status = new LoadingViewModel();
            proc.Start();

            proc.ErrorDataReceived += (sender, args) => UpdateStatus(args.Data);
            proc.BeginErrorReadLine();

            proc.OutputDataReceived += (sender, args) => Persister.Write(LogPersister.LogLevel.Standard, args.Data);
            proc.BeginOutputReadLine();

            await proc.WaitForExitAsync();
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
        }
        finally
        {
            proc.Kill();
        }

        var status = proc.ExitCode switch
        {
            0 or 100 => VideoTaskStatus.Completed,
            1 or 2 => VideoTaskStatus.Error, 
            101 => VideoTaskStatus.Cancelled,

            _ => VideoTaskStatus.Unknown
        };

        Source = Source with
        {
            Status = status,
            Completed = DateTime.Now
        };

        _ytDlp.UpdateTask(Source);
        Status = new SimpleStatusViewModel(status);
    }

    public static string FormatTimeSpan(TimeSpan timeSpan) => string.Join(" ", new[]
    {
            FormatPart((int)timeSpan.TotalHours, "h."),
            FormatPart(timeSpan.Minutes, "min."),
            FormatPart(timeSpan.Seconds, "sec.")
        }.Where(p => p is not null));


    public static string? FormatPart(int quantity, string name)
        => quantity > 0 ? $"{quantity} {name}" : null;
}