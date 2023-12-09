using GameEngine.ModelLoading;
using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;

namespace Minecraft.WorldBuilding
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum BlockType : byte
    {
        None = 0,
        Air,
        Dirt,
        Grass,
        Stone,
        Sand,
        Water,
        OakLeaves,
        OakLog,
        Cactus,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum BlockVisibility : byte
    {
        None = 0,
        Opaque,
        Transparent
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum BlockShape : byte  
    {
        None = 0,
        Square, 
        X
    }

    [DataContract]
    internal struct BlockStruct
    {
        [DataMember]
        internal Vector3i Position;
        [DataMember]
        internal BlockType Type;

        public override string ToString()
        {
            return "Position: " + this.Position + "Type: " + this.Type;
        }
    }

    internal static class Block
    {
        internal static readonly int NumTexturesRow = 5;
        internal static readonly int NumTexturesColumn = 3;

        internal static Texture Texture { get; private set; }
        internal static List<Vertex> Vertices = new List<Vertex>()
        {
            // z-
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f,  0.0f, -1.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            
            // z+
            new Vertex { Position = new Vector3(-0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f,  0.0f,  1.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            
            // x-
            new Vertex { Position = new Vector3(-0.5f,  0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            
            // x+
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f, -0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(1.0f,  0.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            
            // y-
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f,  0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f), Normal = new Vector3(0.0f, -1.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },

            // y+
            new Vertex { Position = new Vector3(-0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(1.0f, 1.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3( 0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(1.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f,  0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(0.0f, 0.0f) },
            new Vertex { Position = new Vector3(-0.5f,  0.5f, -0.5f), Normal = new Vector3(0.0f,  1.0f,  0.0f), TexCoords = new Vector2(0.0f, 1.0f) }
        };

        private static Dictionary<BlockType, BlockConfig> BlockConfigs = new Dictionary<BlockType, BlockConfig>();

        internal static Vector2 GetTexCoordsOffset(BlockType blockType)
        {
            Vector2i texCoordsIndex = new Vector2i();
            if (BlockConfigs.TryGetValue(blockType, out BlockConfig? config))
            {
                texCoordsIndex = new Vector2i(config.TextureIndexRow, config.TextureIndexColumn);
            }

            Vector2 texCoords = new Vector2()
            {
                X = (float)texCoordsIndex.Y / NumTexturesRow,
                Y = -(float)texCoordsIndex.X / NumTexturesColumn
            };
            return texCoords;
        }

        internal static BlockVisibility GetBlockVisibility(BlockType blockType)
        {
            if (BlockConfigs.TryGetValue(blockType, out BlockConfig? config))
            {
                return config.BlockVisibility;
            }
            return BlockVisibility.None;
        }

        internal static BlockShape GetBlockShape(BlockType blockType)
        {
            if (BlockConfigs.TryGetValue(blockType, out BlockConfig? config))
            {
                return config.BlockShape;
            }
            return BlockShape.None;
        }

        internal static void Init()
        {
            Texture = Texture.LoadFromFile("Resources/Textures.png");
            CalculateTexCoords();

            BlockConfig[]? blockConfigs;
            using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Minecraft.WorldBuilding.Block.BlockConfig.json"))
            {
                if (stream == null)
                    throw new Exception("Failed loading BlockConfig.json");
            
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    blockConfigs = JsonSerializer.Deserialize<BlockConfig[]?>(json);
                }
            }

            if (blockConfigs != null)
            {
                foreach (BlockConfig blockConfig in blockConfigs)
                {
                    BlockConfigs.Add(blockConfig.BlockType, blockConfig);
                }
            }
            else
            {
                throw new Exception("Failed loading BlockConfig.json");
            }
        }

        private static void CalculateTexCoords()
        {
            Vector2 texCoords = new();
            int indicies = 0;
            // z-
            {
                // 0.0, 0.0 -> 3/4, 2/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 0;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 5;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 2/4, 2/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 1;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 3/4, 1
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, 1);
                indicies = 4;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 2/4, 1
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, 1);
                indicies = 2;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 3;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }

            // z+
            {
                // 0.0, 0.0 -> 2/4, 0
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, 0.0f + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 8;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 9;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 3/4, 0
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, 0.0f + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 10;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 2/4, 1/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 7;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 3/4, 1/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 6;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 11;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }

            // x-
            {
                // 0.0, 0.0 -> 2/4, 2/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 16;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 2/4, 1/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 14;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 15;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 1/4, 2/3
                texCoords = new Vector2((1.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 12;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 17;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 1/4, 1/3
                texCoords = new Vector2((1.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 13;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }

            // x+
            {
                // 0.0, 0.0 -> 3/4, 2/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 22;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 3/4, 1/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 20;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 21;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 1, 2/3
                texCoords = new Vector2(1.0f / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 18;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 23;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 1, 1/3
                texCoords = new Vector2(1.0f / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 19;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }

            // y-
            {
                // 0.0, 0.0 -> 3/4, 1/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 28;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 2/4, 1/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 26;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 27;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 3/4, 2/3
                texCoords = new Vector2((3.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 24;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 29;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 2/4, 2/3
                texCoords = new Vector2((2.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 25;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }

            // y+
            {
                // 0.0, 0.0 -> 0, 1/3
                texCoords = new Vector2(0.0f / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 34;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 0.0 -> 1/4, 1/3
                texCoords = new Vector2((1.0f / 4.0f) / NumTexturesRow, (1.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 32;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 33;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 0.0, 1.0 -> 0, 2/3
                texCoords = new Vector2(0.0f / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 30;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                indicies = 35;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
                // 1.0, 1.0 -> 1/4, 2/3
                texCoords = new Vector2((1.0f / 4.0f) / NumTexturesRow, (2.0f / 3.0f) / NumTexturesColumn + ((NumTexturesColumn - 1.0f) / NumTexturesColumn));
                indicies = 31;
                Vertices[indicies] = new Vertex { Position = Vertices[indicies].Position, Normal = Vertices[indicies].Normal, TexCoords = texCoords };
            }
        }

        private class BlockConfig
        {
            public BlockType BlockType { get; set; }
            public BlockVisibility BlockVisibility { get; set; }
            public BlockShape BlockShape { get; set; }
            public int TextureIndexRow { get; set; }
            public int TextureIndexColumn { get; set; }
        }
    }
}
