﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using NavigationMVVM;
using NavigationMVVM.Services;
using System.IO;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public partial class CutTaskViewModel : ObservableDisposableObject
    {
        private const string LOGS_DIRECTORY = "logs";

        private YtDlp _ytDlp;
        private readonly Video _video;
        private readonly TimeSpan _totalDuration;

        private readonly YtDlpStatusViewModel _ytDlpStatus;
        private readonly string _logsDirectory;      

        private bool _canShowStatus = false;
        private LogPersister _persister;

        public CutTaskViewModel(CutVideoTask task, YtDlp ytDlp, ParameterNavigationService<CutViewModelParameters, CutViewModel> cutNavidation,
            NavigationService<ObservableDisposableObject> backNavigation, NavigationService<ObservableDisposableObject> dashboardNavigation)
        {
            _ytDlp = ytDlp;
            Source = task;

            _ytDlpStatus = new();

            _video = _ytDlp.GetVideoById(task.VideoId);
            var format = _video.Info.Formats.SingleOrDefault(f => f.FormatId == task.VideoFormatId);

            _totalDuration = TimeSpan.FromSeconds(task.TimeRange?.GetDuration() ?? _video.Info.Duration);
            _logsDirectory = Path.Combine(YtConfig.Global.CacheDirectory, LOGS_DIRECTORY);

            DonePercent = task.Status == VideoTaskStatus.Completed ? 100 : 0;

            Status = new SimpleStatusViewModel(task.Status);

            RunCommand = new RelayCommand(async () => await OnRun(), () => !IsRunning);
            OpenDirectoryCommand = new RelayCommand(OnOpenDirectory);

            EditTaskCommand = new RelayCommand(() => cutNavidation.Navigate(new()
            {
                BackNavigation = backNavigation,
                DashboardNavigation = dashboardNavigation,

                Source = task,
                VideoInfo = _video.Info
            }));
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

        private CutVideoTask _source;
        public CutVideoTask Source 
        { 
            get => _source;
            private set => SetProperty(ref _source, value);
        }

        public string? TimeRangeString 
            => FormatTimeSpan(TimeSpan.FromSeconds(Source?.TimeRange?.GetDuration() ?? 0));

        private ObservableDisposableObject _status;
        public ObservableDisposableObject Status 
        {
            get => _status;
            private set => SetProperty(ref _status, value); 
        }        

        public RelayCommand RunCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand OpenDirectoryCommand { get; }
        
        private async Task OnRun()
        {
            IsRunning = true;

            Directory.CreateDirectory(_logsDirectory);

            _persister?.Dispose();
            _persister = new LogPersister(Path.Combine(_logsDirectory, $"{Source.Id}.{DateTime.Now:yyyyMMdd_HHmmss}.log"));

            await OnUpdate();

            _persister.Stop();            

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

            _persister.Write(LogPersister.LogLevel.Standard, str);

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

                proc.OutputDataReceived += (sender, args) => _persister.Write(LogPersister.LogLevel.Standard, args.Data);
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

            var status = proc.ExitCode == 0 ? VideoTaskStatus.Completed : VideoTaskStatus.Error;
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
}
