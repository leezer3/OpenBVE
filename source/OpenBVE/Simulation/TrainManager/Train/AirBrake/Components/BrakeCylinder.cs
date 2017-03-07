namespace OpenBve
{
	public static partial class TrainManager
	{
		internal class BrakeCylinder
		{
			internal double CurrentPressure;
			internal double EmergencyMaximumPressure;
			internal double ServiceMaximumPressure;
			internal double EmergencyChargeRate;
			internal double ServiceChargeRate;
			internal double ReleaseRate;
			internal double SoundPlayedForPressure;
		}
	}
}
