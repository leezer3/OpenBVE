using OpenBveApi.Math;

namespace MechanikRouteParser
{
	internal class RouteObject
	{
		internal int objectIndex;
		internal Vector3 Position;

		internal RouteObject(int index, Vector3 position)
		{
			this.objectIndex = index;
			this.Position = position;
		}
	}
}
