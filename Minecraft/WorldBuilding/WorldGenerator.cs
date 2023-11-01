using Minecraft.System;
using GameEngine;
using GameEngine.MainLooping;
using GameEngine.Rendering;
using GameEngine.Shadering;
using Minecraft.Entitys;
using Minecraft.WorldBuilding;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Assimp;
using System.Drawing;

namespace Minecraft.WorldBuilding
{
    internal static class WorldGenerator
    {
        internal static int Seed { get; private set; } = 0;
        private static FastNoiseLite Noise;

        internal static void Init()
        {
            Noise = new FastNoiseLite(Seed);
            Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            Noise.SetFrequency(0.0038f);
            Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            Noise.SetFractalOctaves(4);
        }

        internal static Dictionary<Vector3i, BlockType> GenerateChunkColumn(Vector2i position)
        {
            int xChunk = position.X * Chunk.Size - (Chunk.Size / 2);
            int yChunk = position.Y * Chunk.Size - (Chunk.Size / 2);
            int waterLevel = 100;
            int[,] height = ConvertNoiseToHeight(GetNoiseData(xChunk, yChunk, Chunk.Size), Chunk.Size);
            Dictionary<Vector3i, BlockType> blocks = new Dictionary<Vector3i, BlockType>();
            
            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    for (int y = 0; y <= height[x, z]; y++)
                    {
                        if (y == height[x, z])
                            blocks.Add(new Vector3i(x, y, z), BlockType.Grass);
                        else if (y < height[x, z] && y > height[x, z] - 5)
                            blocks.Add(new Vector3i(x, y, z), BlockType.Dirt);
                        else
                            blocks.Add(new Vector3i(x, y, z), BlockType.Stone);
                    }
                    for (int y = height[x, z] + 1; y < ChunkColumn.Height; y++)
                    {
                        if (y < waterLevel)
                            blocks.Add(new Vector3i(x, y, z), BlockType.Water);
                        else
                            blocks.Add(new Vector3i(x, y, z), BlockType.Air);
                    }
                }
            }

            return blocks;
        }

        private static float[,] GetNoiseData(int x, int y, int length)
        {
            float[,] data = new float[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    data[i, j] = Noise.GetNoise(x + i, y + j);
                }
            }
            return data;
        }

        private static int[,] ConvertNoiseToHeight(float[,] noise, int length)
        {
            int[,] data = new int[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    data[i, j] = (int)((noise[i, j] + 1) * (ChunkColumn.Height / 2));
                }
            }
            return data;
        }
    }
}
