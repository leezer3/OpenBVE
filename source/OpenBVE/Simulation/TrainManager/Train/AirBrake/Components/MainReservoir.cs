namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the main reservoir of an air-brake system</summary>
		internal class MainReservoir
		{
			/// <summary>The current air pressure in this reservoir</summary>
			internal double CurrentPressure;
			/// <summary>The coefficient governing pressure transfer to the equalizing reservoir</summary>
			internal double EqualizingReservoirCoefficient;
			/// <summary>The coefficient governing pressure transfer to the brake pipe</summary>
			internal double BrakePipeCoefficient;
			/// <summary>The parent air-brake</summary>
			private readonly CarAirBrake AirBrake;

			/// <summary>Creates a new main reservoir</summary>
			/// <param name="airBrake">The parent air-brake</param>
			internal MainReservoir(CarAirBrake airBrake)
			{
				this.CurrentPressure = 0.0;
				this.EqualizingReservoirCoefficient = 0.0;
				this.BrakePipeCoefficient = 0.0;
				this.AirBrake = airBrake;
			}

			/// <summary>Updates the pressure of the brake pipe from this reservoir</summary>
			/// <param name="Train">The train</param>
			/// <param name="CarIndex">The car index</param>
			/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
			internal void UpdateBrakePipe(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				if (AirBrake.BrakePipe.CurrentPressure > AirBrake.EqualizingReservoir.CurrentPressure + CarAirBrake.Tolerance)
				{
					// brake pipe exhaust valve
					double r = Train.Specs.CurrentEmergencyBrake.Actual ? AirBrake.BrakePipe.EmergencyRate : AirBrake.BrakePipe.ServiceRate;
					double d = AirBrake.BrakePipe.CurrentPressure - AirBrake.EqualizingReservoir.CurrentPressure;
					double m = AirBrake.EqualizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					AirBrake.BrakePipe.CurrentPressure -= r;
				}
				else if (AirBrake.BrakePipe.CurrentPressure + CarAirBrake.Tolerance < AirBrake.EqualizingReservoir.CurrentPressure)
				{
					// fill brake pipe from main reservoir
					double r = AirBrake.BrakePipe.ChargeRate;
					double d = AirBrake.EqualizingReservoir.CurrentPressure - AirBrake.BrakePipe.CurrentPressure;
					double m = AirBrake.EqualizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					d = AirBrake.BrakePipe.NormalPressure - AirBrake.BrakePipe.CurrentPressure;
					if (r > d) r = d;
					double f = BrakePipeCoefficient;
					double s = r * f;
					if (s > CurrentPressure)
					{
						r *= CurrentPressure / s;
						s = CurrentPressure;
					}
					AirBrake.BrakePipe.CurrentPressure += 0.5 * r;
					CurrentPressure -= 0.5 * s;
				}
			}
		}
	}
}
