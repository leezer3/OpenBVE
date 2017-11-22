using OpenBveApi.Sounds;

namespace OpenBve
{
	internal static partial class Sounds
	{

		/// <summary>Represents a sound buffer.</summary>
		internal class SoundBuffer : SoundHandle
		{
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

			/// <summary>Creates a new sound buffer</summary>
			/// <param name="path">The on-disk path to the sound</param>
			/// <param name="radius">The radius of the sound</param>
			internal SoundBuffer(string path, double radius)
			{
				this.Origin = new PathOrigin(path);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.Ignore = false;
			}

			/// <summary>Creates a new sound buffer</summary>
			/// <param name="sound">The raw sound source, loaded via an API plugin</param>
			/// <param name="radius">The radius of the sound</param>
			internal SoundBuffer(OpenBveApi.Sounds.Sound sound, double radius)
			{
				this.Origin = new RawOrigin(sound);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.Ignore = false;
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
