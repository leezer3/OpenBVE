using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class World
	{
		internal static void RotatePlane(ref Vector3 Vector, double cosa, double sina)
		{
			double u = Vector.X * cosa - Vector.Z * sina;
			double v = Vector.X * sina + Vector.Z * cosa;
			Vector.X = u;
			Vector.Z = v;
		}
	}
}
