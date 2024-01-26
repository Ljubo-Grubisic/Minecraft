using Minecraft.Entitys;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Minecraft.WorldBuilding
{
    internal enum StructureType
    {
        None = -1,

        OakTree,
        BirchTree,
        SpruceTree,
        JungleTree,
        AcaciaTree,
        Cactus,

        SandCastle,
        Iglu,
        SunkenShip,
        ViligerHut,
        Iceberg,

        Count
    }

    internal static class Structure
    {
        public static bool SaveBulding = false;
        private static Dictionary<StructureType, List<StructureSave>> StructureSaves = new Dictionary<StructureType, List<StructureSave>>();

        private static Dictionary<Vector2i, List<(BlockStruct, bool)>> ChunkColumnGhostBlocks = new Dictionary<Vector2i, List<(BlockStruct, bool)>>();
        private static Dictionary<Vector2i, List<(Vector2i Position, StructureType Structure, int StructureIndex)>> StructuresGenerated = new Dictionary<Vector2i, List<(Vector2i Position, StructureType Structure, int StructureIndex)>>();

        static Structure()
        {
            InitStructureBlocksBinary();
            Player.PlayerChangeBlock += Player_PlayerChangeBlock;
        }

        internal static void AddVegetation(NoiseMap vegetation, ref Dictionary<Vector3i, BlockType> blocks, Vector2i chunkColumnPosition,
            int[,] height, BiomeType[,] biome, int waterLevel)
        {
            int xChunk = chunkColumnPosition.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunkColumnPosition.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);

            int numStructures = vegetation.ConvertMapedValueToIntScale(vegetation.GetMapedNoiseValue(xChunk, yChunk), 0, 4);
            float[,] randomData = WorldGenerator.Random.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);

            List<(Vector2i Position, float Value)> vegetationData = new List<(Vector2i, float)>();
            for (int i = 0; i < ChunkColumn.ChunkSize; i++)
            {
                for (int j = 0; j < ChunkColumn.ChunkSize; j++)
                {
                    if (height[i, j] > waterLevel)
                        vegetationData.Add((new Vector2i(i, j), randomData[i, j]));
                }
            }

            // Remove spaces that are ocupied by structures in outher chunks
            List<(Vector2i Position, StructureType Structure, int StructureIndex)>? positions = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
            List<(Vector2i Position, StructureType Structure, int StructureIndex)> allStructurePositions = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        // Get all structures that are generated near this chunk -> allStructPositions
                        if (StructuresGenerated.TryGetValue(chunkColumnPosition + new Vector2i(i, j), out positions))
                        {
                            positions.ForEach((value) =>
                            {
                                value.Position += new Vector2i(i * ChunkColumn.ChunkSize, j * ChunkColumn.ChunkSize);
                                allStructurePositions.Add(value);
                                vegetationData.Add(((value.Position.X, value.Position.Y), -1));
                            });
                        }
                    }
                }
            }

            // Remove blocks that are near structures that are generated
            RemoveNotSpawnableBlocksFast(ref vegetationData, allStructurePositions, height, biome);

            // Remove items that are not in the chunk 
            vegetationData = vegetationData.Remove((value) =>
            {
                if (value.Value == -1)
                    return true; return false;
            });

            // Generate structures
            List<(Vector2i Position, StructureType Structure, int StructureIndex)> validStructures = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
            for (int i = 0; i < numStructures; i++)
            {
                if (vegetationData.Count > 0)
                {
                    // Sort the vegetationDataList with the vegetation value going from largest to smallest
                    vegetationData.Sort((item1, item2) =>
                    {
                        if (item1.Value < item2.Value)
                            return -1;
                        if (item1.Value == item2.Value)
                            return 0;
                        return 1;
                    });

                    Vector2i index = vegetationData[0].Position;
                    StructureType structureType = Biome.GetBiomeConfigStructureByPosition(biome[vegetationData[0].Position.X, vegetationData[0].Position.Y], vegetationData[0].Position);

                    if (structureType != StructureType.None)
                    {
                        Vector3i position = new Vector3i(index.X, height[index.X, index.Y] + 1, index.Y);
                        int structureIndex = AddStructure(ref blocks, structureType, position, chunkColumnPosition);
                        validStructures.Add((new Vector2i(index.X, index.Y), structureType, structureIndex));

                        RemoveNotSpawnableBlocksFast(ref vegetationData, allStructurePositions, height, biome);
                    }
                }
            }
            if (validStructures.Count > 0)
            {
                if (StructuresGenerated.TryGetValue(chunkColumnPosition, out List<(Vector2i Position, StructureType Structure, int StructureIndex)>? value))
                {
                    value.AddRange(validStructures);
                    StructuresGenerated[chunkColumnPosition] = value;
                }
                else
                    StructuresGenerated.Add(chunkColumnPosition, validStructures);
            }
        }

        internal static void AddRareStructure(ref Dictionary<Vector3i, BlockType> blocks, Vector2i chunkColumnPosition, int[,] height, BiomeType[,] biome, int waterLevel, int chanceOutOf100)
        {
            int randomNumber = WorldGenerator.Random.ConvertMapedValueToIntScale(WorldGenerator.Random.GetMapedNoiseValue(chunkColumnPosition.X, chunkColumnPosition.Y), -1, 101);

            if (randomNumber < chanceOutOf100)
            {
                int xChunk = chunkColumnPosition.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
                int yChunk = chunkColumnPosition.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);

                float[,] randomData = WorldGenerator.Random.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);

                List<(Vector2i Position, float Value)> structureData = new List<(Vector2i, float)>();
                for (int i = 0; i < ChunkColumn.ChunkSize; i++)
                {
                    for (int j = 0; j < ChunkColumn.ChunkSize; j++)
                    {
                        structureData.Add((new Vector2i(i, j), randomData[i, j]));
                    }
                }

                // Remove spaces that are ocupied by structures in outher chunks
                List<(Vector2i Position, StructureType Structure, int StructureIndex)>? positions = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
                List<(Vector2i Position, StructureType Structure, int StructureIndex)> allStructurePositions = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // Get all structures that are generated near this chunk -> allStructPositions
                        if (StructuresGenerated.TryGetValue(chunkColumnPosition + new Vector2i(i, j), out positions))
                        {
                            positions.ForEach((value) =>
                            {
                                value.Position += new Vector2i(i * ChunkColumn.ChunkSize, j * ChunkColumn.ChunkSize);
                                allStructurePositions.Add(value);
                                structureData.Add(((value.Position.X, value.Position.Y), -1));
                            });
                        }
                    }
                }

                // Remove blocks that are near structures that are generated
                RemoveNotSpawnableBlocksRareFast(ref structureData, allStructurePositions, height, biome);

                // Remove items that are not in the chunk 
                structureData = structureData.Remove((value) =>
                {
                    if (value.Value == -1)
                        return true; return false;
                });

                (Vector2i Position, StructureType Structure, int StructureIndex)? validStructures = null;
                if (structureData.Count > 0)
                {
                    // Sort the vegetationDataList with the vegetation value going from largest to smallest
                    structureData.Sort((item1, item2) =>
                    {
                        if (item1.Value < item2.Value)
                            return -1;
                        if (item1.Value == item2.Value)
                            return 0;
                        return 1;
                    });

                    Vector2i index = structureData[0].Position;
                    StructureType structureType = Biome.GetBiomeConfig(biome[structureData[0].Position.X, structureData[0].Position.Y]).RareStructure;

                    if (structureType != StructureType.None)
                    {
                        Vector3i position = new Vector3i(index.X, height[index.X, index.Y] + 1, index.Y);
                        int structureIndex = AddStructure(ref blocks, structureType, position, chunkColumnPosition, true);
                        validStructures = (new Vector2i(index.X, index.Y), structureType, structureIndex);

                        RemoveNotSpawnableBlocksRareFast(ref structureData, allStructurePositions, height, biome);
                    }
                }

                if (validStructures != null)
                {
                    if (StructuresGenerated.TryGetValue(chunkColumnPosition, out List<(Vector2i Position, StructureType Structure, int StructureIndex)>? value))
                    {
                        value.Add(((Vector2i Position, StructureType Structure, int StructureIndex))validStructures);
                        StructuresGenerated[chunkColumnPosition] = value;
                    }
                    else
                        StructuresGenerated.Add(chunkColumnPosition, new List<(Vector2i Position, StructureType Structure, int StructureIndex)> { ((Vector2i Position, StructureType Structure, int StructureIndex))validStructures });
                }
            }
        }

        internal static void UnloadChunk()
        {
            List<KeyValuePair<Vector2i, List<(BlockStruct, bool)>>> chunkColumnGhostBlocksList = ChunkColumnGhostBlocks.ToList();
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

            List<KeyValuePair<Vector2i, List<(Vector2i Position, StructureType Structure, int StructureIndex)>>> structuresGeneratedList = StructuresGenerated.ToList();
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
            if (ChunkColumnGhostBlocks.TryGetValue(position, out List<(BlockStruct, bool)>? ghostBlocks))
            {
                foreach ((BlockStruct Block, bool Overwrite) block in ghostBlocks)
                {
                    if (block.Overwrite)
                    {
                        blocks[block.Block.Position] = block.Block.Type;
                    }
                    else
                    {
                        if (block.Block.Type != BlockType.Air)
                            blocks[block.Block.Position] = block.Block.Type;
                    }
                }
            }
        }

        #region StructureSave getters
        private static int GetNumListsStructType(StructureType structureType)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? blocks))
            {
                return blocks.Count;
            }
            throw new Exception("Structure not loaded");
        }

        private static List<BlockStruct> GetBlocksByStructure(StructureType structureType, int index)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? structureSave))
            {
                return structureSave[index].Blocks;
            }
            throw new Exception("Structure not loaded");
        }

        private static List<Vector2i> GetAreaByStructure(StructureType structureType, int index)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? structureSave))
            {
                return structureSave[index].Area;
            }
            throw new Exception("Structure not loaded");
        }

        private static int GetSizeByStructure(StructureType structureType, int index)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? structureSave))
            {
                return structureSave[index].Size;
            }
            throw new Exception("Structure not loaded");
        }
        #endregion

        #region General Getters and List Manipulation
        private static List<BlockStruct> TranslateList(List<BlockStruct> list, Vector3i vector)
        {
            List<BlockStruct> values = new List<BlockStruct>();
            for (int i = 0; i < list.Count; i++)
            {
                values.Add(new BlockStruct { Position = list[i].Position + vector, Type = list[i].Type });
            }
            return values;
        }

        private static int GetIndexForStructure(StructureType structureType, Vector3i structurePosition)
        {
            int numLists = GetNumListsStructType(structureType);
            if (numLists == -1)
                throw new Exception("Invalid structure type, this structure type has no block lists or isnt loaded");

            int index = WorldGenerator.Random.ConvertMapedValueToIntScale(WorldGenerator.Random.GetMapedNoiseValue(structurePosition.X, structurePosition.Y), -1, numLists);
            if (index == numLists)
                index = numLists - 1;
            if (index == -1)
                index = 0;

            return index;
        }

        private static List<BlockStruct> RotateBlocksAlgorithm(List<BlockStruct> blocks, Vector3i structurePosition)
        {
            int rotationIndex = WorldGenerator.Random.ConvertMapedValueToIntScale(WorldGenerator.Random.GetMapedNoiseValue(structurePosition.X, structurePosition.Y), -1, 5);

            if (rotationIndex == 5)
                rotationIndex = 4;
            if (rotationIndex == -1)
                rotationIndex = 0;

            List<BlockStruct> rotatedBlocks = RotateBlocks(blocks, rotationIndex * 90);

            return rotatedBlocks;
        }

        private static List<BlockStruct> RotateBlocks(List<BlockStruct> blocks, int rotationAngle)
        {
            List<BlockStruct> rotatedBlocks = new List<BlockStruct>();

            foreach (var block in blocks)
            {
                // Rotate the block's position based on the specified angle
                Vector3i rotatedPosition = RotateVector(block.Position, rotationAngle);

                // Create a new rotated block with the same type
                BlockStruct rotatedBlock = new BlockStruct
                {
                    Position = rotatedPosition,
                    Type = block.Type
                };

                rotatedBlocks.Add(rotatedBlock);
            }

            return rotatedBlocks;
        }

        private static Vector3i RotateVector(Vector3i vector, int rotationAngle)
        {
            switch (rotationAngle)
            {
                case 90:
                    // Rotate 90 degrees clockwise
                    return new Vector3i(vector.Z, vector.Y, -vector.X);
                case 180:
                    // Rotate 180 degrees
                    return new Vector3i(-vector.X, vector.Y, -vector.Z);
                case 270:
                    // Rotate 90 degrees counterclockwise
                    return new Vector3i(-vector.Z, vector.Y, vector.X);
                default:
                    // No rotation
                    return vector;
            }
        }
        #endregion

        #region Large list manipulation
        private static void RemoveNotSpawnableBlocks(ref List<(Vector2i, float)> listOfValues, List<(Vector2i Position, StructureType Structure, int StructureIndex)> allStructurePositions, int[,] height, BiomeType[,] biome)
        {
            List<(Vector2i, float)> vegetationDataToRemove = new List<(Vector2i, float)>();
            foreach ((Vector2i Position, float Value) vegetationValue in listOfValues)
            {
                bool hasBlockBeenRemoved = false;
                for (int i = 0; i < allStructurePositions.Count; i++)
                {
                    List<Vector2i> globalStructureArea = GetAreaByStructure(allStructurePositions[i].Structure, allStructurePositions[i].StructureIndex);

                    foreach (Vector2i globalBlock in globalStructureArea)
                    {
                        if (vegetationValue.Value != -1)
                        {
                            StructureType type = Biome.GetBiomeConfig(biome[vegetationValue.Position.X, vegetationValue.Position.Y]).PrimaryVegetationStructure;
                            int index = GetIndexForStructure(type, new Vector3i(vegetationValue.Position.X, height[vegetationValue.Position.X, vegetationValue.Position.Y] + 1, vegetationValue.Position.Y));
                            List<Vector2i> localStructureArea = localStructureArea = GetAreaByStructure(type, index);

                            for (int j = 0; j < localStructureArea.Count; j++)
                            {
                                Vector2i localBlock = localStructureArea[j];
                                if (globalBlock + allStructurePositions[i].Position == localBlock + vegetationValue.Position)
                                {
                                    vegetationDataToRemove.Add(vegetationValue);
                                    hasBlockBeenRemoved = true;
                                    break;
                                }
                            }
                        }
                        if (hasBlockBeenRemoved)
                            break;
                    }
                    if (hasBlockBeenRemoved)
                        break;
                }
            }
            for (int i = 0; i < vegetationDataToRemove.Count; i++)
            {
                listOfValues.Remove(vegetationDataToRemove[i]);
            }
        }

        private static void RemoveNotSpawnableBlocksFast(ref List<(Vector2i, float)> listOfValues, List<(Vector2i Position, StructureType Structure, int StructureIndex)> allStructurePositions, int[,] height, BiomeType[,] biome)
        {
            listOfValues = listOfValues.Remove(((Vector2i Position, float Value) value) =>
            {
                for (int j = 0; j < allStructurePositions.Count; j++)
                {
                    if (value.Value != -1)
                    {
                        int spacing = GetSizeByStructure(allStructurePositions[j].Structure, allStructurePositions[j].StructureIndex);
                        StructureType type = Biome.GetBiomeConfigStructureByPosition(biome[value.Position.X, value.Position.Y], value.Position);

                        if (type != StructureType.None)
                        {
                            int index = GetIndexForStructure(type, new Vector3i(value.Position.X, height[value.Position.X, value.Position.Y] + 1, value.Position.Y));
                            spacing += GetSizeByStructure(type, index);

                            if (Math.Abs(value.Position.X - allStructurePositions[j].Position.X) < spacing
                            && Math.Abs(value.Position.Y - allStructurePositions[j].Position.Y) < spacing)
                                return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            });
        }

        private static void RemoveNotSpawnableBlocksRareFast(ref List<(Vector2i, float)> listOfValues, List<(Vector2i Position, StructureType Structure, int StructureIndex)> allStructurePositions, int[,] height, BiomeType[,] biome)
        {
            listOfValues = listOfValues.Remove(((Vector2i Position, float Value) value) =>
            {
                for (int j = 0; j < allStructurePositions.Count; j++)
                {
                    if (value.Value != -1)
                    {
                        int spacing = GetSizeByStructure(allStructurePositions[j].Structure, allStructurePositions[j].StructureIndex);
                        StructureType type = Biome.GetBiomeConfig(biome[value.Position.X, value.Position.Y]).RareStructure;

                        if (type != StructureType.None)
                        {
                            int index = GetIndexForStructure(type, new Vector3i(value.Position.X, height[value.Position.X, value.Position.Y] + 1, value.Position.Y));
                            spacing += GetSizeByStructure(type, index);

                            if (Math.Abs(value.Position.X - allStructurePositions[j].Position.X) < spacing
                            && Math.Abs(value.Position.Y - allStructurePositions[j].Position.Y) < spacing)
                                return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            });
        }

        private static int AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i strucuturePosition, Vector2i chunkColumnPosition, bool overwrite = false)
        {
            int index = GetIndexForStructure(structureType, strucuturePosition);

            List<BlockStruct> structureBlocks = GetBlocksByStructure(structureType, index);
            structureBlocks = RotateBlocksAlgorithm(structureBlocks, strucuturePosition);

            bool isBlockXPositive, isBlockXNegative, isBlockZPositive, isBlockZNegative;

            foreach (BlockStruct block in structureBlocks)
            {
                Vector3i blockPosition = block.Position + strucuturePosition;
                if (!ChunkColumn.IsOutOfRange(blockPosition))
                {
                    if (overwrite)
                    {
                        blocks[blockPosition] = block.Type;
                    }
                    else
                    {
                        if (blocks[blockPosition] == BlockType.Air)
                            blocks[blockPosition] = block.Type;
                    }
                }
                else
                {
                    Vector3i blockPositionNeigbor = blockPosition;
                    bool isOutOfRange = false;

                    Vector2i chunkColumnPositionNeighbor;

                    do
                    {
                        chunkColumnPositionNeighbor = new Vector2i();
                        isBlockXPositive = blockPositionNeigbor.X > ChunkColumn.ChunkSize - 1;
                        isBlockXNegative = blockPositionNeigbor.X < 0;

                        isBlockZPositive = blockPositionNeigbor.Z > ChunkColumn.ChunkSize - 1;
                        isBlockZNegative = blockPositionNeigbor.Z < 0;

                        if (isBlockXPositive)
                            chunkColumnPositionNeighbor.X += 1;
                        else if (isBlockXNegative)
                            chunkColumnPositionNeighbor.X += -1;

                        if (isBlockZPositive)
                            chunkColumnPositionNeighbor.Y += 1;
                        else if (isBlockZNegative)
                            chunkColumnPositionNeighbor.Y += -1;

                        blockPositionNeigbor.X -= chunkColumnPositionNeighbor.X * ChunkColumn.ChunkSize;
                        blockPositionNeigbor.Z -= chunkColumnPositionNeighbor.Y * ChunkColumn.ChunkSize;


                        isOutOfRange = blockPositionNeigbor.X > ChunkColumn.ChunkSize - 1 || blockPositionNeigbor.X < 0 ||
                            blockPositionNeigbor.Z > ChunkColumn.ChunkSize - 1 || blockPositionNeigbor.Z < 0;
                    } while (isOutOfRange);

                    ChunkColumn? chunkColumnNeigbor = ChunkManager.GetChunkColumn(chunkColumnPositionNeighbor + chunkColumnPosition);
                    if (chunkColumnNeigbor != null)
                    {
                        if (overwrite)
                        {
                            ChunkManager.ChangeBlock(chunkColumnNeigbor.Position, new BlockStruct { Position = blockPositionNeigbor, Type = block.Type }, false);
                        }
                        else
                        {
                            if (chunkColumnNeigbor.GetBlockType(blockPositionNeigbor) == BlockType.Air)
                                ChunkManager.ChangeBlock(chunkColumnNeigbor.Position, new BlockStruct { Position = blockPositionNeigbor, Type = block.Type }, false);
                        }
                    }
                    else
                    {
                        if (!ChunkColumnGhostBlocks.ContainsKey(chunkColumnPositionNeighbor + chunkColumnPosition))
                        {
                            ChunkColumnGhostBlocks.Add(chunkColumnPositionNeighbor + chunkColumnPosition,
                                new List<(BlockStruct, bool)> { (new BlockStruct { Position = blockPositionNeigbor, Type = block.Type }, overwrite) });
                        }
                        else
                        {
                            ChunkColumnGhostBlocks[chunkColumnPositionNeighbor + chunkColumnPosition].Add(
                                (new BlockStruct { Position = blockPositionNeigbor, Type = block.Type }, overwrite));
                        }
                    }
                }
            }
            return index;
        }
        #endregion

        #region Creating and Loading StructureSaves
        #region XML
        private static Vector3i firstBlockPosition;
        private static bool onStartup = true;
        private static void CreateEditStructureSaveXML(Player sender, PlayerChangeBlockEventArgs args)
        {
            if (onStartup)
            {
                firstBlockPosition = args.BlockPosition;
                onStartup = false;
            }

            if (!Directory.Exists(SaveManager.SaveDirectory))
            {
                Directory.CreateDirectory(SaveManager.SaveDirectory);
            }
            if (!File.Exists(SaveManager.SaveDirectory + "/structure.xml"))
            {
                File.Create(SaveManager.SaveDirectory + "/structure.xml").Dispose();
                firstBlockPosition = args.BlockPosition;
            }

            DataContractSerializer serializer = new DataContractSerializer(typeof(StructureSave));

            StructureSave? save = new StructureSave();

            Stream streamReader = File.OpenRead(SaveManager.SaveDirectory + "/structure.xml");

            if (streamReader.Length > 0)
                save = (StructureSave?)serializer.ReadObject(streamReader);

            streamReader.Dispose();

            if (save != null)
            {
                if (save.Blocks != null)
                {
                    save.Blocks.Add(new BlockStruct() { Position = args.BlockPosition - firstBlockPosition, Type = args.Type });
                }
                else
                {
                    List<BlockStruct> blocks = new List<BlockStruct>
                    {
                        new BlockStruct() { Position = args.BlockPosition - firstBlockPosition, Type = args.Type }
                    };
                    save.Blocks = blocks;
                }


                List<Vector3i> positions = new List<Vector3i>();
                List<int> indicies = new List<int>();
                for (int i = 0; i < save.Blocks.Count; i++)
                {
                    int index = positions.IndexOf(save.Blocks[i].Position);
                    if (index != -1)
                        indicies.Add(index);

                    if (save.Blocks[i].Type == BlockType.Air)
                        indicies.Add(i);

                    positions.Add(save.Blocks[i].Position);
                }
                indicies.Sort();
                indicies = indicies.Distinct().ToList();
                for (int i = indicies.Count - 1; i >= 0; i--)
                {
                    save.Blocks.RemoveAt(indicies[i]);
                }

                File.Delete(SaveManager.SaveDirectory + "/structure.xml");

                Stream streamWriter = File.OpenWrite(SaveManager.SaveDirectory + "/structure.xml");

                serializer.WriteObject(streamWriter, save);

                streamWriter.Dispose();
            }
        }

        private static void InitStructureBlocksXML()
        {
            LoadStructureSavesXML(StructureType.AcaciaTree, "AcaciaTree", 3);
            LoadStructureSavesXML(StructureType.BirchTree, "BirchTree", 4);
            LoadStructureSavesXML(StructureType.Cactus, "Cactus", 4);
            LoadStructureSavesXML(StructureType.JungleTree, "JungleTree", 6);
            LoadStructureSavesXML(StructureType.OakTree, "OakTree", 5);
            LoadStructureSavesXML(StructureType.SpruceTree, "SpruceTree", 8);

            LoadStructureSavesXML(StructureType.SandCastle, "SandCastle", 2);
            LoadStructureSavesXML(StructureType.Iglu, "Iglu", 1);
            LoadStructureSavesXML(StructureType.SunkenShip, "SunkenShip", 1);
            LoadStructureSavesXML(StructureType.ViligerHut, "ViligerHut", 1);
        }

        private static void LoadStructureSavesXML(StructureType stuctureType, string structureName, int numStructure)
        {
            if (numStructure > 1)
            {
                List<StructureSave> structureSaves = new List<StructureSave>();
                for (int i = 1; i <= numStructure; i++)
                {
                    StructureSave? save = LoadStructureSaveXML(structureName + "." + structureName + i + ".xml");

                    if (save != null)
                    {
                        structureSaves.Add(save);
                    }
                    else
                    {
                        throw new Exception("Falied loading structures");
                    }
                }
                StructureSaves.Add(stuctureType, structureSaves);
            }
            else
            {
                List<StructureSave> structureSaves = new List<StructureSave>();
                StructureSave? save = LoadStructureSaveXML("Rare" + "." + structureName + ".xml");

                if (save != null)
                {
                    structureSaves.Add(save);
                }
                else
                {
                    throw new Exception("Falied loading structures");
                }

                StructureSaves.Add(stuctureType, structureSaves);
            }
        }

        private static StructureSave? LoadStructureSaveXML(string fileName)
        {
            bool isFileEmpty = false;

            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Minecraft.Resources.Structures." + fileName);

            if (stream != null)
            {
                StreamReader reader = new StreamReader(stream);

                if (reader.BaseStream.Length == 0)
                    isFileEmpty = true;

                reader.Dispose();

                if (!isFileEmpty)
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(StructureSave));

                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Minecraft.Resources.Structures." + fileName);

                    if (stream != null)
                    {
                        StructureSave? save = (StructureSave?)serializer.ReadObject(stream);
                        stream.Dispose();

                        if (save != null)
                        {
                            save.Area = new List<Vector2i>();
                            save.Blocks.RemoveDoubleXZ().ForEach((value) => { save.Area.Add(value.Position.Xz); });
                            int largestAreaPosition = -1;
                            foreach (Vector2i position in save.Area)
                            {
                                Vector2i absolutePosition = new Vector2i(Math.Abs(position.X), Math.Abs(position.Y));

                                if (absolutePosition.X > largestAreaPosition || absolutePosition.Y > largestAreaPosition)
                                {
                                    if (absolutePosition.X > absolutePosition.Y)
                                        largestAreaPosition = absolutePosition.X;
                                    else
                                        largestAreaPosition = absolutePosition.Y;
                                }
                            }
                            save.Size = largestAreaPosition;
                            return save;
                        }
                    }
                    else
                    {
                        throw new Exception("Failed loading structures");
                    }
                }
            }
            else
            {
                throw new Exception("Failed loading structures");
            }

            return null;
        }
        #endregion

        #region Binary
        private static void InitStructureBlocksBinary()
        {
            LoadStructureSavesBinary(StructureType.AcaciaTree, "AcaciaTree", 3);
            LoadStructureSavesBinary(StructureType.BirchTree, "BirchTree", 4);
            LoadStructureSavesBinary(StructureType.Cactus, "Cactus", 4);
            LoadStructureSavesBinary(StructureType.JungleTree, "JungleTree", 6);
            LoadStructureSavesBinary(StructureType.OakTree, "OakTree", 5);
            LoadStructureSavesBinary(StructureType.SpruceTree, "SpruceTree", 8);
                              
            LoadStructureSavesBinary(StructureType.SandCastle, "SandCastle", 1);
            LoadStructureSavesBinary(StructureType.Iglu, "Iglu", 1);
            LoadStructureSavesBinary(StructureType.SunkenShip, "SunkenShip", 1);
            LoadStructureSavesBinary(StructureType.ViligerHut, "ViligerHut", 1);
        }

        private static void LoadStructureSavesBinary(StructureType stuctureType, string structureName, int numStructure)
        {
            List<StructureSave> structureSaves = new List<StructureSave>();
            for (int i = 0; i < numStructure; i++)
            {
                StructureSave? save = LoadStructureSaveBinary(structureName + "." + structureName + i + ".dat");

                if (save != null)
                {
                    structureSaves.Add(save);
                }
                else
                {
                    throw new Exception("Falied loading structures");
                }
            }
            StructureSaves.Add(stuctureType, structureSaves);
        }

        private static StructureSave? LoadStructureSaveBinary(string fileName)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            Stream stream = SaveManager.GetStreamFormAssembly("Minecraft.Resources.Structures.Dat." + fileName);

            StructureSave? save = (StructureSave?)binaryFormatter.Deserialize(stream);

            stream.Dispose();

            if (save != null)
            {
                save.Area = new List<Vector2i>();
                save.Blocks.RemoveDoubleXZ().ForEach((value) => { save.Area.Add(value.Position.Xz); });
                int largestAreaPosition = -1;

                foreach (Vector2i position in save.Area)
                {
                    Vector2i absolutePosition = new Vector2i(Math.Abs(position.X), Math.Abs(position.Y));

                    if (absolutePosition.X > largestAreaPosition || absolutePosition.Y > largestAreaPosition)
                    {
                        if (absolutePosition.X > absolutePosition.Y)
                            largestAreaPosition = absolutePosition.X;
                        else
                            largestAreaPosition = absolutePosition.Y;
                    }
                }
                save.Size = largestAreaPosition;
                return save;
            }
            throw new Exception("Failed loading structures");
        }

        private static void CreateEditStructureSaveBinary(Player sender, PlayerChangeBlockEventArgs args)
        {
            if (onStartup)
            {
                firstBlockPosition = args.BlockPosition;
                onStartup = false;
            }

            SaveManager.EnsureDirectory(SaveManager.SaveDirectory);
            bool newFileCreated = SaveManager.EnsureFile(SaveManager.SaveDirectory + "/structure.dat");
            if (newFileCreated)
                firstBlockPosition = args.BlockPosition;
            

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            StructureSave? save = new StructureSave();

            Stream streamReader = File.OpenRead(SaveManager.SaveDirectory + "/structure.dat");

            if (streamReader.Length > 0)
                save = (StructureSave?)binaryFormatter.Deserialize(streamReader);

            streamReader.Dispose();

            if (save != null)
            {
                if (save.Blocks != null)
                {
                    save.Blocks.Add(new BlockStruct() { Position = args.BlockPosition - firstBlockPosition, Type = args.Type });
                }
                else
                {
                    List<BlockStruct> blocks = new List<BlockStruct>
                    {
                        new BlockStruct() { Position = args.BlockPosition - firstBlockPosition, Type = args.Type }
                    };
                    save.Blocks = blocks;
                }


                List<Vector3i> positions = new List<Vector3i>();
                List<int> indicies = new List<int>();
                for (int i = 0; i < save.Blocks.Count; i++)
                {
                    int index = positions.IndexOf(save.Blocks[i].Position);
                    if (index != -1)
                        indicies.Add(index);

                    if (save.Blocks[i].Type == BlockType.Air)
                        indicies.Add(i);

                    positions.Add(save.Blocks[i].Position);
                }
                indicies.Sort();
                indicies = indicies.Distinct().ToList();
                for (int i = indicies.Count - 1; i >= 0; i--)
                {
                    save.Blocks.RemoveAt(indicies[i]);
                }

                File.Delete(SaveManager.SaveDirectory + "/structure.dat");

                Stream streamWriter = File.OpenWrite(SaveManager.SaveDirectory + "/structure.dat");

                binaryFormatter.Serialize(streamWriter, save);

                streamWriter.Dispose();
            }
        }

        private static void ConvertAllStructureSavesToBinary()
        {
            SaveManager.EnsureDirectory(SaveManager.SaveDirectory);
            SaveManager.EnsureDirectory(SaveManager.SaveDirectory + "/bin");

            ChangeXMLStructureSaveToBinary(StructureType.AcaciaTree, "AcaciaTree");
            ChangeXMLStructureSaveToBinary(StructureType.BirchTree, "BirchTree");
            ChangeXMLStructureSaveToBinary(StructureType.Cactus, "Cactus");
            ChangeXMLStructureSaveToBinary(StructureType.JungleTree, "JungleTree");
            ChangeXMLStructureSaveToBinary(StructureType.OakTree, "OakTree");
            ChangeXMLStructureSaveToBinary(StructureType.SpruceTree, "SpruceTree");

            ChangeXMLStructureSaveToBinary(StructureType.SandCastle, "SandCastle");
            ChangeXMLStructureSaveToBinary(StructureType.Iglu, "Iglu");
            ChangeXMLStructureSaveToBinary(StructureType.SunkenShip, "SunkenShip");
            ChangeXMLStructureSaveToBinary(StructureType.ViligerHut, "ViligerHut");
        }

        private static void ChangeXMLStructureSaveToBinary(StructureType structureType, string structureName)
        {
            List<StructureSave> saves = StructureSaves[structureType];
            SaveManager.EnsureDirectory(SaveManager.SaveDirectory + "/bin/" + structureName);

            List<StructureSave> savesBin = ChangeFromBinToNormal(saves);

            for (int i = 0; i < saves.Count; i++)
            {
                Stream stream = File.Open(SaveManager.SaveDirectory + "/bin/" + structureName + "/" + structureName + i + ".dat", FileMode.Create);

                BinaryFormatter binaryFormatter = new BinaryFormatter();

                binaryFormatter.Serialize(stream, savesBin[i]);

                stream.Dispose();
            }
        }
        #endregion
        private static void Player_PlayerChangeBlock(Player sender, PlayerChangeBlockEventArgs args)
        {
            if (SaveBulding)
            {
                CreateEditStructureSaveBinary(sender, args);
            }
        }

        public static List<StructureSave> ChangeFromBinToNormal(List<StructureSave> bin)
        {
            List<StructureSave> result = new List<StructureSave>();
            for (int i = 0; i < bin.Count; i++)
            {
                StructureSave structureSave = new StructureSave();
                structureSave.Area = bin[i].Area;
                structureSave.Size = bin[i].Size;

                List<BlockStruct> blocks = new List<BlockStruct>();
                for (int j = 0; j < bin[i].Blocks.Count; j++)
                {
                    BlockStruct block = new BlockStruct
                    {
                        Position = bin[i].Blocks[j].Position,
                        Type = bin[i].Blocks[j].Type
                    };

                    blocks.Add(block);
                }
                structureSave.Blocks = blocks;

                result.Add(structureSave);
            }

            return result;
        }

        [Serializable()]
        public class StructureSave : ISerializable
        {
            public List<BlockStruct> Blocks { get; set; } = new List<BlockStruct>();
            public List<Vector2i> Area { get; set; } = new List<Vector2i>();
            public int Size;

            public StructureSave() { }
            public StructureSave(SerializationInfo info, StreamingContext context)
            {
                List<BlockStruct>? blocks = (List<BlockStruct>?)info.GetValue("Blocks", typeof(List<BlockStruct>));
                if (blocks != null)
                    this.Blocks = blocks;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Blocks", Blocks);
            }
        }
        #endregion
    }
}
