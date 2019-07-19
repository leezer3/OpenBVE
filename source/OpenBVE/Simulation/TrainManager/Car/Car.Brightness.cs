namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal partial class Car
		{
			/// <summary>Contains the brightness properties</summary>
			internal struct CarBrightness
			{
				/// <summary>The previous brightness to be interpolated from</summary>
				internal float PreviousBrightness;
				/// <summary>The track position to interpolate from</summary>
				internal double PreviousTrackPosition;
				/// <summary>The next brightness to interpolate to</summary>
				internal float NextBrightness;
				/// <summary>The track position to interpolate to</summary>
				internal double NextTrackPosition;
			}
		}
	}
}
