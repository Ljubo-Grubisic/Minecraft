using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal enum BiomeType
    {
        None = -1,

        GravelOcean,
        ClayOcean,
        SandOcean,
        StoneOcean,

        OakForest,
        BirchForest,
        OakBirchForest,
        SpruceForest,
        Desert,
        Savanna,
        Jungle,

        MoutainSpruceForest,

        MountainPeek,

        Count
    }

    internal enum BiomeCategory
    {
        None = -1,

        Ocean,
        Plain,
        Moutain,
        Peek,

        Count
    }

    internal enum VegatationStructureType
    {
        None = -1,

        Primary,
        Secondary,

        Count
    }

    internal class BiomeCategoryConfig
    {
        public BiomeCategory Category { get; set; }
        public Vector2i HeightRange { get; set; }
        public BiomeType[] BiomeTypes { get; set; }
    }

    internal class BiomeConfig
    {
        public BiomeType Type { get; set; }

        public int MaxNumVegetationStructures { get; set; }
        public int TopLayerHeight { get; set; } = 5;

        public Vector2 TemperatureRange { get; set; }
        public Vector2 HumidityRange { get; set; }

        public StructureType PrimaryVegetationStructure { get; set; }
        public StructureType SecondaryVegetationStructure { get; set; }
        public StructureType RareStructure { get; set; }

        public BlockType TopBlockType { get; set; }
        public BlockType MiddleBlockType { get; set; }
        public BlockType BottomBlockType { get; set; }
    }

    internal static class Biome
    {
        private static Dictionary<BiomeType, BiomeConfig> BiomeConfigs = new Dictionary<BiomeType, BiomeConfig>();
        private static Dictionary<BiomeCategory, BiomeCategoryConfig> BiomeCategoryConfigs = new Dictionary<BiomeCategory, BiomeCategoryConfig>();

        internal static Dictionary<Vector3i, BlockType> GenerateBiome(NoiseMap vegetation, Vector2i chunkPosition, int[,] height, int[,] temperature, int[,] humidity)
        {
            BiomeType[,] biome = new BiomeType[ChunkColumn.ChunkSize, ChunkColumn.ChunkSize];
            Dictionary<Vector3i, BlockType> blocks = new Dictionary<Vector3i, BlockType>();

            for (int x = 0; x < ChunkColumn.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkColumn.ChunkSize; z++)
                {
                    BiomeConfig biomeConfig = GetBiome(height[x, z], temperature[x, z], humidity[x, z]);
                    biome[x, z] = biomeConfig.Type;

                    for (int y = 0; y <= height[x, z]; y++)
                    {
                        if (y == height[x, z])
                            blocks[new Vector3i(x, y, z)] = biomeConfig.TopBlockType;
                        else if (y < height[x, z] && y > height[x, z] - biomeConfig.TopLayerHeight - 1)
                            blocks[new Vector3i(x, y, z)] = biomeConfig.MiddleBlockType;
                        else if (y < height[x, z] && y > height[x, z] - biomeConfig.TopLayerHeight)
                            blocks[new Vector3i(x, y, z)] = biomeConfig.BottomBlockType;
                        else
                            blocks[new Vector3i(x, y, z)] = BlockType.Stone;
                    }
                    for (int y = height[x, z] + 1; y <= ChunkColumn.Height; y++)
                    {
                        if (y < WorldGenerator.WaterLevel)
                            blocks[new Vector3i(x, y, z)] = BlockType.Water;
                        else
                            blocks[new Vector3i(x, y, z)] = BlockType.Air;
                    }
                }
            }

            Structure.AddVegetation(vegetation, ref blocks, chunkPosition, height, biome, WorldGenerator.WaterLevel);
            Structure.AddRareStructure(ref blocks, chunkPosition, height, biome, WorldGenerator.WaterLevel, 30);

            return blocks;
        }

        internal static BiomeConfig GetBiomeConfig(BiomeType type)
        {
            if (BiomeConfigs.TryGetValue(type, out BiomeConfig? biome))
            {
                if (biome != null)
                    return biome;
            }
            throw new Exception("Biome not loaded");
        }

        internal static BiomeCategoryConfig GetBiomeCategoryConfig(BiomeCategory category)
        {
            if (BiomeCategoryConfigs.TryGetValue(category, out BiomeCategoryConfig? biomeCategory))
            {
                if (biomeCategory != null)
                    return biomeCategory;
            }
            throw new Exception("BiomeCategory not loaded");
        }

        internal static StructureType GetBiomeConfigStructureByPosition(BiomeType type, Vector2i position)
        {
            BiomeConfig biomeConfig = GetBiomeConfig(type);

            if (biomeConfig.SecondaryVegetationStructure == StructureType.None)
            {
                return biomeConfig.PrimaryVegetationStructure;
            }

            int rotationIndex = WorldGenerator.Random.ConvertMapedValueToIntScale(WorldGenerator.Random.GetMapedNoiseValue(position.X, position.Y), -1, 2);

            if (rotationIndex == -1)
                rotationIndex = 0;
            if (rotationIndex == 2)
                rotationIndex = 1;

            if (rotationIndex == 0)
                return GetBiomeConfig(type).PrimaryVegetationStructure;
            if (rotationIndex == 1)
                return GetBiomeConfig(type).SecondaryVegetationStructure;

            throw new Exception("Faild getting random number");
        }

        private static BiomeConfig GetBiome(int height, int temperature, int humidity)
        {
            BiomeCategory biomeCategory = GetBiomeCategory(height);
            BiomeCategoryConfig biomeCategoryConfig = GetBiomeCategoryConfig(biomeCategory);

            foreach (BiomeType type in biomeCategoryConfig.BiomeTypes)
            {
                BiomeConfig biomeConfig = GetBiomeConfig(type);

                if (temperature >= biomeConfig.TemperatureRange.X && temperature <= biomeConfig.TemperatureRange.Y &&
                    humidity >= biomeConfig.HumidityRange.X && humidity <= biomeConfig.HumidityRange.Y)
                {
                    return biomeConfig;
                }
            }
            throw new Exception("Invalid input no BiomeConfig has requested attributes");
        }

        private static BiomeCategory GetBiomeCategory(int height)
        {
            foreach (BiomeCategoryConfig category in BiomeCategoryConfigs.Values)
            {
                if (height >= category.HeightRange.X && height <= category.HeightRange.Y)
                {
                    return category.Category;
                }
            }
            throw new Exception("No category fits height: " + height);
        }

        internal static void Init()
        {
            BiomeConfig SandOcean = new BiomeConfig
            {
                Type = BiomeType.SandOcean,
                MaxNumVegetationStructures = 0,

                TemperatureRange = new Vector2(0, 22.5f),
                HumidityRange = new Vector2(20, 100),

                PrimaryVegetationStructure = StructureType.None,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.SunkenShip,

                TopBlockType = BlockType.Sand,
                MiddleBlockType = BlockType.SandStone,
                BottomBlockType = BlockType.SandStone,
            };
            BiomeConfig GravelOcean = new BiomeConfig
            {
                Type = BiomeType.GravelOcean,
                MaxNumVegetationStructures = 0,

                TemperatureRange = new Vector2(22.5f, 45),
                HumidityRange = new Vector2(50, 100),

                PrimaryVegetationStructure = StructureType.None,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.SunkenShip,

                TopBlockType = BlockType.Gravel,
                MiddleBlockType = BlockType.Gravel,
                BottomBlockType = BlockType.Stone,
            };
            BiomeConfig ClayOcean = new BiomeConfig
            {
                Type = BiomeType.ClayOcean,
                MaxNumVegetationStructures = 0,

                TemperatureRange = new Vector2(22.5f, 45),
                HumidityRange = new Vector2(0, 50),

                PrimaryVegetationStructure = StructureType.None,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.SunkenShip,

                TopBlockType = BlockType.Clay,
                MiddleBlockType = BlockType.Gravel,
                BottomBlockType = BlockType.Stone,
            };
            BiomeConfig StoneOcean = new BiomeConfig
            {
                Type = BiomeType.StoneOcean,
                MaxNumVegetationStructures = 0,

                TemperatureRange = new Vector2(0, 22.5f),
                HumidityRange = new Vector2(0, 20),

                PrimaryVegetationStructure = StructureType.None,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.SunkenShip,

                TopBlockType = BlockType.Stone,
                MiddleBlockType = BlockType.Gravel,
                BottomBlockType = BlockType.Stone,
            };

            BiomeConfig OakForest = new BiomeConfig
            {
                Type = BiomeType.OakForest,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(15, 30),
                HumidityRange = new Vector2(0, 33),

                PrimaryVegetationStructure = StructureType.OakTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.ViligerHut,

                TopBlockType = BlockType.PlainGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };
            BiomeConfig BirchForest = new BiomeConfig
            {
                Type = BiomeType.BirchForest,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(0, 15),
                HumidityRange = new Vector2(0, 33),

                PrimaryVegetationStructure = StructureType.BirchTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.ViligerHut,

                TopBlockType = BlockType.PlainGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };
            BiomeConfig OakBrichForest = new BiomeConfig
            {
                Type = BiomeType.OakBirchForest,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(15, 30),
                HumidityRange = new Vector2(33, 66),

                PrimaryVegetationStructure = StructureType.BirchTree,
                SecondaryVegetationStructure = StructureType.OakTree,
                RareStructure = StructureType.ViligerHut,

                TopBlockType = BlockType.PlainGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };
            BiomeConfig SpruceForest = new BiomeConfig
            {
                Type = BiomeType.SpruceForest,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(0, 15),
                HumidityRange = new Vector2(33, 100),

                PrimaryVegetationStructure = StructureType.SpruceTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.Iglu,

                TopBlockType = BlockType.SpruceForestGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };
            BiomeConfig Desert = new BiomeConfig
            {
                Type = BiomeType.Desert,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(30, 45),
                HumidityRange = new Vector2(0, 33),

                PrimaryVegetationStructure = StructureType.Cactus,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.SandCastle,

                TopBlockType = BlockType.Sand,
                MiddleBlockType = BlockType.Sand,
                BottomBlockType = BlockType.SandStone,
            };
            BiomeConfig Savanna = new BiomeConfig
            {
                Type = BiomeType.Savanna,
                MaxNumVegetationStructures = 4,

                TemperatureRange = new Vector2(30, 45),
                HumidityRange = new Vector2(33, 66),

                PrimaryVegetationStructure = StructureType.AcaciaTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.ViligerHut,

                TopBlockType = BlockType.SavannaGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };
            BiomeConfig Jungle = new BiomeConfig
            {
                Type = BiomeType.Jungle,
                MaxNumVegetationStructures = 3,

                TemperatureRange = new Vector2(15, 45),
                HumidityRange = new Vector2(66, 100),

                PrimaryVegetationStructure = StructureType.JungleTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.ViligerHut,

                TopBlockType = BlockType.JungleGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };

            BiomeConfig MoutainSpruceForest = new BiomeConfig
            {
                Type = BiomeType.MoutainSpruceForest,
                MaxNumVegetationStructures = 3,

                TemperatureRange = new Vector2(0, 45),
                HumidityRange = new Vector2(0, 100),

                PrimaryVegetationStructure = StructureType.SpruceTree,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.Iglu,

                TopBlockType = BlockType.SnowCoveredGrass,
                MiddleBlockType = BlockType.Dirt,
                BottomBlockType = BlockType.Dirt,
            };

            BiomeConfig MountainPeek = new BiomeConfig
            {
                Type = BiomeType.MountainPeek,
                MaxNumVegetationStructures = 1,

                TemperatureRange = new Vector2(0, 45),
                HumidityRange = new Vector2(0, 100),

                PrimaryVegetationStructure = StructureType.None,
                SecondaryVegetationStructure = StructureType.None,
                RareStructure = StructureType.Iglu,

                TopBlockType = BlockType.BlueIce,
                MiddleBlockType = BlockType.Stone,
                BottomBlockType = BlockType.Stone,
            };

            BiomeConfigs.Add(SandOcean.Type, SandOcean);
            BiomeConfigs.Add(GravelOcean.Type, GravelOcean);
            BiomeConfigs.Add(ClayOcean.Type, ClayOcean);
            BiomeConfigs.Add(StoneOcean.Type, StoneOcean);

            BiomeConfigs.Add(OakForest.Type, OakForest);
            BiomeConfigs.Add(BirchForest.Type, BirchForest);
            BiomeConfigs.Add(OakBrichForest.Type, OakBrichForest);
            BiomeConfigs.Add(SpruceForest.Type, SpruceForest);
            BiomeConfigs.Add(Desert.Type, Desert);
            BiomeConfigs.Add(Savanna.Type, Savanna);
            BiomeConfigs.Add(Jungle.Type, Jungle);

            BiomeConfigs.Add(MoutainSpruceForest.Type, MoutainSpruceForest);

            BiomeConfigs.Add(MountainPeek.Type, MountainPeek);

            BiomeCategoryConfig Ocean = new BiomeCategoryConfig
            {
                Category = BiomeCategory.Ocean,
                HeightRange = new Vector2i(0, WorldGenerator.WaterLevel),
                BiomeTypes = new BiomeType[] { BiomeType.SandOcean, BiomeType.StoneOcean, BiomeType.GravelOcean, BiomeType.ClayOcean }
            };
            BiomeCategoryConfig Plain = new BiomeCategoryConfig
            {
                Category = BiomeCategory.Plain,
                HeightRange = new Vector2i(WorldGenerator.WaterLevel, (int)(ChunkColumn.Height / 1.7)),
                BiomeTypes = new BiomeType[] { BiomeType.OakForest, BiomeType.BirchForest, BiomeType.OakBirchForest, BiomeType.SpruceForest, BiomeType.Desert, BiomeType.Savanna, BiomeType.Jungle }
            };
            BiomeCategoryConfig Moutain = new BiomeCategoryConfig
            {
                Category = BiomeCategory.Moutain,
                HeightRange = new Vector2i((int)(ChunkColumn.Height / 1.7), (int)(ChunkColumn.Height / 1.2)),
                BiomeTypes = new BiomeType[] { BiomeType.MoutainSpruceForest }
            };
            BiomeCategoryConfig MoutainPeek = new BiomeCategoryConfig
            {
                Category = BiomeCategory.Peek,
                HeightRange = new Vector2i((int)(ChunkColumn.Height / 1.2), ChunkColumn.Height),
                BiomeTypes = new BiomeType[] { BiomeType.MountainPeek }
            };

            BiomeCategoryConfigs.Add(Ocean.Category, Ocean);
            BiomeCategoryConfigs.Add(Plain.Category, Plain);
            BiomeCategoryConfigs.Add(Moutain.Category, Moutain);
            BiomeCategoryConfigs.Add(MoutainPeek.Category, MoutainPeek);
        }
    }
}
