using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	/// <summary>
	/// Class representing an OpenGL/OpenTK VAO
	/// </summary>
	public class VertexArrayObject : IDisposable
	{
		public static List<VertexArrayObject> Disposable = new List<VertexArrayObject>();

		private readonly int handle;
		private VertexBufferObject vbo;
		private IndexBufferObject ibo;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		public VertexArrayObject()
		{
			GL.GenVertexArrays(1, out handle);

			Disposable.Add(this);
		}

		/// <summary>
		/// Binds the VAO ready for use
		/// </summary>
		public void Bind()
		{
			GL.BindVertexArray(handle);
		}

		/// <summary>
		/// Adds a VBO object to the VAO needs to have one to draw, if a second is added it will replace the first and the first will be disposed of
		/// </summary>
		/// <param name="VBO">The VBO object to be added</param>
		public void SetVBO(VertexBufferObject VBO)
		{
			if (vbo != null)
			{
				UnBind();
				vbo.Dispose();
				Bind();
			}

			vbo = VBO;
			vbo.Bind();
			vbo.BufferData();
		}

		/// <summary>
		/// Adds a IBO object to the VAO needs to have one to draw, if a second is added it will replace the first and the first will be disposed of
		/// </summary>
		/// <param name="IBO">The IBO object to be added</param>
		public void SetIBO(IndexBufferObject IBO)
		{
			if (ibo != null)
			{
				UnBind();
				ibo.Dispose();
				Bind();
			}

			ibo = IBO;
			ibo.Bind();
			ibo.BufferData();
		}

		/// <summary>
		/// Update the VertexData into the the VBO
		/// </summary>
		public void UpdateVBO(LibRenderVertex[] VertexData, int Offset = 0)
		{
			vbo.Bind();
			vbo.BufferSubData(VertexData, Offset);
		}

		/// <summary>
		/// Draw using VAO
		/// </summary>
		/// <param name="VertexLayout"></param>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		public void Draw(VertexLayout VertexLayout, PrimitiveType DrawMode)
		{
			vbo.Bind();
			vbo.EnableAttribute(VertexLayout);
			vbo.SetAttribute(VertexLayout);
			ibo.Draw(DrawMode);
			vbo.DisableAttribute(VertexLayout);
		}

		/// <summary>
		/// Draw using VAO
		/// </summary>
		/// <param name="VertexLayout"></param>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		/// <param name="Start">Start position of vertex index</param>
		/// <param name="Count">Number of vertex indices to use</param>
		public void Draw(VertexLayout VertexLayout, PrimitiveType DrawMode, int Start, int Count)
		{
			vbo.Bind();
			vbo.EnableAttribute(VertexLayout);
			vbo.SetAttribute(VertexLayout);
			ibo.Draw(DrawMode, Start, Count);
			vbo.DisableAttribute(VertexLayout);
		}

		/// <summary>
		/// Unbinds the VAO deactivating the VAO from use
		/// </summary>
		public void UnBind()
		{
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		/// <summary>
		/// Dispose method to clean up the VAO releases the OpenGL Buffer
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				ibo?.Dispose();
				vbo?.Dispose();

				GL.DeleteVertexArray(handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}
	}

	public static class VAOExtensions
	{
		/// <summary>Create an OpenGL/OpenTK VAO for a mesh</summary>
		/// <param name="isDynamic"></param>
		public static void CreateVAO(ref Mesh mesh, bool isDynamic)
		{
			var hint = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

			var vertexData = new List<LibRenderVertex>();
			var indexData = new List<ushort>();

			var normalsVertexData = new List<LibRenderVertex>();
			var normalsIndexData = new List<ushort>();

			for (int i = 0; i < mesh.Faces.Length; i++)
			{
				mesh.Faces[i].IboStartIndex = indexData.Count;
				mesh.Faces[i].NormalsIboStartIndex = normalsIndexData.Count;

				foreach (var vertex in mesh.Faces[i].Vertices)
				{
					var data = new LibRenderVertex
					{
						Position = mesh.Vertices[vertex.Index].Coordinates,
						Normal = vertex.Normal,
						UV = mesh.Vertices[vertex.Index].TextureCoordinates
					};

					var coloredVertex = mesh.Vertices[vertex.Index] as ColoredVertex;

					if (coloredVertex != null)
					{
						data.Color = coloredVertex.Color;
					}
					else
					{
						data.Color = Color128.White;
					}

					vertexData.Add(data);

					var normalsData = new LibRenderVertex[2];
					normalsData[0].Position = data.Position;
					normalsData[1].Position = data.Position + data.Normal;

					for (int j = 0; j < normalsData.Length; j++)
					{
						normalsData[j].Color = Color128.White;
					}

					normalsVertexData.AddRange(normalsData);
				}

				indexData.AddRange(Enumerable.Range(mesh.Faces[i].IboStartIndex, mesh.Faces[i].Vertices.Length).Select(x => (ushort) x));
				normalsIndexData.AddRange(Enumerable.Range(mesh.Faces[i].NormalsIboStartIndex, mesh.Faces[i].Vertices.Length * 2).Select(x => (ushort) x));
			}

			VertexArrayObject VAO = (VertexArrayObject) mesh.VAO;
			VAO?.UnBind();
			VAO?.Dispose();

			VAO = new VertexArrayObject();
			VAO.Bind();
			VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), hint));
			VAO.SetIBO(new IndexBufferObject(indexData.ToArray(), hint));
			VAO.UnBind();
			mesh.VAO = VAO;
			VertexArrayObject NormalsVAO = (VertexArrayObject) mesh.NormalsVAO;
			NormalsVAO?.UnBind();
			NormalsVAO?.Dispose();

			NormalsVAO = new VertexArrayObject();
			NormalsVAO.Bind();
			NormalsVAO.SetVBO(new VertexBufferObject(normalsVertexData.ToArray(), hint));
			NormalsVAO.SetIBO(new IndexBufferObject(normalsIndexData.ToArray(), hint));
			NormalsVAO.UnBind();
			mesh.NormalsVAO = NormalsVAO;
		}

		public static void CreateVAO(this StaticBackground background)
		{
			float y0, y1;

			if (background.KeepAspectRatio)
			{
				int tw = background.Texture.Width;
				int th = background.Texture.Height;
				double hh = System.Math.PI * background.BackgroundImageDistance * th / (tw * background.Repetition);
				y0 = (float)(-0.5 * hh);
				y1 = (float)(1.5 * hh);
			}
			else
			{
				y0 = (float)(-0.125 * background.BackgroundImageDistance);
				y1 = (float)(0.375 * background.BackgroundImageDistance);
			}

			const int n = 32;
			Vector3f[] bottom = new Vector3f[n];
			Vector3f[] top = new Vector3f[n];
			double angleValue = 2.61799387799149 - 3.14159265358979 / n;
			const double angleIncrement = 6.28318530717958 / n;

			/*
			 * To ensure that the whole background cylinder is rendered inside the viewing frustum,
			 * the background is rendered before the scene with z-buffer writes disabled. Then,
			 * the actual distance from the camera is irrelevant as long as it is inside the frustum.
			 * */
			for (int i = 0; i < n; i++)
			{
				float x = (float)(background.BackgroundImageDistance * System.Math.Cos(angleValue));
				float z = (float)(background.BackgroundImageDistance * System.Math.Sin(angleValue));
				bottom[i] = new Vector3f(x, y0, z);
				top[i] = new Vector3f(x, y1, z);
				angleValue += angleIncrement;
			}

			float textureStart = 0.5f * (float)background.Repetition / n;
			float textureIncrement = -(float)background.Repetition / n;
			float textureX = textureStart;

			List<LibRenderVertex> vertexData = new List<LibRenderVertex>();
			List<ushort> indexData = new List<ushort>();

			for (int i = 0; i < n; i++)
			{
				int j = (i + 1) % n;
				int indexOffset = vertexData.Count;

				// side wall
				vertexData.Add(new LibRenderVertex
				{
					Position = top[i],
					UV = new Vector2(textureX, 0.005f),
					Color = Color128.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = bottom[i],
					UV = new Vector2(textureX, 0.995f),
					Color = Color128.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = bottom[j],
					UV = new Vector2(textureX + textureIncrement, 0.995f),
					Color = Color128.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = top[j],
					UV = new Vector2(textureX + textureIncrement, 0.005f),
					Color = Color128.White
				});

				indexData.AddRange(new[] { 0, 1, 2, 3 }.Select(x => x + indexOffset).Select(x => (ushort) x));

				// top cap
				vertexData.Add(new LibRenderVertex
				{
					Position = new Vector3f(0.0f, top[i].Y, 0.0f),
					UV = new Vector2(textureX + 0.5f * textureIncrement, 0.1f),
					Color = Color128.White
				});

				indexData.AddRange(new[] { 0, 3, 4 }.Select(x => x + indexOffset).Select(x => (ushort) x));

				// bottom cap
				vertexData.Add(new LibRenderVertex
				{
					Position = new Vector3f(0.0f, bottom[i].Y, 0.0f),
					UV = new Vector2(textureX + 0.5f * textureIncrement, 0.9f),
					Color = Color128.White
				});

				indexData.AddRange(new[] { 5, 2, 1 }.Select(x => x + indexOffset).Select(x => (ushort) x));

				// finish
				textureX += textureIncrement;
			}

			VertexArrayObject VAO = (VertexArrayObject) background.VAO;
			VAO?.UnBind();
			VAO?.Dispose();

			VAO = new VertexArrayObject();
			VAO.Bind();
			VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), BufferUsageHint.StaticDraw));
			VAO.SetIBO(new IndexBufferObject(indexData.ToArray(), BufferUsageHint.StaticDraw));
			VAO.UnBind();
			background.VAO = VAO;
		}
	}
}

