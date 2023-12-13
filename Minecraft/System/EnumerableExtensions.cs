using Minecraft.WorldBuilding;
using OpenTK.Mathematics;

namespace Minecraft.System
{
    internal static class EnumerableExtensions
    {
        #region General
        internal static List<T> Add<T>(List<T> values1, List<T> values2)
        {
            List<T> list = new List<T> { Capacity = values1.Count + values2.Count };
            list.AddRange(values1 as List<T>);
            list.AddRange(values2 as List<T>);

            return list;
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> firstDictionary, Dictionary<TKey, TValue> secondDictionary)
        {
            Dictionary<TKey, TValue> mergedDictionary = new Dictionary<TKey, TValue>(firstDictionary);

            foreach (var kvp in secondDictionary)
            {
                if (!mergedDictionary.ContainsKey(kvp.Key))
                {
                    mergedDictionary[kvp.Key] = kvp.Value;
                }
            }

            return mergedDictionary;
        }

        internal static float Average(this float[,] values)
        {
            if (values == null)
                return 0;

            float sum = 0;
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    sum += values[i, j];
                }
            }

            return sum / values.Length;
        }

        internal static (int, int) IndexOfLargest(this float[,] values)
        {
            float largest = 0;
            (int, int) index = (0, 0);
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (largest < values[i, j])
                    {
                        largest = values[i, j];
                        index = (i, j);
                    }
                }
            }
            return index;
        }

        internal static T[] FlattenArray<T>(this T[,] array)
        {
            // Get the total number of elements in the 2D array
            int totalElements = array.Length;

            // Use LINQ to flatten the 2D array into a 1D array
            T[] flattenedArray = new T[totalElements];
            var query = from T element in array
                        select element;

            // Copy the elements to the flattened array
            Array.Copy(query.ToArray(), flattenedArray, totalElements);

            return flattenedArray;
        }

        internal static List<T> Remove<T>(this List<T> values, Func<T, bool> function)
        {
            List<int> indicies = new List<int>();
            for (int i = 0; i < values.Count; i++)
            {
                bool remove = function.Invoke(values[i]);
                if (remove)
                    indicies.Add(i);
            }

            indicies.Sort();
            for (int i = indicies.Count - 1; i >= 0; i--)
            {
                values.RemoveAt(indicies[i]);
            }
            return values;
        }
        #endregion

        #region List Extensions
        internal static bool Contains(this List<ChunkColumn> values, Vector2i position)
        {
            foreach (ChunkColumn value in values)
            {
                if (value != null)
                {
                    if (value.Position == position)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static int IndexOf(this List<ChunkColumn> values, Vector2i position)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != null)
                {
                    if (values[i].Position == position)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal static void Remove(this List<ChunkColumn> values, Vector2i position)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != null)
                {
                    if (values[i].Position == position)
                    {
                        values.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        internal static List<T> Remove<T>(this List<T> values, List<T> valuesToBeRemoved)
        {
            List<int> indicies = new List<int>();
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (valuesToBeRemoved.Contains(values[i]))
                {
                    indicies.Add(i);
                }
            }

            foreach (int index in indicies)
            {
                values.RemoveAt(index);
            }
            indicies.Clear();

            return values;
        }

        internal static void RemoveUnloadedItems(this List<ChunkColumn> values)
        {
            List<int> indicies = new List<int>();
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (values[i].IsUnloaded)
                {
                    indicies.Add(i);
                }
            }

            foreach (int index in indicies)
            {
                values.RemoveAt(index);
            }
            indicies.Clear();
        }

        internal static int IndexOf<Value>(this List<Tuple<float, Value>> tuples, float key)
        {
            for (int i = 0; i < tuples.Count; i++)
            {
                if (tuples[i].Item1 == key)
                    return i;
            }
            return -1;
        }

        internal static Vector2 ValueOfLesser(this List<Vector2> vector2s, float x)
        {
            int lastIndexLesser = -1;
            for (int i = 0; i < vector2s.Count; i++)
            {
                if (vector2s[i].X < x)
                    lastIndexLesser = i;
                else if (vector2s[i].X == x)
                    return vector2s[i];
                else if (vector2s[i].X > x)
                    return vector2s[lastIndexLesser];
            }
            return new Vector2(0, 0);
        }

        internal static Vector2 ValueOfGreater(this List<Vector2> vector2s, float x)
        {
            int lastIndexLesser = -1;
            for (int i = vector2s.Count - 1; i > 0; i--)
            {
                if (vector2s[i].X > x)
                    lastIndexLesser = i;
                else if (vector2s[i].X == x)
                    return vector2s[i];
                else if (vector2s[i].X < x)
                    return vector2s[lastIndexLesser];
            }
            return new Vector2(1, 1);
        }

        internal static List<(TKey, TValue)> ConvertKeyValuePairToTuple<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            List<(TKey, TValue)> result = new List<(TKey, TValue)>();
            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                result.Add((keyValuePairs[i].Key, keyValuePairs[i].Value));
            }
            return result;
        }

        internal static Dictionary<TKey, TValue> RemoveDoubleKeys<TKey, TValue>(this Dictionary<TKey, (TKey, TValue)> dictionary)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            for (int i = 0; i < dictionary.Count; i++)
            {
                result.Add(dictionary.Values.ToArray()[i].Item1, dictionary.Values.ToArray()[i].Item2);
            }

            return result;
        }

        internal static Dictionary<TKey, TValue> RemoveDoubleKeys<TKey, TValue>(this Dictionary<TKey, KeyValuePair<TKey, TValue>> dictionary)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            for (int i = 0; i < dictionary.Count; i++)
            {
                result.Add(dictionary.Values.ToArray()[i].Key, dictionary.Values.ToArray()[i].Value);
            }

            return result;
        }

        internal static List<BlockStruct> RemoveDoubleXZ(this List<BlockStruct> values)
        {
            List<Vector2i> position = new List<Vector2i>();
            List<BlockStruct> blockStructsToRemove = new List<BlockStruct>();
            List<BlockStruct> result = new List<BlockStruct>();

            for (int i = 0; i < values.Count; i++)
            {
                result.Add(values[i]);
                if (position.Contains(values[i].Position.Xz))
                    blockStructsToRemove.Add(values[i]);
                else
                    position.Add(values[i].Position.Xz);
            }

            for (int i = 0; i < blockStructsToRemove.Count; i++)
            {
                result.Remove(blockStructsToRemove);
            }

            return result;
        }
        #endregion

        #region Queue Extensions
        internal static bool Contains(this Queue<ChunkColumn> values, Vector2i position)
        {
            foreach (ChunkColumn value in values)
            {
                if (value != null)
                {
                    if (value.Position == position)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void SortByDistanceMinMax(this Queue<Vector2i> queue, Vector3 position)
        {
            List<Vector2i> queueToList = queue.ToList();
            queue.Clear();

            List<float> ChunksDistance = new List<float>();
            List<Tuple<float, Vector2i>> ChunksPosition = new List<Tuple<float, Vector2i>>();

            Vector2 PlayerPositionInChunk = position.Xz / ChunkColumn.ChunkSize;

            for (int i = 0; i < queueToList.Count; i++)
            {
                float distance = Vector2.Distance(queueToList[i], PlayerPositionInChunk);

                ChunksPosition.Add(Tuple.Create(distance, queueToList[i]));
                ChunksDistance.Add(distance);
            }

            ChunksDistance.Sort();

            for (int i = 0; i < ChunksDistance.Count; i++)
            {
                int index = ChunksPosition.IndexOf(ChunksDistance[i]);
                Vector2i buffer = ChunksPosition[index].Item2;
                ChunksPosition.RemoveAt(index);
                queue.Enqueue(buffer);
            }
        }

        internal static void SortByDistanceMinMax(this Queue<ChunkColumn> queue, Vector3 position)
        {
            List<ChunkColumn> queueToList = queue.ToList();
            queue.Clear();

            List<float> ChunksDistance = new List<float>();
            List<Tuple<float, ChunkColumn>> ChunksPosition = new List<Tuple<float, ChunkColumn>>();

            Vector2 PlayerPositionInChunk = position.Xz / ChunkColumn.ChunkSize;

            for (int i = 0; i < queueToList.Count; i++)
            {
                float distance = Vector2.Distance(queueToList[i].Position, PlayerPositionInChunk);

                ChunksPosition.Add(Tuple.Create(distance, queueToList[i]));
                ChunksDistance.Add(distance);
            }

            ChunksDistance.Sort();

            for (int i = 0; i < ChunksDistance.Count; i++)
            {
                int index = ChunksPosition.IndexOf(ChunksDistance[i]);
                ChunkColumn buffer = ChunksPosition[index].Item2;
                ChunksPosition.RemoveAt(index);
                queue.Enqueue(buffer);
            }
        }

        internal static void SortByDistanceMaxMin(this Queue<ChunkColumn> queue, Vector3 position)
        {
            List<ChunkColumn> queueToList = queue.ToList();
            queue.Clear();

            List<ChunkColumn> ChunksPositionSorted = new List<ChunkColumn>();
            List<float> ChunksDistance = new List<float>();
            List<Tuple<float, ChunkColumn>> ChunksPosition = new List<Tuple<float, ChunkColumn>>();

            Vector2 PlayerPositionInChunk = position.Xz / ChunkColumn.ChunkSize;

            for (int i = 0; i < queueToList.Count; i++)
            {
                float distance = Vector2.Distance(queueToList[i].Position, PlayerPositionInChunk);

                ChunksPosition.Add(Tuple.Create(distance, queueToList[i]));
                ChunksDistance.Add(distance);
            }

            ChunksDistance.Sort();

            for (int i = ChunksDistance.Count - 1; i > -1; i--)
            {
                int index = ChunksPosition.IndexOf(ChunksDistance[i]);
                ChunkColumn buffer = ChunksPosition[index].Item2;
                ChunksPosition.RemoveAt(index);
                queue.Enqueue(buffer);
            }
        }

        internal static Queue<T> ToQueue<T>(this List<T> values)
        {
            Queue<T> queue = new Queue<T>();

            foreach (T value in values)
            {
                if (value != null)
                    queue.Enqueue(value);
            }

            return queue;
        }

        internal static void Remove(this Queue<Vector2i> values, Func<Vector2i, bool> keeper)
        {
            IEnumerable<Vector2i> chunksToRemove = values.ToArray().Where(keeper);
            values.Clear();

            foreach (Vector2i chunk in chunksToRemove)
            {
                values.Enqueue(chunk);
            }
        }
        internal static void Remove(this Queue<ChunkColumn> values, Func<ChunkColumn, bool> keeper)
        {
            IEnumerable<ChunkColumn> chunksToRemove = values.ToArray().Where(keeper);
            values.Clear();

            foreach (ChunkColumn chunk in chunksToRemove)
            {
                values.Enqueue(chunk);
            }
        }


        internal static void RemoveNotIn(this Queue<Vector2i> values, List<Vector2i> valuesNotToRemove)
        {
            List<Vector2i> valuesToList = values.ToList();
            values.Clear();

            for (int i = 0; i < valuesToList.Count; i++)
            {
                bool IsChunkInRenderDistance = false;
                foreach (Vector2i position in valuesNotToRemove)
                {
                    if (valuesToList[i] == position)
                    {
                        IsChunkInRenderDistance = true;
                        break;
                    }
                }
                if (!IsChunkInRenderDistance)
                {
                    valuesToList.RemoveAt(i);
                }
            }
            for (int i = 0; i < valuesToList.Count; i++)
            {
                values.Enqueue(valuesToList[i]);
            }
        }

        internal static void RemoveNotIn(this Queue<ChunkColumn> values, List<Vector2i> valuesNotToRemove)
        {
            List<ChunkColumn> valuesToList = values.ToList();
            values.Clear();

            for (int i = 0; i < valuesToList.Count; i++)
            {
                bool IsChunkInRenderDistance = false;
                foreach (Vector2i position in valuesNotToRemove)
                {
                    if (valuesToList[i].Position == position)
                    {
                        IsChunkInRenderDistance = true;
                        break;
                    }
                }
                if (!IsChunkInRenderDistance)
                {
                    valuesToList.RemoveAt(i);
                }
            }
            for (int i = 0; i < valuesToList.Count; i++)
            {
                values.Enqueue(valuesToList[i]);
            }
        }
        #endregion

        #region Priority Queue Extensions
        private static readonly Func<(Vector2i, float), Vector2i> keySelector = chunk => chunk.Item1;

        internal static bool Contains(this PriorityQueue<Vector2i, float> values, Vector2i position)
        {
            Dictionary<Vector2i, (Vector2i, float)> dictionary = values.UnorderedItems.ToDictionary(keySelector);

            if (dictionary.ContainsKey(position))
                return true;
            return false;
        }

        internal static void Remove(this PriorityQueue<Vector2i, float> values, Func<(Vector2i, float), bool> keeper)
        {
            IEnumerable<(Vector2i, float)> chunksToRemove = values.UnorderedItems.ToList().Where(keeper);
            values.Clear();

            foreach ((Vector2i, float) chunk in chunksToRemove)
            {
                values.Enqueue(chunk.Item1, chunk.Item2);
            }
        }
        #endregion
    }
}