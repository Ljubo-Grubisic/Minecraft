using GameEngine.Shadering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GameEngine.ModelLoading
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
    }

    public class Mesh : IDisposable
    {
        public List<Vertex> Vertices;
        public List<int> Indices;
        public List<Texture> Textures;

        public int VAO { get; private set; }
        public int VBO { get; private set; }
        public int EBO { get; private set; }

        public Mesh(List<Vertex> vertices, List<int> indices, List<Texture> textures)
        {
            this.Vertices = vertices;
            this.Indices = indices;
            this.Textures = textures;
            
            this.Vertices.TrimExcess();
            this.Indices.TrimExcess();
            this.Textures.TrimExcess();

            SetupMesh();
        }

        public void Draw(Shader shader)
        {
            int diffuseNumber = 1, specularNumber = 1;

            for (int i = 0; i < Textures.Count; i++)
            {
                GL.ActiveTexture((TextureUnit)i);

                string number = "";
                string name = "";
                TextureType type = Textures[i].Type;
                if (type == TextureType.Diffuse)
                {
                    name = "texture_diffuse";
                    number = diffuseNumber.ToString();
                    diffuseNumber++;
                }
                else if (type == TextureType.Specular)
                {
                    name = "texture_specular";
                    number = specularNumber.ToString();
                    specularNumber++;
                }

                shader.SetInt("material." + name + number, i);
                GL.BindTexture(TextureTarget.Texture2D, Textures[i].Handle);
            }

            GL.BindVertexArray(VAO);
            GL.DrawElements(BeginMode.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        private unsafe void SetupMesh()
        {
            this.VAO = GL.GenVertexArray();
            this.VBO = GL.GenBuffer();
            this.EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * sizeof(Vertex), Vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * sizeof(int), Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)));

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords)));

            GL.BindVertexArray(0);
        }

        public void Dispose()
        { 
            GL.DeleteVertexArray(this.VAO);
            GL.DeleteBuffer(this.VBO);
            GL.DeleteBuffer(this.EBO);
            this.Vertices.Clear();
            this.Indices.Clear();
            this.Textures.Clear();
        }
    }
}
