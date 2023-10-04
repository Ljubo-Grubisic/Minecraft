using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading;
using Minecraft;

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
        Generate,
        Bake,
        Unload
    }

    internal static class ChunkManager
    {
        internal static List<Chunk> LoadedChunks = new List<Chunk>();

        private static Queue<Vector2i> ChunksWaitingToGenerate = new Queue<Vector2i>();
        private static Queue<Chunk> ChunksWaitingToBake = new Queue<Chunk>();
        private static Queue<Chunk> ChunksWaitingToUnload = new Queue<Chunk>();

        internal static int SpawnChunkSize { get; set; } = 5;

        internal static int TicksPerSecond { get; set; } = 20;
        internal static float TimeUntilUpdate = 1.0f / TicksPerSecond;

        private static Thread ChunkManagingThread;

        internal static void Init()
        {
            LoadSpawnChunks();
            ChunkManagingThread = new Thread(Loop) { IsBackground = true, Name = "ChunkManagingThread", Priority = ThreadPriority.AboveNormal };
            ChunkManagingThread.Start();
        }

        internal static void Loop()
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
                            Vector2i chunkPosition = ChunksWaitingToGenerate.Dequeue();
                            ChunksWaitingToBake.Enqueue(new Chunk(chunkPosition));
                            break;
                        case ChunkLoadingSteps.Bake:
                            Chunk chunk = ChunksWaitingToBake.Dequeue();
                            Program.Minecraft.QueueOperation(() =>
                            {
                                BakeChunk(chunk);
                                LoadedChunks.Add(chunk);
                            });
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
                    ChunkPositions.Add(new Vector2i((int)Math.Round(PlayerPositionInChunk.X + x), (int)Math.Round(PlayerPositionInChunk.Y + z)));
                }
            }

            return ChunkPositions;
        }

        private static bool ChunkManagerContainsChunk(Vector2i position)
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

        internal static void LoadSpawnChunks()
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

        internal static void BakeChunk(Chunk chunk)
        {
            List<int> index = new List<int>();
            List<Chunk?> neighbors = new List<Chunk?>();

            index.Clear();
            neighbors.Clear();

            index.Add(LoadedChunks.IndexOf(new Vector2i(-1, 0) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(1, 0) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(0, -1) + chunk.Position));
            index.Add(LoadedChunks.IndexOf(new Vector2i(0, 1) + chunk.Position));

            for (int j = 0; j < index.Count; j++)
            {
                if (index[j] == -1)
                {
                    neighbors.Add(null);
                }
                else
                {
                    neighbors.Add(LoadedChunks[index[j]]);
                }
            }

            chunk.Bake(neighbors);
        } 

        internal static void BakeAllChunks()
        {
            List<int> index = new List<int>();
            List<Chunk?> neighbors = new List<Chunk?>();
            for (int i = 0; i < LoadedChunks.Count; i++)
            {
                index.Clear();
                neighbors.Clear();

                index.Add(LoadedChunks.IndexOf(new Vector2i(-1, 0) + LoadedChunks[i].Position));
                index.Add(LoadedChunks.IndexOf(new Vector2i(1, 0) + LoadedChunks[i].Position));
                index.Add(LoadedChunks.IndexOf(new Vector2i(0, -1) + LoadedChunks[i].Position));
                index.Add(LoadedChunks.IndexOf(new Vector2i(0, 1) + LoadedChunks[i].Position));

                for (int j = 0; j < index.Count; j++)
                {
                    if (index[j] == -1)
                    {
                        neighbors.Add(null);
                    }
                    else
                    {
                        neighbors.Add(LoadedChunks[index[j]]);
                    }
                }

                LoadedChunks[i].Bake(neighbors);
            }
        }
    }
}
