namespace OpenBve.BrakeSystems
{
	/// <summary>An air compressor</summary>
	class Compressor
	{
		/// <summary>Whether this compressor is currently active</summary>
		internal bool Enabled;

		/// <summary>The compression rate in Pa/s</summary>
		internal readonly double Rate;

		internal Compressor(double rate)
		{
			Rate = rate;
			Enabled = false;
		}
	}
}
