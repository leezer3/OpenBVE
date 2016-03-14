namespace Plugin

import System
import OpenBveApi.Runtime

class Plugin(IRuntime):

	public def Load(properties as LoadProperties) as bool:
		return true
		
	public def Unload() as void:
		pass;
		
	public def SetVehicleSpecs(specs as VehicleSpecs) as void:
		pass;
		
	public def Initialize(mode as InitializationModes) as void:
		pass;
	
	public def Elapse(data as ElapseData) as void:
		pass;
		
	public def SetReverser(reverser as int) as void:
		pass;
		
	public def SetPower(power as int) as void:
		pass;
		
	public def SetBrake(brake as int) as void:
		pass;
		
	public def KeyDown(key as VirtualKeys) as void:
		pass;
		
	public def KeyUp(key as VirtualKeys) as void:
		pass;
		
	public def HornBlow(type as HornTypes) as void:
		pass;
		
	public def DoorChange(oldState as DoorStates, newState as DoorStates) as void:
		pass;
		
	public def SetSignal(signal as (SignalData)) as void:
		pass
		
	public def SetBeacon(beacon as BeaconData) as void:
		pass
		
	public def PerformAI(data as AIData) as void:
		pass