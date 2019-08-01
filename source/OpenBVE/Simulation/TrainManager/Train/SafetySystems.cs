using OpenBveApi.Runtime;

namespace OpenBve
{
	internal struct SafetySystems
	{
		/// <summary>The PassAlarm</summary>
		internal TrainManager.PassAlarm PassAlarm;
		/// <summary>The PilotLamp</summary>
		internal TrainManager.PilotLamp PilotLamp;

		internal DoorInterlockStates DoorInterlockState;
	}
}
