//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Aditiya Afrizal, The OpenBVE Project
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

using OpenBveApi.Objects;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Shaders
{
	/// <summary>Shader program used for the shadow map depth pass.</summary>
	public class ShadowDepthShader : AbstractShader
	{
		private readonly int uLightSpaceMatrix;
		private readonly int uModelMatrix;
		private readonly int uTexture;
		private readonly int uHasTexture;
		private readonly int uAlphaCutoff;
		private readonly int uMaterialAlpha; // Uniform location for material color alpha
		private readonly int uMaterialFlags;
		
		// Cache
		private bool? lastHasTexture;
		private float? lastAlphaCutoff;
		private float? lastMaterialAlpha;
		private int? lastMaterialFlags;
		private OpenTK.Matrix4[] matrixCache;

		public ShadowDepthShader(BaseRenderer Renderer,  string vertexShaderName, string fragmentShaderName, bool isFromStream = false) : base(Renderer, vertexShaderName, fragmentShaderName, isFromStream, false)
		{
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
			uMaterialFlags = GL.GetUniformLocation(Handle, "uMaterialFlags");
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
			if (lastHasTexture != hasTexture)
			{
				GL.Uniform1(uHasTexture, hasTexture ? 1 : 0);
				lastHasTexture = hasTexture;
			}
		}

		public void SetAlphaCutoff(float cutoff)
		{
			if (lastAlphaCutoff != cutoff)
			{
				GL.Uniform1(uAlphaCutoff, cutoff);
				lastAlphaCutoff = cutoff;
			}
		}

		/// <summary>Sets the material color alpha (0.0–1.0) for semi-transparent shadow discard</summary>
		public void SetMaterialAlpha(float alpha)
		{
			if (lastMaterialAlpha != alpha)
			{
				GL.Uniform1(uMaterialAlpha, alpha);
				lastMaterialAlpha = alpha;
			}
		}

		/// <summary>Sets the material flags for shadow discard</summary>
		public void SetMaterialFlags(MaterialFlags flags)
		{
			if (lastMaterialFlags != (int)flags)
			{
				GL.Uniform1(uMaterialFlags, (int)flags);
				lastMaterialFlags = (int)flags;
			}
		}

		public void SetModelMatrix(OpenBveApi.Math.Matrix4D m)
		{
			OpenTK.Matrix4 matrix = ConvertToMatrix4(m);
			GL.UniformMatrix4(uModelMatrix, false, ref matrix);
		}

		public void SetCurrentAnimationMatricies(OpenBveApi.Objects.ObjectState objectState)
		{
			if (objectState.Matricies == null)
			{
				return;
			}
			int count = objectState.Matricies.Length;
			if (matrixCache == null || matrixCache.Length < count)
			{
				matrixCache = new OpenTK.Matrix4[count];
			}

			for (int i = 0; i < count; i++)
			{
				matrixCache[i] = ConvertToMatrix4(objectState.Matricies[i]);
			}

			unsafe
			{
				if (objectState.MatrixBufferIndex == 0)
				{
					objectState.MatrixBufferIndex = GL.GenBuffer();
				}

				GL.BindBuffer(BufferTarget.UniformBuffer, objectState.MatrixBufferIndex);
				GL.BufferData(BufferTarget.UniformBuffer, sizeof(OpenTK.Matrix4) * count, matrixCache, BufferUsageHint.StaticDraw);
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
	}
}
