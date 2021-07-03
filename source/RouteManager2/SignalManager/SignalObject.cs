using OpenBveApi.Math;
using OpenBveApi.World;

namespace RouteManager2.SignalManager
{
	/// <summary>An abstract signal object - All signals must inherit from this class</summary>
	public abstract class SignalObject
	{
		public virtual void Create(Vector3 wpos, Transformation RailTransformation, Transformation LocalTransformation, int SectionIndex, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness)
		{}
	}
}
