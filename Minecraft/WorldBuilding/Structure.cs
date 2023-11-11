using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        internal static void Init()
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

        internal static void AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i position)
        {
            List<BlockStruct> structureBlocks = GetBlocksByStructure(structureType);
            foreach (BlockStruct block in structureBlocks)
            {
                blocks[block.Position + position] = block.Type;
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
