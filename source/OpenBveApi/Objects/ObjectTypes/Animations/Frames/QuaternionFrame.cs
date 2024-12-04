using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
    /// <summary>An quaternion animation frame</summary>
    public class QuaternionFrame : AbstractFrame
    {
		/// <summary>The quaternion</summary>
	    public Quaternion Quaternion;

		/// <summary>Creates a QuaternionFrame</summary>
	    public QuaternionFrame(int frameNumber, Quaternion frame) : base(frameNumber)
	    {
			Quaternion = frame;
	    }
    }
}
