namespace OpenBveApi.Objects
{
	/// <summary>A helper struct describing the world properties of an object</summary>
    public struct WorldProperties
    {
		/// <summary>The key of the rail on which the object is placed</summary>
		public int RailKey;
		/// <summary>The absolute track position at which the object is placed</summary>
		public double TrackPosition;
		/// <summary>The track distance at which this is displayed by the renderer</summary>
		public double StartingDistance;
		/// <summary>The ending distance at which this is hidden by the renderer</summary>
		public double EndingDistance;
		/// <summary>The brightness value of this object</summary>
		public double Brightness;
		/// <summary>The index of the signalling section this object is attached to</summary>
		public int SectionIndex;

		/// <summary>Creates a new instance of this struct</summary>
		public WorldProperties(int railKey, double trackPosition, double startingDistance, double endingDistance, int sectionIndex = -1, double brightness = 1.0)
		{
			RailKey = railKey;
			TrackPosition = trackPosition;
			StartingDistance = startingDistance;
			EndingDistance = endingDistance;
			Brightness = brightness;
			SectionIndex = sectionIndex;
		}

		/// <summary>Creates a new instance of this struct</summary>
		public WorldProperties(double trackPosition, int sectionIndex, double brightness)
		{
			RailKey = 0;
			TrackPosition = trackPosition;
			SectionIndex = sectionIndex;
			Brightness = brightness;
			// Mechanik visibility- not used
			StartingDistance = 0; 
			EndingDistance = 0;
		}
    }
}
