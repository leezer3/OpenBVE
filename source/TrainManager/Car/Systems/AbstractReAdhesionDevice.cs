namespace TrainManager.Car.Systems
{
	/// <summary>An abstract readhesion device</summary>
	public abstract class AbstractReAdhesionDevice
	{
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase Car;

		internal AbstractReAdhesionDevice(CarBase car)
		{
			Car = car;
		}

		/// <summary>Called once a frame to update the re-adhesion device when powering</summary>
		/// <param name="TimeElapsed">The elapsed time</param>
		public virtual void Update(double TimeElapsed)
		{

		}

		/// <summary>Called to reset the readhesion device after a jump</summary>
		public virtual void Jump()
		{

		}
	}
}
