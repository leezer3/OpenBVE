using OpenBveApi.Runtime;
using SoundManager;
using TrainManager.Car;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train
		{
			/// <summary>Is called once a frame, to update the door states of the train</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			internal void UpdateTrainDoors(double TimeElapsed)
			{
				DoorStates oldState = DoorStates.None;
				DoorStates newState = DoorStates.None;
				for (int i = 0; i < Cars.Length; i++)
				{
					bool ld = Cars[i].Doors[0].AnticipatedOpen;
					bool rd = Cars[i].Doors[1].AnticipatedOpen;
					double os = Cars[i].Specs.DoorOpenFrequency;
					double cs = Cars[i].Specs.DoorCloseFrequency;

					for (int j = 0; j < Cars[i].Doors.Length; j++)
					{
						if (Cars[i].Doors[j].Direction == -1 | Cars[i].Doors[j].Direction == 1)
						{
							bool shouldBeOpen = Cars[i].Doors[j].Direction == -1 ? ld : rd;
							if (Cars[i].Doors[j].State > 0.0)
							{
								if (Cars[i].Doors[j].Direction == -1)
								{
									oldState |= DoorStates.Left;
								}
								else
								{
									oldState |= DoorStates.Right;
								}
							}

							if (shouldBeOpen)
							{
								// open
								Cars[i].Doors[j].State += os * TimeElapsed;
								if (Cars[i].Doors[j].State > 1.0)
								{
									Cars[i].Doors[j].State = 1.0;
								}
							}
							else
							{
								// close
								if (Cars[i].Doors[j].DoorLockDuration > 0.0)
								{
									if (Cars[i].Doors[j].State > Cars[i].Doors[j].DoorLockState)
									{
										Cars[i].Doors[j].State -= cs * TimeElapsed;
									}

									if (Cars[i].Doors[j].State < Cars[i].Doors[j].DoorLockState)
									{
										Cars[i].Doors[j].State = Cars[i].Doors[j].DoorLockState;
									}

									Cars[i].Doors[j].DoorLockDuration -= TimeElapsed;
									if (Cars[i].Doors[j].DoorLockDuration < 0.0)
									{
										Cars[i].Doors[j].DoorLockDuration = 0.0;
									}
								}
								else
								{
									Cars[i].Doors[j].State -= cs * TimeElapsed;
								}

								if (Cars[i].Doors[j].AnticipatedReopen && Cars[i].Doors[j].State < Cars[i].Doors[j].InterferingObjectRate)
								{
									Cars[i].Doors[j].State = Cars[i].Doors[j].InterferingObjectRate;
								}

								if (Cars[i].Doors[j].State < 0.0)
								{
									Cars[i].Doors[j].State = 0.0;
								}
							}

							if (Cars[i].Doors[j].State > 0.0)
							{
								if (Cars[i].Doors[j].Direction == -1)
								{
									newState |= DoorStates.Left;
								}
								else
								{
									newState |= DoorStates.Right;
								}
							}
						}
					}
				}

				if (SafetySystems.PilotLamp != null)
				{
					SafetySystems.PilotLamp.Update(newState);
				}

				if (oldState != newState)
				{
					if (Plugin != null)
					{
						Plugin.DoorChange(oldState, newState);
					}
				}
			}

			/// <summary>Called once a frame for each train when arriving at a station, in order to update the automatic doors</summary>
			/// <param name="StationIndex">The index of the train's next station</param>
			/// <param name="BackwardsTolerance">The backwards tolerance for this stop point</param>
			/// <param name="ForwardsTolerance">The forwards tolerance for this stop point</param>
			internal void AttemptToOpenDoors(int StationIndex, double BackwardsTolerance, double ForwardsTolerance)
			{
				if ((GetDoorsState(Program.CurrentRoute.Stations[StationIndex].OpenLeftDoors, Program.CurrentRoute.Stations[StationIndex].OpenRightDoors) & TrainDoorState.AllOpened) == 0)
				{
					if (StationDistanceToStopPoint < BackwardsTolerance & -StationDistanceToStopPoint < ForwardsTolerance)
					{
						OpenDoors(Program.CurrentRoute.Stations[StationIndex].OpenLeftDoors, Program.CurrentRoute.Stations[StationIndex].OpenRightDoors);
					}

				}
			}

			/// <summary>Called once a frame for each train whilst stopped at a station with the doors open, in order to update the automatic doors</summary>
			internal void AttemptToCloseDoors()
			{
				if (Program.CurrentRoute.SecondsSinceMidnight >= StationDepartureTime - 1.0 / Cars[DriverCar].Specs.DoorCloseFrequency)
				{
					if ((GetDoorsState(true, true) & TrainDoorState.AllClosed) == 0)
					{
						CloseDoors(true, true);
						Specs.DoorClosureAttempted = true;
					}
				}
			}
		}
	}
}
