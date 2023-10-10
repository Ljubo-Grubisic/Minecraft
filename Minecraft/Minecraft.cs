using GameEngine;
using GameEngine.MainLooping;
using GameEngine.Rendering;
using GameEngine.Shadering;
using Minecraft.WorldBuilding;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine.ModelLoading;

namespace Minecraft
{
    public class Minecraft : Game
    {
        internal Shader Shader { get; set; }
        internal Player Player { get; set; }

        private bool WireFrameMode = false;

        private Queue<Action> Actions { get; set; } = new Queue<Action>();

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        internal void QueueOperation(Action operation)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                bool running = true;
                while (running)
                {
                    lock (Actions)
                    {
                        Actions.Enqueue(operation);
                        running = false;
                        break;
                    }
                }
            });
        }

        protected override void OnInit()
        {
            Block.Init();
            Player = new Player(PlayerMovementType.FreeCam);
            ChunkManager.Init();
        }

        protected override Camera OnCreateCamera()
        {
            return new Camera(new Vector3(0, 50.0f, 0), this.Size.X / this.Size.Y) { MaxViewDistance = 500.0f, Speed = 100f };
        }

        protected override void OnLoadShaders()
        {
            Shader = new Shader("Shaders/vertex_shader.glsl", "Shaders/fragment_shader.glsl");
        }
        protected override void OnLoadTextures()
        {
        }
        protected override void OnLoadModels()
        {
        }

        protected override void OnUpdate(FrameEventArgs args)
        {
            Console.WriteLine(Math.Round(1 / args.Time));
            //Console.WriteLine(Camera.Position);

            lock (Actions)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    lock (ChunkManager.ChunksLoaded)
                        Actions.Dequeue().Invoke();
                }
            }

            Player.Update(Camera);

            if (KeyboardManager.OnKeyPressed(Keys.P))
            {
                if (WireFrameMode)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    WireFrameMode = false;
                }
                else
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    WireFrameMode = true;
                }
            }

        }
        protected override void OnRender(FrameEventArgs args, Matrix4 view, Matrix4 projection)
        {
            Shader.Use();
            Shader.SetMatrix("view", view);
            Shader.SetMatrix("projection", projection);

            lock (ChunkManager.ChunksLoaded)
            {
                foreach (Chunk chunk in ChunkManager.ChunksLoaded)
                {
                    GL.BindVertexArray(chunk.Mesh.VAO);

                    Shader.SetMatrix("model", Matrix4.CreateTranslation(new Vector3(chunk.Position.X * Chunk.Size.X, 0.0f, chunk.Position.Y * Chunk.Size.Z)));

                    GL.BindTexture(TextureTarget.Texture2D, Block.Texture.Handle);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Count);

                    GL.BindVertexArray(0);
                }
            }

        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
