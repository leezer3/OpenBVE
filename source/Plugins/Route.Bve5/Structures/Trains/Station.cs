using OpenBveApi.Runtime;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	internal struct Station
	{
		string Name;
		StationStopMode StopMode;
		StationType StationType;
		double ArrivalTime;
		double DepartureTime;
		bool OpenLeftDoors;
		bool OpenRightDoors;
		bool PassAlarm;
		SafetySystem SafetyDevice;
	}
}
