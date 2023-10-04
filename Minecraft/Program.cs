using GameEngine.Rendering;
using GameEngine.MainLooping;
using System.Runtime.CompilerServices;

namespace Minecraft
{
    internal class Program
    {
        internal static Minecraft Minecraft { get; private set; }

        static void Main(string[] args)
        {
            Minecraft.Window window = Minecraft.CreateWindow("Minecraft", 1280, 720);
            Minecraft = new Minecraft(window.GameWindowSettings, window.NativeWindowSettings);
            Minecraft.Run();
        }
    }
}