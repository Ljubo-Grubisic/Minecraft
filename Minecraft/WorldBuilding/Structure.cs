using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Management;

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

        internal static void AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i position, Vector2i chunkColumnPosition)
        {
            List<BlockStruct> structureBlocks = GetBlocksByStructure(structureType);
            bool isBlockXPositive, isBlockXNegative, isBlockZPositive, isBlockZNegative;

            foreach (BlockStruct block in structureBlocks)
            {
                Vector3i blockPosition = block.Position + position;
                if (!ChunkColumn.IsOutOfRange(blockPosition))
                {
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
                        ChunkManager.ChangeBlock(chunkColumnNeigbor, new BlockStruct { Position = blockPositionNeigbor, Type = block.Type });
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

        internal static void AddGhostBlocks(ref Dictionary<Vector3i, BlockType> blocks, Vector2i position)
        {
            if (ChunkColumnGhostBlocks.TryGetValue(position, out List<BlockStruct>? ghostBlocks))
            {
                foreach (BlockStruct block in ghostBlocks)
                {
                    blocks[block.Position] = block.Type;
                }
                //ChunkColumnGhostBlocks.Remove(position);
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
    }
}
