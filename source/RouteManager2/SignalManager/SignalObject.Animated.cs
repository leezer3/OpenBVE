using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace RouteManager2.SignalManager
{
	/// <summary>Defines an animated signal object:
	/// The object is provided with the aspect number, and then should deal with the rest
	/// </summary>
	public class AnimatedObjectSignalData : SignalObject
	{
		/// <summary>Creates a new AnimatedObjectSignalData</summary>
		/// <param name="signalObject">The object</param>
		public AnimatedObjectSignalData(UnifiedObject signalObject)
		{
			Object = signalObject;
		}
		
		/// <summary>The animated object</summary>
		private readonly UnifiedObject Object;

		/// <summary>Creates the object within the game world</summary>
		public override void Create(Vector3 wpos, Transformation railTransformation, Transformation localTransformation, int sectionIndex, double startingDistance, double endingDistance, double trackPosition, double brightness)
		{
			Object.CreateObject(wpos, railTransformation, localTransformation, sectionIndex, startingDistance, endingDistance, trackPosition, 1.0);
		}
	}
}
