using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Solskogen2017.TEXTURE
{
    public class TextureManager
    {
        public static List<Texture> textures = new List<Texture>();
        public static List<string> texturesLoaded = new List<string>();
        public static List<int> uses = new List<int>();

        public static Texture loadTexture(string path)
        {
            string[] loaded = texturesLoaded.ToArray();
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i].Equals(path))
                {
                    return textures.ElementAt(i);
                }
            }
            Console.WriteLine("Texture passed: " + path);
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException(path);

            int id = GL.GenTexture();
            Texture tex = new Texture(id);
            textures.Add(tex);
            texturesLoaded.Add(path);
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap("../../" + path);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Console.WriteLine("Got texture " + path + " with id " + id);
            return tex;
        }
        public static void unloadTexture(Texture tex)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures.ElementAt(i).Handle == tex.Handle)
                {
                    GL.DeleteTexture(tex.Handle);
                    textures.RemoveAt(i);
                    texturesLoaded.RemoveAt(i);
                }
            }
        }
        public static void unloadTextures()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                GL.DeleteTexture(textures.ElementAt(i).Handle);
            }
            textures.Clear();
            texturesLoaded.Clear();
        }
    }
}

