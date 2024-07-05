using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace kc_yt_downloader.Model
{
    public class YtDlp
    {
        private const string VIDEOS_CACHE_FILEPATH = "videos.json";
        private const string TASKS_CACHE_FILEPATH = "tasks.json";

        private List<Video> _videoCache = [];
        private List<CutVideoTask> _tasksCache = [];

        private readonly string _videoCachePath;
        private readonly string _tasksCachePath;

        public YtDlp(string cacheDirectory)
        {
            _videoCachePath = String.IsNullOrEmpty(cacheDirectory) ? VIDEOS_CACHE_FILEPATH
                : Path.Combine(cacheDirectory, VIDEOS_CACHE_FILEPATH);

            _tasksCachePath = String.IsNullOrEmpty(cacheDirectory) ? TASKS_CACHE_FILEPATH
                : Path.Combine(cacheDirectory, TASKS_CACHE_FILEPATH);
        }

        public void Open()
        {
            var dir = Path.GetDirectoryName(_videoCachePath);

            if(!String.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
            
            if (File.Exists(_videoCachePath))
                _videoCache = JsonConvert.DeserializeObject<List<Video>>(File.ReadAllText(_videoCachePath));

            if (File.Exists(_tasksCachePath))
                _tasksCache = JsonConvert.DeserializeObject<List<CutVideoTask>>(File.ReadAllText(_tasksCachePath));
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_videoCache, Formatting.Indented);
            File.WriteAllText(_videoCachePath, json);

            json = JsonConvert.SerializeObject(_tasksCache, Formatting.Indented);
            File.WriteAllText(_tasksCachePath, json);
        }

        public CutVideoTask[] GetCachedTasks()
            => [.. _tasksCache];
        public Video[] GetCachedData()
            => [.. _videoCache];

        public void DeleteVideo(Video video)
        {
            _tasksCache = _tasksCache
                .Where(t => t.VideoId != video.Id)
                .ToList();

            _videoCache = _videoCache
                .Where(v => v.Id != video.Id)
                .ToList();

            Save();
        }

        public Video? GetVideoById(string id)
            => _videoCache.SingleOrDefault(v => v.Id == id);

        public Video? GetVideoByUrl(string url)
        {
            var probe = _videoCache.SingleOrDefault(v => v.AvailableURLs.Contains(url));
            if (probe is not null)
                return probe;

            var json = DownloadInfo(url);

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

                (video, int idx) = similar.FirstOrDefault();
                video = new Video(json, info, [.. video.AvailableURLs, url]);

                _videoCache[idx] = video;
            }
            else
            {
                video = new Video(json, info, url);
                _videoCache.Add(video);
            }    

            Save();

            return video;
        }

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
            var id = _tasksCache.Count + 1;
            _tasksCache.Add(task with { Id = id });

            Save();
        }
        
        public Process RunTask(int id)
        {
            var task = _tasksCache.SingleOrDefault(t => t.Id == id);

            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
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

        private static string? DownloadInfo(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--dump-json {url}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            var proc = new Process { StartInfo = startInfo };

            try
            {
                var sb = new StringBuilder();
                proc.Start();

                while (!proc.StandardOutput.EndOfStream && proc.Responding && !proc.HasExited)
                {
                    var data = proc.StandardOutput.ReadToEnd();
                    sb.AppendLine(data);
                }

                return sb.ToString();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                proc.Kill();
            }

            return null;
        }
    }
}
