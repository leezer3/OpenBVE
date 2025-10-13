namespace TrainManager.BrakeSystems
{
	/// <summary>A brake pipe</summary>
	public class BrakePipe
	{
		/// <summary>The current pressure in Pa</summary>
		public double CurrentPressure;
		/// <summary>The normal working pressure in Pa</summary>
		public readonly double NormalPressure;
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The rate in Pa/s when making a service brake application</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate in Pa/s when making an EB brake application</summary>
		internal readonly double EmergencyRate;
		/// <summary>The number of pascals leaked by the brake pipe each second</summary>
		public readonly double LeakRate = 500000.0;
		/// <summary>The volume of the brake pipe in m³</summary>
		public double Volume = 0;

		/// <summary>Creates a functional brake pipe</summary>
		public BrakePipe(double normalPressure, double chargeRate, double serviceRate, double emergencyRate, bool electricCommand)
		{
			ChargeRate = chargeRate;
			ServiceRate = serviceRate;
			NormalPressure = normalPressure;
			EmergencyRate = emergencyRate;
			CurrentPressure = electricCommand ? 0.0 : normalPressure;
		}

		/// <summary>Creates a dummy brake pipe</summary>
		public BrakePipe(double pressure)
		{
			ChargeRate = 0.0;
			ServiceRate = 0.0;
			NormalPressure = pressure;
			EmergencyRate = 0.0;
			CurrentPressure = pressure;
		}
	}
}
