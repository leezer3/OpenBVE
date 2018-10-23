namespace OpenBveApi.Interface
{
	/// <summary>The different types of log messages</summary>
	public enum MessageType
	{
		/// <summary>The message contains information only</summary>
		Information,
		/// <summary>The message is a warning</summary>
		/// <remarks>May normally be ignored</remarks>
		Warning,
		/// <summary>The message is an error</summary>
		/// <remarks>May generally be ignored, but may have issues such as missing textures etc.</remarks>
		Error,
		/// <summary>The message is a critical error</summary>
		/// <remarks>The affected object etc. will generally not function correctly</remarks>
		Critical
	}
}
