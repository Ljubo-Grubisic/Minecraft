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
        #endregion

        #region List Extensions
        internal static bool Contains(this List<Chunk> values, Vector2i position)
        {
            foreach (Chunk value in values)
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

        internal static int IndexOf(this List<Chunk> values, Vector2i position)
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

        internal static void Remove(this List<Chunk> values, Vector2i position)
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
            bool needsGarbageCollector = false;
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (valuesToBeRemoved.Contains(values[i]))
                {
                    indicies.Add(i);
                }
            }

            if (indicies.Count > 50)
                needsGarbageCollector = true;

            foreach (int index in indicies)
            {
                values.RemoveAt(index);
            }
            indicies.Clear();

            if (needsGarbageCollector)
                GC.Collect();

            return values;
        }

        internal static void RemoveUnloadedItems(this List<Chunk> values)
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
        #endregion

        #region Queue Extensions
        internal static bool Contains(this Queue<Chunk> values, Vector2i position)
        {
            foreach (Chunk value in values)
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

            Vector2 PlayerPositionInChunk = position.Xz / Chunk.Size.Xz;

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

        internal static void SortByDistanceMinMax(this Queue<Chunk> queue, Vector3 position)
        {
            List<Chunk> queueToList = queue.ToList();
            queue.Clear();

            List<float> ChunksDistance = new List<float>();
            List<Tuple<float, Chunk>> ChunksPosition = new List<Tuple<float, Chunk>>();

            Vector2 PlayerPositionInChunk = position.Xz / Chunk.Size.Xz;

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
                Chunk buffer = ChunksPosition[index].Item2;
                ChunksPosition.RemoveAt(index);
                queue.Enqueue(buffer);
            }
        }

        internal static void SortByDistanceMaxMin(this Queue<Chunk> queue, Vector3 position)
        {
            List<Chunk> queueToList = queue.ToList();
            queue.Clear();

            List<Chunk> ChunksPositionSorted = new List<Chunk>();
            List<float> ChunksDistance = new List<float>();
            List<Tuple<float, Chunk>> ChunksPosition = new List<Tuple<float, Chunk>>();

            Vector2 PlayerPositionInChunk = position.Xz / Chunk.Size.Xz;

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
                Chunk buffer = ChunksPosition[index].Item2;
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
        internal static void Remove(this Queue<Chunk> values, Func<Chunk, bool> keeper)
        {
            IEnumerable<Chunk> chunksToRemove = values.ToArray().Where(keeper);
            values.Clear();

            foreach (Chunk chunk in chunksToRemove)
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

        internal static void RemoveNotIn(this Queue<Chunk> values, List<Vector2i> valuesNotToRemove)
        {
            List<Chunk> valuesToList = values.ToList();
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