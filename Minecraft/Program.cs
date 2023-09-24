using GameEngine.Rendering;
using GameEngine.MainLooping;

namespace Minecraft
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Minecraft.Window window = Minecraft.CreateWindow("Minecraft", 1280, 720);
            Minecraft minecraft = new Minecraft(window.GameWindowSettings, window.NativeWindowSettings);
            minecraft.Run();
        }
    }
}