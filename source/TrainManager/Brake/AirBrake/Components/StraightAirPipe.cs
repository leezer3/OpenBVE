namespace TrainManager.BrakeSystems
{
	/// <summary>A straight air pipe</summary>
	public class StraightAirPipe
	{
		/// <summary>The current pressure in Pa</summary>
		public double CurrentPressure;
		/// <summary>The rate in Pa/s when making a service brake application</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate in Pa/s when making an EB application</summary>
		internal readonly double EmergencyRate;
		/// <summary>The release rate in Pa/s</summary>
		internal readonly double ReleaseRate;

		/// <summary>Creates a functional straight air pipe</summary>
		public StraightAirPipe(double serviceRate, double emergencyRate, double releaseRate)
		{
			CurrentPressure = 0.0;
			ServiceRate = serviceRate;
			EmergencyRate = emergencyRate;
			ReleaseRate = releaseRate;
		}

		/// <summary>Creates a dummy straight air pipe</summary>
		public StraightAirPipe(double pressure)
		{
			CurrentPressure = pressure;
			ServiceRate = 0.0;
			EmergencyRate = 0.0;
			ReleaseRate = 0.0;
		}
		
	}
}
