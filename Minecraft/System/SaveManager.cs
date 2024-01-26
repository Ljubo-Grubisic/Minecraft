using Minecraft.WorldBuilding;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Minecraft.System
{
    internal static class SaveManager
    {
        internal static string SaveDirectory { get; } = "Saves";

        static SaveManager()
        {
            EnsureDirectory(SaveDirectory);
            EnsureFile(SaveDirectory + "/chunks.xml");
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

        internal static Stream GetStreamFormAssembly(string path)
        {
            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            if (stream == null)
                throw new Exception("Failed getting assemlby: " + path);

            return stream;
        }

        /// <summary>
        /// Ensures that the directory exists by creating it, if it 
        /// doesnt exist
        /// </summary>
        /// <param name="localDirectory"></param>
        /// <returns>True is the directory is created, false it the directory already
        /// existed</returns>
        internal static bool EnsureDirectory(string localDirectory)
        {
            if (!Directory.Exists(localDirectory))
            {
                Directory.CreateDirectory(localDirectory);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ensures that the file exists by creating it, if it 
        /// doesnt exist
        /// </summary>
        /// <param name="localDirectory"></param>
        /// <returns>True is the file is created, false it the file already
        /// existed</returns>
        internal static bool EnsureFile(string localDirectory)
        {
            if (!File.Exists(localDirectory))
            {
                File.Create(localDirectory).Close();
                return true;
            }
            return false;
        }
    }
}
