namespace OpenBve
{
	public static partial class TrainManager
	{
		internal class TrailerCar : Car
		{
			internal TrailerCar(Train train, int index) : base(train, index)
			{
			}

			internal override void UpdateAcceleration(double TimeElapsed)
			{
				//Trailer car cannot spin it's wheels
				WheelSpin = 0.0;
				const double a = 0.0;
				FrontAxle.CurrentWheelSlip = false;
				RearAxle.CurrentWheelSlip = false;

				if (!Derailed)
				{
					if (Specs.CurrentAccelerationOutput < a)
					{
						if (Specs.CurrentAccelerationOutput < 0.0)
						{
							Specs.CurrentAccelerationOutput += Specs.JerkBrakeDown * TimeElapsed;
						}
						else
						{
							Specs.CurrentAccelerationOutput += Specs.JerkPowerUp * TimeElapsed;
						}

						if (Specs.CurrentAccelerationOutput > a)
						{
							Specs.CurrentAccelerationOutput = a;
						}
					}
					else
					{
						Specs.CurrentAccelerationOutput -= Specs.JerkPowerDown * TimeElapsed;
						if (Specs.CurrentAccelerationOutput < a)
						{
							Specs.CurrentAccelerationOutput = a;
						}
					}
				}
				else
				{
					Specs.CurrentAccelerationOutput = 0.0;
				}
			}
		}
	}
}
