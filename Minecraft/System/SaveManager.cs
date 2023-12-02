using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
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

        internal static Dictionary<Vector3i, BlockType> LoadChunk(Vector2i position)
        {
            bool isFileEmpty = false;

            StreamReader reader = new StreamReader(File.OpenRead(SaveDirectory + "/chunks.xml"));

            if (reader.BaseStream.Length == 0)
                isFileEmpty = true;

            reader.Dispose();

            if (!isFileEmpty)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<Vector2i, ChunkColumn>));

                Dictionary<Vector2i, ChunkColumn>? columns = new Dictionary<Vector2i, ChunkColumn>();

                Stream stream = File.OpenRead(SaveDirectory + "/chunks.xml");
                columns = (Dictionary<Vector2i, ChunkColumn>?)serializer.ReadObject(stream);
                stream.Dispose();

                if (columns != null)
                {
                    if (columns.TryGetValue(position, out ChunkColumn? column))
                        return column.BlocksChanged;
                    else
                        return new Dictionary<Vector3i, BlockType>();
                }
            }
            return new Dictionary<Vector3i, BlockType>();
        }

        internal static void SaveChunk(ChunkColumn chunk)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<Vector2i, ChunkColumn>));

            Dictionary<Vector2i, ChunkColumn>? columns = new Dictionary<Vector2i, ChunkColumn>();

            Stream streamReader = File.OpenRead(SaveDirectory + "/chunks.xml");

            if (streamReader.Length > 0)
                columns = (Dictionary<Vector2i, ChunkColumn>?)serializer.ReadObject(streamReader);

            streamReader.Dispose();

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

                Stream streamWriter = File.OpenWrite(SaveDirectory + "/chunks.xml");

                serializer.WriteObject(streamWriter, columns);

                streamWriter.Dispose();
            }
        }
    }
}
