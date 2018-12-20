using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using Solskogen2017.MATH;
using Solskogen2017.GRAPHICS;

namespace Solskogen2017.EFFECTS
{
    class VoxelizerPart : Part
    {
        private Voxelizer[] voxels = new Voxelizer[5];
        private Voxelizer voxel;
        int voxelX = 50;
        int voxelY = 50;
        int voxelZ = 50;
        public VoxelizerPart()
        {
            voxel = new Voxelizer(voxelX, voxelY, voxelZ, new Vector3(voxelX*2f, voxelY * 2f, voxelZ * 2f), new Vector3(0f, 0f, 0f));
            /*for (int i = 0; i < voxels.Length; i++)
            {
                voxels[i] = new Voxelizer(voxelX, voxelY, voxelZ, new Vector3(voxelX, voxelY, voxelZ), new Vector3(0f, 0f, voxelZ*i));
                /*voxel.plotParticle(new Vector3(1f, 1f, 1f), new Vector3(1f, 0f, 0f));
                voxel.plotParticle(new Vector3(1f, 1f, 2f), new Vector3(0f, 1f, 0f));
                voxel.plotParticle(new Vector3(1f, 1f, 3f), new Vector3(0f, 0f, 1f));*/
                /*Vector3 color = new Vector3(0.2f, 0.2f, 1.0f);
                PerlinNoise noise = new PerlinNoise(69);
                for (int x = 0; x < voxelX; x++)
                {
                    for (int y = 0; y < voxelY; y++)
                    {
                        for (int z = 0; z < voxelZ; z++)
                        {
                            Vector3 center = new Vector3(voxelX / 2, z, voxelZ / 2);
                            float noiseValue = (noise.Noise3D((float)x * 0.03f, (float)y * 0.03f, (float)(z+i*voxelZ) * 0.03f)+
                                                noise.Noise3D((float)x * 0.01f, (float)y * 0.01f, (float)(z + i * voxelZ) * 0.01f)+
                                                noise.Noise3D((float)x * 0.04f, (float)y * 0.05f, (float)(z + i * voxelZ) * 0.05f)
                                ) /3f;
                            float distCenter = (float)Math.Sqrt(Math.Pow((float)x - (voxelX / 2f), 2) + Math.Pow((float)y - (voxelY / 2f), 2) + Math.Pow((float)z - (z), 2)) / 35f;
                            voxels[i].Voxels[x, y, z] = distCenter * 1f + (noiseValue - 0.5f) * 0.4f < 0.5f;
                            voxels[i].Colors[x, y, z] = color;
                        }
                    }
                }
                voxels[i].generateMesh();
            }*/
        }
        private double func(Vector3 dist, float ballRadius)
        {
            double rp = dist.X* dist.X+ dist.Y+ dist.Y+ dist.Z* dist.Z;

            return Math.Pow(1f-Math.Pow(Math.Min(dist.Length/ballRadius, 1), 3f), 3f);
        }
        public override void tick(float dt, float time)
        {
            Vector3 color = new Vector3(0.2f, 0.2f, 1.0f);
            Vector3[] positions = new Vector3[]
            {
                new Vector3(25, 25, 25),
                new Vector3((float)Math.Sin(time)*10f+25f, (float)Math.Cos(time*1.2f)*15f+25f, (float)Math.Sin(time*0.2f)*10f+20f),
                new Vector3((float)Math.Sin(time)*15f+15f, (float)Math.Cos(time*1.2f)*5f+25f, (float)Math.Sin(time*0.2f)*20f+30f)
            };
            /*Vector3[] positions = new Vector3[]
            {
                new Vector3(25, 25, 25),
                new Vector3(40, 25, 25),
                new Vector3(25, 25, 40)
            };*/
            for (int x = 0; x < voxelX; x++)
            {
                for (int y = 0; y < voxelY; y++)
                {
                    for (int z = 0; z < voxelZ; z++)
                    {
                        double accum = 0.0;
                        for (int v = 0; v < positions.Length; v++)
                        {
                            accum += func(new Vector3(positions[v].X - x, positions[v].Y - y, positions[v].Z - z), 10f);
                        }
                        voxel.Voxels[x, y, z] = accum < 0.5f;
                        voxel.Colors[x, y, z] = color;
                    }
                }
            }
            voxel.generateMesh();
        }
        public override void render(Context context, RenderPass pass, float dt, float time)
        {
            //context.LightPos = new Vector3(50f+(float)Math.Sin(time)*50f, 50f, 50f + (float)Math.Cos(time) * 50f);
            /*for(int i = 0; i < voxels.Length; i++)
            {
                voxels[i].render(context);
            }*/
            voxel.render(context, pass);
        }
    }
}
