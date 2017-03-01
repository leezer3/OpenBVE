Imports System
Imports OpenBveApi.Runtime

Public Class Plugin
	Implements IRuntime
	
	''' <summary>Is called when the plugin is loaded.</summary>
	''' <param name="properties">The properties supplied to the plugin on loading.</param>
	''' <returns>Whether the plugin was loaded successfully.</returns>
	''' <remarks>If the plugin was not loaded successfully, the plugin should set the Reason property to supply the reason of failure.</remarks>
	Public Function Load(properties As LoadProperties) As Boolean Implements IRuntime.Load
		Return True
	End Function
	
	''' <summary>Is called when the plugin is unloaded.</summary>
	Public Sub Unload() Implements IRuntime.Unload
	End Sub
	
	''' <summary>Is called after loading to inform the plugin about the specifications of the train.</summary>
	''' <param name="specs">The specifications of the train.</param>
	Public Sub SetVehicleSpecs(specs As VehicleSpecs) Implements IRuntime.SetVehicleSpecs
	End Sub
	
	''' <summary>Is called when the plugin should initialize or reinitialize.</summary>
	''' <param name="mode">The mode of initialization.</param>
	Public Sub Initialize(mode As InitializationModes) Implements IRuntime.Initialize
	End Sub
	
	''' <summary>Is called every frame.</summary>
	''' <param name="data">The data passed to the plugin.</param>
	Public Sub Elapse(data as ElapseData) Implements IRuntime.Elapse
	End Sub
	
	''' <summary>Is called when the driver changes the reverser.</summary>
	''' <param name="reverser">The new reverser position.</param>
	Public Sub SetReverser(reverser As Integer) Implements IRuntime.SetReverser
	End Sub
	
	''' <summary>Is called when the driver changes the power notch.</summary>
	''' <param name="powerNotch">The new power notch.</param>
	Public Sub SetPower(powerNotch As Integer) Implements IRuntime.SetPower
	End Sub
	
	''' <summary>Is called when the driver changes the brake notch.</summary>
	''' <param name="brakeNotch">The new brake notch.</param>
	Public Sub SetBrake(brakeNotch As Integer) Implements IRuntime.SetBrake
	End Sub
	
	''' <summary>Is called when a virtual key is pressed.</summary>
	''' <param name="key">The virtual key that was pressed.</param>
	Public Sub KeyDown(key As VirtualKeys) Implements IRuntime.KeyDown
	End Sub
	
	''' <summary>Is called when a virtual key is released.</summary>
	''' <param name="key">The virtual key that was released.</param>
	Public Sub KeyUp(key As VirtualKeys) Implements IRuntime.KeyUp
	End Sub
	
	''' <summary>Is called when a horn is played or when the music horn is stopped.</summary>
	''' <param name="type">The type of horn.</param>
	Public Sub HornBlow(type As HornTypes) Implements IRuntime.HornBlow
	End Sub
	
	''' <summary>Is called when the state of the doors changes.</summary>
	''' <param name="oldState">The old state of the doors.</param>
	''' <param name="newState">The new state of the doors.</param>
	Public Sub DoorChange(oldState As DoorStates, newState As DoorStates) Implements IRuntime.DoorChange
	End Sub
	
	''' <summary>Is called when the aspect in the current or in any of the upcoming sections changes, or when passing section boundaries.</summary>
	''' <param name="data">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
	''' <remarks>The signal array is guaranteed to have at least one element. When accessing elements other than index 0, you must check the bounds of the array first.</remarks>
	Public Sub SetSignal(signal As SignalData()) Implements IRuntime.SetSignal
	End Sub
	
	''' <summary>Is called when the train passes a beacon.</summary>
	''' <param name="beacon">The beacon data.</param>
	Public Sub SetBeacon(beacon As BeaconData) Implements IRuntime.SetBeacon
	End Sub
	
	''' <summary>Is called when the plugin should perform the AI.</summary>
	''' <param name="data">The AI data.</param>
	Public Sub PerformAI(data As AIData) Implements IRuntime.PerformAI
	End Sub
	
End Class