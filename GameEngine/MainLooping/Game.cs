using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using GameEngine.ModelLoading;
using GameEngine.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace GameEngine.MainLooping
{
    public abstract partial class Game
    {
        protected abstract void OnInit();
        protected abstract Camera OnCreateCamera();
        protected abstract void OnLoadShaders();
        protected abstract void OnLoadTextures();
        protected abstract void OnLoadModels();

        protected abstract void OnUpdate(FrameEventArgs args);
        protected abstract void OnRender(FrameEventArgs args, Matrix4 view, Matrix4 projection);

        protected abstract void OnWindowResize(ResizeEventArgs args);

        public static unsafe Window CreateWindow(string title, int width, int height)
        {
            GameWindowSettings gws = GameWindowSettings.Default;
            NativeWindowSettings nws = NativeWindowSettings.Default;

            nws.APIVersion = Version.Parse("4.2.0");
            nws.Size = new Vector2i(width, height);
            nws.Title = title;
            nws.StartFocused = true;
            nws.StartVisible = true;

            Monitor* monitor = GLFW.GetPrimaryMonitor();
            int x, y, monitorWidth, monitorHeight;
            GLFW.GetMonitorWorkarea(monitor, out x, out y, out monitorWidth, out monitorHeight);
            x = (monitorWidth - width) / 2;
            y = (monitorHeight - height) / 2;

            nws.Location = new Vector2i(x, y);

            Window Window = new Window(gws, nws);
            return Window;
        }

        public class Window
        {
            public GameWindowSettings GameWindowSettings { get; private set; }
            public NativeWindowSettings NativeWindowSettings { get; private set; }

            public Window(GameWindowSettings gws, NativeWindowSettings nws)
            {
                this.GameWindowSettings = gws;
                this.NativeWindowSettings = nws;
            }
        }
    }
}
