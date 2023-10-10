using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Windows.Markup;

namespace Minecraft.WorldBuilding
{
    internal static class Extensions
    {
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
    }

    internal enum ChunkLoadingSteps
    {
        None = 0,
        Generate = 1,
        Bake = 2,
        Unload = 3
    }

    internal static class ChunkManager
    {
        internal static List<Chunk> ChunksLoaded = new List<Chunk>();

        private static Queue<Vector2i> ChunksWaitingToGenerate = new Queue<Vector2i>();
        private static Queue<Chunk> ChunksWaitingToBake = new Queue<Chunk>();
        private static Queue<Chunk> ChunksWaitingToUnload = new Queue<Chunk>();

        /// <summary>
        /// This list contains chunks that are just know loaded. The point of this list
        /// is to remove the problem where a chunk is loaded but isnt it LoadedChunks
        /// because the main thread hasnt added it yet
        /// </summary>
        private static List<Chunk> ChunksJustGenerated = new List<Chunk>();

        internal static int SpawnChunkSize { get; set; } = 5;

        internal static int TicksPerSecond { get; set; } = 1000;
        internal static float TimeUntilUpdate = 1.0f / TicksPerSecond;

        private static Thread ChunkManagingThread;

        internal static void Init()
        {
            LoadSpawnChunks();
            ChunkManagingThread = new Thread(Loop) { IsBackground = true, Name = "ChunkManagingThread", Priority = ThreadPriority.AboveNormal };
            ChunkManagingThread.Start();
        }

        private static void Loop()
        {
            float totalTimeBeforeUpdate = 0f;
            float previousTimeElapsed = 0f;
            float deltaTime = 0f;
            float totalTimeElapsed = 0f;
            ChunkLoadingSteps chunkLoadingSteps = ChunkLoadingSteps.None;
            int garbageCollectorCounter = 0;

            while (ChunkManagingThread.IsAlive)
            {
                totalTimeElapsed = (float)GLFW.GetTime();
                deltaTime = totalTimeElapsed - previousTimeElapsed;
                previousTimeElapsed = totalTimeElapsed;

                totalTimeBeforeUpdate += deltaTime;

                if (totalTimeBeforeUpdate >= TimeUntilUpdate)
                {
                    totalTimeBeforeUpdate = 0;

                    List<Vector2i> ChunkPositionsAroundPlayer = ChunkPositionAroundPlayer(Program.Minecraft.Player);
                    foreach (Vector2i position in ChunkPositionsAroundPlayer)
                    {
                        if (!ChunkManagerContainsChunk(position))
                        {
                            ChunksWaitingToGenerate.Enqueue(position);
                        }
                    }

                    lock (ChunksLoaded)
                    {
                        for (int i = 0; i < ChunksLoaded.Count; i++)
                        {
                            if (!ChunksWaitingToUnload.Contains(ChunksLoaded[i]))
                            {
                                bool IsChunkInRenderDistance = false;
                                foreach (Vector2i position in ChunkPositionsAroundPlayer)
                                {
                                    if (ChunksLoaded[i].Position == position)
                                    {
                                        IsChunkInRenderDistance = true;
                                        break;
                                    }
                                }
                                if (!IsChunkInRenderDistance)
                                {
                                    ChunksWaitingToUnload.Enqueue(ChunksLoaded[i]);
                                }
                            }
                        }
                    }

                    ChunksJustGenerated.RemoveUnloadedItems();
                    ChunksJustGenerated = RemoveItems(ChunksJustGenerated, ChunksLoaded);

                    for (int i = 0; i < ChunksLoaded.Count; i++)
                    {
                        for (int j = 0; j < ChunksLoaded.Count; j++)
                        {
                            if (ChunksLoaded[i] == ChunksLoaded[j] && i != j)
                                Console.WriteLine("Duplo!");
                        }
                    }

                    switch (chunkLoadingSteps)
                    {
                        case ChunkLoadingSteps.Generate:

                            if (ChunksWaitingToGenerate.Count != 0)
                            {
                                Chunk chunkGenerated = new Chunk(ChunksWaitingToGenerate.Dequeue());
                                ChunksWaitingToBake.Enqueue(chunkGenerated);

                                ChunksJustGenerated = RemoveItems(ChunksJustGenerated, ChunksLoaded);
                                ChunksJustGenerated.Add(chunkGenerated);

                                List<Chunk> totalChunksList = new List<Chunk>();
                                totalChunksList.AddRange(ChunksLoaded);
                                totalChunksList.AddRange(ChunksJustGenerated);
                                List<Chunk> neighbors = FindNeighbors(FindNeighborIndexs(chunkGenerated, totalChunksList), totalChunksList);

                                foreach (Chunk neighbor in neighbors)
                                {
                                    if (neighbor != null)
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                }
                            }
                            break;

                        case ChunkLoadingSteps.Bake:

                            if (ChunksWaitingToBake.Count != 0)
                            {
                                Chunk chunkBaked;
                                do
                                {
                                    chunkBaked = ChunksWaitingToBake.Dequeue();
                                } 
                                while ((ChunksWaitingToUnload.Contains(chunkBaked.Position) || chunkBaked.IsUnloaded) && ChunksWaitingToBake.Count != 0);

                                if (!ChunksWaitingToUnload.Contains(chunkBaked.Position) && !chunkBaked.IsUnloaded)
                                {
                                    Program.Minecraft.QueueOperation(() =>
                                    {
                                        if (!ChunksLoaded.Contains(chunkBaked))
                                            ChunksLoaded.Add(chunkBaked);
                                        BakeChunk(chunkBaked);
                                    });
                                }
                            }
                            break;

                        case ChunkLoadingSteps.Unload:

                            if (ChunksWaitingToUnload.Count != 0)
                            {
                                Chunk chunkUnloaded = ChunksWaitingToUnload.Dequeue();
                                chunkUnloaded.Unload();
                                lock (ChunksLoaded)
                                    ChunksLoaded.Remove(chunkUnloaded.Position);

                                List<Chunk> totalChunksList = new List<Chunk> { Capacity = ChunksLoaded.Count + ChunksJustGenerated.Count };
                                totalChunksList.AddRange(ChunksLoaded);
                                totalChunksList.AddRange(ChunksJustGenerated);
                                List<Chunk> neighbors = FindNeighbors(FindNeighborIndexs(chunkUnloaded, totalChunksList), totalChunksList);
                                totalChunksList.Clear();

                                foreach (Chunk neighbor in neighbors)
                                {
                                    if (neighbor != null && !ChunksWaitingToUnload.Contains(neighbor.Position))
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                }

                                if (garbageCollectorCounter == 2)
                                {
                                    GC.Collect();
                                    garbageCollectorCounter = 0;
                                }

                                garbageCollectorCounter++;
                            }
                            chunkLoadingSteps = ChunkLoadingSteps.None;
                            break;
                    }
                    chunkLoadingSteps++;
                }
            }
        }

