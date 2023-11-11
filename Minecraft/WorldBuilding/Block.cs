using GameEngine.ModelLoading;
using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Minecraft.WorldBuilding
{
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

    internal enum BlockVisibilty
    {
        Opaque,
        Transparent
    }

    internal struct BlockStruct
    {
        internal Vector3i Position;
        internal BlockType Type;
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

        private static List<(BlockType, Vector2i)> TextureIndex = new List<(BlockType, Vector2i)>();

        internal static Vector2 GetTexCoordsOffset(BlockType type)
        {
            Vector2 texCoords = new Vector2()
            {
                X = (float)TexCoordsIndexOfBlockType(type).Y / NumTexturesRow,
                Y = -(float)TexCoordsIndexOfBlockType(type).X / NumTexturesColumn
            };

            return texCoords;
        }

        private static Vector2i TexCoordsIndexOfBlockType(BlockType blockType)
        {
            foreach ((BlockType, Vector2i) tuple in TextureIndex)
            {
                if (tuple.Item1 == blockType)
                {
                    return tuple.Item2;
                }
            }
            return Vector2i.Zero;
        }

        internal static void Init()
        {
            Texture = Texture.LoadFromFile("Resources/Textures.png");
            CalculateTexCoords();

            TextureIndex.Add((BlockType.Dirt, new Vector2i(0, 0)));
            TextureIndex.Add((BlockType.Grass, new Vector2i(0, 1)));
            TextureIndex.Add((BlockType.Stone, new Vector2i(0, 2)));
            TextureIndex.Add((BlockType.Sand, new Vector2i(0, 3)));
            TextureIndex.Add((BlockType.Water, new Vector2i(0, 4)));

            TextureIndex.Add((BlockType.OakLeaves, new Vector2i(1, 0)));
            TextureIndex.Add((BlockType.OakLog, new Vector2i(1, 2)));
            TextureIndex.Add((BlockType.Cactus, new Vector2i(1, 1)));
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
    }
}
