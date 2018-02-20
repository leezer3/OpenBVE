namespace OpenBve
{
	/*
	 * This file contains definitions for the generic BVE data structures as parsed from a routefile
	 */
	internal partial class CsvRwRouteParser
	{
		private struct Rail
		{
			internal bool RailStart;
			internal bool RailStartRefreshed;
			internal double RailStartX;
			internal double RailStartY;
			internal bool RailEnd;
			internal double RailEndX;
			internal double RailEndY;
		}
		private struct WallDike
		{
			/// <summary>Whether the wall/ dike is shown for this block</summary>
			internal bool Exists;
			/// <summary>The routefile index of the object</summary>
			internal int Type;
			/// <summary>The direction the object(s) are placed in: -1 for left, 0 for both, 1 for right</summary>
			internal int Direction;
		}
		private struct FreeObj
		{
			/// <summary>The track position of the object</summary>
			internal double TrackPosition;
			/// <summary>The routefile index of the object</summary>
			internal int Type;
			/// <summary>The X position of the object (m)</summary>
			internal double X;
			/// <summary>The Y position of the object (m)</summary>
			internal double Y;
			/// <summary>The yaw of the object (radians)</summary>
			internal double Yaw;
			/// <summary>The pitch of the object (radians)</summary>
			internal double Pitch;
			/// <summary>The roll of the object (radians)</summary>
			internal double Roll;
		}
		private struct Pole
		{
			internal bool Exists;
			internal int Mode;
			internal double Location;
			internal double Interval;
			internal int Type;
		}
		private struct Form
		{
			internal int PrimaryRail;
			internal int SecondaryRail;
			internal int FormType;
			internal int RoofType;
			internal const int SecondaryRailStub = 0;
			internal const int SecondaryRailL = -1;
			internal const int SecondaryRailR = -2;
		}
		private struct Crack
		{
			internal int PrimaryRail;
			internal int SecondaryRail;
			internal int Type;
		}
		private struct Signal
		{
			internal double TrackPosition;
			internal int Section;
			internal int SignalCompatibilityObjectIndex;
			internal int SignalObjectIndex;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal bool ShowObject;
			internal bool ShowPost;
		}
		private struct Section
		{
			internal double TrackPosition;
			internal int[] Aspects;
			internal int DepartureStationIndex;
			internal bool Invisible;
			internal Game.SectionType Type;
		}
		private struct Limit
		{
			internal double TrackPosition;
			internal double Speed;
			internal int Direction;
			internal int Cource;
		}
		private struct Stop
		{
			internal double TrackPosition;
			internal int Station;
			internal int Direction;
			internal double ForwardTolerance;
			internal double BackwardTolerance;
			internal int Cars;
		}
		private struct Brightness
		{
			internal double TrackPosition;
			internal float Value;
		}

		internal struct Marker
		{
			internal double StartingPosition;
			internal double EndingPosition;
			internal MessageManager.Message Message;
		}

		internal struct StopRequest
		{
			internal int StationIndex;
			internal int MaxNumberOfCars;
			internal double TrackPosition;
			internal TrackManager.RequestStop Early;
			internal TrackManager.RequestStop OnTime;
			internal TrackManager.RequestStop Late;
			internal bool FullSpeed;
		}
		private enum SoundType { World, TrainStatic, TrainDynamic }

		private struct Sound
		{
			internal double TrackPosition;
			internal Sounds.SoundBuffer SoundBuffer;
			internal SoundType Type;
			internal double X;
			internal double Y;
			//TODO:
			//This is always set to a constant 15.0 on loading a sound, and never touched again
			//I presume Michelle intended to have sounds with different radii available
			//This would require a custom or extended command which allowed the radius value to be set
			internal double Radius;
			internal double Speed;
		}
		private struct Transponder
		{
			internal double TrackPosition;
			internal int Type;
			internal bool ShowDefaultObject;
			internal int BeaconStructureIndex;
			internal int Data;
			internal int Section;
			internal bool ClipToFirstRedSection;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct DestinationEvent
		{
			internal double TrackPosition;
			internal int Type;
			internal bool TriggerOnce;
			internal int BeaconStructureIndex;
			internal int NextDestination;
			internal int PreviousDestination;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct PointOfInterest
		{
			internal double TrackPosition;
			internal int RailIndex;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal string Text;
		}
		private struct RailCycle {
            internal int RailCycleIndex;
            internal int CurrentCycle;
        }
		private class Block
		{
			internal int Background;
			internal Brightness[] Brightness;
			internal Game.Fog Fog;
			internal bool FogDefined;
			internal int[] Cycle;
			internal RailCycle[] RailCycle;
			internal double Height;
			internal Rail[] Rail;
			internal int[] RailType;
			internal WallDike[] RailWall;
			internal WallDike[] RailDike;
			internal Pole[] RailPole;
			internal FreeObj[][] RailFreeObj;
			internal FreeObj[] GroundFreeObj;
			internal Form[] Form;
			internal Crack[] Crack;
			internal Signal[] Signal;
			internal Section[] Section;
			internal Limit[] Limit;
			internal Stop[] Stop;
			internal Sound[] Sound;
			internal Transponder[] Transponder;
			internal DestinationEvent[] DestinationChanges;
			internal PointOfInterest[] PointsOfInterest;
			internal TrackManager.TrackElement CurrentTrackState;
			internal double Pitch;
			internal double Turn;
			internal int Station;
			internal bool StationPassAlarm;
			internal double Accuracy;
			internal double AdhesionMultiplier;
		}
		private struct StructureData
		{
			internal ObjectDictionary RailObjects;
			internal ObjectManager.UnifiedObject[][] Poles;
			internal ObjectDictionary Ground;
			internal ObjectDictionary WallL;
			internal ObjectDictionary WallR;
			internal ObjectDictionary DikeL;
			internal ObjectDictionary DikeR;
			internal ObjectDictionary FormL;
			internal ObjectDictionary FormR;
			internal ObjectDictionary FormCL;
			internal ObjectDictionary FormCR;
			internal ObjectDictionary RoofL;
			internal ObjectDictionary RoofR;
			internal ObjectDictionary RoofCL;
			internal ObjectDictionary RoofCR;
			internal ObjectDictionary CrackL;
			internal ObjectDictionary CrackR;
			internal ObjectDictionary FreeObjects;
			internal ObjectDictionary Beacon;
			internal int[][] Cycle;
			internal int[][] RailCycle;
			internal int[] Run;
			internal int[] Flange;
		}
	}
}
