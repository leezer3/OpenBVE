using System;
using OpenBve.RouteManager;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace OpenBve.SignalManager
{
	/// <summary>Defines a complete signalling section</summary>
	public struct Section
	{
		/// <summary>The index of the previous section</summary>
		public int PreviousSection;
		/// <summary>The index of the next section</summary>
		public int NextSection;
		/// <summary>Holds a reference to all trains currently within this section</summary>
		public AbstractTrain[] Trains;
		/// <summary>Whether the primary train within the section has reached the station stop point (For departure signals)</summary>
		public bool TrainReachedStopPoint;
		/// <summary>The index of the station (if applicable)</summary>
		public int StationIndex;
		/// <summary>Whether this is an invisible section</summary>
		public bool Invisible;
		/// <summary>The track position at which this section is placed</summary>
		public double TrackPosition;
		/// <summary>The type of section</summary>
		public SectionType Type;
		/// <summary>The aspects attached to this section</summary>
		public SectionAspect[] Aspects;
		/// <summary>A public read-only variable, which returns the current aspect to external scripts</summary>
		public int currentAspect
		{
			get
			{
				return CurrentAspect;
			}
		}
		/// <summary>The current aspect</summary>
		public int CurrentAspect;
		/// <summary>The number of free sections ahead of this section</summary>
		public int FreeSections;

		/// <summary>Updates the specified signal section</summary>
		/// <param name="secondsSinceMidnight">The in-game time in seconds since midnight</param>
		public void Update(double secondsSinceMidnight)
		{
			// preparations
			int zeroaspect;
			bool settored = false;
			if (Type == SectionType.ValueBased)
			{
				// value-based
				zeroaspect = 0;
				for (int i = 1; i < Aspects.Length; i++)
				{
					if (Aspects[i].Number < Aspects[zeroaspect].Number)
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
			int d = StationIndex;
			if (d >= 0)
			{
				// look for train in previous blocks
				int l = PreviousSection;
				AbstractTrain train = null;
				while (true)
				{
					if (l >= 0)
					{
						train = CurrentRoute.Sections[l].GetFirstTrain(false);
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
					for (int i = 0; i < CurrentRoute.Trains.Length; i++)
					{
						if (CurrentRoute.Trains[i].State == TrainState.Available)
						{
							if (CurrentRoute.Trains[i].TimetableDelta > b)
							{
								b = CurrentRoute.Trains[i].TimetableDelta;
								train = CurrentRoute.Trains[i];
							}
						}
					}
				}
				// set to red where applicable
				if (train != null)
				{
					if (!TrainReachedStopPoint)
					{
						if (train.Station == d)
						{
							int c = CurrentRoute.Stations[d].GetStopIndex(train.NumberOfCars);
							if (c >= 0)
							{
								double p0 = train.FrontCarTrackPosition();
								double p1 = CurrentRoute.Stations[d].Stops[c].TrackPosition - CurrentRoute.Stations[d].Stops[c].BackwardTolerance;
								if (p0 >= p1)
								{
									TrainReachedStopPoint = true;
								}
							}
							else
							{
								TrainReachedStopPoint = true;
							}
						}
					}
					double t = -15.0;
					if (CurrentRoute.Stations[d].DepartureTime >= 0.0)
					{
						t = CurrentRoute.Stations[d].DepartureTime - 15.0;
					}
					else if (CurrentRoute.Stations[d].ArrivalTime >= 0.0)
					{
						t = CurrentRoute.Stations[d].ArrivalTime;
					}
					if (train.IsPlayerTrain & CurrentRoute.Stations[d].Type != StationType.Normal & CurrentRoute.Stations[d].DepartureTime < 0.0)
					{
						settored = true;
					}
					else if (t >= 0.0 & secondsSinceMidnight < t - train.TimetableDelta)
					{
						settored = true;
					}
					else if (!TrainReachedStopPoint)
					{
						settored = true;
					}
				}
				else if (CurrentRoute.Stations[d].Type != StationType.Normal)
				{
					settored = true;
				}
			}
			// train in block
			if (!IsFree())
			{
				settored = true;
			}
			// free sections
			int newaspect = -1;
			if (settored)
			{
				FreeSections = 0;
				newaspect = zeroaspect;
			}
			else
			{
				int n = NextSection;
				if (n >= 0)
				{
					if (CurrentRoute.Sections[n].FreeSections == -1)
					{
						FreeSections = -1;
					}
					else
					{
						FreeSections = CurrentRoute.Sections[n].FreeSections + 1;
					}
				}
				else
				{
					FreeSections = -1;
				}
			}
			// change aspect
			if (newaspect == -1)
			{
				if (Type == SectionType.ValueBased)
				{
					// value-based
					int n = NextSection;
					int a = Aspects[Aspects.Length - 1].Number;
					if (n >= 0 && CurrentRoute.Sections[n].CurrentAspect >= 0)
					{

						a = CurrentRoute.Sections[n].Aspects[CurrentRoute.Sections[n].CurrentAspect].Number;
					}
					for (int i = Aspects.Length - 1; i >= 0; i--)
					{
						if (Aspects[i].Number > a)
						{
							newaspect = i;
						}
					}
					if (newaspect == -1)
					{
						newaspect = Aspects.Length - 1;
					}
				}
				else
				{
					// index-based
					if (FreeSections >= 0 & FreeSections < Aspects.Length)
					{
						newaspect = FreeSections;
					}
					else
					{
						newaspect = Aspects.Length - 1;
					}
				}
			}
			// apply new aspect
			CurrentAspect = newaspect;
			// update previous section
			if (PreviousSection >= 0 && PreviousSection < CurrentRoute.Sections.Length)
			{
				CurrentRoute.Sections[PreviousSection].Update(secondsSinceMidnight);
			}
		}

		/// <summary>Called when a train enters the section</summary>
		/// <param name="Train">The train</param>
		public void Enter(AbstractTrain Train)
		{
			int n = this.Trains.Length;
			for (int i = 0; i < n; i++)
			{
				if (this.Trains[i] == Train) return;
			}

			Array.Resize<AbstractTrain>(ref this.Trains, n + 1);
			this.Trains[n] = Train;
		}

		/// <summary>Called when a train leaves the section</summary>
		/// <param name="Train">The train</param>
		public void Leave(AbstractTrain Train)
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

					Array.Resize<AbstractTrain>(ref this.Trains, n - 1);
					return;
				}
			}
		}

		/// <summary>Checks whether a train is currently within the section</summary>
		/// <param name="Train">The train</param>
		/// <returns>True if the train is within the section, false otherwise</returns>
		public bool Exists(AbstractTrain Train)
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
		public bool IsFree(AbstractTrain train)
		{
			for (int i = 0; i < this.Trains.Length; i++)
			{
				if (this.Trains[i] != train & (this.Trains[i].State == TrainState.Available | this.Trains[i].State == TrainState.Bogus))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>Checks whether the section is free</summary>
		/// <returns>Whether the section is free</returns>
		public bool IsFree()
		{
			for (int i = 0; i < this.Trains.Length; i++)
			{
				if (this.Trains[i].State == TrainState.Available | this.Trains[i].State == TrainState.Bogus)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>Gets the first train within the section</summary>
		/// <param name="AllowBogusTrain">Whether bogus trains are to be allowed</param>
		/// <returns>The first train within the section, or null if no trains are found</returns>
		public AbstractTrain GetFirstTrain(bool AllowBogusTrain)
		{
			for (int i = 0; i < this.Trains.Length; i++)
			{
				if (this.Trains[i].State == TrainState.Available)
				{
					return this.Trains[i];
				}

				if (AllowBogusTrain & this.Trains[i].State == TrainState.Bogus)
				{
					return this.Trains[i];
				}
			}

			return null;
		}

		/// <summary>Gets the signal data for a plugin.</summary>
		/// <param name="train">The train.</param>
		/// <returns>The signal data.</returns>
		public SignalData GetPluginSignal(AbstractTrain train)
		{
			if (Exists(train))
			{
				int aspect;
				if (IsFree(train))
				{
					if (Type == SectionType.IndexBased)
					{
						if (NextSection != -1)
						{
							int value = CurrentRoute.Sections[NextSection].FreeSections;
							if (value == -1)
							{
								value = Aspects.Length - 1;
							}
							else
							{
								value++;
								if (value >= Aspects.Length)
								{
									value = Aspects.Length - 1;
								}
								if (value < 0)
								{
									value = 0;
								}
							}
							aspect = Aspects[value].Number;
						}
						else
						{
							aspect = Aspects[Aspects.Length - 1].Number;
						}
					}
					else
					{
						aspect = Aspects[Aspects.Length - 1].Number;
						if (NextSection != -1)
						{
							int value = Aspects[CurrentAspect].Number;
							for (int i = 0; i < Aspects.Length; i++)
							{
								if (Aspects[i].Number > value)
								{
									aspect = Aspects[i].Number;
									break;
								}
							}
						}
					}
				}
				else
				{
					aspect = Aspects[CurrentAspect].Number;
				}
				double position = train.FrontCarTrackPosition();
				double distance = TrackPosition - position;
				return new SignalData(aspect, distance);
			}
			else
			{
				int aspect = Aspects[CurrentAspect].Number;
				double position = train.FrontCarTrackPosition();
				double distance = TrackPosition - position;
				return new SignalData(aspect, distance);
			}
		}
	}
}
