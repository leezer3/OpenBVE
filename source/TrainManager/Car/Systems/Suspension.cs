using System;
using SoundManager;

namespace TrainManager.Car.Systems
{
	public class Suspension
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;
		/// <summary>The left spring sound</summary>
		public CarSound SpringL;
		/// <summary>The right spring sound</summary>
		public CarSound SpringR;
		/// <summary>Holds the timer for the spring sounds</summary>
		private double springTimer;
		/// <summary>The angle used to determine whether the spring sounds are to be played</summary>
		private double SpringPlayedAngle;

		public Suspension(CarBase car)
		{
			baseCar = car;
			SpringPlayedAngle = 0;
		}

		public void Update(double TimeElapsed)
		{
			double a = baseCar.Specs.RollDueToShakingAngle;
			double diff = a - SpringPlayedAngle;
			const double angleTolerance = 0.001;

			if (Math.Abs(diff) < angleTolerance && springTimer > 1.0)
			{
				// Absolute difference in angle shows car has not moved in the last second, so pause both sounds
				SpringL?.Pause();
				SpringR?.Pause();
				springTimer = 0;
			}

			if (diff < -angleTolerance)
			{
				// Pause the opposite spring sound unconditionally
				SpringR?.Pause();

				if (SpringL != null && !SpringL.IsPlaying)
				{
					SpringL.Play(baseCar, true);
				}
				SpringPlayedAngle = a;
				springTimer = 0;
			}
			else if (diff > angleTolerance)
			{
				// Pause the opposite spring sound unconditionally
				SpringL?.Pause();

				if (SpringR != null && !SpringR.IsPlaying)
				{
					SpringR.Play(baseCar, true);
				}
				SpringPlayedAngle = a;
				springTimer = 0;
			}
			springTimer += TimeElapsed;
		}
	}
}
