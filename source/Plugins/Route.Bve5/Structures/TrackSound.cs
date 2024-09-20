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

namespace Route.Bve5
{
	internal class RunSound
	{
		/// <summary>The track position this sound applies from</summary>
		internal readonly double TrackPosition;
		/// <summary>The sound index to be used</summary>
		internal readonly int SoundIndex;

		internal RunSound(double trackPosition, int soundIndex)
		{
			TrackPosition = trackPosition;
			SoundIndex = soundIndex;
		}

		internal void Create(int currentElement, double startingDistance, ref int currentRunIndex, int currentFlangeIndex)
		{
			if (SoundIndex != currentRunIndex)
			{
				double d = TrackPosition - startingDistance;
				if (d > 0.0)
				{
					d = 0.0;
				}
				Plugin.CurrentRoute.Tracks[0].Elements[currentElement].Events.Add(new RailSoundsChangeEvent(d, currentRunIndex, currentFlangeIndex, SoundIndex, currentFlangeIndex));
				currentRunIndex = SoundIndex;
			}
		}
	}

	internal class FlangeSound
	{
		/// <summary>The track position this sound applies from</summary>
		internal readonly double TrackPosition;
		/// <summary>The sound index to be used</summary>
		internal readonly int SoundIndex;

		internal FlangeSound(double trackPosition, int soundIndex)
		{
			TrackPosition = trackPosition;
			SoundIndex = soundIndex;
		}

		internal void Create(int currentElement, double startingDistance, int currentRunIndex, ref int currentFlangeIndex)
		{
			if (SoundIndex != currentFlangeIndex)
			{
				double d = TrackPosition - startingDistance;
				if (d > 0.0)
				{
					d = 0.0;
				}
				Plugin.CurrentRoute.Tracks[0].Elements[currentElement].Events.Add(new RailSoundsChangeEvent(d, currentRunIndex, currentFlangeIndex, currentRunIndex, SoundIndex));
				currentFlangeIndex = SoundIndex;
			}
		}
	}
}
