using OpenBve.SafetySystems;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal struct TrainSafetySystems
	{
		/// <summary>The PassAlarm</summary>
		internal PassAlarm PassAlarm;
		/// <summary>The PilotLamp</summary>
		internal PilotLamp PilotLamp;
		/// <summary>The station adjust alarm</summary>
		internal StationAdjustAlarm StationAdjust;

		internal DoorInterlockStates DoorInterlockState;
	}
}
