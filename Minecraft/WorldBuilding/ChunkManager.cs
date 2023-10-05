using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

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
        internal static List<Chunk> LoadedChunks = new List<Chunk>();

        private static Queue<Vector2i> ChunksWaitingToGenerate = new Queue<Vector2i>();
        private static Queue<Chunk> ChunksWaitingToBake = new Queue<Chunk>();
        private static Queue<Chunk> ChunksWaitingToUnload = new Queue<Chunk>();

        internal static int SpawnChunkSize { get; set; } = 5;

        internal static int TicksPerSecond { get; set; } = 60;
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

                    switch (chunkLoadingSteps)
                    {
                        case ChunkLoadingSteps.Generate:

                            if (ChunksWaitingToGenerate.Count != 0)
                            {
                                Vector2i chunkPosition = ChunksWaitingToGenerate.Dequeue();
                                Chunk chunk = new Chunk(chunkPosition);
                                ChunksWaitingToBake.Enqueue(chunk);
                                List<Chunk> neighbors = FindNeighbors(FindNeighborIndexs(chunk));

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
                                Chunk chunk = ChunksWaitingToBake.Dequeue();
                                Program.Minecraft.QueueOperation(() =>
                                {
                                    LoadedChunks.Add(chunk);
                                    BakeChunk(chunk);
                                });
                            }
                            break;

                        case ChunkLoadingSteps.Unload:

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
            lock (LoadedChunks)
            {
                if (LoadedChunks.Contains(position))
                    return true;
                else if (ChunksWaitingToGenerate.Contains(position))
                    return true;
                else if (ChunksWaitingToBake.Contains(position))
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
                    LoadedChunks.Add(new Chunk(new Vector2i(i - (SpawnChunkSize / 2), j - (SpawnChunkSize / 2))));
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
            for (int i = 0; i < LoadedChunks.Count; i++)
            {
                index = FindNeighborIndexs(LoadedChunks[i]);
                neighbors = FindNeighbors(index);

                LoadedChunks[i].Bake(neighbors);
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
                    neighbors.Add(LoadedChunks[neighborsIndex[j]]);
                }
            }

            return neighbors;
        }

        private static List<int> FindNeighborIndexs(Chunk chunk)
        {
            List<int> index = new List<int>();

            index.Add(LoadedChunks.IndexOf(new Vector2i(-1, 0) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(1, 0) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(0, -1) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(0, 1) + chunk.Position));

            return index;
        }
    }
}
