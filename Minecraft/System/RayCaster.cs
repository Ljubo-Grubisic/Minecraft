using Minecraft.WorldBuilding;
using OpenTK.Mathematics;

namespace Minecraft.System
{
    internal static class RayCaster
    {
        internal static Vector3i? FindBlockLookingAt(Vector3 cameraPosition, Vector3 cameraDirection, float MaxDistance)
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

                (Vector2i worldChunkPosition, Vector3i localBlockPosition) = ChunkColumn.ConvertWorldBlockPositionToLocal(new Vector3i(worldX, y, worldZ));

                // Check if the coordinates are within the bounds of your world
                if (!ChunkColumn.IsOutOfRange(new Vector3i(localBlockPosition.X, y, localBlockPosition.Z)))
                {
                    if (ChunkManager.TryGetChunkColumn(new Vector2i(worldChunkPosition.X, worldChunkPosition.Y), out ChunkColumn? chunkColumn))
                    {
                        BlockType blockType = chunkColumn.GetBlockType(new Vector3i(localBlockPosition.X, y, localBlockPosition.Z));
                        if (Block.GetBlockVisibility(blockType) != BlockVisibility.Transparent)
                            return new Vector3i(worldX, y, worldZ);
                    }
                }
                else
                {
                    throw new Exception("Block out of range");
                }
            }

            return null;
        }

        internal static Vector3i? FindBlockLookingOut(Vector3 cameraPosition, Vector3 cameraDirection, float MaxDistance)
        {
            float rayCastStep = 0.2f;

            // Get the camera's forward vector
            Vector3 cameraForward = cameraDirection;

            // Set up the ray with the camera position as the starting point
            Vector3 rayStart = cameraPosition;

            // Iterate along the ray in steps
            int? previousWorldX = null, previousY = null, previousWorldZ = null;

            for (float distance = 0; distance < MaxDistance; distance += rayCastStep)
            {
                // Calculate the current position along the ray
                Vector3 currentPos = rayStart + distance * cameraForward;

                // Convert the current position to block coordinates
                int worldX = (int)Math.Round(currentPos.X);
                int worldZ = (int)Math.Round(currentPos.Z);
                int y = (int)Math.Round(currentPos.Y);

                (Vector2i worldChunkPosition, Vector3i localBlockPosition) = ChunkColumn.ConvertWorldBlockPositionToLocal(new Vector3i(worldX, y, worldZ));

                // Check if the coordinates are within the bounds of your world
                if (!ChunkColumn.IsOutOfRange(new Vector3i(localBlockPosition.X, y, localBlockPosition.Z)))
                {
                    if (ChunkManager.TryGetChunkColumn(new Vector2i(worldChunkPosition.X, worldChunkPosition.Y), out ChunkColumn? chunkColumn))
                    {
                        BlockType blockType = chunkColumn.GetBlockType(new Vector3i(localBlockPosition.X, y, localBlockPosition.Z));
                        if (Block.GetBlockVisibility(blockType) != BlockVisibility.Transparent)
                        {
                            if (previousWorldX != null && previousY != null && previousWorldZ != null)
                                return new Vector3i((int)previousWorldX, (int)previousY, (int)previousWorldZ);
                            else
                                return null;
                        }
                    }
                }
                else
                {
                    throw new Exception("Block out of range");
                }

                previousWorldX = worldX;
                previousY = y;
                previousWorldZ = worldZ;
            }

            return null;
        }
    }
}
