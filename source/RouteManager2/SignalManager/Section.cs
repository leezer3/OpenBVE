using System;
using System.Linq;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace RouteManager2.SignalManager
{
	/// <summary>Defines a complete signalling section</summary>
	public class Section
	{
		/// <summary>The previous section</summary>
		public readonly Section PreviousSection;

		/// <summary>The next section</summary>
		public Section NextSection;

		/// <summary>Holds a reference to all trains currently within this section</summary>
		public AbstractTrain[] Trains;

		/// <summary>Whether the primary train within the section has reached the station stop point (For departure signals)</summary>
		public bool TrainReachedStopPoint;

		/// <summary>The index of the station (if applicable)</summary>
		public int StationIndex;

		/// <summary>Whether this is an invisible section</summary>
		public bool Invisible;

		/// <summary>The track position at which this section is placed</summary>
		public readonly double TrackPosition;

		/// <summary>The type of section</summary>
		public readonly SectionType Type;

		/// <summary>The aspects attached to this section</summary>
		public readonly SectionAspect[] Aspects;

		/// <summary>The current aspect</summary>
		public int CurrentAspect;

		/// <summary>The number of free sections ahead of this section</summary>
		public int FreeSections;

		/// <summary>The last update time for this section</summary>
		internal double LastUpdate;

		internal double RedTimer;

		/// <summary>Whether this section has been announced with accessibility in use</summary>
		public bool AccessibilityAnnounced;

		public Section(double trackPosition, SectionAspect[] aspects, SectionType type, Section previousSection = null)
		{
			TrackPosition = trackPosition;
			Aspects = aspects;
			Type = type;
			Trains = new AbstractTrain[] {};
			PreviousSection = previousSection;
			NextSection = null;
			CurrentAspect = -1;
			RedTimer = -1;
		}

		/// <summary>Called when a train enters the section</summary>
		/// <param name="Train">The train</param>
		public void Enter(AbstractTrain Train)
		{
			int n = Trains.Length;

			for (int i = 0; i < n; i++)
			{
				if (Trains[i] == Train)
				{
					return;
				}
			}

			Array.Resize(ref Trains, n + 1);
			Trains[n] = Train;
		}

		/// <summary>Called when a train leaves the section</summary>
		/// <param name="Train">The train</param>
		public void Leave(AbstractTrain Train)
		{
			int n = Trains.Length;

			for (int i = 0; i < n; i++)
			{
				if (Trains[i] == Train)
				{
					for (int j = i; j < n - 1; j++)
					{
						Trains[j] = Trains[j + 1];
					}

					Array.Resize(ref Trains, n - 1);
					return;
				}
			}
		}

		/// <summary>Checks whether a train is currently within the section</summary>
		/// <param name="Train">The train</param>
		/// <returns>True if the train is within the section, false otherwise</returns>
		public bool Exists(AbstractTrain Train)
		{
			return Trains.Any(t => t == Train);
		}

		/// <summary>Checks whether the section is free, disregarding the specified train.</summary>
		/// <param name="train">The train to disregard.</param>
		/// <returns>Whether the section is free, disregarding the specified train.</returns>
		public bool IsFree(AbstractTrain train)
		{
			return Trains.All(t => !(t != train & (t.State == TrainState.Available | t.State == TrainState.Bogus)));
		}

		/// <summary>Checks whether the section is free</summary>
		/// <returns>Whether the section is free</returns>
		public bool IsFree()
		{
			return Trains.All(t => !(t.State == TrainState.Available | t.State == TrainState.Bogus));
		}

		/// <summary>Gets the first train within the section</summary>
		/// <param name="AllowBogusTrain">Whether bogus trains are to be allowed</param>
		/// <returns>The first train within the section, or null if no trains are found</returns>
		public AbstractTrain GetFirstTrain(bool AllowBogusTrain)
		{
			for (int i = 0; i < Trains.Length; i++)
			{
				if (Trains[i].State == TrainState.Available)
				{
					return Trains[i];
				}

				if (AllowBogusTrain & Trains[i].State == TrainState.Bogus)
				{
					return Trains[i];
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
						if (NextSection != null)
						{
							int value = NextSection.FreeSections;

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

						if (NextSection != null)
						{
							int value = NextSection.Aspects[NextSection.CurrentAspect].Number;

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
