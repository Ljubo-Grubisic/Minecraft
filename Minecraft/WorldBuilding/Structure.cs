using Minecraft.Entitys;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

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

        private static Dictionary<Vector2i, List<BlockStruct>> ChunkColumnGhostBlocks = new Dictionary<Vector2i, List<BlockStruct>>();
        private static Dictionary<Vector2i, List<(Vector2i Position, StructureType Structure, int StructureIndex)>> StructuresGenerated = new Dictionary<Vector2i, List<(Vector2i Position, StructureType Structure, int StructureIndex)>>();

        private static NoiseMap Random;

        static Structure()
        {
            InitStructureBlocks();
            Random = new NoiseMap(WorldGenerator.Seed, 0.25f, FastNoiseLite.NoiseType.OpenSimplex2);
            Player.PlayerChangeBlock += Player_PlayerChangeBlock;
        }

        internal static void AddVegetation(NoiseMap vegetation, ref Dictionary<Vector3i, BlockType> blocks, Vector2i chunkColumnPosition, int[,] height, BiomeType[,] biome, int waterLevel)
        {
            int xChunk = chunkColumnPosition.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunkColumnPosition.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);

            int numStructures = vegetation.ConvertMapedValueToIntScale(vegetation.GetMapedNoiseValue(xChunk, yChunk), 0, 4);
            float[,] randomData = Random.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);

            List<(Vector2i, float)> vegetationDataList = new List<(Vector2i, float)>();
            for (int i = 0; i < ChunkColumn.ChunkSize; i++)
            {
                for (int j = 0; j < ChunkColumn.ChunkSize; j++)
                {
                    if (height[i, j] > waterLevel)
                        vegetationDataList.Add((new Vector2i(i, j), randomData[i, j]));
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
                                vegetationDataList.Add(((value.Position.X, value.Position.Y), -1));
                            });
                        }
                    }
                }
            }

            // Remove blocks that are near structures that are generated
            RemoveNotSpawnableBlocksFast(ref vegetationDataList, allStructurePositions, height, biome);

            // Remove items that are not in the chunk 
            vegetationDataList = vegetationDataList.Remove((value) =>
            {
                if (value.Item2 == -1)
                    return true; return false;
            });

            // Generate structures
            List<(Vector2i Position, StructureType Structure, int StructureIndex)> validStructures = new List<(Vector2i Position, StructureType Structure, int StructureIndex)>();
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
                    StructureType structureType = Biome.GetBiomeConfig(biome[vegetationDataList[0].Item1.X, vegetationDataList[0].Item1.Y]).MainVegetationStructure;
                    if (structureType != StructureType.None)
                    {
                        Vector3i position = new Vector3i(index.X, height[index.X, index.Y] + 1, index.Y);
                        int structureIndex = AddStructure(ref blocks, structureType, position, chunkColumnPosition);
                        validStructures.Add((new Vector2i(index.X, index.Y), structureType, structureIndex));

                        RemoveNotSpawnableBlocksFast(ref vegetationDataList, allStructurePositions, height, biome);
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
            if (ChunkColumnGhostBlocks.TryGetValue(position, out List<BlockStruct>? ghostBlocks))
            {
                foreach (BlockStruct block in ghostBlocks)
                {
                    if (block.Type != BlockType.Air)
                        blocks[block.Position] = block.Type;
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

            int index = Random.ConvertMapedValueToIntScale(Random.GetMapedNoiseValue(structurePosition.X, structurePosition.Y), 0, numLists);
            if (index == numLists)
                index = numLists - 1;

            return index;
        }

        private static List<BlockStruct> RotateBlocksAlgorithm(List<BlockStruct> blocks, Vector3i structurePosition)
        {
            int rotationIndex = Random.ConvertMapedValueToIntScale(Random.GetMapedNoiseValue(structurePosition.X, structurePosition.Y), 0, 5);

            if (rotationIndex == 5)
                rotationIndex = 4;

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
                            StructureType type = Biome.GetBiomeConfig(biome[vegetationValue.Position.X, vegetationValue.Position.Y]).MainVegetationStructure;
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
                        StructureType type = Biome.GetBiomeConfig(biome[value.Position.X, value.Position.Y]).MainVegetationStructure;
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

        private static int AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i strucuturePosition, Vector2i chunkColumnPosition)
        {
            int index = GetIndexForStructure(structureType, strucuturePosition);

            List<BlockStruct> structureBlocks = GetBlocksByStructure(structureType, index);
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
                    bool isOutOfRange = false;

                    do
                    {
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
            return index;
        }
        #endregion

        #region Creating and Loading StructureSaves
        private static Vector3i firstBlockPosition;
        private static bool onStartup = true;
        private static void CreateEditStructureSave(Player sender, PlayerChangeBlockEventArgs args)
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
                    save.Type = StructureType.OakTree;
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

        private static void InitStructureBlocks()
        {
            LoadStructureSaves(StructureType.AcaciaTree, "AcaciaTree", 3);
            LoadStructureSaves(StructureType.BirchTree, "BirchTree", 4);
            LoadStructureSaves(StructureType.Cactus, "Cactus", 4);
            LoadStructureSaves(StructureType.JungleTree, "JungleTree", 6);
            LoadStructureSaves(StructureType.OakTree, "OakTree", 5);
            LoadStructureSaves(StructureType.SpruceTree, "SpruceTree", 8);
        }

        private static void LoadStructureSaves(StructureType stuctureType, string structureName, int numStructure)
        {
            List<StructureSave> structureSaves = new List<StructureSave>();
            for (int i = 1; i <= numStructure; i++)
            {
                StructureSave? save = LoadStructureSave(structureName + "." + structureName + i + ".xml");

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

        private static StructureSave? LoadStructureSave(string fileName)
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

        private static void Player_PlayerChangeBlock(Player sender, PlayerChangeBlockEventArgs args)
        {
            if (SaveBulding)
            {
                CreateEditStructureSave(sender, args);
            }
        }

        [DataContract]
        private class StructureSave
        {
            [DataMember]
            public StructureType Type { get; set; }
            [DataMember]
            public List<BlockStruct> Blocks { get; set; }
            public List<Vector2i> Area { get; set; } = new List<Vector2i>();
            public int Size;

            public StructureSave()
            {
                Blocks = new List<BlockStruct>();
            }
        }
        #endregion
    }
}
