using System;

namespace OpenBveApi
{
	/// <summary>An abstract container for the VAO</summary>
	public abstract class AbstractVAO : IDisposable
	{
		/// <summary>The dispose method</summary>
		public abstract void Dispose();
	}
}
