using System.Collections.Generic;
using OpenBveApi.Textures;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	internal struct RouteData
	{
		internal double TrackPosition;
		internal double BlockInterval;
		internal bool SignedCant;
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

		internal List<ObjectPointer> SoundList;
		internal List<ObjectPointer> Sound3DList;
		internal ObjectPointer[] ObjectList;
		internal Station[] StationList;

		internal int UsedObjects;
		internal int UsedSounds;
		internal int UsedSounds3D;
		internal float LastBrightness;
	}
}
