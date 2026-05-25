using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Managers
{
	/// <summary>
	/// Manages the lifecycle of GPU resources to prevent leaks.
	/// </summary>
	public class GpuResourceManager
	{
		private static readonly List<int> vaosToDelete = new List<int>();
		private static readonly List<int> vbosToDelete = new List<int>();
		private static readonly List<int> ibosToDelete = new List<int>();

		/// <summary>
		/// Queues a Vertex Array Object for deletion.
		/// </summary>
		public static void QueueVaoForDeletion(int handle)
		{
			if (handle <= 0) return;
			lock (vaosToDelete)
			{
				vaosToDelete.Add(handle);
			}
		}

		/// <summary>
		/// Queues a Vertex Buffer Object for deletion.
		/// </summary>
		public static void QueueVboForDeletion(int handle)
		{
			if (handle <= 0) return;
			lock (vbosToDelete)
			{
				vbosToDelete.Add(handle);
			}
		}

		/// <summary>
		/// Queues an Index Buffer Object for deletion.
		/// </summary>
		public static void QueueIboForDeletion(int handle)
		{
			if (handle <= 0) return;
			lock (ibosToDelete)
			{
				ibosToDelete.Add(handle);
			}
		}

		/// <summary>
		/// Deletes all queued resources. Must be called from a thread with a valid OpenGL context.
		/// </summary>
		public void ReleaseResources()
		{
			lock (vaosToDelete)
			{
				if (vaosToDelete.Count > 0)
				{
					foreach (int handle in vaosToDelete)
					{
						int h = handle;
						GL.DeleteVertexArrays(1, ref h);
					}
					vaosToDelete.Clear();
				}
			}

			lock (vbosToDelete)
			{
				if (vbosToDelete.Count > 0)
				{
					foreach (int handle in vbosToDelete)
					{
						int h = handle;
						GL.DeleteBuffers(1, ref h);
					}
					vbosToDelete.Clear();
				}
			}

			lock (ibosToDelete)
			{
				if (ibosToDelete.Count > 0)
				{
					foreach (int handle in ibosToDelete)
					{
						int h = handle;
						GL.DeleteBuffers(1, ref h);
					}
					ibosToDelete.Clear();
				}
			}
		}
	}
}
