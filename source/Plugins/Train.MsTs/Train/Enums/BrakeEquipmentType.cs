﻿// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Train.MsTs
{
	enum BrakeEquipmentType
	{
		/// <summary>Handbrake is fitted</summary>
		Handbrake = 1,
		/// <summary>Manual brake fitted.</summary>
		Manual_Brake = 2,
		/// <summary>3 position retaining valve is fitted.</summary>
		Retainer_3_Position = 3,
		/// <summary>4 position retaining valve is fitted</summary>
		/// <remarks>This is meant for freight wagons only</remarks>
		Retainer_4_Position = 4,
		/// <summary>Twin-pipe vacuum brake is fitted.</summary>
		Vacuum_Brake = 5,
		/// <summary>Single-pipe vacuum brake is fitted.</summary>
		Vacuum_Single_Pipe = 6,
		/// <summary>Standard triple valve is fitted.</summary>
		Triple_Valve = 7,
		/// <summary>Triple valve that permits partial releasing of the brakes.</summary>
		Graduated_Release_Triple_Valve = 8,
		/// <summary>Electrically controlled brake system is fitted. Release and application of the brakes are independently controlled.</summary>
		EP_Brake = 9,
		EP_Brakes = 9,
		/// <summary>Same functionality as ep_brake</summary>
		ECP_Brake = 10,
		/// <summary>Air tank used for normal service brake applications. This is required for all brake systems.</summary>
		Auxiliary_Reservoir = 11,
		Auxilary_Reservoir = 11,
		/// <summary>Air tank used for emergency applications.</summary>
		/// <remarks>This is optional.</remarks>
		Emergency_Brake_Reservoir = 12,
		/// <summary>Electronic or computer controller on the vehicle that can be set to independently control any parameter of the braking system.</summary>
		Distributor = 13,
		/// <summary>One pipe controls and supplies the air brakes.</summary>
		Air_Single_Pipe = 14,
		/// <summary>One pipe for control air, one pipe for supply air</summary>
		Air_Twin_Pipe = 15,
		Air_Brake = 15
	}
}
