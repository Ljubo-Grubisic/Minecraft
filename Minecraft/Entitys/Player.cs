using GameEngine.MainLooping;
using GameEngine.ModelLoading;
using GameEngine.Rendering;
using GameEngine.Shadering;
using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine;

namespace Minecraft.Entitys
{
    internal enum PlayerMovementType
    {
        FreeCam,
        Realistic
    }

    internal class Player : ICloneable
    {
        internal int RenderDistance { get; set; } = 36;
        internal PlayerMovementType MovementType { get; set; }

        internal Vector3 Position { get; set; }

        internal Player(PlayerMovementType movementType)
        {
            MovementType = movementType;
        }

        internal void Update(Camera camera)
        {
            Position = camera.Position;
        }

        public object Clone()
        {
            return new Player(MovementType);
        }
    }
}
