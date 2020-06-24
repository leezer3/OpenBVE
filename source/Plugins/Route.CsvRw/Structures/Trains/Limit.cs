namespace CsvRwRouteParser
{
	internal class Limit
	{
		/// <summary>The track position at which the limit is placed</summary>
		internal readonly double TrackPosition;
		/// <summary>The speed limit to be enforced</summary>
		/// <remarks>Stored in km/h, has been transformed by UnitOfSpeed if appropriate</remarks>
		internal readonly double Speed;
		/// <summary>The side of the auto-generated speed limit post</summary>
		internal readonly int Direction;
		/// <summary>The cource (little arrow) on the speed limit post denoting a diverging JA limit</summary>
		internal readonly int Cource;

		internal Limit(double trackPosition, double speed, int direction, int cource)
		{
			TrackPosition = trackPosition;
			Speed = speed;
			Direction = direction;
			Cource = cource;
		}
	}
}
