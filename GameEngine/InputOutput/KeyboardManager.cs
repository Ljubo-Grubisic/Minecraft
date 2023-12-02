using GameEngine.MainLooping;
using GameEngine.ModelLoading;
using GameEngine.Rendering;
using GameEngine.Shadering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Key = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace GameEngine
{
    public static class KeyboardManager
    {
        private static bool[] KeysPressed = new bool[(int)Key.LastKey];
        private static bool[] KeysReleased = new bool[(int)Key.LastKey];

        private static bool[] KeysHandler = new bool[(int)Key.LastKey];
        private static bool[] KeysHandlerDown = new bool[(int)Key.LastKey];
        private static bool[] KeysHandlerUp = new bool[(int)Key.LastKey];

        private static bool[] UpHandler = new bool[(int)Key.LastKey];
        private static float[] TimeHandler = new float[(int)Key.LastKey];

        private static NativeWindow Window;

        public static void Init(NativeWindow window)
        {
            Window = window;
            window.KeyDown += Window_KeyPressed;
            window.KeyUp += Window_KeyReleased;
        }

        public static void Update()
        {
            for (int i = 0; i < (int)Key.LastKey; i++)
            {
                KeysPressed[i] = false;
                KeysReleased[i] = false;
            }
        }

        private static void Window_KeyPressed(KeyboardKeyEventArgs e)
        {
            KeysPressed[(int)e.Key] = true;
        }

        private static void Window_KeyReleased(KeyboardKeyEventArgs e)
        {
            KeysReleased[(int)e.Key] = true;
        }

        /// <summary>
        /// Returns true only when the key is pressed
        /// </summary>
        /// <param name="key">The key you want to check</param>
        /// <returns></returns>
        public static bool OnKeyPressed(Key key)
        {
            return KeysPressed[(int)key];
        }

        /// <summary>
        /// Returns true only when the key is released
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool OnKeyReleased(Key key)
        {
            return KeysReleased[(int)key];
        }

        /// <summary>
        /// Returns true if the key is down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyDown(Key key)
        {
            return Window.KeyboardState.IsKeyPressed(key);
        }

        /// <summary>
        /// Returns true after you have pressed the button for 1 s
        /// </summary>
        /// <param name="key">The key you want to check</param>
        /// <param name="timeTillTrue">The time it will take until the function will return true</param>
        /// <returns></returns>
        public static bool OnKeyDownForTime(Key key, float timeTillTrue, float totalTimeElapsed)
        {
            if (OnKeyPressed(key))
            {
                TimeHandler[(int)key] = totalTimeElapsed;
            }
            if (!IsKeyDown(key))
            {
                TimeHandler[(int)key] = 0;
            }
            if (IsKeyDown(key) && totalTimeElapsed - TimeHandler[(int)key] > timeTillTrue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string ReadInput(string input, string acceptableCharacters)
        {
            string output;

            output = Keys(acceptableCharacters);

            return output;
        }


        private static string Keys(string acceptableCharacters)
        {
            string output = "";
            string buffer;

            // Increment LockKeys for each key
            for (int i = 0; i < (int)Key.LastKey; i++)
            {
                if (!IsKeyDown((Key)i))
                {
                    KeysHandler[i] = true;
                }
                if (IsKeyDown((Key)i))
                {
                    if (KeysHandler[i])
                    {
                        buffer = ((Key)i).ToString();
                        if (buffer.Contains("Num"))
                        {
                            buffer = buffer.Remove(0, 3);
                        }
                        if (buffer == "Period")
                        {
                            buffer = buffer.Replace("Period", ".");
                        }
                        if (acceptableCharacters.Contains(buffer))
                        {
                            output += buffer;
                        }
                    }
                    KeysHandler[i] = false;
                }
            }
            return output;
        }
    }
}
