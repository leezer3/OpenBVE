using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using LibRender2.Fogs;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using Vector4 = OpenBveApi.Math.Vector4;

namespace LibRender2.Shaders
{
	/// <summary>
	/// Class to represent an OpenGL/OpenTK Shader program
	/// </summary>
	public class Shader : IDisposable
	{
		private readonly int handle;
		private int vertexShader;
		private int fragmentShader;
		public readonly VertexLayout VertexLayout;
		public readonly UniformLayout UniformLayout;
		private bool disposed;
		private bool isActive;
		private readonly BaseRenderer renderer;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Renderer">A reference to the base renderer</param>
		/// <param name="VertexShaderName">file path and name to vertex shader source</param>
		/// <param name="FragmentShaderName">file path and name to fragment shader source</param>
		/// <param name="IsFromStream"></param>
		public Shader(BaseRenderer Renderer, string VertexShaderName, string FragmentShaderName, bool IsFromStream = false)
		{
			renderer = Renderer;
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
		}

		/// <summary>Loads the shader source and compiles the shader</summary>
		/// <param name="shaderSource">Shader source code string</param>
		/// <param name="shaderType">type of shader VertexShader or FragmentShader</param>
		private void LoadShader(string shaderSource, ShaderType shaderType)
		{
			int status;

			switch (shaderType)
			{
				case ShaderType.VertexShader:
					vertexShader = GL.CreateShader(shaderType);
					GL.ShaderSource(vertexShader, shaderSource);
					GL.CompileShader(vertexShader);
					GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
					if (status == 0)
					{
						throw new ApplicationException(GL.GetShaderInfoLog(vertexShader));
					}
					break;
				case ShaderType.FragmentShader:
				
					fragmentShader = GL.CreateShader(shaderType);
					GL.ShaderSource(fragmentShader, shaderSource);
					GL.CompileShader(fragmentShader);
					GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);

					if (status == 0)
					{
						throw new ApplicationException(GL.GetShaderInfoLog(fragmentShader));
					}
					break;
				default:
					throw new InvalidOperationException("Attempted to load an unknown shader type");
			}
		}

		/// <summary>Activates the shader program for use</summary>
		public void Activate()
		{
			if (isActive)
			{
				return;
			}
			GL.UseProgram(handle);
			isActive = true;
			renderer.lastVAO = -1;
			renderer.CurrentShader = this;
		}

		public VertexLayout GetVertexLayout()
		{
			return new VertexLayout
			{
				Position = (short)GL.GetAttribLocation(handle, "iPosition"),
				Normal = (short)GL.GetAttribLocation(handle, "iNormal"),
				UV = (short)GL.GetAttribLocation(handle, "iUv"),
				Color = (short)GL.GetAttribLocation(handle, "iColor")
			};
		}

