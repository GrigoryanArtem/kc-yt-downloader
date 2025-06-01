using kc_yt_downloader.Model;
using System.Text.Json;

namespace kc_yt_downloader.DataMigration;
public class VideoDataMigrator
{
    public static void Migrate(string inputJsonPath, string outputIndexPath)
    {
        if (!File.Exists(inputJsonPath))
        {
            Console.WriteLine($"File {inputJsonPath} not found.");
            return;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        Console.Error.WriteLine($"Reading {inputJsonPath}");
        var json = File.ReadAllText(inputJsonPath);
        var videos = JsonSerializer.Deserialize<Video[]>(json)!;

        var indexEntries = new List<VideoPreview>();
        foreach (var video in videos)
        {
            var info = video.Info;

            var infoLite = new VideoPreviewInfo
            {
                Title = info.Title,
                Thumbnails = info.Thumbnails,
                Duration = info.Duration,
                UploadDate = info.UploadDate,
                OriginalUrl = info.OriginalUrl
            };

            indexEntries.Add(new VideoPreview
            {
                Id = video.Id,
                AvailableURLs = video.AvailableURLs,
                ParseDate = video.ParseDate,
                Info = infoLite
            });
        }

        var indexJson = JsonSerializer.Serialize(indexEntries, options);
        File.WriteAllText(outputIndexPath, indexJson);

        Console.Error.WriteLine($"Migration completed: {videos.Length} videos processed.");
    }
}