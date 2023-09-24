using OpenTK.Mathematics;

namespace Minecraft.WorldBuilding
{
    internal class Chunk
    {
        internal Vector2i Position {  get; private set; }
        internal Block[,,] Blocks = new Block[16, 126, 16];

        internal Chunk(Vector2i position, Block[,,] blocks)
        {
            Position = position;
            Blocks = blocks;
        }

        internal static Block[,,] GenerateChunk(Block block)
        {
            Block[,,] blocks = new Block[16, 126, 16];
            Block bufferBlock = new Block(new Vector3i(), block.Texture);

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 126; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        bufferBlock.Position.X = x - 8;
                        bufferBlock.Position.Y = y - 63;
                        bufferBlock.Position.Z = z - 8;
                        blocks[x, y, z] = (Block)bufferBlock.Clone();
                    }
                }
            }
            return blocks;
        }
    }
}
