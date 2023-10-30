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
        internal static int Seed { get; private set; } = 00000;
        private static FastNoiseLite Noise;

        internal static void Init()
        {
            Noise = new FastNoiseLite(Seed);
            Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        }

        internal static Dictionary<Vector3i, BlockType> GenerateChunk(Chunk chunk)
        {
            int xChunk = chunk.Position.X * Chunk.Size.X;
            int yChunk = chunk.Position.Y * Chunk.Size.Z;
            int[,] height = ConvertNoiseToHeight(GetNoiseData(xChunk, yChunk, Chunk.Size.X), Chunk.Size.X);
            Dictionary<Vector3i, BlockType> blocks = new Dictionary<Vector3i, BlockType>();
            
            for (int x = 0; x < Chunk.Size.X; x++)
            {
                for (int z = 0; z < Chunk.Size.Z; z++)
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
                    for (int y = height[x, z] + 1; y < Chunk.Size.Y; y++)
                    {
                        blocks.Add(new Vector3i(x, y, z), BlockType.Air);
                    }
                }
            }

            //int height = 125;
            //if (chunk.Position.X % 2 == 0)
            //    height += 3;
            //else 
            //    height -= 2;
            //if (chunk.Position.Y % 2 == 0)
            //    height += 3;
            //else
            //    height -= 2;
            //
            //for (int x = 0; x < Chunk.Size.X; x++)
            //{
            //    for (int z = 0; z < Chunk.Size.Z; z++)
            //    {
            //        for (int y = 0; y <= height; y++)
            //        {   
            //            if (y == height)
            //                blocks.Add(new Vector3i(x, y, z), BlockType.Grass);
            //            else if (y < height && y > height - 5)
            //                blocks.Add(new Vector3i(x, y, z), BlockType.Dirt);
            //            else
            //                blocks.Add(new Vector3i(x, y, z), BlockType.Stone);
            //        }
            //        for (int y = height + 1; y < Chunk.Size.Y; y++)
            //        {
            //            blocks.Add(new Vector3i(x, y, z), BlockType.Air);
            //        }
            //    }
            //}

            return blocks;
        }

        private static float[,] GetNoiseData(int x, int y, int length)
        {
            float[,] data = new float[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    data[i, j] = Noise.GetNoise(x + i, y, + j);
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
                    data[i, j] = (int)(noise[i, j] * (Chunk.Size.Y / 2)) + (Chunk.Size.Y / 2);
                }
            }
            return data;
        }
    }
}
