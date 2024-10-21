#pragma warning disable IDE1006
using OpenBveApi.Math;

namespace OpenBveApi.FunctionScripting
{
    /// <summary>Interfaces with the CS Script Library </summary>
	    public interface AnimationScript
    {
		/// <summary> Call to execute this script </summary>
		/// <param name="Train">A reference to the nearest train</param>
		/// <param name="CarIndex">The object's car index in a train, if is a car object</param>
		/// <param name="Position">The object's absolute in world position</param>
		/// <param name="TrackPosition">The object's track position</param>
		/// <param name="SectionIndex"></param>
		/// <param name="IsPartOfTrain">Whether this object forms part of a train</param>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		/// <param name="CurrentState">The value at the last invocation</param>
		double ExecuteScript(Trains.AbstractTrain Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState);

		/// <summary> Clone this script object </summary>
		/// <returns> An shallow copy </returns>
		AnimationScript Clone();

		/// <summary> The result on the last invocation. 0 if not yet invoked. </summary>
		double LastResult {
			get; set;
		}
		/// <summary>The maximum pinned result or NaN to set no maximum</summary>
		double Maximum {
			get; set;
		}
		/// <summary>The minimum pinned result or NaN to set no minimum</summary>
		double Minimum {
			get; set;
		}
	}
}
