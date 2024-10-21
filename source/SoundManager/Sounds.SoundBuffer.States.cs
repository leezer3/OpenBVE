namespace SoundManager
{
	/// <summary>The states of a sound buffer</summary>
	public enum SoundBufferState
	{
		/// <summary>The buffer has not yet been loaded</summary>
		NotLoaded,
		/// <summary>The buffer is currently loading</summary>
		Loading,
		/// <summary>The buffer has been loaded</summary>
		Loaded,
		/// <summary>The buffer is invalid</summary>
		Invalid
	}
}
