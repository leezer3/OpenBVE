using System;
using OpenBveApi.Runtime;

namespace Plugin {
	public partial class Plugin : IRuntime {

		/// <summary>Provides constants related to sound instructions.</summary>
		internal static class SoundInstructions {
			// constants
			/// <summary>Instructs the sound to stop. The numerical value of this constant is -10000.</summary>
			public const int Stop = -10000;
			/// <summary>Instructs the sound to play in a loop. The numerical value of this constant is 0.</summary>
			public const int PlayLooping = 0;
			/// <summary>Instructs the sound to play once. The numerical value of this constant is 1.</summary>
			public const int PlayOnce = 1;
			/// <summary>Instructs the sound to continue. The numerical value of this constant is 2.</summary>
			public const int Continue = 2;
		}

		/// <summary>Represents the helper class that provides sound instructions in a way similar to Win32 plugins.</summary>
		internal class SoundHelper {
			// members
			/// <summary>The array of sound instructions as modified since the last call to Elapse.</summary>
			private int[] Sound;
			/// <summary>The array of sound instructions as they were in the last call to Elapse.</summary>
			private int[] LastSound;
			/// <summary>The array of sound handles as obtained from the host application.</summary>
			private SoundHandle[] Handles;
			/// <summary>The callback function for playing sounds.</summary>
			private PlaySoundDelegate PlaySound;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="playSound">The callback function for playing sounds.</param>
			/// <param name="count">The amount of sound instructions to use. This is 256 in Win32 plugins but can be a different value if needed.</param>
			internal SoundHelper(PlaySoundDelegate playSound, int count) {
				this.Sound = new int[count];
				this.LastSound = new int[count];
				this.Handles = new SoundHandle[count];
				this.PlaySound = playSound;
			}
			// indexer
			/// <summary>Gets or sets the specified sound instruction.</summary>
			internal int this[int index] {
				get {
					return this.Sound[index];
				}
				set {
					this.Sound[index] = value;
				}
			}
			// properties
			/// <summary>Gets the length of the sound instruction array.</summary>
			internal int Length {
				get {
					return this.Sound.Length;
				}
			}
			// functions
			/// <summary>Updates the sound instructions. Should be called at the end of every call to Elapse.</summary>
			internal void Update() {
				for (int i = 0; i < this.Sound.Length; i++) {
					if (this.Sound[i] != this.LastSound[i]) {
						if (this.Sound[i] == SoundInstructions.Stop) {
							if (this.Handles[i] != null) {
								this.Handles[i].Stop();
								this.Handles[i] = null;
							}
						} else if (this.Sound[i] > SoundInstructions.Stop & this.Sound[i] <= SoundInstructions.PlayLooping) {
							double volume = (double)(this.Sound[i] - SoundInstructions.Stop) / (double)(SoundInstructions.PlayLooping - SoundInstructions.Stop);
							if (this.Handles[i] != null && this.Handles[i].Playing) {
								this.Handles[i].Volume = volume;
							} else {
								this.Handles[i] = this.PlaySound(i, volume, 1.0, true);
							}
						} else if (this.Sound[i] == SoundInstructions.PlayOnce) {
							if (this.Handles[i] != null) {
								this.Handles[i].Stop();
							}
							this.Handles[i] = this.PlaySound(i, 1.0, 1.0, false);
							this.Sound[i] = SoundInstructions.Continue;
						}
						this.LastSound[i] = this.Sound[i];
					}
				}
			}
		}
		
		/// <summary>The sound class that works just like the sound array in Win32 plugins.</summary>
		/// <remarks>Be sure to call the constructor in the Load call and the Update() function at the end of every Elapse call.</remarks>
		internal SoundHelper Sound = null;
		
	}
}