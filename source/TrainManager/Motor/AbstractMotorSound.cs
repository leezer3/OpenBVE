using OpenBveApi.Math;
using TrainManager.Car;

namespace TrainManager.Motor
{
	/// <summary>An abstract motor sound</summary>
	public abstract class AbstractMotorSound
	{
		/// <summary>The position of the sound</summary>
		public Vector3 Position;
		/// <summary>Holds a reference to the car</summary>
		public readonly CarBase Car;

		protected AbstractMotorSound(CarBase car)
		{
			Car = car;
		}

		/// <summary>Updates the motor sounds</summary>
		public virtual void Update(double timeElapsed)
		{

		}
	}
}
