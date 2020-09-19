namespace OpenBveApi.Runtime
{
	/// <summary>Represents the interface for performing runtime train services.</summary>
	public interface IRuntime
	{
		/// <summary>Is called when the plugin is loaded.</summary>
		/// <param name="properties">The properties supplied to the plugin on loading.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		/// <remarks>If the plugin was not loaded successfully, the plugin should set the Reason property to supply the reason of failure.</remarks>
		bool Load(LoadProperties properties);

		/// <summary>Is called when the plugin is unloaded.</summary>
		void Unload();

		/// <summary>Is called after loading to inform the plugin about the specifications of the train.</summary>
		/// <param name="specs">The specifications of the train.</param>
		void SetVehicleSpecs(VehicleSpecs specs);

		/// <summary>Is called when the plugin should initialize or reinitialize.</summary>
		/// <param name="mode">The mode of initialization.</param>
		void Initialize(InitializationModes mode);

		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data passed to the plugin.</param>
		void Elapse(ElapseData data);

		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		void SetReverser(int reverser);

		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		void SetPower(int powerNotch);

		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		void SetBrake(int brakeNotch);

		/// <summary>Is called when a virtual key is pressed.</summary>
		/// <param name="key">The virtual key that was pressed.</param>
		void KeyDown(VirtualKeys key);

		/// <summary>Is called when a virtual key is released.</summary>
		/// <param name="key">The virtual key that was released.</param>
		void KeyUp(VirtualKeys key);

		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		void HornBlow(HornTypes type);

		/// <summary>Is called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		void DoorChange(DoorStates oldState, DoorStates newState);

		/// <summary>Is called when the aspect in the current or in any of the upcoming sections changes, or when passing section boundaries.</summary>
		/// <param name="data">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
		/// <remarks>The signal array is guaranteed to have at least one element. When accessing elements other than index 0, you must check the bounds of the array first.</remarks>
		void SetSignal(SignalData[] data);

		/// <summary>Is called when the train passes a beacon.</summary>
		/// <param name="data">The beacon data.</param>
		void SetBeacon(BeaconData data);

		/// <summary>Is called when the plugin should perform the AI.</summary>
		/// <param name="data">The AI data.</param>
		void PerformAI(AIData data);
	}
}
