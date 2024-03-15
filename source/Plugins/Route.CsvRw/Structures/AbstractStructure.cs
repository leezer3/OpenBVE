namespace CsvRwRouteParser
{
	/// <summary>An abstract class for structures to be placed</summary>
	internal abstract class AbstractStructure
	{
		/// <summary>The track position</summary>
		internal readonly double TrackPosition;
		/// <summary>The rail index</summary>
		internal readonly int RailIndex;

		protected AbstractStructure(double trackPosition, int railIndex = 0)
		{
			TrackPosition = trackPosition;
			RailIndex = railIndex;
		}
	}
}
