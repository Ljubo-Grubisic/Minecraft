using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rendering
{
    public class Camera : ICloneable
    {
        public Vector3 Position { get; set; }

        public float AspectRatio { get; set; }
        public float Speed { get; set; } = 2.5f;
        public float Sensitivity { get; set; } = 0.1f;
        public float MinViewDistance { get; set; } = 0.1f;
        public float MaxViewDistance { get; set; } = 100.0f;

        /// <summary>
        /// In radians
        /// </summary>
        private float fov;
        /// <summary>
        /// In radians
        /// </summary>
        private float zoom;

        public Vector3 Front { get; private set; } = -Vector3.UnitZ;
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; }

        /// <summary>
        /// In radians
        /// </summary>
        private float yaw = -MathHelper.PiOver2;
        /// <summary>
        /// In radians
        /// </summary>
        private float pitch = 0;

        /// <summary>
        /// In Degrees 
        /// </summary>
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set { fov = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 1, 120)); }
        }
        /// <summary>
        /// In Degrees 
        /// </summary>
        public float Zoom
        {
            get => MathHelper.RadiansToDegrees(zoom);
            set { zoom = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 1, Fov - 1)); }
        }

        /// <summary>
        /// In degress
        /// </summary>
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set { yaw = MathHelper.DegreesToRadians(value); }
        }
        /// <summary>
        /// In degress
        /// </summary>
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set 
            {
                if (value > 89)
                    pitch = MathHelper.DegreesToRadians(89);
                else if (value < -89)
                    pitch = MathHelper.DegreesToRadians(-89);
                else
                    pitch = MathHelper.DegreesToRadians(value);
            }
        }

        public Camera(Vector3 position, float aspectRatio, float fov = 90)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Fov = fov;
        }

        internal Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov - zoom, AspectRatio, MinViewDistance, MaxViewDistance);
        }

        internal Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        internal Matrix4 GetViewMatrix(Vector3 LookingAt)
        {
            return Matrix4.LookAt(Position, LookingAt, Up);
        }

        internal void UpdateKeys(MouseState mouse, KeyboardState keyboard, float time)
        {
            Yaw += MathHelper.Clamp(mouse.Delta.X * Sensitivity, -89, 89);
            Pitch += MathHelper.Clamp(-mouse.Delta.Y * Sensitivity, -89, 89);

            Vector3 Direction = new Vector3();
            Direction.X = MathF.Cos(yaw) * MathF.Cos(pitch);
            Direction.Y = MathF.Sin(pitch);
            Direction.Z = MathF.Sin(yaw) * MathF.Cos(pitch);

            Front = Vector3.Normalize(Direction);
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Cross(Right, Front);

            //Zoom += mouse.ScrollDelta.Y;

            if (keyboard.IsKeyDown(Keys.Space))
            {
                Position += new Vector3(0.0f, Speed * time, 0.0f);
            }
            if (keyboard.IsKeyDown(Keys.LeftControl))
            {
                Position -= new Vector3(0.0f, Speed * time, 0.0f);
            }
            if (keyboard.IsKeyDown(Keys.W))
            {
                Position += Speed * time * Front;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                Position -= Speed * time * Front;
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                Position -= Speed * time * Vector3.Normalize(Vector3.Cross(Front, Up));
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                Position += Speed * time * Vector3.Normalize(Vector3.Cross(Front, Up));
            }
        }

        public object Clone()
        {
            return new Camera(Position, AspectRatio, Fov);
        }
    }
}
