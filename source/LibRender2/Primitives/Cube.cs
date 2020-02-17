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

		internal Cube(BaseRenderer renderer)
		{
			this.renderer = renderer;

			LibRenderVertex[] vertexData =
			{
				// back
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				},

				// right
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},

				// top
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, 1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},

				// front
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, 1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},

				// left
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, 1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, 1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},

				// bottom
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, -1.0f),
					UV = Vector2f.Null,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, -1.0f),
					UV = Vector2f.Right,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(1.0f, -1.0f, 1.0f),
					UV = Vector2f.One,
					Color = Color128.White
				},
				new LibRenderVertex
				{
					Position = new Vector3f(-1.0f, -1.0f, 1.0f),
					UV = Vector2f.Down,
					Color = Color128.White
				}
			};
			if (renderer.currentOptions.IsUseNewRenderer)
			{
				defaultVAO = new VertexArrayObject();
				defaultVAO.Bind();
				defaultVAO.SetVBO(new VertexBufferObject(vertexData, BufferUsageHint.StaticDraw));
				defaultVAO.SetIBO(new IndexBufferObject(Enumerable.Range(0, vertexData.Length).Select(x => (ushort) x).ToArray(), BufferUsageHint.StaticDraw));
				defaultVAO.UnBind();
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
		public void Draw(OpenBveApi.Math.Vector3 Position, OpenBveApi.Math.Vector3 Direction, OpenBveApi.Math.Vector3 Up, OpenBveApi.Math.Vector3 Side, double Size, OpenBveApi.Math.Vector3 Camera, Texture TextureIndex)
		{
			if (renderer.currentOptions.IsUseNewRenderer)
			{
				Draw(Position, Direction, Up, Side, new OpenBveApi.Math.Vector3(Size, Size, Size), Camera, TextureIndex);
			}
			else
			{
				DrawImmediate(Position, Direction, Up, Side, new OpenBveApi.Math.Vector3(Size, Size, Size), Camera, TextureIndex);
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
		public void Draw(OpenBveApi.Math.Vector3 Position, OpenBveApi.Math.Vector3 Direction, OpenBveApi.Math.Vector3 Up, OpenBveApi.Math.Vector3 Side, OpenBveApi.Math.Vector3 Size, OpenBveApi.Math.Vector3 Camera, Texture TextureIndex)
		{
			if (renderer.currentOptions.IsUseNewRenderer)
			{
				Draw(defaultVAO, Position, Direction, Up, Side, Size, Camera, TextureIndex);
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
		public void Draw(VertexArrayObject VAO, OpenBveApi.Math.Vector3 Position, OpenBveApi.Math.Vector3 Direction, OpenBveApi.Math.Vector3 Up, OpenBveApi.Math.Vector3 Side, OpenBveApi.Math.Vector3 Size, OpenBveApi.Math.Vector3 Camera, Texture TextureIndex)
		{
			renderer.DefaultShader.Activate();
			renderer.ResetShader(renderer.DefaultShader);
			// matrix
			renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			renderer.DefaultShader.SetCurrentModelViewMatrix(Matrix4D.Scale(Size) * (Matrix4D)new Transformation(Direction, Up, Side) * Matrix4D.CreateTranslation(Position.X - Camera.X, Position.Y - Camera.Y, -Position.Z + Camera.Z) * renderer.CurrentViewMatrix);

			// texture
			if (TextureIndex != null && renderer.currentHost.LoadTexture(TextureIndex, OpenGlTextureWrapMode.ClampClamp))
			{
				renderer.DefaultShader.SetIsTexture(true);
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
			renderer.lastVAO = -1;
			VAO.UnBind();
			renderer.DefaultShader.Deactivate();

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
		public void DrawImmediate(OpenBveApi.Math.Vector3 Position, OpenBveApi.Math.Vector3 Direction, OpenBveApi.Math.Vector3 Up, OpenBveApi.Math.Vector3 Side, OpenBveApi.Math.Vector3 Size, OpenBveApi.Math.Vector3 Camera, Texture TextureIndex)
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
			Faces[0] = new int[] { 0, 1, 2, 3 };
			Faces[1] = new int[] { 0, 4, 5, 1 };
			Faces[2] = new int[] { 0, 3, 7, 4 };
			Faces[3] = new int[] { 6, 5, 4, 7 };
			Faces[4] = new int[] { 6, 7, 3, 2 };
			Faces[5] = new int[] { 6, 2, 1, 5 };
			if (TextureIndex == null || !renderer.currentHost.LoadTexture(TextureIndex, OpenGlTextureWrapMode.ClampClamp))
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
				return;
			}
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, TextureIndex.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			Vector2[][] t = new Vector2[6][];
			t[0] = new Vector2[] { new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0) };
			t[1] = new Vector2[] { new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0) };
			t[2] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
			t[3] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
			t[4] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
			t[5] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
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
