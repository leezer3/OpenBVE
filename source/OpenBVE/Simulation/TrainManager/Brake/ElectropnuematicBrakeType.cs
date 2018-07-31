namespace OpenBve.BrakeSystems
{
	/// <summary>The types of electropnuematic control system available for a train with dynamic (motor assisted) braking</summary>
	internal enum EletropneumaticBrakeType
	{
		/// <summary>The motor will always brake in addition to the physical brake, leading to a stronger braking force at all times.</summary>
		None = 0,
		/// <summary>The pressure to the brake cylinder is interrupted when the train travels above the brake control speed, and the electric brake is used instead.
		/// When below the control speed, the physical brake operates normally, while the electric brake is not used.
		/// </summary>
		ClosingElectromagneticValve = 1,
		/// <summary>The motor is used to brake the train with the deceleration setting as specified in the Performance section.
		/// If the motor cannot provide this deceleration on its own, the physical brake is additionally used.
		/// When the train travels below the brake control speed, the physical brakes are used.
		/// However, as the physical brakes need time to fill the brake cylinder, the electric brake is still used to compensate for this delay.
		/// </summary>
		DelayFillingControl = 2
	}
}
