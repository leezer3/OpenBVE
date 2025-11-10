// ReSharper disable UnusedMember.Global
namespace Formats.OpenBve
{
	public enum SoundCfgSection
	{
		/// <summary>Unknown section</summary>
		Unknown = 0,
		/// <summary>Run sounds</summary>
		Run,
		/// <summary>Flange sounds</summary>
		Flange,
		/// <summary>Motor sounds</summary>
		Motor,
		/// <summary>Switch sounds</summary>
		Switch,
		/// <summary>Brake sounds</summary>
		Brake,
		/// <summary>Compressor sounds</summary>
		Compressor,
		/// <summary>Air suspension sounds</summary>
		Suspension = 7,
		Spring = 7,
		/// <summary>Horn sounds</summary>
		Horn,
		/// <summary>Door open / close sounds</summary>
		Door,
		/// <summary>ATS Plugin sounds</summary>
		ATS = 10,
		Plugin = 10,
		/// <summary>Train ready to start buzzer sound</summary>
		Buzzer,
		/// <summary>Pilot lamp switch sound</summary>
		PilotLamp,
		/// <summary>Sounds played when the brake handle is moved</summary>
		BrakeHandle,
		/// <summary>Sounds played when the power handle is moved</summary>
		MasterController = 14,
		PowerHandle = 14,
		/// <summary>Sounds played when the reverseer is moved</summary>
		Reverser = 15,
		ReverserHandle = 15,
		/// <summary>Breaker sounds</summary>
		Breaker,
		/// <summary>Miscellaneous sounds</summary>
		Others,
		/// <summary>Windscreen / raindrop sounds</summary>
		Windscreen,
		/// <summary>Sounds to be played in a loop during the simulation</summary>
		Loop = 19,
		Noise = 19,
		/// <summary>Front axle point sounds</summary>
		PointFrontAxle = 20,
		SwitchFrontAxle = 20,
		/// <summary>Rear axle point sounds</summary>
		PointRearAxle = 21,
		SwitchRearAxle = 21,
		/// <summary>Sounds relating to the brake shoe rubbing on the wheel</summary>
		Shoe = 22,
		Rub = 22,
		/// <summary>Sounds relating to stop requests</summary>
		RequestStop,
		/// <summary>Sounds relating to panel touch elements</summary>
		Touch,
		/// <summary>Sounds relating to the operation of the sanders</summary>
		Sanders,
		/// <summary>Coupling sounds</summary>
		Coupler,
		/// <summary>Drivers supervision device sounds</summary>
		DriverSupervisionDevice,
		/// <summary>Headlights sounds</summary>
		Headlights,
		/// <summary>Pantograph sounds</summary>
		Pantograph
	}
}
