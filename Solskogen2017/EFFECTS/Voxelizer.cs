using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Solskogen2017.DIAGNOSTICS;
using Solskogen2017.GRAPHICS;

namespace Solskogen2017.EFFECTS
{
    class Voxelizer
    {
        private const int MAX_VERTEX_LIMIT = 500000;
        private const int CUBE_SIDES = 6;
        private const int MAX_INDICE_LIMIT = 9000;

        private int xw, yw, zw;
        private bool[,,] voxels;
        private Vector3[,,] colors;
        private Vector3 size;
        private Vector3 position;
        private Matrix4 model = Matrix4.CreateTranslation(new Vector3(0,0,0));

        //Graphiics stuff
        private static GRAPHICS.Program shaderProgram = null;
        private static int positionAttribLocation;
        private static int colorAttribLocation;
        private static int normalAttribLocation;
        private static int vUniformLocation;
        private static int pUniformLocation;
        private static int lightVUniformLocation;
        private static int lightPUniformLocation;
        private static int mUniformLocation;
        private static int lightPosUniformLocation;

        private static int renderPassUniformLocation;
        private static int shadowMapTextureUniformLocation;
        //Shader subroutine stuff
        private static int defaultPassSubroutine;
        private static int depthPassSubroutine;
        private static int diffusePassSubroutine;
        private static int normalPassSubroutine;

        //For reference: y+, y-, x+, x-, z+, z-
        private int[] vaos = new int[CUBE_SIDES];
        private int[] vertexBuffers = new int[CUBE_SIDES];
        private int[] colorBuffers = new int[CUBE_SIDES];
        private int[] normalBuffers = new int[CUBE_SIDES];

        private VertexArray[] verticeArray = new VertexArray[CUBE_SIDES];
        private VertexArray[] colorArray = new VertexArray[CUBE_SIDES];
        private VertexArray[] normalArray = new VertexArray[CUBE_SIDES];
        private ushort[,] indices = new ushort[CUBE_SIDES,MAX_INDICE_LIMIT];
        private int[] indiceCount = new int[CUBE_SIDES];
        private int[] vertexCount = new int[CUBE_SIDES];

        private class VertexArray
        {
            public float[] array = new float[MAX_VERTEX_LIMIT];
        }

