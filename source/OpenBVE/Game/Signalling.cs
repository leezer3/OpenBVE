using System;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBve.RouteManager;
using OpenBve.SignalManager;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Updates all signal sections</summary>
		internal static void UpdateAllSections()
		{
			if (CurrentRoute.Sections.Length != 0)
			{
				UpdateSection(CurrentRoute.Sections.Length - 1);
			}
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="SectionIndex">The index of the section to update</param>
		internal static void UpdateSection(int SectionIndex)
		{
			if (SectionIndex >= CurrentRoute.Sections.Length || CurrentRoute.Sections == null)
			{
				return;
			}
			// preparations
			int zeroaspect;
			bool settored = false;
			if (CurrentRoute.Sections[SectionIndex].Type == SectionType.ValueBased)
			{
				// value-based
				zeroaspect = 0;
				for (int i = 1; i < CurrentRoute.Sections[SectionIndex].Aspects.Length; i++)
				{
					if (CurrentRoute.Sections[SectionIndex].Aspects[i].Number < CurrentRoute.Sections[SectionIndex].Aspects[zeroaspect].Number)
					{
						zeroaspect = i;
					}
				}
			}
			else
			{
				// index-based
				zeroaspect = 0;
			}
			// hold station departure signal at red
			int d = CurrentRoute.Sections[SectionIndex].StationIndex;
			if (d >= 0)
			{
				// look for train in previous blocks
				int l = CurrentRoute.Sections[SectionIndex].PreviousSection;
				TrainManager.Train train = null;
				while (true)
				{
					if (l >= 0)
					{
						train = (TrainManager.Train) CurrentRoute.Sections[l].GetFirstTrain(false);
						if (train != null)
						{
							break;
						}
						l = CurrentRoute.Sections[l].PreviousSection;
					}
					else
					{
						break;
					}
				}
				if (train == null)
				{
					double b = -double.MaxValue;
					for (int i = 0; i < TrainManager.Trains.Length; i++)
					{
						if (TrainManager.Trains[i].State == TrainState.Available)
						{
							if (TrainManager.Trains[i].TimetableDelta > b)
							{
								b = TrainManager.Trains[i].TimetableDelta;
								train = TrainManager.Trains[i];
							}
						}
					}
				}
				// set to red where applicable
				if (train != null)
				{
					if (!CurrentRoute.Sections[SectionIndex].TrainReachedStopPoint)
					{
						if (train.Station == d)
						{
							int c = Game.Stations[d].GetStopIndex(train.Cars.Length);
							if (c >= 0)
							{
								double p0 = train.FrontCarTrackPosition();
								double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;
								if (p0 >= p1)
								{
									CurrentRoute.Sections[SectionIndex].TrainReachedStopPoint = true;
								}
							}
							else
							{
								CurrentRoute.Sections[SectionIndex].TrainReachedStopPoint = true;
							}
						}
					}
					double t = -15.0;
					if (Stations[d].DepartureTime >= 0.0)
					{
						t = Stations[d].DepartureTime - 15.0;
					}
					else if (Stations[d].ArrivalTime >= 0.0)
					{
						t = Stations[d].ArrivalTime;
					}
					if (train == TrainManager.PlayerTrain & Stations[d].Type != StationType.Normal & Stations[d].DepartureTime < 0.0)
					{
						settored = true;
					}
					else if (t >= 0.0 & SecondsSinceMidnight < t - train.TimetableDelta)
					{
						settored = true;
					}
					else if (!CurrentRoute.Sections[SectionIndex].TrainReachedStopPoint)
					{
						settored = true;
					}
				}
				else if (Stations[d].Type != StationType.Normal)
				{
					settored = true;
				}
			}
			// train in block
			if (!CurrentRoute.Sections[SectionIndex].IsFree())
			{
				settored = true;
			}
			// free sections
			int newaspect = -1;
			if (settored)
			{
				CurrentRoute.Sections[SectionIndex].FreeSections = 0;
				newaspect = zeroaspect;
			}
			else
			{
				int n = CurrentRoute.Sections[SectionIndex].NextSection;
				if (n >= 0)
				{
					if (CurrentRoute.Sections[n].FreeSections == -1)
					{
						CurrentRoute.Sections[SectionIndex].FreeSections = -1;
					}
					else
					{
						CurrentRoute.Sections[SectionIndex].FreeSections = CurrentRoute.Sections[n].FreeSections + 1;
					}
				}
				else
				{
					CurrentRoute.Sections[SectionIndex].FreeSections = -1;
				}
			}
			// change aspect
			if (newaspect == -1)
			{
				if (CurrentRoute.Sections[SectionIndex].Type == SectionType.ValueBased)
				{
					// value-based
					int n = CurrentRoute.Sections[SectionIndex].NextSection;
					int a = CurrentRoute.Sections[SectionIndex].Aspects[CurrentRoute.Sections[SectionIndex].Aspects.Length - 1].Number;
					if (n >= 0 && CurrentRoute.Sections[n].CurrentAspect >= 0)
					{

						a = CurrentRoute.Sections[n].Aspects[CurrentRoute.Sections[n].CurrentAspect].Number;
					}
					for (int i = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1; i >= 0; i--)
					{
						if (CurrentRoute.Sections[SectionIndex].Aspects[i].Number > a)
						{
							newaspect = i;
						}
					}
					if (newaspect == -1)
					{
						newaspect = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1;
					}
				}
				else
				{
					// index-based
					if (CurrentRoute.Sections[SectionIndex].FreeSections >= 0 & CurrentRoute.Sections[SectionIndex].FreeSections < CurrentRoute.Sections[SectionIndex].Aspects.Length)
					{
						newaspect = CurrentRoute.Sections[SectionIndex].FreeSections;
					}
					else
					{
						newaspect = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1;
					}
				}
			}
			// apply new aspect
			CurrentRoute.Sections[SectionIndex].CurrentAspect = newaspect;
			// update previous section
			if (CurrentRoute.Sections[SectionIndex].PreviousSection >= 0)
			{
				UpdateSection(CurrentRoute.Sections[SectionIndex].PreviousSection);
			}
		}

		// update plugin sections
		/// <summary>Updates the plugin to inform about sections.</summary>
		/// <param name="train">The train.</param>
		internal static void UpdatePluginSections(TrainManager.Train train)
		{
			if (train.Plugin != null)
			{
				SignalData[] data = new SignalData[16];
				int count = 0;
				int start = train.CurrentSectionIndex >= 0 ? train.CurrentSectionIndex : 0;
				for (int i = start; i < CurrentRoute.Sections.Length; i++)
				{
					SignalData signal = CurrentRoute.Sections[i].GetPluginSignal(train);
					if (data.Length == count)
					{
						Array.Resize<SignalData>(ref data, data.Length << 1);
					}
					data[count] = signal;
					count++;
					if (signal.Aspect == 0 | count == 16)
					{
						break;
					}
				}
				Array.Resize<SignalData>(ref data, count);
				train.Plugin.UpdateSignals(data);
			}
		}
	}
}
