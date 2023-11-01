using Minecraft.System;
using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal class ChunkColumn
    {
        internal Vector2i Position { get; private set; }
        internal static int Height = 256;

        internal Chunk[] Chunks { get; private set; } = new Chunk[Height / Chunk.Size];

        internal bool IsUnloaded { get; private set; } = false;
        internal bool IsBaked
        {
            get
            {
                foreach (Chunk chunk in Chunks)
                {
                    if (!chunk.IsBaked)
                        return false;
                    else return true;
                }
                throw new Exception("No ChunksLoaded");
            }
        }


        internal ChunkColumn(Vector2i position)
        {
            this.Position = position;
            Generate();
        }

        #region Public
        internal void Bake(List<ChunkColumn> neigborChunks)
        {
            for (int i = 0; i < Chunks.Length; i++)
            {
                Chunks[i].Bake(FindNeigbors(i, neigborChunks));
            }
        }

        internal void Unload()
        {
            this.IsUnloaded = true;
            foreach (Chunk chunk in Chunks)
            {
                chunk.Unload();
            }
        }
        #endregion

        #region Private
        private void Generate()
        {
            List<BlockStruct>? blocksChanged = SaveManager.LoadChunkColumn(this.Position);
            Dictionary<Vector3i, BlockType> blocks = WorldGenerator.GenerateChunkColumn(this.Position);

            if (blocksChanged != null)
            {
                for (int i = 0; i < Chunks.Length; i++)
                {
                    this.Chunks[i] = new Chunk(SeparateBlocks(blocks, i), SeparateBlockStructs(blocksChanged, i));
                }
            }
            else
            {
                for (int i = 0; i < Chunks.Length; i++)
                {
                    this.Chunks[i] = new Chunk(SeparateBlocks(blocks, i), new List<BlockStruct>());
                }
            }
        }

        private Dictionary<Vector3i, BlockType> SeparateBlocks(Dictionary<Vector3i, BlockType> blocks, int chunkIndex)
        {
            Dictionary<Vector3i, BlockType> result = new Dictionary<Vector3i, BlockType>();

            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int y = chunkIndex * Chunk.Size; y < chunkIndex * Chunk.Size + Chunk.Size; y++)
                {
                    for (int z = 0; z < Chunk.Size; z++)
                    {
                        result.Add(new Vector3i(x, y - chunkIndex * Chunk.Size, z), blocks[new Vector3i(x, y, z)]);
                    }
                }
            }
            return result;
        }

        private List<BlockStruct> SeparateBlockStructs(List<BlockStruct> blockStructs, int chunkIndex)
        {
            List<BlockStruct> result = new List<BlockStruct>();

            for (int i = 0; i < blockStructs.Count; i++)
            {
                if (blockStructs[i].Position.Y > chunkIndex * Chunk.Size && blockStructs[i].Position.Y < (chunkIndex * Chunk.Size) + Chunk.Size)
                {
                    result.Add(blockStructs[i]);
                }
            }
            return result;
        }

        private Dictionary<Vector3i, Chunk> FindNeigbors(int chunkIndex, List<ChunkColumn> chunkColumns)
        {
            Dictionary<Vector3i, Chunk> chunks = new Dictionary<Vector3i, Chunk>();
            if (chunkIndex < this.Chunks.Length - 1)
                chunks.Add(new Vector3i(0, 1, 0), Chunks[chunkIndex + 1]);
            if (chunkIndex > 0)
                chunks.Add(new Vector3i(0, -1, 0), Chunks[chunkIndex - 1]);

            {
                int index = chunkColumns.IndexOf(new Vector2i(1, 0) + this.Position);

                if (index != -1)
                {
                    chunks.Add(new Vector3i(1, 0, 0), chunkColumns[index].Chunks[chunkIndex]);
                }
            }
            {
                int index = chunkColumns.IndexOf(new Vector2i(-1, 0) + this.Position);

                if (index != -1)
                {
                    chunks.Add(new Vector3i(-1, 0, 0), chunkColumns[index].Chunks[chunkIndex]);
                }
            }
            {
                int index = chunkColumns.IndexOf(new Vector2i(0, 1) + this.Position);

                if (index != -1)
                {
                    chunks.Add(new Vector3i(0, 0, 1), chunkColumns[index].Chunks[chunkIndex]);
                }
            }
            {
                int index = chunkColumns.IndexOf(new Vector2i(0, -1) + this.Position);

                if (index != -1)
                {
                    chunks.Add(new Vector3i(0, 0, -1), chunkColumns[index].Chunks[chunkIndex]);
                }
            }
            return chunks;
        }
        #endregion
    }
}
