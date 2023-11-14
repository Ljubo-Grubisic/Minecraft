using GameEngine.ModelLoading;
using Minecraft.System;
using OpenTK.Mathematics;
using System.Runtime.Serialization;

namespace Minecraft.WorldBuilding
{
    [DataContract]
    public class ChunkColumn
    {
        [DataMember]
        internal Vector2i Position { get; private set; }
        internal static int ChunkSize { get; } = 16;
        internal static int Height { get; } = 256;

        [DataMember]
        internal List<BlockStruct> BlocksChanged { get; private set; } = new List<BlockStruct>();
        private Dictionary<Vector3i, BlockType> Blocks = new Dictionary<Vector3i, BlockType>();
        private Dictionary<int, BlockType> Layers = new Dictionary<int, BlockType>();

        internal Mesh Mesh { get; private set; }

        internal bool IsUnloaded { get; private set; } = false;
        internal bool IsBaking { get; set; } = false;

        internal ChunkColumn(Vector2i position)
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

        internal void ChangeBlockType(BlockStruct blockStruct)
        {
            Blocks[blockStruct.Position] = blockStruct.Type;
        }

        internal void Unload()
        {
            this.IsUnloaded = true;
            SaveManager.SaveChunk(this);
            if (Mesh != null)
            {
                ActionManager.QueueAction(ActionManager.Thread.Main, () => this.Mesh.Dispose());
            }
        }

        internal void Bake(List<ChunkColumn> neighborChunks)
        {
            this.IsBaking = true;
            ThreadPool.QueueUserWorkItem((sender) =>
            {
                List<Vertex> vertices = new List<Vertex>();
                Vertex vertexBuffer = new Vertex();
                Vector3i index = new Vector3i();

                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            index.X = x; index.Y = y; index.Z = z;
                            BlockStruct block = new BlockStruct();

                            block.Type = this.GetBlockType(index);

                            block.Position.X = x - (ChunkSize / 2);
                            block.Position.Y = y - (Height / 2);
                            block.Position.Z = z - (ChunkSize / 2);

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

                ActionManager.QueueAction(ActionManager.Thread.Main, () =>
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

            List<BlockStruct> blocksChanged = SaveManager.LoadChunk(Position);
            this.BlocksChanged = blocksChanged;

            foreach (BlockStruct block in this.BlocksChanged)
            {
                this.Blocks[block.Position] = block.Type;
            }

            OptimizeBlocksToLayers();
        }

        public void OptimizeBlocksToLayers()
        {
            Vector3i index = new Vector3i();
            BlockType blockType = new BlockType();
            for (int y = 0; y < Height; y++)
            {
                bool firstBlock = true;
                bool isLayerSame = true;

                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int z = 0; z < ChunkSize; z++)
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

                    for (int x = 0; x < ChunkSize; x++)
                    {
                        for (int z = 0; z < ChunkSize; z++)
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

        private bool IsBlockSideCovered(BlockStruct block, Vector3i offset, List<ChunkColumn> neighborChunks)
        {
            Vector3i position = block.Position + offset + (new Vector3i(ChunkSize, Height, ChunkSize) / 2);
            BlockType blockType = new BlockType();

            if (!IsOutOfRange(position))
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
                        if (neighborChunks[index].GetBlockType(new Vector3i(ChunkSize - 1, position.Y, position.Z)) != BlockType.Air)
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
                        if (neighborChunks[index].GetBlockType(new Vector3i(position.X, position.Y, ChunkSize - 1)) != BlockType.Air)
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

        internal static bool IsOutOfRange(Vector3i position)
        {
            if (position.X < 0 || position.Y < 0 || position.Z < 0)
            {
                return true;
            }
            else if (position.X > ChunkSize - 1 || position.Y > Height - 1 || position.Z > ChunkSize - 1)
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
