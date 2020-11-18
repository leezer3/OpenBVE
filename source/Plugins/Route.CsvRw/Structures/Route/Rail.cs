using OpenBveApi.Math;

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

		internal Vector2 MidPoint
		{
			get
			{
				return new Vector2(RailEnd - RailStart);
			}
		}
	}
}