        private static List<Vector2i> ChunkPositionAroundPlayer(Player player)
        {
            List<Vector2i> ChunkPositions = new List<Vector2i>();
            Vector2 PlayerPositionInChunk = player.Position.Xz / Chunk.Size.Xz;

            for (int x = 0; x < player.RenderDistance; x++)
            {
                for (int z = 0; z < player.RenderDistance; z++)
                {
                    ChunkPositions.Add(new Vector2i((int)Math.Round(PlayerPositionInChunk.X - player.RenderDistance / 2 + x), (int)Math.Round(PlayerPositionInChunk.Y - player.RenderDistance / 2 + z)));
                }
            }

            return ChunkPositions;
        }

        private static bool ChunkManagerContainsChunk(Vector2i position)
        {
            lock (ChunksLoaded)
            {
                if (ChunksLoaded.Contains(position))
                    return true;
                else if (ChunksWaitingToGenerate.Contains(position))
                    return true;
                else if (ChunksWaitingToBake.Contains(position))
                    return true;
                else if (ChunksWaitingToUnload.Contains(position))
                    return true;
                else if (ChunksJustGenerated.Contains(position))
                    return true;
                else
                    return false;
            }
        }

        private static void LoadSpawnChunks()
        {
            for (int i = 0; i < SpawnChunkSize; i++)
            {
                for (int j = 0; j < SpawnChunkSize; j++)
                {
                    ChunksLoaded.Add(new Chunk(new Vector2i(i - (SpawnChunkSize / 2), j - (SpawnChunkSize / 2))));
                }
            }

            BakeAllChunks();
        }

        private static void BakeChunk(Chunk chunk)
        {
            chunk.Bake(FindNeighbors(FindNeighborIndexs(chunk)));
        }

        private static void BakeAllChunks()
        {
            List<int> index = new List<int>();
            List<Chunk?> neighbors = new List<Chunk?>();
            for (int i = 0; i < ChunksLoaded.Count; i++)
            {
                index = FindNeighborIndexs(ChunksLoaded[i]);
                neighbors = FindNeighbors(index);

                ChunksLoaded[i].Bake(neighbors);
            }
        }


        private static List<Chunk?> FindNeighbors(List<int> neighborsIndex)
        {
            List<Chunk?> neighbors = new List<Chunk?>();

            for (int j = 0; j < neighborsIndex.Count; j++)
            {
                if (neighborsIndex[j] == -1)
                {
                    neighbors.Add(null);
                }
                else
                {
                    neighbors.Add(ChunksLoaded[neighborsIndex[j]]);
                }
            }

            return neighbors;
        }
        private static List<Chunk?> FindNeighbors(List<int> neighborsIndex, List<Chunk> chunks)
        {
            List<Chunk?> neighbors = new List<Chunk?>();

            for (int j = 0; j < neighborsIndex.Count; j++)
            {
                if (neighborsIndex[j] == -1)
                {
                    neighbors.Add(null);
                }
                else
                {
                    neighbors.Add(chunks[neighborsIndex[j]]);
                }
            }

            return neighbors;
        }

        private static List<int> FindNeighborIndexs(Chunk chunk)
        {
            List<int> index = new List<int>
            {
                ChunksLoaded.IndexOf(new Vector2i(-1, 0) + chunk.Position),
                ChunksLoaded.IndexOf(new Vector2i(1, 0) + chunk.Position),
                ChunksLoaded.IndexOf(new Vector2i(0, -1) + chunk.Position),
                ChunksLoaded.IndexOf(new Vector2i(0, 1) + chunk.Position)
            };

            return index;
        }

        private static List<int> FindNeighborIndexs(Chunk chunk, List<Chunk> chunks)
        {
            List<int> index = new List<int>
            {
                chunks.IndexOf(new Vector2i(-1, 0) + chunk.Position),
                chunks.IndexOf(new Vector2i(1, 0) + chunk.Position),
                chunks.IndexOf(new Vector2i(0, -1) + chunk.Position),
                chunks.IndexOf(new Vector2i(0, 1) + chunk.Position)
            };

            return index;
        }

        private static List<T> RemoveItems<T>(List<T> values, List<T> valuesToBeRemoved)
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
    }
}
