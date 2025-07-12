using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;
using kc_yt_downloader.Model.Utility;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace kc_yt_downloader.GUI.ViewModel;

public partial class CutViewModel : ObservableObject
{
    #region Constants

    private const string HTTPS_PROTOCOL = "https";
    private const string NONE = "none";

    #endregion

    private readonly VideoInfo _info;
    private readonly CutViewModelParameters _parameters;

    #region Properties

    [ObservableProperty]
    private bool _isAddingToQueue = false;

    #endregion


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
        AddToQueueCommand = new AsyncRelayCommand(OnAddToQueueCommand);
    }

    public string Title => _info.Title;

    public VideFormatSelectorViewModel VideoFormatsSelector { get; }
    public VideFormatSelectorViewModel AudioFormatsSelector { get; }
    public RecodeViewModel Recode { get; } = new();

    public FileNameControlViewModel FileNameControl { get; }
    public TimeRangeViewModel TimeRange { get; }

    public ICommand BackCommand { get; }
    public ICommand AddToQueueCommand { get; }

    private async Task OnAddToQueueCommand()
    {
        IsAddingToQueue = true;

        var services = App.Current.Services;
        var segments = TimeRange.Segments.ToArray();
        var multipleSegments = segments.Length > 1;

        var ytDlpProxy = services.GetRequiredService<YtDlpProxy>();
        var ytDlp = services.GetRequiredService<YtDlp>();

        var recode = Recode.GetRecode();
        var extension = recode?.Format;

        if (extension is null)
        {
            var formatString = VideoFormatCombiner.Combine(
                VideoFormatsSelector.SelectedFormat?.Id,
                AudioFormatsSelector.SelectedFormat?.Id);

            extension = await ytDlp.PredictFileExtension(_info.WebPageUrl, formatString, CancellationToken.None);
        }

        var tasks = segments.Select((s, i) => new CutVideoTask()
        {
            Name = _info.Title,
            Created = DateTime.Now,

            VideoId = _info.Id,
            URL = _info.WebPageUrl,

            FilePath = FileNameControl.GetFullPath() + (!String.IsNullOrEmpty(s.Suffix)
                ? $"_{s.Suffix}"
                : (multipleSegments
                    ? $"_{i + 1}"
                    : String.Empty)),

            PredictedExtension = extension,

            TimeRange = new() { From = s.From, To = s.To },
            Recode = recode,

            VideoFormatId = VideoFormatsSelector.SelectedFormat?.Id,
            AudioFormatId = AudioFormatsSelector.SelectedFormat?.Id,

            Status = VideoTaskStatus.Prepared
        }).ToArray();


        YtConfig.Global.Save();
        ytDlpProxy.AddTasks(tasks);
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
