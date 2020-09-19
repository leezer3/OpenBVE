using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Routes;

namespace CsvRwRouteParser
{
	internal struct Rail
	{
		internal bool RailStarted;
		internal bool RailStartRefreshed;
		internal Vector2 RailStart;
		internal bool RailEnded;
		internal Vector2 RailEnd;
		internal double CurveCant;
		internal Dictionary<PowerSupplyTypes, PowerSupply> PowerSupplies;
	}
}
