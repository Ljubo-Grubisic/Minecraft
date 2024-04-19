using GameEngine;
using GameEngine.MainLooping;
using GameEngine.Rendering;
using GameEngine.Shadering;
using Minecraft.Entitys;
using Minecraft.WorldBuilding;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Minecraft.System;
using System.Collections.Generic;

namespace Minecraft
{
    public class Minecraft : Game
    {
        internal Shader Shader { get; private set; }
        internal Shader HudShader { get; private set; }
        internal Player Player { get; private set; }

        private bool WireFrameMode = false;

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        protected override void OnInit()
        {
            Player = new Player(PlayerMovementType.FreeCam) { RenderDistance = 37 };
            Biome.Init();
            Block.Init();
            ChunkManager.Init();
        }

        protected override Camera OnCreateCamera()
        {
            return new Camera(new Vector3(21130, 150, -23230), this.Size.X / this.Size.Y) { MaxViewDistance = 1500.0f, Speed = 10f };
        }

        protected override void OnLoadShaders()
        {
            Shader = new Shader("Shaders/vertex_shader.glsl", "Shaders/fragment_shader.glsl");
            HudShader = new Shader("Shaders/vertex_shaderHUD.glsl", "Shaders/fragment_shaderHUD.glsl");
        }
        protected override void OnLoadTextures()
        {
        }
        protected override void OnLoadModels()
        {
            
        }

        protected override void OnUpdate(FrameEventArgs args)
        {
            //Console.WriteLine(Math.Round(1 / args.Time));

            ActionManager.InvokeActions(ActionManager.Thread.Main);

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
            if (KeyboardManager.OnKeyPressed(Keys.B))
            {
                Structure.SaveBulding = !Structure.SaveBulding;
            }
        }
        protected override void OnRender(FrameEventArgs args, Matrix4 view, Matrix4 projection)
        {
            Shader.Use();
            Shader.SetMatrix("view", view);
            Shader.SetMatrix("projection", projection);
            
            // Directional light
            Shader.SetVec3("dirLight.ambient", 0.55f, 0.55f, 0.55f);
            Shader.SetVec3("dirLight.diffuse", 0.65f, 0.65f, 0.65f);
            
            Shader.SetVec3("dirLight.direction", -0.2f, -1.0f, -0.3f);
            
            lock (ChunkManager.ChunksLoaded)
            {
                foreach (ChunkColumn chunk in ChunkManager.ChunksLoaded.Values.ToList())
                {
                    if (chunk.Mesh != null)
                    {
                        GL.BindVertexArray(chunk.Mesh.VAO);
            
                        Shader.SetMatrix("model", Matrix4.CreateTranslation(new Vector3(chunk.Position.X * ChunkColumn.ChunkSize, (ChunkColumn.Height / 2), chunk.Position.Y * ChunkColumn.ChunkSize)));
            
                        GL.BindTexture(TextureTarget.Texture2D, Block.Texture.Handle);
            
                        GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Count);
            
                        GL.BindVertexArray(0);
                    }
                }
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            ChunkManager.Unload();
        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
