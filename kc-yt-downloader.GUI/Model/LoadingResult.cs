namespace kc_yt_downloader.GUI.Model;

public class LoadingResult<T>(bool success, T result)
{
    public bool Success { get; } = success;
    public T Result { get; } = result;

    public static LoadingResult<T> CreateSuccess(T result) => new(true, result);
    public static LoadingResult<T> CreateFailure() => new(false, default!);
}
