using OpenBveApi.Math;
using SoundManager;

namespace TrainEditor2.Simulation.TrainManager
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal struct MotorSoundTableEntry
		{
			internal SoundBuffer Buffer;
			internal int SoundIndex;
			internal float Pitch;
			internal float Gain;
		}

		internal struct MotorSoundTable
		{
			internal MotorSoundTableEntry[] Entries;
			internal SoundBuffer Buffer;
			internal SoundSource Source;
		}

		internal struct MotorSound
		{
			internal MotorSoundTable[] Tables;
			internal Vector3 Position;
			internal double SpeedConversionFactor;
			internal int CurrentAccelerationDirection;
			internal const int MotorP1 = 0;
			internal const int MotorP2 = 1;
			internal const int MotorB1 = 2;
			internal const int MotorB2 = 3;
		}
	}
}
