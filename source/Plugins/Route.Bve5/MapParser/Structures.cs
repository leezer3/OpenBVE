//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.Climate;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private class Rail
		{
			internal Vector2 Position;
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

		private class FreeObj : AbstractStructure
		{
			/// <summary>The yaw of the object (radians)</summary>
			internal readonly double Yaw;
			/// <summary>The pitch of the object (radians)</summary>
			internal readonly double Pitch;
			/// <summary>The roll of the object (radians)</summary>
			internal readonly double Roll;
			
			internal FreeObj(double trackPosition, string key, Vector3 position, double yaw, double pitch, double roll, ObjectTransformType type, double span) : base(trackPosition, key, type, span, position)
			{
				Yaw= yaw;
				Pitch = pitch;
				Roll = roll;
			}
		}

		private class Repeater
		{
			internal readonly string Key;
			internal string TrackKey;
			internal double StartingDistance;
			internal bool StartRefreshed;
			internal double EndingDistance;
			internal double Interval;
			internal string[] ObjectKeys;
			internal Vector3 Position;
			/// <summary>The yaw of the object (radians)</summary>
			internal double Yaw;
			/// <summary>The pitch of the object (radians)</summary>
			internal double Pitch;
			/// <summary>The roll of the object (radians)</summary>
			internal double Roll;

			internal ObjectTransformType Type;

			internal double Span;

			internal Repeater(string key)
			{
				Key = key;
			}
		}

		private class Crack : AbstractStructure
		{
			internal readonly string PrimaryRail;
			internal readonly string SecondaryRail;

			internal Crack(string key, double trackPosition, string primaryRail, string secondaryRail) : base(trackPosition, key, ObjectTransformType.FollowsGradient, 0, Vector3.Zero)
			{
				PrimaryRail = primaryRail;
				SecondaryRail = secondaryRail;
			}
		}

		private class Section
		{
			internal double TrackPosition;
			internal int[] Aspects;
			internal int DepartureStationIndex = -1;
		}

		private class Signal : AbstractStructure
		{
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal int SectionIndex;

			internal Signal(string key, double trackPosition, ObjectTransformType type, double span, Vector3 position) : base(trackPosition, key, type, span, position)
			{

			}
		}

		private class Transponder
		{
			internal double TrackPosition;
			internal int Type;
			internal int Data;
			internal int SectionIndex;
		}

		private class Limit
		{
			internal readonly double TrackPosition;
			internal readonly double Speed;

			internal Limit(double trackPosition, double speed)
			{
				TrackPosition = trackPosition;
				Speed = speed;
			}

		}

		private class Brightness
		{
			internal readonly double TrackPosition;
			internal readonly float Value;

			internal Brightness(double trackPosition, float value)
			{
				TrackPosition = trackPosition;
				Value = value;
			}
		}

		private enum SoundType
		{
			World,
			TrainStatic
		}

		private class Sound
		{
			internal readonly double TrackPosition;
			internal readonly string Key;
			internal readonly SoundType Type;
			internal readonly double X;
			internal readonly double Y;

			internal Sound(double trackPosition, string key, SoundType type, double x = 0, double y = 0)
			{
				TrackPosition = trackPosition;
				Key = key;
				Type = type;
				X = x;
				Y = y;
			}
		}

		private struct TrackSound
		{
			internal double TrackPosition;
			internal int SoundIndex;
		}

		private class Block
		{
			internal double StartingDistance;
			internal Dictionary<string, Rail> Rails;
			internal TrackElement CurrentTrackState;
			internal double Turn;
			internal double Pitch;
			internal bool GradientInterpolateStart;
			internal bool GradientInterpolateEnd;
			internal bool GradientTransitionStart;
			internal bool GradientTransitionEnd;
			internal int StationIndex = -1;
			internal int Stop = -1;
			internal Dictionary<string, List<FreeObj>> FreeObj;
			internal List<Crack> Cracks;
			internal string Background = string.Empty;
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
		
		private class SignalData
		{
			internal string Key;
			internal int[] Numbers;
			internal StaticObject[] BaseObjects;
			internal StaticObject[] GlowObjects;
		}

		private class ScriptedTrain
		{
			internal string Key;
			internal string FilePath;
			internal string TrackKey;
			internal int Direction;
			internal List<CarObject> CarObjects;
			internal List<CarSound> CarSounds;
		}

		private class CarObject
		{
			internal readonly string Key;
			internal readonly double Distance;
			internal readonly double Span;
			internal readonly double Z;

			internal CarObject(string key, double distance, double span, double z)
			{
				Key = key;
				Distance= distance;
				Span= span;
				Z= z;
			}
		}

		private class CarSound
		{
			internal readonly string Key;
			internal readonly double Distance1;
			internal readonly double Distance2;
			internal readonly string Function;

			internal CarSound(string key, double distance1, double distance2, string function)
			{
				Key = key;
				Distance1 = distance1;
				Distance2 = distance2;
				Function = function;
			}
		}
	}
}
