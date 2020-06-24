using OpenBveApi.Math;

namespace CsvRwRouteParser
{
	internal class PointOfInterest
	{
		/// <summary>The track position at which the PointOfInterest is placed</summary>
		internal readonly double TrackPosition;
		/// <summary>The rail index to which the PointOfInterest is attached</summary>
		internal readonly int RailIndex;
		/// <summary>The relative position to the rail</summary>
		internal readonly Vector2 Position;
		/// <summary>The yaw value</summary>
		internal readonly double Yaw;
		/// <summary>The pitch value</summary>
		internal readonly double Pitch;
		/// <summary>The roll value</summary>
		internal readonly double Roll;
		/// <summary>The text to display when jumping to the PointOfInterest</summary>
		/// <remarks>Typically station name etc.</remarks>
		internal readonly string Text;

		internal PointOfInterest(double trackPosition, int railIndex, string text, Vector2 position, double yaw, double pitch, double roll)
		{
			TrackPosition = trackPosition;
			RailIndex = railIndex;
			Text = text;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
		}
	}
}
