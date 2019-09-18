namespace OpenBveApi.Trains
{
	/// <summary>Contains a set of brightness properties to be interpolated</summary>
	public struct Brightness
	{
		/// <summary>The previous brightness to be interpolated from</summary>
		public float PreviousBrightness;
		/// <summary>The track position to interpolate from</summary>
		public double PreviousTrackPosition;
		/// <summary>The next brightness to interpolate to</summary>
		public float NextBrightness;
		/// <summary>The track position to interpolate to</summary>
		public double NextTrackPosition;
	}
}
