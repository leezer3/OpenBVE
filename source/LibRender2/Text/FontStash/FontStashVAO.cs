using System;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	public class FontStashVAO : IDisposable
	{
		private readonly uint _handle;
		private readonly int _stride;

		public FontStashVAO(int stride)
		{
			if (stride <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(stride));
			}

			_stride = stride;

			GL.GenVertexArrays(1, out _handle);
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(_handle);
		}

		public void Bind()
		{
			GL.BindVertexArray(_handle);
		}

		public unsafe void VertexAttribPointer(int location, int size, VertexAttribPointerType type, bool normalized, int offset)
		{
			GL.EnableVertexAttribArray(location);
			GL.VertexAttribPointer(location, size, type, normalized, _stride, new IntPtr(offset));
		}
	}
}
