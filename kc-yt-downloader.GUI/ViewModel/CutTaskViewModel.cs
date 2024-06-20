using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.Model;
using NavigationMVVM;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel
{
    public class CutTaskViewModel : ObservableDisposableObject
    {
        private YtDlp _ytDlp;
        private readonly Video _video;
        private readonly int _totalFrames;

        private readonly YtDlpStatusViewModel _ytDlpStatus;

        public CutTaskViewModel(CutVideoTask task, YtDlp ytDlp)
        {
            _ytDlp = ytDlp;
            Source = task;

            _ytDlpStatus = new();

            _video = _ytDlp.GetVideoById(task.VideoId);
            var format = _video.Info.Formats.SingleOrDefault(f => f.FormatId == task.VideoFormatId);

            var duration = task.TimeRange?.GetDuration() ?? _video.Info.Duration;

            _totalFrames = (int)(duration * format.Fps);


            Status = new SimpleStatusViewModel(task.Status);
            RunCommand = new RelayCommand(async () => await OnRun());
            OpenDirectoryCommand = new RelayCommand(OnOpenDirectory);
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
        
        public CutVideoTask Source { get; }

        private ObservableDisposableObject _status;
        public ObservableDisposableObject Status 
        {
            get => _status;
            private set => SetProperty(ref _status, value); 
        }        

        public ICommand RunCommand { get; }
        public ICommand OpenDirectoryCommand { get; }

        private async Task OnRun()
        {            
            await OnUpdate();            
        }

        private Regex STATUS_REGEX =new(@"frame=\s*(?<frame>.*?)\s*fps=\s*(?<fps>.*?)\s*q=\s*(?<q>.*?)\s*size=\s*(?<size>.*?)\s*time=\s*(?<time>.*?)\s*bitrate=\s*(?<bitrate>.*?)\s*speed=\s*(?<speed>.*?)\s", RegexOptions.Compiled);
        private void UpdateStatus(string str)
        {
            if (str is null)
                return;

            var match = STATUS_REGEX.Match(str);

            if (!match.Success)
                return;

            _ytDlpStatus.Frame = match.Groups["frame"].Value;
            _ytDlpStatus.FPS = match.Groups["fps"].Value;
            _ytDlpStatus.Size = match.Groups["size"].Value;
            _ytDlpStatus.Time = match.Groups["time"].Value;
            _ytDlpStatus.BitRate = match.Groups["bitrate"].Value;
            _ytDlpStatus.Speed = match.Groups["speed"].Value;

            DonePercent = (double) Convert.ToInt32(_ytDlpStatus.Frame) / _totalFrames * 100.0;
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
                Status = _ytDlpStatus;
                proc.Start();

                proc.ErrorDataReceived += (sender, args) => UpdateStatus(args.Data);
                proc.BeginErrorReadLine();

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

            Status = new SimpleStatusViewModel(VideoTaskStatus.Completed);
        }
    }
}
