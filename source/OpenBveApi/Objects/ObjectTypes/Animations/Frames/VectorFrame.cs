using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A vector animation frame</summary>
    public class VectorFrame : AbstractFrame
    {
		/// <summary>The vector</summary>
	    public Vector3 Vector;

		/// <summary>Creates a VectorFrame</summary>
	    public VectorFrame(int frameNumber, Vector3 vector) : base(frameNumber)
	    {
			Vector = vector;
	    }
    }
}
