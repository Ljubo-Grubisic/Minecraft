using GameEngine.ModelLoading;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal Vector2i Position { get; private set; }

        internal static readonly Vector3i Size = new Vector3i(16, 256, 16);

        internal List<BlockStruct> BlocksChanged { get; private set; } = new List<BlockStruct>();
        private Dictionary<Vector3i, BlockType> Blocks = new Dictionary<Vector3i, BlockType>();
        private Dictionary<int, BlockType> Layers = new Dictionary<int, BlockType>();

        internal Mesh Mesh { get; private set; }

        internal bool IsUnloaded { get; private set; } = false;
        internal bool IsBaking { get; set; } = false;

        internal Chunk(Vector2i position)
        {
            this.Position = position;
            Generate();
        }

        #region Public
        internal BlockType GetBlockType(Vector3i position)
        {
            BlockType blockType;
            if (Blocks.TryGetValue(position, out blockType))
                return blockType;
            else if (Layers.TryGetValue(position.Y, out blockType))
                return blockType;
            else
                throw new IndexOutOfRangeException("Index out of range");
        }

        internal void Unload()
        {
            this.IsUnloaded = true;
            SaveManager.SaveChunk(this);
            if (Mesh != null)
            {
                ActionManager.QueueAction(() => this.Mesh.Dispose());
            }
        }

        internal void Bake(List<Chunk> neighborChunks)
        {
            this.IsBaking = true;
            ThreadPool.QueueUserWorkItem((sender) =>
            {
                List<Vertex> vertices = new List<Vertex>();
                Vertex vertexBuffer = new Vertex();
                Vector3i index = new Vector3i();

                for (int x = 0; x < Size.X; x++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        for (int y = 0; y < Size.Y; y++)
                        {
                            index.X = x; index.Y = y; index.Z = z;
                            BlockStruct block = new BlockStruct();

                            block.Type = this.GetBlockType(index);

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
                                        vertexBuffer.TexCoords += Block.GetTexCoordsOffset(block.Type);
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
        #endregion

        #region Private
        private void Generate()
        {
            this.Blocks = WorldGenerator.GenerateChunk(this);

            List<BlockStruct>? blocksChanged = SaveManager.LoadChunk(Position);
            if (blocksChanged != null)
                this.BlocksChanged = blocksChanged;

            foreach (BlockStruct block in this.BlocksChanged)
            {
                this.Blocks[block.Position] = block.Type;
            }

            OptimizeBlocksToLayers();
        }

        private void OptimizeBlocksToLayers()
        {
            Vector3i index = new Vector3i();
            BlockType blockType = new BlockType();
            for (int y = 0; y < Size.Y; y++)
            {
                bool firstBlock = true;
                bool isLayerSame = true;

                for (int x = 0; x < Size.X; x++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        index.X = x; index.Y = y; index.Z = z;
                        if (firstBlock)
                        {
                            blockType = Blocks[index];
                            firstBlock = false;
                        }
                        if (blockType != Blocks[index])
                        {
                            isLayerSame = false;
                        }
                    }
                }

                if (isLayerSame)
                {
                    this.Layers.Add(y, blockType);

                    for (int x = 0; x < Size.X; x++)
                    {
                        for (int z = 0; z < Size.Z; z++)
                        {
                            index.X = x; index.Y = y; index.Z = z;

                            this.Blocks.Remove(index);
                        }
                    }
                }
            }

            this.Blocks.TrimExcess();
            this.Layers.TrimExcess();
        }

        private bool IsBlockSideCovered(BlockStruct block, Vector3i offset, List<Chunk> neighborChunks)
        {
            Vector3i position = block.Position + offset + (Size / 2);
            BlockType blockType = new BlockType();

            if (!IsOutOfRange(position, Size))
            {
                if (this.GetBlockType(position) != BlockType.Air)
                {
                    return true;
                }
            }
            else
            {
                if (offset.X == -1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(-1, 0) + this.Position);

                    if (index != -1)
                    {
                        if (neighborChunks[index].GetBlockType(new Vector3i(Chunk.Size.X - 1, position.Y, position.Z)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.X == 1)
                {
                    int index = neighborChunks.IndexOf(new Vector2i(1, 0) + this.Position);
                    if (index != -1)
                    {
                        if (neighborChunks[index].GetBlockType(new Vector3i(0, position.Y, position.Z)) != BlockType.Air)
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
                        if (neighborChunks[index].GetBlockType(new Vector3i(position.X, position.Y, Chunk.Size.Z - 1)) != BlockType.Air)
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
                        if (neighborChunks[index].GetBlockType(new Vector3i(position.X, position.Y, 0)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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
        #endregion
    }
}
