using OpenBveApi.Routes;
using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// axle
		internal class Axle
		{
			internal TrackFollower Follower;
			internal bool CurrentWheelSlip;
			internal double Position;
			internal CarSound[] PointSounds;
#pragma warning disable 649
			//set by dynamic elsewhere
			internal int FlangeIndex;
			internal int RunIndex;
#pragma warning restore 649
			/// <summary>Stores whether the point sound has been triggered</summary>
			internal bool PointSoundTriggered;

			private readonly Car baseCar;

			private readonly double coefficientOfFriction;

			internal Axle(TrainManager.Train Train, TrainManager.Car Car, double CoefficientOfFriction)
			{
				Follower = new TrackFollower(Program.CurrentHost, Train, Car);
				baseCar = Car;
				coefficientOfFriction = CoefficientOfFriction;
			}

			internal double GetResistance(Train Train, double Speed)
			{
				double t;
				if (baseCar.Index == 0 & baseCar.CurrentSpeed >= 0.0 || baseCar.Index == Train.Cars.Length - 1 & baseCar.CurrentSpeed <= 0.0)
				{
					t = baseCar.Specs.ExposedFrontalArea;
				}
				else
				{
					t = baseCar.Specs.UnexposedFrontalArea;
				}
				double f = t * baseCar.Specs.AerodynamicDragCoefficient * Train.Specs.CurrentAirDensity / (2.0 * baseCar.CurrentMass);
				double a = Program.CurrentRoute.Atmosphere.AccelerationDueToGravity * baseCar.Specs.CoefficientOfRollingResistance + f * Speed * Speed;
				return a;
			}

			internal double CriticalWheelSlipAccelerationForElectricMotor()
			{
				double NormalForceAcceleration = Follower.WorldUp.Y * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
				// TODO: Implement formula that depends on speed here.
				return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
			}
			internal double CriticalWheelSlipAccelerationForFrictionBrake()
			{
				double NormalForceAcceleration = Follower.WorldUp.Y * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
				// TODO: Implement formula that depends on speed here.
				return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
			}
		}
	}
}
