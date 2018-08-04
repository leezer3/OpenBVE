namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal partial class Car
		{
			internal struct CarBrightness
			{
				internal float PreviousBrightness;
				internal double PreviousTrackPosition;
				internal float NextBrightness;
				internal double NextTrackPosition;
			}
		}
	}
}
