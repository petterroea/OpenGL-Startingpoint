using System;
using OpenTK;
using Solskogen2017.GRAPHICS;
using Solskogen2017.MATH;

namespace Solskogen2017.EFFECTS
{
    class TerrainPart : Part
    {
		private Voxelizer voxel;
		int voxelX = 128;
		int voxelY = 64;
		int voxelZ = 128;

		public TerrainPart()
		{
            voxel = new Voxelizer(voxelX, voxelY, voxelZ, new Vector3(voxelX * 1f, voxelY * 1f, voxelZ * 1f), new Vector3(voxelX / -2f, 0f, voxelZ / -2f));
            PerlinNoise noise = new PerlinNoise(1);

            Vector3 grassColor = new Vector3(139f / 255f, 195f / 255f, 74f / 255f);
            Vector3 dirtColor = new Vector3(121f / 255f, 85f / 255f, 72f / 255f);


            for (int x = 0; x < voxelX; x++)
            {
                for (int z = 0; z < voxelZ; z++)
                {
                    float n = noise.Perlin2D((float)x, (float)z, 0.003f) * 0.5f +
                                   noise.Perlin2D((float)x, (float)z, 0.03f) * 0.1f;


                    n = n * voxelY;
                    for (int y = 0; y < voxelY; y++)
                    {
						voxel.Voxels[x, y, z] = y>(int)n;
                        voxel.Colors[x, y, z] = n-5>y?dirtColor:grassColor;
                    }
                }
            }
            voxel.generateMesh();
		}
		private double func(Vector3 dist, float ballRadius)
		{
			double rp = dist.X * dist.X + dist.Y + dist.Y + dist.Z * dist.Z;

			return Math.Pow(1f - Math.Pow(Math.Min(dist.Length / ballRadius, 1), 3f), 3f);
		}
		public override void tick(float dt, float time)
		{
			
			//voxel.generateMesh();
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
