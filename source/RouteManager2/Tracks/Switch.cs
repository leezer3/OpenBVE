namespace RouteManager2.Tracks
{
	/// <summary>Holds the data for a single switch</summary>
	public class Switch
	{
		/// <summary>The currently set track</summary>
		public int currentlySetTrack()
		{
			return availableTracks[setTrack];
		}

		private int setTrack;

		/// <summary>The list of available track indicies</summary>
		public readonly int[] availableTracks;

		public Switch(int[] tracks, int initialTrack)
		{
			availableTracks = tracks;
			for (int i = 0; i < availableTracks.Length; i++)
			{
				if (availableTracks[i] == initialTrack)
				{
					setTrack = i;
				}
			}
		}
	}
}
