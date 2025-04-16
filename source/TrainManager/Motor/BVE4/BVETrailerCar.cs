using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVETrailerCar : TractionModel
	{
		public BVETrailerCar(CarBase car) : base(car, null, false)
		{
			MaximumPossibleAcceleration = 0;
		}

		public override void Update(double timeElapsed)
		{
			// Nothing to do
		}
	}
}
