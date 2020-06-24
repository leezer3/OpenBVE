namespace CsvRwRouteParser
{
	internal class Brightness
	{
		/// <summary>The track position for the brightness change</summary>
		internal readonly double TrackPosition;
		/// <summary>The new brightness value</summary>
		internal readonly float Value;

		internal Brightness(double trackPosition, float value)
		{
			TrackPosition = trackPosition;
			Value = value;
		}
	}
}
