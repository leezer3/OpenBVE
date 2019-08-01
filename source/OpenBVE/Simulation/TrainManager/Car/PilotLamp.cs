using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve
{
	public partial class TrainManager
	{
		internal class PilotLamp
		{
			/// <summary>Played once when all doors are closed</summary>
			internal CarSound OnSound;
			/// <summary>Played once when the first door opens</summary>
			internal CarSound OffSound;
			/// <summary>Holds the reference to the base car</summary>
			private readonly AbstractCar baseCar;
			/// <summary>The previous state of the train doors</summary>
			private DoorStates oldState;

			internal PilotLamp(AbstractCar car)
			{
				baseCar = car;
				oldState = DoorStates.None;
				OnSound = new CarSound();
				OffSound = new CarSound();
			}

			internal void Update(DoorStates newState)
			{
				if (oldState != DoorStates.None & newState == DoorStates.None)
				{
					SoundBuffer buffer = OnSound.Buffer;
					if (buffer != null)
					{
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, OnSound.Position, baseCar, false);
					}
				}
				else if (oldState == DoorStates.None & newState != DoorStates.None)
				{
					SoundBuffer buffer = OffSound.Buffer;
					if (buffer != null)
					{
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, OffSound.Position, baseCar, false);
					}
				}
				oldState = newState;
			}
		}
	}
}
