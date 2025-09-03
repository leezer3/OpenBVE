using System;
using System.IO;
using System.Reflection;
using System.Text;
using LibRender2.Fogs;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;
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
		/// <param name="vertexShaderName">file path and name to vertex shader source</param>
		/// <param name="fragmentShaderName">file path and name to fragment shader source</param>
		/// <param name="isFromStream"></param>
		public Shader(BaseRenderer Renderer, string vertexShaderName, string fragmentShaderName, bool isFromStream = false)
		{
			renderer = Renderer;
			handle = GL.CreateProgram();

			if (isFromStream)
			{
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{vertexShaderName}.vert"))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							LoadShader(reader.ReadToEnd(), ShaderType.VertexShader);
						}
					}
				}
				using (Stream stream = thisAssembly.GetManifestResourceStream($"LibRender2.{fragmentShaderName}.frag"))
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
				LoadShader(File.ReadAllText(vertexShaderName, Encoding.UTF8), ShaderType.VertexShader);
				LoadShader(File.ReadAllText(fragmentShaderName, Encoding.UTF8), ShaderType.FragmentShader);
			}

			GL.AttachShader(handle, vertexShader);
			GL.AttachShader(handle, fragmentShader);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);
			GL.BindFragDataLocation(handle, 0, "fragColor");
			GL.LinkProgram(handle);
			GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int status);

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

			if (renderer.CurrentShader != null)
			{
				renderer.CurrentShader.isActive = false;
			}
			GL.UseProgram(handle);
			isActive = true;
			renderer.lastVAO = -1;
			renderer.CurrentShader = this;
			renderer.RestoreAlphaFunc();
		}

		public VertexLayout GetVertexLayout()
		{
			return new VertexLayout
			{
				Position = (short)GL.GetAttribLocation(handle, "iPosition"),
				Normal = (short)GL.GetAttribLocation(handle, "iNormal"),
				UV = (short)GL.GetAttribLocation(handle, "iUv"),
				Color = (short)GL.GetAttribLocation(handle, "iColor"),
				MatrixChain = (short)GL.GetAttribLocation(handle, "iMatrixChain"),
			};


		}

		public UniformLayout GetUniformLayout()
		{
			return new UniformLayout
			{
				CurrentAnimationMatricies = (short)GL.GetUniformBlockIndex(handle, "uAnimationMatricies"),
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
				Texture = (short)GL.GetUniformLocation(handle, "uTexture"),
				Brightness = (short)GL.GetUniformLocation(handle, "uBrightness"),
				Opacity = (short)GL.GetUniformLocation(handle, "uOpacity"),
				ObjectIndex = (short)GL.GetUniformLocation(handle, "uObjectIndex"),
				Point = (short)GL.GetUniformLocation(handle, "uPoint"),
				Size = (short)GL.GetUniformLocation(handle, "uSize"),
				Color = (short)GL.GetUniformLocation(handle, "uColor"),
				Coordinates = (short)GL.GetUniformLocation(handle, "uCoordinates"),
				AtlasLocation = (short)GL.GetUniformLocation(handle, "uAtlasLocation"),
				AlphaFunction = (short)GL.GetUniformLocation(handle, "uAlphaTest"),
			};
		}

		/// <summary>Deactivates the shader</summary>
		public void Deactivate()
		{
			if (!isActive)
			{
				return;
			}
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
			renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
			Matrix4 matrix = ConvertToMatrix4(ProjectionMatrix);
			GL.ProgramUniformMatrix4(handle, UniformLayout.CurrentProjectionMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the animation matricies
		/// </summary>
		public void SetCurrentAnimationMatricies(ObjectState objectState)
		{
			renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
			Matrix4[] matriciesToShader = new Matrix4[objectState.Matricies.Length];

			for (int i = 0; i < objectState.Matricies.Length; i++)
			{
				matriciesToShader[i] = ConvertToMatrix4(objectState.Matricies[i]);
			}

			unsafe
			{
				if (objectState.MatrixBufferIndex == 0)
				{
					objectState.MatrixBufferIndex = GL.GenBuffer();
				}

				GL.BindBuffer(BufferTarget.UniformBuffer, objectState.MatrixBufferIndex);
				GL.BufferData(BufferTarget.UniformBuffer, sizeof(Matrix4) * matriciesToShader.Length, matriciesToShader, BufferUsageHint.StaticDraw);
			}

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
			renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
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
			GL.ProgramUniformMatrix4(handle, UniformLayout.CurrentModelViewMatrix, false, ref matrix);
		}
		
		/// <summary>
		/// Set the texture matrix
		/// </summary>
		/// <param name="TextureMatrix"></param>
		public void SetCurrentTextureMatrix(Matrix4D TextureMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(TextureMatrix);
			GL.ProgramUniformMatrix4(handle, UniformLayout.CurrentTextureMatrix, false, ref matrix);
		}

		public void SetIsLight(bool IsLight)
		{
			GL.ProgramUniform1(handle, UniformLayout.IsLight, IsLight ? 1 : 0);
		}

		public void SetLightPosition(Vector3 LightPosition)
		{
			GL.ProgramUniform3(handle, UniformLayout.LightPosition, (float)LightPosition.X, (float)LightPosition.Y, (float)LightPosition.Z);
		}

		public void SetLightAmbient(Color24 LightAmbient)
		{
			GL.ProgramUniform3(handle, UniformLayout.LightAmbient, LightAmbient.R / 255.0f, LightAmbient.G / 255.0f, LightAmbient.B / 255.0f);
		}

		public void SetLightDiffuse(Color24 LightDiffuse)
		{
			GL.ProgramUniform3(handle, UniformLayout.LightDiffuse, LightDiffuse.R / 255.0f, LightDiffuse.G / 255.0f, LightDiffuse.B / 255.0f);
		}

		public void SetLightSpecular(Color24 LightSpecular)
		{
			GL.ProgramUniform3(handle, UniformLayout.LightSpecular, LightSpecular.R / 255.0f, LightSpecular.G / 255.0f, LightSpecular.B / 255.0f);
		}

		public void SetLightModel(Vector4 LightModel)
		{
			GL.ProgramUniform4(handle, UniformLayout.LightModel, (float)LightModel.X, (float)LightModel.Y, (float)LightModel.Z, (float)LightModel.W);
		}

		public void SetMaterialAmbient(Color32 MaterialAmbient)
		{
			GL.ProgramUniform4(handle, UniformLayout.MaterialAmbient, MaterialAmbient.R / 255.0f, MaterialAmbient.G / 255.0f, MaterialAmbient.B / 255.0f, MaterialAmbient.A / 255.0f);
		}

		public void SetMaterialDiffuse(Color32 MaterialDiffuse)
		{
			GL.ProgramUniform4(handle, UniformLayout.MaterialDiffuse, MaterialDiffuse.R / 255.0f, MaterialDiffuse.G / 255.0f, MaterialDiffuse.B / 255.0f, MaterialDiffuse.A / 255.0f);
		}

		public void SetMaterialSpecular(Color32 MaterialSpecular)
		{
			GL.ProgramUniform4(handle, UniformLayout.MaterialSpecular, MaterialSpecular.R / 255.0f, MaterialSpecular.G / 255.0f, MaterialSpecular.B / 255.0f, MaterialSpecular.A / 255.0f);
		}

		public void SetMaterialEmission(Color24 MaterialEmission)
		{
			GL.ProgramUniform3(handle, UniformLayout.MaterialEmission, MaterialEmission.R / 255.0f, MaterialEmission.G / 255.0f, MaterialEmission.B / 255.0f);
		}

		public void SetMaterialShininess(float materialShininess)
		{
			GL.ProgramUniform1(handle, UniformLayout.MaterialShininess, materialShininess);
		}

		public void SetMaterialFlags(MaterialFlags Flags)
		{
			GL.ProgramUniform1(handle, UniformLayout.MaterialFlags, (int)Flags);
		}

		public void SetIsFog(bool IsFog)
		{
			GL.ProgramUniform1(handle, UniformLayout.IsFog, IsFog ? 1 : 0);
		}

		public void SetFog(Fog Fog)
		{
			GL.ProgramUniform1(handle, UniformLayout.FogStart, Fog.Start);
			GL.ProgramUniform1(handle, UniformLayout.FogEnd, Fog.End);
			GL.ProgramUniform3(handle, UniformLayout.FogColor, Fog.Color.R / 255.0f, Fog.Color.G / 255.0f, Fog.Color.B / 255.0f);
			GL.ProgramUniform1(handle, UniformLayout.FogIsLinear, Fog.IsLinear ? 1 : 0);
			GL.ProgramUniform1(handle, UniformLayout.FogDensity, Fog.Density);
		}
		
		public void DisableTexturing()
		{
			if (renderer.LastBoundTexture != renderer.whitePixel.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp]) 
			{
				/*
				 * If we do not want to use a texture, set a single white pixel instead
				 * This eliminates some shader branching, and is marginally faster in some cases
				 */
				renderer.currentHost.LoadTexture(ref renderer.whitePixel, OpenGlTextureWrapMode.ClampClamp);
				GL.BindTexture(TextureTarget.Texture2D, renderer.whitePixel.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
				renderer.LastBoundTexture = renderer.whitePixel.OpenGlTextures[(int) OpenGlTextureWrapMode.ClampClamp];
			}
		}

		public void SetTexture(int textureUnit)
		{
			GL.ProgramUniform1(handle, UniformLayout.Texture, textureUnit);
		}

		private float lastBrightness;

		public void SetBrightness(float brightness)
		{
			if(brightness == lastBrightness)
			{
				return;
			}
			lastBrightness = brightness;
			GL.ProgramUniform1(handle, UniformLayout.Brightness, brightness);
		}

		public void SetOpacity(float opacity)
		{
			GL.ProgramUniform1(handle, UniformLayout.Opacity, opacity);
		}

		public void SetObjectIndex(int objectIndex)
		{
			GL.ProgramUniform1(handle, UniformLayout.ObjectIndex, objectIndex);
		}

		public void SetPoint(Vector2 point)
		{
			GL.ProgramUniform2(handle, UniformLayout.Point, (float)point.X, (float)point.Y);
		}

		public void SetSize(Vector2 size)
		{
			GL.ProgramUniform2(handle, UniformLayout.Size, (float)size.X, (float) size.Y);
		}

		public void SetColor(Color128 color)
		{
			GL.ProgramUniform4(handle, UniformLayout.Color, color.R, color.G, color.B, color.A);
		}

		public void SetCoordinates(Vector2 coordinates)
		{
			GL.ProgramUniform2(handle, UniformLayout.Coordinates, (float)coordinates.X, (float)coordinates.Y);
		}

		public void SetAtlasLocation(Vector4 atlasLocation)
		{
			GL.ProgramUniform4(handle, UniformLayout.AtlasLocation, (float)atlasLocation.X, (float)atlasLocation.Y, (float)atlasLocation.Z, (float)atlasLocation.W);
		}

		public void SetAlphaFunction(AlphaFunction alphaFunction, float alphaComparison)
		{
			GL.ProgramUniform2(handle, UniformLayout.AlphaFunction, (int)alphaFunction, alphaComparison);
			
		}

		public void SetAlphaTest(bool enabled)
		{
			if (!enabled)
			{
				GL.ProgramUniform2(handle, UniformLayout.AlphaFunction, (int)AlphaFunction.Never, 1.0f);
			}
		}

		#endregion
	}
}
