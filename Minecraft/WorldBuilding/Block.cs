using GameEngine.ModelLoading;
using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Minecraft.WorldBuilding
{
    internal class Block : ICloneable
    {
        internal Vector3i Position;
        internal Texture Texture { get; set; }

        public static List<Vertex> Vertices = new List<Vertex>()
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
         
        internal static int Vao { get; private set; }
        internal static int Vbo { get; private set; }

        internal Block(Vector3i position, Texture texture)
        {
            Position = position;
            Texture = texture;
        }

        public object Clone()
        {
            return new Block(this.Position, this.Texture);
        }
    }
}
