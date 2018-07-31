namespace OpenBve.BrakeSystems
{
	/// <summary>A straight air pipe</summary>
	class StraightAirPipe
	{
		/// <summary>The current pressure</summary>
		internal double CurrentPressure;
		/// <summary>The rate in Pa/s when making a service brake application</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate in Pa/s when making an EB application</summary>
		internal readonly double EmergencyRate;
		/// <summary>The release rate in Pa/s</summary>
		internal readonly double ReleaseRate;

		internal StraightAirPipe(double serviceRate, double emergencyRate, double releaseRate)
		{
			CurrentPressure = 0.0;
			ServiceRate = serviceRate;
			EmergencyRate = emergencyRate;
			ReleaseRate = releaseRate;
		}
		
	}
}
