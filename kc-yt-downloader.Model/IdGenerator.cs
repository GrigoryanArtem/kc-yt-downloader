namespace kc_yt_downloader.Model;

public static class IdGenerator
{
    #region Constants

    private const string ID_FILE = ".id";

    private const int START_ID = 1;
    private const int BLOCK_SIZE = 10;

    #endregion

    private static readonly object _lock = new();

    private static int _currentId;
    private static int _maxIdInBlock;

    public static int Next()
    {
        if (_currentId < _maxIdInBlock)
        {
            return Interlocked.Increment(ref _currentId) - 1;
        }

        lock (_lock)
        {
            if (_currentId >= _maxIdInBlock)
                AllocateNextBlock();

            return _currentId++;
        }
    }

    #region Private methods

    private static void AllocateNextBlock()
    {
        _currentId = ReadId();
        _maxIdInBlock = _currentId + BLOCK_SIZE;

        WriteId(_maxIdInBlock);
    }

    private static int ReadId()
    {
        int? id = null;

        if (File.Exists(ID_FILE))
        {
            using var fileStream = new FileStream(ID_FILE, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new BinaryReader(fileStream);

            id = reader.ReadInt32();

            if (id <= 0)
                throw new InvalidDataException("ID must be positive");
        }

        return id ?? START_ID;
    }

    private static void WriteId(int id)
    {
        using var fileStream = new FileStream(ID_FILE, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(fileStream);

        writer.Write(id);
    }

    #endregion    
}
