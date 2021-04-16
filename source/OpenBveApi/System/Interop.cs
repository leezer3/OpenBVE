using System.ServiceModel;
using OpenBveApi.Runtime;

namespace OpenBveApi.Interop
{
	/// <summary>Provides the WCF interface for sending data to the 32-bit plugin proxy</summary>
	[ServiceContract(CallbackContract = typeof(IAtsPluginCallback))]
	public interface IAtsPluginProxy
	{
		/// <summary>Sets the plugin file to load</summary>
		/// <param name="fileName">The absolute on-disk path to the plugin file</param>
		/// <param name="SimulationProcessID">The ProcessID of the main game to monitor</param>
		[OperationContract]
		void SetPluginFile(string fileName, int SimulationProcessID);

		/// <summary>Called to load and initialize the plugin.</summary>
		/// <param name="specs">The train specifications.</param>
		/// <param name="mode">The initialization mode of the train.</param>
		/// <returns>Whether loading the plugin was successful.</returns>
		[OperationContract]
		bool Load(VehicleSpecs specs, InitializationModes mode);

		/// <summary>Called to unload the plugin</summary>
		[OperationContract]
		void Unload();

		/// <summary>Called before the train jumps to a different location.</summary>
		/// <param name="mode">The initialization mode of the train.</param>
		[OperationContract]
		void BeginJump(InitializationModes mode);

		/// <summary>Called every frame to update the plugin.</summary>
		/// <param name="proxyData">The data passed to the plugin on Elapse.</param>
		[OperationContract]
		ElapseProxy Elapse(ElapseProxy proxyData);

		/// <summary>Called to indicate a change of the reverser.</summary>
		/// <param name="reverser">The reverser.</param>
		[OperationContract]
		void SetReverser(int reverser);

		/// <summary>Called to indicate a change of the power notch.</summary>
		/// <param name="powerNotch">The power notch.</param>
		[OperationContract]
		void SetPowerNotch(int powerNotch);

		/// <summary>Called to indicate a change of the brake notch.</summary>
		/// <param name="brakeNotch">The brake notch.</param>
		[OperationContract]
		void SetBrake(int brakeNotch);

		/// <summary>Called when a virtual key is pressed.</summary>
		[OperationContract]
		void KeyDown(VirtualKeys key);

		/// <summary>Called when a virtual key is released.</summary>
		[OperationContract]
		void KeyUp(VirtualKeys key);

		/// <summary>Called when a horn is played or stopped.</summary>
		[OperationContract]
		void HornBlow(HornTypes type);

		/// <summary>Called when the state of the doors changes.</summary>
		[OperationContract]
		void DoorChange(DoorStates oldState, DoorStates newState);

		/// <summary>Is called when the aspect in the current or any of the upcoming sections changes.</summary>
		/// <param name="aspect">The aspect of the signalling section.</param>
		[OperationContract]
		void SetSignal(int aspect);

		/// <summary>Called when the train passes a beacon.</summary>
		/// <param name="beacon">The beacon data.</param>
		[OperationContract]
		void SetBeacon(BeaconData beacon);
	};

	/// <summary>Provides the WCF interface for recieving data from the 32-bit plugin proxy</summary>
	[ServiceContract]
	public interface IAtsPluginCallback
	{
		/// <summary>Raised when the underlying Win32 plugin reports an error or crashes</summary>
		/// <param name="Error">The error information</param>
		/// <param name="Critical">Whether this is critical and the plugin may not continue</param>
		[OperationContract(IsOneWay = true)]
		void ReportError(string Error, bool Critical = false);
	}
}
