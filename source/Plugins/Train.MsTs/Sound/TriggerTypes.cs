// ReSharper disable UnusedMember.Global
namespace Train.MsTs
{
    internal enum SoundTrigger
	{
		// -1 appears to be used to skip the sound
		// unclear as to why it's not just left out, copy + paste or internal MSTS validation?
		// some of the time, these exist, but a lot of the time the sound file is missing
		Skip = -1,
		VariableControlled = 0,
		DynamicBrakeIncrease = 2,
		DynamicBrakeOff = 3,
		SanderOn = 4,
		SanderOff = 5,
		WiperOn = 6,
		WiperOff = 7,
		HornOn= 8,
		HornOff = 9,
		BellOn = 10,
		BellOff = 11,
		CompressorOn = 12,
		CompressorOff = 13,
		TrainBrakePressureIncrease = 14,
		ReverserChange = 15,
		ThrottleChange = 16,
		TrainBrakeChange = 17,
		EngineBrakeChange = 18,
		// 19 not listed
		DynamicBrakeChange = 20,
		EngineBrakePressureIncrease = 21,
		EngineBrakePressureDecrease = 22,
		EnginePowerOn = 23,
		EnginePowerOff = 24,
		// 25 + 26 not listed
		SteamEjector2On = 27,
		SteamEjector2Off = 28,
		//29 + 30 not listed
		SteamEjector1On = 30,
		SteamEjector1Off = 31,
		DamperChange = 32,
		BlowerChange = 33,
		CylinderCocksToggle = 34,
		// 35 not listed
		FireboxDoorChange = 36,
		LightSwitchToggle = 37,
		WaterScoopDown = 38,
		WaterScoopUp = 39,
		// 40 not listed
		FireboxDoorClose = 41,
		SteamSafetyValveOn = 42,
		SteamSafetyValveOff = 43,
		SteamHeatChange = 44,
		Pantograph1Up = 45,
		Pantograph1Down = 46,
		Pantograph1Toggle = 47,
		VigilanceAlarmReset = 48,
		// 49 - 53 not listed

		// 
		// acelaeng.sms has 53 commented as Brake Normal Apply and 54 as BrakeEmergencyApply
		// 
		// OpenRails forum suggests that trigger 53 never actually worked
		// 
		TrainBrakePressureDecrease = 54,
		//55 not listed
		VigilanceAlarmOn = 56,
		VigilanceAlarmOff = 57,
		Couple = 58,
		CoupleB = 59,
		CoupleC = 60,
		Uncouple = 61,
		UncoupleB = 62,
		UncoupleC = 63,
		// 64 + 65 not listed
		Pantograph2Up = 66,
		Pantograph2Down = 67,
		/*
		 * NOTE:
		 * https://open-rails.readthedocs.io/en/latest/sound.html
		 * ORTS specific triggers
		 *
		 * Not looking to handle most of these at the minute.
		 */
		WaterPump1On = 90,
		WaterPump1Off = 91,
		WaterPump2On = 92,
		WaterPump2Off = 93,
		GearUp = 101,
		GearDown = 102,
		ReverserToForwardBackward = 103,
		ReverserToNeutral = 104,
		DoorOpen = 105,
		DoorClose = 106,
		MirrorOpen = 107,
		MirrorClose = 108

	}
}
