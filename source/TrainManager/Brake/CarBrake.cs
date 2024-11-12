using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public abstract class CarBrake
	{
		internal const double Tolerance = 5000.0;

		/// <summary>Contains a reference to the base car</summary>
		/// <remarks>Train data should be accessed via this reference</remarks>
		internal readonly CarBase Car;

		/// <summary>Whether this is a main or auxiliary brake system</summary>
		public BrakeType BrakeType;

		public EqualizingReservoir EqualizingReservoir;

		public MainReservoir MainReservoir;

		public AuxiliaryReservoir AuxiliaryReservoir;

		public BrakePipe BrakePipe;

		public BrakeCylinder BrakeCylinder;

		public Compressor AirCompressor;

		internal EletropneumaticBrakeType ElectropneumaticBrakeType;

		public StraightAirPipe StraightAirPipe;
		
		/// <summary>The speed at which the brake control system activates in m/s</summary>
		public double BrakeControlSpeed;

		/// <summary>The current deceleration provided by the electric motor</summary>
		public double MotorDeceleration;
		
		/// <summary>The delay between motor deceleration being requested and it activating</summary>
		internal double MotorDecelerationDelayUp;

		/// <summary>The delay between motor deceleration stopping and this being reflected</summary>
		internal double MotorDecelerationDelayDown;

		/// <summary>Timer used for motor deceleration delay</summary>
		internal double MotorDecelerationDelayTimer;

		/// <summary>The last motor deceleration figure returned</summary>
		internal double LastMotorDeceleration;

		/// <summary>The last brake handle position</summary>
		internal int LastHandlePosition;

		/// <summary>The air sound currently playing</summary>
		public CarSound AirSound = new CarSound();
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

		internal AccelerationCurve[] DecelerationCurves;
		/// <summary>A non-negative floating point number representing the jerk in m/s when the deceleration produced by the electric brake is increased.</summary>
		public double JerkUp;
		/// <summary>A non-negative floating point number representing the jerk in m/s when the deceleration produced by the electric brake is decreased.</summary>
		public double JerkDown;

		protected CarBrake(CarBase car)
		{
			Car = car;
		}
		
		/// <summary>Updates the brake system</summary>
		/// <param name="timeElapsed">The frame time elapsed</param>
		/// <param name="currentSpeed">The current speed of the train</param>
		/// <param name="brakeHandle">The controlling brake handle (NOTE: May either be the loco brake if fitted or the train brake)</param>
		/// <param name="deceleration">The deceleration output provided</param>
		public abstract void Update(double timeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double deceleration);

		internal double GetRate(double ratio, double factor)
		{
			ratio = ratio < 0.0 ? 0.0 : ratio > 1.0 ? 1.0 : ratio;
			ratio = 1.0 - ratio;
			return 1.5 * factor * (1.01 - ratio * ratio);
		}

		/// <summary>Calculates the max possible deceleration given a brake notch and speed</summary>
		/// <param name="notch">The brake notch</param>
		/// <param name="currentSpeed">The speed</param>
		/// <returns>The deceleration in m/s</returns>
		public double DecelerationAtServiceMaximumPressure(int notch, double currentSpeed)
		{
			if (notch == 0)
			{
				return this.DecelerationCurves[0].GetAccelerationOutput(currentSpeed, 1.0);
			}
			if (this.DecelerationCurves.Length >= notch)
			{
				return this.DecelerationCurves[notch - 1].GetAccelerationOutput(currentSpeed, 1.0);
			}
			return this.DecelerationCurves[this.DecelerationCurves.Length - 1].GetAccelerationOutput(currentSpeed, 1.0);
		}

		/// <summary>Gets the current motor deceleration figure</summary>
		/// <param name="timeElapsed">The time elapsed since the last time this was updated</param>
		/// <param name="brakeHandle">The controlling brake handle</param>
		public virtual double CurrentMotorDeceleration(double timeElapsed, AbstractHandle brakeHandle)
		{
			return MotorDeceleration;
		}
	}
}
