using System;
using OpenBveApi.Runtime;

namespace Plugin {
	/// <summary>Represents an abstract device.</summary>
	internal abstract class Device {
		
		/// <summary>Is called when the device should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal abstract void Initialize(InitializationModes mode);

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		/// <param name="blocking">Whether the device is blocked or will block subsequent devices.</param>
		internal abstract void Elapse(ElapseData data, ref bool blocking);
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		internal abstract void SetReverser(int reverser);
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		internal abstract void SetPower(int powerNotch);
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		internal abstract void SetBrake(int brakeNotch);
		
		/// <summary>Is called when a key is pressed.</summary>
		/// <param name="key">The key.</param>
		internal abstract void KeyDown(VirtualKeys key);
		
		/// <summary>Is called when a key is released.</summary>
		/// <param name="key">The key.</param>
		internal abstract void KeyUp(VirtualKeys key);
		
		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		internal abstract void HornBlow(HornTypes type);
		
		/// <summary>Is called to inform about signals.</summary>
		/// <param name="signal">The signal data.</param>
		internal abstract void SetSignal(SignalData[] signal);
		
		/// <summary>Is called when a beacon is passed.</summary>
		/// <param name="beacon">The beacon data.</param>
		internal abstract void SetBeacon(BeaconData beacon);

	}
}