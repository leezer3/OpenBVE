using OpenBveApi.Interface;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents an abstract safety system fitted to a train</summary>
	public abstract class AbstractSafetySystem
	{
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase baseCar;
		/// <summary>The type of device</summary>
		internal readonly InterventionMode Type;
		/// <summary>The sound played when the device is triggered</summary>
		public CarSound TriggerSound;
		/// <summary>Whether the alarm is to loop</summary>
		internal readonly bool LoopingAlarm;
		/// <summary>The sound played when the device is reset</summary>
		public CarSound ResetSound;
		/// <summary>The required stop time to reset the device</summary>
		internal readonly double RequiredStopTime;
		/// <summary>The time after which the device will intervene</summary>
		internal readonly double InterventionTime;
		/// <summary>Whether a device intervention has been triggered</summary>
		public bool Triggered;
		/// <summary>The update timer</summary>
		internal double Timer;
		/// <summary>The stop timer</summary>
		internal double StopTimer;

		internal AbstractSafetySystem(CarBase car, InterventionMode type, bool loopingAlarm, double interventionTime, double requiredStopTime)
		{
			baseCar = car;
			Type = type;
			LoopingAlarm = loopingAlarm;
			InterventionTime = interventionTime;
			RequiredStopTime = requiredStopTime;
		}

		/// <summary>Updates the safety system</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		public abstract void Update(double TimeElapsed);

		/// <summary>Method called when a control is pressed</summary>
		/// <param name="Control">The control</param>
		public virtual void ControlDown(Translations.Command Control)
		{

		}

		/// <summary>Method called when a control is released</summary>
		/// <param name="Control">The control</param>
		public virtual void ControlUp(Translations.Command Control)
		{

		}
	}
}
