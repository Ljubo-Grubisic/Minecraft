using GameEngine;
using GameEngine.Rendering;
using Minecraft.System;
using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minecraft.Entitys
{
    internal enum PlayerMovementType
    {
        FreeCam,
        Realistic
    }

    internal class Player : ICloneable
    {
        internal int RenderDistance { get; set; } = 33;
        internal PlayerMovementType MovementType { get; set; }

        internal Vector3 Position { get; set; }

        private BlockType InHand = BlockType.OakLog;

        internal Player(PlayerMovementType movementType)
        {
            MovementType = movementType;
        }

        internal void Update(Camera camera)
        {
            Position = camera.Position;

            if (MouseManager.OnButtonPressed(MouseButton.Left))
            {
                (Vector2i ChunkPosition, Vector3i BlockPosition)? rayCastData = RayCaster.FindBlockLookingAt(camera.Position, camera.Front, (RenderDistance * ChunkColumn.ChunkSize) / 6);
                if (rayCastData != null)
                {
                    ChunkManager.ChangeBlock(rayCastData.Value.ChunkPosition, new BlockStruct { Position = rayCastData.Value.BlockPosition, Type = BlockType.Air }, true);
                }
            }
            if (MouseManager.OnButtonPressed(MouseButton.Right))
            {
                (Vector2i ChunkPosition, Vector3i BlockPosition)? rayCastData = RayCaster.FindBlockLookingOut(camera.Position, camera.Front, (RenderDistance * ChunkColumn.ChunkSize) / 6);
                if (rayCastData != null)
                {
                    ChunkManager.ChangeBlock(rayCastData.Value.ChunkPosition, new BlockStruct { Position = rayCastData.Value.BlockPosition, Type = this.InHand }, true);
                }
            }
        }

        public object Clone()
        {
            return new Player(MovementType);
        }
    }
}
