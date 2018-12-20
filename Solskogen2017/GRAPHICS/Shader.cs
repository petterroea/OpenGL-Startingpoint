using System;
using OpenTK.Graphics.OpenGL;

namespace Solskogen2017.GRAPHICS
{
	public class Shader
	{
		public int ShaderId { get; private set;}
		public Shader (ShaderType type, string path)
		{
			ShaderId = GL.CreateShader (type);
			Console.WriteLine ("Creating shader type " + type.ToString () + " with id " + ShaderId);
			string lines = System.IO.File.ReadAllText ("../../" + path);
			GL.ShaderSource (ShaderId, lines);
			GL.CompileShader (ShaderId);

			int status;
			GL.GetShader(ShaderId, ShaderParameter.CompileStatus, out status);
			if (status == 0) {
				Console.WriteLine ("Error compiling " + type.ToString() +"!");
				string log;
				GL.GetShaderInfoLog (ShaderId, out log);
				Console.WriteLine (log);
				throw new ApplicationException ("Shader compilation error: \n" + log);
			}
		}
	}
}

