using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace kc_yt_downloader.Model
{
    public class YtDlp
    {
        private const string CACHE_FILEPATH = "cache.json";

        private List<Video> _cache = [];
        private readonly string _cachePath;

        public YtDlp(string cacheDirectory)
        {
            _cachePath = String.IsNullOrEmpty(cacheDirectory) ? CACHE_FILEPATH 
                : Path.Combine(cacheDirectory, CACHE_FILEPATH);
        }

        public void Open()
        {
            var dir = Path.GetDirectoryName(_cachePath);

            if(!String.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);
            
            if (!File.Exists(_cachePath))
                return;

            var json = File.ReadAllText(_cachePath);
            _cache = JsonConvert.DeserializeObject<List<Video>>(json);
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
            File.WriteAllText(_cachePath, json);
        }

        public Video[] GetCachedData()
            => [.. _cache];

        public Video? GetVideo(string url)
        {
            var probe = _cache.SingleOrDefault(v => v.AvailableURLs.Contains(url));
            if (probe is not null)
                return probe;

            var json = DownloadInfo(url);

            if (String.IsNullOrEmpty(json))
                return null;

            var info = Video.ParseJson(json);

            var similar = _cache.Select((video, idx) => (video, idx))
                .Where(v => v.video.Id == info.Id)
                .ToArray();

            Video video;
            if (similar.Length != 0)
            {
                if (similar.Length > 1)
                    throw new YtCacheException($"Cache has more than one copy of the video {info.Id}");

                (video, int idx) = similar.FirstOrDefault();
                video = new Video(json, info, [.. video.AvailableURLs, url]);

                _cache[idx] = video;
            }
            else
            {
                video = new Video(json, info, url);
                _cache.Add(video);
            }    

            Save();

            return video;
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
