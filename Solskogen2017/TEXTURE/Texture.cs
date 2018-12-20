using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Solskogen2017.GRAPHICS;

namespace Solskogen2017.TEXTURE
{
    public class Texture
    {
        private static GRAPHICS.Program shaderProgram;
        private static int textureUniformLocation = 0;
        private static int vertexAttribLocation = 0;
        private static int texcoordAttribLocation = 0;
        private static int vbohandle = 0;

        private static float[] texCoords = new float[] {0f, 0f, 0f, 1f, 1f, 1f, 1f, 0f };
        private static float[] vertexCoords = new float[] { -1f, -1f, -1f, 1f, 1f, 1f, 1f, -1f };

        private static float[] mesh = new float[]
        { //Texture, mesh
            0f, 0f, -1f, -1f,
            0f, 1f, -1f, 1f,
            1f, 1f, 1f, 1f,
            1f, 0f, 1f, -1f
        };

        private int _handle;


        public Texture(int handle)
        {
            this._handle = handle;
        }
        public int Handle
        {
            get { return _handle; }
        }

        private static void initBlitterShader()
        {
            Shader vertexShader = new Shader(ShaderType.VertexShader, "blitter_vertex.c");
            Shader fragmentShader = new Shader(ShaderType.FragmentShader, "blitter_fragment.c");
            shaderProgram = new GRAPHICS.Program();
            shaderProgram.attatchShader(vertexShader);
            shaderProgram.attatchShader(fragmentShader);
            shaderProgram.link();

            textureUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "texture");
            vertexAttribLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "vertex");
            texcoordAttribLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "texcoord");

            vbohandle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbohandle);
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.Length * 4, mesh, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(vertexAttribLocation, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, sizeof(float) * 2);
            GL.VertexAttribPointer(texcoordAttribLocation, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);

            GL.EnableVertexAttribArray(vertexAttribLocation);
            GL.EnableVertexAttribArray(texcoordAttribLocation);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void Blit()
        {
            if(shaderProgram==null)
            {
                initBlitterShader();
            }
            GL.Disable(EnableCap.CullFace);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbohandle);

            shaderProgram.use();

            GL.Uniform1(textureUniformLocation, 0);
            GL.BindTexture(TextureTarget.Texture2D, _handle);

            // draw a cube
            GL.DrawArrays(BeginMode.Quads, 0, 4);
            // deactivate vertex arrays after drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
