using Minecraft.Entitys;
using Minecraft.System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Immutable;
using System.Linq;

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
        internal static Dictionary<Vector2i, Chunk> ChunksLoaded = new Dictionary<Vector2i, Chunk>();

        private static PriorityQueue<Vector2i, float> ChunksWaitingToGenerate = new PriorityQueue<Vector2i, float>();
        private static Queue<Chunk> ChunksWaitingToBake = new Queue<Chunk>();
        private static Queue<Chunk> ChunksWaitingToUnload = new Queue<Chunk>();

        internal static int SpawnChunkSize { get; set; } = 1;

        internal static int TicksPerSecond { get; set; } = 250;
        internal static float TimeUntilUpdate = 1.0f / TicksPerSecond;

        private static readonly Func<(Vector2i, float), Vector2i> keySelectorPriorityQueue = chunk => chunk.Item1;
        private static readonly Func<Chunk, Vector2i> keySelector = chunk => chunk.Position;
        private static readonly Func<KeyValuePair<Vector2i, Chunk>, bool> keyRemoverDictionary = chunk =>
        {
            return GetDistanceFromPlayer(chunk.Value.Position) > (Program.Minecraft.Player.RenderDistance / 2.0f);
        };
        private static readonly Func<(Vector2i, float), bool> keyKeeperPriorityQueueVector2i = chunk =>
        {
            return GetDistanceFromPlayer(chunk.Item1) < (Program.Minecraft.Player.RenderDistance / 2.0f);
        };

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
                    Console.WriteLine("ChunkManaginThread fps:" + (float)Math.Round(1 / totalTimeBeforeUpdate));
                    totalTimeBeforeUpdate = 0;

                    // Generate chunks that are close to the player
                    List<Vector2i> ChunkPositionsAroundPlayer = ChunkPositionAroundPlayer(Program.Minecraft.Player);

                    Dictionary<Vector2i, (Vector2i, float)> dictionary = ChunksWaitingToGenerate.UnorderedItems.ToDictionary(keySelectorPriorityQueue);
                    foreach (Vector2i position in ChunkPositionsAroundPlayer)
                    {
                        if (!ChunkManagerContainsChunk(position, dictionary))
                        {
                            ChunksWaitingToGenerate.Enqueue(position, GetDistanceFromPlayer(position));
                        }
                    }

                    // Unload chunks that are far from the player
                    lock (ChunksLoaded)
                    {
                        IEnumerable<KeyValuePair<Vector2i, Chunk>> chunksToUnload = ChunksLoaded.Where(keyRemoverDictionary);

                        foreach (KeyValuePair<Vector2i, Chunk> chunkToUnload in chunksToUnload)
                        {
                            if (!ChunksWaitingToUnload.Contains(chunkToUnload.Value))
                            {
                                ChunksWaitingToUnload.Enqueue(chunkToUnload.Value);
                            }
                        }
                    }

                    ChunksWaitingToGenerate.Remove(keyKeeperPriorityQueueVector2i);

                    switch (chunkLoadingSteps)
                    {
                        case ChunkLoadingSteps.Generate:

                            if (ChunksWaitingToGenerate.Count != 0)
                            {
                                Chunk chunkGenerated = new Chunk(ChunksWaitingToGenerate.Dequeue());
                                ChunksWaitingToBake.Enqueue(chunkGenerated);

                                lock (ChunksLoaded)
                                {
                                    if (!ChunksLoaded.ContainsKey(chunkGenerated.Position))
                                        ChunksLoaded.Add(chunkGenerated.Position, chunkGenerated);
                                }
                                
                                foreach (Chunk neighbor in FindNeighbors(chunkGenerated))
                                {
                                    if (neighbor != null && !ChunksWaitingToUnload.Contains(neighbor.Position) && !ChunksWaitingToBake.Contains(neighbor))
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

                                if (!ChunksWaitingToUnload.Contains(chunkBaked.Position) || !chunkBaked.IsUnloaded)
                                {
                                    BakeChunk(chunkBaked);
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

                                foreach (Chunk neighbor in FindNeighbors(chunkUnloaded))
                                {
                                    if (neighbor != null && !ChunksWaitingToUnload.Contains(neighbor.Position))
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                }

                                if (garbageCollectorCounter == 100)
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
            Vector2i ChunkPosition = new Vector2i();

            for (int x = 0; x < player.RenderDistance; x++)
            {
                for (int z = 0; z < player.RenderDistance; z++)
                {
                    ChunkPosition = new Vector2i((int)Math.Round(PlayerPositionInChunk.X - player.RenderDistance / 2 + x), (int)Math.Round(PlayerPositionInChunk.Y - player.RenderDistance / 2 + z));
                    float distance = Vector2.Distance(PlayerPositionInChunk, ChunkPosition);
                    if (distance < player.RenderDistance / 2.0f)
                        ChunkPositions.Add(ChunkPosition);
                }
            }

            return ChunkPositions;
        }

        private static float GetDistanceFromPlayer(Vector2i chunkPosition)
        {
            return Vector2.Distance(Program.Minecraft.Player.Position.Xz / Chunk.Size.Xz, chunkPosition);
        }

        private static bool ChunkManagerContainsChunk(Vector2i position, Dictionary<Vector2i, (Vector2i, float)> chunksWaitingToGenerate)
        {
            lock (ChunksLoaded)
            {
                if (ChunksLoaded.ContainsKey(position))
                    return true;
                else if (chunksWaitingToGenerate.ContainsKey(position))
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
                    Vector2i position = new Vector2i(i - (SpawnChunkSize / 2), j - (SpawnChunkSize / 2));
                    ChunksLoaded.Add(position, new Chunk(position));
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
                neighbors = FindNeighbors(ChunksLoaded.Values.ToList()[i]);

                ChunksLoaded.Values.ToList()[i].Bake(neighbors);
            }
        }

        private static List<Chunk> FindNeighbors(Chunk chunk)
        {
            List<Chunk> chunks = new List<Chunk>();
            Chunk? chunkBuffer = null;

            if (ChunksLoaded.TryGetValue(new Vector2i(-1, 0) + chunk.Position, out chunkBuffer))
                chunks.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(1, 0) + chunk.Position, out chunkBuffer))
                chunks.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(0, -1) + chunk.Position, out chunkBuffer))
                chunks.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(0, 1) + chunk.Position, out chunkBuffer))
                chunks.Add(chunkBuffer);

            return chunks;
        }

        private static List<Chunk> FindNeighbors(Chunk chunk, Dictionary<Vector2i, Chunk> chunks)
        {
            List<Chunk> chunksList = new List<Chunk>();
            Chunk? chunkBuffer = null;
            if (chunks.TryGetValue(new Vector2i(-1, 0) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (chunks.TryGetValue(new Vector2i(1, 0) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (chunks.TryGetValue(new Vector2i(0, -1) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (chunks.TryGetValue(new Vector2i(0, 1) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);

            return chunksList;
        }
    }
}
