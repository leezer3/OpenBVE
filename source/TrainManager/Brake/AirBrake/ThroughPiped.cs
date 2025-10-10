using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class ThroughPiped : CarBrake
	{
		public ThroughPiped(CarBase car) : base(car)
		{
			decelerationCurves = new AccelerationCurve[] { };
			brakeType = BrakeType.None;
			brakePipe = new BrakePipe(0);
		}

		public override void Update(double TimeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double Deceleration)
		{
			Deceleration = 0.0;
		}
	}
}
