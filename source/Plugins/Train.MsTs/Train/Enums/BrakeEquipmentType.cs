namespace Train.MsTs
{
	enum BrakeEquipmentType
	{
		/// <summary>Handbrake is fitted</summary>
		handbrake,
		/// <summary>Manual brake fitted.</summary>
		manual_brake,
		/// <summary>3 position retaining valve is fitted.</summary>
		retainer_3_position,
		/// <summary>4 position retaining valve is fitted</summary>
		/// <remarks>This is meant for freight wagons only</remarks>
		retainer_4_position,
		/// <summary>Vacuum brake is fitted.</summary>
		vacuum_brake,
		/// <summary>Standard triple valve is fitted.</summary>
		triple_valve,
		/// <summary>Triple valve that permits partial releasing of the brakes.</summary>
		graduated_release_triple_valve,
		/// <summary>Electrically controlled brake system is fitted. Release and application of the brakes are independently controlled.</summary>
		ep_brake,
		/// <summary>Same functionality as ep_brake</summary>
		ecp_brake,
		/// <summary>Air tank used for normal service brake applications. This is required for all brake systems.</summary>
		auxilary_reservoir,
		/// <summary>Air tank used for emergency applications.</summary>
		/// <remarks>This is optional.</remarks>
		emergency_brake_reservoir,
		/// <summary>Electronic or computer controller on the vehicle that can be set to independently control any parameter of the braking system.</summary>
		distributor
	}
}
