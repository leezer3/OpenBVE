using TrainManager.Car;

namespace TrainManager.Motor
{
	public class Bve5TrailerCar : TractionModel
	{
		public Bve5TrailerCar(CarBase car) : base(car)
		{
		}

		public override void Update(double timeElapsed)
		{
			// Nothing to do
		}
	}
}
