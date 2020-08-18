using System;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	/// <summary>
	/// Class representing the abstract IBO
	/// </summary>
	public abstract class IndexBufferObject : IDisposable
	{
		/// <summary>The openGL buffer name</summary>
		private readonly int handle;
		protected readonly BufferUsageHint drawType;
		private bool disposed;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="DrawType">The hint representing how the object is to be used and therefore guides OpenGL's optimization of the object</param>
		protected IndexBufferObject(BufferUsageHint DrawType)
		{
			GL.GenBuffers(1, out handle);
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
		internal abstract void BufferData();

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		internal abstract void Draw(PrimitiveType DrawMode);

		/// <summary>
		/// Draws the object
		/// </summary>
		/// <param name="DrawMode">Specifies the primitive or primitives that will be created from vertices</param>
		/// <param name="Start">Start position of vertex index</param>
		/// <param name="Count">Number of vertex indices to use</param>
		internal abstract void Draw(PrimitiveType DrawMode, int Start, int Count);

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

		~IndexBufferObject()
		{
			if (!disposed)
			{
				BaseRenderer.iboToDelete.Add(handle);
			}
		}
	}

	public abstract class IndexBufferObject<T> : IndexBufferObject where T : struct
	{
		protected readonly T[] indexData;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="IndexData">An int array of vertex indices that make the object</param>
		/// <param name="DrawType">The hint representing how the object is to be used and therefore guides OpenGL's optimization of the object</param>
		protected IndexBufferObject(T[] IndexData, BufferUsageHint DrawType) : base(DrawType)
		{
			indexData = IndexData;
		}
	}

	public class IndexBufferObjectUS : IndexBufferObject<ushort>
	{
		public IndexBufferObjectUS(ushort[] IndexData, BufferUsageHint DrawType) : base(IndexData, DrawType)
		{
		}

		internal override void BufferData()
		{
			GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(ushort), indexData, drawType);
		}

		internal override void Draw(PrimitiveType DrawMode)
		{
			GL.DrawElements(DrawMode, indexData.Length, DrawElementsType.UnsignedShort, 0);
		}

		internal override void Draw(PrimitiveType DrawMode, int Start, int Count)
		{
			GL.DrawElements(DrawMode, Count, DrawElementsType.UnsignedShort, Start * sizeof(ushort));
		}
	}

	public class IndexBufferObjectUI : IndexBufferObject<uint>
	{
		public IndexBufferObjectUI(uint[] IndexData, BufferUsageHint DrawType) : base(IndexData, DrawType)
		{
		}

		internal override void BufferData()
		{
			GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(uint), indexData, drawType);
		}

		internal override void Draw(PrimitiveType DrawMode)
		{
			GL.DrawElements(DrawMode, indexData.Length, DrawElementsType.UnsignedInt, 0);
		}

		internal override void Draw(PrimitiveType DrawMode, int Start, int Count)
		{
			GL.DrawElements(DrawMode, Count, DrawElementsType.UnsignedInt, Start * sizeof(uint));
		}
	}
}
