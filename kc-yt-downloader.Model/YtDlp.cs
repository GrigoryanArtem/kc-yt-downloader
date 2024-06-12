using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace kc_yt_downloader.Model
{
    public class YtDlp
    {
        private const string CACHE_FILEPATH = "cache.json";

        private Dictionary<string, string> _json = [];

        public VideoInfo[] GetCachedData()
        {
            Open();
            return _json.Select(kv => JsonConvert.DeserializeObject<VideoInfo>(kv.Value)).ToArray();
        }            

        public void Open()
        {
            if (!File.Exists(CACHE_FILEPATH))
                return;

            var json = File.ReadAllText(CACHE_FILEPATH);
            _json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_json, Formatting.Indented);
            File.WriteAllText(CACHE_FILEPATH, json);
        }

        public string? GetPureJson(string url)
        {
            if (_json.TryGetValue(url, out var urlJson))
                return urlJson;

            var json = DownloadInfo(url);
            if (json is not null)
                _json.Add(url, json);

            // TODO remove
            Save();
            return json;
        }

        public VideoInfo? GetInfo(string url)
        {
            var json = GetPureJson(url);
            return json is not null ? JsonConvert.DeserializeObject<VideoInfo>(json) : null;
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
