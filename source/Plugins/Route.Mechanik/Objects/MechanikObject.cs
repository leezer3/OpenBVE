using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace MechanikRouteParser
{
	internal struct MechanikObject
	{
		internal MechnikObjectType Type;
		internal Vector3 TopLeft;
		internal double ScaleFactor;
		internal int TextureIndex;
		internal StaticObject Object;
	}
}
