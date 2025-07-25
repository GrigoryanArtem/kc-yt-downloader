namespace kc_yt_downloader.DataMigration;

internal class Program
{
    static void Main(string[] args)
    {
        string inputJsonPath = "videos.json";
        string outputIndexPath = "index.json";

        VideoDataMigrator.Migrate(inputJsonPath, outputIndexPath);
    }
}
