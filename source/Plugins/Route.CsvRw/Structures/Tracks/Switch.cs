namespace CsvRwRouteParser
{
	internal class Switch
	{
		/// <summary>The second track index</summary>
		internal int SecondTrack;
		/// <summary>The initial setting of the switch</summary>
		internal int InitialSetting;
		/// <summary>Whether this is a trailing switch</summary>
		internal bool Trailing = false;
		/// <summary>Whether this is a spring return switch</summary>
		internal bool SpringReturn = false;
		/// <summary>The textual name for the switch</summary>
		internal string Name;
		/// <summary>The track names</summary>
		internal string[] TrackNames;
		/// <summary>Whether the switch has a fixed route</summary>
		internal bool FixedRoute;
	}
}
