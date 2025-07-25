using kc_yt_downloader.Model.Enums;
using kc_yt_downloader.Model.Exceptions;
using kc_yt_downloader.Model.Processing;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace kc_yt_downloader.Model;

public class YtDlp(string cacheDirectory)
{
    private const string YT_DLP = "yt-dlp";
    private const string VIDEOS_INDEX_FILEPATH = "index.json";
    private const string TASKS_CACHE_FILEPATH = "tasks.json";

    private List<VideoPreview> _videoCache = [];
    private List<CutVideoTask> _tasksCache = [];

    private readonly string _videoIndexPath = String.IsNullOrEmpty(cacheDirectory)
        ? VIDEOS_INDEX_FILEPATH
        : Path.Combine(cacheDirectory, VIDEOS_INDEX_FILEPATH);

    private readonly string _tasksCachePath = String.IsNullOrEmpty(cacheDirectory)
        ? TASKS_CACHE_FILEPATH
        : Path.Combine(cacheDirectory, TASKS_CACHE_FILEPATH);

    public void Open()
    {
        var dir = Path.GetDirectoryName(_videoIndexPath);

        if (!String.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(_videoIndexPath))
            _videoCache = JsonConvert.DeserializeObject<List<VideoPreview>>(File.ReadAllText(_videoIndexPath));

        if (File.Exists(_tasksCachePath))
            _tasksCache = JsonConvert.DeserializeObject<List<CutVideoTask>>(File.ReadAllText(_tasksCachePath));
    }

    public void Save()
    {
        var json = JsonConvert.SerializeObject(_videoCache, Formatting.Indented);
        File.WriteAllText(_videoIndexPath, json);

        json = JsonConvert.SerializeObject(_tasksCache, Formatting.Indented);
        File.WriteAllText(_tasksCachePath, json);
    }

    public CutVideoTask[] GetCachedTasks()
        => [.. _tasksCache];
    public VideoPreview[] GetCachedData()
        => [.. _videoCache];

    public void DeleteVideo(VideoPreview video)
    {
        _tasksCache = [.. _tasksCache.Where(t => t.VideoId != video.Id)];
        _videoCache = [.. _videoCache.Where(v => v.Id != video.Id)];

        Save();
    }

    public VideoPreview? GetVideoById(string id)
        => _videoCache.SingleOrDefault(v => v.Id == id);

    public VideoPreview? GetPreviewVideoByUrl(string url)
        => _videoCache.SingleOrDefault(v => v.AvailableURLs.Contains(url));

    #region Commands

    public async Task<string> PredictFileExtension(string videoId, string formatString, CancellationToken cancellationToken)
    {
        var command = YtDlpCommands.PredictExtension(videoId, formatString);

        await command.Run(cancellationToken);

        if (command.ExitCode != ProcessExitCode.Success)
            throw new CommandException($"Failed to predict file extension for video {videoId}.", command);

        return command.Output.Trim();
    }

    public async Task<string> YtDlpVersion(CancellationToken cancellationToken)
    {
        var version = YtDlpCommands.Version();

        await version.Run(cancellationToken);

        if (version.ExitCode != ProcessExitCode.Success)
            throw new CommandException($"Failed to get yt-dlp version", version);

        return version.Output.Trim();
    }

    public async Task<Video?> GetVideoByUrl(string url, CancellationToken cancellationToken)
    {
        var jsonDump = YtDlpCommands.DumpJson(url);

        await jsonDump.Run(cancellationToken);

        if (jsonDump.ExitCode != ProcessExitCode.Success)
            throw new CommandException($"Failed to get video info for {url}.", jsonDump);

        var json = jsonDump.Output;
        if (String.IsNullOrEmpty(json))
            return null;

        var info = Video.ParseJson(json);

        var similar = _videoCache.Select((video, idx) => (video, idx))
            .Where(v => v.video.Id == info.Id)
            .ToArray();

        Video video;
        if (similar.Length != 0)
        {
            if (similar.Length > 1)
                throw new YtCacheException($"Cache has more than one copy of the video {info.Id}");

            var (video_short, idx) = similar.FirstOrDefault();
            string[] availableURLs = [.. video_short.AvailableURLs, url, info.WebPageUrl];
            video = new Video(json, info, [.. availableURLs.Distinct()]);

            _videoCache[idx] = video.ToIndexEntry();
        }
        else
        {
            video = new Video(json, info, url);
            _videoCache.Add(video.ToIndexEntry());
        }

        Save();

        return video;
    }

    #endregion



    public void DeleteTask(CutVideoTask task)
    {
        var idx = _tasksCache
            .Select((tsk, idx) => (tsk, idx))
            .First(d => d.tsk.Id == task.Id).idx;

        _tasksCache = _tasksCache
            .Where(t => t.Id != task.Id)
            .ToList();

        Save();
    }

    public void UpdateTask(CutVideoTask task)
    {
        var idx = _tasksCache
            .Select((tsk, idx) => (tsk, idx))
            .First(d => d.tsk.Id == task.Id).idx;

        _tasksCache[idx] = task;
        Save();
    }

    public void AddTask(CutVideoTask task)
    {
        var id = (int)((DateTime.Now - new DateTime(year: 2024, month: 1, day: 1)).Ticks / 100);
        _tasksCache.Add(task with { Id = id });

        Save();
    }

    public Process RunTask(int id)
    {
        var task = _tasksCache.SingleOrDefault(t => t.Id == id);

        var startInfo = new ProcessStartInfo
        {
            FileName = YT_DLP,
            Arguments = task.ToArgs(),
            UseShellExecute = false,

            RedirectStandardError = true,
            RedirectStandardOutput = true,

            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,

            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        return new Process { StartInfo = startInfo };
    }

    public async Task<bool> UpdateYtDlpAsync(YtDlpUpdateChannel updateChannel = YtDlpUpdateChannel.Stable, IProgress<string>? progress = null)
    {
        var args = MapChannelToArgs(updateChannel);

        var startInfo = new ProcessStartInfo
        {
            FileName = YT_DLP,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                progress?.Report(e.Data);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                progress?.Report(e.Data);
        };

        progress?.Report($"Updating yt-dlp ({updateChannel})...");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var success = process.ExitCode == 0;
        if (success)
        {
            progress?.Report("yt-dlp updated!");
        }
        else
        {
            progress?.Report($"yt-dlp not updated, exit code: {process.ExitCode}");
        }

        return success;
    }

    public static string MapChannelToArgs(YtDlpUpdateChannel channel) => channel switch
    {
        YtDlpUpdateChannel.Stable => "-U",
        YtDlpUpdateChannel.Nightly => "--update-to nightly",
        YtDlpUpdateChannel.Master => "--update-to master",

        _ => throw new ArgumentOutOfRangeException(nameof(channel), $"Unknown update channel: {channel}"),
    };
}
