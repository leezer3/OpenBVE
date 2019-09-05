using SoundManager;

namespace TrainEditor2.Simulation.TrainManager
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The set of sounds attached to a car</summary>
		internal struct CarSounds
		{
			internal MotorSound Motor;
			internal CarSound[] Run;
			internal double[] RunVolume;
		}
	}
}
