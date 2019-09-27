using System.Linq;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Primitives
{
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
					Position = new Vector3(1.0f, 1.0f, -1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, -1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, -1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, -1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				},

				// right
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, 1.0f, -1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, -1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, 1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, 1.0f, 1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},

				// top
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, 1.0f, -1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, 1.0f, 1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, 1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, -1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},

				// front
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, 1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, 1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, 1.0f, 1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, 1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},

				// left
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, 1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, -1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, -1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, 1.0f, 1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},

				// bottom
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, 1.0f),
					UV = Vector2.Zero,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, 1.0f),
					UV = Vector2.UnitX,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(1.0f, -1.0f, -1.0f),
					UV = Vector2.One,
					Color = Color4.White
				},
				new LibRenderVertex
				{
					Position = new Vector3(-1.0f, -1.0f, -1.0f),
					UV = Vector2.UnitY,
					Color = Color4.White
				}
			};

			defaultVAO = new VertexArrayObject();
			defaultVAO.Bind();
			defaultVAO.SetVBO(new VertexBufferObject(vertexData, BufferUsageHint.StaticDraw));
			defaultVAO.SetIBO(new IndexBufferObject(Enumerable.Range(0, vertexData.Length).ToArray(), BufferUsageHint.StaticDraw));
			defaultVAO.UnBind();
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
			Draw(Position, Direction, Up, Side, new OpenBveApi.Math.Vector3(Size, Size, Size), Camera, TextureIndex);
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
			Draw(defaultVAO, Position, Direction, Up, Side, Size, Camera, TextureIndex);
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
			renderer.DefaultShader.Use();
			renderer.ResetShader(renderer.DefaultShader);

			// matrix
			renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			renderer.DefaultShader.SetCurrentModelViewMatrix(Matrix4d.Scale((Vector3d)Size) * (Matrix4d)new Transformation(Direction, Up, Side) * Matrix4d.CreateTranslation(Position.X - Camera.X, Position.Y - Camera.Y, -Position.Z + Camera.Z) * renderer.CurrentViewMatrix);

			// texture
			if (TextureIndex != null && renderer.currentHost.LoadTexture(TextureIndex, OpenGlTextureWrapMode.ClampClamp))
			{
				renderer.DefaultShader.SetIsTexture(true);
				renderer.DefaultShader.SetTexture(0);

				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, TextureIndex.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				GL.Disable(EnableCap.Texture2D);
			}

			// render polygon
			VAO.Bind();
			VAO.Draw(renderer.DefaultShader.VertexLayout, PrimitiveType.Quads);
			VAO.UnBind();

			GL.BindTexture(TextureTarget.Texture2D, 0);
			renderer.DefaultShader.NonUse();

			GL.Disable(EnableCap.Texture2D);
		}
	}
}