		public UniformLayout GetUniformLayout()
		{
			return new UniformLayout
			{
				CurrentProjectionMatrix = (short)GL.GetUniformLocation(handle, "uCurrentProjectionMatrix"),
				CurrentModelViewMatrix = (short)GL.GetUniformLocation(handle, "uCurrentModelViewMatrix"),
				CurrentTextureMatrix = (short)GL.GetUniformLocation(handle, "uCurrentTextureMatrix"),
				IsLight = (short)GL.GetUniformLocation(handle, "uIsLight"),
				LightPosition = (short)GL.GetUniformLocation(handle, "uLight.position"),
				LightAmbient = (short)GL.GetUniformLocation(handle, "uLight.ambient"),
				LightDiffuse = (short)GL.GetUniformLocation(handle, "uLight.diffuse"),
				LightSpecular = (short)GL.GetUniformLocation(handle, "uLight.specular"),
				LightModel = (short)GL.GetUniformLocation(handle, "uLight.lightModel"),
				MaterialAmbient = (short)GL.GetUniformLocation(handle, "uMaterial.ambient"),
				MaterialDiffuse = (short)GL.GetUniformLocation(handle, "uMaterial.diffuse"),
				MaterialSpecular = (short)GL.GetUniformLocation(handle, "uMaterial.specular"),
				MaterialEmission = (short)GL.GetUniformLocation(handle, "uMaterial.emission"),
				MaterialShininess = (short)GL.GetUniformLocation(handle, "uMaterial.shininess"),
				MaterialFlags = (short)GL.GetUniformLocation(handle, "uMaterialFlags"),
				MaterialIsAdditive = (short)GL.GetUniformLocation(handle, "uIsAdditive"),
				IsFog = (short)GL.GetUniformLocation(handle, "uIsFog"),
				FogStart = (short)GL.GetUniformLocation(handle, "uFogStart"),
				FogEnd = (short)GL.GetUniformLocation(handle, "uFogEnd"),
				FogColor = (short)GL.GetUniformLocation(handle, "uFogColor"),
				FogIsLinear = (short)GL.GetUniformLocation(handle, "uFogIsLinear"),
				FogDensity = (short)GL.GetUniformLocation(handle, "uFogDensity"),
				IsTexture = (short)GL.GetUniformLocation(handle, "uIsTexture"),
				Texture = (short)GL.GetUniformLocation(handle, "uTexture"),
				Brightness = (short)GL.GetUniformLocation(handle, "uBrightness"),
				Opacity = (short)GL.GetUniformLocation(handle, "uOpacity"),
				ObjectIndex = (short)GL.GetUniformLocation(handle, "uObjectIndex")
			};
		}

		/// <summary>Deactivates the shader</summary>
		public void Deactivate()
		{
			isActive = false;
			GL.UseProgram(0);
			renderer.lastVAO = -1;
		}

