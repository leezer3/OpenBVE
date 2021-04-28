namespace OpenBveApi
{
	/// <summary>The current state of the simulation</summary>
	public enum SimulationState
	{
		/// <summary>Loading is currently in progress</summary>
		Loading = 0,
		/// <summary>The simulation is running normally</summary>
		Running = 1,
		/// <summary>The simulation is paused</summary>
		Paused = 2,
		/// <summary>The simulation is running in minimalistic mode</summary>
		MinimalisticSimulation = 3
	}
}
