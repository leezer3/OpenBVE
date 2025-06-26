//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, The OpenBVE Project
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


using OpenBveApi.Math;
using OpenBveApi.Colors;
using System.Linq;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.World;

namespace LibRender2.Primitives
{
	public class Particle
	{
		private readonly BaseRenderer renderer;
		private readonly VertexArrayObject[] particlesVAO;

		public Particle(BaseRenderer renderer)
		{
			this.renderer = renderer;
			if (!renderer.ForceLegacyOpenGL)
			{
				try
				{
					particlesVAO = new VertexArrayObject[12];
					for (int i = 0; i < 3; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							LibRenderVertex[] vertexData =
							{
								new LibRenderVertex
								{
									Position = new Vector3f(-1.0f, 1.0f, 0.0f),
									UV = new Vector2f(i * 0.25, j * 0.25),
									Color = Color128.White
								},
								new LibRenderVertex
								{
									Position = new Vector3f(1.0f, 1.0f, 0.0f),
									UV = new Vector2f(0.25 + i * 0.25, j * 0.25),
									Color = Color128.White
								},
								new LibRenderVertex
								{
									Position = new Vector3f(1.0f, -1.0f, 0.0f),
									UV = new Vector2f(0.25 + i * 0.25, 0.25 + j * 0.25),
									Color = Color128.White
								},
								new LibRenderVertex
								{
									Position = new Vector3f(-1.0f, -1.0f, 0.0f),
									UV = new Vector2f(i * 0.25, 0.25 + j * 0.25),
									Color = Color128.White
								}
							};

							particlesVAO[i * 4 + j] = new VertexArrayObject();
							particlesVAO[i * 4 + j].Bind();
							particlesVAO[i * 4 + j].SetVBO(new VertexBufferObject(vertexData, BufferUsageHint.StaticDraw));
							particlesVAO[i * 4 + j].SetIBO(new IndexBufferObjectUS(Enumerable.Range(0, vertexData.Length).Select(x => (ushort)x).ToArray(), BufferUsageHint.StaticDraw));
							particlesVAO[i * 4 + j].SetAttributes(renderer.DefaultShader.VertexLayout);
							particlesVAO[i * 4 + j].UnBind();
						}



					}

				}
				catch
				{
					renderer.ForceLegacyOpenGL = true;
				}

			}
		}

		public void Draw(int particleIndex, Vector3 worldPosition, Vector3 worldDirection, Vector3 worldUp, Vector3 worldSide, Vector2 size, Texture texture, float opacity)
		{
			if (renderer.ForceLegacyOpenGL)
			{
				DrawImmediate(worldPosition, worldDirection, worldUp, worldSide, size, texture, opacity);
			}
			else
			{
				renderer.UnsetBlendFunc();
				renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				DrawRetained(particleIndex, worldPosition, worldDirection, worldUp, worldSide, size, texture, opacity);
				renderer.SetBlendFunc();
				renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				DrawRetained(particleIndex, worldPosition, worldDirection, worldUp, worldSide, size, texture, opacity);
			}
		}

		private void DrawRetained(int particleIndex, Vector3 worldPosition, Vector3 worldDirection, Vector3 worldUp, Vector3 worldSide, Vector2 size, Texture texture, float opacity)
		{
			// spherical billboard
			Matrix4D modelViewMatrix = (Matrix4D)new Transformation(worldDirection, worldUp, worldSide) * Matrix4D.CreateTranslation(worldPosition.X - renderer.Camera.AbsolutePosition.X, worldPosition.Y - renderer.Camera.AbsolutePosition.Y, -worldPosition.Z + renderer.Camera.AbsolutePosition.Z) * renderer.CurrentViewMatrix;
			modelViewMatrix.Row0.Xyz = new Vector3(1, 0, 0);
			modelViewMatrix.Row1.Xyz = new Vector3(0, 1, 0);
			modelViewMatrix.Row2.Xyz = new Vector3(0, 0, 1);
			Matrix4D scale = Matrix4D.Scale(size.X, size.Y, size.X);
			modelViewMatrix = scale * modelViewMatrix;
			renderer.DefaultShader.SetMaterialFlags(MaterialFlags.None);
			renderer.DefaultShader.SetCurrentModelViewMatrix(modelViewMatrix);
			renderer.DefaultShader.SetOpacity(opacity);
			renderer.DefaultShader.SetIsLight(false);
			// texture
			if (texture != null && renderer.currentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				GL.Disable(EnableCap.Texture2D);
			}

			particlesVAO[particleIndex].Bind();
			particlesVAO[particleIndex].Draw(PrimitiveType.Quads);
			GL.Disable(EnableCap.Texture2D);

		}

		private void DrawImmediate(Vector3 worldPosition, Vector3 worldDirection, Vector3 worldUp, Vector3 worldSide, Vector2 size, Texture texture, float opacity)
		{
		}
	}
}
