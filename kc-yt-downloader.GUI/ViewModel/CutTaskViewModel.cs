using CommunityToolkit.Mvvm.Input;
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

            var speed = match.Groups["speed"].Value;
            var bitRate = match.Groups["bitrate"].Value;
            var size = match.Groups["size"].Value;
            var time = match.Groups["time"].Value;

            var ts = TimeSpan.Parse(time);

            _ytDlpStatus.Frame = match.Groups["frame"].Value;
            _ytDlpStatus.FPS = match.Groups["fps"].Value;
            _ytDlpStatus.Size = size.Length > 2 && size[^2..] == "kB" ? size[..^2] : size;
            _ytDlpStatus.Time = ts.ToString(@"hh\:mm\:ss");
            _ytDlpStatus.BitRate = bitRate.Length > 7 && bitRate[^7..] == "kbits/s" ? bitRate[..^7] : bitRate;
            _ytDlpStatus.Speed = speed.Length > 1 && speed[^1..] == "x" ? speed[..^1] : speed;

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

            Source = Source with 
            { 
                Status = VideoTaskStatus.Completed,
                Completed = DateTime.Now
            };

            _ytDlp.UpdateTask(Source);
            Status = new SimpleStatusViewModel(Source.Status);
        }
    }
}
