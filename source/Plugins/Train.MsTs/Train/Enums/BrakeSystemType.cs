namespace Train.MsTs
{
	enum BrakeSystemType
	{
		/// <summary>One pipe controls and supplies the air brakes.</summary>
		Air_single_pipe,
		/// <summary>Two pipes are used. One to control the brakes, the other to charge the reserviors.</summary>
		Air_twin_pipe,
		/// <summary>The car uses a manual braking system.</summary>
		Manual_Braking,
		/// <summary>One pipe is used to supply and control the vacuum brakes.</summary>
		Vacuum_single_pipe,
		/// <summary>Two pipes are used. One controls the vacuum brakes, the other supply the vacuum reservior.</summary>
		Vacuum_twin_pipe,
		/// <summary>The brakes are controlled by a computer or complex electrical control system.</summary>
		ECP,
		/// <summary>The brakes are a combination of standard air brakes and electrical control signals.</summary>
		EP,
		/// <summary>The vehicle has no brakes</summary>
		/// <remarks>Air pipe pressure will be passed through this vehicle</remarks>
		Air_piped,
		/// <summary>The vehicle has no brakes</summary>
		/// <remarks>Vacuum pipe pressure will be passed through this vehicle</remarks>
		Vacuum_piped
	}
}
