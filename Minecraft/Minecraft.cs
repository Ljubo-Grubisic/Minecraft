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
using BenchmarkDotNet.Running;

namespace Minecraft
{
    public class Minecraft : Game
    {
        internal Shader Shader { get; set; }
        internal Player Player { get; set; }

        private bool WireFrameMode = false;

        public Minecraft(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {

        }


        protected override void OnInit()
        {
            Block.Init();
            ChunkManager.Init();
            Player = new Player(PlayerMovementType.FreeCam) { RenderDistance = 65 };
        }

        protected override Camera OnCreateCamera()
        {
            return new Camera(new Vector3(0, 50.0f, 0), this.Size.X / this.Size.Y) { MaxViewDistance = 1500.0f, Speed = 100f };
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
            //Console.WriteLine(Math.Round(1 / args.Time));

            ActionManager.InvokeActions();

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

                        Shader.SetMatrix("model", Matrix4.CreateTranslation(new Vector3(chunk.Position.X * ChunkColumn.ChunkSize, 0.0f, chunk.Position.Y * ChunkColumn.ChunkSize)));

                        GL.BindTexture(TextureTarget.Texture2D, Block.Texture.Handle);

                        GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Count);

                        GL.BindVertexArray(0);
                    }
                }
            }

        }

        protected override void OnWindowResize(ResizeEventArgs args)
        {
        }
    }
}
