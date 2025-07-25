﻿using kc_yt_downloader.Model;
using Newtonsoft.Json;
using System.IO;

namespace kc_yt_downloader.GUI.Model;

public class YtConfig
{
    private const string CONFIG_PATH = "config.json";

    #region Singleton

    private static readonly Lazy<YtConfig> _lazy = new(Load);

    private YtConfig() { }
    public static YtConfig Global => _lazy.Value;

    #endregion

    #region Properties

    public string ExtensionListenerUrl { get; set; } = "http://localhost:5000/api/cut/";
    public string DataDirectory { get; set; } = "data";
    public Dictionary<VideoTaskStatus, int> ExpirationTimes { get; set; } = [];
    public int BatchSize { get; set; } = 2;
    public SelectedSettings SelectedSettings { get; set; } = new();

    #endregion

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
        return JsonConvert.DeserializeObject<YtConfig>(json)!;
    }
}
