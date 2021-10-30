using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.Climate;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private class IncludeInfo
		{
			internal bool IsInclude;
			internal readonly string BaseDirectory;

			internal IncludeInfo(bool isInclude, string baseDirectory)
			{
				IsInclude = isInclude;
				BaseDirectory = baseDirectory;
			}
		}

		private struct Rail
		{
			internal double RailX;
			internal double RailY;
			internal double CurveCant;
			internal double RadiusH;
			internal double RadiusV;
			internal bool CurveInterpolateStart;
			internal bool CurveInterpolateEnd;
			internal bool CurveTransitionStart;
			internal bool CurveTransitionEnd;
			internal bool InterpolateX;
			internal bool InterpolateY;
		}

		private class Station
		{
			internal string Key;
			internal string Name;
			internal StationStopMode StopMode;
			internal StationType StationType;
			internal double ArrivalTime;
			internal double DepartureTime;
			internal double StopTime;
			internal double DefaultTime;
			internal bool ForceStopSignal;
			internal bool DepartureSignalUsed;
			internal double AlightingTime;
			internal double PassengerRatio;
			internal string ArrivalSoundKey;
			internal string DepartureSoundKey;
			internal double ReopenDoor;
			internal double InterferenceInDoor;
		}

		private struct FreeObj
		{
			/// <summary>The track position of the object</summary>
			internal double TrackPosition;
			/// <summary>The routefile key of the object</summary>
			internal string Key;
			/// <summary>The X position of the object (m)</summary>
			internal double X;
			/// <summary>The Y position of the object (m)</summary>
			internal double Y;
			/// <summary>The Z position of the object (m)</summary>
			internal double Z;
			/// <summary>The yaw of the object (radians)</summary>
			internal double Yaw;
			/// <summary>The pitch of the object (radians)</summary>
			internal double Pitch;
			/// <summary>The roll of the object (radians)</summary>
			internal double Roll;

			internal int Type;

			internal double Span;
		}

		private class Repeater
		{
			internal string Key;
			internal string TrackKey;
			internal double StartingDistance;
			internal bool StartRefreshed;
			internal double EndingDistance;
			internal double Interval;
			internal string[] ObjectKeys;
			/// <summary>The X position of the object (m)</summary>
			internal double X;
			/// <summary>The Y position of the object (m)</summary>
			internal double Y;
			/// <summary>The Z position of the object (m)</summary>
			internal double Z;
			/// <summary>The yaw of the object (radians)</summary>
			internal double Yaw;
			/// <summary>The pitch of the object (radians)</summary>
			internal double Pitch;
			/// <summary>The roll of the object (radians)</summary>
			internal double Roll;

			internal int Type;

			internal double Span;
		}

		private struct Crack
		{
			/// <summary>The track position of the object</summary>
			internal double TrackPosition;
			/// <summary>The routefile key of the object</summary>
			internal string Key;

			internal int PrimaryRail;
			internal int SecondaryRail;
		}

		private class Section
		{
			internal double TrackPosition;
			internal int[] Aspects;
			internal int DepartureStationIndex = -1;
		}

		private struct Signal
		{
			internal double TrackPosition;
			internal string SignalObjectKey;
			internal double X;
			internal double Y;
			internal double Z;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal int Type;
			internal double Span;
			internal int SectionIndex;
		}

		private class Transponder
		{
			internal double TrackPosition;
			internal int Type;
			internal int Data;
			internal int SectionIndex;
		}

		private struct Limit
		{
			internal double TrackPosition;
			internal double Speed;
		}

		private struct Brightness
		{
			internal double TrackPosition;
			internal float Value;
		}

		private enum SoundType
		{
			World,
			TrainStatic
		}

		private struct Sound
		{
			internal double TrackPosition;
			internal string Key;
			internal SoundType Type;
			internal double X;
			internal double Y;
		}

		private struct TrackSound
		{
			internal double TrackPosition;
			internal int SoundIndex;
		}

		private class Block
		{
			internal double StartingDistance;
			internal Rail[] Rails;
			internal TrackElement CurrentTrackState;
			internal double Turn;
			internal double Pitch;
			internal bool GradientInterpolateStart;
			internal bool GradientInterpolateEnd;
			internal bool GradientTransitionStart;
			internal bool GradientTransitionEnd;
			internal int Station = -1;
			internal int Stop = -1;
			internal List<FreeObj>[] FreeObj;
			internal List<Crack> Cracks;
			internal int Background = -1;
			internal List<Section> Sections;
			internal List<Signal>[] Signals;
			internal List<Transponder> Transponders;
			internal List<Limit> Limits;
			internal Fog Fog;
			internal bool FogDefined;
			internal List<Brightness> BrightnessChanges;
			internal double Accuracy = 2.0;
			internal bool AccuracyDefined;
			internal double AdhesionMultiplier = 1.0;
			internal bool AdhesionMultiplierDefined;
			internal List<Sound> SoundEvents;
			internal List<TrackSound> RunSounds;
			internal List<TrackSound> FlangeSounds;
			internal bool JointSound;
		}

		private class Background
		{
			internal readonly string Key;
			internal readonly BackgroundHandle Handle;

			internal Background(string key, BackgroundHandle handle)
			{
				Key = key;
				Handle = handle;
			}
		}

		private class SignalData
		{
			internal string Key;
			internal int[] Numbers;
			internal StaticObject[] BaseObjects;
			internal StaticObject[] GlowObjects;
		}

		private class OtherTrain
		{
			internal string Key;
			internal string FilePath;
			internal string TrackKey;
			internal int Direction;
			internal List<CarObject> CarObjects;
			internal List<CarSound> CarSounds;
		}

		private struct CarObject
		{
			internal string Key;
			internal double Distance;
			internal double Span;
			internal double Z;
		}

		private struct CarSound
		{
			internal string Key;
			internal double Distance1;
			internal double Distance2;
			internal string Function;
		}
	}
}
