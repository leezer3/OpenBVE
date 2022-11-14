using System;

namespace TrainManager.TractionModels.Steam
{
	/// <summary>Represents the valve gear and related properties</summary>
	public class ValveGear
	{
		/// <summary>Holds a reference to the base engine</summary>
		public readonly SteamEngine Engine;
		/// <summary>The circumference of the wheel</summary>
		private readonly double wheelCircumference;
		/// <summary>The current rotational position of the wheel</summary>
		public double WheelPosition;
		/// <summary>The connecting rod</summary>
		internal ValveGearRod ConnectingRod;
		/// <summary>The crank rod</summary>
		internal ValveGearRod CrankRod;

		public ValveGear(SteamEngine engine)
		{
			Engine = engine;
			wheelCircumference = Engine.Car.FrontAxle.WheelRadius * 2 * Math.PI;
		}

		private double previousLocation;

		internal void Update()
		{
			double distanceTravelled = Engine.Car.TrackPosition - previousLocation;
			previousLocation = Engine.Car.TrackPosition;

			double percentageRotated = (distanceTravelled / wheelCircumference) * 0.35;
			if (Math.Abs(percentageRotated) > 100)
			{
				percentageRotated = 0;
			}

			if (WheelPosition - percentageRotated <= 100 && WheelPosition - percentageRotated >= 0)
			{
				WheelPosition -= percentageRotated;
			}
			else
			{
				WheelPosition = 100 - percentageRotated;
			}
			
		}
	}
}
