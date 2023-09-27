using GameEngine.ModelLoading;
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

        internal static readonly Vector3i Size = new Vector3i(16, 256, 16);
        internal Block[,,] Blocks = new Block[Size.X, Size.Y, Size.Z];

        internal Mesh Mesh { get; private set; }

        internal unsafe Chunk(Vector2i position, Block[,,] blocks)
        {
            this.Position = position;
            this.Blocks = blocks;
            this.Mesh = Bake();
        }

        internal Mesh Bake()
        {
            List<Vertex> vertices = new List<Vertex>();
            Vertex vertexBuffer = new Vertex();

            foreach (Block block in Blocks)
            {
                if (block.Type != BlockType.Air)
                {
                    if (!IsBlockSideCovered(block, new Vector3i(0, 0, -1)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, 0, 1)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 6];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(-1, 0, 0)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 12];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(1, 0, 0)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i+ 18];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, -1, 0)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 24];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                    if (!IsBlockSideCovered(block, new Vector3i(0, 1, 0)))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            vertexBuffer = Block.Vertices[i + 30];
                            vertexBuffer.Position += block.Position;
                            vertexBuffer.TexCoords += block.GetTexCoordsByIndex();
                            vertices.Add(vertexBuffer);
                        }
                    }
                }
            }

            return new Mesh(vertices, new(), new());
        }

        internal bool IsBlockSideCovered(Block block, Vector3i offset)
        {
            Vector3i position = block.Position + offset + (Size / 2);
            if (!IsOutOfRange(position, Size))
            {
                if (Blocks[position.X, position.Y, position.Z].Type != BlockType.Air)
                {
                    return true;
                }
            }
            return false;
        }

        internal static Block[,,] GenerateChunk()
        {
            Block[,,] blocks = new Block[Size.X, Size.Y, Size.Z];
            Block bufferBlock = new Block();

            int height = 64 + (Size.Y / 2);

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
                        blocks[x, y, z] = (Block)bufferBlock.Clone();
                    }
                    for (int y = height + 1; y < Size.Y; y++)
                    {
                        bufferBlock.Position.X = x - (Size.X / 2);
                        bufferBlock.Position.Y = y - (Size.Y / 2);
                        bufferBlock.Position.Z = z - (Size.Z / 2);
                        bufferBlock.Type = BlockType.Air;
                        blocks[x, y, z] = (Block)bufferBlock.Clone();
                    }
                }
            }
            return blocks;
        }

        internal static bool IsOutOfRange(Vector3i position, Vector3i size)
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
