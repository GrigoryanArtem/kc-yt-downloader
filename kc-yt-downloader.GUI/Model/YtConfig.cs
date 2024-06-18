using Newtonsoft.Json;
using System.IO;

namespace kc_yt_downloader.GUI.Model
{
    public class YtConfig
    {
        private const string CONFIG_PATH = "config.json";
        private static readonly Lazy<YtConfig> _lazy = new(Load);

        private YtConfig() { }    
        public static YtConfig Global => _lazy.Value;

        public string CacheDirectory { get; init; } = "data";

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(CONFIG_PATH, json);
        }

        private static YtConfig Load()
        {
            if (!File.Exists(CONFIG_PATH))
            {
                var defaultConfig = new YtConfig();
                defaultConfig.Save();

                return defaultConfig;
            }

            var json = File.ReadAllText(CONFIG_PATH);
            return JsonConvert.DeserializeObject<YtConfig>(json);
        }
    }
}
