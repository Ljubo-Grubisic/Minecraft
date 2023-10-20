using GameEngine.ModelLoading;
using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal Vector2i Position { get; private set; }

        internal static readonly Vector3i Size = new Vector3i(16, 128, 16);
        internal BlockType[,,] Blocks = new BlockType[Size.X, Size.Y, Size.Z];

        internal Mesh Mesh { get; private set; }

        internal bool IsUnloaded { get; private set; } = false;
        internal bool IsBaking { get; set; } = false;

        internal Chunk(Vector2i position)
        {
            this.Position = position;
            GenerateChunk();
        }

        internal void GenerateChunk()
        {
            BlockType bufferBlock = new BlockType();

            int height = 32 + (Size.Y / 2);

            for (int x = 0; x < Size.X; x++)
            {
                for (int z = 0; z < Size.Z; z++)
                {
                    for (int y = 0; y <= height; y++)
                    {
                        if (y == height)
                            this.Blocks[x, y, z] = BlockType.Grass;
                        else if (y < height && y > height - 5)
                            this.Blocks[x, y, z] = BlockType.Dirt;
                        else
                            this.Blocks[x, y, z] = BlockType.Stone;
                    }
                    for (int y = height + 1; y < Size.Y; y++)
                    {
                        this.Blocks[x, y, z] = BlockType.Air;
                    }
                }
            }
        }

        internal void Bake(List<Chunk> neighborChunks)
        {
            this.IsBaking = true;
            ThreadPool.QueueUserWorkItem((sender) =>
            {
                List<Vertex> vertices = new List<Vertex>();
                Vertex vertexBuffer = new Vertex();

                for (int x = 0; x < Size.X; x++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        for (int y = 0; y < Size.Y; y++)
                        {
                            BlockStruct block = new BlockStruct();

                            block.Type = Blocks[x, y, z];
                            block.Position.X = x - (Size.X / 2);
                            block.Position.Y = y - (Size.Y / 2);
                            block.Position.Z = z - (Size.Z / 2);

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
                                        vertexBuffer = Block.Vertices[i + 18];
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
                    }
                }

                ActionManager.QueueAction(() =>
                {
                    Mesh mesh = new Mesh(vertices, new(), new());
                    if (this.Mesh != null)
                        this.Mesh.Dispose();
                    this.Mesh = mesh;
                    this.IsBaking = false;
                });
            });
        }
        internal bool IsBlockSideCovered(BlockStruct block, Vector3i offset, List<Chunk> neighborChunks)
        {
            Vector3i position = block.Position + offset + (Size / 2);
            if (!IsOutOfRange(position, Size))
            {
                if (Blocks[position.X, position.Y, position.Z] != BlockType.Air)
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
                        if (neighborChunks[index].Blocks[Chunk.Size.X - 1, position.Y, position.Z] != BlockType.Air)
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
                        if (neighborChunks[index].Blocks[0, position.Y, position.Z] != BlockType.Air)
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
                        if (neighborChunks[index].Blocks[position.X, position.Y, Chunk.Size.Z - 1] != BlockType.Air)
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
                        if (neighborChunks[index].Blocks[position.X, position.Y, 0] != BlockType.Air)
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
            if (Mesh != null)
                ActionManager.QueueAction(() => this.Mesh.Dispose());
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
