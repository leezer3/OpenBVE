using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	public class BufferObject<T> : IDisposable
	{
		private readonly int _handle;
		private readonly BufferTarget _bufferType;
		private readonly int _size;

		public unsafe BufferObject(int size, BufferTarget bufferType, bool isDynamic)
		{
			ErrorCode e = GL.GetError();
			if (e != ErrorCode.NoError)
			{
				int b = 0;
			}
			_bufferType = bufferType;
			_size = size;

			_handle = GL.GenBuffer();

			Bind();

			var elementSizeInBytes = Marshal.SizeOf<T>();
			GL.BufferData(bufferType, size * elementSizeInBytes, IntPtr.Zero, isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
			e = GL.GetError();
			if (e != ErrorCode.NoError)
			{
				int b = 0;
			}
		}

		public void Bind()
		{
			GL.BindBuffer(_bufferType, _handle);
		}

		public void Dispose()
		{
			GL.DeleteBuffer(_handle);

		}

		public unsafe void SetData(T[] data, int startIndex, int elementCount)
		{
			ErrorCode e = GL.GetError();
			if (e != ErrorCode.NoError)
			{
				int b = 0;
			}
			Bind();

			fixed (T* dataPtr = &data[startIndex])
			{
				var elementSizeInBytes = sizeof(T);

				GL.BufferSubData(_bufferType, IntPtr.Zero, elementCount * elementSizeInBytes, new IntPtr(dataPtr));
			}

			e = GL.GetError();
			if (e != ErrorCode.NoError)
			{
				int b = 0;
			}
		}
	}
}
