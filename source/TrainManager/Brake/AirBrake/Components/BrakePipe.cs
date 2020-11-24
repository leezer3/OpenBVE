namespace TrainManager.BrakeSystems
{
	/// <summary>A brake pipe</summary>
	public class BrakePipe
	{
		/// <summary>The current pressure</summary>
		public double CurrentPressure;
		/// <summary>The normal working pressure</summary>
		public readonly double NormalPressure;
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The rate in Pa/s when making a service brake application</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate in Pa/s when making an EB brake application</summary>
		internal readonly double EmergencyRate;
		/// <summary>The number of pascals leaked by the brake pipe each second</summary>
		public readonly double LeakRate = 500000.0;

		public BrakePipe(double normalPressure, double chargeRate, double serviceRate, double emergencyRate, bool electricCommand)
		{
			ChargeRate = chargeRate;
			ServiceRate = serviceRate;
			NormalPressure = normalPressure;
			EmergencyRate = emergencyRate;
			CurrentPressure = electricCommand ? 0.0 : normalPressure;
		}
	}
}
