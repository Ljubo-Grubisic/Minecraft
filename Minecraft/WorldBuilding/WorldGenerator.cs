using Minecraft.System;
using OpenTK.Mathematics;
using System.Collections.Concurrent;

namespace Minecraft.WorldBuilding
{
    internal static class WorldGenerator
    {
        internal static int Seed { get; private set; } = 0;
        private static NoiseMap Continentalness { get; set; }

        static WorldGenerator()
        {
            (double, double) A = (0.0646594264678, 0.0421697707709);
            (double, double) B = (0.2469652647377, 0.0534643126145);
            (double, double) C = (0.3695780128234, 0.4491941222822);
            (double, double) D = (0.5297772982252, 0.460267828764);
            (double, double) E = (0.6286918518095, 0.7012849804553);
            (double, double) F = (0.7567929922885, 0.704295008785);
            (double, double) G = (0.8503963947414, 0.909765892218);
            (double, double) H = (0.9485658168261, 0.9828222063276);

            Continentalness = new NoiseMap(Seed, 0.0012f, FastNoiseLite.NoiseType.OpenSimplex2);
            Continentalness.CreateSpline(new Vector2((float)A.Item1, (float)A.Item2));
            Continentalness.CreateSpline(new Vector2((float)B.Item1, (float)B.Item2));
            Continentalness.CreateSpline(new Vector2((float)C.Item1, (float)C.Item2));
            Continentalness.CreateSpline(new Vector2((float)D.Item1, (float)D.Item2));
            Continentalness.CreateSpline(new Vector2((float)E.Item1, (float)E.Item2));
            Continentalness.CreateSpline(new Vector2((float)F.Item1, (float)F.Item2));
            Continentalness.CreateSpline(new Vector2((float)G.Item1, (float)G.Item2));
            Continentalness.CreateSpline(new Vector2((float)H.Item1, (float)H.Item2));
        }

        internal static Dictionary<Vector3i, BlockType> GenerateChunk(ChunkColumn chunk)
        {
            int xChunk = chunk.Position.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunk.Position.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int waterLevel = 100;
            int[,] height = Continentalness.ConvertMapedDataToIntScale(Continentalness.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize), 0, ChunkColumn.Height);
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

            Random random = new Random();
            int randomX = random.Next(0, ChunkColumn.ChunkSize);
            int randomZ = random.Next(0, ChunkColumn.ChunkSize);
            randomX = 0;
            randomZ = 0;

            if (height[randomX, randomZ] > waterLevel)
                Structure.AddStructure(ref blocks, StructureType.OakTree, new Vector3i(randomX, height[randomX, randomZ] + 1, randomZ), chunk.Position);

            Structure.AddGhostBlocks(ref blocks, chunk.Position);
            
            return blocks;
        }
    }
}
