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

using RouteManager2.Events;
using System.Linq;

namespace Route.Bve5
{
	internal class Brightness
	{
		/// <summary>The track position of the brightness value</summary>
		internal readonly double TrackPosition;
		/// <summary>The absolute brightness value</summary>
		/// <remarks>Interpolation will be done between adjoining values</remarks>
		internal readonly float Value;

		internal Brightness(double trackPosition, float value)
		{
			TrackPosition = trackPosition;
			Value = value;
		}

		internal void Create(int currentElement, double startingDistance, ref int CurrentBrightnessElement, ref int CurrentBrightnessEvent, ref float CurrentBrightnessValue, ref double CurrentBrightnessTrackPosition)
		{
			for (int i = 0; i < Plugin.CurrentRoute.Tracks.Count; i++)
			{
				int k = Plugin.CurrentRoute.Tracks.ElementAt(i).Key;
				double d = TrackPosition - startingDistance;
				Plugin.CurrentRoute.Tracks[k].Elements[currentElement].Events.Add(new BrightnessChangeEvent(d, Value, CurrentBrightnessValue, TrackPosition - CurrentBrightnessTrackPosition));
				if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
				{
					BrightnessChangeEvent bce = (BrightnessChangeEvent)Plugin.CurrentRoute.Tracks[0].Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
					bce.NextBrightness = Value;
					bce.NextDistance = TrackPosition - CurrentBrightnessTrackPosition;
				}
			}
			CurrentBrightnessElement = currentElement;
			CurrentBrightnessEvent = Plugin.CurrentRoute.Tracks[0].Elements[currentElement].Events.Count - 1;
			CurrentBrightnessValue = Value;
			CurrentBrightnessTrackPosition = TrackPosition;
		}
	}
}
