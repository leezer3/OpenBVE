using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;
using OpenTK.Audio.OpenAL;

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
		public SoundBufferState Loaded;
		/// <summary>The OpenAL sound name. Only valid if the sound is loaded.</summary>
		public int OpenAlBufferName;
		/// <summary>Backing property for duration</summary>
		private double _duration;
		/// <summary>The duration of the sound in seconds. Only valid if the sound is loaded.</summary>
		public double Duration
		{
			get
			{
				// N.B. This call is actually made on the main thread
				Load();
				return _duration;
			}
			
		}
		/// <summary>Whether to ignore further attemps to load the sound after previous attempts have failed.</summary>
		internal bool Ignore;
		/// <summary>The function script controlling this sound's volume.</summary>
		internal FunctionScript PitchFunction;
		/// <summary>The function script controlling this sound's volume.</summary>
		internal FunctionScript VolumeFunction;

		internal double InternalVolumeFactor;

		/// <summary>Creates a new sound buffer</summary>
		/// <param name="host">The host application</param>
		/// <param name="path">The on-disk path to the sound to load</param>
		/// <param name="radius">The radius for this sound</param>
		internal SoundBuffer(HostInterface host, string path, double radius)
		{
			Origin = new PathOrigin(path, host);
			Radius = radius;
			Loaded = SoundBufferState.NotLoaded;
			OpenAlBufferName = 0;
			_duration = 0.0;
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
			Loaded = SoundBufferState.NotLoaded;
			OpenAlBufferName = 0;
			_duration = 0.0;
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
			Loaded = SoundBufferState.NotLoaded;
			OpenAlBufferName = 0;
			_duration = 0.0;
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
			Loaded = SoundBufferState.NotLoaded;
			OpenAlBufferName = 0;
			_duration = 0.0;
			InternalVolumeFactor = 0.5;
			Ignore = false;
			PitchFunction = null;
			VolumeFunction = null;
		}

		/// <summary>Creates a clone of this sound buffer</summary>
		/// <returns>The new buffer</returns>
		public SoundBuffer Clone()
		{
			return new SoundBuffer(this.Origin)
			{
				Radius = this.Radius,
				Loaded = SoundBufferState.NotLoaded,
				OpenAlBufferName = 0,
				_duration = 0.0,
				InternalVolumeFactor = this.InternalVolumeFactor,
				Ignore = false,
				PitchFunction = this.PitchFunction,
				VolumeFunction = this.VolumeFunction
			};
		}

		/// <summary>Loads the buffer into OpenAL</summary>
		public void Load()
		{
			if (Loaded == SoundBufferState.Loaded)
			{
				return;
			}
			if (Ignore)
			{
				return;
			}
			if (Origin.GetSound(out Sound sound))
			{
				if (sound.BitsPerSample == 8 | sound.BitsPerSample == 16)
				{
					byte[] bytes = sound.GetMonoMix();
					AL.GenBuffers(1, out OpenAlBufferName);
					ALFormat format = sound.BitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
					AL.BufferData(OpenAlBufferName, format, bytes, bytes.Length, sound.SampleRate);
					_duration = sound.Duration;
					Loaded = SoundBufferState.Loaded;
					return;
				}
			}
			Ignore = true;
		}

		/// <summary>Unloads the buffer if loaded by OpenAL</summary>
		public void Unload()
		{
			if (Loaded == SoundBufferState.Loaded)
			{
				AL.DeleteBuffers(1, ref OpenAlBufferName);
				OpenAlBufferName = 0;
				Loaded = SoundBufferState.NotLoaded;
				Ignore = false;
			}
		}
	}
}
