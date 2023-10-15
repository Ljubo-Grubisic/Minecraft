using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace Minecraft.System
{
    internal static class EnumerableExtensions
    {
        internal static List<T> Add<T>(List<T> values1, List<T> values2)
        {
            List<T> list = new List<T> { Capacity = values1.Count + values2.Count };
            list.AddRange(values1 as List<T>);
            list.AddRange(values2 as List<T>);

            return list;
        }

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

        internal static Queue<Vector2i> RemoveNotIn(this Queue<Vector2i> values, List<Vector2i> valuesNotToRemove)
        {
            List<Vector2i> chunksWaitingToGenerate = values.ToList();
            for (int i = 0; i < chunksWaitingToGenerate.Count; i++)
            {
                bool IsChunkInRenderDistance = false;
                foreach (Vector2i position in valuesNotToRemove)
                {
                    if (chunksWaitingToGenerate[i] == position)
                    {
                        IsChunkInRenderDistance = true;
                        break;
                    }
                }
                if (!IsChunkInRenderDistance)
                {
                    chunksWaitingToGenerate.RemoveAt(i);
                }
            }
            return chunksWaitingToGenerate.ToQueue();
        }
        internal static Queue<Chunk> RemoveNotIn(this Queue<Chunk> values, List<Vector2i> valuesNotToRemove)
        {
            List<Chunk> chunksWaitingToGenerate = values.ToList();
            for (int i = 0; i < chunksWaitingToGenerate.Count; i++)
            {
                bool IsChunkInRenderDistance = false;
                foreach (Vector2i position in valuesNotToRemove)
                {
                    if (chunksWaitingToGenerate[i].Position == position)
                    {
                        IsChunkInRenderDistance = true;
                        break;
                    }
                }
                if (!IsChunkInRenderDistance)
                {
                    chunksWaitingToGenerate.RemoveAt(i);
                }
            }
            return chunksWaitingToGenerate.ToQueue();
        }

    }
}