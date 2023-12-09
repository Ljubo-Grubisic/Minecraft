using Minecraft.Entitys;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Minecraft.WorldBuilding
{
    internal enum StructureType
    {
        OakTree,
        Cactus,
    }

    internal static class Structure
    {
        private static Dictionary<StructureType, List<StructureSave>> StructureSaves = new Dictionary<StructureType, List<StructureSave>>();

        private static Dictionary<Vector2i, List<BlockStruct>> ChunkColumnGhostBlocks = new Dictionary<Vector2i, List<BlockStruct>>();
        private static Dictionary<Vector2i, List<Vector2i>> StructuresGenerated = new Dictionary<Vector2i, List<Vector2i>>();

        private static NoiseMap Random;

        static Structure()
        {
            InitStructureBlocks();
            Random = new NoiseMap(WorldGenerator.Seed, 0.25f, FastNoiseLite.NoiseType.OpenSimplex2);
            Player.PlayerChangeBlock += Player_PlayerChangeBlock;
        }

        internal static void AddVegetation(NoiseMap vegetation, ref Dictionary<Vector3i, BlockType> blocks, int[,] height, int waterLevel, Vector2i chunkColumnPosition)
        {
            int xChunk = chunkColumnPosition.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunkColumnPosition.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);

            int vegetationData = vegetation.ConvertMapedValueToIntScale(vegetation.GetMapedNoiseValue(xChunk, yChunk), 0, 4);
            float[,] randomData = Random.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);

            int spacingBetweenStructures = 6;

            List<(Vector2i, float)> vegetationDataList = new List<(Vector2i, float)>();
            for (int i = 0; i < ChunkColumn.ChunkSize; i++)
            {
                for (int j = 0; j < ChunkColumn.ChunkSize; j++)
                {
                    if (height[i, j] > waterLevel)
                        vegetationDataList.Add((new Vector2i(i, j), randomData[i, j]));
                }
            }

            int numStructures = vegetationData;

            // Remove spaces that are ocupied by structures in outher chunks
            List<Vector2i>? positions = new List<Vector2i>();
            List<Vector2i> allStructurePositions = new List<Vector2i>();
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
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
                    if (Math.Abs(value.Item1.X - allStructurePositions[j].X) <= spacingBetweenStructures
                    && Math.Abs(value.Item1.Y - allStructurePositions[j].Y) <= spacingBetweenStructures)
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

        private static int GetNumListsStructType(StructureType structureType)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? blocks))
            {
                return blocks.Count;
            }
            return -1;
        }

        private static List<BlockStruct> GetBlocksByStructure(StructureType structureType, int index)
        {
            if (StructureSaves.TryGetValue(structureType, out List<StructureSave>? blocks))
            {
                return blocks[index].Blocks;
            }
            return new List<BlockStruct>();
        }

        private static void AddStructure(ref Dictionary<Vector3i, BlockType> blocks, StructureType structureType, Vector3i strucuturePosition, Vector2i chunkColumnPosition)
        {
            int numLists = GetNumListsStructType(structureType);
            if (numLists == -1)
                throw new Exception("Invalid structure type, this structure type has no block lists or isnt loaded");

            int index = Random.ConvertMapedValueToIntScale(Random.GetMapedNoiseValue(strucuturePosition.X, strucuturePosition.Y), 0, numLists);
            if (index == numLists)
                index = numLists - 1;

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

        private static Vector3i firstBlockPosition;
        private static bool onStartup = true;
        private static void Player_PlayerChangeBlock(Player sender, PlayerChangeBlockEventArgs args)
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
                for (int i = indicies.Count - 1; i >= 0 ; i--)
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
            List<StructureSave> structureSaves = new List<StructureSave>();

            for (int i = 1; i < 6; i++)
            {
                StructureSave? save = LoadStructureSave("OakTree.OakTree" + i + ".xml");

                if (save != null)
                {
                    structureSaves.Add(save);
                }
                else
                {
                    throw new Exception("Falied loading structures");
                }
            }

            StructureSaves.Add(StructureType.OakTree, structureSaves);
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

        [DataContract]
        private class StructureSave
        {
            [DataMember]
            public StructureType Type { get; set; }
            [DataMember]
            public List<BlockStruct> Blocks { get; set; }

            public StructureSave()
            {
                Blocks = new List<BlockStruct>();
            }
        }
    }
}
