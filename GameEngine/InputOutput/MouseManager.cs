using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Button = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTK.Windowing.Common;

namespace GameEngine
{
    public class MouseManager
    {
        public static Vector2i MouseOffSet = new Vector2i(8, 30);
        private static bool[] KeyHandlers = new bool[(int)Button.Last];

        private static NativeWindow Window;

        public delegate void MouseWheelManager(MouseWheelEventArgs args);
        public static event MouseWheelManager MouseWheel;

        public static void Init(NativeWindow window)
        {
            Window = window;
            Window.MouseWheel += Window_MouseWheel;
        }

        private static void Window_MouseWheel(MouseWheelEventArgs obj)
        {
            MouseWheel?.Invoke(obj);
        }

        public static bool OnButtonPressed(Button button)
        {
            if (!IsButtonDown(button))
            {
                KeyHandlers[(int)button] = true;
                return false;
            }
            else if (IsButtonDown(button))
            {
                if (KeyHandlers[(int)button])
                {
                    KeyHandlers[(int)button] = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public static bool IsButtonDown(Button button)
        {
            return Window.IsMouseButtonDown(button);
        }
    }
}
