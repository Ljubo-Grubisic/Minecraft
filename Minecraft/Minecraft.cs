using GameEngine.MainLooping;
using GameEngine.ModelLoading;
using GameEngine.Rendering;
using GameEngine.Shadering;
using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace Minecraft
{
    public class Minecraft : Game
    {
        internal Shader Shader { get; set; }

        internal Chunk Chunk { get; set; }

        internal Block Block { get; set; }

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            
        }

        protected override void OnInit()
        {
            Block.Init();
        }

        protected override Camera OnCreateCamera()
        {
            return new Camera(new Vector3(0, 0, 4), (float)this.Size.X / (float)this.Size.Y, 45) { Speed = 5 };
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
            //Chunk = new Chunk(new Vector2i(), Chunk.GenerateChunk());
            Block.Init();
            Block = new Block(new(), BlockType.Stone);
        }

        
        protected override void OnUpdate(FrameEventArgs args)
        {
            Console.WriteLine(Math.Round(1 / args.Time));
        }
        protected override void OnRender(FrameEventArgs args, Matrix4 view, Matrix4 projection)
        {
            Shader.Use();
            Shader.SetMatrix("view", view);
            Shader.SetMatrix("projection", projection);

            //GL.BindVertexArray(Chunk.Mesh.VAO);  
            //
            //Shader.SetMatrix("model", Matrix4.CreateTranslation(new()));
            //
            //GL.BindTexture(TextureTarget.Texture2D, Block.Texture.Handle);
            //
            //GL.DrawArrays(PrimitiveType.Triangles, 0, Chunk.Mesh.Vertices.Count);
            //
            //GL.BindVertexArray(0);

            GL.BindVertexArray(Block.Vao);
            
            Shader.SetMatrix("model", Matrix4.CreateTranslation(Block.Position));
        
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            
            GL.BindVertexArray(0);
        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
