using kc_yt_downloader.GUI.ViewModel;

namespace kc_yt_downloader.GUI.Model;

public class TasksFactory(YtDlpProxy ytDlp)
{
    public CutViewLoadingViewModel CreateCutViewLoadingViewModel(string url, CutVideoBatch batch = default!) => new(async () =>
    {
        var video = await ytDlp.GetVideo(url, CancellationToken.None);

        return video is null
            ? LoadingResult<CutViewModelParameters>.CreateFailure()
            : LoadingResult<CutViewModelParameters>.CreateSuccess(new()
            {
                Batch = batch,
                Video = video
            });
    });
}
