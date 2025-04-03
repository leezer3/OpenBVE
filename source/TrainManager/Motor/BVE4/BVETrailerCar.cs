﻿using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVETrailerCar : AbstractEngine
	{
		public BVETrailerCar(CarBase car) : base(car, null, false)
		{
			MaximumAcceleration = 0;
		}

		public override void Update(double timeElapsed)
		{
			// Nothing to do
		}
	}
}
