using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenBveApi.Graphics
{
	/// <summary>
	/// Class that represents an OpenGL/OpenTK vertex buffer object 
	/// </summary>
	public class VertexBufferObject : IDisposable
	{
		private readonly int handle;
		private readonly LibRenderVertex[] vertexData;
		private readonly BufferUsageHint drawType;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="VertexData">An array containing the actual vertex coordinates and texture coordinates as X, Y, Z, U, V</param>
		/// <param name="DrawType">The hint representing how the object is to be used and therefore guides OpenGL's optimization of the object</param>
		public VertexBufferObject(LibRenderVertex[] VertexData, BufferUsageHint DrawType)
		{
			GL.GenBuffers(1, out handle);
			vertexData = VertexData;
			drawType = DrawType;
		}

		/// <summary>
		/// Binds the VBO ready for use
		/// </summary>
		internal void Bind()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
		}

		/// <summary>
		/// Copies the VertexData into the the VBO must be done before using the VBO
		/// </summary>
		internal void BufferData()
		{
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * LibRenderVertex.SizeInBytes), vertexData, drawType);
		}

		/// <summary>
		/// Update the VertexData into the the VBO
		/// </summary>
		internal void BufferSubData(LibRenderVertex[] VertexData, int Offset = 0)
		{
			GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(Offset * LibRenderVertex.SizeInBytes), new IntPtr(VertexData.Length * LibRenderVertex.SizeInBytes), VertexData);
		}

		/// <summary>
		/// Enables the attribute
		/// </summary>
		internal void EnableAttribute(VertexLayout VertexLayout)
		{
			if (VertexLayout.Position >= 0)
			{
				GL.EnableVertexAttribArray(VertexLayout.Position);
			}

			if (VertexLayout.Normal >= 0)
			{
				GL.EnableVertexAttribArray(VertexLayout.Normal);
			}

			if (VertexLayout.UV >= 0)
			{
				GL.EnableVertexAttribArray(VertexLayout.UV);
			}

			if (VertexLayout.Color >= 0)
			{
				GL.EnableVertexAttribArray(VertexLayout.Color);
			}
		}

		/// <summary>
		/// Sets the attribute ready for use in the VAO
		/// </summary>
		internal void SetAttribute(VertexLayout VertexLayout)
		{
			if (VertexLayout.Position >= 0)
			{
				GL.VertexAttribPointer(VertexLayout.Position, 3, VertexAttribPointerType.Float, false, LibRenderVertex.SizeInBytes, 0);
			}

			if (VertexLayout.Normal >= 0)
			{
				GL.VertexAttribPointer(VertexLayout.Normal, 3, VertexAttribPointerType.Float, false, LibRenderVertex.SizeInBytes, Vector3.SizeInBytes);
			}

			if (VertexLayout.UV >= 0)
			{
				GL.VertexAttribPointer(VertexLayout.UV, 2, VertexAttribPointerType.Float, false, LibRenderVertex.SizeInBytes, Vector3.SizeInBytes * 2);
			}

			if (VertexLayout.Color >= 0)
			{
				GL.VertexAttribPointer(VertexLayout.Color, 4, VertexAttribPointerType.Float, false, LibRenderVertex.SizeInBytes, Vector3.SizeInBytes * 2 + Vector2.SizeInBytes);
			}
		}

		/// <summary>
		/// Disables the attribute
		/// </summary>
		internal void DisableAttribute(VertexLayout VertexLayout)
		{
			if (VertexLayout.Position >= 0)
			{
				GL.DisableVertexAttribArray(VertexLayout.Position);
			}

			if (VertexLayout.Normal >= 0)
			{
				GL.DisableVertexAttribArray(VertexLayout.Normal);
			}

			if (VertexLayout.UV >= 0)
			{
				GL.DisableVertexAttribArray(VertexLayout.UV);
			}

			if (VertexLayout.Color >= 0)
			{
				GL.DisableVertexAttribArray(VertexLayout.Color);
			}
		}

		/// <summary>
		/// Dispose method to clean up the VBO releases the OpenGL Buffer
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				GL.DeleteBuffer(handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}
	}
}
