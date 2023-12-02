using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using GameEngine.ModelLoading;
using GameEngine.Rendering;

namespace GameEngine.MainLooping
{
    public abstract partial class Game : GameWindow
    {
        protected Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override unsafe void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);
            this.CursorState = CursorState.Grabbed;
            KeyboardManager.Init(this);
            MouseManager.Init(this);

            OnInit();
            this.Camera = OnCreateCamera();
            OnLoadShaders();
            OnLoadTextures();
            OnLoadModels();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            Camera.UpdateKeys(MouseState, KeyboardState, (float)args.Time);

            OnUpdate(args);

            KeyboardManager.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            Clear();

            OnRender(args, Camera.GetViewMatrix(), Camera.GetProjectionMatrix());

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            Camera.AspectRatio = Size.X / (float)Size.Y;
            OnWindowResize(e);
        }

        private void Clear()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
