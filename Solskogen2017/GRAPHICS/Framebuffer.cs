using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solskogen2017.TEXTURE;

namespace Solskogen2017.GRAPHICS
{
    class Framebuffer
    {
        private GameWindow _gameWindow;
        private Texture _texture;
        private int _framebufferId;
        private int _depthRenderBuffer;

        private System.Drawing.Size _framebufferSize;

        public Framebuffer(GameWindow window)
        {
            _gameWindow = window;

            generateFramebuffer();
        }

        private void generateFramebuffer()
        {
			_framebufferId = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);

			_texture = new Texture(GL.GenTexture());
			GL.BindTexture(TextureTarget.Texture2D, _texture.Handle);
			DIAGNOSTICS.Debug.DebugConsole("Framebuffer", "Creating framebuffer texture with size " + _gameWindow.Width + "," + _gameWindow.Height);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _gameWindow.Width, _gameWindow.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)0);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _texture.Handle, 0);
            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture.Handle, 0);

            _depthRenderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRenderBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _gameWindow.Width, _gameWindow.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRenderBuffer);

			GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

			//GL.DrawBuffer(DrawBufferMode.None);

			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				throw new Exception("Framebuffer creation failed lol");
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _framebufferSize = _gameWindow.Size;
        }

        public Texture Texture
        {
            get
            {
                return _texture;
            }
        }

        public void BeginDraw()
        {
            if (_framebufferSize != _gameWindow.Size)
            {
                DIAGNOSTICS.Debug.DebugConsole("Framebuffer", "Gamewindow resized. Creating new framebuffers.");
                Dispose();
                generateFramebuffer();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferId);
            GL.Viewport(0, 0, _gameWindow.Width, _gameWindow.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        public void EndDraw()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void Dispose()
        {
            GL.DeleteFramebuffer(_framebufferId);
            GL.DeleteTexture(_texture.Handle);
            _texture = null;
            _framebufferId = 0;
        }
    }
}
