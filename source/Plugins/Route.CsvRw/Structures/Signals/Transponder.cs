using OpenBveApi.Math;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class Transponder
	{
		/// <summary>The track position at which the transponder is placed</summary>
		internal readonly double TrackPosition;
		/// <summary>The type of transponder</summary>
		/// <remarks>Commonly set to frequency in hz</remarks>
		internal int Type; //Unable to set as readonly as may be changed in postprocessing
		/// <summary>The index of the beacon object to display</summary>
		/// <remarks>Special values:
		/// -2 : Display compatability object for beacon type
		/// -1 : Display no object</remarks>
		internal readonly int BeaconStructureIndex;
		/// <summary>An optional data value to be transmitted by the beacon to any recievers passing over it</summary>
		internal readonly int Data;
		/// <summary>The index of the section to which the beacon refers</summary>
		internal readonly int SectionIndex;
		/// <summary>Whether the signal data returned by the beacon clips to the first red section, or returns all possible sections</summary>
		internal readonly bool ClipToFirstRedSection;
		/// <summary>The position of the beacon object if displayed</summary>
		internal readonly Vector2 Position;
		/// <summary>The yaw of the beacon object if displayed</summary>
		internal readonly double Yaw;
		/// <summary>The pitch of the beacon object if displayed</summary>
		internal readonly double Pitch;
		/// <summary>The roll of the beacon object if displayed</summary>
		internal readonly double Roll;

		internal Transponder(double trackPosition, int type, int data, Vector2 position, int sectionIndex, int beaconStructureIndex = -1, bool clipToFirstRedSection = true, double yaw = 0, double pitch = 0, double roll = 0)
		{
			TrackPosition = trackPosition;
			Type = type;
			Data = data;
			Position = position;
			SectionIndex = sectionIndex;
			ClipToFirstRedSection = clipToFirstRedSection;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			BeaconStructureIndex = beaconStructureIndex;
		}

		internal Transponder(double trackPosition, TransponderTypes type, int data)
		{
			TrackPosition = trackPosition;
			Type = (int)type;
			Data = data;
			Position = new Vector2();
			SectionIndex = -1;
			ClipToFirstRedSection = false;
			Yaw = 0;
			Pitch = 0;
			Roll = 0;
			BeaconStructureIndex = -1;
		}
	}
}
