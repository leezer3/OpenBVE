using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;

namespace LibRender2
{
	/// <summary>
	/// Class representing the abstract IBO
	/// </summary>
	public abstract class IndexBufferObject : IDisposable
	{
		/// <summary>
		/// Binds the IBO/EBO ready for use
		/// </summary>
		internal abstract void Bind();

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

		public void Dispose()
		{
		}
	}
}
