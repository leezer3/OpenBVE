using System;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBve.SignalManager;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Holds all signal sections within the game</summary>
		public static Section[] Sections = new Section[] { };

		/// <summary>Updates all signal sections</summary>
		internal static void UpdateAllSections()
		{
			if (Sections.Length != 0)
			{
				UpdateSection(Sections.Length - 1);
			}
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="SectionIndex">The index of the section to update</param>
		internal static void UpdateSection(int SectionIndex)
		{
			if (SectionIndex >= Sections.Length || Sections == null)
			{
				return;
			}
			// preparations
			int zeroaspect;
			bool settored = false;
			if (Sections[SectionIndex].Type == SectionType.ValueBased)
			{
				// value-based
				zeroaspect = 0;
				for (int i = 1; i < Sections[SectionIndex].Aspects.Length; i++)
				{
					if (Sections[SectionIndex].Aspects[i].Number < Sections[SectionIndex].Aspects[zeroaspect].Number)
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
			int d = Sections[SectionIndex].StationIndex;
			if (d >= 0)
			{
				// look for train in previous blocks
				int l = Sections[SectionIndex].PreviousSection;
				TrainManager.Train train = null;
				while (true)
				{
					if (l >= 0)
					{
						train = (TrainManager.Train)Sections[l].GetFirstTrain(false);
						if (train != null)
						{
							break;
						}
						l = Sections[l].PreviousSection;
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
					if (!Sections[SectionIndex].TrainReachedStopPoint)
					{
						if (train.Station == d)
						{
							int c = Game.Stations[d].GetStopIndex(train.Cars.Length);
							if (c >= 0)
							{
								double p0 = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxle.Position + 0.5 * train.Cars[0].Length;
								double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;
								if (p0 >= p1)
								{
									Sections[SectionIndex].TrainReachedStopPoint = true;
								}
							}
							else
							{
								Sections[SectionIndex].TrainReachedStopPoint = true;
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
					else if (!Sections[SectionIndex].TrainReachedStopPoint)
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
			if (!Sections[SectionIndex].IsFree())
			{
				settored = true;
			}
			// free sections
			int newaspect = -1;
			if (settored)
			{
				Sections[SectionIndex].FreeSections = 0;
				newaspect = zeroaspect;
			}
			else
			{
				int n = Sections[SectionIndex].NextSection;
				if (n >= 0)
				{
					if (Sections[n].FreeSections == -1)
					{
						Sections[SectionIndex].FreeSections = -1;
					}
					else
					{
						Sections[SectionIndex].FreeSections = Sections[n].FreeSections + 1;
					}
				}
				else
				{
					Sections[SectionIndex].FreeSections = -1;
				}
			}
			// change aspect
			if (newaspect == -1)
			{
				if (Sections[SectionIndex].Type == SectionType.ValueBased)
				{
					// value-based
					int n = Sections[SectionIndex].NextSection;
					int a = Sections[SectionIndex].Aspects[Sections[SectionIndex].Aspects.Length - 1].Number;
					if (n >= 0 && Sections[n].CurrentAspect >= 0)
					{

						a = Sections[n].Aspects[Sections[n].CurrentAspect].Number;
					}
					for (int i = Sections[SectionIndex].Aspects.Length - 1; i >= 0; i--)
					{
						if (Sections[SectionIndex].Aspects[i].Number > a)
						{
							newaspect = i;
						}
					}
					if (newaspect == -1)
					{
						newaspect = Sections[SectionIndex].Aspects.Length - 1;
					}
				}
				else
				{
					// index-based
					if (Sections[SectionIndex].FreeSections >= 0 & Sections[SectionIndex].FreeSections < Sections[SectionIndex].Aspects.Length)
					{
						newaspect = Sections[SectionIndex].FreeSections;
					}
					else
					{
						newaspect = Sections[SectionIndex].Aspects.Length - 1;
					}
				}
			}
			// apply new aspect
			Sections[SectionIndex].CurrentAspect = newaspect;
			// update previous section
			if (Sections[SectionIndex].PreviousSection >= 0)
			{
				UpdateSection(Sections[SectionIndex].PreviousSection);
			}
		}

		/// <summary>Gets the signal data for a plugin.</summary>
		/// <param name="train">The train.</param>
		/// <param name="section">The absolute section index, referencing Game.Sections[].</param>
		/// <returns>The signal data.</returns>
		internal static OpenBveApi.Runtime.SignalData GetPluginSignal(TrainManager.Train train, int section)
		{
			if (Sections[section].Exists(train))
			{
				int aspect;
				if (Sections[section].IsFree(train))
				{
					if (Sections[section].Type == SectionType.IndexBased)
					{
						if (section + 1 < Sections.Length)
						{
							int value = Sections[section + 1].FreeSections;
							if (value == -1)
							{
								value = Sections[section].Aspects.Length - 1;
							}
							else
							{
								value++;
								if (value >= Sections[section].Aspects.Length)
								{
									value = Sections[section].Aspects.Length - 1;
								}
								if (value < 0)
								{
									value = 0;
								}
							}
							aspect = Sections[section].Aspects[value].Number;
						}
						else
						{
							aspect = Sections[section].Aspects[Sections[section].Aspects.Length - 1].Number;
						}
					}
					else
					{
						aspect = Sections[section].Aspects[Sections[section].Aspects.Length - 1].Number;
						if (section < Sections.Length - 1)
						{
							int value = Sections[section + 1].Aspects[Sections[section + 1].CurrentAspect].Number;
							for (int i = 0; i < Sections[section].Aspects.Length; i++)
							{
								if (Sections[section].Aspects[i].Number > value)
								{
									aspect = Sections[section].Aspects[i].Number;
									break;
								}
							}
						}
					}
				}
				else
				{
					aspect = Sections[section].Aspects[Sections[section].CurrentAspect].Number;
				}
				double position = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxle.Position + 0.5 * train.Cars[0].Length;
				double distance = Sections[section].TrackPosition - position;
				return new OpenBveApi.Runtime.SignalData(aspect, distance);
			}
			else
			{
				int aspect = Sections[section].Aspects[Sections[section].CurrentAspect].Number;
				double position = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxle.Position + 0.5 * train.Cars[0].Length;
				double distance = Sections[section].TrackPosition - position;
				return new OpenBveApi.Runtime.SignalData(aspect, distance);
			}
		}

		// update plugin sections
		/// <summary>Updates the plugin to inform about sections.</summary>
		/// <param name="train">The train.</param>
		internal static void UpdatePluginSections(TrainManager.Train train)
		{
			if (train.Plugin != null)
			{
				OpenBveApi.Runtime.SignalData[] data = new OpenBveApi.Runtime.SignalData[16];
				int count = 0;
				int start = train.CurrentSectionIndex >= 0 ? train.CurrentSectionIndex : 0;
				for (int i = start; i < Sections.Length; i++)
				{
					OpenBveApi.Runtime.SignalData signal = GetPluginSignal(train, i);
					if (data.Length == count)
					{
						Array.Resize<OpenBveApi.Runtime.SignalData>(ref data, data.Length << 1);
					}
					data[count] = signal;
					count++;
					if (signal.Aspect == 0 | count == 16)
					{
						break;
					}
				}
				Array.Resize<OpenBveApi.Runtime.SignalData>(ref data, count);
				train.Plugin.UpdateSignals(data);
			}
		}
	}
}
