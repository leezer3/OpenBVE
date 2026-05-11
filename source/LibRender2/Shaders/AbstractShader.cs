//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Christopher Lees, S520, Aditiya Afrizal, The OpenBVE Project
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

using LibRender2.Fogs;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace LibRender2.Shaders
{
	public class AbstractShader : IDisposable
	{
		/// <summary>The OpenGL handle to the complete shader</summary>
		internal readonly int Handle;
		/// <summary>The OpenGL handle to the vertex shader component</summary>
		internal int VertexShader;
		/// <summary>The OpenGL handle to the fragment shader component</summary>
		internal int FragmentShader;
		/// <summary>Holds a reference to the base renderer</summary>
		internal readonly BaseRenderer Renderer;

		internal bool IsActive;
		public AbstractShader(BaseRenderer renderer, string vertexShaderName, string fragmentShaderName, bool isFromStream, bool fragColor)
		{
			Handle = GL.CreateProgram();
			Renderer = renderer;
			if (isFromStream)
			{
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{vertexShaderName}.vert"))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						{
							LoadShader(reader.ReadToEnd(), ShaderType.VertexShader);
						}
					}
				}
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{fragmentShaderName}.frag"))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						{
							LoadShader(reader.ReadToEnd(), ShaderType.FragmentShader);
						}
					}
				}
			}
			else
			{
				LoadShader(File.ReadAllText(vertexShaderName, Encoding.UTF8), ShaderType.VertexShader);
				LoadShader(File.ReadAllText(fragmentShaderName, Encoding.UTF8), ShaderType.FragmentShader);
			}
			GL.AttachShader(Handle, VertexShader);
			GL.AttachShader(Handle, FragmentShader);

			GL.DeleteShader(VertexShader);
			GL.DeleteShader(FragmentShader);
			if (fragColor)
			{
				GL.BindFragDataLocation(Handle, 0, "fragColor");
			}
			GL.LinkProgram(Handle);

			GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
			if (success == 0)
			{
				string infoLog = GL.GetProgramInfoLog(Handle);
				throw new Exception($"Shader Link error: {infoLog}");
			}
		}

		/// <summary>Loads the shader source and compiles the shader</summary>
		/// <param name="shaderSource">Shader source code string</param>
		/// <param name="shaderType">type of shader VertexShader or FragmentShader</param>
		internal void LoadShader(string shaderSource, ShaderType shaderType)
		{
			int status;

			switch (shaderType)
			{
				case ShaderType.VertexShader:
					VertexShader = GL.CreateShader(shaderType);
					GL.ShaderSource(VertexShader, shaderSource);
					GL.CompileShader(VertexShader);
					GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out status);
					if (status == 0)
					{
						throw new ApplicationException(GL.GetShaderInfoLog(VertexShader));
					}
					break;
				case ShaderType.FragmentShader:

					FragmentShader = GL.CreateShader(shaderType);
					GL.ShaderSource(FragmentShader, shaderSource);
					GL.CompileShader(FragmentShader);
					GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out status);

					if (status == 0)
					{
						throw new ApplicationException(GL.GetShaderInfoLog(FragmentShader));
					}
					break;
				default:
					throw new InvalidOperationException("Attempted to load an unknown shader type");
			}
		}


		/// <summary>Activates the shader program for use</summary>
		public virtual void Activate()
		{
			if (IsActive)
			{
				return;
			}

			if (Renderer.CurrentShader != null)
			{
				Renderer.CurrentShader.IsActive = false;
			}
			GL.UseProgram(Handle);
			IsActive = true;
			Renderer.lastVAO = -1;
			Renderer.CurrentShader = this;
			Renderer.RestoreAlphaFunc();
		}

		/// <summary>Deactivates the shader</summary>
		public void Deactivate()
		{
			if (!IsActive)
			{
				return;
			}
			IsActive = false;
			GL.UseProgram(0);
			Renderer.lastVAO = -1;
		}

		/// <summary>Specifies the OpenGL alpha function to perform</summary>
		/// <param name="alphaFunction">The comparison to use</param>
		/// <param name="alphaComparison">The value to compare</param>
		public virtual void SetAlphaFunction(AlphaFunction alphaFunction, float alphaComparison)
		{

		}

		/// <summary>Sets whether AlphaTesting is enabled</summary>
		public virtual void SetAlphaTest(bool enabled)
		{

		}

		/// <summary>Sets whether Fog is enabled</summary>
		public virtual void SetFog(bool enabled)
		{
		}

		/// <summary>Sets the current fog</summary>
		public virtual void SetFog(Fog Fog)
		{
		}

		private bool disposed;

		/// <summary>Cleans up, releasing the underlying openTK/OpenGL shader program</summary>
		public void Dispose()
		{
			if (!disposed)
			{
				GL.DeleteProgram(Handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}
	}
}
