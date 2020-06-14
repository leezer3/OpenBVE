using System;
using OpenBveApi.Trains;
using RouteManager2;
using RouteManager2.Events;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
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

		internal void Create(CurrentRoute CurrentRoute, Parser.Block[] Blocks, int CurrentBlock, int CurrentTrackElement, double[] SignalSpeeds, double StartingDistance, double BlockInterval)
		{
			int m = CurrentRoute.Sections.Length;
			Array.Resize(ref CurrentRoute.Sections, m + 1);
			// create associated transponders
			for (int g = 0; g <= CurrentBlock; g++)
			{
				for (int l = 0; l < Blocks[g].Transponders.Length; l++)
				{
					if (Blocks[g].Transponders[l].Type != -1 & Blocks[g].Transponders[l].SectionIndex == m)
					{
						int o = CurrentRoute.Tracks[0].Elements[CurrentTrackElement - CurrentBlock + g].Events.Length;
						Array.Resize(ref CurrentRoute.Tracks[0].Elements[CurrentTrackElement - CurrentBlock + g].Events, o + 1);
						double dt = Blocks[g].Transponders[l].TrackPosition - StartingDistance + (double) (CurrentBlock - g) * BlockInterval;
						CurrentRoute.Tracks[0].Elements[CurrentTrackElement - CurrentBlock + g].Events[o] = new TransponderEvent(CurrentRoute, dt, Blocks[g].Transponders[l].Type, Blocks[g].Transponders[l].Data, m, Blocks[g].Transponders[l].ClipToFirstRedSection);
						Blocks[g].Transponders[l].Type = -1;
					}
				}
			}

			// create section
			CurrentRoute.Sections[m] = new RouteManager2.SignalManager.Section();
			CurrentRoute.Sections[m].TrackPosition = TrackPosition;
			CurrentRoute.Sections[m].Aspects = new SectionAspect[Aspects.Length];
			for (int l = 0; l < Aspects.Length; l++)
			{
				CurrentRoute.Sections[m].Aspects[l].Number = Aspects[l];
				if (Aspects[l] >= 0 & Aspects[l] < SignalSpeeds.Length)
				{
					CurrentRoute.Sections[m].Aspects[l].Speed = SignalSpeeds[Aspects[l]];
				}
				else
				{
					CurrentRoute.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
				}
			}

			CurrentRoute.Sections[m].Type = Type;
			CurrentRoute.Sections[m].CurrentAspect = -1;
			if (m > 0)
			{
				CurrentRoute.Sections[m].PreviousSection = CurrentRoute.Sections[m - 1];
				CurrentRoute.Sections[m - 1].NextSection = CurrentRoute.Sections[m];
			}
			else
			{
				CurrentRoute.Sections[m].PreviousSection = null;
			}

			CurrentRoute.Sections[m].NextSection = null;
			CurrentRoute.Sections[m].StationIndex = DepartureStationIndex;
			CurrentRoute.Sections[m].Invisible = Invisible;
			CurrentRoute.Sections[m].Trains = new AbstractTrain[] { };
			// create section change event
			double d = TrackPosition - StartingDistance;
			int p = CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events.Length;
			Array.Resize(ref CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events, p + 1);
			CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events[p] = new SectionChangeEvent(CurrentRoute, d, m - 1, m);
		}
	}
}
