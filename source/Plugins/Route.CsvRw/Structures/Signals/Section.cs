using System;
using System.Collections.Generic;
using RouteManager2;
using RouteManager2.Events;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	internal class Section : AbstractStructure
	{

		internal Section(double trackPosition, int[] aspects, int departureStationIndex, SectionType type, bool invisible = false) : base(trackPosition)
		{
			Aspects = aspects;
			DepartureStationIndex = departureStationIndex;
			Type = type;
			Invisible = invisible;
		}

		private readonly int[] Aspects;
		private readonly int DepartureStationIndex;
		private readonly bool Invisible;
		private readonly SectionType Type;

		internal void Create(CurrentRoute CurrentRoute, List<Block> Blocks, int CurrentBlock, int CurrentTrackElement, double[] SignalSpeeds, double StartingDistance, double BlockInterval)
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
						int blockIdx = CurrentTrackElement - CurrentBlock + g;
						if (blockIdx < 0)
						{
							/*
							 * Section created at track position zero attempts to create
							 * the associated transponders in the preceeding block
							 */
							blockIdx = 0;
						}
						double dt = Blocks[g].Transponders[l].TrackPosition - StartingDistance + (CurrentBlock - g) * BlockInterval;
						CurrentRoute.Tracks[0].Elements[blockIdx].Events.Add(new TransponderEvent(CurrentRoute, dt, Blocks[g].Transponders[l].Type, Blocks[g].Transponders[l].Data, m, Blocks[g].Transponders[l].ClipToFirstRedSection));
						Blocks[g].Transponders[l].Type = -1;
					}
				}
			}

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
			CurrentRoute.Tracks[0].Elements[CurrentTrackElement].Events.Add(new SectionChangeEvent(CurrentRoute, d, m - 1, m));
		}
	}
}
