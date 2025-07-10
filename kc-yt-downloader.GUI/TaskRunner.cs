using CommunityToolkit.Mvvm.ComponentModel;
using kc_yt_downloader.GUI.Model;
using kc_yt_downloader.Model;

namespace kc_yt_downloader.GUI;

public partial class AutoTaskRunner(YtDlpProxy ytDlpProxy) : ObservableObject, IDisposable
{
    public enum RunType
    {
        Single,
        Batch
    }
    
    #region Members

    private readonly HashSet<Task> _ranTasks = [];

    private CancellationTokenSource? _activeCancelationTokenSource;
    private int _batchSize;

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _active;

    #endregion

    public void Activate(RunType runType)
    {
        if (Active)
            return;

        Active = true;

        _activeCancelationTokenSource = new();
        _batchSize = runType == RunType.Batch ? YtConfig.Global.BatchSize : 1;

        Task.Run(() => RunBackground(_activeCancelationTokenSource.Token));


        GlobalSnackbarMessageQueue.WriteInfo($"Started {runType} mode auto processing");
    }

    public void Stop()
    {
        if (!Active)
            return;

        _activeCancelationTokenSource.Cancel();

        Active = false;

        GlobalSnackbarMessageQueue.WriteInfo("Stopped auto processing");
    }

    private async Task RunBackground(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var runTask in _ranTasks.Where(t => t.IsCompleted))
                _ranTasks.Remove(runTask);

            if (_ranTasks.Count == _batchSize)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                continue;
            }

            var group = ytDlpProxy.Tasks
                .FirstOrDefault(t => t.Status == VideoTaskStatus.Prepared);

            if(group is null || group.Items.Count == 0)
            {
                Stop();
                return;
            }

            var tasks = group?.Items
                .OrderBy(t => t.Source.Created)
                .Take(_batchSize - _ranTasks.Count)
                .ToArray() ?? [];

            foreach (var task in tasks)
            {
                var runTask = task.RunTask();
                _ranTasks.Add(runTask);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);            
        }
    }

    public void Dispose()
        => Stop();
    
}
