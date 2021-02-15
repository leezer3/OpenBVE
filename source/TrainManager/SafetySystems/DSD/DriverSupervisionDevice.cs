using OpenBveApi.Trains;
using SoundManager;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a driver supervision device</summary>
	public class DriverSupervisionDevice
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly AbstractCar baseCar;
		/// <summary>The update timer</summary>
		private double Timer;
		/// <summary>The time after which the DSD will intervene</summary>
		private readonly double InterventionTime;
		/// <summary>Whether a DSD intervention has been triggered</summary>
		public bool Triggered;
		/// <summary>The type of device</summary>
		public readonly DriverSupervisionDeviceTypes Type;
		/// <summary>The sound played when the device is triggered</summary>
		public CarSound TriggerSound;
		/// <summary>Whether the alarm is to loop</summary>
		public bool LoopingAlarm;
		/// <summary>The sound played when the device is reset</summary>
		public CarSound ResetSound;

		public DriverSupervisionDevice(AbstractCar Car, DriverSupervisionDeviceTypes type, double interventionTime)
		{
			baseCar = Car;
			Type = type;
			InterventionTime = interventionTime;
			TriggerSound = new CarSound();
			ResetSound = new CarSound();
		}

		public void Update(double TimeElapsed)
		{
			Timer += TimeElapsed;
			if (Timer > InterventionTime && !Triggered)
			{
				Triggered = true;
				TriggerSound.Play(baseCar, LoopingAlarm);
			}
		}

		public void Reset()
		{
			TriggerSound.Stop();
			Timer = 0.0;
			Triggered = false;
		}
	}
}
