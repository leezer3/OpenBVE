using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class ThroughPiped : CarBrake
	{
		public ThroughPiped(CarBase car) : base(car)
		{
			DecelerationCurves = new AccelerationCurve[] { };
			BrakeType = BrakeType.None;
		}

		public override void Update(double timeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double deceleration)
		{
			deceleration = 0.0;
		}
	}
}
