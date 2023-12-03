namespace TrainManager.Brake
{
	/// <summary>Describes the behaviour of the brake system when uncoupled</summary>
	public enum UncouplingBehaviour
	{
		/// <summary>Emergency brake is applied to both consists</summary>
		Emergency,
		/// <summary>Emergency brake is applied to the player consist</summary>
		/// <remarks>The uncoupled consist will retain the previous brake setting</remarks>
		EmergencyPlayer,
		/// <summary>Emergency brake is applied to the uncoupled consist</summary>
		/// <remarks>The player consist will retain the previous brake setting</remarks>
		EmergencyUncoupledConsist,
		/// <summary>The brake is released on the uncoupled consist</summary>
		/// <remarks>The player consist will retain the previous brake setting</remarks>
		ReleasedUncoupledConsist,
		/// <summary>The brakes are released on both consists</summary>
		Released
	}
}
