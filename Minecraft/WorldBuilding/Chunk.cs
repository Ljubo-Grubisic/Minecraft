using GameEngine.ModelLoading;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using GameEngine.Shadering;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal Vector2i Position {  get; private set; }

        internal static readonly Vector3i Size = new Vector3i(16, 128, 16);
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
            List<Texture> textures = new List<Texture>();
            Vertex vertexBuffer = new Vertex();

            foreach (Block block in Blocks)
            {
                if (!IsBlockSideCovered(block, new Vector3i(0, 0, -1)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
                if (!IsBlockSideCovered(block, new Vector3i(0, 0, 1)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i + 6];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
                if (!IsBlockSideCovered(block, new Vector3i(-1, 0, 0)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i + 12];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
                if (!IsBlockSideCovered(block, new Vector3i(1, 0, 0)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i+ 18];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
                if (!IsBlockSideCovered(block, new Vector3i(0, -1, 0)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i + 24];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
                if (!IsBlockSideCovered(block, new Vector3i(0, 1, -1)))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        vertexBuffer = Block.Vertices[i + 30];
                        vertexBuffer.Position += block.Position;
                        vertices.Add(vertexBuffer);
                    }
                }
            }

            return new Mesh(vertices, new(), textures);
        }

        internal bool IsBlockSideCovered(Block block, Vector3i offset)
        {
            if (block != null)
            {
                Vector3i position = block.Position + offset + (Size / 2);
                if (!IsOutOfRange(position, Size))
                {
                    if (Blocks[position.X, position.Y, position.Z] != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        internal static Block[,,] GenerateChunk(Block block)
        {
            Block[,,] blocks = new Block[Size.X, Size.Y, Size.Z];
            Block bufferBlock = new Block(new Vector3i(), block.Texture);

            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        bufferBlock.Position.X = x - (Size.X / 2);
                        bufferBlock.Position.Y = y - (Size.Y / 2);
                        bufferBlock.Position.Z = z - (Size.Z / 2);
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
