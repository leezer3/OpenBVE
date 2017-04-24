namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the brake cylinder of an air-brake system</summary>
		internal class BrakeCylinder
		{
			/// <summary>The current pressure in Pa</summary>
			internal double CurrentPressure;
			/// <summary>The maximum pressure when in the EB brake notch</summary>
			internal double EmergencyMaximumPressure;
			/// <summary>The maximum pressure when in the highest service brake notch</summary>
			internal double ServiceMaximumPressure;
			/// <summary>The rate at which the brake cylinder charges when in the EB notch</summary>
			internal double EmergencyChargeRate;
			/// <summary>The rate at which the brake cylinder charges when in the highest service brake notch</summary>
			internal double ServiceChargeRate;
			/// <summary>The rate at which the brake cylinder releases</summary>
			internal double ReleaseRate;
			/// <summary>The pressure related sound to be played</summary>
			internal double SoundPlayedForPressure;
		}
	}
}
