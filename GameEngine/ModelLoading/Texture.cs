using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;

namespace GameEngine.ModelLoading
{
    public enum TextureType
    {
        None,
        Diffuse,
        Specular
    }

    public class Texture
    {
        public int Handle { get; private set; }
        public string Path { get; private set; }
        public TextureType Type { get; private set; }

        private static List<Image> images = new List<Image>();

        public Texture(int handle, string path, TextureType type)
        {
            Handle = handle;
            Path = path;
            Type = type;
        }

        public static Texture LoadFromFile(string path, TextureType type)
        {
            int handle;

            int index;
            if (IsImageLoaded(images, path, out index))
            {
                handle = images[index].id;
            }
            else
            {
                handle = GL.GenTexture();

                // Bind the handle
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, handle);

                StbImage.stbi_set_flip_vertically_on_load(1);

                using (Stream stream = File.OpenRead(path))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

                    images.Add(new Image { imageResult = image, path = path, id = handle });
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            return new Texture(handle, path, type);
        }

        public static Texture LoadFromFile(string path)
        {
            int handle;

            int index;
            if (IsImageLoaded(images, path, out index))
            {
                handle = images[index].id;
            }
            else
            {
                handle = GL.GenTexture();

                // Bind the handle
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, handle);

                StbImage.stbi_set_flip_vertically_on_load(1);

                using (Stream stream = File.OpenRead(path))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

                    images.Add(new Image { imageResult = image, path = path, id = handle });
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            return new Texture(handle, path, TextureType.None);
        }

        private static bool IsImageLoaded(List<Image> images, string path, out int index)
        {
            bool result = false;
            index = -1;

            for (int i = 0; i < images.Count; i++)
            {
                if (images[i].path == path)
                {
                    result = true;
                    index = i;
                    break;
                }
            }

            return result;
        }

        private struct Image
        {
            internal ImageResult imageResult;
            internal int id;
            internal string path;
        }
    }
}
