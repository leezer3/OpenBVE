namespace OpenBveShared
{
	/// <summary>The available types of timetable to display</summary>
	public enum TimetableState
	{
		/// <summary>No timetable</summary>
		None = 0,
		/// <summary>The custom timetable set via Route.Timetable</summary>
		Custom = 1,
		/// <summary>The default auto-generated timetable</summary>
		Default = 2
	}
}
