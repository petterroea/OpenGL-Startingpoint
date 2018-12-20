using System;
using OpenTK;
using Solskogen2017.GRAPHICS;
using Solskogen2017.MATH;

namespace Solskogen2017.EFFECTS
{
	class MagicaVoxelPart : Part
	{
		private Voxelizer[] voxels;
		int voxelX = 128;
		int voxelY = 64;
		int voxelZ = 128;

		public MagicaVoxelPart()
		{
			voxels = MagicaVoxelImporter.readMagicaFile("monu10.vox", new Vector3(50f, 50f, 50f));
            for(int i = 0; i < voxels.Length; i++)
            {
                voxels[i].generateMesh();
                voxels[i].Position = new Vector3(-25f, 0f, -25f);
            }
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
            for(int i = 0; i < voxels.Length; i++)
            {
                voxels[i].render(context, pass);
            }
			//voxel.render(context, pass);
		}
	}
}