		/// <summary>Cleans up, releasing the underlying openTK/OpenGL shader program</summary>
		public void Dispose()
		{
			if (!disposed)
			{
				GL.DeleteProgram(handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}

		private Matrix4 ConvertToMatrix4(Matrix4D mat)
		{
			return new Matrix4(
				(float)mat.Row0.X, (float)mat.Row0.Y, (float)mat.Row0.Z, (float)mat.Row0.W,
				(float)mat.Row1.X, (float)mat.Row1.Y, (float)mat.Row1.Z, (float)mat.Row1.W,
				(float)mat.Row2.X, (float)mat.Row2.Y, (float)mat.Row2.Z, (float)mat.Row2.W,
				(float)mat.Row3.X, (float)mat.Row3.Y, (float)mat.Row3.Z, (float)mat.Row3.W
			);
		}

		#region SetUniform

		/// <summary>
		/// Set the projection matrix
		/// </summary>
		/// <param name="ProjectionMatrix"></param>
		public void SetCurrentProjectionMatrix(Matrix4D ProjectionMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(ProjectionMatrix);
			GL.UniformMatrix4(UniformLayout.CurrentProjectionMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the model view matrix
		/// </summary>
		/// <param name="ModelViewMatrix">
		/// <para>The model view matrix computed with row-major</para>
		/// <para>ScaleMatrix * RotateMatrix * TranslationMatrix * ViewMatrix</para>
		/// </param>
		public void SetCurrentModelViewMatrix(Matrix4D ModelViewMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(ModelViewMatrix);

			// When transpose is false, B is equal to the transposed matrix of A.
			// B = transpose(A) = transpose(M * V) = transpose(V) * transpose(M)
			//
			// The symbols are defined as follows:
			// M: ModelMatrix, V: ViewMatrix
			//
			// Matrix4 (row-major)
			// A =
			// | m11 m12 m13 m14 |
			// | m21 m22 m23 m24 |
			// | m31 m32 m33 m34 |
			// | m41 m42 m43 m44 |
			//
			// OpenGL (column-major)
			// B =
			// | m11 m21 m31 m41 |
			// | m12 m22 m32 m42 |
			// | m13 m23 m33 m43 |
			// | m14 m24 m34 m44 |
			GL.UniformMatrix4(UniformLayout.CurrentModelViewMatrix, false, ref matrix);
		}
		
		/// <summary>
		/// Set the texture matrix
		/// </summary>
		/// <param name="TextureMatrix"></param>
		public void SetCurrentTextureMatrix(Matrix4D TextureMatrix)
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
			GL.Uniform3(UniformLayout.LightPosition, (float)LightPosition.X, (float)LightPosition.Y, (float)LightPosition.Z);
		}

		public void SetLightAmbient(Color24 LightAmbient)
		{
			GL.Uniform3(UniformLayout.LightAmbient, LightAmbient.R / 255.0f, LightAmbient.G / 255.0f, LightAmbient.B / 255.0f);
		}

		public void SetLightDiffuse(Color24 LightDiffuse)
		{
			GL.Uniform3(UniformLayout.LightDiffuse, LightDiffuse.R / 255.0f, LightDiffuse.G / 255.0f, LightDiffuse.B / 255.0f);
		}

		public void SetLightSpecular(Color24 LightSpecular)
		{
			GL.Uniform3(UniformLayout.LightSpecular, LightSpecular.R / 255.0f, LightSpecular.G / 255.0f, LightSpecular.B / 255.0f);
		}

		public void SetLightModel(Vector4 LightModel)
		{
			GL.Uniform4(UniformLayout.LightModel, (float)LightModel.X, (float)LightModel.Y, (float)LightModel.Z, (float)LightModel.W);
		}

		public void SetMaterialAmbient(Color32 MaterialAmbient)
		{
			GL.Uniform4(UniformLayout.MaterialAmbient, MaterialAmbient.R / 255.0f, MaterialAmbient.G / 255.0f, MaterialAmbient.B / 255.0f, MaterialAmbient.A / 255.0f);
		}

		public void SetMaterialDiffuse(Color32 MaterialDiffuse)
		{
			GL.Uniform4(UniformLayout.MaterialDiffuse, MaterialDiffuse.R / 255.0f, MaterialDiffuse.G / 255.0f, MaterialDiffuse.B / 255.0f, MaterialDiffuse.A / 255.0f);
		}

		public void SetMaterialSpecular(Color32 MaterialSpecular)
		{
			GL.Uniform4(UniformLayout.MaterialSpecular, MaterialSpecular.R / 255.0f, MaterialSpecular.G / 255.0f, MaterialSpecular.B / 255.0f, MaterialSpecular.A / 255.0f);
		}

		public void SetMaterialEmission(Color24 MaterialEmission)
		{
			GL.Uniform3(UniformLayout.MaterialEmission, MaterialEmission.R / 255.0f, MaterialEmission.G / 255.0f, MaterialEmission.B / 255.0f);
		}

		public void SetMaterialShininess(float MaterialShininess)
		{
			GL.Uniform1(UniformLayout.MaterialShininess, MaterialShininess);
		}

		public void SetMaterialFlags(MaterialFlags Flags)
		{
			GL.Uniform1(UniformLayout.MaterialFlags, (int)Flags);
		}

		public void SetIsFog(bool IsFog)
		{
			GL.Uniform1(UniformLayout.IsFog, IsFog ? 1 : 0);
		}

		public void SetFog(Fog Fog)
		{
			GL.Uniform1(UniformLayout.FogStart, Fog.Start);
			GL.Uniform1(UniformLayout.FogEnd, Fog.End);
			GL.Uniform3(UniformLayout.FogColor, Fog.Color.R / 255.0f, Fog.Color.G / 255.0f, Fog.Color.B / 255.0f);
			GL.Uniform1(UniformLayout.FogIsLinear, Fog.IsLinear ? 1 : 0);
			GL.Uniform1(UniformLayout.FogDensity, Fog.Density);
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
