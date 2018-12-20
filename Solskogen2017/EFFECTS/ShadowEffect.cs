using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solskogen2017.GRAPHICS;
using Solskogen2017.TEXTURE;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Solskogen2017.EFFECTS
{
    class ShadowEffect
    {
        private const int SHADOW_SIZE = 1024;

        private Texture _shadowTexture;
        private int _shadowFramebuffer;
        private GameWindow _window;

        public ShadowEffect(GameWindow window)
        {
            this._window = window;
            _shadowFramebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowFramebuffer);

            _shadowTexture = new Texture(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, _shadowTexture.Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _shadowTexture.Handle, 0);

            GL.DrawBuffer(DrawBufferMode.None);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer creation failed lol");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

        public void BeginShadowRendering(Context context)
        {
            context.CurrentMode = Context.MatrixMode.LIGHT;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowFramebuffer);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, SHADOW_SIZE, SHADOW_SIZE);
        }

        public void EndShadowRendering(Context context)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            context.CurrentMode = Context.MatrixMode.CAMERA;
            GL.Viewport(0, 0, _window.Size.Width, _window.Size.Height);
        }

        public Texture ShadowTexture
        {
            get
            {
                return _shadowTexture;
            }
        }
    }
}
