using OpenBveApi.Sounds;

namespace OpenBve {
	internal static partial class Sounds {
		
		/// <summary>Represents a sound buffer.</summary>
		internal class SoundBuffer : SoundHandle {
			// --- members ---
			/// <summary>The origin where the sound can be loaded from.</summary>
			internal readonly SoundOrigin Origin;
			/// <summary>The default effective radius.</summary>
			internal double Radius;
			/// <summary>Whether the sound is loaded and the OpenAL sound name is valid.</summary>
			internal bool Loaded;
			/// <summary>The OpenAL sound name. Only valid if the sound is loaded.</summary>
			internal int OpenAlBufferName;
			/// <summary>The duration of the sound in seconds. Only valid if the sound is loaded.</summary>
			internal double Duration;
			/// <summary>Whether to ignore further attemps to load the sound after previous attempts have failed.</summary>
			internal bool Ignore;
			/// <summary>The function script controlling this sound's volume.</summary>
			internal FunctionScripts.FunctionScript PitchFunction;
			/// <summary>The function script controlling this sound's volume.</summary>
			internal FunctionScripts.FunctionScript VolumeFunction;

			internal double InternalVolumeFactor;

			/// <summary>Creates a new sound buffer</summary>
			/// <param name="path">The on-disk path to the sound to load</param>
			/// <param name="radius">The radius for this sound</param>
			internal SoundBuffer(string path, double radius) {
				this.Origin = new PathOrigin(path);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.InternalVolumeFactor = 0.5;
				this.Ignore = false;
				this.PitchFunction = null;
				this.VolumeFunction = null;
				
			}

			/// <summary>Creates a new sound buffer</summary>
			/// <param name="sound">The raw sound source, loaded via an API plugin</param>
			/// <param name="radius">The radius of the sound</param>
			internal SoundBuffer(OpenBveApi.Sounds.Sound sound, double radius) {
				this.Origin = new RawOrigin(sound);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.InternalVolumeFactor = 0.5;
				this.Ignore = false;
				this.PitchFunction = null;
				this.VolumeFunction = null;
			}
			/// <summary>Creates a new uninitialized sound buffer</summary>
			internal SoundBuffer()
			{
				this.Origin = null;
				this.Radius = 0.0;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.InternalVolumeFactor = 0.5;
				this.Ignore = false;
				this.PitchFunction = null;
				this.VolumeFunction = null;
			}

			internal SoundBuffer(SoundOrigin origin)
			{
				this.Origin = origin;
				this.Radius = 0.0;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.InternalVolumeFactor = 0.5;
				this.Ignore = false;
				this.PitchFunction = null;
				this.VolumeFunction = null;
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

			/// <summary>Attempts to load a new sound buffer</summary>
			/// <param name="FileName">The on-disk path to the sound</param>
			/// <param name="radius">The radius of the sound</param>
			/// <returns>The new sound buffer OR null if the call does not succeed</returns>
			internal static SoundBuffer TryToLoad(string FileName, double radius)
			{
				if (FileName != null)
				{
					if (System.IO.File.Exists(FileName))
					{
						try
						{
							return RegisterBuffer(FileName, radius);
						}
						catch
						{
							return null;
						}
					}
				}
				return null;
			}
		}
		
	}
}
