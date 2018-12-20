using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Solskogen2017.TEXTURE;


namespace Solskogen2017.EFFECTS
{
    class DeferredRenderingStep
    {
        GameWindow _gameWindow;
        int _framebufferHandle;
        int _depthRenderBuffer;

        //Textures
        private Texture _diffuseTexture;
        private Texture _normalTexture;
        private Texture _positionTexture;

        public DeferredRenderingStep(GameWindow window)
        {
            _gameWindow = window;

            generateFramebuffers();
        }

        public void generateFramebuffers()
        {
            DIAGNOSTICS.Debug.DebugConsole("Framebuffer", "Texture sizes: " + _gameWindow.Width + "," + _gameWindow.Height);
            //Framebuffer
            _framebufferHandle = GL.GenFramebuffer();

            //Diffuse texture
            _diffuseTexture = new Texture(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, _diffuseTexture.Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _gameWindow.Width, _gameWindow.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _diffuseTexture.Handle, 0);

            _normalTexture = new Texture(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, _normalTexture.Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16, _gameWindow.Width, _gameWindow.Height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, _normalTexture.Handle, 0);

            _positionTexture = new Texture(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, _positionTexture.Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, _gameWindow.Width, _gameWindow.Height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, _positionTexture.Handle, 0);

            _depthRenderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRenderBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _gameWindow.Width, _gameWindow.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRenderBuffer);
        }

        public void BeginStep()
        {

        }

        public void EndStep()
        {

        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferHandle);
        }
    }
}
