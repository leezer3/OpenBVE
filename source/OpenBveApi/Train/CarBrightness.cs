namespace OpenBveApi.Trains
{
	/// <summary>Contains a set of brightness properties to be interpolated</summary>
	public class Brightness
	{
		/// <summary>Holds a reference to the containing car</summary>
		private readonly AbstractCar Car;
		/// <summary>The previous brightness to be interpolated from</summary>
		public float PreviousBrightness;
		/// <summary>The track position to interpolate from</summary>
		public double PreviousTrackPosition;
		/// <summary>The next brightness to interpolate to</summary>
		public float NextBrightness;
		/// <summary>The track position to interpolate to</summary>
		public double NextTrackPosition;

		/// <summary>Creates a new brightness properties class</summary>
		public Brightness(AbstractCar car)
		{
			Car = car;
		}

		/// <summary>Gets the current brightness</summary>
		/// <param name="CabBrightness">The current cab brightness</param>
		/// <returns>The current brightness value</returns>
		public float CurrentBrightness(double CabBrightness)
		{
			float b = (float) (Car.Brightness.NextTrackPosition - Car.Brightness.PreviousTrackPosition);
			//1.0f represents a route brightness value of 255
			//0.0f represents a route brightness value of 0
			if (b != 0.0f)
			{
				b = (float) (Car.RearAxle.Follower.TrackPosition - Car.Brightness.PreviousTrackPosition) / b;
				if (b < 0.0f) b = 0.0f;
				if (b > 1.0f) b = 1.0f;
				b = Car.Brightness.PreviousBrightness * (1.0f - b) + Car.Brightness.NextBrightness * b;
			}
			else
			{
				b = Car.Brightness.PreviousBrightness;
			}

			//Calculate the cab brightness
			double ccb = System.Math.Round(255.0 * (1.0 - b));
			//DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
			return (float) System.Math.Min(CabBrightness, ccb);
		}
	}
}
