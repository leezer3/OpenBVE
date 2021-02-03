using OpenBveApi.Runtime;
using TrainManager.Car;

namespace TrainManager.Trains
{
	public abstract partial class TrainBase
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
	}
}
