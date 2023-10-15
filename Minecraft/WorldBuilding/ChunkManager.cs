using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Immutable;
using System.Windows.Markup;

namespace Minecraft.WorldBuilding
{
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

        internal static int SpawnChunkSize { get; set; } = 5;

        internal static int TicksPerSecond { get; set; } = 10000;
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
                    
                    List<Vector2i> chunksWaitingToGenerate = ChunksWaitingToGenerate.ToList();
                    for (int i = 0; i < chunksWaitingToGenerate.Count; i++)
                    {
                        bool IsChunkInRenderDistance = false;
                        foreach (Vector2i position in ChunkPositionsAroundPlayer)
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
                    ChunksWaitingToGenerate = chunksWaitingToGenerate.ToQueue();

                    switch (chunkLoadingSteps)
                    {
                        case ChunkLoadingSteps.Generate:

                            if (ChunksWaitingToGenerate.Count != 0)
                            {
                                Chunk chunkGenerated = new Chunk(ChunksWaitingToGenerate.Dequeue());
                                ChunksWaitingToBake.Enqueue(chunkGenerated);

                                List<Chunk> neighbors = FindNeighbors(chunkGenerated, EnumerableExtensions.Add(ChunksLoaded, ChunksWaitingToBake.ToList()));

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
                                    lock (ChunksLoaded)
                                    {
                                        if (!ChunksLoaded.Contains(chunkBaked))
                                            ChunksLoaded.Add(chunkBaked);
                                    }
                                    Program.Minecraft.QueueOperation(() =>
                                    {
                                        BakeChunk(chunkBaked);
                                    });
                                }
                            }
                            break;

                        case ChunkLoadingSteps.Unload:

                            if (ChunksWaitingToUnload.Count != 0)
                            {
                                Chunk chunkUnloaded = ChunksWaitingToUnload.Dequeue();

                                Program.Minecraft.QueueOperation(() => {
                                    chunkUnloaded.Unload();
                                });

                                lock (ChunksLoaded)
                                    ChunksLoaded.Remove(chunkUnloaded.Position);

                                List<Chunk> neighbors = FindNeighbors(chunkUnloaded, EnumerableExtensions.Add(ChunksLoaded, ChunksWaitingToBake.ToList()));

                                foreach (Chunk neighbor in neighbors)
                                {
                                    if (neighbor != null && !ChunksWaitingToUnload.Contains(neighbor.Position))
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                }

                                if (garbageCollectorCounter == 10)
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
            chunk.Bake(FindNeighbors(chunk));
        }

        private static void BakeAllChunks()
        {
            List<Chunk?> neighbors = new List<Chunk?>();
            for (int i = 0; i < ChunksLoaded.Count; i++)
            {
                neighbors = FindNeighbors(ChunksLoaded[i]);

                ChunksLoaded[i].Bake(neighbors);
            }
        }


        private static List<Chunk?> FindNeighbors(Chunk chunk)
        {
            List<Chunk?> neighbors = new List<Chunk?>();
            List<int> neighborsIndex = FindNeighborIndexs(chunk);

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
        private static List<Chunk?> FindNeighbors(Chunk chunk, List<Chunk> chunks)
        {
            List<Chunk?> neighbors = new List<Chunk?>();
            List<int> neighborsIndex = FindNeighborIndexs(chunk, chunks);

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
    }
}
