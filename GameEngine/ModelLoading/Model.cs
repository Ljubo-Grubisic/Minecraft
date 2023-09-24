using Assimp;
using GameEngine.Shadering;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using AssimpMesh = Assimp.Mesh;
using AssimpTextureType = Assimp.TextureType;

namespace GameEngine.ModelLoading
{
    internal static class Extensions
    {
        internal static Vector2 ConvertAssimpVector2(this Vector3D AssimpVector)
        {
            // Reinterpret the assimp vector into an OpenTK vector.
            return Unsafe.As<Vector3D, Vector2>(ref AssimpVector);
        }

        internal static Vector3 ConvertAssimpVector3(this Vector3D AssimpVector)
        {
            // Reinterpret the assimp vector into an OpenTK vector.
            return Unsafe.As<Vector3D, Vector3>(ref AssimpVector);
        }

        internal static Matrix4 ConvertAssimpMatrix4(this Matrix4x4 AssimpMatrix)
        {
            // Take the column-major assimp matrix and convert it to a row-major OpenTK matrix.
            return Matrix4.Transpose(Unsafe.As<Matrix4x4, Matrix4>(ref AssimpMatrix));
        }
    }

    public class Model
    {
        private List<Mesh> meshes = new();
        private string directory;

        public Model(string path)
        {
            LoadModel(path);
        }

        public void Draw(Shader shader)
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Draw(shader);
            }
        }

        private void LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();

            LogStream logstream = new LogStream(delegate (String msg, String userData)
            {
                Console.WriteLine(msg);
            });
            logstream.Attach();

            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                Console.WriteLine("Unable to load model from: " + path);
                return;
            }

            this.directory = path.Remove(path.LastIndexOf('/') + 1);

            ProcessNode(scene.RootNode, scene, Matrix4.Identity);

            importer.Dispose();
        }
        
        private void ProcessNode(Node node, Scene scene, Matrix4 parentTransform)
        {
            Matrix4 transform = node.Transform.ConvertAssimpMatrix4() * parentTransform;

            for (int i = 0; i < node.MeshCount; i++)
            {
                AssimpMesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(ProcessMesh(mesh, scene, transform));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene, transform);
            }
        }

        private Mesh ProcessMesh(AssimpMesh mesh, Scene scene, Matrix4 transform)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();
            List<Texture> textures = new List<Texture>();

            Matrix4 inverseTransform = Matrix4.Invert(transform);

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();

                // Positions
                Vector3 position = mesh.Vertices[i].ConvertAssimpVector3();
                Vector3 transformedPosition = Vector3.TransformPosition(position, transform);
                vertex.Position = transformedPosition;

                // Normals
                if (mesh.HasNormals)
                {
                    Vector3 normal = mesh.Normals[i].ConvertAssimpVector3();
                    Vector3 transformedNormal = Vector3.TransformNormalInverse(normal, inverseTransform);
                    vertex.Normal = transformedNormal;
                }

                // Texture coordinates
                if (mesh.TextureCoordinateChannels[0].Count != 0 && mesh.TextureCoordinateChannels[0] != null)
                {
                    Vector2 texCoords = mesh.TextureCoordinateChannels[0][i].ConvertAssimpVector2();

                    vertex.TexCoords = texCoords;
                }
                else
                    vertex.TexCoords = new Vector2();

                vertices.Add(vertex); 
            }

            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add(face.Indices[j]);
                }
            }

            if (mesh.MaterialIndex >= 0)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
                List<Texture> diffuseMaps = LoadMaterialTextures(material, AssimpTextureType.Diffuse);
                textures.InsertRange(textures.Count, diffuseMaps);
                List<Texture> specularMaps = LoadMaterialTextures(material, AssimpTextureType.Specular);
                textures.InsertRange(textures.Count, specularMaps);
            }

            return new Mesh(vertices, indices, textures);
        }

        private List<Texture> LoadMaterialTextures(Material material, AssimpTextureType type)
        {
            List<Texture> textures = new List<Texture>();

            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                TextureSlot textureSlot = new();
                bool skip = !material.GetMaterialTexture(type, i, out textureSlot);
                for (int j = 0; j < textures.Count; j++)
                {
                    if (textures[j].Path == textureSlot.FilePath)
                    {
                        textures.Add(textures[j]);
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    Texture texture = Texture.LoadFromFile(this.directory + textureSlot.FilePath, (TextureType)type);
                    textures.Add(texture);
                }
            }

            return textures;
        }
    }
}
