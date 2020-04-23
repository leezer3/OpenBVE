using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>Defines an animated signal object:
	/// The object is provided with the aspect number, and then should deal with the rest
	/// </summary>
	internal class AnimatedObjectSignalData : SignalData
	{
		internal UnifiedObject Objects;
		internal override void Create(Vector3 wpos, Transformation railTransformation, Transformation auxTransformation, int sectionIndex, bool accurateObjectDisposal, double startingDistance, double endingDistance, double blockInterval, double trackPosition, double brightness)
		{
			Objects.CreateObject(wpos, railTransformation, auxTransformation, sectionIndex, accurateObjectDisposal, startingDistance, endingDistance, blockInterval, trackPosition, 1.0, false);
		}
	}
}
