using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>An abstract signal - All signals must inherit from this class</summary>
	internal abstract class SignalData
	{
		/// <summary>Creates the signal object within the world</summary>
		internal abstract void Create(Vector3 wpos, Transformation RailTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockInterval, double TrackPosition, double Brightness);
	}
}
