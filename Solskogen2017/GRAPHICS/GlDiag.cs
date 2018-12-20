using System;
using OpenTK.Graphics.OpenGL;

namespace Solskogen2017.GRAPHICS
{
	public class GlDiag
	{
		public static void PrintDiag()
		{
			Console.WriteLine ("### OPENGL DIAGNOSTICS ###");
			Console.WriteLine ("---General---");
			Console.WriteLine ("OpenGL Major version                  : " + GL.GetInteger (GetPName.MajorVersion));
			Console.WriteLine ("OpenGL Minor version                  : " + GL.GetInteger (GetPName.MinorVersion));
			Console.WriteLine ("---Capabilities---");
			Console.WriteLine ("GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS   : " + GL.GetInteger (GetPName.MaxCombinedTextureImageUnits));
			Console.WriteLine ("GL_MAX_VERTEX_ATTRIBS                 :" + GL.GetInteger(GetPName.MaxVertexAttribs));
		}
	}
}

