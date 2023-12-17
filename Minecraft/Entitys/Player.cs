using GameEngine;
using GameEngine.Rendering;
using Minecraft.System;
using Minecraft.WorldBuilding;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

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

        internal delegate void PlayerChangeBlockEventHandler(Player sender, PlayerChangeBlockEventArgs args);
        internal static event PlayerChangeBlockEventHandler PlayerChangeBlock;

        private BlockType InHand = BlockType.OakLog;

        internal Player(PlayerMovementType movementType)
        {
            MovementType = movementType;

            MouseManager.MouseWheel += MouseManager_MouseWheel;
        }

        internal void Update(Camera camera)
        {
            Position = camera.Position;

            if (MouseManager.OnButtonPressed(MouseButton.Left))
            {
                Vector3i? rayCastData = RayCaster.FindBlockLookingAt(camera.Position, camera.Front, (RenderDistance * ChunkColumn.ChunkSize) / 6);
                if (rayCastData != null)
                {
                    ChunkManager.ChangeBlock(new BlockStruct { Position = (Vector3i)rayCastData, Type = BlockType.Air }, true);
                    OnPlayerChangeBlock((Vector3i)rayCastData, BlockType.Air);
                }
            }
            if (MouseManager.OnButtonPressed(MouseButton.Right))
            {
                Vector3i? rayCastData = RayCaster.FindBlockLookingOut(camera.Position, camera.Front, (RenderDistance * ChunkColumn.ChunkSize) / 6);
                if (rayCastData != null)
                {
                    ChunkManager.ChangeBlock(new BlockStruct { Position = (Vector3i)rayCastData, Type = InHand }, true);
                    OnPlayerChangeBlock((Vector3i)rayCastData, InHand);
                }
            }
        }


        public object Clone()
        {
            return new Player(MovementType);
        }

        protected virtual void OnPlayerChangeBlock(Vector3i blockPosition,  BlockType type)
        {
            PlayerChangeBlock?.Invoke(this, new PlayerChangeBlockEventArgs(blockPosition, type));
        }

        private void MouseManager_MouseWheel(MouseWheelEventArgs args)
        {
            InHand = (BlockType)((int)InHand + (int)args.Offset.Y);
            if ((int)InHand <= 1)
            {
                InHand = (BlockType)2;
            }
            else if ((int)InHand >= (int)BlockType.TotalPlusOne - 1) 
            {
                InHand = (BlockType)(int)BlockType.TotalPlusOne - 1;
            }
            Console.Clear();
            Console.WriteLine(InHand);
        }
    }

    internal class PlayerChangeBlockEventArgs : EventArgs
    {
        public Vector3i BlockPosition { get; private set; }
        public BlockType Type { get; private set; }

        public PlayerChangeBlockEventArgs(Vector3i blockPosition, BlockType type) : base()
        {
            this.BlockPosition = blockPosition;
            this.Type = type;
        }
    }
}
