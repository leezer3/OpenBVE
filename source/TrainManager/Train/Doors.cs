using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using TrainManager.Car;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		/// <inheritdoc/>
		public override void OpenDoors(bool Left, bool Right)
		{
			bool sl = false, sr = false;
			for (int i = 0; i < Cars.Length; i++)
			{
				if (Left & !Cars[i].Doors[0].AnticipatedOpen & (SafetySystems.DoorInterlockState == DoorInterlockStates.Left | SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
				{
					Cars[i].Doors[0].AnticipatedOpen = true;
					sl = true;
				}

				if (Right & !Cars[i].Doors[1].AnticipatedOpen & (SafetySystems.DoorInterlockState == DoorInterlockStates.Right | SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
				{
					Cars[i].Doors[1].AnticipatedOpen = true;
					sr = true;
				}
			}

			if (sl)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[0].OpenSound.Play(Cars[i].Specs.DoorOpenPitch, 1.0, Cars[i], false);
					for (int j = 0; j < Cars[i].Doors.Length; j++)
					{
						if (Cars[i].Doors[j].Direction == -1)
						{
							Cars[i].Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}

			if (sr)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[1].OpenSound.Play(Cars[i].Specs.DoorOpenPitch, 1.0, Cars[i], false);
					for (int j = 0; j < Cars[i].Doors.Length; j++)
					{
						if (Cars[i].Doors[j].Direction == 1)
						{
							Cars[i].Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}
		}

		/// <inheritdoc/>
		public override void CloseDoors(bool Left, bool Right)
		{
			bool sl = false, sr = false;
			for (int i = 0; i < Cars.Length; i++)
			{
				if (Left & Cars[i].Doors[0].AnticipatedOpen & (SafetySystems.DoorInterlockState == DoorInterlockStates.Left | SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
				{
					Cars[i].Doors[0].AnticipatedOpen = false;
					sl = true;
				}

				if (Right & Cars[i].Doors[1].AnticipatedOpen & (SafetySystems.DoorInterlockState == DoorInterlockStates.Right | SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
				{
					Cars[i].Doors[1].AnticipatedOpen = false;
					sr = true;
				}
			}

			if (sl)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[0].CloseSound.Play(Cars[i].Specs.DoorClosePitch, 1.0, Cars[i], false);
				}
			}

			if (sr)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[1].CloseSound.Play(Cars[i].Specs.DoorClosePitch, 1.0, Cars[i], false);
				}
			}
		}

		/// <summary>Returns the combination of door states encountered in a </summary>
		/// <param name="Left">Whether to include left doors.</param>
		/// <param name="Right">Whether to include right doors.</param>
		/// <returns>A bit mask combining encountered door states.</returns>
		public TrainDoorState GetDoorsState(bool Left, bool Right)
		{
			bool opened = false, closed = false, mixed = false;
			for (int i = 0; i < Cars.Length; i++)
			{
				for (int j = 0; j < Cars[i].Doors.Length; j++)
				{
					if (Left & Cars[i].Doors[j].Direction == -1 | Right & Cars[i].Doors[j].Direction == 1)
					{
						if (Cars[i].Doors[j].State == 0.0)
						{
							closed = true;
						}
						else if (Cars[i].Doors[j].State == 1.0)
						{
							opened = true;
						}
						else
						{
							mixed = true;
						}
					}
				}
			}

			TrainDoorState Result = TrainDoorState.None;
			if (opened) Result |= TrainDoorState.Opened;
			if (closed) Result |= TrainDoorState.Closed;
			if (mixed) Result |= TrainDoorState.Mixed;
			if (opened & !closed & !mixed) Result |= TrainDoorState.AllOpened;
			if (!opened & closed & !mixed) Result |= TrainDoorState.AllClosed;
			if (!opened & !closed & mixed) Result |= TrainDoorState.AllMixed;
			return Result;
		}

		/// <summary>Called once a frame for each train when arriving at a station, in order to update the automatic doors</summary>
		/// <param name="NextStation">The train's next station</param>
		/// <param name="BackwardsTolerance">The backwards tolerance for this stop point</param>
		/// <param name="ForwardsTolerance">The forwards tolerance for this stop point</param>
		public void AttemptToOpenDoors(Station NextStation, double BackwardsTolerance, double ForwardsTolerance)
		{
			if ((GetDoorsState(NextStation.OpenLeftDoors, NextStation.OpenRightDoors) & TrainDoorState.AllOpened) == 0)
			{
				if (StationDistanceToStopPoint < BackwardsTolerance & -StationDistanceToStopPoint < ForwardsTolerance)
				{
					OpenDoors(NextStation.OpenLeftDoors, NextStation.OpenRightDoors);
				}

			}
		}

		/// <summary>Called once a frame for each train whilst stopped at a station with the doors open, in order to update the automatic doors</summary>
		public void AttemptToCloseDoors()
		{
			if (TrainManagerBase.currentHost.InGameTime >= StationDepartureTime - 1.0 / Cars[DriverCar].Specs.DoorCloseFrequency)
			{
				if ((GetDoorsState(true, true) & TrainDoorState.AllClosed) == 0)
				{
					CloseDoors(true, true);
					Specs.DoorClosureAttempted = true;
				}
			}
		}

		/// <summary>Is called once a frame, to update the door states of the train</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		public void UpdateDoors(double TimeElapsed)
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

			SafetySystems.PilotLamp?.Update(newState);

			if (oldState != newState)
			{
				Plugin?.DoorChange(oldState, newState);
				if (IsPlayerTrain)
				{
					for (int j = 0; j < InputDevicePlugin.AvailablePluginInfos.Count; j++)
					{
						if (InputDevicePlugin.AvailablePluginInfos[j].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable 
							&& InputDevicePlugin.AvailablePlugins[j] is ITrainInputDevice trainInputDevice)
						{
							trainInputDevice.DoorChange(oldState, newState);
						}
					}
				}
			}
		}
	}
}
