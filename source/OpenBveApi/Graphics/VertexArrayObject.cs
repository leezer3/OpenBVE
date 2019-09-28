using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace OpenBveApi.Graphics
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
}
