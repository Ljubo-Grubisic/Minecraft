using Minecraft.WorldBuilding;
using OpenTK.Mathematics;

namespace Minecraft.System
{
    internal static class StringExtensions
    {
        internal static string CopyUntil(this string source, int startIndex, char stopCharacter)
        {
            char currentCaracter = '?';
            string result = "";

            int indexOfStopCharacter = -1;
            while (currentCaracter != stopCharacter)
            {
                indexOfStopCharacter++;
                currentCaracter = source[indexOfStopCharacter + startIndex + 1];
            }
            if (indexOfStopCharacter != -1)
            {
                indexOfStopCharacter += startIndex + 1;

                for (int i = startIndex; i < indexOfStopCharacter; i++)
                {
                    result += source[i];
                }
            }

            return result;
        }

        internal static string Copy(this string source, int startIndex)
        {
            string result = "";

            for (int i = startIndex; i < source.Length; i++)
            {
                result += source[i];
            }

            return result;
        }
    }

    internal static class SaveManager
    {
        internal static string SaveDirectory { get; } = "Saves";

        internal static void Init()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            if (!File.Exists(SaveDirectory + "/chunks.txt"))
            {
                File.Create(SaveDirectory + "/chunks.txt");
            }
        }

        // CX*Y*B*X*Y*Z*,B*X*Y*Z*,;
        internal static void SaveChunkColumn(ChunkColumn chunk, List<BlockStruct> blocksChanged)
        {
            string text = "";
            Dictionary<Vector2i, (List<BlockStruct>, int)> chunksBlocksChanged = new Dictionary<Vector2i, (List<BlockStruct>, int)>();
            (List<BlockStruct>, int) chunkBlocksChanged;

            using (StreamReader reader = File.OpenText(SaveDirectory + "/chunks.txt"))
            {
                text = reader.ReadToEnd();
            }
            if (text.Length > 0)
            {
                chunksBlocksChanged = ParseChunkSaveWithIndex(text);
            }

            if (chunksBlocksChanged.TryGetValue(chunk.Position, out chunkBlocksChanged))
            {
                if (blocksChanged != chunkBlocksChanged.Item1)
                {
                    File.WriteAllText(SaveDirectory + "/chunks.txt", RemoveTextUntilChar(text, ';', chunkBlocksChanged.Item2));
                    if (blocksChanged.Count > 0)
                    {
                        using (StreamWriter writer = File.AppendText(SaveDirectory + "/chunks.txt"))
                        {

                            writer.Write("C" + "X" + chunk.Position.X + "Y" + chunk.Position.Y);
                            foreach (BlockStruct block in blocksChanged)
                            {
                                writer.Write("B" + (int)block.Type + "X" + block.Position.X + "Y" + block.Position.Y + "Z" + block.Position.Z + ",");
                            }
                            writer.Write(";");
                            writer.Flush();
                        }
                    }
                }
            }
            else
            {
                if (blocksChanged.Count > 0)
                {
                    using (StreamWriter writer = File.AppendText(SaveDirectory + "/chunks.txt"))
                    {
                        writer.Write("C" + "X" + chunk.Position.X + "Y" + chunk.Position.Y);
                        foreach (BlockStruct block in blocksChanged)
                        {
                            writer.Write("B" + (int)block.Type + "X" + block.Position.X + "Y" + block.Position.Y + "Z" + block.Position.Z + ",");
                        }
                        writer.Write(";");
                        writer.Flush();
                    }
                }
            }
        }

        internal static List<BlockStruct>? LoadChunkColumn(Vector2i position)
        {
            Dictionary<Vector2i, List<BlockStruct>> chunksBlocksChanged = new Dictionary<Vector2i, List<BlockStruct>>();
            List<BlockStruct> blocks = new List<BlockStruct>();
            string text = "";

            using (StreamReader reader = File.OpenText(SaveDirectory + "/chunks.txt"))
            {
                text = reader.ReadToEnd();
            }
            if (text.Length > 0)
            {
                chunksBlocksChanged = ParseChunkSave(text);
                if (chunksBlocksChanged.TryGetValue(position, out blocks))
                {
                    return blocks;
                }
            }
            return null;
        }

