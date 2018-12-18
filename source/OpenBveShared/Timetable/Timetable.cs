using System;
using OpenBveApi.Objects;

namespace OpenBveShared
{
	public class Timetable
	{
		/// <summary>The currently displayed timetable</summary>
		public static TimetableState CurrentTimetable = TimetableState.None;

		/// <summary>Whether there is a custom timetable available</summary>
		public static bool CustomTimetableAvailable;

		/// <summary>Holds the animated objects for custom timetables</summary>
		public static AbstractAnimatedObject[] CustomObjects = new AbstractAnimatedObject[16];

		/// <summary>The number of custom timetable objects used</summary>
		public static int CustomObjectsUsed;

		/// <summary>Adds a custom timetable object</summary>
		/// <param name="obj">The object to add</param>
		public static void AddObjectForCustomTimetable(AbstractAnimatedObject obj) {
			if (CustomObjectsUsed >= CustomObjects.Length) {
				Array.Resize<AbstractAnimatedObject>(ref CustomObjects, CustomObjects.Length << 1);
			}
			CustomObjects[CustomObjectsUsed] = obj;
			CustomObjectsUsed++;
		}
	}
}
