namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the brake pipe of an air-brake system</summary>
		internal class BrakePipe
		{
			/// <summary>The current pressure in Pa</summary>
			internal double CurrentPressure;
			/// <summary>The normal pressure in Pa</summary>
			internal double NormalPressure;
			/// <summary>The rate at which pressure passes through this pipe in Pa per second</summary>
			internal double FlowSpeed;
			/// <summary>The rate at which the pipe recharges from the reservoir in Pa per second</summary>
			internal double ChargeRate;
			/// <summary>The pressure exhaust rate when using service brakes</summary>
			internal double ServiceRate;
			/// <summary>The pressure exhaust rate when using EB</summary>
			internal double EmergencyRate;
		}
	}
}