        private static string RemoveTextUntilChar(string input, char targetChar, int startIndex)
        {
            if (startIndex >= 0 && startIndex < input.Length)
            {
                int targetIndex = input.IndexOf(targetChar, startIndex);
                if (targetIndex != -1)
                {
                    return input.Remove(startIndex, targetIndex - startIndex);
                }
            }

            return input;
        }

        private static Dictionary<Vector2i, List<BlockStruct>> ParseChunkSave(string chunkSaveFile)
        {
            Dictionary<Vector2i, List<BlockStruct>> chunksBlocksChanged = new Dictionary<Vector2i, List<BlockStruct>>();
            string chunkSave;
            string blockSave;

            Vector2i position;
            BlockStruct block;
            List<BlockStruct> blocks = new List<BlockStruct>();

            for (int i = 0; i < chunkSaveFile.Length; i++)
            {
                if (chunkSaveFile[i] == 'C')
                {
                    chunkSave = chunkSaveFile.CopyUntil(i, ';');
                    position.X = Convert.ToInt32(chunkSave.CopyUntil(2, 'Y'));
                    position.Y = Convert.ToInt32(chunkSave.CopyUntil(chunkSave.IndexOf('Y') + 1, 'B'));

                    for (int j = 0; j < chunkSave.Length; j++)
                    {
                        if (chunkSave[j] == 'B')
                        {
                            blockSave = 'B' + chunkSave.CopyUntil(j + 1, ',');
                            block.Type = (BlockType)Convert.ToInt32(blockSave.CopyUntil(1, 'X'));
                            block.Position.X = Convert.ToInt32(blockSave.CopyUntil(blockSave.IndexOf('X') + 1, 'Y'));
                            block.Position.Y = Convert.ToInt32(blockSave.CopyUntil(blockSave.IndexOf('Y') + 1, 'Z'));
                            block.Position.Z = Convert.ToInt32(blockSave.Copy(blockSave.IndexOf('Z') + 1));
                            blocks.Add(block);
                        }
                    }

                    chunksBlocksChanged.Add(position, blocks);
                }
            }
            return chunksBlocksChanged;
        }

        private static Dictionary<Vector2i, (List<BlockStruct>, int)> ParseChunkSaveWithIndex(string chunkSaveFile)
        {
            Dictionary<Vector2i, (List<BlockStruct>, int)> chunksBlocksChanged = new Dictionary<Vector2i, (List<BlockStruct>, int)>();
            string chunkSave;
            string blockSave;

            Vector2i position;
            BlockStruct block;
            List<BlockStruct> blocks = new List<BlockStruct>();

            for (int i = 0; i < chunkSaveFile.Length; i++)
            {
                if (chunkSaveFile[i] == 'C')
                {
                    chunkSave = chunkSaveFile.CopyUntil(i, ';');
                    position.X = Convert.ToInt32(chunkSave.CopyUntil(2, 'Y'));
                    position.Y = Convert.ToInt32(chunkSave.CopyUntil(chunkSave.IndexOf('Y') + 1, 'B'));

                    for (int j = 0; j < chunkSave.Length; j++)
                    {
                        if (chunkSave[j] == 'B')
                        {
                            blockSave = 'B' + chunkSave.CopyUntil(j + 1, ',');
                            block.Type = (BlockType)Convert.ToInt32(blockSave.CopyUntil(1, 'X'));
                            block.Position.X = Convert.ToInt32(blockSave.CopyUntil(blockSave.IndexOf('X') + 1, 'Y'));
                            block.Position.Y = Convert.ToInt32(blockSave.CopyUntil(blockSave.IndexOf('Y') + 1, 'Z'));
                            block.Position.Z = Convert.ToInt32(blockSave.Copy(blockSave.IndexOf('Z') + 1));
                            blocks.Add(block);
                        }
                    }

                    chunksBlocksChanged.Add(position, (blocks, i));
                }
            }
            return chunksBlocksChanged;
        }
    }
}
