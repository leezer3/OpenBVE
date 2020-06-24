using System;
using RouteManager2;
using RouteManager2.Events;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	internal class Section
	{

		internal Section(double trackPosition, int[] aspects, int departureStationIndex, SectionType type, bool invisible = false)
		{
			TrackPosition = trackPosition;
			Aspects = aspects;
			DepartureStationIndex = departureStationIndex;
			Type = type;
			Invisible = invisible;
		}

		private readonly double TrackPosition;
		private readonly int[] Aspects;
		private readonly int DepartureStationIndex;
		private readonly bool Invisible;
		private readonly SectionType Type;

		internal void Create(CurrentRoute CurrentRoute, Block[] Blocks, int CurrentBlock, int CurrentTrackElement, double[] SignalSpeeds, double StartingDistance, double BlockInterval)
		{
			int m = CurrentRoute.Sections.Length;
			Array.Resize(ref CurrentRoute.Sections, m + 1);
			
			// create section
			SectionAspect[] newAspects = new SectionAspect[Aspects.Length];
			for (int l = 0; l < Aspects.Length; l++)
			{
				newAspects[l].Number = Aspects[l];
				if (Aspects[l] >= 0 & Aspects[l] < SignalSpeeds.Length)
				{
					newAspects[l].Speed = SignalSpeeds[Aspects[l]];
				}
				else
				{
					newAspects[l].Speed = double.PositiveInfinity;
				}
			}
			
			if (m > 0)
			{
				CurrentRoute.Sections[m] = new RouteManager2.SignalManager.Section(TrackPosition, newAspects, Type, CurrentRoute.Sections[m - 1]);
				CurrentRoute.Sections[m - 1].NextSection = CurrentRoute.Sections[m];
			}
			else
			{
				CurrentRoute.Sections[m] = new RouteManager2.SignalManager.Section(TrackPosition, newAspects, Type);
			}

			CurrentRoute.Sections[m].StationIndex = DepartureStationIndex;
			CurrentRoute.Sections[m].Invisible = Invisible;
			// create section change event
			double d = TrackPosition - StartingDistance;
			int p = CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events.Length;
			Array.Resize(ref CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events, p + 1);
			CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events[p] = new SectionChangeEvent(CurrentRoute, d, m - 1, m);
		}
	}
}
