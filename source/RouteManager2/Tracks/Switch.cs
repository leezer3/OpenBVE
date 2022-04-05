namespace RouteManager2.Tracks
{
	/// <summary>Holds the data for a single switch</summary>
	public class Switch
	{
		/// <summary>The type of switch</summary>
		public SwitchType Type;

		/// <summary>The currently set track</summary>
		public int CurrentlySetTrack => availableTracks[setTrack];

		/// <summary>The left-hand track index</summary>
		public readonly int LeftTrack;

		private int setTrack;

		/// <summary>The list of available track indicies</summary>
		public readonly int[] availableTracks;

		/// <summary>The track position</summary>
		public readonly double TrackPosition;

		public readonly string Name;

		public Switch(int[] tracks, int initialTrack, double trackPosition, SwitchType type, string name)
		{
			Type = type;
			LeftTrack = type != SwitchType.LeftHanded ? tracks[0] : tracks[1];
			availableTracks = tracks;
			TrackPosition = trackPosition;
			for (int i = 0; i < availableTracks.Length; i++)
			{
				if (availableTracks[i] == initialTrack)
				{
					setTrack = i;
				}
			}

			Name = name;
		}

		/// <summary>Toggles the switch to the next track</summary>
		public void Toggle()
		{
			setTrack++;
			if (setTrack > availableTracks.Length - 1)
			{
				setTrack = 0;
			}
		}
	}
}
