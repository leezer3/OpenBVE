using System;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>The types of section</summary>
		internal enum SectionType
		{
			/// <summary>A section aspect may have any value</summary>
			ValueBased, 
			/// <summary>Section aspect count upwards from zero (0,1,2,3....)</summary>
			IndexBased
		}

		/// <summary>A signalling aspect attached to a track section</summary>
		internal struct SectionAspect
		{
			/// <summary>The aspect number</summary>
			internal int Number;
			/// <summary>The speed limit associated with this aspect number</summary>
			internal double Speed;

			/// <summary>Creates a new signalling aspect</summary>
			/// <param name="Number">The aspect number</param>
			/// <param name="Speed">The speed limit</param>
			internal SectionAspect(int Number, double Speed)
			{
				this.Number = Number;
				this.Speed = Speed;
			}
		}

		/// <summary>Defines a complete signalling section</summary>
		public struct Section
		{
			/// <summary>The index of the previous section</summary>
			internal int PreviousSection;
			/// <summary>The index of the next section</summary>
			internal int NextSection;
			/// <summary>Holds a reference to all trains currently within this section</summary>
			internal TrainManager.Train[] Trains;
			/// <summary>Whether the primary train within the section has reached the station stop point (For departure signals)</summary>
			internal bool TrainReachedStopPoint;
			/// <summary>The index of the station (if applicable)</summary>
			internal int StationIndex;
			/// <summary>Whether this is an invisible section</summary>
			internal bool Invisible;
			/// <summary>The track position at which this section is placed</summary>
			internal double TrackPosition;
			/// <summary>The type of section</summary>
			internal SectionType Type;
			/// <summary>The aspects attached to this section</summary>
			internal SectionAspect[] Aspects;
			/// <summary>A public read-only variable, which returns the current aspect to external scripts</summary>
			public int currentAspect { get { return CurrentAspect; } }
			/// <summary>The current aspect</summary>
			internal int CurrentAspect;
			/// <summary>The number of free sections ahead of this section</summary>
			internal int FreeSections;

			/// <summary>Called when a train enters the section</summary>
			/// <param name="Train">The train</param>
			internal void Enter(TrainManager.Train Train)
			{
				int n = this.Trains.Length;
				for (int i = 0; i < n; i++)
				{
					if (this.Trains[i] == Train) return;
				}
				Array.Resize<TrainManager.Train>(ref this.Trains, n + 1);
				this.Trains[n] = Train;
			}

			/// <summary>Called when a train leaves the section</summary>
			/// <param name="Train">The train</param>
			internal void Leave(TrainManager.Train Train)
			{
				int n = this.Trains.Length;
				for (int i = 0; i < n; i++)
				{
					if (this.Trains[i] == Train)
					{
						for (int j = i; j < n - 1; j++)
						{
							this.Trains[j] = this.Trains[j + 1];
						}
						Array.Resize<TrainManager.Train>(ref this.Trains, n - 1);
						return;
					}
				}
			}

			/// <summary>Checks whether a train is currently within the section</summary>
			/// <param name="Train">The train</param>
			/// <returns>True if the train is within the section, false otherwise</returns>
			internal bool Exists(TrainManager.Train Train)
			{
				for (int i = 0; i < this.Trains.Length; i++)
				{
					if (this.Trains[i] == Train)
						return true;
				}
				return false;
			}
			/// <summary>Checks whether the section is free, disregarding the specified train.</summary>
			/// <param name="train">The train to disregard.</param>
			/// <returns>Whether the section is free, disregarding the specified train.</returns>
			internal bool IsFree(TrainManager.Train train)
			{
				for (int i = 0; i < this.Trains.Length; i++)
				{
					if (this.Trains[i] != train & (this.Trains[i].State == TrainManager.TrainState.Available | this.Trains[i].State == TrainManager.TrainState.Bogus))
					{
						return false;
					}
				}
				return true;
			}

			/// <summary>Checks whether the section is free</summary>
			/// <returns>Whether the section is free</returns>
			internal bool IsFree()
			{
				for (int i = 0; i < this.Trains.Length; i++)
				{
					if (this.Trains[i].State == TrainManager.TrainState.Available | this.Trains[i].State == TrainManager.TrainState.Bogus)
					{
						return false;
					}
				}
				return true;
			}

			/// <summary>Gets the first train within the section</summary>
			/// <param name="AllowBogusTrain">Whether bogus trains are to be allowed</param>
			/// <returns>The first train within the section, or null if no trains are found</returns>
			internal TrainManager.Train GetFirstTrain(bool AllowBogusTrain)
			{
				for (int i = 0; i < this.Trains.Length; i++)
				{
					if (this.Trains[i].State == TrainManager.TrainState.Available)
					{
						return this.Trains[i];
					}
					if (AllowBogusTrain & this.Trains[i].State == TrainManager.TrainState.Bogus)
					{
						return this.Trains[i];
					}
				}
				return null;
			}
		}

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
						train = Sections[l].GetFirstTrain(false);
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
						if (TrainManager.Trains[i].State == TrainManager.TrainState.Available)
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
							int c = GetStopIndex(d, train.Cars.Length);
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
					if (train == TrainManager.PlayerTrain & Stations[d].StationType != StationType.Normal & Stations[d].DepartureTime < 0.0)
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
				else if (Stations[d].StationType != StationType.Normal)
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
