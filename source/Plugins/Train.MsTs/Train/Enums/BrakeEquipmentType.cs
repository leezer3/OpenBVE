// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Train.MsTs
{
	enum BrakeEquipmentType
	{
		/// <summary>Handbrake is fitted</summary>
		Handbrake,
		/// <summary>Manual brake fitted.</summary>
		Manual_Brake,
		/// <summary>3 position retaining valve is fitted.</summary>
		Retainer_3_Position,
		/// <summary>4 position retaining valve is fitted</summary>
		/// <remarks>This is meant for freight wagons only</remarks>
		Retainer_4_Position,
		/// <summary>Vacuum brake is fitted.</summary>
		Vacuum_Brake,
		/// <summary>Standard triple valve is fitted.</summary>
		Triple_Valve,
		/// <summary>Triple valve that permits partial releasing of the brakes.</summary>
		Graduated_Release_Triple_Valve,
		/// <summary>Electrically controlled brake system is fitted. Release and application of the brakes are independently controlled.</summary>
		EP_Brake,
		/// <summary>Same functionality as ep_brake</summary>
		ECP_Brake,
		/// <summary>Air tank used for normal service brake applications. This is required for all brake systems.</summary>
		Auxilary_Reservoir,
		/// <summary>Air tank used for emergency applications.</summary>
		/// <remarks>This is optional.</remarks>
		Emergency_Brake_Reservoir,
		/// <summary>Electronic or computer controller on the vehicle that can be set to independently control any parameter of the braking system.</summary>
		Distributor
	}
}
