//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, The OpenBVE Project
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


using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;

namespace LibRender2.Primitives
{
	/// <summary>A Cube of nomimal 1.0 size</summary>
	public class Cube
	{
		private readonly BaseRenderer renderer;
		private readonly VertexArrayObject defaultVAO;

		/// <summary>Creates a new cube</summary>
		public Cube(BaseRenderer renderer) : this(renderer, Color128.White)
		{
		}

		/// <summary>Creates a new colored cube</summary>
		public Cube(BaseRenderer renderer, Color128 color)
		{
			this.renderer = renderer;

			LibRenderVertex[] vertexData =
			{
				// back
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = color
				},

				// right
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = color
				},

				// top
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.One,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = color
				},

				// front
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.One,
					Color = color
				},

				// left
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.One,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = color
				},

				// bottom
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = color
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = color
				}
			};

			if (!renderer.ForceLegacyOpenGL)
			{
				try
				{
					defaultVAO = new VertexArrayObject();
					defaultVAO.Bind();
					defaultVAO.SetVBO(new VertexBufferObject(vertexData, BufferUsageHint.StaticDraw));
					defaultVAO.SetIBO(new IndexBufferObjectUS(Enumerable.Range(0, vertexData.Length).Select(x => (ushort)x).ToArray(), BufferUsageHint.StaticDraw));
					defaultVAO.SetAttributes(renderer.DefaultShader.VertexLayout);
					defaultVAO.UnBind();
				}
				catch
				{
					renderer.ForceLegacyOpenGL = true;
				}
				
			}
		}

		/// <summary>Draws a 3D cube</summary>
		/// <param name="Position">The position in world-space</param>
		/// <param name="Direction">The direction vector</param>
		/// <param name="Up">The up vector</param>
		/// <param name="Side">The side vector</param>
		/// <param name="Size">The size of the cube in M</param>
		/// <param name="Camera">The camera position</param>
		/// <param name="TextureIndex">The texture to apply</param>
		public void Draw(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, double Size, Vector3 Camera, Texture TextureIndex)
		{
			if (renderer.AvailableNewRenderer)
			{
				Draw(Position, Direction, Up, Side, new Vector3(Size, Size, Size), Camera, TextureIndex);
			}
			else
			{
				DrawImmediate(Position, Direction, Up, Side, new Vector3(Size, Size, Size), Camera, TextureIndex);
			}
		}

		/// <summary>Draws a 3D cube</summary>
		/// <param name="Position">The position in world-space</param>
		/// <param name="Direction">The direction vector</param>
		/// <param name="Up">The up vector</param>
		/// <param name="Side">The side vector</param>
		/// <param name="Size">A 3D vector describing the size of the cube</param>
		/// <param name="Camera">The camera position</param>
		/// <param name="TextureIndex">The texture to apply</param>
		public void Draw(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, Vector3 Size, Vector3 Camera, Texture TextureIndex)
		{
			if (renderer.AvailableNewRenderer)
			{
				DrawRetained(defaultVAO, Position, Direction, Up, Side, Size, Camera, TextureIndex);
			}
			else
			{
				DrawImmediate(Position, Direction, Up, Side, Size, Camera, TextureIndex);
			}
		}

		/// <summary>Draws a 3D cube</summary>
		/// <param name="VAO"></param>
		/// <param name="Position">The position in world-space</param>
		/// <param name="Direction">The direction vector</param>
		/// <param name="Up">The up vector</param>
		/// <param name="Side">The side vector</param>
		/// <param name="Size">A 3D vector describing the size of the cube</param>
		/// <param name="Camera">The camera position</param>
		/// <param name="TextureIndex">The texture to apply</param>
		private void DrawRetained(VertexArrayObject VAO, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, Vector3 Size, Vector3 Camera, Texture TextureIndex)
		{
			renderer.DefaultShader.Activate();
			renderer.ResetShader(renderer.DefaultShader);
			// matrix
			renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			renderer.DefaultShader.SetCurrentModelViewMatrix(Matrix4D.Scale(Size) * (Matrix4D)new Transformation(Direction, Up, Side) * Matrix4D.CreateTranslation(Position.X - Camera.X, Position.Y - Camera.Y, -Position.Z + Camera.Z) * renderer.CurrentViewMatrix);

			// texture
			if (TextureIndex != null && renderer.currentHost.LoadTexture(ref TextureIndex, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, TextureIndex.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				GL.Disable(EnableCap.Texture2D);
			}

			// render polygon
			VAO.Bind();
			VAO.Draw(PrimitiveType.Quads);
			GL.Disable(EnableCap.Texture2D);
		}

		/// <summary>Draws a 3D cube in immediate mode</summary>
		/// <param name="Position">The position in world-space</param>
		/// <param name="Direction">The direction vector</param>
		/// <param name="Up">The up vector</param>
		/// <param name="Side">The side vector</param>
		/// <param name="Size">A 3D vector describing the size of the cube</param>
		/// <param name="Camera">The camera position</param>
		/// <param name="TextureIndex">The texture to apply</param>
		private void DrawImmediate(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, Vector3 Size, Vector3 Camera, Texture TextureIndex)
		{
			renderer.LastBoundTexture = null;
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			OpenTK.Matrix4d perspective = OpenTK.Matrix4d.Perspective(renderer.Camera.VerticalViewingAngle, -renderer.Screen.AspectRatio, 0.2, 1000.0);
			GL.MultMatrix(ref perspective);
			double dx = renderer.Camera.AbsoluteDirection.X;
			double dy = renderer.Camera.AbsoluteDirection.Y;
			double dz = renderer.Camera.AbsoluteDirection.Z;
			double ux = renderer.Camera.AbsoluteUp.X;
			double uy = renderer.Camera.AbsoluteUp.Y;
			double uz = renderer.Camera.AbsoluteUp.Z;
			OpenTK.Matrix4d lookat = OpenTK.Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadMatrix(ref lookat);
			Vector3[] v = new Vector3[8];
			v[0] = new Vector3(Size.X, Size.Y, -Size.Z);
			v[1] = new Vector3(Size.X, -Size.Y, -Size.Z);
			v[2] = new Vector3(-Size.X, -Size.Y, -Size.Z);
			v[3] = new Vector3(-Size.X, Size.Y, -Size.Z);
			v[4] = new Vector3(Size.X, Size.Y, Size.Z);
			v[5] = new Vector3(Size.X, -Size.Y, Size.Z);
			v[6] = new Vector3(-Size.X, -Size.Y, Size.Z);
			v[7] = new Vector3(-Size.X, Size.Y, Size.Z);
			for (int i = 0; i < 8; i++)
			{
				v[i].Rotate(Direction, Up, Side);
				v[i] += Position - Camera;
			}
			int[][] Faces = new int[6][];
			Faces[0] = new[] { 0, 1, 2, 3 };
			Faces[1] = new[] { 0, 4, 5, 1 };
			Faces[2] = new[] { 0, 3, 7, 4 };
			Faces[3] = new[] { 6, 5, 4, 7 };
			Faces[4] = new[] { 6, 7, 3, 2 };
			Faces[5] = new[] { 6, 2, 1, 5 };
			if (TextureIndex == null || !renderer.currentHost.LoadTexture(ref TextureIndex, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Disable(EnableCap.Texture2D);
				for (int i = 0; i < 6; i++)
				{
					GL.Begin(PrimitiveType.Quads);
					for (int j = 0; j < 4; j++)
					{
						GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
					}
					GL.End();
				}
				GL.PopMatrix();

				GL.MatrixMode(MatrixMode.Projection);
				GL.PopMatrix();
				return;
			}
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, TextureIndex.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			Vector2[][] t = new Vector2[6][];
			t[0] = new[] { new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0) };
			t[1] = new[] { new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0) };
			t[2] = new[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
			t[3] = new[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
			t[4] = new[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
			t[5] = new[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
			for (int i = 0; i < 6; i++)
			{
				GL.Begin(PrimitiveType.Quads);
				GL.Color3(1.0, 1.0, 1.0);
				for (int j = 0; j < 4; j++)
				{
					GL.TexCoord2(t[i][j].X, t[i][j].Y);
					GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
				}
				GL.End();
			}
			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}
	}
}
