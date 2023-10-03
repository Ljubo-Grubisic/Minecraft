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

namespace Minecraft
{
    internal enum PlayerMovementType
    {
        FreeCam,
        Realistic
    }

    internal class Player : ICloneable
    {
        internal int ViewDistance { get; set; } = 16;
        internal PlayerMovementType MovementType { get; set; }

        internal Vector3 Position { get => Camera.Position; set => Camera.Position = value; }

        private Camera Camera { get; set; }

        internal Player(Camera camera, PlayerMovementType movementType)
        {
            Camera = camera;
            MovementType = movementType;
        }

        internal void Update(Camera camera)
        {
            this.Camera = camera;
        }

        public object Clone()
        {
            return new Player(Camera, MovementType);
        }
    }
}
