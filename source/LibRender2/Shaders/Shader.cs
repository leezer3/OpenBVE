using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using OpenBveApi.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Shaders
{
	/// <summary>
	/// Class to represent an OpenGL/OpenTK Shader program
	/// </summary>
	public class Shader : IDisposable
	{
		public static List<Shader> Disposable = new List<Shader>();

		private readonly int handle;
		private int vertexShader;
		private int fragmentShader;
		public readonly VertexLayout VertexLayout;
		public readonly UniformLayout UniformLayout;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="VertexShaderName">file path and name to vertex shader source</param>
		/// <param name="FragmentShaderName">file path and name to fragment shader source</param>
		/// <param name="IsFromStream"></param>
		public Shader(string VertexShaderName, string FragmentShaderName, bool IsFromStream = false)
		{
			int status;
			handle = GL.CreateProgram();

			if (IsFromStream)
			{
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{VertexShaderName}.vert"))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							LoadShader(reader.ReadToEnd(), ShaderType.VertexShader);
						}
					}
				}
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{FragmentShaderName}.frag"))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							LoadShader(reader.ReadToEnd(), ShaderType.FragmentShader);
						}
					}
				}
			}
			else
			{
				LoadShader(File.ReadAllText(VertexShaderName, Encoding.UTF8), ShaderType.VertexShader);
				LoadShader(File.ReadAllText(FragmentShaderName, Encoding.UTF8), ShaderType.FragmentShader);
			}

			GL.AttachShader(handle, vertexShader);
			GL.AttachShader(handle, fragmentShader);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);

			GL.LinkProgram(handle);
			GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out status);

			if (status == 0)
			{
				throw new ApplicationException(GL.GetProgramInfoLog(handle));
			}

			VertexLayout = GetVertexLayout();
			UniformLayout = GetUniformLayout();

			Disposable.Add(this);
		}

		/// <summary>
		/// Loads the shader source and compiles the shader 
		/// </summary>
		/// <param name="shaderSource">Shader source code string</param>
		/// <param name="shaderType">type of shader VertexShader or FragmentShader</param>
		private void LoadShader(string shaderSource, ShaderType shaderType)
		{
			int status;

			if (shaderType == ShaderType.VertexShader)
			{
				vertexShader = GL.CreateShader(shaderType);
				GL.ShaderSource(vertexShader, shaderSource);
				GL.CompileShader(vertexShader);
				GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);

				if (status == 0)
				{
					throw new ApplicationException(GL.GetShaderInfoLog(vertexShader));
				}
			}

			if (shaderType == ShaderType.FragmentShader)
			{
				fragmentShader = GL.CreateShader(shaderType);
				GL.ShaderSource(fragmentShader, shaderSource);
				GL.CompileShader(fragmentShader);
				GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);

				if (status == 0)
				{
					throw new ApplicationException(GL.GetShaderInfoLog(fragmentShader));
				}
			}
		}

		/// <summary>
		/// Activate the shader program for use
		/// </summary>
		public void Use()
		{
			GL.UseProgram(handle);
		}

		public VertexLayout GetVertexLayout()
		{
			return new VertexLayout
			{
				Position = GL.GetAttribLocation(handle, "iPosition"),
				Normal = GL.GetAttribLocation(handle, "iNormal"),
				UV = GL.GetAttribLocation(handle, "iUv"),
				Color = GL.GetAttribLocation(handle, "iColor")
			};
		}

		public UniformLayout GetUniformLayout()
		{
			return new UniformLayout
			{
				CurrentProjectionMatrix = GL.GetUniformLocation(handle, "uCurrentProjectionMatrix"),
				CurrentModelViewMatrix = GL.GetUniformLocation(handle, "uCurrentModelViewMatrix"),
				CurrentNormalMatrix = GL.GetUniformLocation(handle, "uCurrentNormalMatrix"),
				CurrentTextureMatrix = GL.GetUniformLocation(handle, "uCurrentTextureMatrix"),
				IsLight = GL.GetUniformLocation(handle, "uIsLight"),
				LightPosition = GL.GetUniformLocation(handle, "uLight.position"),
				LightAmbient = GL.GetUniformLocation(handle, "uLight.ambient"),
				LightDiffuse = GL.GetUniformLocation(handle, "uLight.diffuse"),
				LightSpecular = GL.GetUniformLocation(handle, "uLight.specular"),
				MaterialAmbient = GL.GetUniformLocation(handle, "uMaterial.ambient"),
				MaterialDiffuse = GL.GetUniformLocation(handle, "uMaterial.diffuse"),
				MaterialSpecular = GL.GetUniformLocation(handle, "uMaterial.specular"),
				MaterialEmission = GL.GetUniformLocation(handle, "uMaterial.emission"),
				MaterialShininess = GL.GetUniformLocation(handle, "uMaterial.shininess"),
				IsFog = GL.GetUniformLocation(handle, "uIsFog"),
				FogStart = GL.GetUniformLocation(handle, "uFogStart"),
				FogEnd = GL.GetUniformLocation(handle, "uFogEnd"),
				FogColor = GL.GetUniformLocation(handle, "uFogColor"),
				IsTexture = GL.GetUniformLocation(handle, "uIsTexture"),
				Texture = GL.GetUniformLocation(handle, "uTexture"),
				Brightness = GL.GetUniformLocation(handle, "uBrightness"),
				Opacity = GL.GetUniformLocation(handle, "uOpacity"),
				ObjectIndex = GL.GetUniformLocation(handle, "uObjectIndex")
			};
		}

		public void NonUse()
		{
			GL.UseProgram(0);
		}

		/// <summary>
		/// cleans up, releasing the underlying openTK/OpenGL shader program
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				GL.DeleteProgram(handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}

		private Matrix4 ConvertToMatrix4(Matrix4d mat)
		{
			return new Matrix4(
				(float)mat.M11, (float)mat.M12, (float)mat.M13, (float)mat.M14,
				(float)mat.M21, (float)mat.M22, (float)mat.M23, (float)mat.M24,
				(float)mat.M31, (float)mat.M32, (float)mat.M33, (float)mat.M34,
				(float)mat.M41, (float)mat.M42, (float)mat.M43, (float)mat.M44
			);
		}

		#region SetUniform

		/// <summary>
		/// Set the projection matrix
		/// </summary>
		/// <param name="ProjectionMatrix"></param>
		public void SetCurrentProjectionMatrix(Matrix4d ProjectionMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(ProjectionMatrix);
			GL.UniformMatrix4(UniformLayout.CurrentProjectionMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the model view matrix
		/// </summary>
		/// <param name="ModelViewMatrix"></param>
		public void SetCurrentModelViewMatrix(Matrix4d ModelViewMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(ModelViewMatrix);
			GL.UniformMatrix4(UniformLayout.CurrentModelViewMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the normal matrix
		/// </summary>
		/// <param name="NormalMatrix"></param>
		public void SetCurrentNormalMatrix(Matrix4d NormalMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(NormalMatrix);
			GL.UniformMatrix4(UniformLayout.CurrentNormalMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the texture matrix
		/// </summary>
		/// <param name="TextureMatrix"></param>
		public void SetCurrentTextureMatrix(Matrix4d TextureMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(TextureMatrix);
			GL.UniformMatrix4(UniformLayout.CurrentTextureMatrix, false, ref matrix);
		}

		public void SetIsLight(bool IsLight)
		{
			GL.Uniform1(UniformLayout.IsLight, IsLight ? 1 : 0);
		}

		public void SetLightPosition(Vector3 LightPosition)
		{
			GL.Uniform3(UniformLayout.LightPosition, LightPosition);
		}

		public void SetLightAmbient(Color4 LightAmbient)
		{
			GL.Uniform4(UniformLayout.LightAmbient, LightAmbient);
		}

		public void SetLightDiffuse(Color4 LightDiffuse)
		{
			GL.Uniform4(UniformLayout.LightDiffuse, LightDiffuse);
		}

		public void SetLightSpecular(Color4 LightSpecular)
		{
			GL.Uniform4(UniformLayout.LightSpecular, LightSpecular);
		}

		public void SetMaterialAmbient(Color4 MaterialAmbient)
		{
			GL.Uniform4(UniformLayout.MaterialAmbient, MaterialAmbient);
		}

		public void SetMaterialDiffuse(Color4 MaterialDiffuse)
		{
			GL.Uniform4(UniformLayout.MaterialDiffuse, MaterialDiffuse);
		}

		public void SetMaterialSpecular(Color4 MaterialSpecular)
		{
			GL.Uniform4(UniformLayout.MaterialSpecular, MaterialSpecular);
		}

		public void SetMaterialEmission(Color4 MaterialEmission)
		{
			GL.Uniform4(UniformLayout.MaterialEmission, MaterialEmission);
		}

		public void SetMaterialShininess(float MaterialShininess)
		{
			GL.Uniform1(UniformLayout.MaterialShininess, MaterialShininess);
		}

		public void SetIsFog(bool IsFog)
		{
			GL.Uniform1(UniformLayout.IsFog, IsFog ? 1 : 0);
		}

		public void SetFogStart(float FogStart)
		{
			GL.Uniform1(UniformLayout.FogStart, FogStart);
		}

		public void SetFogEnd(float FogEnd)
		{
			GL.Uniform1(UniformLayout.FogEnd, FogEnd);
		}

		public void SetFogColor(Color4 FogColor)
		{
			GL.Uniform4(UniformLayout.FogColor, FogColor);
		}

		public void SetIsTexture(bool IsTexture)
		{
			GL.Uniform1(UniformLayout.IsTexture, IsTexture ? 1 : 0);
		}

		public void SetTexture(int TextureUnit)
		{
			GL.Uniform1(UniformLayout.Texture, TextureUnit);
		}

		public void SetBrightness(float Brightness)
		{
			GL.Uniform1(UniformLayout.Brightness, Brightness);
		}

		public void SetOpacity(float Opacity)
		{
			GL.Uniform1(UniformLayout.Opacity, Opacity);
		}

		public void SetObjectIndex(int ObjectIndex)
		{
			GL.Uniform1(UniformLayout.ObjectIndex, ObjectIndex);
		}

		#endregion
	}
}
