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

using OpenBveApi.Trains;
using RouteManager2.Events;
using RouteManager2.SignalManager;
using System;

namespace Route.Bve5
{
	internal class Section
	{
		/// <summary>The track position of the section</summary>
		internal readonly double TrackPosition;
		/// <summary>The aspects displayed by the station</summary>
		internal readonly int[] Aspects;
		/// <summary>The station index (if departure controlled)</summary>
		internal int DepartureStationIndex = -1;

		internal Section(double trackPosition, int[] aspects)
		{
			TrackPosition = trackPosition;
			Aspects = aspects;
		}

		internal void Create(RouteData Data, int currentElement, double startingDistance)
		{
			int m = Plugin.CurrentRoute.Sections.Length;
			Array.Resize(ref Plugin.CurrentRoute.Sections, m + 1);
			RouteManager2.SignalManager.Section previousSection = null;
			if (m > 0)
			{
				previousSection = Plugin.CurrentRoute.Sections[m - 1];
			}

			Plugin.CurrentRoute.Sections[m] = new RouteManager2.SignalManager.Section(TrackPosition, new SectionAspect[Aspects.Length], SectionType.IndexBased, previousSection);

			if (m > 0)
			{
				Plugin.CurrentRoute.Sections[m - 1].NextSection = Plugin.CurrentRoute.Sections[m];
			}
			// create section

			for (int l = 0; l < Aspects.Length; l++)
			{
				Plugin.CurrentRoute.Sections[m].Aspects[l].Number = Aspects[l];
				if (Aspects[l] >= 0 & Aspects[l] < Data.SignalSpeeds.Length)
				{
					Plugin.CurrentRoute.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Aspects[l]];
				}
				else
				{
					Plugin.CurrentRoute.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
				}
			}
			Plugin.CurrentRoute.Sections[m].CurrentAspect = -1;
			Plugin.CurrentRoute.Sections[m].StationIndex = DepartureStationIndex;
			Plugin.CurrentRoute.Sections[m].Invisible = false;
			Plugin.CurrentRoute.Sections[m].Trains = new AbstractTrain[] { };

			// create section change event
			double d = TrackPosition - startingDistance;
			Plugin.CurrentRoute.Tracks[0].Elements[currentElement].Events.Add(new SectionChangeEvent(Plugin.CurrentRoute, d, m - 1, m));
		}
	}
}
