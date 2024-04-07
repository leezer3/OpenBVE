//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, odaykufan, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//Please note that this plugin is based upon code originally released into into the public domain by Odaykufan:
//http://web.archive.org/web/20140225072517/http://odakyufan.zxq.net:80/odakyufanats/index.html

using OpenBveApi.Runtime;

namespace OpenBveAts {
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
			this.Eb = new Sound(5);
			this.LoopingSounds = new[] { this.AtsBell, this.AtsChime, this.Eb };
			// --- play once ---
			this.AtsPBell = new Sound(2);
			this.AtcBell = new Sound(2);
			this.ToAts = new Sound(3);
			this.ToAtc = new Sound(4);
			this.PlayOnceSounds = new[] { this.AtsPBell, this.AtcBell, this.ToAts, this.ToAtc };
		}

		
		// --- functions ---
		
		/// <summary>Is called every frame.</summary>
		internal void Elapse() {
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
