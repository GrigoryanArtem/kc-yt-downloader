using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.GUI.Model.Messages;
using kc_yt_downloader.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel;

public class CutViewModel : ObservableObject
{
    private const string HTTPS_PROTOCOL = "https";
    private const string NONE = "none";

    private readonly VideoInfo _info;
    private readonly CutViewModelParameters _parameters;

    public CutViewModel(CutViewModelParameters parameters)
    {
        var ytDlp = App.Current.Services.GetRequiredService<YtDlp>();

        _parameters = parameters;
        _info = _parameters.Video.Info;

        var formats = _info.FormatId.Split("+", StringSplitOptions.RemoveEmptyEntries);
        var (vf, af) = (formats[0], formats[1]);

        var httpsFormats = _info.Formats
            .Where(f => f.Protocol == HTTPS_PROTOCOL)
            .OrderByDescending(f => f.Quality ?? 0)
            .ToArray();

        VideoFormatsSelector = new(httpsFormats.Where(f => f.VCodec != NONE && f.ACodec == NONE)
            .Select(f => new VideoFormatViewModel(f))
            .ToArray(), vf);

        AudioFormatsSelector = new(httpsFormats.Where(f => f.ACodec != NONE && f.VCodec == NONE)
            .Select(f => new VideoFormatViewModel(f))
            .ToArray(), af);

        FileNameControl = String.IsNullOrEmpty(parameters.Batch?.FilePath) 
            ? FileNameControlViewModel.CreateFromName(_info.Title)
            : FileNameControlViewModel.CreateFromPath(parameters.Batch.FilePath);
        
        TimeRange = new(parameters.Batch?.Segments, _info.DurationString);

        if (parameters.Batch is not null)
            InitBatch(parameters.Batch);

        BackCommand = NavigationHistory.NavigateBackCommand;
        AddToQueueCommand = new RelayCommand(OnAddToQueueCommand);
    }

    public string Title => _info.Title;

    public VideFormatSelectorViewModel VideoFormatsSelector { get; }
    public VideFormatSelectorViewModel AudioFormatsSelector { get; }
    public RecodeViewModel Recode { get; } = new();

    public FileNameControlViewModel FileNameControl { get; }
    public TimeRangeViewModel TimeRange { get; }

    public ICommand BackCommand { get; }
    public ICommand AddToQueueCommand { get; }

    private void OnAddToQueueCommand()
    {
        var ranges = TimeRange.GetTimeRanges();
        var tasks = ranges.Select((tr, i) => new CutVideoTask()
        {
            Name = _info.Title,
            Created = DateTime.Now,

            VideoId = _info.Id,
            URL = _info.WebPageUrl,
            FilePath = FileNameControl.GetFullPath() + (ranges.Length > 1 ? i.ToString() : String.Empty),

            TimeRange = tr,
            Recode = Recode.GetRecode(),

            VideoFormatId = VideoFormatsSelector.SelectedFormat?.Id,
            AudioFormatId = AudioFormatsSelector.SelectedFormat?.Id,

            Status = VideoTaskStatus.Prepared
        }).ToArray();

        YtConfig.Global.Save();
        WeakReferenceMessenger.Default.Send(new AddTaskMessage() { Tasks = tasks });
        NavigationHistory.Current.NavigateBack();
    }

    private void InitBatch(CutVideoBatch task)
    {
        VideoFormatsSelector.SelectedFormat = VideoFormatsSelector.Formats?.FirstOrDefault(f => f.Id == task.VideoFormatId);
        AudioFormatsSelector.SelectedFormat = AudioFormatsSelector.Formats?.FirstOrDefault(f => f.Id == task.AudioFormatId);

        Recode.NeedRecode = task.Recode is not null;
        if (Recode.NeedRecode)
        {
            Recode.SelectedFormat = task.Recode.Format;
        }
    }
}
