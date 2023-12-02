using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Button = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace GameEngine
{
    public class MouseManager
    {
        public static Vector2i MouseOffSet = new Vector2i(8, 30);
        private static bool[] KeyHandlers = new bool[(int)Button.Last];

        private static NativeWindow Window;

        public static void Init(NativeWindow window)
        {
            Window = window;
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
