namespace OpenBveApi.Runtime
{
	/// <summary>Represents a virtual key.</summary>
	public enum VirtualKeys
	{
		/// <summary>The virtual S key. The default assignment is [Space]. The numerical value of this constant is 0.</summary>
		S = 0,
		/// <summary>The virtual A1 key. The default assignment is [Insert]. The numerical value of this constant is 1.</summary>
		A1 = 1,
		/// <summary>The virtual A2 key. The default assignment is [Delete]. The numerical value of this constant is 2.</summary>
		A2 = 2,
		/// <summary>The virtual B1 key. The default assignment is [Home]. The numerical value of this constant is 3.</summary>
		B1 = 3,
		/// <summary>The virtual B2 key. The default assignment is [End]. The numerical value of this constant is 4.</summary>
		B2 = 4,
		/// <summary>The virtual C1 key. The default assignment is [PageUp]. The numerical value of this constant is 5.</summary>
		C1 = 5,
		/// <summary>The virtual C2 key. The default assignment is [PageDown]. The numerical value of this constant is 6.</summary>
		C2 = 6,
		/// <summary>The virtual D key. The default assignment is [2]. The numerical value of this constant is 7.</summary>
		D = 7,
		/// <summary>The virtual E key. The default assignment is [3]. The numerical value of this constant is 8.</summary>
		E = 8,
		/// <summary>The virtual F key. The default assignment is [4]. The numerical value of this constant is 9.</summary>
		F = 9,
		/// <summary>The virtual G key. The default assignment is [5]. The numerical value of this constant is 10.</summary>
		G = 10,
		/// <summary>The virtual H key. The default assignment is [6]. The numerical value of this constant is 11.</summary>
		H = 11,
		/// <summary>The virtual I key. The default assignment is [7]. The numerical value of this constant is 12.</summary>
		I = 12,
		/// <summary>The virtual J key. The default assignment is [8]. The numerical value of this constant is 13.</summary>
		J = 13,
		/// <summary>The virtual K key. The default assignment is [9]. The numerical value of this constant is 14.</summary>
		K = 14,
		/// <summary>The virtual L key. The default assignment is [N/A]. The numerical value of this constant is 15.</summary>
		L = 15,
		/// <summary>The virtual M key. The default assignment is [N/A]. The numerical value of this constant is 16.</summary>
		M = 16,
		/// <summary>The virtual N key. The default assignment is [N/A]. The numerical value of this constant is 17.</summary>
		N = 17,
		/// <summary>The virtual O key. The default assignment is [N/A]. The numerical value of this constant is 18.</summary>
		O = 18,
		/// <summary>The virtual P key. The default assignment is [N/A]. The numerical value of this constant is 19.</summary>
		P = 19,
		
		//Keys Added
		//Common Keys
		/// <summary>Increases the speed of the windscreen wipers. The default assignment is [N/A]. The numerical value of this constant is 20.</summary>
		WiperSpeedUp = 20,
		/// <summary>Decreases the speed of the windscreen wipers. The default assignment is [N/A]. The numerical value of this constant is 21.</summary>
		WiperSpeedDown = 21,
		/// <summary>Fills fuel. The default assignment is [N/A]. The numerical value of this constant is 22.</summary>
		FillFuel = 22,
		//Steam locomotive
		/// <summary>Toggles the live-steam injector. The default assignment is [N/A]. The numerical value of this constant is 23.</summary>
		LiveSteamInjector = 23,
		/// <summary>Toggles the exhaust steam injector. The default assignment is [N/A]. The numerical value of this constant is 24.</summary>
		ExhaustSteamInjector = 24,
		/// <summary>Increases the cutoff. The default assignment is [N/A]. The numerical value of this constant is 25.</summary>
		IncreaseCutoff = 25,
		/// <summary>Decreases the cutoff. The default assignment is [N/A]. The numerical value of this constant is 26.</summary>
		DecreaseCutoff = 26,
		/// <summary>Toggles the blowers. The default assignment is [N/A]. The numerical value of this constant is 27.</summary>
		Blowers = 27,
		//Diesel Locomotive
		/// <summary>Starts the engine. The default assignment is [N/A]. The numerical value of this constant is 28.</summary>
		EngineStart = 28,
		/// <summary>Stops the engine. The default assignment is [N/A]. The numerical value of this constant is 29.</summary>
		EngineStop = 29,
		/// <summary>Changes gear up. The default assignment is [N/A]. The numerical value of this constant is 30.</summary>
		GearUp = 30,
		/// <summary>Changes gear down. The default assignment is [N/A]. The numerical value of this constant is 31.</summary>
		GearDown = 31,
		//Electric Locomotive
		/// <summary>Raises the pantograph. The default assignment is [N/A]. The numerical value of this constant is 32.</summary>
		RaisePantograph = 32,
		/// <summary>Lowers the pantograph. The default assignment is [N/A]. The numerical value of this constant is 33.</summary>
		LowerPantograph = 33,
		/// <summary>Toggles the main breaker. The default assignment is [N/A]. The numerical value of this constant is 34.</summary>
		MainBreaker = 34,
		/// <summary>Called when the driver presses the left door button [NOTE: This is called whether or not opening succeeds/ is blocked]</summary>
		LeftDoors = 35,
		/// <summary>Called when the driver presses the right door button [NOTE: This is called whether or not opening succeeds/ is blocked]</summary>
		RightDoors = 36
	}
}
