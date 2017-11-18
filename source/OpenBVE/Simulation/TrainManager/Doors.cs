using System;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>A train door</summary>
		internal struct Door
		{
			/// <summary>A value of -1 (left) or 1 (right).</summary>
			internal int Direction;
			/// <summary>A value between 0 (closed) and 1 (opened).</summary>
			internal double State;
			/// <summary>The value of the state at which a door lock simulation is scheduled.</summary>
			internal double DoorLockState;
			/// <summary>The duration of the scheduled door lock simulation.</summary>
			internal double DoorLockDuration;
			/// <summary>Stores whether the doors button is currently pressed</summary>
			internal bool ButtonPressed;
			/// <summary>Whether this door is anticipated to be open at the next station stop</summary>
			internal bool AnticipatedOpen;
			/// <summary>Played once when this set of doors opens</summary>
			internal CarSound OpenSound;
			/// <summary>Played once when this set of doors closes</summary>
			internal CarSound CloseSound;
		}

		/// <summary>The states of the door lock.</summary>
		internal enum DoorInterlockStates
		{
			/// <summary>The train doors are fully unlocked.</summary>
			Unlocked = 0,
			/// <summary>The train doors are unlocked only on the left side.</summary>
			Left = 1,
			/// <summary>The train doors are unlocked only on the right side.</summary>
			Right = 2,
			/// <summary>The train doors are fully locked.</summary>
			Locked = 3,
		}

		/// <summary>The potential states of the train's doors.</summary>
		[Flags]
		internal enum TrainDoorState
		{
			None = 0,
			/// <summary>Fully closed doors are present in the train.</summary>
			Closed = 1,
			/// <summary>Fully opened doors are present in the train.</summary>
			Opened = 2,
			/// <summary>Doors are present in the train which are neither fully closed nor fully opened.</summary>
			Mixed = 4,
			/// <summary>All doors in the train are fully closed.</summary>
			AllClosed = 8,
			/// <summary>All doors in the train are fully opened.</summary>
			AllOpened = 16,
			/// <summary>All doors in the train are neither fully closed nor fully opened.</summary>
			AllMixed = 32
		}
	
		/// <summary>Is called once a frame, to update the door states of the specified train</summary>
		/// <param name="Train">The train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		private static void UpdateTrainDoors(Train Train, double TimeElapsed)
		{
			OpenBveApi.Runtime.DoorStates oldState = OpenBveApi.Runtime.DoorStates.None;
			OpenBveApi.Runtime.DoorStates newState = OpenBveApi.Runtime.DoorStates.None;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				bool ld = Train.Cars[i].Doors[0].AnticipatedOpen;
				bool rd = Train.Cars[i].Doors[1].AnticipatedOpen;
				double os = Train.Cars[i].Specs.DoorOpenFrequency;
				double cs = Train.Cars[i].Specs.DoorCloseFrequency;

				for (int j = 0; j < Train.Cars[i].Doors.Length; j++)
				{
					if (Train.Cars[i].Doors[j].Direction == -1 | Train.Cars[i].Doors[j].Direction == 1)
					{
						bool shouldBeOpen = Train.Cars[i].Doors[j].Direction == -1 ? ld : rd;
						if (Train.Cars[i].Doors[j].State > 0.0)
						{
							if (Train.Cars[i].Doors[j].Direction == -1)
							{
								oldState |= OpenBveApi.Runtime.DoorStates.Left;
							}
							else
							{
								oldState |= OpenBveApi.Runtime.DoorStates.Right;
							}
						}
						if (shouldBeOpen)
						{
							// open
							Train.Cars[i].Doors[j].State += os * TimeElapsed;
							if (Train.Cars[i].Doors[j].State > 1.0)
							{
								Train.Cars[i].Doors[j].State = 1.0;
							}
						}
						else
						{
							// close
							if (Train.Cars[i].Doors[j].DoorLockDuration > 0.0)
							{
								if (Train.Cars[i].Doors[j].State > Train.Cars[i].Doors[j].DoorLockState)
								{
									Train.Cars[i].Doors[j].State -= cs * TimeElapsed;
								}
								if (Train.Cars[i].Doors[j].State < Train.Cars[i].Doors[j].DoorLockState)
								{
									Train.Cars[i].Doors[j].State = Train.Cars[i].Doors[j].DoorLockState;
								}
								Train.Cars[i].Doors[j].DoorLockDuration -= TimeElapsed;
								if (Train.Cars[i].Doors[j].DoorLockDuration < 0.0)
								{
									Train.Cars[i].Doors[j].DoorLockDuration = 0.0;
								}
							}
							else
							{
								Train.Cars[i].Doors[j].State -= cs * TimeElapsed;
							}
							if (Train.Cars[i].Doors[j].State < 0.0)
							{
								Train.Cars[i].Doors[j].State = 0.0;
							}
						}
						if (Train.Cars[i].Doors[j].State > 0.0)
						{
							if (Train.Cars[i].Doors[j].Direction == -1)
							{
								newState |= OpenBveApi.Runtime.DoorStates.Left;
							}
							else
							{
								newState |= OpenBveApi.Runtime.DoorStates.Right;
							}
						}
					}
				}
			}

			if (oldState != OpenBveApi.Runtime.DoorStates.None & newState == OpenBveApi.Runtime.DoorStates.None)
			{
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.PilotLampOn.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.PilotLampOn.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
			else if (oldState == OpenBveApi.Runtime.DoorStates.None & newState != OpenBveApi.Runtime.DoorStates.None)
			{
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.PilotLampOff.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.PilotLampOff.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
			if (oldState != newState)
			{
				if (Train.Plugin != null)
				{
					Train.Plugin.DoorChange(oldState, newState);
				}
			}
		}

		/// <summary>Called once a frame for each train when arriving at a station, in order to update the automatic doors</summary>
		/// <param name="Train">The train</param>
		/// <param name="StationIndex">The index of the train's next station</param>
		/// <param name="BackwardsTolerance">The backwards tolerance for this stop point</param>
		/// <param name="ForwardsTolerance">The forwards tolerance for this stop point</param>
		internal static void AttemptToOpenDoors(Train Train, int StationIndex, double BackwardsTolerance, double ForwardsTolerance)
		{
			if ((GetDoorsState(Train, Game.Stations[StationIndex].OpenLeftDoors, Game.Stations[StationIndex].OpenRightDoors) & TrainDoorState.AllOpened) == 0)
			{
					if (Train.StationDistanceToStopPoint < BackwardsTolerance & -Train.StationDistanceToStopPoint < ForwardsTolerance)
					{
						OpenTrainDoors(Train, Game.Stations[StationIndex].OpenLeftDoors, Game.Stations[StationIndex].OpenRightDoors);
					}
				
			}
		}

		/// <summary>Called once a frame for each train whilst stopped at a station with the doors open, in order to update the automatic doors</summary>
		/// <param name="Train">The train</param>
		internal static void AttemptToCloseDoors(Train Train)
		{
			if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - 1.0 / Train.Cars[Train.DriverCar].Specs.DoorCloseFrequency)
			{
				if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0)
				{
					CloseTrainDoors(Train, true, true);
					Train.Specs.DoorClosureAttempted = true;
				}
			}
		}

		/// <summary>Opens the left-hand or right-hand doors for the specified train</summary>
		/// <param name="Train">The train</param>
		/// <param name="Left">Whether to open the left-hand doors</param>
		/// <param name="Right">Whether to open the right-hand doors</param>
		internal static void OpenTrainDoors(Train Train, bool Left, bool Right)
		{
			bool sl = false, sr = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (Left & !Train.Cars[i].Doors[0].AnticipatedOpen)
				{
					Train.Cars[i].Doors[0].AnticipatedOpen = true;
					sl = true;
				}
				if (Right & !Train.Cars[i].Doors[1].AnticipatedOpen)
				{
					Train.Cars[i].Doors[1].AnticipatedOpen = true;
					sr = true;
				}
			}
			if (sl)
			{
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Sounds.SoundBuffer buffer = Train.Cars[i].Doors[0].OpenSound.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Doors[0].OpenSound.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorOpenPitch, 1.0, pos, Train, i, false);
					}
					for (int j = 0; j < Train.Cars[i].Doors.Length; j++)
					{
						if (Train.Cars[i].Doors[j].Direction == -1)
						{
							Train.Cars[i].Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}
			if (sr)
			{
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Sounds.SoundBuffer buffer = Train.Cars[i].Doors[1].OpenSound.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Doors[1].OpenSound.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorOpenPitch, 1.0, pos, Train, i, false);
					}
					for (int j = 0; j < Train.Cars[i].Doors.Length; j++)
					{
						if (Train.Cars[i].Doors[j].Direction == 1)
						{
							Train.Cars[i].Doors[j].DoorLockDuration = 0.0;
						}
					}
				}
			}
		}

		/// <summary>Closes the left-hand or right-hand doors for the specified train</summary>
		/// <param name="Train">The train</param>
		/// <param name="Left">Whether to close the left-hand doors</param>
		/// <param name="Right">Whether to close the right-hand doors</param>
		internal static void CloseTrainDoors(Train Train, bool Left, bool Right)
		{
			bool sl = false, sr = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (Left & Train.Cars[i].Doors[0].AnticipatedOpen)
				{
					Train.Cars[i].Doors[0].AnticipatedOpen = false;
					sl = true;
				}
				if (Right & Train.Cars[i].Doors[1].AnticipatedOpen)
				{
					Train.Cars[i].Doors[1].AnticipatedOpen = false;
					sr = true;
				}
			}
			if (sl)
			{
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Sounds.SoundBuffer buffer = Train.Cars[i].Doors[0].CloseSound.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Doors[0].CloseSound.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorClosePitch, 1.0, pos, Train, i, false);
					}
				}
			}
			if (sr)
			{
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Sounds.SoundBuffer buffer = Train.Cars[i].Doors[1].CloseSound.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Doors[1].CloseSound.Position;
						Sounds.PlaySound(buffer, Train.Cars[i].Specs.DoorClosePitch, 1.0, pos, Train, i, false);
					}
				}
			}
		}

		
		/// <summary>Returns the combination of door states encountered in a train.</summary>
		/// <param name="Train">The train to consider.</param>
		/// <param name="Left">Whether to include left doors.</param>
		/// <param name="Right">Whether to include right doors.</param>
		/// <returns>A bit mask combining encountered door states.</returns>
		internal static TrainDoorState GetDoorsState(Train Train, bool Left, bool Right)
		{
			bool opened = false, closed = false, mixed = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				for (int j = 0; j < Train.Cars[i].Doors.Length; j++)
				{
					if (Left & Train.Cars[i].Doors[j].Direction == -1 | Right & Train.Cars[i].Doors[j].Direction == 1)
					{
						if (Train.Cars[i].Doors[j].State == 0.0)
						{
							closed = true;
						}
						else if (Train.Cars[i].Doors[j].State == 1.0)
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
	}
}
