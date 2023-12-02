﻿using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal enum StructureType
    {
        OakTree,
        Cactus,
    }

    internal static class Structure
    {
        private static List<BlockStruct> OakTreeBlocks = new List<BlockStruct>();
        private static List<BlockStruct> CactusBlocks = new List<BlockStruct>();

        private static Dictionary<StructureType, List<BlockStruct>> StructureIndex = new Dictionary<StructureType, List<BlockStruct>>();

        private static Dictionary<Vector2i, List<BlockStruct>> ChunkColumnGhostBlocks = new Dictionary<Vector2i, List<BlockStruct>>();
        private static Dictionary<Vector2i, List<Vector2i>> StructuresGenerated = new Dictionary<Vector2i, List<Vector2i>>();

        static Structure()
        {
            InitStructureBlocks();
            StructureIndex.Add(StructureType.OakTree, OakTreeBlocks);
            StructureIndex.Add(StructureType.Cactus, CactusBlocks);
        }

        internal static List<BlockStruct> GetBlocksByStructure(StructureType structureType)
        {
            if (StructureIndex.TryGetValue(structureType, out List<BlockStruct>? blocks))
            {
                return blocks;
            }
            return new List<BlockStruct>();
        }

        internal static void AddVegetation(NoiseMap vegetation, ref Dictionary<Vector3i, BlockType> blocks, int[,] height, int waterLevel, Vector2i chunkColumnPosition)
        {
            int xChunk = chunkColumnPosition.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunkColumnPosition.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            float[,] vegetationData = vegetation.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);
            int spacingBetweenStructures = 6;

            List<(Vector2i, float)> vegetationDataList = new List<(Vector2i, float)>();
            for (int i = 0; i < ChunkColumn.ChunkSize; i++)
            {
                for (int j = 0; j < ChunkColumn.ChunkSize; j++)
                {
                    if (height[i, j] > waterLevel)
                        vegetationDataList.Add((new Vector2i(i, j), vegetationData[i, j]));
                }
            }

            int numStructures = (int)(vegetationData.Average() * ((ChunkColumn.ChunkSize * ChunkColumn.ChunkSize) / (5f * 5f)) / 2f);

            // Remove spaces that are ocupied by structures in outher chunks
            List<Vector2i>? positions = new List<Vector2i>();
            List<Vector2i> allStructurePositions = new List<Vector2i>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        // Get all structures that are generated near this chunk -> allStructPositions
                        if (StructuresGenerated.TryGetValue(chunkColumnPosition + new Vector2i(i, j), out positions))
                        {
                            positions.ForEach((item) =>
                            {
                                item += new Vector2i(i * ChunkColumn.ChunkSize, j * ChunkColumn.ChunkSize);
                                allStructurePositions.Add(item);
                                vegetationDataList.Add(((item.X, item.Y), -1));
                            });
                        }
                    }
                }
            }

            // Remove blocks that are near structures that are generated
            vegetationDataList = vegetationDataList.Remove((value) =>
            {
                for (int j = 0; j < allStructurePositions.Count; j++)
                {
                    if (Math.Abs(value.Item1.X - allStructurePositions[j].X) < spacingBetweenStructures 
                    && Math.Abs(value.Item1.Y - allStructurePositions[j].Y) < spacingBetweenStructures)
                        return true;
                }
                return false;
            });
            // Remove items that are not in the chunk 
            vegetationDataList = vegetationDataList.Remove((value) =>
            {
                if (value.Item2 == -1)
                    return true; return false;
            });

            // Generate structures
            List<Vector2i> validStructurePositions = new List<Vector2i>();
            for (int i = 0; i < numStructures; i++)
            {
                if (vegetationDataList.Count > 0)
                {
                    // Sort the vegetationDataList with the vegetation value going from largest to smallest
                    vegetationDataList.Sort((item1, item2) =>
                    {
                        if (item1.Item2 < item2.Item2)
                            return -1;
                        if (item1.Item2 == item2.Item2)
                            return 0;
                        return 1;
                    });

                    Vector2i index = vegetationDataList[0].Item1;

                    Vector3i position = new Vector3i(index.X, height[index.X, index.Y] + 1, index.Y);
                    AddStructure(ref blocks, StructureType.OakTree, position, chunkColumnPosition);
                    validStructurePositions.Add(new Vector2i(index.X, index.Y));


                    vegetationDataList = vegetationDataList.Remove((value) =>
                    {
                        if (Math.Abs(value.Item1.X - index.X) < spacingBetweenStructures && Math.Abs(value.Item1.Y - index.Y) < spacingBetweenStructures)
                            return true; return false;
                    });
                }
            }
            if (validStructurePositions.Count > 0)
            {
                if (StructuresGenerated.TryGetValue(chunkColumnPosition, out List<Vector2i>? value))
                {
                    value.AddRange(validStructurePositions);
                    StructuresGenerated[chunkColumnPosition] = value;
                }
                else
                    StructuresGenerated.Add(chunkColumnPosition, validStructurePositions);
            }
        }

        internal static void UnloadChunk()
        {
            List<KeyValuePair<Vector2i, List<BlockStruct>>> chunkColumnGhostBlocksList = ChunkColumnGhostBlocks.ToList();
            List<int> indicies = new List<int>();
            for (int i = 0; i < chunkColumnGhostBlocksList.Count; i++)
            {
                bool removeChunk = true;
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        lock (ChunkManager.ChunksLoaded)
                        {
                            if (ChunkManager.ChunksLoaded.ContainsKey(new Vector2i(chunkColumnGhostBlocksList[i].Key.X + x, chunkColumnGhostBlocksList[i].Key.Y + y)))
                                removeChunk = false;
                        }
                    }
                }
                if (removeChunk)
                    indicies.Add(i);
            }
            indicies.Sort();
            for (int i = indicies.Count - 1; i >= 0; i--)
            {
                chunkColumnGhostBlocksList.RemoveAt(indicies[i]);
            }
            ChunkColumnGhostBlocks = chunkColumnGhostBlocksList.ToDictionary(value => value.Key).RemoveDoubleKeys();
            indicies.Clear();

            List<KeyValuePair<Vector2i, List<Vector2i>>> structuresGeneratedList = StructuresGenerated.ToList();
            for (int i = 0; i < structuresGeneratedList.Count; i++)
            {
                bool removeChunk = true;
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        lock (ChunkManager.ChunksLoaded)
                        {
                            if (ChunkManager.ChunksLoaded.ContainsKey(new Vector2i(structuresGeneratedList[i].Key.X + x, structuresGeneratedList[i].Key.Y + y)))
                                removeChunk = false;
                        }
                    }
                }
                if (removeChunk)
                    indicies.Add(i);
            }
            indicies.Sort();
            for (int i = indicies.Count - 1; i >= 0; i--)
            {
                structuresGeneratedList.RemoveAt(indicies[i]);
            }
            StructuresGenerated = structuresGeneratedList.ToDictionary(value => value.Key).RemoveDoubleKeys();
        }

        internal static void AddGhostBlocks(ref Dictionary<Vector3i, BlockType> blocks, Vector2i position)
        {
            if (ChunkColumnGhostBlocks.TryGetValue(position, out List<BlockStruct>? ghostBlocks))
            {
                foreach (BlockStruct block in ghostBlocks)
                {
                    if (block.Type != BlockType.Air)
                        blocks[block.Position] = block.Type;
                }
            }
        }

        internal static List<BlockStruct> TranslateList(List<BlockStruct> list, Vector3i vector)
        {
            List<BlockStruct> values = new List<BlockStruct>();
            for (int i = 0; i < list.Count; i++)
            {
                values.Add(new BlockStruct { Position = list[i].Position + vector, Type = list[i].Type });
            }
            return values;
        }

        private static void AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i strucuturePosition, Vector2i chunkColumnPosition)
        {
            List<BlockStruct> structureBlocks = GetBlocksByStructure(structureType);
            bool isBlockXPositive, isBlockXNegative, isBlockZPositive, isBlockZNegative;

            foreach (BlockStruct block in structureBlocks)
            {
                Vector3i blockPosition = block.Position + strucuturePosition;
                if (!ChunkColumn.IsOutOfRange(blockPosition))
                {
                    if (blocks[blockPosition] == BlockType.Air)
                        blocks[blockPosition] = block.Type;
                }
                else
                {
                    Vector2i chunkColumnPositionNeighbor = new Vector2i();
                    Vector3i blockPositionNeigbor = blockPosition;

                    isBlockXPositive = blockPosition.X > ChunkColumn.ChunkSize - 1;
                    isBlockXNegative = blockPosition.X < 0;

                    isBlockZPositive = blockPosition.Z > ChunkColumn.ChunkSize - 1;
                    isBlockZNegative = blockPosition.Z < 0;

                    if (isBlockXPositive)
                        chunkColumnPositionNeighbor.X = 1;
                    else if (isBlockXNegative)
                        chunkColumnPositionNeighbor.X = -1;

                    if (isBlockZPositive)
                        chunkColumnPositionNeighbor.Y = 1;
                    else if (isBlockZNegative)
                        chunkColumnPositionNeighbor.Y = -1;

                    blockPositionNeigbor.X -= chunkColumnPositionNeighbor.X * ChunkColumn.ChunkSize;
                    blockPositionNeigbor.Z -= chunkColumnPositionNeighbor.Y * ChunkColumn.ChunkSize;

                    ChunkColumn? chunkColumnNeigbor = ChunkManager.GetChunkColumn(chunkColumnPositionNeighbor + chunkColumnPosition);
                    if (chunkColumnNeigbor != null)
                    {
                        if (chunkColumnNeigbor.GetBlockType(blockPositionNeigbor) == BlockType.Air)
                            ChunkManager.ChangeBlock(chunkColumnNeigbor.Position, new BlockStruct { Position = blockPositionNeigbor, Type = block.Type }, false);
                    }
                    else
                    {
                        if (!ChunkColumnGhostBlocks.ContainsKey(chunkColumnPositionNeighbor + chunkColumnPosition))
                        {
                            ChunkColumnGhostBlocks.Add(chunkColumnPositionNeighbor + chunkColumnPosition, new List<BlockStruct> { new BlockStruct { Position = blockPositionNeigbor, Type = block.Type } });
                        }
                        else
                        {
                            ChunkColumnGhostBlocks[chunkColumnPositionNeighbor + chunkColumnPosition].Add(new BlockStruct { Position = blockPositionNeigbor, Type = block.Type });
                        }
                    }
                }
            }
        }

        private static void InitStructureBlocks()
        {
            // Oak tree
            {
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 0, 0), Type = BlockType.OakLog });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 1, 0), Type = BlockType.OakLog });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 2, 0), Type = BlockType.OakLog });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 3, 0), Type = BlockType.OakLog });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, 0), Type = BlockType.OakLog });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 5, 0), Type = BlockType.OakLog });

                {
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 4, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 4, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, -1), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 4, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 4, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 4, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 4, -1), Type = BlockType.OakLeaves });
                }
                {
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 4, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 4, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 4, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 4, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 4, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 4, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 4, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 4, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 4, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 4, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 4, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 4, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 4, -2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 4, -2), Type = BlockType.OakLeaves });
                }

                {
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 5, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 5, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 5, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 5, -1), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 5, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 5, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 5, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 5, -1), Type = BlockType.OakLeaves });
                }
                {
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 5, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 5, 0), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 5, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 5, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 5, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 5, 1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 5, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 5, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 5, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 5, -1), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 5, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 5, -2), Type = BlockType.OakLeaves });

                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 5, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 5, 2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(2, 5, -2), Type = BlockType.OakLeaves });
                    OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-2, 5, -2), Type = BlockType.OakLeaves });
                }

                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 6, 0), Type = BlockType.OakLeaves });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(1, 6, 0), Type = BlockType.OakLeaves });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(-1, 6, 0), Type = BlockType.OakLeaves });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 6, 1), Type = BlockType.OakLeaves });
                OakTreeBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 6, -1), Type = BlockType.OakLeaves });
            }

            CactusBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 0, 0), Type = BlockType.Cactus });
            CactusBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 1, 0), Type = BlockType.Cactus });
            CactusBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 2, 0), Type = BlockType.Cactus });
            CactusBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 3, 0), Type = BlockType.Cactus });
            CactusBlocks.Add(new BlockStruct() { Position = new Vector3i(0, 4, 0), Type = BlockType.Cactus });
        }
    }
}
