namespace OpenBve.BrakeSystems
{
	abstract class CarBrake
	{
		internal const double Tolerance = 5000.0;

		/// <summary>Contains a reference to the EB handle of the controlling train</summary>
		internal TrainManager.EmergencyHandle emergencyHandle;

		/// <summary>Contains a reference to the reverser handle of the controlling train</summary>
		internal TrainManager.ReverserHandle reverserHandle;

		/// <summary>Whether this is a main or auxiliary brake system</summary>
		internal BrakeType brakeType;

		internal EqualizingReservoir equalizingReservoir;

		internal MainReservoir mainReservoir;

		internal AuxiliaryReservoir auxiliaryReservoir;

		internal BrakePipe brakePipe;

		internal BrakeCylinder brakeCylinder;

		internal Compressor airCompressor;

		internal EletropneumaticBrakeType electropneumaticBrakeType;

		internal StraightAirPipe straightAirPipe;

		/// <summary>Stores whether the car is a motor car</summary>
		internal bool isMotorCar;

		/// <summary>The speed at which the brake control system activates in m/s</summary>
		internal double brakeControlSpeed;

		/// <summary>The current deceleration provided by the electric motor</summary>
		internal double motorDeceleration;

		/// <summary>The index of the air sound currently playing</summary>
		internal AirSound airSound;

		internal TrainManager.AccelerationCurve[] decelerationCurves;

		/// <summary>Updates the brake system</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="currentSpeed">The current speed of the train</param>
		/// <param name="brakeHandle">The controlling brake handle (NOTE: May either be the loco brake if fitted or the train brake)</param>
		/// <param name="Deceleration">The deceleration output provided</param>
		internal abstract void Update(double TimeElapsed, double currentSpeed, TrainManager.AbstractHandle brakeHandle, out double Deceleration);

		internal double GetRate(double Ratio, double Factor)
		{
			Ratio = Ratio < 0.0 ? 0.0 : Ratio > 1.0 ? 1.0 : Ratio;
			Ratio = 1.0 - Ratio;
			return 1.5 * Factor * (1.01 - Ratio * Ratio);
		}

		/// <summary>Calculates the max possible deceleration given a brake notch and speed</summary>
		/// <param name="Notch">The brake notch</param>
		/// <param name="currentSpeed">The speed</param>
		/// <returns>The deceleration in m/s</returns>
		internal double DecelerationAtServiceMaximumPressure(int Notch, double currentSpeed)
		{
			if (Notch == 0)
			{
				return this.decelerationCurves[0].GetAccelerationOutput(currentSpeed, 1.0);
			}
			if (this.decelerationCurves.Length >= Notch)
			{
				return this.decelerationCurves[Notch - 1].GetAccelerationOutput(currentSpeed, 1.0);
			}
			return this.decelerationCurves[this.decelerationCurves.Length - 1].GetAccelerationOutput(currentSpeed, 1.0);
		}
	}
}
