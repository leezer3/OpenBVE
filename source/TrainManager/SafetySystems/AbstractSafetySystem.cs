using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	public abstract class AbstractSafetySystem
	{
		/// <summary>The base car</summary>
		internal readonly CarBase baseCar;

		protected AbstractSafetySystem(CarBase car)
		{
			baseCar = car;
		}

		/// <summary>Updates the safety system</summary>
		/// <param name="timeElapsed">The elapsed time</param>
		public virtual void Update(double timeElapsed)
		{
		}
	}
}
