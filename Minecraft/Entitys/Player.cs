using GameEngine.Rendering;
using OpenTK.Mathematics;

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
