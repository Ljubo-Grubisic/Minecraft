using GameEngine.ModelLoading;
using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal static int Size { get; } = 32;

        internal List<BlockStruct> BlocksChanged { get; private set; } = new List<BlockStruct>();
        private Dictionary<Vector3i, BlockType> Blocks = new Dictionary<Vector3i, BlockType>();
        private Dictionary<int, BlockType> Layers = new Dictionary<int, BlockType>();

        internal Mesh? Mesh { get; private set; }
        internal bool IsBaked { get; set; } = false;

        internal Chunk(Dictionary<Vector3i, BlockType> blocks, List<BlockStruct> blocksChanged)
        {
            this.Blocks = blocks;
            this.BlocksChanged = blocksChanged;

            foreach (BlockStruct block in this.BlocksChanged)
            {
                this.Blocks[block.Position] = block.Type;
            }

            OptimizeBlocksToLayers();
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
            this.IsBaked = false;
            if (Mesh != null)
            {
                ActionManager.QueueAction(() => this.Mesh.Dispose());
            }
        }

        internal void Bake(Dictionary<Vector3i, Chunk> neighborChunks)
        {
            ThreadPool.QueueUserWorkItem((sender) =>
            {
                List<Vertex> vertices = new List<Vertex>();
                Vertex vertexBuffer = new Vertex();
                Vector3i index = new Vector3i();

                for (int x = 0; x < Size; x++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        for (int y = 0; y < Size; y++)
                        {
                            index.X = x; index.Y = y; index.Z = z;
                            BlockStruct block = new BlockStruct();

                            block.Type = this.GetBlockType(index);

                            block.Position.X = x - (Size / 2);
                            block.Position.Y = y - (Size / 2);
                            block.Position.Z = z - (Size / 2);

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
                    this.IsBaked = true;
                });
            });
        }
        #endregion

        #region Private
        private void OptimizeBlocksToLayers()
        {
            Vector3i index = new Vector3i();
            BlockType blockType = new BlockType();
            for (int y = 0; y < Size; y++)
            {
                bool firstBlock = true;
                bool isLayerSame = true;

                for (int x = 0; x < Size; x++)
                {
                    for (int z = 0; z < Size; z++)
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

                    for (int x = 0; x < Size; x++)
                    {
                        for (int z = 0; z < Size; z++)
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

        private bool IsBlockSideCovered(BlockStruct block, Vector3i offset, Dictionary<Vector3i, Chunk> neighborChunks)
        {
            Vector3i position = block.Position + offset + (new Vector3i(Size) / 2);
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
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(Chunk.Size - 1, position.Y, position.Z)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.X == 1)
                {
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(0, position.Y, position.Z)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Z == -1)
                {
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(position.X, position.Y, Chunk.Size - 1)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Z == 1)
                {
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(position.X, position.Y, 0)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Y == -1)
                {
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(position.X, Chunk.Size - 1, position.Z)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
                else if (offset.Y == 1)
                {
                    Chunk? chunkBuffer = null;
                    if (neighborChunks.TryGetValue(offset, out chunkBuffer))
                    {
                        if (chunkBuffer.GetBlockType(new Vector3i(position.X, 0, position.Z)) != BlockType.Air)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsOutOfRange(Vector3i position, int size)
        {
            if (position.X < 0 || position.Y < 0 || position.Z < 0)
            {
                return true;
            }
            else if (position.X > size - 1 || position.Y > size - 1 || position.Z > size - 1)
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
