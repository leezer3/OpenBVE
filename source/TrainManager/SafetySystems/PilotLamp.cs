using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using SoundManager;

namespace TrainManager.SafetySystems
{
	public class PilotLamp
	{
		/// <summary>Played once when all doors are closed</summary>
		public CarSound OnSound;
		/// <summary>Played once when the first door opens</summary>
		public CarSound OffSound;
		/// <summary>Holds the reference to the base car</summary>
		private readonly AbstractCar baseCar;
		/// <summary>The previous state of the train doors</summary>
		private DoorStates oldState;
		/// <summary>Whether the pilot lamp is currently lit</summary>
		public bool Lit;

		public PilotLamp(AbstractCar car)
		{
			baseCar = car;
			oldState = DoorStates.None;
			OnSound = new CarSound();
			OffSound = new CarSound();
		}

		public void Update(DoorStates newState)
		{
			if (oldState != DoorStates.None & newState == DoorStates.None)
			{
				Lit = true;
				OnSound.Play(baseCar, false);
			}
			else if (oldState == DoorStates.None & newState != DoorStates.None)
			{
				Lit = false;
				OffSound.Play(baseCar, false);
			}
			oldState = newState;
		}
	}
}
