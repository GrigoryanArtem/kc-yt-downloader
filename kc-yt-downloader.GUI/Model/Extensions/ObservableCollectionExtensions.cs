using System.Collections.ObjectModel;

namespace kc_yt_downloader.GUI.Model.Extensions;

public static class ObservableCollectionExtensions
{
    public static void SyncItems<T>(
       this ObservableCollection<T> collection,
       IEnumerable<T> source,
       Func<T, T, bool> identitySelector)
    {
        var updated = source.ToArray();
        var toAdd = updated
            .Where(item => !collection.Any(existing => identitySelector(existing, item)))
            .ToArray();

        var toRemove = collection
            .Where(existing => !updated.Any(newItem => identitySelector(existing, newItem)))
            .ToArray();

        Array.ForEach(toRemove, item => collection.Remove(item));
        Array.ForEach(toAdd, item => collection.Insert(0, item));

        for (int i = 0; i < updated.Length; i++)
        {
            var desiredItem = updated[i];
            var currentIndex = collection.IndexOf(desiredItem);

            if (currentIndex != i && currentIndex >= 0)
                collection.Move(currentIndex, i);
        }
    }
}
