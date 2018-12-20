using System;
using OpenTK.Graphics.OpenGL;

namespace Solskogen2017.GRAPHICS
{
	public class Program
	{
		public int ProgramId { get; private set;}
		public Program ()
		{
			ProgramId = GL.CreateProgram ();
		}
		public void attatchShader(Shader shader) 
		{
			GL.AttachShader (ProgramId, shader.ShaderId);
		}
		public void link() 
		{
			GL.LinkProgram (ProgramId);
			int status;
			GL.GetProgram (ProgramId, GetProgramParameterName.LinkStatus, out status);
			if (status == 0) {
				string log = GL.GetProgramInfoLog (ProgramId);
				Console.WriteLine ("Failed linking!\n " + log);
				throw new ApplicationException (log);
			}
		}
		public void use()
		{
			GL.UseProgram (ProgramId);
		}
	}
}

