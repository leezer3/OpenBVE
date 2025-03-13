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
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2.Climate;
using TrainManager.Motor;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		internal class Block
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
			internal Dictionary<string, List<FreeObj>> FreeObjects;
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
			internal List<RunSound> RunSounds;
			internal List<FlangeSound> FlangeSounds;
			internal bool JointSound;
		}

		internal class SignalData
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
			internal readonly BVE5AISoundControl Function;

			internal CarSound(string key, double distance1, double distance2, BVE5AISoundControl function)
			{
				Key = key;
				Distance1 = distance1;
				Distance2 = distance2;
				Function = function;
			}
		}
	}
}
