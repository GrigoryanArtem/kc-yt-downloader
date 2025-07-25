namespace kc_yt_downloader.GUI.Model.Extensions;

public static class ExceptionExtensions
{
    public static IEnumerable<Exception> GetInnerExceptions(this Exception exception)
    {
        var current = exception.InnerException;

        while (current != null)
        {
            yield return current;
            current = current.InnerException;
        }
    }
}
