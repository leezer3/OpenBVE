using System;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	/// <summary>
	/// Class representing an OpenGL/OpenTK IBO/EBO
	/// </summary>
	public class IndexBufferObject : IDisposable
	{
		/// <summary>The openGL buffer name</summary>
		private readonly int handle;
		private readonly ushort[] indexData;
		private readonly BufferUsageHint drawType;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="IndexData">An int array of vertex indices that make the object</param>
		/// <param name="DrawType">The hint representing how the object is to be used and therefore guides OpenGL's optimization of the object</param>
		public IndexBufferObject(ushort[] IndexData, BufferUsageHint DrawType)
		{
			GL.GenBuffers(1, out handle);
			indexData = IndexData;
			drawType = DrawType;
		}

		/// <summary>
		/// Binds the IBO/EBO ready for use
		/// </summary>
		internal void Bind()
		{
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle);
		}

		/// <summary>Copies the Indices into the IBO/EBO</summary>
		/// <remarks>Must be called before attempting to use the IBO/ EBO</remarks>
		internal void BufferData()
		{
			GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(ushort), indexData, drawType);
		}

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		internal void Draw(PrimitiveType DrawMode)
		{
			GL.DrawElements(DrawMode, indexData.Length, DrawElementsType.UnsignedShort, 0);
		}

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		/// <param name="Start">Start position of vertex index</param>
		/// <param name="Count">Number of vertex indices to use</param>
		internal void Draw(PrimitiveType DrawMode, int Start, int Count)
		{
			GL.DrawElements(DrawMode, Count, DrawElementsType.UnsignedShort, Start * sizeof(ushort));
		}

		/// <summary>
		/// Dispose method to clean up the IBO/EBO releases the OpenGL Buffer
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
