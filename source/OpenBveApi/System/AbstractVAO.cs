using System;

namespace OpenBveApi
{
	/// <summary>An abstract container for the VAO</summary>
	/// <remarks>The VAO implementation is part of the renderer, but we require an abstract API class
	/// so it can be set as disposable</remarks>
	public abstract class AbstractVAO : IDisposable
	{
		/// <summary>The dispose method</summary>
		public abstract void Dispose();
	}
}
