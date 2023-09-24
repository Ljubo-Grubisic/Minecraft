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

        internal Texture DirtTexture {  get; set; }
        internal Block DirtBlock { get; set; }

        internal Chunk Chunk { get; set; }

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            
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
            DirtTexture = Texture.LoadFromFile("Resources/Textures/dirt.png");
        }
        protected override void OnLoadModels()
        {
            Block.InitBlocks(Shader);
            DirtBlock = new Block(new Vector3i(0, 0, 0), DirtTexture);

            Chunk = new Chunk(new Vector2i(), Chunk.GenerateChunk(DirtBlock));
        }

        
        protected override void OnUpdate(FrameEventArgs args)
        {
        }
        protected override void OnRender(FrameEventArgs args, Matrix4 view, Matrix4 projection)
        {
            Shader.Use();
            Shader.SetMatrix("view", view);
            Shader.SetMatrix("projection", projection);

            GL.BindVertexArray(Block.Vao);

            foreach (Block block in Chunk.Blocks)
            {
                Shader.SetMatrix("model", Matrix4.CreateTranslation(block.Position));

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }

            GL.BindVertexArray(0);
        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
