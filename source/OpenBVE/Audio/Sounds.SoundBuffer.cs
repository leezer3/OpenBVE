using OpenBveApi.Sounds;

namespace OpenBve {
	internal static partial class Sounds {
		
		/// <summary>Represents a sound buffer.</summary>
		internal class SoundBuffer : SoundHandle {
			// --- members ---
			/// <summary>The origin where the sound can be loaded from.</summary>
			internal SoundOrigin Origin;
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
			// --- constructors ---
			internal SoundBuffer(string path, double radius) {
				this.Origin = new PathOrigin(path);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.Ignore = false;
			}
			internal SoundBuffer(OpenBveApi.Sounds.Sound sound, double radius) {
				this.Origin = new RawOrigin(sound);
				this.Radius = radius;
				this.Loaded = false;
				this.OpenAlBufferName = 0;
				this.Duration = 0.0;
				this.Ignore = false;
			}
		}
		
	}
}