﻿using Microsoft.Diagnostics.Tracing.Parsers.JScript;
using Minecraft.Entitys;
using Minecraft.System;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Immutable;

namespace Minecraft.WorldBuilding
{
    internal enum ChunkLoadingSteps
    {
        None = 0,
        Generate,
        Bake,
        Unload
    }

    internal static class ChunkManager
    {
        internal static Dictionary<Vector2i, ChunkColumn> ChunksLoaded = new Dictionary<Vector2i, ChunkColumn>();

        private static PriorityQueue<Vector2i, float> ChunksWaitingToGenerate = new PriorityQueue<Vector2i, float>();
        private static Queue<ChunkColumn> ChunksWaitingToBake = new Queue<ChunkColumn>();
        private static Queue<ChunkColumn> ChunksWaitingToUnload = new Queue<ChunkColumn>();

        private static Dictionary<Vector2i, ChunkColumn> ChunksWaitingToBakeDictionary = new Dictionary<Vector2i, ChunkColumn>();
        private static Dictionary<Vector2i, ChunkColumn> ChunksWaitingToUnloadDictionary = new Dictionary<Vector2i, ChunkColumn>();

        internal static int SpawnChunkSize { get; set; } = 1;

        internal static float TicksPerSecond { get; set; } = 200f;
        internal static float TimeUntilUpdate = 1.0f / TicksPerSecond;

        private static readonly Func<KeyValuePair<Vector2i, ChunkColumn>, bool> keyRemoverDictionaryByDistance = chunk =>
        {
            return GetDistanceFromPlayer(chunk.Value.Position) > (Program.Minecraft.Player.RenderDistance / 2.0f);
        };

        private static Thread ChunkManagingThread;

        internal static void Init()
        {
            LoadSpawnChunks();
            ChunkManagingThread = new Thread(Loop) { IsBackground = true, Name = "ChunkManagingThread", Priority = ThreadPriority.AboveNormal };
            ChunkManagingThread.Start();
        }

        internal static ChunkColumn? GetChunkColumn(Vector2i position)
        {
            lock (ChunksLoaded)
            {
                if (ChunksLoaded.TryGetValue(position, out ChunkColumn? chunkColumn))
                {
                    return chunkColumn;
                }
            }
            return null;
        }

