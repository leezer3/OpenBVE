using OpenBveApi.Math;

namespace OpenBve.BrakeSystems
{
	/// <summary>Defines an air-brake</summary>
	public partial class AirBrake
	{
		/// <summary>The root abstract class defining a car air-brake system</summary>
		internal abstract class CarAirBrake
		{
			/// <summary>The type of air brake</summary>
			internal BrakeType Type;
			/// <summary>The compressor, or a null reference if not fitted</summary>
			internal AirCompressor Compressor;
			/// <summary>The main reservoir, or a null reference if not fitted</summary>
			internal MainReservoir MainReservoir;
			/// <summary>The equalizing reservoir, or a null reference if not fitted</summary>
			internal EqualizingReservior EqualizingReservoir;
			/// <summary>The brake pipe</summary>
			internal BrakePipe BrakePipe;
			/// <summary>The straight air pipe</summary>
			internal StraightAirPipe StraightAirPipe;
			/// <summary>The auxilary reservoir, or a null reference if not fitted</summary>
			internal AuxillaryReservoir AuxillaryReservoir;
			/// <summary>The brake cylinder, or a null reference if not fitted</summary>
			internal BrakeCylinder BrakeCylinder;
			/// <summary>The sound played when the brake cylinder pressure reaches zero</summary>
			internal Sounds.SoundBuffer AirZero;
			/// <summary>The sound played when the brake cylinder pressure decreases from normal pressure</summary>
			internal Sounds.SoundBuffer AirNormal;
			/// <summary>The sound played when the brake cylinder pressure decreases from high pressure</summary>
			internal Sounds.SoundBuffer AirHigh;
			/// <summary>The position of the air sound within the car</summary>
			internal Vector3 AirSoundPosition;
			/// <summary>The current Air Sound to be played</summary>
			internal AirSound AirSound = AirSound.None;
			/// <summary>The deceleration this brake system provides at maximum service pressure</summary>
			internal double DecelerationAtServiceMaximumPressure;
			/// <summary>The speed at which the electric regenerative brake behaviour changes (Dependant on brake type)</summary>
			internal double ControlSpeed;

			internal const double Tolerance = 5000.0;

			internal void UpdateSystem(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				//If we are in a car with a compressor & equalizing reservoir, update them
				if (Type == BrakeType.Main)
				{
					Train.Cars[CarIndex].Specs.AirBrake.Compressor.Update(Train, CarIndex, TimeElapsed);
					Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoir.Update(Train, CarIndex, TimeElapsed);
				}
				AirSound = AirSound.None;
				//Update the abstract brake system method
				Update(Train, CarIndex, TimeElapsed);
				//Finally, play the air sound if appropriate
				switch (AirSound)
				{
					case AirSound.Zero:
						if (AirZero != null)
						{
							Sounds.PlaySound(AirZero, 1.0, 1.0, AirSoundPosition, Train, CarIndex, false);
						}
						break;
					case AirSound.Normal:
						if (AirNormal != null)
						{
							Sounds.PlaySound(AirNormal, 1.0, 1.0, AirSoundPosition, Train, CarIndex, false);
						}
						break;
					case AirSound.High:
						if (AirHigh != null)
						{
							Sounds.PlaySound(AirHigh, 1.0, 1.0, AirSoundPosition, Train, CarIndex, false);
						}
						break;
				}
			}

			internal abstract void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed);
		}

		private static double GetRate(double Ratio, double Factor)
		{
			Ratio = Ratio < 0.0 ? 0.0 : Ratio > 1.0 ? 1.0 : Ratio;
			Ratio = 1.0 - Ratio;
			return 1.5 * Factor * (1.01 - Ratio * Ratio);
		}
	}
}
