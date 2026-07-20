using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibRender2
{
	/// <summary>
	/// Class representing an OpenGL/OpenTK VAO
	/// </summary>
	public class VertexArrayObject : IDisposable
	{
		internal readonly int handle;
		private VertexBufferObject vbo;
		private IndexBufferObject ibo;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		public VertexArrayObject()
		{
			GL.GenVertexArrays(1, out handle);
			if (handle == 0)
			{
				throw new InvalidOperationException("Failed to generate the required vertex array handle- No openGL context.");
			}
		}

		/// <summary>
		/// Binds the VAO ready for use
		/// </summary>
		public void Bind()
		{
			GL.BindVertexArray(handle);
		}

		/// <summary>
		/// Sets the VAO vertex layout attributes
		/// </summary>
		/// <remarks>
		/// These attributes remain valid unless a different shader is used to draw the object, and will be remembered by the VAO
		/// </remarks>
		public void SetAttributes(VertexLayout VertexLayout)
		{
			vbo.Bind();
			vbo.EnableAttribute(VertexLayout);
			vbo.SetAttribute(VertexLayout);
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
		/// Updates the VertexData in the the VBO
		/// </summary>
		public void UpdateVBO(LibRenderVertex[] VertexData, int Offset = 0)
		{
			vbo.Bind();
			vbo.BufferSubData(VertexData, Offset);
		}

		/// <summary>
		/// Draw using VAO
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		public void Draw(PrimitiveType DrawMode)
		{
			ibo.Draw(DrawMode);
		}

		/// <summary>
		/// Draw using VAO
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		/// <param name="Start">Start position of vertex index</param>
		/// <param name="Count">Number of vertex indices to use</param>
		public void Draw(PrimitiveType DrawMode, int Start, int Count)
		{
			ibo.Draw(DrawMode, Start, Count);
		}

		/// <summary>
		/// Draws the VAO using non-indexed rendering (no IBO required).</summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices.</param>
		/// <param name="Start">Start position of the first vertex.</param>
		/// <param name="Count">Number of vertices to render.</param>
		/// <remarks>
		/// Used by the debug normals overlay, where the vertex buffer is already laid out as contiguous line pairs.
		/// Avoids holding a duplicate index buffer in RAM.
		/// </remarks>
		public void DrawArrays(PrimitiveType DrawMode, int Start, int Count)
		{
			GL.DrawArrays(DrawMode, Start, Count);
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
			if (disposed)
			{
				return;
			}

			ibo?.Dispose();
			vbo?.Dispose();

			GL.DeleteVertexArray(handle);
			GC.SuppressFinalize(this);
			disposed = true;
		}

		~VertexArrayObject()
		{
			if (disposed)
			{
				return;
			}

			lock (BaseRenderer.vaoToDelete)
			{
				BaseRenderer.vaoToDelete.Add(handle);
			}
		}
	}

	public static class VAOExtensions
	{
		/// <summary>Create an OpenGL/OpenTK VAO for a mesh</summary>
		/// <param name="mesh">The mesh</param>
		/// <param name="isDynamic">Whether the mesh is dynamic (e.g. part of an animated object / train)</param>
		/// <param name="vertexLayout">The vertex layout to use</param>
		/// <param name="renderer">A reference to the base renderer</param>
		public static void CreateVAO(Mesh mesh, bool isDynamic, VertexLayout vertexLayout, BaseRenderer renderer)
		{
			if (mesh.VAO is VertexArrayObject)
			{
				return;
			}
			if (!renderer.GameWindow.Context.IsCurrent)
			{
				renderer.RunInRenderThread(() =>
				{
					createVAO(mesh, isDynamic, vertexLayout, renderer);
				}, 2000);
			}
			else
			{
				createVAO(mesh, isDynamic, vertexLayout, renderer);
			}
		}

		
		private static void createVAO(Mesh mesh, bool isDynamic, VertexLayout vertexLayout, BaseRenderer renderer)
		{
			if (mesh == null)
			{
				return;
			}
			try
			{
				var hint = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

				/* n.b. Initial length should be at least number of vertices in mesh. (may be greater if some are re-used with different UV)
				 *      This marginally helps loading very large objects (as default C# list starts at a capacity of 4 then doubles exponentially)     
				 */

			int totalFaceVertices = 0;
			for (int i = 0; i < mesh.Faces.Length; i++)
			{
				totalFaceVertices += mesh.Faces[i].Vertices.Length;
			}

			var vertexData = new List<LibRenderVertex>(totalFaceVertices);
			var indexData = new List<uint>(totalFaceVertices);

			for (int i = 0; i < mesh.Faces.Length; i++)
			{
				mesh.Faces[i].IboStartIndex = indexData.Count;

				foreach (var vertex in mesh.Faces[i].Vertices)
				{
					vertexData.Add(new LibRenderVertex(mesh.Vertices[vertex.Index], vertex.Normal));
				}

				indexData.AddRange(Enumerable.Range(mesh.Faces[i].IboStartIndex, mesh.Faces[i].Vertices.Length).Select(x => (uint)x));
			}

			VertexArrayObject VAO = (VertexArrayObject)mesh.VAO;
			VAO?.UnBind();
			VAO?.Dispose();

			VAO = new VertexArrayObject();
			VAO.Bind();
			VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), hint));
			if (indexData.Count > 65530)
			{
				//Marginal headroom, although it probably doesn't matter
				VAO.SetIBO(new IndexBufferObjectUI(indexData.ToArray(), hint));
			}
			else
			{
				VAO.SetIBO(new IndexBufferObjectUS(indexData.Select(x => (ushort)x).ToArray(), hint));
			}

			VAO.SetAttributes(vertexLayout);
			VAO.UnBind();
			mesh.VAO = VAO;

			/*
			 * The normals VAO is only used for the debug normals overlay (OptionNormals).
			 * It is built lazily via CreateNormalsVAO to avoid holding a second large
			 * vertex buffer in RAM for every loaded mesh.
			 */
			VertexArrayObject NormalsVAO = (VertexArrayObject)mesh.NormalsVAO;
			NormalsVAO?.UnBind();
			NormalsVAO?.Dispose();
			mesh.NormalsVAO = null;
			}
			catch (Exception e)
			{
				renderer.currentHost.AddMessage(MessageType.Error, false, $"Creating VAO failed with the following error: {e}");
			}
		}

		/// <summary>Creates the OpenGL/OpenTK VAO for the normals overlay of a mesh.</summary>
		/// <remarks>This is built lazily on first use of the normals debug view, as it duplicates the vertex data.</remarks>
		public static void CreateNormalsVAO(Mesh mesh, bool isDynamic, VertexLayout vertexLayout, BaseRenderer renderer)
		{
			if (mesh == null || mesh.NormalsVAO is VertexArrayObject)
			{
				return;
			}
			if (!renderer.GameWindow.Context.IsCurrent)
			{
				renderer.RunInRenderThread(() =>
				{
					createNormalsVAO(mesh, isDynamic, vertexLayout, renderer);
				}, 2000);
			}
			else
			{
				createNormalsVAO(mesh, isDynamic, vertexLayout, renderer);
			}
		}

		private static void createNormalsVAO(Mesh mesh, bool isDynamic, VertexLayout vertexLayout, BaseRenderer renderer)
		{
			if (mesh == null)
			{
				return;
			}

			try
			{
				var hint = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

				int totalFaceVertices = 0;
				for (int i = 0; i < mesh.Faces.Length; i++)
				{
					totalFaceVertices += mesh.Faces[i].Vertices.Length;
				}

				// Debug normals overlay: a contiguous list of line pairs (vertex -> vertex + normal).
				// Rendered with DrawArrays (no IBO), so we do not hold a duplicate index buffer in RAM.
				// Colour is baked per-vertex (purple) because the shader multiplies the uniform colour by the
				// (zeroed) vertex colour attribute when colour is disabled, which would otherwise render black.
				var normalsVertexData = new List<LibRenderVertex>(totalFaceVertices * 2);
				var normalColor = new Color128(0.6f, 0.2f, 1.0f, 1.0f); // purple

				// Scale normal lines relative to the mesh bounding box so they stay a sensible on-screen size.
				// A fixed 1-unit length blows up into huge overdraw when the camera is close to a small mesh.
				double scale = 0.01;
				if (mesh.Vertices.Length > 0)
				{
					Vector3 min = mesh.Vertices[0].Coordinates;
					Vector3 max = mesh.Vertices[0].Coordinates;
					for (int v = 1; v < mesh.Vertices.Length; v++)
					{
						Vector3 c = mesh.Vertices[v].Coordinates;
						min.X = Math.Min(min.X, c.X);
						min.Y = Math.Min(min.Y, c.Y);
						min.Z = Math.Min(min.Z, c.Z);
						max.X = Math.Max(max.X, c.X);
						max.Y = Math.Max(max.Y, c.Y);
						max.Z = Math.Max(max.Z, c.Z);
					}

					double size = Math.Max(max.X - min.X, Math.Max(max.Y - min.Y, max.Z - min.Z));
					if (size > 0.0)
					{
						scale = size * 0.01;
					}
				}

				for (int i = 0; i < mesh.Faces.Length; i++)
				{
					foreach (var vertex in mesh.Faces[i].Vertices)
					{
						Vector3 coordinates = mesh.Vertices[vertex.Index].Coordinates;
						Vector3 tip = coordinates + vertex.Normal * scale;
						// Pass the normal so the shader's lighting produces a visible (non-black) colour.
						// UV/MatrixChain are omitted so the shader cannot sample any texture.
						normalsVertexData.Add(new LibRenderVertex(coordinates, vertex.Normal, normalColor));
						normalsVertexData.Add(new LibRenderVertex(tip, vertex.Normal, normalColor));
					}
				}

				VertexArrayObject NormalsVAO = (VertexArrayObject)mesh.NormalsVAO;
				NormalsVAO?.UnBind();
				NormalsVAO?.Dispose();

				NormalsVAO = new VertexArrayObject();
				NormalsVAO.Bind();
				NormalsVAO.SetVBO(new VertexBufferObject(normalsVertexData.ToArray(), hint));
				// Position + Normal + Colour: UV/matrix omitted so the shader cannot sample any texture,
				// but the normal is supplied so lighting yields a visible (non-black) colour.
				var normalsLayout = new VertexLayout
				{
					Position = vertexLayout.Position,
					Normal = vertexLayout.Normal,
					Color = vertexLayout.Color
				};
				NormalsVAO.SetAttributes(normalsLayout);
				NormalsVAO.UnBind();
				mesh.NormalsVAO = NormalsVAO;
			}
			catch (Exception e)
			{
				renderer.currentHost.AddMessage(MessageType.Error, false,
					$"Creating normals VAO failed with the following error: {e}");
			}
		}

		/// <summary>Create an OpenGL/OpenTK VAO for a static background</summary>
		/// <param name="background">The background</param>
		/// <param name="vertexLayout">The vertex layout to use</param>
		/// <param name="renderer">A reference to the base renderer</param>
		public static void CreateVAO(this StaticBackground background, VertexLayout vertexLayout, BaseRenderer renderer)
		{
			try
			{
				float y0, y1;

				if (background.KeepAspectRatio)
				{
					double hh = Math.PI * background.BackgroundImageDistance * background.Texture.Height / (background.Texture.Width * background.Repetition);
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
					float x = (float)(background.BackgroundImageDistance * Math.Cos(angleValue));
					float z = (float)(background.BackgroundImageDistance * Math.Sin(angleValue));
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
						UV = new Vector2f(textureX, 0.005f),
						Color = Color128.White
					});

					vertexData.Add(new LibRenderVertex
					{
						Position = bottom[i],
						UV = new Vector2f(textureX, 0.995f),
						Color = Color128.White
					});

					vertexData.Add(new LibRenderVertex
					{
						Position = bottom[j],
						UV = new Vector2f(textureX + textureIncrement, 0.995f),
						Color = Color128.White
					});

					vertexData.Add(new LibRenderVertex
					{
						Position = top[j],
						UV = new Vector2f(textureX + textureIncrement, 0.005f),
						Color = Color128.White
					});

					indexData.AddRange(new[] { 0, 1, 2 }.Select(x => x + indexOffset).Select(x => (ushort)x));
					indexData.AddRange(new[] { 0, 2, 3 }.Select(x => x + indexOffset).Select(x => (ushort)x));

					// top cap
					vertexData.Add(new LibRenderVertex
					{
						Position = new Vector3f(0.0f, top[i].Y, 0.0f),
						UV = new Vector2f(textureX + 0.5f * textureIncrement, 0.1f),
						Color = Color128.White
					});

					indexData.AddRange(new[] { 0, 3, 4 }.Select(x => x + indexOffset).Select(x => (ushort)x));

					// bottom cap
					vertexData.Add(new LibRenderVertex
					{
						Position = new Vector3f(0.0f, bottom[i].Y, 0.0f),
						UV = new Vector2f(textureX + 0.5f * textureIncrement, 0.9f),
						Color = Color128.White
					});

					indexData.AddRange(new[] { 5, 2, 1 }.Select(x => x + indexOffset).Select(x => (ushort)x));

					// finish
					textureX += textureIncrement;
				}

				VertexArrayObject VAO = (VertexArrayObject)background.VAO;
				VAO?.UnBind();
				VAO?.Dispose();

				VAO = new VertexArrayObject();
				VAO.Bind();
				VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), BufferUsageHint.StaticDraw));
				VAO.SetIBO(new IndexBufferObjectUS(indexData.ToArray(), BufferUsageHint.StaticDraw));
				VAO.SetAttributes(vertexLayout);
				VAO.UnBind();
				background.VAO = VAO;
			}
			catch (Exception e)
			{
				renderer.currentHost.AddMessage(MessageType.Error, false, $"Creating VAO failed with the following error: {e}");
			}
		}
	}
}

