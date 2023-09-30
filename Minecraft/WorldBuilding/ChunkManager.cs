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
                    Chunks.Add(new Chunk(new Vector2i(i - (SpawnChunkArea.X / 2), j - (SpawnChunkArea.X / 2)), Chunk.GenerateChunk()));
                }
            }
        }
    }
}
