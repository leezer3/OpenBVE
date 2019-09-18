using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace SoundManager
{
	/// <summary>Represents a sound buffer.</summary>
	public class SoundBuffer : SoundHandle
	{
		// --- members ---
		/// <summary>The origin where the sound can be loaded from.</summary>
		internal readonly SoundOrigin Origin;
		/// <summary>The default effective radius.</summary>
		internal double Radius;
		/// <summary>Whether the sound is loaded and the OpenAL sound name is valid.</summary>
		public bool Loaded;
		/// <summary>The OpenAL sound name. Only valid if the sound is loaded.</summary>
		public int OpenAlBufferName;
		/// <summary>The duration of the sound in seconds. Only valid if the sound is loaded.</summary>
		public double Duration;
		/// <summary>Whether to ignore further attemps to load the sound after previous attempts have failed.</summary>
		internal bool Ignore;
		/// <summary>The function script controlling this sound's volume.</summary>
		internal FunctionScript PitchFunction;
		/// <summary>The function script controlling this sound's volume.</summary>
		internal FunctionScript VolumeFunction;

		internal double InternalVolumeFactor;

		/// <summary>Creates a new sound buffer</summary>
		/// <param name="path">The on-disk path to the sound to load</param>
		/// <param name="radius">The radius for this sound</param>
		internal SoundBuffer(HostInterface host, string path, double radius)
		{
			Origin = new PathOrigin(path, host);
			Radius = radius;
			Loaded = false;
			OpenAlBufferName = 0;
			Duration = 0.0;
			InternalVolumeFactor = 0.5;
			Ignore = false;
			PitchFunction = null;
			VolumeFunction = null;

		}

		/// <summary>Creates a new sound buffer</summary>
		/// <param name="sound">The raw sound source, loaded via an API plugin</param>
		/// <param name="radius">The radius of the sound</param>
		internal SoundBuffer(Sound sound, double radius)
		{
			Origin = new RawOrigin(sound);
			Radius = radius;
			Loaded = false;
			OpenAlBufferName = 0;
			Duration = 0.0;
			InternalVolumeFactor = 0.5;
			Ignore = false;
			PitchFunction = null;
			VolumeFunction = null;
		}

		/// <summary>Creates a new uninitialized sound buffer</summary>
		internal SoundBuffer()
		{
			Origin = null;
			Radius = 0.0;
			Loaded = false;
			OpenAlBufferName = 0;
			Duration = 0.0;
			InternalVolumeFactor = 0.5;
			Ignore = false;
			PitchFunction = null;
			VolumeFunction = null;
		}

		/// <summary>Creates a new sound buffer from the specified sound origin</summary>
		/// <param name="origin">The SoundOrigin describing where to load the sound from</param>
		internal SoundBuffer(SoundOrigin origin)
		{
			Origin = origin;
			Radius = 0.0;
			Loaded = false;
			OpenAlBufferName = 0;
			Duration = 0.0;
			InternalVolumeFactor = 0.5;
			Ignore = false;
			PitchFunction = null;
			VolumeFunction = null;
		}

		/// <summary>Creates a clone of the specified sound buffer</summary>
		/// <param name="b">The buffer to clone</param>
		/// <returns>The new buffer</returns>
		internal SoundBuffer Clone(SoundBuffer b)
		{
			return new SoundBuffer(b.Origin)
			{
				Radius = b.Radius,
				Loaded = false,
				OpenAlBufferName = 0,
				Duration = b.Duration,
				InternalVolumeFactor = b.InternalVolumeFactor,
				Ignore = false,
				PitchFunction = b.PitchFunction,
				VolumeFunction = b.VolumeFunction
			};
		}
	}
}
