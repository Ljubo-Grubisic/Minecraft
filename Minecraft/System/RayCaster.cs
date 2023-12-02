using Minecraft.WorldBuilding;
using OpenTK.Mathematics;

namespace Minecraft.System
{
    internal static class RayCaster
    {
        internal static (Vector2i ChunkPosition, Vector3i BlockPosition)? FindBlockLookingAt(Vector3 cameraPosition, Vector3 cameraDirection, float MaxDistance)
        {
            float rayCastStep = 0.2f;

            // Get the camera's forward vector
            Vector3 cameraForward = cameraDirection;

            // Set up the ray with the camera position as the starting point
            Vector3 rayStart = cameraPosition;

            // Iterate along the ray in steps
            for (float distance = 0; distance < MaxDistance; distance += rayCastStep)
            {
                // Calculate the current position along the ray
                Vector3 currentPos = rayStart + distance * cameraForward;

                // Convert the current position to block coordinates
                int worldX = (int)Math.Round(currentPos.X);
                int worldZ = (int)Math.Round(currentPos.Z);
                int y = (int)Math.Round(currentPos.Y);

                int worldChunkColumnX = worldX / ChunkColumn.ChunkSize;
                int worldChunkColumnZ = worldZ / ChunkColumn.ChunkSize;

                int chunkColumnX, chunkColumnZ;
                chunkColumnX = worldX % ChunkColumn.ChunkSize;
                chunkColumnZ = worldZ % ChunkColumn.ChunkSize;

                chunkColumnX += ChunkColumn.ChunkSize / 2;
                chunkColumnZ += ChunkColumn.ChunkSize / 2;

                if (chunkColumnX < 0)
                {
                    chunkColumnX += ChunkColumn.ChunkSize;
                    worldChunkColumnX--;
                }
                if (chunkColumnZ < 0)
                {
                    chunkColumnZ += ChunkColumn.ChunkSize;
                    worldChunkColumnZ--;
                }

                if (chunkColumnX > ChunkColumn.ChunkSize - 1)
                {
                    chunkColumnX -= ChunkColumn.ChunkSize;
                    worldChunkColumnX++;
                }
                if (chunkColumnZ > ChunkColumn.ChunkSize - 1)
                {
                    chunkColumnZ -= ChunkColumn.ChunkSize;
                    worldChunkColumnZ++;
                }

                // Check if the coordinates are within the bounds of your world
                if (!ChunkColumn.IsOutOfRange(new Vector3i(chunkColumnX, y, chunkColumnZ)))
                {
                    if (ChunkManager.TryGetChunkColumn(new Vector2i(worldChunkColumnX, worldChunkColumnZ), out ChunkColumn? chunkColumn))
                    {
                        BlockType blockType = chunkColumn.GetBlockType(new Vector3i(chunkColumnX, y, chunkColumnZ));
                        if (Block.GetBlockVisibility(blockType) != BlockVisibility.Transparent)
                            return (new Vector2i(worldChunkColumnX, worldChunkColumnZ), new Vector3i(chunkColumnX, y, chunkColumnZ));
                    }
                }
                else
                {
                    throw new Exception("Block out of range");
                }
            }

            return null;
        }

        internal static (Vector2i ChunkPosition, Vector3i BlockPosition)? FindBlockLookingOut(Vector3 cameraPosition, Vector3 cameraDirection, float MaxDistance)
        {
            float rayCastStep = 0.2f;

            // Get the camera's forward vector
            Vector3 cameraForward = cameraDirection;

            // Set up the ray with the camera position as the starting point
            Vector3 rayStart = cameraPosition;

            // Iterate along the ray in steps
            int? previousWorldChunkColumnX = null, previousWorldChunkColumnZ = null, previousChunkColumnX = null, previousChunkColumnZ = null, previousY = null;

            for (float distance = 0; distance < MaxDistance; distance += rayCastStep)
            {
                // Calculate the current position along the ray
                Vector3 currentPos = rayStart + distance * cameraForward;

                // Convert the current position to block coordinates
                int worldX = (int)Math.Round(currentPos.X);
                int worldZ = (int)Math.Round(currentPos.Z);
                int y = (int)Math.Round(currentPos.Y);

                int worldChunkColumnX = worldX / ChunkColumn.ChunkSize;
                int worldChunkColumnZ = worldZ / ChunkColumn.ChunkSize;

                int chunkColumnX, chunkColumnZ;
                chunkColumnX = worldX % ChunkColumn.ChunkSize;
                chunkColumnZ = worldZ % ChunkColumn.ChunkSize;

                chunkColumnX += ChunkColumn.ChunkSize / 2;
                chunkColumnZ += ChunkColumn.ChunkSize / 2;

                if (chunkColumnX < 0)
                {
                    chunkColumnX += ChunkColumn.ChunkSize;
                    worldChunkColumnX--;
                }
                if (chunkColumnZ < 0)
                {
                    chunkColumnZ += ChunkColumn.ChunkSize;
                    worldChunkColumnZ--;
                }

                if (chunkColumnX > ChunkColumn.ChunkSize - 1)
                {
                    chunkColumnX -= ChunkColumn.ChunkSize;
                    worldChunkColumnX++;
                }
                if (chunkColumnZ > ChunkColumn.ChunkSize - 1)
                {
                    chunkColumnZ -= ChunkColumn.ChunkSize;
                    worldChunkColumnZ++;
                }

                // Check if the coordinates are within the bounds of your world
                if (!ChunkColumn.IsOutOfRange(new Vector3i(chunkColumnX, y, chunkColumnZ)))
                {
                    if (ChunkManager.TryGetChunkColumn(new Vector2i(worldChunkColumnX, worldChunkColumnZ), out ChunkColumn? chunkColumn))
                    {
                        BlockType blockType = chunkColumn.GetBlockType(new Vector3i(chunkColumnX, y, chunkColumnZ));
                        if (Block.GetBlockVisibility(blockType) != BlockVisibility.Transparent)
                        {
                            if (previousWorldChunkColumnX != null && previousWorldChunkColumnZ != null && previousChunkColumnX != null && previousChunkColumnZ != null && previousY != null)
                                return (new Vector2i((int)previousWorldChunkColumnX, (int)previousWorldChunkColumnZ), new Vector3i((int)previousChunkColumnX, (int)previousY, (int)previousChunkColumnZ));
                            else
                                return null;
                        }
                    }
                }
                else
                {
                    throw new Exception("Block out of range");
                }

                previousWorldChunkColumnX = worldChunkColumnX;
                previousWorldChunkColumnZ = worldChunkColumnZ;
                previousChunkColumnX = chunkColumnX;
                previousChunkColumnZ = chunkColumnZ;
                previousY = y;
            }

            return null;
        }
    }
}
