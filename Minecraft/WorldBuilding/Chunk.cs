﻿using GameEngine.ModelLoading;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using GameEngine.Shadering;
using System.Diagnostics;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal Vector2i Position { get; private set; }

        internal static readonly Vector3i Size = new Vector3i(16, 128, 16);
        internal BlockStruct[,,] Blocks = new BlockStruct[Size.X, Size.Y, Size.Z];

        internal Mesh Mesh { get; private set; }

        internal bool IsGenerated { get; private set; } = false;
        internal bool IsUnloaded { get; private set; } = false;

        internal Chunk(Vector2i position)
        {
            this.Position = position;
            GenerateChunk();
        }

        internal void GenerateChunk()
        {
            BlockStruct bufferBlock = new BlockStruct();

            int height = 32 + (Size.Y / 2);

            for (int x = 0; x < Size.X; x++)
            {
                for (int z = 0; z < Size.Z; z++)
                {
                    for (int y = 0; y <= height; y++)
                    {
                        bufferBlock.Position.X = x - (Size.X / 2);
                        bufferBlock.Position.Y = y - (Size.Y / 2);
                        bufferBlock.Position.Z = z - (Size.Z / 2);
                        if (y == height)
                            bufferBlock.Type = BlockType.Grass;
                        else if (y < height && y > height - 5)
                            bufferBlock.Type = BlockType.Dirt;
                        else
                            bufferBlock.Type = BlockType.Stone;
                        this.Blocks[x, y, z] = bufferBlock;
                    }
                    for (int y = height + 1; y < Size.Y; y++)
                    {
                        bufferBlock.Position.X = x - (Size.X / 2);
                        bufferBlock.Position.Y = y - (Size.Y / 2);
                        bufferBlock.Position.Z = z - (Size.Z / 2);
                        bufferBlock.Type = BlockType.Air;
                        this.Blocks[x, y, z] = bufferBlock;
                    }
                }
            }
            this.IsGenerated = true;
        }

        internal void Bake(List<Chunk> neighborChunks)
        {
            List<Vertex> vertices = new List<Vertex>();
            Vertex vertexBuffer = new Vertex();

            foreach (BlockStruct block in Blocks)
            {
                if (block.Type != BlockType.Air)
                {
                    if (!IsBlockSideCovered(block, new Vector3i(0, 0, -1), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, 0, 1), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 6];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type); ;
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(-1, 0, 0), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 12];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(1, 0, 0), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i+ 18];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, -1, 0), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 24];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, 1, 0), neighborChunks))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 30];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
                            vertices.Add(vertexBuffer);
                        }
                    }
                }
            }

            this.Mesh = new Mesh(vertices, new(), new());
        }

        internal bool IsBlockSideCovered(BlockStruct block, Vector3i offset, List<Chunk> neighborChunks)
        {
            Vector3i position = block.Position + offset + (Size / 2);
            if (!IsOutOfRange(position, Size))
            {
                if (Blocks[position.X, position.Y, position.Z].Type != BlockType.Air)
                {
                    return true;
                }
            }
            else
            {
                if (offset.X == 1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(1, 0) + this.Position);
                    if (index != -1)
                    {
                        if (neighborChunks[index].Blocks[Chunk.Size.X - 1, position.Y, position.Z].Type != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.X == -1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(-1, 0) + this.Position);
                    if (index != -1)
                    {
                        if (neighborChunks[index].Blocks[0, position.Y, position.Z].Type != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Z == 1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(0, 1) + this.Position);
                    if (index != -1)
                    {
                        if (neighborChunks[index].Blocks[position.X, position.Y, Chunk.Size.Z - 1].Type != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Z == -1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(0, -1) + this.Position);
                    if (index != -1)
                    {
                        if (neighborChunks[index].Blocks[position.X, position.Y, 0].Type != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal void Unload()
        {
            this.Mesh.Dispose();
            this.IsUnloaded = true;
        }

        private static bool IsOutOfRange(Vector3i position, Vector3i size)
        {
            if (position.X < 0 || position.Y < 0 || position.Z < 0)
            {
                return true;
            }
            else if (position.X > size.X - 1 || position.Y > size.Y - 1 || position.Z > size.Z - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
