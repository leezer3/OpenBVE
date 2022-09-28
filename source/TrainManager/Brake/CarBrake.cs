using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public abstract class CarBrake
	{
		public readonly CarBase Car;

		internal const double Tolerance = 5000.0;
		
		/// <summary>Whether this is a main or auxiliary brake system</summary>
		public BrakeType brakeType;

		public EqualizingReservoir equalizingReservoir;

		public MainReservoir mainReservoir;

		public AuxiliaryReservoir auxiliaryReservoir;

		public BrakePipe brakePipe;

		public BrakeCylinder brakeCylinder;

		public Compressor airCompressor;

		internal EletropneumaticBrakeType electropneumaticBrakeType;

		public StraightAirPipe straightAirPipe;

		/// <summary>The speed at which the brake control system activates in m/s</summary>
		public double brakeControlSpeed;

		/// <summary>The current deceleration provided by the electric motor</summary>
		public double motorDeceleration;
		
		/// <summary>The delay between motor deceleration being requested and it activating</summary>
		internal double motorDecelerationDelayUp;

		/// <summary>The delay between motor deceleration stopping and this being reflected</summary>
		internal double motorDecelerationDelayDown;

		/// <summary>Timer used for motor deceleration delay</summary>
		internal double motorDecelerationDelayTimer;

		/// <summary>The last motor deceleration figure returned</summary>
		internal double lastMotorDeceleration;

		/// <summary>The last brake handle position</summary>
		internal int lastHandlePosition;

		/// <summary>The air sound currently playing</summary>
		public CarSound airSound = new CarSound();
		/// <summary>Played when the pressure in the brake cylinder is decreased from a non-high to a non-zero value</summary>
		public CarSound Air = new CarSound();
		/// <summary>Played when the pressure in the brake cylinder is decreased from a high value</summary>
		public CarSound AirHigh = new CarSound();
		/// <summary>Played when the pressure in the brake cylinder is decreased to zero</summary>
		public CarSound AirZero = new CarSound();
		/// <summary>Played when the brake shoe rubs against the wheels</summary>
		public CarSound Rub = new CarSound();
		/// <summary>The sound played when the brakes are released</summary>
		public CarSound Release = new CarSound();

		internal AccelerationCurve[] decelerationCurves;
		/// <summary>A non-negative floating point number representing the jerk in m/s when the deceleration produced by the electric brake is increased.</summary>
		public double JerkUp;
		/// <summary>A non-negative floating point number representing the jerk in m/s when the deceleration produced by the electric brake is decreased.</summary>
		public double JerkDown;
		
		/// <summary>Updates the brake system</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="currentSpeed">The current speed of the train</param>
		/// <param name="brakeHandle">The controlling brake handle (NOTE: May either be the loco brake if fitted or the train brake)</param>
		/// <param name="Deceleration">The deceleration output provided</param>
		public abstract void Update(double TimeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double Deceleration);

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
		public double DecelerationAtServiceMaximumPressure(int Notch, double currentSpeed)
		{
			if (decelerationCurves == null || decelerationCurves.Length == 0)
			{
				return 0;
			}
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

		/// <summary>Gets the current motor deceleration figure</summary>
		/// <param name="TimeElapsed">The time elapsed since the last time this was updated</param>
		/// <param name="BrakeHandle">The controlling brake handle</param>
		public virtual double CurrentMotorDeceleration(double TimeElapsed, AbstractHandle BrakeHandle)
		{
			return motorDeceleration;
		}

		public CarBrake(CarBase car)
		{
			Car = car;
		}
	}
}
