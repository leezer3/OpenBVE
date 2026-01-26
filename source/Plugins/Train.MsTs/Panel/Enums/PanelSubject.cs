// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
namespace Train.MsTs
{
	internal enum PanelSubject
	{
		Accelerometer,
		Alerter_Display,
		Ammeter,
		Aspect_Display,
		AWS,
		Bell,
		Blower,
		Brake_Cyl,
		Brake_Pipe,
		Cab_Radio,
		Clock,
		CP_Handle,
		CPH_Display,
		Cutoff,
		Cyl_Cocks,
		Dampers_Back,
		Dampers_Front,
		Direction,
		Direction_Display,
		Dynamic_Brake,
		Dynamic_Brake_Display,
		Engine_Brake,
		Engine_Braking_Button,
		Emergency_Brake,
		Eq_Res,
		Firebox,
		Firehole,
		Friction_Braking,
		Front_Hlight,
		Fuel_Gauge,
		Gears,
		Horn,
		Load_Meter,
		Line_Voltage,
		Main_Res,
		Overspeed,
		Pantograph,
		Panto_Display,
		Penalty_App,
		Regulator,
		Reset,
		Reverser_Plate,
		Sanders,
		Sanding,
		Small_Ejector,
		Speedlim_Display,
		Speedometer,
		Steamchest_Pr,
		Steamheat_Pressure,
		Steam_Inj1,
		Steam_Inj2,
		Steam_Pr,
		Tender_Water,
		Boiler_Water,
		Throttle,
		Throttle_Display,
		Traction_Braking,
		Train_Brake,
		Vacuum_Reservoir_Pressure,
		Water_Injector1,
		Water_Injector2,
		Wheelslip,
		Whistle,
		Wipers,

		// From MSTSBin v1.7 documentation
		Ammeter_Abs,
		Doors_Display,
		Pantograph2,    // pantograph2 state [need to dig into this, but I think Pantograph2 was 'broken' until BIN patch, although appears in original GLOBAL folder defines and some default stock]
		Pantographs_4c, // 4-state combined controller for pantograph 1+2
		Pantographs_4,  // with end position
		Pantographs_5,  // 5-state combined controller for pantograph 1+2
		RPM,
		Speed_Projected, // projected speed in one minute
		SpeedLimit, // signal limit, not track limit

		// probably MSTSBin
		Dynamic_Brake_Force,
	}
}
