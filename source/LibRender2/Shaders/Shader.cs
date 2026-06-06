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

using System.Collections.Generic;
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
	public class Shader : AbstractShader
	{
		public readonly VertexLayout VertexLayout;
		public readonly UniformLayout UniformLayout;
		private readonly int uShadowEnabledLocation;
		private readonly int uShadowStrengthLocation;
		private readonly int uShadowCascadeCountLocation;
		private readonly int uShadowMap0Location;
		private readonly int uShadowMap1Location;
		private readonly int uShadowMap2Location;
		private readonly int uShadowMap3Location;
		private readonly int uShadowSplit0Location;
		private readonly int uShadowSplit1Location;
		private readonly int uShadowSplit2Location;
		private readonly int uShadowSplit3Location;
		private readonly int uShadowBias0Location;
		private readonly int uShadowBias1Location;
		private readonly int uShadowBias2Location;
		private readonly int uShadowBias3Location;
		private readonly int uShadowNormalBias0Location;
		private readonly int uShadowNormalBias1Location;
		private readonly int uShadowNormalBias2Location;
		private readonly int uShadowNormalBias3Location;
		private readonly int uLightSpaceMatrix0Location;
		private readonly int uLightSpaceMatrix1Location;
		private readonly int uLightSpaceMatrix2Location;
		private readonly int uLightSpaceMatrix3Location;
		private readonly int uModelMatrixLocation;
		private readonly int uCurrentViewMatrixLocation;
		private readonly int uDynamicLightCountLocation;
		private readonly int[] uDynamicLightTypeLocation = new int[16];
		private readonly int[] uDynamicLightPositionLocation = new int[16];
		private readonly int[] uDynamicLightDirectionLocation = new int[16];
		private readonly int[] uDynamicLightColorLocation = new int[16];
		private readonly int[] uDynamicLightRangeLocation = new int[16];
		private readonly int[] uDynamicLightRangeSquaredLocation = new int[16];
		private readonly int[] uDynamicLightSpotCutoffLocation = new int[16];
		private readonly int[] uDynamicLightPowerLocation = new int[16];
		private readonly int[] uDynamicLightExposureLocation = new int[16];
		private readonly int[] uDynamicLightNormalizeLocation = new int[16];
		private readonly int[] uDynamicLightRadiusLocation = new int[16];
		private readonly int[] uDynamicLightSoftFalloffLocation = new int[16];
		private readonly int[] uDynamicLightSoftnessLocation = new int[16];


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Renderer">A reference to the base renderer</param>
		/// <param name="vertexShaderName">file path and name to vertex shader source</param>
		/// <param name="fragmentShaderName">file path and name to fragment shader source</param>
		/// <param name="isFromStream"></param>
		public Shader(BaseRenderer Renderer, string vertexShaderName, string fragmentShaderName, bool isFromStream = false) : base(Renderer, vertexShaderName, fragmentShaderName, isFromStream, true)
		{
			uShadowEnabledLocation = GL.GetUniformLocation(Handle, "uShadowEnabled");
			uShadowStrengthLocation = GL.GetUniformLocation(Handle, "uShadowStrength");
			uShadowCascadeCountLocation = GL.GetUniformLocation(Handle, "uShadowCascadeCount");
			uShadowMap0Location = GL.GetUniformLocation(Handle, "uShadowMap0");
			uShadowMap1Location = GL.GetUniformLocation(Handle, "uShadowMap1");
			uShadowMap2Location = GL.GetUniformLocation(Handle, "uShadowMap2");
			uShadowMap3Location = GL.GetUniformLocation(Handle, "uShadowMap3");
			uShadowSplit0Location = GL.GetUniformLocation(Handle, "uShadowSplit0");
			uShadowSplit1Location = GL.GetUniformLocation(Handle, "uShadowSplit1");
			uShadowSplit2Location = GL.GetUniformLocation(Handle, "uShadowSplit2");
			uShadowSplit3Location = GL.GetUniformLocation(Handle, "uShadowSplit3");
			uShadowBias0Location = GL.GetUniformLocation(Handle, "uShadowBias0");
			uShadowBias1Location = GL.GetUniformLocation(Handle, "uShadowBias1");
			uShadowBias2Location = GL.GetUniformLocation(Handle, "uShadowBias2");
			uShadowBias3Location = GL.GetUniformLocation(Handle, "uShadowBias3");
			uShadowNormalBias0Location = GL.GetUniformLocation(Handle, "uShadowNormalBias0");
			uShadowNormalBias1Location = GL.GetUniformLocation(Handle, "uShadowNormalBias1");
			uShadowNormalBias2Location = GL.GetUniformLocation(Handle, "uShadowNormalBias2");
			uShadowNormalBias3Location = GL.GetUniformLocation(Handle, "uShadowNormalBias3");
			uLightSpaceMatrix0Location = GL.GetUniformLocation(Handle, "uLightSpaceMatrix0");
			uLightSpaceMatrix1Location = GL.GetUniformLocation(Handle, "uLightSpaceMatrix1");
			uLightSpaceMatrix2Location = GL.GetUniformLocation(Handle, "uLightSpaceMatrix2");
			uLightSpaceMatrix3Location = GL.GetUniformLocation(Handle, "uLightSpaceMatrix3");
			uModelMatrixLocation = GL.GetUniformLocation(Handle, "uModelMatrix");
			uCurrentViewMatrixLocation = GL.GetUniformLocation(Handle, "uCurrentViewMatrix");
			uDynamicLightCountLocation = GL.GetUniformLocation(Handle, "uDynamicLightCount");
			for (int i = 0; i < 16; i++)
			{
				uDynamicLightTypeLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].type");
				uDynamicLightPositionLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].position");
				uDynamicLightDirectionLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].direction");
				uDynamicLightColorLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].color");
				uDynamicLightRangeLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].range");
				uDynamicLightRangeSquaredLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].rangeSquared");
				uDynamicLightSpotCutoffLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].spotCutoff");
				uDynamicLightPowerLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].power");
				uDynamicLightExposureLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].exposure");
				uDynamicLightNormalizeLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].isNormalize");
				uDynamicLightRadiusLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].radius");
				uDynamicLightSoftFalloffLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].softFalloff");
				uDynamicLightSoftnessLocation[i] = GL.GetUniformLocation(Handle, $"uDynamicLights[{i}].softness");
			}

			VertexLayout = GetVertexLayout();
			UniformLayout = GetUniformLayout();

			// Initialise shadow map units to something non-zero to avoid sampler collision with uTexture
			// Note: GL spec forbids different sampler types (sampler2D and sampler2DShadow) targeting the same unit
			GL.ProgramUniform1(Handle, uShadowMap0Location, 4);
			GL.ProgramUniform1(Handle, uShadowMap1Location, 5);
			GL.ProgramUniform1(Handle, uShadowMap2Location, 6);
			GL.ProgramUniform1(Handle, uShadowMap3Location, 7);
			// Also ensure shadow is disabled by default
			GL.ProgramUniform1(Handle, uShadowEnabledLocation, 0);
			GL.ProgramUniform1(Handle, uShadowCascadeCountLocation, 0);
			GL.ProgramUniform1(Handle, uShadowStrengthLocation, 1.0f);
		}
		
		public VertexLayout GetVertexLayout()
		{
			return new VertexLayout
			{
				Position = (short)GL.GetAttribLocation(Handle, "iPosition"),
				Normal = (short)GL.GetAttribLocation(Handle, "iNormal"),
				UV = (short)GL.GetAttribLocation(Handle, "iUv"),
				Color = (short)GL.GetAttribLocation(Handle, "iColor"),
				MatrixChain = (short)GL.GetAttribLocation(Handle, "iMatrixChain"),
			};


		}
		public UniformLayout GetUniformLayout()
		{
			return new UniformLayout
			{
				CurrentAnimationMatricies = (short)GL.GetUniformBlockIndex(Handle, "uAnimationMatricies"),
				CurrentProjectionMatrix = (short)GL.GetUniformLocation(Handle, "uCurrentProjectionMatrix"),
				CurrentModelViewMatrix = (short)GL.GetUniformLocation(Handle, "uCurrentModelViewMatrix"),
				CurrentTextureMatrix = (short)GL.GetUniformLocation(Handle, "uCurrentTextureMatrix"),
				IsLight = (short)GL.GetUniformLocation(Handle, "uIsLight"),
				LightPosition = (short)GL.GetUniformLocation(Handle, "uLight.position"),
				LightAmbient = (short)GL.GetUniformLocation(Handle, "uLight.ambient"),
				LightDiffuse = (short)GL.GetUniformLocation(Handle, "uLight.diffuse"),
				LightSpecular = (short)GL.GetUniformLocation(Handle, "uLight.specular"),
				LightModel = (short)GL.GetUniformLocation(Handle, "uLight.lightModel"),
				MaterialAmbient = (short)GL.GetUniformLocation(Handle, "uMaterial.ambient"),
				MaterialDiffuse = (short)GL.GetUniformLocation(Handle, "uMaterial.diffuse"),
				MaterialSpecular = (short)GL.GetUniformLocation(Handle, "uMaterial.specular"),
				MaterialEmission = (short)GL.GetUniformLocation(Handle, "uMaterial.emission"),
				MaterialShininess = (short)GL.GetUniformLocation(Handle, "uMaterial.shininess"),
				MaterialFlags = (short)GL.GetUniformLocation(Handle, "uMaterialFlags"),
				MaterialIsAdditive = (short)GL.GetUniformLocation(Handle, "uIsAdditive"),
				IsFog = (short)GL.GetUniformLocation(Handle, "uIsFog"),
				FogStart = (short)GL.GetUniformLocation(Handle, "uFogStart"),
				FogEnd = (short)GL.GetUniformLocation(Handle, "uFogEnd"),
				FogColor = (short)GL.GetUniformLocation(Handle, "uFogColor"),
				FogIsLinear = (short)GL.GetUniformLocation(Handle, "uFogIsLinear"),
				FogDensity = (short)GL.GetUniformLocation(Handle, "uFogDensity"),
				Texture = (short)GL.GetUniformLocation(Handle, "uTexture"),
				Brightness = (short)GL.GetUniformLocation(Handle, "uBrightness"),
				Opacity = (short)GL.GetUniformLocation(Handle, "uOpacity"),
				ObjectIndex = (short)GL.GetUniformLocation(Handle, "uObjectIndex"),
				Point = (short)GL.GetUniformLocation(Handle, "uPoint"),
				Size = (short)GL.GetUniformLocation(Handle, "uSize"),
				Color = (short)GL.GetUniformLocation(Handle, "uColor"),
				Coordinates = (short)GL.GetUniformLocation(Handle, "uCoordinates"),
				AtlasLocation = (short)GL.GetUniformLocation(Handle, "uAtlasLocation"),
				AlphaFunction = (short)GL.GetUniformLocation(Handle, "uAlphaTest"),
				LightSpaceMatrix0 = (short)GL.GetUniformLocation(Handle, "uLightSpaceMatrix0"),
				LightSpaceMatrix1 = (short)GL.GetUniformLocation(Handle, "uLightSpaceMatrix1"),
				LightSpaceMatrix2 = (short)GL.GetUniformLocation(Handle, "uLightSpaceMatrix2"),
				ShadowMap0 = (short)GL.GetUniformLocation(Handle, "uShadowMap0"),
				ShadowMap1 = (short)GL.GetUniformLocation(Handle, "uShadowMap1"),
				ShadowMap2 = (short)GL.GetUniformLocation(Handle, "uShadowMap2"),
				CurrentViewMatrix = (short)GL.GetUniformLocation(Handle, "uCurrentViewMatrix"),
			};
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
			Renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
			Matrix4 matrix = ConvertToMatrix4(ProjectionMatrix);
			GL.ProgramUniformMatrix4(Handle, UniformLayout.CurrentProjectionMatrix, false, ref matrix);
		}

		/// <summary>
		/// Set the animation matricies
		/// </summary>
		public void SetCurrentAnimationMatricies(ObjectState objectState)
		{
			Renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
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
			Renderer.lastObjectState = null; // clear the cached object state, as otherwise it might be stale
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
			GL.ProgramUniformMatrix4(Handle, UniformLayout.CurrentModelViewMatrix, false, ref matrix);
		}
		
		/// <summary>
		/// Set the texture matrix
		/// </summary>
		/// <param name="TextureMatrix"></param>
		public void SetCurrentTextureMatrix(Matrix4D TextureMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(TextureMatrix);
			GL.ProgramUniformMatrix4(Handle, UniformLayout.CurrentTextureMatrix, false, ref matrix);
		}

		public void SetIsLight(bool IsLight)
		{
			GL.ProgramUniform1(Handle, UniformLayout.IsLight, IsLight ? 1 : 0);
		}

		public void SetLightPosition(Vector3 LightPosition)
		{
			GL.ProgramUniform3(Handle, UniformLayout.LightPosition, (float)LightPosition.X, (float)LightPosition.Y, (float)LightPosition.Z);
		}

		public void SetLightAmbient(Color24 LightAmbient)
		{
			GL.ProgramUniform3(Handle, UniformLayout.LightAmbient, LightAmbient.R / 255.0f, LightAmbient.G / 255.0f, LightAmbient.B / 255.0f);
		}

		public void SetLightDiffuse(Color24 LightDiffuse)
		{
			GL.ProgramUniform3(Handle, UniformLayout.LightDiffuse, LightDiffuse.R / 255.0f, LightDiffuse.G / 255.0f, LightDiffuse.B / 255.0f);
		}

		public void SetLightSpecular(Color24 LightSpecular)
		{
			GL.ProgramUniform3(Handle, UniformLayout.LightSpecular, LightSpecular.R / 255.0f, LightSpecular.G / 255.0f, LightSpecular.B / 255.0f);
		}

		public void SetLightModel(Vector4 LightModel)
		{
			GL.ProgramUniform4(Handle, UniformLayout.LightModel, (float)LightModel.X, (float)LightModel.Y, (float)LightModel.Z, (float)LightModel.W);
		}

		public void SetMaterialAmbient(Color32 MaterialAmbient)
		{
			GL.ProgramUniform4(Handle, UniformLayout.MaterialAmbient, MaterialAmbient.R / 255.0f, MaterialAmbient.G / 255.0f, MaterialAmbient.B / 255.0f, MaterialAmbient.A / 255.0f);
		}

		public void SetMaterialDiffuse(Color32 MaterialDiffuse)
		{
			GL.ProgramUniform4(Handle, UniformLayout.MaterialDiffuse, MaterialDiffuse.R / 255.0f, MaterialDiffuse.G / 255.0f, MaterialDiffuse.B / 255.0f, MaterialDiffuse.A / 255.0f);
		}

		public void SetMaterialSpecular(Color32 MaterialSpecular)
		{
			GL.ProgramUniform4(Handle, UniformLayout.MaterialSpecular, MaterialSpecular.R / 255.0f, MaterialSpecular.G / 255.0f, MaterialSpecular.B / 255.0f, MaterialSpecular.A / 255.0f);
		}

		// Accepts Color32 for API consistency, but only sends RGB (vec3) to the GLSL shader
		public void SetMaterialEmission(Color32 MaterialEmission)
		{
			GL.ProgramUniform3(Handle, UniformLayout.MaterialEmission, MaterialEmission.R / 255.0f, MaterialEmission.G / 255.0f, MaterialEmission.B / 255.0f);
		}

		public void SetMaterialShininess(float materialShininess)
		{
			GL.ProgramUniform1(Handle, UniformLayout.MaterialShininess, materialShininess);
		}

		public void SetMaterialFlags(MaterialFlags Flags)
		{
			GL.ProgramUniform1(Handle, UniformLayout.MaterialFlags, (int)Flags);
		}

		public override void SetFog(bool enabled)
		{
			GL.ProgramUniform1(Handle, UniformLayout.IsFog, enabled ? 1 : 0);
		}

		public override void SetFog(Fog Fog)
		{
			GL.ProgramUniform1(Handle, UniformLayout.FogStart, Fog.Start);
			GL.ProgramUniform1(Handle, UniformLayout.FogEnd, Fog.End);
			GL.ProgramUniform3(Handle, UniformLayout.FogColor, Fog.Color.R / 255.0f, Fog.Color.G / 255.0f, Fog.Color.B / 255.0f);
			GL.ProgramUniform1(Handle, UniformLayout.FogIsLinear, Fog.IsLinear ? 1 : 0);
			GL.ProgramUniform1(Handle, UniformLayout.FogDensity, Fog.Density);
		}
		
		public void DisableTexturing()
		{
			if (Renderer.LastBoundTexture != Renderer.whitePixel.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp]) 
			{
				/*
				 * If we do not want to use a texture, set a single white pixel instead
				 * This eliminates some shader branching, and is marginally faster in some cases
				 */
				Renderer.currentHost.LoadTexture(ref Renderer.whitePixel, OpenGlTextureWrapMode.ClampClamp);
				GL.BindTexture(TextureTarget.Texture2D, Renderer.whitePixel.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
				Renderer.LastBoundTexture = Renderer.whitePixel.OpenGlTextures[(int) OpenGlTextureWrapMode.ClampClamp];
			}
		}

		public void SetTexture(int textureUnit)
		{
			GL.ProgramUniform1(Handle, UniformLayout.Texture, textureUnit);
		}

		private float lastBrightness;

		public void SetBrightness(float brightness)
		{
			if(brightness == lastBrightness)
			{
				return;
			}
			lastBrightness = brightness;
			GL.ProgramUniform1(Handle, UniformLayout.Brightness, brightness);
		}

		public void SetOpacity(float opacity)
		{
			GL.ProgramUniform1(Handle, UniformLayout.Opacity, opacity);
		}

		public void SetObjectIndex(int objectIndex)
		{
			GL.ProgramUniform1(Handle, UniformLayout.ObjectIndex, objectIndex);
		}

		public void SetPoint(Vector2 point)
		{
			GL.ProgramUniform2(Handle, UniformLayout.Point, (float)point.X, (float)point.Y);
		}

		public void SetSize(Vector2 size)
		{
			GL.ProgramUniform2(Handle, UniformLayout.Size, (float)size.X, (float) size.Y);
		}

		public void SetColor(Color128 color)
		{
			GL.ProgramUniform4(Handle, UniformLayout.Color, color.R, color.G, color.B, color.A);
		}

		public void SetCoordinates(Vector2 coordinates)
		{
			GL.ProgramUniform2(Handle, UniformLayout.Coordinates, (float)coordinates.X, (float)coordinates.Y);
		}

		public void SetAtlasLocation(Vector4 atlasLocation)
		{
			GL.ProgramUniform4(Handle, UniformLayout.AtlasLocation, (float)atlasLocation.X, (float)atlasLocation.Y, (float)atlasLocation.Z, (float)atlasLocation.W);
		}

		public override void SetAlphaFunction(AlphaFunction alphaFunction, float alphaComparison)
		{
			GL.ProgramUniform2(Handle, UniformLayout.AlphaFunction, (int)alphaFunction, alphaComparison);
			
		}

		public override void SetAlphaTest(bool enabled)
		{
			if (!enabled)
			{
				GL.ProgramUniform2(Handle, UniformLayout.AlphaFunction, (int)AlphaFunction.Never, 1.0f);
			}
		}

		public void SetShadowEnabled(bool enabled)
		{
			GL.ProgramUniform1(Handle, uShadowEnabledLocation, enabled ? 1 : 0);
		}

		public void SetCascadeLightSpaceMatrix(int cascade, OpenBveApi.Math.Matrix4D matrix)
		{
			int loc;
			switch (cascade)
			{
				case 0: loc = uLightSpaceMatrix0Location; break;
				case 1: loc = uLightSpaceMatrix1Location; break;
				case 2: loc = uLightSpaceMatrix2Location; break;
				case 3: loc = uLightSpaceMatrix3Location; break;
				default: return;
			}
			Matrix4 OpenTKMatrix = ConvertToMatrix4(matrix);
			GL.ProgramUniformMatrix4(Handle, loc, false, ref OpenTKMatrix);
		}

		public void SetCascadeShadowMapUnit(int cascade, int textureUnit)
		{
			int loc;
			switch (cascade)
			{
				case 0: loc = uShadowMap0Location; break;
				case 1: loc = uShadowMap1Location; break;
				case 2: loc = uShadowMap2Location; break;
				case 3: loc = uShadowMap3Location; break;
				default: return;
			}
			GL.ProgramUniform1(Handle, loc, textureUnit);
		}

		public void SetShadowSplitDistance(int cascade, float distance)
		{
			int loc;
			switch (cascade)
			{
				case 0: loc = uShadowSplit0Location; break;
				case 1: loc = uShadowSplit1Location; break;
				case 2: loc = uShadowSplit2Location; break;
				case 3: loc = uShadowSplit3Location; break;
				default: return;
			}
			GL.ProgramUniform1(Handle, loc, distance);
		}

		public void SetCascadeBias(int cascade, float bias)
		{
			int loc;
			switch (cascade)
			{
				case 0: loc = uShadowBias0Location; break;
				case 1: loc = uShadowBias1Location; break;
				case 2: loc = uShadowBias2Location; break;
				case 3: loc = uShadowBias3Location; break;
				default: return;
			}
			GL.ProgramUniform1(Handle, loc, bias);
		}

		public void SetNormalBias(int cascade, float bias)
		{
			int loc;
			switch (cascade)
			{
				case 0: loc = uShadowNormalBias0Location; break;
				case 1: loc = uShadowNormalBias1Location; break;
				case 2: loc = uShadowNormalBias2Location; break;
				case 3: loc = uShadowNormalBias3Location; break;
				default: return;
			}
			GL.ProgramUniform1(Handle, loc, bias);
		}

		public void SetShadowCascadeCount(int count)
		{
			GL.ProgramUniform1(Handle, uShadowCascadeCountLocation, count);
		}

		public void SetShadowStrength(float strength)
		{
			GL.ProgramUniform1(Handle, uShadowStrengthLocation, strength);
		}

		public void SetCurrentViewMatrix(OpenBveApi.Math.Matrix4D viewMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(viewMatrix);
			GL.ProgramUniformMatrix4(Handle, uCurrentViewMatrixLocation, false, ref matrix);
		}

		public void SetCurrentModelMatrix(OpenBveApi.Math.Matrix4D modelMatrix)
		{
			Matrix4 matrix = ConvertToMatrix4(modelMatrix);
			GL.ProgramUniformMatrix4(Handle, uModelMatrixLocation, false, ref matrix);
		}

		private static float[] Matrix4DToFloatArray(OpenBveApi.Math.Matrix4D m)
		{
			return new float[]
			{
				(float)m.Row0.X, (float)m.Row0.Y, (float)m.Row0.Z, (float)m.Row0.W,
				(float)m.Row1.X, (float)m.Row1.Y, (float)m.Row1.Z, (float)m.Row1.W,
				(float)m.Row2.X, (float)m.Row2.Y, (float)m.Row2.Z, (float)m.Row2.W,
				(float)m.Row3.X, (float)m.Row3.Y, (float)m.Row3.Z, (float)m.Row3.W
			};
		}

		private readonly List<SceneLight> lastBoundLights = new List<SceneLight>();
		private Matrix4D lastViewMatrix = Matrix4D.Identity;

		public void SetDynamicLights(List<SceneLight> lights, Matrix4D viewMatrix, int maxLimit)
		{
			int count = System.Math.Min(lights.Count, maxLimit);
			
			bool identical = lastBoundLights.Count == count && lastViewMatrix == viewMatrix;
			if (identical)
			{
				for (int i = 0; i < count; i++)
				{
					if (lastBoundLights[i] != lights[i])
					{
						identical = false;
						break;
					}
				}
			}

			if (identical)
			{
				return;
			}

			lastBoundLights.Clear();
			for (int i = 0; i < count; i++)
			{
				lastBoundLights.Add(lights[i]);
			}
			lastViewMatrix = viewMatrix;

			GL.ProgramUniform1(Handle, uDynamicLightCountLocation, count);
			for (int i = 0; i < count; i++)
			{
				SceneLight light = lights[i];
				
				Matrix4D lightViewMatrix = Renderer.Camera != null ? Renderer.Camera.TranslationMatrix * viewMatrix : viewMatrix;
				Vector3 viewPos = light.Position;
				viewPos.Transform(lightViewMatrix, false);

				Vector3 viewDir = light.Direction;
				viewDir.Transform(viewMatrix, true);
				viewDir.Normalize();

				GL.ProgramUniform1(Handle, uDynamicLightTypeLocation[i], (int)light.Type);
				GL.ProgramUniform3(Handle, uDynamicLightPositionLocation[i], (float)viewPos.X, (float)viewPos.Y, (float)viewPos.Z);
				GL.ProgramUniform3(Handle, uDynamicLightDirectionLocation[i], (float)viewDir.X, (float)viewDir.Y, (float)viewDir.Z);
				GL.ProgramUniform4(Handle, uDynamicLightColorLocation[i], light.Color.R, light.Color.G, light.Color.B, light.Color.A);
				GL.ProgramUniform1(Handle, uDynamicLightRangeLocation[i], light.Range);
				GL.ProgramUniform1(Handle, uDynamicLightRangeSquaredLocation[i], light.RangeSquared);
				GL.ProgramUniform1(Handle, uDynamicLightSpotCutoffLocation[i], light.SpotCutoff);
				GL.ProgramUniform1(Handle, uDynamicLightPowerLocation[i], light.Power);
				GL.ProgramUniform1(Handle, uDynamicLightExposureLocation[i], light.Exposure);
				GL.ProgramUniform1(Handle, uDynamicLightNormalizeLocation[i], light.NormalizeCone ? 1 : 0);
				GL.ProgramUniform1(Handle, uDynamicLightRadiusLocation[i], light.Radius);
				GL.ProgramUniform1(Handle, uDynamicLightSoftFalloffLocation[i], light.SoftFalloff ? 1 : 0);
				GL.ProgramUniform1(Handle, uDynamicLightSoftnessLocation[i], light.Softness);
			}
		}

		#endregion
	}
}
