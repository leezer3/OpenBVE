using TrainManager.Car;
using TrainManager.Power;

namespace TrainManager.Motor
{
	public class BVEMotorCar : AbstractEngine
	{
		public BVEMotorCar(CarBase car, AccelerationCurve[] accelerationCurves) : base(car, accelerationCurves, true)
		{
		}

		public override void Update(double timeElapsed)
		{
			MotorSounds.Update(timeElapsed);
		}
	}
}
