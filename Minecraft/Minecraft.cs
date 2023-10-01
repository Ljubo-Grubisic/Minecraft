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
    public class Minecraft : Game
    {
        internal Shader Shader { get; set; }

        internal Chunk Chunk { get; set; }

        private bool WireFrameMode = false;

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            
        }

        protected override void OnInit()
        {
            Block.Init();
            ChunkManager.Init();
            ChunkManager.BakeChunks();
        }

        protected override Camera OnCreateCamera()
        {
            return new Camera(new Vector3(0, 50, 4), (float)this.Size.X / (float)this.Size.Y, 45) { Speed = 5 };
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

            foreach (Chunk chunk in ChunkManager.Chunks)
            {
                GL.BindVertexArray(chunk.Mesh.VAO);

                Shader.SetMatrix("model", Matrix4.CreateTranslation(new Vector3(chunk.Position.X * Chunk.Size.X, 0.0f, chunk.Position.Y * Chunk.Size.Z)));

                GL.BindTexture(TextureTarget.Texture2D, Block.Texture.Handle);

                GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Count);

                GL.BindVertexArray(0);
            }
        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
