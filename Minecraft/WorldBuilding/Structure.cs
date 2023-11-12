using Minecraft.System;
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

        private static List<(StructureType, List<BlockStruct>)> StructureIndex = new List<(StructureType, List<BlockStruct>)>();

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
            StructureIndex.Add((StructureType.OakTree, OakTreeBlocks));
            StructureIndex.Add((StructureType.Cactus, CactusBlocks));
        }

        internal static List<BlockStruct> GetBlocksByStructure(StructureType structureType)
        {
            foreach ((StructureType, List<BlockStruct>) tuple in StructureIndex)
            {
                if (tuple.Item1 == structureType)
                {
                    return tuple.Item2;
                }
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
                    // ChunkColumn x+
                    isBlockXPositive = blockPosition.X > ChunkColumn.ChunkSize - 1;
                    isBlockXNegative = blockPosition.X < 0;

                    isBlockZPositive = blockPosition.Z > ChunkColumn.ChunkSize - 1;
                    isBlockZNegative = blockPosition.Z < 0;

                    chunkColumnPositionNeighbor.X = isBlockXPositive ? 1 : 0;
                    chunkColumnPositionNeighbor.X = isBlockXNegative ? -1 : 0;

                    chunkColumnPositionNeighbor.Y = isBlockZPositive ? 1 : 0;
                    chunkColumnPositionNeighbor.Y = isBlockZNegative ? -1 : 0;

                    
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
    }
}
