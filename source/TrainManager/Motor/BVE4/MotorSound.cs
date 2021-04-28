using OpenBveApi.Math;

namespace TrainManager.Motor
{
	public struct BVEMotorSound
	{
		/// <summary>The motor sound tables</summary>
		public BVEMotorSoundTable[] Tables;
		/// <summary>The position of the sound</summary>
		public Vector3 Position;
		/// <summary>The speed conversion factor</summary>
		public double SpeedConversionFactor;
		/// <summary>The current direction of acceleration of the train</summary>
		public int CurrentAccelerationDirection;
		/*
		 * BVE2 / BVE4 magic numbers
		 */
		public const int MotorP1 = 0;
		public const int MotorP2 = 1;
		public const int MotorB1 = 2;
		public const int MotorB2 = 3;
	}
}
