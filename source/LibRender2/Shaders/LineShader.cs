//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Shaders
{
	/// <summary>Shader used to draw untextured line strips, such as the rail path overlay.</summary>
	public class LineShader : AbstractShader
	{
		private readonly int uCurrentProjectionMatrix;
		private readonly int uCurrentModelViewMatrix;
		private readonly int uColor;

		public readonly VertexLayout VertexLayout;

		public LineShader(BaseRenderer renderer) : base(renderer, "line", "line", true, true)
		{
			uCurrentProjectionMatrix = GL.GetUniformLocation(Handle, "uCurrentProjectionMatrix");
			uCurrentModelViewMatrix = GL.GetUniformLocation(Handle, "uCurrentModelViewMatrix");
			uColor = GL.GetUniformLocation(Handle, "uColor");
			VertexLayout = new VertexLayout
			{
				Position = (short)GL.GetAttribLocation(Handle, "iPosition")
			};
		}

		public void SetCurrentProjectionMatrix(Matrix4D matrix)
		{
			SetMatrix(uCurrentProjectionMatrix, matrix);
		}

		public void SetCurrentModelViewMatrix(Matrix4D matrix)
		{
			SetMatrix(uCurrentModelViewMatrix, matrix);
		}

		public void SetColor(Color128 color)
		{
			GL.ProgramUniform4(Handle, uColor, color.R, color.G, color.B, color.A);
		}

		private void SetMatrix(int location, Matrix4D mat)
		{
			Matrix4 matrix = new Matrix4(
				(float)mat.Row0.X, (float)mat.Row0.Y, (float)mat.Row0.Z, (float)mat.Row0.W,
				(float)mat.Row1.X, (float)mat.Row1.Y, (float)mat.Row1.Z, (float)mat.Row1.W,
				(float)mat.Row2.X, (float)mat.Row2.Y, (float)mat.Row2.Z, (float)mat.Row2.W,
				(float)mat.Row3.X, (float)mat.Row3.Y, (float)mat.Row3.Z, (float)mat.Row3.W
			);
			GL.ProgramUniformMatrix4(Handle, location, false, ref matrix);
		}
	}
}
