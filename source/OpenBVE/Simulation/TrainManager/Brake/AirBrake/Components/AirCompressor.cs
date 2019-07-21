using SoundManager;

namespace OpenBve.BrakeSystems
{
	/// <summary>An air compressor</summary>
	class Compressor
	{
		/// <summary>Whether this compressor is currently active</summary>
		internal bool Enabled;

		/// <summary>The compression rate in Pa/s</summary>
		internal readonly double Rate;
		/// <summary>The sound played when the compressor loop starts</summary>
		internal CarSound StartSound;
		/// <summary>The sound played whilst the compressor is running</summary>
		internal CarSound LoopSound;
		/// <summary>The sound played when the compressor loop stops</summary>
		internal CarSound EndSound;
		

		internal Compressor(double rate)
		{
			Rate = rate;
			Enabled = false;
			StartSound = new CarSound();
			LoopSound = new CarSound();
			EndSound = new CarSound();
		}
	}
}
