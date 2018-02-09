using OpenBveApi.Runtime;

namespace Plugin {
	internal class Sounds {
		
		// --- classes ---
		
		/// <summary>Represents a looping sound.</summary>
		internal class Sound {
			internal readonly int Index;
			internal SoundHandle Handle;
			internal bool IsToBePlayed;
			internal Sound(int index) {
				this.Index = index;
				this.Handle = null;
			}
			internal void Play() {
				this.IsToBePlayed = true;
			}
		}
		
		
		// --- members ---
		
		private readonly PlaySoundDelegate PlaySound;
		
		
		// --- looping sounds ---
		
		internal readonly Sound AtsBell;

		internal readonly Sound AtsChime;
		
		internal readonly Sound Eb;

		private readonly Sound[] LoopingSounds;
		
		
		// --- play once sounds ---
		
		internal readonly Sound AtsPBell;
		
		internal readonly Sound AtcBell;
		
		internal readonly Sound ToAts;
		
		internal readonly Sound ToAtc;
		
		private readonly Sound[] PlayOnceSounds;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of sounds.</summary>
		/// <param name="playSound">The delegate to the function to play sounds.</param>
		internal Sounds(PlaySoundDelegate playSound) {
			this.PlaySound = playSound;
			// --- looping ---
			this.AtsBell = new Sound(0);
			this.AtsChime = new Sound(1);
			this.Eb = new Sounds.Sound(5);
			this.LoopingSounds = new Sound[] { this.AtsBell, this.AtsChime, this.Eb };
			// --- play once ---
			this.AtsPBell = new Sound(2);
			this.AtcBell = new Sound(2);
			this.ToAts = new Sound(3);
			this.ToAtc = new Sound(4);
			this.PlayOnceSounds = new Sound[] { this.AtsPBell, this.AtcBell, this.ToAts, this.ToAtc };
		}

		
		// --- functions ---
		
		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		internal void Elapse(ElapseData data) {
			foreach (Sound sound in this.LoopingSounds) {
				if (sound.IsToBePlayed) {
					if (sound.Handle == null || sound.Handle.Stopped) {
						sound.Handle = PlaySound(sound.Index, 1.0, 1.0, true);
					}
				} else {
					if (sound.Handle != null && sound.Handle.Playing) {
						sound.Handle.Stop();
					}
				}
				sound.IsToBePlayed = false;
			}
			foreach (Sound sound in this.PlayOnceSounds) {
				if (sound.IsToBePlayed) {
					PlaySound(sound.Index, 1.0, 1.0, false);
					sound.IsToBePlayed = false;
				}
			}
		}
		
	}
}