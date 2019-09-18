namespace OpenBveApi.Sounds
{
	/// <summary>Represents sound raw data.</summary>
	public class RawOrigin : SoundOrigin {
		
		/// <summary>The sound raw data.</summary>
		public readonly Sound Sound;
		// --- constructors ---
		/// <summary>Creates a new raw data origin.</summary>
		/// <param name="sound">The sound raw data.</param>
		public RawOrigin(Sound sound) {
			this.Sound = sound;
		}
		
		/// <summary>Gets the sound from this origin.</summary>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether the sound could be obtained successfully.</returns>
		public override bool GetSound(out OpenBveApi.Sounds.Sound sound) {
			sound = this.Sound;
			return true;
		}
	}
}
