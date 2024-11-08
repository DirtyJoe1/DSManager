using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager.Resources.Services
{
    //По одиночке объяснять, что делает каждый метод нет смысла
    //Суть такая, ObservableCollection, это такой класс, который при обновлении(изменении, добавлении, удалении)
    //элементов вызывает обновление интерфейса(UI), но тут проблема, если такой коллекции изменить содержимое целиком
    //на новое, то обновление UI не будет, потому что коллекция будет ссылаться на старое содержимое.
    //Поэтому вот такой статический класс, который может обновлять содержимое и при этом вызывать обновление UI.
    //Два вида методов синхронные и асинхронные.
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
    }
}
