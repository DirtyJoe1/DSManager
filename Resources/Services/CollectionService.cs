using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Resources.Services
{
    public static class CollectionService
    {
        public static void ReplaceItemsInCollection<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static async Task ReplaceItemsInCollectionAsync<T>(ObservableCollection<T> collection, IAsyncEnumerable<T> items)
        {
            collection.Clear();
            await foreach (var item in items)
            {
                collection.Add(item);
            }
        }
        public static void RemoveItemFromCollection(ObservableCollection<string> collection, string item)
        {
            if (collection.Contains(item))
            {
                collection.Remove(item);
            }
        }
    }
}
