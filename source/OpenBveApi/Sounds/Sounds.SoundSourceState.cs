namespace OpenBveApi.Sounds
{
	/// <summary>Represents the state of a sound source.</summary>
	public enum SoundSourceState
	{
		/// <summary>The sound will start playing once in audible range. The OpenAL sound name is not yet valid.</summary>
		PlayPending,
		/// <summary>The sound is playing and the OpenAL source name is valid.</summary>
		Playing,
		/// <summary>The sound will stop playing. The OpenAL sound name is still valid.</summary>
		StopPending,
		/// <summary>The sound has stopped and will be removed from the list of sound sources. The OpenAL source name is not valid any longer.</summary>
		Stopped
	}
}
