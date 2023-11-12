using FastSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using System.Runtime.Serialization;
using System.Xml;

namespace Minecraft.System
{
    internal static class SaveManager
    {
        internal static string SaveDirectory { get; } = "Saves";
        private static bool Startup = true;

        static SaveManager()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            if (!File.Exists(SaveDirectory + "/chunks.xml"))
            {
                FileStream fileStream = File.Create(SaveDirectory + "/chunks.xml");
                fileStream.Close();
            }
        }

        internal static void SaveChunk(ChunkColumn chunk)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<Vector2i, ChunkColumn>));

            Dictionary<Vector2i, ChunkColumn>? columns = new Dictionary<Vector2i, ChunkColumn>();

            if (!Startup)
            {
                using (Stream stream = File.OpenRead(SaveDirectory + "/chunks.xml"))
                {
                    columns = (Dictionary<Vector2i, ChunkColumn>?)serializer.ReadObject(stream);
                }
            }

            if (columns != null)
            {
                if (columns.ContainsKey(chunk.Position))
                {
                    columns.Remove(chunk.Position);
                    columns.Add(chunk.Position, chunk);
                }
                else
                {
                    columns.Add(chunk.Position, chunk);
                }
            }

            using (Stream stream = File.OpenWrite(SaveDirectory + "/chunks.xml"))
            {
                serializer.WriteObject(stream, columns);
            }
            Startup = false;
        }

        internal static List<BlockStruct> LoadChunk(Vector2i position)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<Vector2i, ChunkColumn>));

            Dictionary<Vector2i, ChunkColumn>? columns = new Dictionary<Vector2i, ChunkColumn>();
            using (Stream stream = File.OpenRead(SaveDirectory + "/chunks.xml"))
            {
                columns = (Dictionary<Vector2i, ChunkColumn>?)serializer.ReadObject(stream);
            }

            if (columns != null)
            {
                if (columns.TryGetValue(position, out ChunkColumn column))
                    return column.BlocksChanged;
                else
                    return new List<BlockStruct>();
            }
            else
                return new List<BlockStruct>();
        }

        internal static void AddBlocksToChunkSave(List<BlockStruct> blocks, Vector2i position)
        {
        }
    }
}
