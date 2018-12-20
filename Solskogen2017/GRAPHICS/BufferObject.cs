using System;
using OpenTK.Graphics.OpenGL;

namespace Solskogen2017.GRAPHICS
{
	public class BufferObject
	{
		public int BufferId { get; private set; }
		public BufferTarget Target { get; private set;}
		public BufferObject (BufferTarget target)
		{
			BufferId = GL.GenBuffer ();
			Target = target;
		}

		public void uploadFloats(float[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
		{
			GL.BindBuffer (Target, BufferId);
			GL.BufferData(Target, new IntPtr(data.Length*sizeof(float)), data, usage);
		}

		public void uploadInts(int[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
		{
			GL.BindBuffer (Target, BufferId);
			GL.BufferData (Target, new IntPtr(data.Length*sizeof(int)), data, usage);
		}

		public void dispose()
		{
			GL.DeleteBuffer (BufferId);
		}

		public void bind()
		{
			GL.BindBuffer (Target, BufferId);
		}
	}
}