        public Voxelizer(int x, int y, int z, Vector3 size, Vector3 position)
        {
            this.xw = x;
            this.yw = y;
            this.zw = z;

            voxels = new bool[x,y,z];
            colors = new Vector3[x,y,z];
            this.position = position;
            this.size = size;
            this.model = Matrix4.CreateTranslation(position);

            for(int i = 0; i < verticeArray.Length; i++)
            {
                verticeArray[i] = new VertexArray();
                colorArray[i] = new VertexArray();
                normalArray[i] = new VertexArray();
            }

            //Check if shader is initialized
            if(shaderProgram == null)
            {
                initShader();
            }

            //Vertex arrays
            GL.GenVertexArrays(CUBE_SIDES, vaos);
            for(int i = 0; i < CUBE_SIDES; i++)
            {
                GL.BindVertexArray(vaos[i]);

                vertexBuffers[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffers[i]);
                //GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(positionAttribLocation, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(positionAttribLocation);

                colorBuffers[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffers[i]);
                //GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * colors.Length, colors, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(colorAttribLocation, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(colorAttribLocation);

                normalBuffers[i] = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffers[i]);
                //GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * colors.Length, colors, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(normalAttribLocation, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(normalAttribLocation);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public bool[,,] Voxels
        {
            get
            {
                return voxels;
            }
        }

        public Vector3[,,] Colors
        {
            get
            {
                return colors;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                this.model = Matrix4.CreateTranslation(position);
            }
        }

        private static void initShader()
        {
            Shader vertexShader = new Shader(ShaderType.VertexShader, "voxel_vertex.c");
            Shader fragmentShader = new Shader(ShaderType.FragmentShader, "voxel_fragment.c");
            shaderProgram = new GRAPHICS.Program();
            shaderProgram.attatchShader(fragmentShader);
            shaderProgram.attatchShader(vertexShader);
            shaderProgram.link();

            vUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "V");
            pUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "P");
            lightVUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "LightV");
            lightPUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "LightP");
            mUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "M");
            lightPosUniformLocation = GL.GetUniformLocation(shaderProgram.ProgramId, "lightPos");

            positionAttribLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "pos");
            normalAttribLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "normal");
            colorAttribLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "color");
            renderPassUniformLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "currentPass");
            shadowMapTextureUniformLocation = GL.GetAttribLocation(shaderProgram.ProgramId, "shadowMap");

            defaultPassSubroutine = GL.GetSubroutineIndex(shaderProgram.ProgramId, ShaderType.FragmentShader, "Default");
            depthPassSubroutine = GL.GetSubroutineIndex(shaderProgram.ProgramId, ShaderType.FragmentShader, "Depth");
            diffusePassSubroutine = GL.GetSubroutineIndex(shaderProgram.ProgramId, ShaderType.FragmentShader, "Diffuse");
            normalPassSubroutine = GL.GetSubroutineIndex(shaderProgram.ProgramId, ShaderType.FragmentShader, "Normal");

            Debug.DebugConsole("Voxelizer", "Got Model transform location: " + mUniformLocation);
            Debug.DebugConsole("Voxelizer", "Got View transform location: " + vUniformLocation);
            Debug.DebugConsole("Voxelizer", "Got Projection transform location: " + pUniformLocation);
            Debug.DebugConsole("Voxelizer", "Got position attribute location: " + positionAttribLocation);
            Debug.DebugConsole("Voxelizer", "Got color attribute location: " + colorAttribLocation);

            Debug.DebugConsole("Voxelizer", "Shader initiialized");
        }

        public void resetMatrix()
        {
            for (int x = 0; x < xw; x++)
            {
                for (int y = 0; y < yw; y++)
                {
                    for (int z = 0; z < zw; z++)
                    {
                        voxels[x, y, z] = false;
                    }
                }
            }
        }

        public void generateMesh()
        {
            Vector3 scaleFactor = new Vector3((1f / (float)xw) * size.X, (1f / (float)yw) * size.Y, (1f / (float)zw)*size.Z);
            Vector3 half = new Vector3(scaleFactor.X / 2f, scaleFactor.Y / 2f, scaleFactor.Z / 2f);
            int voxelCount = 0;

            for (int i = 0; i < CUBE_SIDES; i++)
            {
                indiceCount[i] = 0;
                vertexCount[i] = 0;
            }
            for (int x = 0; x < xw; x++)
            {
                for (int y = 0; y < yw; y++)
                {
                    for (int z = 0; z < zw; z++)
                    {
                        if (!voxels[x, y, z])
                            continue;
                        voxelCount++;
                        Vector3 localPos = new Vector3(scaleFactor.X * (float)x, scaleFactor.Y * (float)y, scaleFactor.Z * (float)z);
                        #region y
                        //y+
                        if (y + 1 != yw && !voxels[x, y + 1, z])
                        {
                            //Add indices (2 tri's)
                            /*indices[0, indiceCount[0]] = 0;
                            indices[0, indiceCount[0] + 1] = 1;
                            indices[0, indiceCount[0] + 2] = 2;
                            indices[0, indiceCount[0] + 3] = 0;
                            indices[0, indiceCount[0] + 4] = 2;
                            indices[0, indiceCount[0] + 5] = 3;
                            indiceCount[0] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[0].array[vertexCount[0] * 3 + 0] = localPos.X - half.X;
                            verticeArray[0].array[vertexCount[0] * 3 + 1] = localPos.Y + half.Y;
                            verticeArray[0].array[vertexCount[0] * 3 + 2] = localPos.Z + half.Z;

                            verticeArray[0].array[vertexCount[0] * 3 + 3] = localPos.X - half.X;
                            verticeArray[0].array[vertexCount[0] * 3 + 4] = localPos.Y + half.Y;
                            verticeArray[0].array[vertexCount[0] * 3 + 5] = localPos.Z - half.Z;

                            verticeArray[0].array[vertexCount[0] * 3 + 6] = localPos.X + half.X;
                            verticeArray[0].array[vertexCount[0] * 3 + 7] = localPos.Y + half.Y;
                            verticeArray[0].array[vertexCount[0] * 3 + 8] = localPos.Z - half.Z;

                            verticeArray[0].array[vertexCount[0] * 3 + 9] = localPos.X + half.X;
                            verticeArray[0].array[vertexCount[0] * 3 + 10] = localPos.Y + half.Y;
                            verticeArray[0].array[vertexCount[0] * 3 + 11] = localPos.Z + half.Z;

                            //Add colors(4)
                            colorArray[0].array[vertexCount[0] * 3 + 0] = colors[x, y, z].X;
                            colorArray[0].array[vertexCount[0] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[0].array[vertexCount[0] * 3 + 2] = colors[x, y, z].Z;

                            colorArray[0].array[vertexCount[0] * 3 + 3] = colors[x, y, z].X;
                            colorArray[0].array[vertexCount[0] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[0].array[vertexCount[0] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[0].array[vertexCount[0] * 3 + 6] = colors[x, y, z].X;
                            colorArray[0].array[vertexCount[0] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[0].array[vertexCount[0] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[0].array[vertexCount[0] * 3 + 9] = colors[x, y, z].X;
                            colorArray[0].array[vertexCount[0] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[0].array[vertexCount[0] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[0].array[vertexCount[0] * 3 + 0] = 0f;
                            normalArray[0].array[vertexCount[0] * 3 + 1] = 1f;
                            normalArray[0].array[vertexCount[0] * 3 + 2] = 0f;

                            normalArray[0].array[vertexCount[0] * 3 + 3] = 0f;
                            normalArray[0].array[vertexCount[0] * 3 + 4] = 1f;
                            normalArray[0].array[vertexCount[0] * 3 + 5] = 0f;

                            normalArray[0].array[vertexCount[0] * 3 + 6] = 0f;
                            normalArray[0].array[vertexCount[0] * 3 + 7] = 1f;
                            normalArray[0].array[vertexCount[0] * 3 + 8] = 0f;

                            normalArray[0].array[vertexCount[0] * 3 + 9] = 0f;
                            normalArray[0].array[vertexCount[0] * 3 + 10] = 1f;
                            normalArray[0].array[vertexCount[0] * 3 + 11] = 0f;

                            vertexCount[0] += 4; //Vertex count also controls color count
                        }
                        //y-
                        if (y - 1 != -1 && !voxels[x, y - 1, z])
                        {
                            //Add indices (2 tri's)
                            /*indices[1, indiceCount[1]] = 0;
                            indices[1, indiceCount[1] + 1] = 1;
                            indices[1, indiceCount[1] + 2] = 2;
                            indices[1, indiceCount[1] + 3] = 0;
                            indices[1, indiceCount[1] + 4] = 2;
                            indices[1, indiceCount[1] + 5] = 3;
                            indiceCount[1] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[1].array[vertexCount[1] * 3 + 0] = localPos.X + half.X;
                            verticeArray[1].array[vertexCount[1] * 3 + 1] = localPos.Y - half.Y;
                            verticeArray[1].array[vertexCount[1] * 3 + 2] = localPos.Z + half.Z;

                            verticeArray[1].array[vertexCount[1] * 3 + 3] = localPos.X + half.X;
                            verticeArray[1].array[vertexCount[1] * 3 + 4] = localPos.Y - half.Y;
                            verticeArray[1].array[vertexCount[1] * 3 + 5] = localPos.Z - half.Z;

                            verticeArray[1].array[vertexCount[1] * 3 + 6] = localPos.X - half.X;
                            verticeArray[1].array[vertexCount[1] * 3 + 7] = localPos.Y - half.Y;
                            verticeArray[1].array[vertexCount[1] * 3 + 8] = localPos.Z - half.Z;

                            verticeArray[1].array[vertexCount[1] * 3 + 9] = localPos.X - half.X;
                            verticeArray[1].array[vertexCount[1] * 3 + 10] = localPos.Y - half.Y;
                            verticeArray[1].array[vertexCount[1] * 3 + 11] = localPos.Z + half.Z;

                            //Add colors(4)
                            colorArray[1].array[vertexCount[1] * 3 + 0] = colors[x, y, z].X;
                            colorArray[1].array[vertexCount[1] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[1].array[vertexCount[1] * 3 + 2] = colors[x, y, z].Z;

                            colorArray[1].array[vertexCount[1] * 3 + 3] = colors[x, y, z].X;
                            colorArray[1].array[vertexCount[1] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[1].array[vertexCount[1] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[1].array[vertexCount[1] * 3 + 6] = colors[x, y, z].X;
                            colorArray[1].array[vertexCount[1] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[1].array[vertexCount[1] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[1].array[vertexCount[1] * 3 + 9] = colors[x, y, z].X;
                            colorArray[1].array[vertexCount[1] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[1].array[vertexCount[1] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[1].array[vertexCount[1] * 3 + 0] = 0f;
                            normalArray[1].array[vertexCount[1] * 3 + 1] = -1f;
                            normalArray[1].array[vertexCount[1] * 3 + 2] = 0f;

                            normalArray[1].array[vertexCount[1] * 3 + 3] = 0f;
                            normalArray[1].array[vertexCount[1] * 3 + 4] = -1f;
                            normalArray[1].array[vertexCount[1] * 3 + 5] = 0f;

                            normalArray[1].array[vertexCount[1] * 3 + 6] = 0f;
                            normalArray[1].array[vertexCount[1] * 3 + 7] = -1f;
                            normalArray[1].array[vertexCount[1] * 3 + 8] = 0f;

                            normalArray[1].array[vertexCount[1] * 3 + 9] = 0f;
                            normalArray[1].array[vertexCount[1] * 3 + 10] = -1f;
                            normalArray[1].array[vertexCount[1] * 3 + 11] = 0f;

                            vertexCount[1] += 4; //Vertex count also controls color count
                        }
                        #endregion
                        #region x
                        //x+
                        if (x + 1 != xw && !voxels[x + 1, y, z])
                        {
                            //Add indices (2 tri's)
                            /*indices[2, indiceCount[2]] = 0;
                            indices[2, indiceCount[2] + 1] = 1;
                            indices[2, indiceCount[2] + 2] = 2;
                            indices[2, indiceCount[2] + 3] = 0;
                            indices[2, indiceCount[2] + 4] = 2;
                            indices[2, indiceCount[2] + 5] = 3;
                            indiceCount[2] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[2].array[vertexCount[2] * 3 + 0] = localPos.X + half.X;
                            verticeArray[2].array[vertexCount[2] * 3 + 1] = localPos.Y + half.Y;
                            verticeArray[2].array[vertexCount[2] * 3 + 2] = localPos.Z + half.Z;

                            verticeArray[2].array[vertexCount[2] * 3 + 3] = localPos.X + half.X;
                            verticeArray[2].array[vertexCount[2] * 3 + 4] = localPos.Y + half.Y;
                            verticeArray[2].array[vertexCount[2] * 3 + 5] = localPos.Z - half.Z;

                            verticeArray[2].array[vertexCount[2] * 3 + 6] = localPos.X + half.X;
                            verticeArray[2].array[vertexCount[2] * 3 + 7] = localPos.Y - half.Y;
                            verticeArray[2].array[vertexCount[2] * 3 + 8] = localPos.Z - half.Z;

                            verticeArray[2].array[vertexCount[2] * 3 + 9] = localPos.X + half.X;
                            verticeArray[2].array[vertexCount[2] * 3 + 10] = localPos.Y - half.Y;
                            verticeArray[2].array[vertexCount[2] * 3 + 11] = localPos.Z + half.Z;

                            //Add colors(4)
                            colorArray[2].array[vertexCount[2] * 3 + 0] = colors[x, y, z].X;
                            colorArray[2].array[vertexCount[2] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[2].array[vertexCount[2] * 3 + 2] = colors[x, y, z].Z;

                            colorArray[2].array[vertexCount[2] * 3 + 3] = colors[x, y, z].X;
                            colorArray[2].array[vertexCount[2] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[2].array[vertexCount[2] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[2].array[vertexCount[2] * 3 + 6] = colors[x, y, z].X;
                            colorArray[2].array[vertexCount[2] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[2].array[vertexCount[2] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[2].array[vertexCount[2] * 3 + 9] = colors[x, y, z].X;
                            colorArray[2].array[vertexCount[2] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[2].array[vertexCount[2] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[2].array[vertexCount[2] * 3 + 0] = 1f;
                            normalArray[2].array[vertexCount[2] * 3 + 1] = 0f;
                            normalArray[2].array[vertexCount[2] * 3 + 2] = 0f;

                            normalArray[2].array[vertexCount[2] * 3 + 3] = 1f;
                            normalArray[2].array[vertexCount[2] * 3 + 4] = 0f;
                            normalArray[2].array[vertexCount[2] * 3 + 5] = 0f;

                            normalArray[2].array[vertexCount[2] * 3 + 6] = 1f;
                            normalArray[2].array[vertexCount[2] * 3 + 7] = 0f;
                            normalArray[2].array[vertexCount[2] * 3 + 8] = 0f;

                            normalArray[2].array[vertexCount[2] * 3 + 9] = 1f;
                            normalArray[2].array[vertexCount[2] * 3 + 10] = 0f;
                            normalArray[2].array[vertexCount[2] * 3 + 11] = 0f;

                            vertexCount[2] += 4; //Vertex count also controls color count
                        }
                        //x-
                        if (x - 1 !=-1 && !voxels[x - 1, y, z])
                        {
                            //Add indices (2 tri's)
                            /*indices[3, indiceCount[3]] = 0;
                            indices[3, indiceCount[3] + 1] = 1;
                            indices[3, indiceCount[3] + 2] = 2;
                            indices[3, indiceCount[3] + 3] = 0;
                            indices[3, indiceCount[3] + 4] = 2;
                            indices[3, indiceCount[3] + 5] = 3;
                            indiceCount[3] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[3].array[vertexCount[3] * 3 + 0] = localPos.X - half.X;
                            verticeArray[3].array[vertexCount[3] * 3 + 1] = localPos.Y - half.Y;
                            verticeArray[3].array[vertexCount[3] * 3 + 2] = localPos.Z + half.Z;

                            verticeArray[3].array[vertexCount[3] * 3 + 3] = localPos.X - half.X;
                            verticeArray[3].array[vertexCount[3] * 3 + 4] = localPos.Y - half.Y;
                            verticeArray[3].array[vertexCount[3] * 3 + 5] = localPos.Z - half.Z;

                            verticeArray[3].array[vertexCount[3] * 3 + 6] = localPos.X - half.X;
                            verticeArray[3].array[vertexCount[3] * 3 + 7] = localPos.Y + half.Y;
                            verticeArray[3].array[vertexCount[3] * 3 + 8] = localPos.Z - half.Z;

                            verticeArray[3].array[vertexCount[3] * 3 + 9] = localPos.X - half.X;
                            verticeArray[3].array[vertexCount[3] * 3 + 10] = localPos.Y + half.Y;
                            verticeArray[3].array[vertexCount[3] * 3 + 11] = localPos.Z + half.Z;

                            //Add colors(4)
                            colorArray[3].array[vertexCount[3] * 3 + 0] = colors[x, y, z].X;
                            colorArray[3].array[vertexCount[3] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[3].array[vertexCount[3] * 3 + 2] = colors[x, y, z].Z;
                            
                            colorArray[3].array[vertexCount[3] * 3 + 3] = colors[x, y, z].X;
                            colorArray[3].array[vertexCount[3] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[3].array[vertexCount[3] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[3].array[vertexCount[3] * 3 + 6] = colors[x, y, z].X;
                            colorArray[3].array[vertexCount[3] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[3].array[vertexCount[3] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[3].array[vertexCount[3] * 3 + 9] = colors[x, y, z].X;
                            colorArray[3].array[vertexCount[3] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[3].array[vertexCount[3] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[3].array[vertexCount[3] * 3 + 0] = -1f;
                            normalArray[3].array[vertexCount[3] * 3 + 1] = 0f;
                            normalArray[3].array[vertexCount[3] * 3 + 2] = 0f;

                            normalArray[3].array[vertexCount[3] * 3 + 3] = -1f;
                            normalArray[3].array[vertexCount[3] * 3 + 4] = 0f;
                            normalArray[3].array[vertexCount[3] * 3 + 5] = 0f;

                            normalArray[3].array[vertexCount[3] * 3 + 6] = -1f;
                            normalArray[3].array[vertexCount[3] * 3 + 7] = 0f;
                            normalArray[3].array[vertexCount[3] * 3 + 8] = 0f;

                            normalArray[3].array[vertexCount[3] * 3 + 9] = -1f;
                            normalArray[3].array[vertexCount[3] * 3 + 10] = 0f;
                            normalArray[3].array[vertexCount[3] * 3 + 11] = 0f;

                            vertexCount[3] += 4; //Vertex count also controls color count
                        }
                        #endregion
                        #region z
                        //z+
                        if (z + 1 != zw && !voxels[x, y, z + 1])
                        {
                            //Add indices (2 tri's)
                            /*indices[4, indiceCount[4]] = 0;
                            indices[4, indiceCount[4] + 1] = 1;
                            indices[4, indiceCount[4] + 2] = 2;
                            indices[4, indiceCount[4] + 3] = 0;
                            indices[4, indiceCount[4] + 4] = 2;
                            indices[4, indiceCount[4] + 5] = 3;
                            indiceCount[4] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[4].array[vertexCount[4] * 3 + 0] = localPos.X + half.X;
                            verticeArray[4].array[vertexCount[4] * 3 + 1] = localPos.Y + half.Y;
                            verticeArray[4].array[vertexCount[4] * 3 + 2] = localPos.Z + half.Z;

                            verticeArray[4].array[vertexCount[4] * 3 + 3] = localPos.X + half.X;
                            verticeArray[4].array[vertexCount[4] * 3 + 4] = localPos.Y - half.Y;
                            verticeArray[4].array[vertexCount[4] * 3 + 5] = localPos.Z + half.Z;

                            verticeArray[4].array[vertexCount[4] * 3 + 6] = localPos.X - half.X;
                            verticeArray[4].array[vertexCount[4] * 3 + 7] = localPos.Y - half.Y;
                            verticeArray[4].array[vertexCount[4] * 3 + 8] = localPos.Z + half.Z;

                            verticeArray[4].array[vertexCount[4] * 3 + 9] = localPos.X - half.X;
                            verticeArray[4].array[vertexCount[4] * 3 + 10] = localPos.Y + half.Y;
                            verticeArray[4].array[vertexCount[4] * 3 + 11] = localPos.Z + half.Z;

                            //Add colors(4)
                            colorArray[4].array[vertexCount[4] * 3 + 0] = colors[x, y, z].X;
                            colorArray[4].array[vertexCount[4] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[4].array[vertexCount[4] * 3 + 2] = colors[x, y, z].Z;

                            colorArray[4].array[vertexCount[4] * 3 + 3] = colors[x, y, z].X;
                            colorArray[4].array[vertexCount[4] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[4].array[vertexCount[4] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[4].array[vertexCount[4] * 3 + 6] = colors[x, y, z].X;
                            colorArray[4].array[vertexCount[4] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[4].array[vertexCount[4] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[4].array[vertexCount[4] * 3 + 9] = colors[x, y, z].X;
                            colorArray[4].array[vertexCount[4] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[4].array[vertexCount[4] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[4].array[vertexCount[4] * 3 + 0] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 1] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 2] = 1f;

                            normalArray[4].array[vertexCount[4] * 3 + 3] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 4] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 5] = 1f;

                            normalArray[4].array[vertexCount[4] * 3 + 6] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 7] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 8] = 1f;

                            normalArray[4].array[vertexCount[4] * 3 + 9] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 10] = 0f;
                            normalArray[4].array[vertexCount[4] * 3 + 11] = 1f;

                            vertexCount[4] += 4; //Vertex count also controls color count
                        }
                        //z-
                        if (z - 1 != -1 && !voxels[x, y, z - 1])
                        {
                            //Add indices (2 tri's)
                            /*indices[5, indiceCount[5]] = 0;
                            indices[5, indiceCount[5] + 1] = 1;
                            indices[5, indiceCount[5] + 2] = 2;
                            indices[5, indiceCount[5] + 3] = 0;
                            indices[5, indiceCount[5] + 4] = 2;
                            indices[5, indiceCount[5] + 5] = 3;
                            indiceCount[5] += 6;*/

                            //Add vertices(4)
                            //verticeArray[0].array[vertexCount[0]] = new Vector3(scaleFactor.X * (float)x + half.X, scaleFactor.Y * (float)y + half.Y, scaleFactor.Z * (float)z + half.Z);
                            verticeArray[5].array[vertexCount[5] * 3 + 0] = localPos.X - half.X;
                            verticeArray[5].array[vertexCount[5] * 3 + 1] = localPos.Y + half.Y;
                            verticeArray[5].array[vertexCount[5] * 3 + 2] = localPos.Z - half.Z;

                            verticeArray[5].array[vertexCount[5] * 3 + 3] = localPos.X - half.X;
                            verticeArray[5].array[vertexCount[5] * 3 + 4] = localPos.Y - half.Y;
                            verticeArray[5].array[vertexCount[5] * 3 + 5] = localPos.Z - half.Z;

                            verticeArray[5].array[vertexCount[5] * 3 + 6] = localPos.X + half.X;
                            verticeArray[5].array[vertexCount[5] * 3 + 7] = localPos.Y - half.Y;
                            verticeArray[5].array[vertexCount[5] * 3 + 8] = localPos.Z - half.Z;

                            verticeArray[5].array[vertexCount[5] * 3 + 9] = localPos.X + half.X;
                            verticeArray[5].array[vertexCount[5] * 3 + 10] = localPos.Y + half.Y;
                            verticeArray[5].array[vertexCount[5] * 3 + 11] = localPos.Z - half.Z;

                            //Add colors(4)
                            colorArray[5].array[vertexCount[5] * 3 + 0] = colors[x, y, z].X;
                            colorArray[5].array[vertexCount[5] * 3 + 1] = colors[x, y, z].Y;
                            colorArray[5].array[vertexCount[5] * 3 + 2] = colors[x, y, z].Z;

                            colorArray[5].array[vertexCount[5] * 3 + 3] = colors[x, y, z].X;
                            colorArray[5].array[vertexCount[5] * 3 + 4] = colors[x, y, z].Y;
                            colorArray[5].array[vertexCount[5] * 3 + 5] = colors[x, y, z].Z;

                            colorArray[5].array[vertexCount[5] * 3 + 6] = colors[x, y, z].X;
                            colorArray[5].array[vertexCount[5] * 3 + 7] = colors[x, y, z].Y;
                            colorArray[5].array[vertexCount[5] * 3 + 8] = colors[x, y, z].Z;

                            colorArray[5].array[vertexCount[5] * 3 + 9] = colors[x, y, z].X;
                            colorArray[5].array[vertexCount[5] * 3 + 10] = colors[x, y, z].Y;
                            colorArray[5].array[vertexCount[5] * 3 + 11] = colors[x, y, z].Z;

                            //Normals
                            normalArray[5].array[vertexCount[5] * 3 + 0] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 1] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 2] = -1f;
                            
                            normalArray[5].array[vertexCount[5] * 3 + 3] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 4] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 5] = -1f;

                            normalArray[5].array[vertexCount[5] * 3 + 6] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 7] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 8] = -1f;

                            normalArray[5].array[vertexCount[5] * 3 + 9] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 10] = 0f;
                            normalArray[5].array[vertexCount[5] * 3 + 11] = -1f;

                            vertexCount[5] += 4; //Vertex count also controls color count
                        }
                        #endregion
                    }
                }
            }
            //Debug.DebugConsole("Voxelizer", "Successfully generated mesh for " + voxelCount + " voxels");
            uploadData();
        }

        private void uploadData()
        {
            for (int i = 0; i < CUBE_SIDES; i++)
            {
                //GL.BindVertexArray(vaos[i]);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffers[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertexCount[i]*3, verticeArray[i].array, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffers[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertexCount[i] * 3, colorArray[i].array, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffers[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertexCount[i] * 3, normalArray[i].array, BufferUsageHint.StaticDraw);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindVertexArray(0);
            //Debug.DebugConsole("Voxelizer.uploadData", "Uploaded vertices and colors");
        }

        public void plotParticle(Vector3 position, Vector3 color)
        {
            Vector3 coordinatePosition = new Vector3((position.X / size.X) * (float)xw, (position.Y / size.Y) * (float)yw, (position.Z / size.Z) * (float)zw);
            voxels[(int)coordinatePosition.X, (int)coordinatePosition.Y, (int)coordinatePosition.Z] = true;
            colors[(int)coordinatePosition.X, (int)coordinatePosition.Y, (int)coordinatePosition.Z] = color;
        }

        public static float[] fuckOpenTk(Matrix4 matrix)
        {
            float[] returnVal = new float[4 * 4];

            /*returnVal[0] = matrix.Column0.X;
            returnVal[1] = matrix.Column0.Y;
            returnVal[2] = matrix.Column0.Z;
            returnVal[3] = matrix.Column0.W;

            returnVal[4] = matrix.Column1.X;
            returnVal[5] = matrix.Column1.Y;
            returnVal[6] = matrix.Column1.Z;
            returnVal[7] = matrix.Column1.W;

            returnVal[8] = matrix.Column2.X;
            returnVal[9] = matrix.Column2.Y;
            returnVal[10] = matrix.Column2.Z;
            returnVal[11] = matrix.Column2.W;

            returnVal[12] = matrix.Column3.X;
            returnVal[13] = matrix.Column3.Y;
            returnVal[14] = matrix.Column3.Z;
            returnVal[15] = matrix.Column3.W;*/

            //Also transposes matrix
            returnVal[0] = matrix.Column0.X;
            returnVal[1] = matrix.Column1.X;
            returnVal[2] = matrix.Column2.X;
            returnVal[3] = matrix.Column3.X;
            
            returnVal[4] = matrix.Column0.Y;
            returnVal[5] = matrix.Column1.Y;
            returnVal[6] = matrix.Column2.Y;
            returnVal[7] = matrix.Column3.Y;

            returnVal[8] = matrix.Column0.Z;
            returnVal[9] = matrix.Column1.Z;
            returnVal[10] = matrix.Column2.Z;
            returnVal[11] = matrix.Column3.Z;

            returnVal[12] = matrix.Column0.W;
            returnVal[13] = matrix.Column1.W;
            returnVal[14] = matrix.Column2.W;
            returnVal[15] = matrix.Column3.W;

            return returnVal;
        }

        public void render(Context context, Part.RenderPass pass)
        {
            Matrix4 lightVpTransform = context.LightView * context.LightProjection;
            //lightVpTransform.Transpose();

            shaderProgram.use();

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            switch (pass)
            {
                case Part.RenderPass.SHADOW:
                    GL.UniformSubroutines(ShaderType.FragmentShader, 1, new int[] { depthPassSubroutine });
                    //GL.CullFace(CullFaceMode.Front);
                    break;
                case Part.RenderPass.DIFFUSE:
                    GL.UniformSubroutines(ShaderType.FragmentShader, 1, new int[] { diffusePassSubroutine });
                    break;
                case Part.RenderPass.NORMAL:
                    GL.UniformSubroutines(ShaderType.FragmentShader, 1, new int[] { normalPassSubroutine });
                    break;
                default:
                    GL.UniformSubroutines(ShaderType.FragmentShader, 1, new int[] { defaultPassSubroutine });
                    GL.BindTexture(TextureTarget.Texture2D, context.ShadowEffect.ShadowTexture.Handle);
                    GL.ProgramUniform1(shaderProgram.ProgramId, shadowMapTextureUniformLocation, 0);
                    //GL.CullFace(CullFaceMode.Back);
                    break;
            }

            //GL.Color3 (1f, 0f, 0f);
            GL.ProgramUniformMatrix4(shaderProgram.ProgramId, vUniformLocation, 1, false, fuckOpenTk(context.CurrentView));
            GL.ProgramUniformMatrix4(shaderProgram.ProgramId, pUniformLocation, 1, false, fuckOpenTk(context.CurrentProjection));
            GL.ProgramUniformMatrix4(shaderProgram.ProgramId, lightVUniformLocation, 1, false, fuckOpenTk(context.LightView));
            GL.ProgramUniformMatrix4(shaderProgram.ProgramId, lightPUniformLocation, 1, false, fuckOpenTk(context.LightProjection));
            GL.ProgramUniformMatrix4(shaderProgram.ProgramId, mUniformLocation, 1, false, fuckOpenTk(model));

            GL.ProgramUniform3(shaderProgram.ProgramId, lightPosUniformLocation, context.LightPos);

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            for (int i = 0; i < CUBE_SIDES; i++)
            {
            //int i = 5;
                GL.BindVertexArray(vaos[i]);

                GL.DrawArrays(BeginMode.Quads, 0, vertexCount[i]);
            }
            

            GL.BindVertexArray(0);
        }

    }
}
