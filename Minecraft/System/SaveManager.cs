using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using System.IO;
using System.Runtime.Serialization;

namespace Minecraft.System
{
    internal static class SaveManager
    {
        internal static string SaveDirectory { get; } = "Saves";

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

            bool isFileEmpty = false;
            using (Stream stream = File.OpenRead(SaveDirectory + "/chunks.xml"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    Span<char> text = new Span<char>();
                    if (reader.Read(text) == 0)
                        isFileEmpty = true;
                }

                if (!isFileEmpty)
                    columns = (Dictionary<Vector2i, ChunkColumn>?)serializer.ReadObject(stream);
            }

            if (chunk.BlocksChanged.Count > 0)
            {
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
            }
        }

        internal static List<BlockStruct> LoadChunk(Vector2i position)
        {
            bool isFileEmpty = false;
            using (StreamReader reader = new StreamReader(File.OpenRead(SaveDirectory + "/chunks.xml")))
            {
                Span<char> text = new Span<char>();
                if (reader.Read(text) == 0)
                    isFileEmpty = true;
            }

            if (!isFileEmpty)
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
            }
            return new List<BlockStruct>();
        }

        internal static void AddBlocksToChunkSave(List<BlockStruct> blocks, Vector2i position)
        {
        }
    }
}
