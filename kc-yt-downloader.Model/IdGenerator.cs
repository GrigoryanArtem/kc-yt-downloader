namespace kc_yt_downloader.Model;

public static class IdGenerator
{
    private const string ID_FILE = ".id";
    private static readonly object _lock = new();

    public static int Next()
    {
        lock (_lock)
        {
            var currentId = File.Exists(ID_FILE) ? Convert.ToInt32(File.ReadAllText(ID_FILE)) : 1;
            File.WriteAllText(ID_FILE, (++currentId).ToString());
            return currentId;
        }
    }
}
