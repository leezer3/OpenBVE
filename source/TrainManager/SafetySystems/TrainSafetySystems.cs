using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	public struct TrainSafetySystems
	{
		/// <summary>The PassAlarm</summary>
		public PassAlarm PassAlarm;
		/// <summary>The PilotLamp</summary>
		public PilotLamp PilotLamp;
		/// <summary>The station adjust alarm</summary>
		public StationAdjustAlarm StationAdjust;
		/// <summary>The state of the door interlock</summary>
		public DoorInterlockStates DoorInterlockState;
	}
}
