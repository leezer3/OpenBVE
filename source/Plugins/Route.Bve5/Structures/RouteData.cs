using OpenBveApi.Textures;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	internal struct RouteData
	{
		internal double TrackPosition;
		internal double BlockInterval;
		/// <summary>OpenBVE runs internally in meters per second
		/// This value is used to convert between the speed set by Options.UnitsOfSpeed and m/s
		/// </summary>
		internal double UnitOfSpeed;
		/// <summary>If this bool is set to FALSE, then objects will disappear when the block in which their command is placed is exited via by the camera
		/// Certain BVE2/4 era routes used this as an animation trick
		/// </summary>
		internal bool AccurateObjectDisposal;
		internal bool SignedCant;
		internal bool FogTransitionMode;
		internal StructureData Structure;
		internal SignalObject[] SignalData;
		internal CompatibilitySignalData[] CompatibilitySignalData;
		internal Texture[] TimetableDaytime;
		internal Texture[] TimetableNighttime;
		internal Background[] Backgrounds;
		internal double[] SignalSpeeds;
		internal Block[] Blocks;
		//internal Marker[] Markers;
		internal int FirstUsedBlock;

		internal ObjectPointer[] ObjectList;
		internal Station[] StationList;

		internal int UsedObjects;
		internal float LastBrightness;
	}
}
