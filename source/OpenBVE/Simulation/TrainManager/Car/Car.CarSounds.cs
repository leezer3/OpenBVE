using SoundManager;
using TrainManager.Motor;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The set of sounds attached to a car</summary>
		internal struct CarSounds
		{
			internal BVEMotorSound Motor;
			internal CarSound[] Flange;
			internal double[] FlangeVolume;

			internal CarSound Loop;
			internal CarSound[] Run;
			internal double[] RunVolume;
			internal double RunNextReasynchronizationPosition;
			internal CarSound SpringL;
			internal CarSound SpringR;
			internal CarSound[] Plugin;
			internal CarSound[] RequestStop;
			internal double FlangePitch;
			internal double SpringPlayedAngle;
			internal CarSound[] Touch;
		}
	}
}
