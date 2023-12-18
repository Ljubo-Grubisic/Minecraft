using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal static class WorldGenerator
    {
        internal static int Seed { get; private set; } = 0;
        internal static int WaterLevel { get; private set; } = 50;

        internal static NoiseMap Random { get; private set; }

        private static NoiseMap Continentalness { get; set; }
        private static NoiseMap Vegetation { get; set; }

        private static NoiseMap Temperature { get; set; }
        private static NoiseMap Humidity { get; set; }

        static WorldGenerator()
        {
            Random = new NoiseMap(WorldGenerator.Seed, 2f, FastNoiseLite.NoiseType.OpenSimplex2);

            {
                (double, double) A = (0.0646594264678, 0.0421697707709);
                (double, double) B = (0.2529110529704, 0.0672673482315);
                (double, double) C = (0.4247048230984, 0.1262672288816);
                (double, double) D = (0.5947633026192, 0.2598846056478);
                (double, double) E = (0.829027534612, 0.6607367359467);
                (double, double) F = (0.9279390992312, 0.8655010276145);
                (double, double) G = (0.7353218418149, 0.466384187923);
                (double, double) H = (0.9747919456298, 0.960942011019);

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

            Vegetation = new NoiseMap(Seed, 0.0009f, noiseType: FastNoiseLite.NoiseType.OpenSimplex2, fractalType: FastNoiseLite.FractalType.None, numFractalOcaves: 0);

            Temperature = new NoiseMap(Seed, 0.00198f);
            Humidity = new NoiseMap(Seed, 0.002f);
        }

        internal static Dictionary<Vector3i, BlockType> GenerateChunk(ChunkColumn chunk)
        {
            int xChunk = chunk.Position.X * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);
            int yChunk = chunk.Position.Y * ChunkColumn.ChunkSize - (ChunkColumn.ChunkSize / 2);


            float[,] mapedHeightData = Continentalness.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);
            int[,] height = Continentalness.ConvertMapedDataToIntScale(mapedHeightData, 0, ChunkColumn.Height);

            float[,] mapedTemperatureData = Temperature.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);
            int[,] temperature = Temperature.ConvertMapedDataToIntScale(mapedTemperatureData, 0, 45);

            float[,] mapedHumidityData = Humidity.GetMapedNoiseData(xChunk, yChunk, ChunkColumn.ChunkSize);
            int[,] humidity = Humidity.ConvertMapedDataToIntScale(mapedHumidityData, 0, 100);

            Dictionary<Vector3i, BlockType> blocks = Biome.GenerateBiome(Vegetation, chunk.Position, height, temperature, humidity);

            Structure.AddGhostBlocks(ref blocks, chunk.Position);

            return blocks;
        }
    }
}