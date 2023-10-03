using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine.Rendering;

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

    internal static class ChunkManager
    {
        internal static List<Chunk> Chunks = new List<Chunk>();
        internal static Queue<Chunk> ChunksWaitingToLoad = new Queue<Chunk>();

        internal static int SpawnChunkSize { get; set; } = 5;

        internal static int TicksPerSecond { get; set; } = 20;
        internal static float TimeUntilUpdate = 1.0f / TicksPerSecond;

        private static Thread ChunkManagingThread;
        private static Player Player;

        internal static void Init(Player player)
        {
            Player = player;
            LoadSpawnChunks();
            ChunkManagingThread = new Thread(Loop) { IsBackground = true, Name = "ChunkManagingThread", Priority = ThreadPriority.AboveNormal };
            ChunkManagingThread.Start();
        }

        internal static void Update(Player player)
        {
            Player = player;
        }

        internal static void Loop()
        {
            float totalTimeBeforeUpdate = 0f;
            float previousTimeElapsed = 0f;
            float deltaTime = 0f;
            float totalTimeElapsed = 0f;
            int counter = 3;

            while (ChunkManagingThread.IsAlive)
            {
                totalTimeElapsed = (float)GLFW.GetTime();
                deltaTime = totalTimeElapsed - previousTimeElapsed;
                previousTimeElapsed = totalTimeElapsed;

                totalTimeBeforeUpdate += deltaTime;

                if (totalTimeBeforeUpdate >= TimeUntilUpdate)
                {
                    totalTimeBeforeUpdate = 0;

                    if (counter == 3)
                    {


                        counter = 0;
                    }

                    counter++;
                }
            }
        }

        internal static void LoadSpawnChunks()
        {
            for (int i = 0; i < SpawnChunkSize; i++)
            {
                for (int j = 0; j < SpawnChunkSize; j++)
                {
                    Chunks.Add(new Chunk(new Vector2i(i - (SpawnChunkSize / 2), j - (SpawnChunkSize / 2))));
                }
            }

            BakeChunks();
        }

        internal static void BakeChunks() 
        {
            List<int> index = new List<int>();
            List<Chunk?> neighbors = new List<Chunk?>();
            for (int i = 0; i < Chunks.Count; i++)
            {
                index.Clear();
                neighbors.Clear();

                index.Add(Chunks.IndexOf(new Vector2i(-1, 0) + Chunks[i].Position));
                index.Add(Chunks.IndexOf(new Vector2i(1, 0) + Chunks[i].Position));
                index.Add(Chunks.IndexOf(new Vector2i(0, -1) + Chunks[i].Position));
                index.Add(Chunks.IndexOf(new Vector2i(0, 1) + Chunks[i].Position));

                for (int j = 0; j < index.Count; j++)
                {
                    if (index[j] == -1)
                    {
                        neighbors.Add(null);
                    }
                    else
                    {
                        neighbors.Add(Chunks[index[j]]);
                    }
                }

                Chunks[i].Bake(neighbors);
            }
        }
    }
}
