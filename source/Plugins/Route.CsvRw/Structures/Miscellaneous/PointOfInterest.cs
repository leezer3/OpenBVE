using OpenBveApi.Math;

namespace CsvRwRouteParser
{
	internal class PointOfInterest : AbstractStructure
	{
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

		internal PointOfInterest(double trackPosition, int railIndex, string text, Vector2 position, double yaw, double pitch, double roll) : base(trackPosition, railIndex)
		{
			Text = text;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
		}
	}
}
