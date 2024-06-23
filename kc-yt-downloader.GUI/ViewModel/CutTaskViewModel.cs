using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using NavigationMVVM;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class CutTaskViewModel : ObservableDisposableObject
    {
        private const string LOGS_DIRECTORY = "logs";

        private YtDlp _ytDlp;
        private readonly Video _video;
        private readonly TimeSpan _totalDuration;

        private readonly YtDlpStatusViewModel _ytDlpStatus;
        private readonly string _logsDirectory;
        private readonly string _errLogFileName;
        private readonly string _outLogFileName;

        private bool _canShowStatus = false;
        

        public CutTaskViewModel(CutVideoTask task, YtDlp ytDlp)
        {
            _ytDlp = ytDlp;
            Source = task;

            _ytDlpStatus = new();

            _video = _ytDlp.GetVideoById(task.VideoId);
            var format = _video.Info.Formats.SingleOrDefault(f => f.FormatId == task.VideoFormatId);

            _totalDuration = TimeSpan.FromSeconds(task.TimeRange?.GetDuration() ?? _video.Info.Duration);
            _logsDirectory = Path.Combine(YtConfig.Global.CacheDirectory, LOGS_DIRECTORY);

            _errLogFileName = Path.Combine(_logsDirectory, $"{task.Id}.err.log.txt");
            _outLogFileName = Path.Combine(_logsDirectory, $"{task.Id}.out.log.txt");

            DonePercent = task.Status == VideoTaskStatus.Completed ? 100 : 0;

            Status = new SimpleStatusViewModel(task.Status);
            RunCommand = new RelayCommand(async () => await OnRun(), () => !IsRunning);
            OpenDirectoryCommand = new RelayCommand(OnOpenDirectory);
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

        private ObservableDisposableObject _status;
        public ObservableDisposableObject Status 
        {
            get => _status;
            private set => SetProperty(ref _status, value); 
        }        

        public RelayCommand RunCommand { get; }
        public ICommand OpenDirectoryCommand { get; }
        
        private async Task OnRun()
        {
            IsRunning = true;

            Directory.CreateDirectory(_logsDirectory);

            var nl = Environment.NewLine;
            var startLogString = $"{nl}RUN {DateTime.Now:yyyy-MM-dd HH:mm:ss}{nl}{nl}";

            File.AppendAllText(_errLogFileName, startLogString);
            File.AppendAllText(_outLogFileName, startLogString);

            await OnUpdate();

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

            File.AppendAllText(_errLogFileName, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} | ERR | {str}" + Environment.NewLine);

            DonePercent = _ytDlpStatus.Time.HasValue ? 
                _ytDlpStatus.Time.Value.TotalSeconds / _totalDuration.TotalSeconds * 100.0 : 0;
        }


        private void OnOpenDirectory()
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(Source.FilePath));

            if (Directory.Exists(dir))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = dir,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
        }

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

                proc.OutputDataReceived += (sender, args) =>  File.AppendAllText(_outLogFileName, 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} | STD | {args.Data}" + Environment.NewLine);
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
    }
}
