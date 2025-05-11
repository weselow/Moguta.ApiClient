using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moguta.ApiClient.Infrastructure
{
    internal static class DataHelper
    {
        /// <summary>
        /// Метод для разбиения списка на порции
        /// </summary>
        public static IEnumerable<List<T>> SplitIntoBatches<T>(List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
            }
        }

        /// <summary>
        /// Возращает словарь со списками. В каждом списке - уникальные значения, дубликаты вынесены в следующий список.
        /// </summary>
        /// <remarks>
        /// <para>Идея в том, чтобы распределить элементы из исходного набора по «слоям»:</para>
        /// <list type="bullet">
        /// <item>в первом слое (списке) оказывается первая (уникальная) встреча каждого ключа,</item>
        /// <item>во втором – вторая встреча для тех, у кого она есть,</item>
        /// <item>в третьем – третья и т.д.</item>
        /// </list> 
        /// </remarks>
        public static Dictionary<int, List<TSource>> FilterAndGroupToUniqueLists<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) where TKey : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            // Словарь для хранения количества вхождений каждого ключа
            var occurrence = new Dictionary<TKey, int>();

            // Словарь, где ключ – номер слоя (начиная с 1), а значение – список элементов этого слоя
            var result = new Dictionary<int, List<TSource>>();

            foreach (var item in source)
            {
                var key = keySelector(item);

                // Определяем номер вхождения для данного ключа
                occurrence.TryGetValue(key, out int currentOccurrence);
                var listNumber = currentOccurrence + 1;

                // Если для данного номера списка еще нет – создаем его
                if (!result.ContainsKey(listNumber))
                    result[listNumber] = new List<TSource>();

                result[listNumber].Add(item);

                // Увеличиваем счётчик для этого ключа
                occurrence[key] = currentOccurrence + 1;
            }

            return result;
        }
    }
}
