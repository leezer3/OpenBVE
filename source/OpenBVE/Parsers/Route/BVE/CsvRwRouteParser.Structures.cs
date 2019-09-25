﻿using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2.Climate;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using SoundManager;

namespace OpenBve
{
	/*
	 * This file contains definitions for the generic BVE data structures as parsed from a routefile
	 */
	internal partial class CsvRwRouteParser
	{
		private struct Rail
		{
			internal bool RailStarted;
			internal bool RailStartRefreshed;
			internal Vector2 RailStart;
			internal bool RailEnded;
			internal Vector2 RailEnd;
			internal double CurveCant;
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
			/// <summary>The position of the object</summary>
			internal Vector2 Position;
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
			internal int SectionIndex;
			internal int SignalCompatibilityObjectIndex;
			internal int SignalObjectIndex;
			internal Vector2 Position;
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
			internal SectionType Type;
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
			internal AbstractMessage Message;
		}

		internal struct StopRequest
		{
			internal int StationIndex;
			internal int MaxNumberOfCars;
			internal double TrackPosition;
			internal RequestStop Early;
			internal RequestStop OnTime;
			internal RequestStop Late;
			internal bool FullSpeed;
		}
		private enum SoundType { World, TrainStatic, TrainDynamic }

		private struct Sound
		{
			internal double TrackPosition;
			internal SoundBuffer SoundBuffer;
			internal SoundType Type;
			internal Vector2 Position;
			//TODO:
			//This is always set to a constant 15.0 on loading a sound, and never touched again
			//I presume Michelle intended to have sounds with different radii available
			//This would require a custom or extended command which allowed the radius value to be set
#pragma warning disable 414
			internal double Radius;
#pragma warning restore 414
			internal double Speed;
			internal bool IsMicSound;
			internal double ForwardTolerance;
			internal double BackwardTolerance;
		}
		private struct Transponder
		{
			internal double TrackPosition;
			internal int Type;
			internal bool ShowDefaultObject;
			internal int BeaconStructureIndex;
			internal int Data;
			internal int SectionIndex;
			internal bool ClipToFirstRedSection;
			internal Vector2 Position;
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
			internal Vector2 Position;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct PointOfInterest
		{
			internal double TrackPosition;
			internal int RailIndex;
			internal Vector2 Position;
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
			internal Brightness[] BrightnessChanges;
			internal Fog Fog;
			internal bool FogDefined;
			internal int[] Cycle;
			internal RailCycle[] RailCycles;
			internal double Height;
			internal Rail[] Rails;
			internal int[] RailType;
			internal WallDike[] RailWall;
			internal WallDike[] RailDike;
			internal Pole[] RailPole;
			internal FreeObj[][] RailFreeObj;
			internal FreeObj[] GroundFreeObj;
			internal Form[] Forms;
			internal Crack[] Cracks;
			internal Signal[] Signals;
			internal Section[] Sections;
			internal Limit[] Limits;
			internal Stop[] StopPositions;
			internal Sound[] SoundEvents;
			internal Transponder[] Transponders;
			internal DestinationEvent[] DestinationChanges;
			internal PointOfInterest[] PointsOfInterest;
			internal TrackElement CurrentTrackState;
			internal double Pitch;
			internal double Turn;
			internal int Station;
			internal bool StationPassAlarm;
			internal double Accuracy;
			internal double AdhesionMultiplier;
		}

		/// <summary>Holds the base structures for a route: These are cloned and transformed for final world placement</summary>
		private struct StructureData
		{
			/// <summary>All currently defined Structure.Rail objects</summary>
			internal ObjectDictionary RailObjects;
			/// <summary>All currently defined Structure.Pole objects</summary>
			internal PoleDictionary Poles;
			/// <summary>All currently defined Structure.Ground objects</summary>
			internal ObjectDictionary Ground;
			/// <summary>All currently defined Structure.WallL objects</summary>
			internal ObjectDictionary WallL;
			/// <summary>All currently defined Structure.WallR objects</summary>
			internal ObjectDictionary WallR;
			/// <summary>All currently defined Structure.DikeL objects</summary>
			internal ObjectDictionary DikeL;
			/// <summary>All currently defined Structure.DikeR objects</summary>
			internal ObjectDictionary DikeR;
			/// <summary>All currently defined Structure.FormL objects</summary>
			internal ObjectDictionary FormL;
			/// <summary>All currently defined Structure.FormR objects</summary>
			internal ObjectDictionary FormR;
			/// <summary>All currently defined Structure.FormCL objects</summary>
			internal ObjectDictionary FormCL;
			/// <summary>All currently defined Structure.FormCR objects</summary>
			internal ObjectDictionary FormCR;
			/// <summary>All currently defined Structure.RoofL objects</summary>
			internal ObjectDictionary RoofL;
			/// <summary>All currently defined Structure.RoofR objects</summary>
			internal ObjectDictionary RoofR;
			/// <summary>All currently defined Structure.RoofCL objects</summary>
			internal ObjectDictionary RoofCL;
			/// <summary>All currently defined Structure.RoofCR objects</summary>
			internal ObjectDictionary RoofCR;
			/// <summary>All currently defined Structure.CrackL objects</summary>
			internal ObjectDictionary CrackL;
			/// <summary>All currently defined Structure.CrackR objects</summary>
			internal ObjectDictionary CrackR;
			/// <summary>All currently defined Structure.FreeObj objects</summary>
			internal ObjectDictionary FreeObjects;
			/// <summary>All currently defined Structure.Beacon objects</summary>
			internal ObjectDictionary Beacon;
			/// <summary>All currently defined cycles</summary>
			internal int[][] Cycles;
			/// <summary>All currently defined RailCycles</summary>
			internal int[][] RailCycles;
			/// <summary>The Run sound index to be played for each railtype idx</summary>
			internal int[] Run;
			/// <summary>The flange sound index to be played for each railtype idx</summary>
			internal int[] Flange;
		}
	}
}
