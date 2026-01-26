using OpenBveApi.Trains;
using SoundManager;

namespace TrainManager.Power
{
	/// <summary>Represents a basic circuit breaker for the electric engine</summary>
	public class Breaker
	{
		/// <summary>Played once when power application is resumed</summary>
		/// <remarks>May be triggered either by driver actions (power, reverser, brake) or similar from a safety system</remarks>
		public CarSound Resume;
		/// <summary>Played once when power application is resumed or interrupted</summary>
		/// <remarks>May be triggered either by driver actions (power, reverser, brake) or similar from a safety system</remarks>
		public CarSound ResumeOrInterrupt;
		/// <summary>Whether the last action of the breaker was to resume power</summary>
		private bool Resumed;
		/// <summary>Holds a reference to the car for sounds playback</summary>
		private readonly AbstractCar Car;

		/// <summary>Creates a new breaker</summary>
		/// <param name="car">The car</param>
		public Breaker(AbstractCar car)
		{
			Resumed = false;
			Car = car;
			Resume = new CarSound();
			ResumeOrInterrupt = new CarSound();
		}

		/// <summary> Updates the breaker</summary>
		/// <param name="BreakerActive">Whether the breaker is currently active</param>
		public void Update(bool BreakerActive)
		{
			if (BreakerActive & !Resumed)
			{
				// resume
				Resume.Play(Car, false);
				ResumeOrInterrupt.Play(Car, false);
				Resumed = true;
			}
			else if (!BreakerActive & Resumed)
			{
				// interrupt
				ResumeOrInterrupt.Play(Car, false);
				Resumed = false;
			}
		}
	}
}
