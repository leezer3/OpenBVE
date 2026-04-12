 using System;
using System.IO;
using System.Reflection;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Shaders
{
	/// <summary>
	/// Shader program used for the shadow map depth pass.
	/// </summary>
	public class ShadowDepthShader : IDisposable
	{
		public int Handle
		{
			get; private set;
		}

		private int uLightSpaceMatrix;
		private int uModelMatrix;
		private int uTexture;
		private int uHasTexture;
		private int uAlphaCutoff;
		private int uMaterialAlpha; // Uniform location for material color alpha

		public ShadowDepthShader()
		{
			// Load shader source from embedded resources or files
			string vertSource = LoadEmbeddedShader("shadow_depth.vert");
			string fragSource = LoadEmbeddedShader("shadow_depth.frag");

			int vertShader = CompileShader(ShaderType.VertexShader, vertSource);
			int fragShader = CompileShader(ShaderType.FragmentShader, fragSource);

			Handle = GL.CreateProgram();
			GL.AttachShader(Handle, vertShader);
			GL.AttachShader(Handle, fragShader);
			GL.LinkProgram(Handle);

			GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
			if (success == 0)
			{
				string infoLog = GL.GetProgramInfoLog(Handle);
				throw new Exception($"[ShadowDepthShader] Link error: {infoLog}");
			}

			GL.DeleteShader(vertShader);
			GL.DeleteShader(fragShader);

			// Explicitly bind the uniform block for matrices to binding point 0
			// This matches BindBufferBase(..., 0, ...) in the rendering pass.
			int matrixBlockIndex = GL.GetUniformBlockIndex(Handle, "matrices");
			if (matrixBlockIndex != -1)
			{
				GL.UniformBlockBinding(Handle, matrixBlockIndex, 0);
			}

			// Cache uniform locations
			uLightSpaceMatrix = GL.GetUniformLocation(Handle, "uLightSpaceMatrix");
			uModelMatrix = GL.GetUniformLocation(Handle, "uModelMatrix");
			uTexture = GL.GetUniformLocation(Handle, "uTexture");
			uHasTexture = GL.GetUniformLocation(Handle, "uHasTexture");
			uAlphaCutoff = GL.GetUniformLocation(Handle, "uAlphaCutoff");
			uMaterialAlpha = GL.GetUniformLocation(Handle, "uMaterialAlpha"); // Cache the material alpha location
		}

		private static string LoadEmbeddedShader(string filename)
		{
			// Try to load from the same directory as the existing shaders
			// OpenBVE typically loads shaders as embedded resources in LibRender2
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = $"LibRender2.{filename}";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream != null)
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						return reader.ReadToEnd();
					}
				}
			}

			// Fallback: try file-based loading next to the assembly
			string dir = Path.GetDirectoryName(assembly.Location) ?? ".";
			string path = Path.Combine(dir, "Data\\Shaders", filename);
			if (File.Exists(path))
			{
				return File.ReadAllText(path);
			}

			throw new FileNotFoundException(
				$"Could not find shadow shader: {filename}");
		}

		private static int CompileShader(ShaderType type, string source)
		{
			int shader = GL.CreateShader(type);
			GL.ShaderSource(shader, source);
			GL.CompileShader(shader);

			GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
			if (success == 0)
			{
				string infoLog = GL.GetShaderInfoLog(shader);
				throw new Exception($"[ShadowDepthShader] Compile error ({type}): {infoLog}");
			}
			return shader;
		}

		public void Use()
		{
			GL.UseProgram(Handle);
		}

		public void SetLightSpaceMatrix(OpenBveApi.Math.Matrix4D m)
		{
			OpenTK.Matrix4 matrix = ConvertToMatrix4(m);
			GL.UniformMatrix4(uLightSpaceMatrix, false, ref matrix);
		}

		public void SetTexture(int unit)
		{
			GL.Uniform1(uTexture, unit);
		}

		public void SetHasTexture(bool hasTexture)
		{
			GL.Uniform1(uHasTexture, hasTexture ? 1 : 0);
		}

		public void SetAlphaCutoff(float cutoff)
		{
			GL.Uniform1(uAlphaCutoff, cutoff);
		}

		/// <summary>Sets the material color alpha (0.0–1.0) for semi-transparent shadow discard</summary>
		public void SetMaterialAlpha(float alpha)
		{
			GL.Uniform1(uMaterialAlpha, alpha);
		}

		public void SetModelMatrix(OpenBveApi.Math.Matrix4D m)
		{
			OpenTK.Matrix4 matrix = ConvertToMatrix4(m);
			GL.UniformMatrix4(uModelMatrix, false, ref matrix);
		}

		public void SetCurrentAnimationMatricies(OpenBveApi.Objects.ObjectState objectState)
		{
			OpenTK.Matrix4[] matriciesToShader = new OpenTK.Matrix4[objectState.Matricies.Length];

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
				GL.BufferData(BufferTarget.UniformBuffer, sizeof(OpenTK.Matrix4) * matriciesToShader.Length, matriciesToShader, BufferUsageHint.StaticDraw);
			}
		}

		private static OpenTK.Matrix4 ConvertToMatrix4(OpenBveApi.Math.Matrix4D mat)
		{
			return new OpenTK.Matrix4(
				(float)mat.Row0.X, (float)mat.Row0.Y, (float)mat.Row0.Z, (float)mat.Row0.W,
				(float)mat.Row1.X, (float)mat.Row1.Y, (float)mat.Row1.Z, (float)mat.Row1.W,
				(float)mat.Row2.X, (float)mat.Row2.Y, (float)mat.Row2.Z, (float)mat.Row2.W,
				(float)mat.Row3.X, (float)mat.Row3.Y, (float)mat.Row3.Z, (float)mat.Row3.W
			);
		}

		private static float[] Matrix4DToFloatArray(OpenBveApi.Math.Matrix4D m)
		{
			// OpenBVE's Matrix4D is row-major; OpenGL expects column-major
			// So we transpose on upload (or set transpose=true)
			return new float[]
			{
				(float)m.Row0.X, (float)m.Row0.Y, (float)m.Row0.Z, (float)m.Row0.W,
				(float)m.Row1.X, (float)m.Row1.Y, (float)m.Row1.Z, (float)m.Row1.W,
				(float)m.Row2.X, (float)m.Row2.Y, (float)m.Row2.Z, (float)m.Row2.W,
				(float)m.Row3.X, (float)m.Row3.Y, (float)m.Row3.Z, (float)m.Row3.W
			};
		}

		public void Dispose()
		{
			if (Handle != 0)
			{
				GL.DeleteProgram(Handle);
				Handle = 0;
			}
		}
	}
}
