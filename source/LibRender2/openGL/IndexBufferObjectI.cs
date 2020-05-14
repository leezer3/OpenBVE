using System;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	/// <summary>
	/// Class representing an OpenGL/OpenTK IBO/EBO
	/// </summary>
	public class IndexBufferObjectI : IndexBufferObject
	{
		/// <summary>The openGL buffer name</summary>
		private readonly int handle;
		private readonly uint[] indexData;
		private readonly BufferUsageHint drawType;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="IndexData">An int array of vertex indices that make the object</param>
		/// <param name="DrawType">The hint representing how the object is to be used and therefore guides OpenGL's optimization of the object</param>
		public IndexBufferObjectI(uint[] IndexData, BufferUsageHint DrawType)
		{
			GL.GenBuffers(1, out handle);
			indexData = IndexData;
			drawType = DrawType;
		}

		/// <summary>
		/// Binds the IBO/EBO ready for use
		/// </summary>
		internal override void Bind()
		{
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle);
		}

		/// <summary>Copies the Indices into the IBO/EBO</summary>
		/// <remarks>Must be called before attempting to use the IBO/ EBO</remarks>
		internal override void BufferData()
		{
			GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(uint), indexData, drawType);
		}

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		internal override void Draw(PrimitiveType DrawMode)
		{
			GL.DrawElements(DrawMode, indexData.Length, DrawElementsType.UnsignedInt, 0);
		}

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		/// <param name="Start">Start position of vertex index</param>
		/// <param name="Count">Number of vertex indices to use</param>
		internal override void Draw(PrimitiveType DrawMode, int Start, int Count)
		{
			GL.DrawElements(DrawMode, Count, DrawElementsType.UnsignedInt, Start * sizeof(uint));
		}

		/// <summary>
		/// Dispose method to clean up the IBO/EBO releases the OpenGL Buffer
		/// </summary>
		public new void Dispose()
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
