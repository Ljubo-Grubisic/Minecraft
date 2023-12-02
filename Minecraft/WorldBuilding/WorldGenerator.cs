using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal static class WorldGenerator
    {
        internal static int Seed { get; private set; } = 0;
        private static NoiseMap Continentalness { get; set; }
        private static NoiseMap Vegetation { get; set; }

        static WorldGenerator()
        {
            (double, double) A = (0.0646594264678, 0.0421697707709);
            (double, double) B = (0.2925936785793, 0.0817570248029);
            (double, double) C = (0.5212511866198, 0.1602858659481);
            (double, double) D = (0.694476571499, 0.3127242046418);
            (double, double) E = (0.9346824385314, 0.6637943179969);
            (double, double) F = (0.9716371873056, 0.8855228106422);
            (double, double) G = (0.8653922845797, 0.4097304201741);
            (double, double) H = (0.9947339052895, 0.9732903389809);

            Continentalness = new NoiseMap(Seed, 0.0012f, FastNoiseLite.NoiseType.OpenSimplex2);
            Continentalness.CreateSpline(new Vector2((float)A.Item1, (float)A.Item2));
            Continentalness.CreateSpline(new Vector2((float)B.Item1, (float)B.Item2));
            Continentalness.CreateSpline(new Vector2((float)C.Item1, (float)C.Item2));
            Continentalness.CreateSpline(new Vector2((float)D.Item1, (float)D.Item2));
            Continentalness.CreateSpline(new Vector2((float)E.Item1, (float)E.Item2));
            Continentalness.CreateSpline(new Vector2((float)F.Item1, (float)F.Item2));
            Continentalness.CreateSpline(new Vector2((float)G.Item1, (float)G.Item2));
            Continentalness.CreateSpline(new Vector2((float)H.Item1, (float)H.Item2));

            Vegetation = new NoiseMap(Seed, 0.15f, fractalType: FastNoiseLite.FractalType.None, numFractalOcaves: 0);
        }

        internal static Dictionary<Vector3i, BlockType> GenerateChunk(ChunkColumn chunk)
        {
            int xChunk = chunk.Position.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunk.Position.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int waterLevel = 50;
            float[,] mapedData = Continentalness.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);
            int[,] height = Continentalness.ConvertMapedDataToIntScale(mapedData, 0, ChunkColumn.Height);
            Dictionary<Vector3i, BlockType> blocks = new Dictionary<Vector3i, BlockType>();

            for (int x = 0; x < ChunkColumn.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkColumn.ChunkSize; z++)
                {
                    for (int y = 0; y <= height[x, z]; y++)
                    {
                        if (y == height[x, z])
                            blocks[new Vector3i(x, y, z)] = BlockType.Grass;
                        else if (y < height[x, z] && y > height[x, z] - 5)
                            blocks[new Vector3i(x, y, z)] = BlockType.Dirt;
                        else
                            blocks[new Vector3i(x, y, z)] = BlockType.Stone;
                    }
                    for (int y = height[x, z] + 1; y <= ChunkColumn.Height; y++)
                    {
                        if (y < waterLevel)
                            blocks[new Vector3i(x, y, z)] = BlockType.Water;
                        else
                            blocks[new Vector3i(x, y, z)] = BlockType.Air;
                    }
                }
            }

            Structure.AddVegetation(Vegetation, ref blocks, height, waterLevel, chunk.Position);
            Structure.AddGhostBlocks(ref blocks, chunk.Position);

            return blocks;
        }
    }
}
