using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.WorldBuilding
{
    internal static class ChunkManager
    {
        internal static List<Chunk> Chunks = new List<Chunk>();

        private static Vector2i SpawnChunkArea = new Vector2i(16, 16);

        internal static void Init()
        {
            for (int i = 0; i < SpawnChunkArea.X; i++)
            {
                for (int j = 0; j < SpawnChunkArea.Y; j++)
                {
                    Chunks.Add(new Chunk(new Vector2i(i - (SpawnChunkArea.X / 2), j - (SpawnChunkArea.Y / 2)), Chunk.GenerateChunk()));
                }
            }
        }

        internal static void BakeChunks() 
        {
            List<int> index = new List<int>();
            List<Chunk> neighbors = new List<Chunk>();
            for (int i = 0; i < Chunks.Count; i++)
            {
                index.Clear();
                neighbors.Clear();

                index.Add(Chunk.IndexOfChunk(new Vector2i(-1, 0) + Chunks[i].Position, Chunks));
                index.Add(Chunk.IndexOfChunk(new Vector2i(1, 0) + Chunks[i].Position, Chunks));
                index.Add(Chunk.IndexOfChunk(new Vector2i(0, -1) + Chunks[i].Position, Chunks));
                index.Add(Chunk.IndexOfChunk(new Vector2i(0, 1) + Chunks[i].Position, Chunks));

                for (int j = 0; j < index.Count; j++)
                {
                    if (index[j] == -1)
                    {
                        neighbors.Add(null);
                    }
                    else
                    {
                        neighbors.Add(Chunks[index[j]]);
                    }
                }

                Chunks[i].Bake(neighbors);
            }
        }
    }
}
