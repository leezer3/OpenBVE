using OpenBveApi.Math;

namespace OpenBveApi.FunctionScripting
{
	/// <summary>Interfaces with the CS Script Library </summary>
	public interface AnimationScript
	{
		/// <summary> Call to execute this script </summary>
		/// <param name="Train">A reference to the nearest train</param>
		/// <param name="Position">The object's absolute in world position</param>
		/// <param name="TrackPosition">The object's track position</param>
		/// <param name="SectionIndex"></param>
		/// <param name="IsPartOfTrain">Whether this object forms part of a train</param>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		double ExecuteScript(Train Train, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed);
	}
}
