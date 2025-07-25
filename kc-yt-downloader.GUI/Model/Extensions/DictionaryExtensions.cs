namespace kc_yt_downloader.GUI.Model.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TValue> func)
         where TKey : notnull
    {
        if (!source.ContainsKey(key))
            source.Add(key, func());

        return source[key];
    }
}
