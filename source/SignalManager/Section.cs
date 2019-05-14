using System;
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