        internal static void ChangeBlock(ChunkColumn chunkColumn, BlockStruct blockStruct)
        {
            lock (ChunksLoaded)
            {
                if (ChunksLoaded.ContainsKey(chunkColumn.Position))
                {
                    ChunksLoaded[chunkColumn.Position].ChangeBlockType(blockStruct);
                    lock (ChunksWaitingToBake)
                    {
                        if (!ChunksWaitingToUnloadDictionary.ContainsKey(chunkColumn.Position) && !ChunksWaitingToBake.Contains(chunkColumn))
                        {
                            ChunksWaitingToBake.Enqueue(chunkColumn);
                        }
                    }
                }
            }
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
                    Console.WriteLine("ChunkManaginThread fps:" + (float)Math.Round(1 / totalTimeBeforeUpdate));
                    totalTimeBeforeUpdate = 0;

                    ActionManager.InvokeActions(ActionManager.Thread.ChunkManager);

                    ChunksWaitingToGenerate.Clear();

                    ChunksWaitingToBakeDictionary = ChunksWaitingToBake.Distinct().ToDictionary(chunk => chunk.Position);
                    ChunksWaitingToUnloadDictionary = ChunksWaitingToUnload.ToDictionary(chunk => chunk.Position);

                    // Generate chunks that are close to the player
                    List<Vector2i> ChunkPositionsAroundPlayer = ChunkPositionAroundPlayer(Program.Minecraft.Player);
                    foreach (Vector2i position in ChunkPositionsAroundPlayer)
                    {
                        if (!ChunkManagerContainsChunk(position))
                        {
                            ChunksWaitingToGenerate.Enqueue(position, GetDistanceFromPlayer(position));
                        }
                    }

                    // Unload chunks that are far from the player
                    IEnumerable<KeyValuePair<Vector2i, ChunkColumn>> chunksToUnload = ChunksLoaded.Where(keyRemoverDictionaryByDistance);
                    foreach (KeyValuePair<Vector2i, ChunkColumn> chunkToUnload in chunksToUnload)
                    {
                        if (!ChunksWaitingToUnload.Contains(chunkToUnload.Value))
                        {
                            ChunksWaitingToUnload.Enqueue(chunkToUnload.Value);
                        }
                    }

                    switch (chunkLoadingSteps)
                    {
                        case ChunkLoadingSteps.Generate:

                            if (ChunksWaitingToGenerate.Count != 0)
                            {
                                ChunkColumn chunkGenerated = new ChunkColumn(ChunksWaitingToGenerate.Dequeue());

                                ChunksWaitingToBake.Enqueue(chunkGenerated);

                                lock (ChunksLoaded)
                                {
                                    if (!ChunksLoaded.ContainsKey(chunkGenerated.Position))
                                        ChunksLoaded.Add(chunkGenerated.Position, chunkGenerated);
                                }

                                foreach (ChunkColumn neighbor in FindNeighbors(chunkGenerated))
                                {
                                    if (!ChunksWaitingToUnloadDictionary.ContainsKey(neighbor.Position))
                                    {
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                    }
                                }
                            }
                            break;

                        case ChunkLoadingSteps.Bake:

                            if (ChunksWaitingToBake.Count != 0)
                            {
                                ChunkColumn chunkBaked;
                                do
                                {
                                    chunkBaked = ChunksWaitingToBake.Dequeue();
                                    chunkBaked.IsBaking = true;
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
                                ChunkColumn chunkUnloaded = ChunksWaitingToUnload.Dequeue();

                                chunkUnloaded.Unload();

                                lock (ChunksLoaded)
                                    ChunksLoaded.Remove(chunkUnloaded.Position);

                                foreach (ChunkColumn neighbor in FindNeighbors(chunkUnloaded))
                                {
                                    if (!ChunksWaitingToUnloadDictionary.ContainsKey(neighbor.Position))
                                        ChunksWaitingToBake.Enqueue(neighbor);
                                }
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
            Vector2 PlayerPositionInChunk = player.Position.Xz / ChunkColumn.ChunkSize;
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
            return Vector2.Distance(Program.Minecraft.Player.Position.Xz / ChunkColumn.ChunkSize, chunkPosition);
        }

        private static bool ChunkManagerContainsChunk(Vector2i position)
        {
            if (ChunksLoaded.ContainsKey(position))
                return true;
            else if (ChunksWaitingToBakeDictionary.ContainsKey(position))
                return true;
            else if (ChunksWaitingToUnloadDictionary.ContainsKey(position))
                return true;
            else
                return false;
        }

        private static void LoadSpawnChunks()
        {
            for (int i = 0; i < SpawnChunkSize; i++)
            {
                for (int j = 0; j < SpawnChunkSize; j++)
                {
                    Vector2i position = new Vector2i(i - (SpawnChunkSize / 2), j - (SpawnChunkSize / 2));
                    ChunksLoaded.Add(position, new ChunkColumn(position));
                }
            }

            BakeAllChunks();
        }

        private static void BakeChunk(ChunkColumn chunk)
        {
            chunk.Bake(FindNeighborsBaked(chunk));
        }

        private static void BakeAllChunks()
        {
            List<ChunkColumn?> neighbors = new List<ChunkColumn?>();
            for (int i = 0; i < ChunksLoaded.Count; i++)
            {
                neighbors = FindNeighbors(ChunksLoaded.Values.ToList()[i]);

                ChunksLoaded.Values.ToList()[i].Bake(neighbors);
            }
        }

        private static List<ChunkColumn> FindNeighbors(ChunkColumn chunk)
        {
            List<ChunkColumn> chunksList = new List<ChunkColumn>();
            ChunkColumn chunkBuffer;

            if (ChunksLoaded.TryGetValue(new Vector2i(-1, 0) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(1, 0) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(0, -1) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);
            if (ChunksLoaded.TryGetValue(new Vector2i(0, 1) + chunk.Position, out chunkBuffer))
                chunksList.Add(chunkBuffer);

            return chunksList;
        }

        private static List<ChunkColumn> FindNeighborsBaked(ChunkColumn chunk)
        {
            List<ChunkColumn> chunksList = new List<ChunkColumn>();
            ChunkColumn chunkBuffer;

            if (ChunksLoaded.TryGetValue(new Vector2i(-1, 0) + chunk.Position, out chunkBuffer))
            {
                if (chunkBuffer.Mesh != null || chunkBuffer.IsBaking)
                    chunksList.Add(chunkBuffer);
            }
            if (ChunksLoaded.TryGetValue(new Vector2i(1, 0) + chunk.Position, out chunkBuffer))
            {
                if (chunkBuffer.Mesh != null || chunkBuffer.IsBaking)
                    chunksList.Add(chunkBuffer);
            }
            if (ChunksLoaded.TryGetValue(new Vector2i(0, -1) + chunk.Position, out chunkBuffer))
            {
                if (chunkBuffer.Mesh != null || chunkBuffer.IsBaking)
                    chunksList.Add(chunkBuffer);
            }
            if (ChunksLoaded.TryGetValue(new Vector2i(0, 1) + chunk.Position, out chunkBuffer))
            {
                if (chunkBuffer.Mesh != null || chunkBuffer.IsBaking)
                    chunksList.Add(chunkBuffer);
            }

            return chunksList;
        }

        private static List<ChunkColumn> FindNeighbors(ChunkColumn chunk, Dictionary<Vector2i, ChunkColumn> chunks)
        {
            List<ChunkColumn> chunksList = new List<ChunkColumn>();
            ChunkColumn? chunkBuffer = null;
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
