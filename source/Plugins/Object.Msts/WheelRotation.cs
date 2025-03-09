using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Plugin
{
	partial class MsTsShapeParser
    {
		/// <summary>Default set of wheel rotation frames</summary>
	    private static readonly QuaternionFrame[] defaultWheelRotationFrames = new[]
	    {
		    new QuaternionFrame(0, new Quaternion(0.707107, 0, 0, -0.707107)),
		    new QuaternionFrame(1, new Quaternion(0.382683, 0, 0, -0.92388)),
			new QuaternionFrame(2, new Quaternion(0, 0, 0, -1)),
			new QuaternionFrame(3, new Quaternion(-0.382683, 0, 0, -0.92388)),
			new QuaternionFrame(4, new Quaternion(-0.707107, 0, 0, -0.707107)),
			new QuaternionFrame(5, new Quaternion(-0.923879, 0, 0, -0.382684)),
			new QuaternionFrame(6, new Quaternion(-1, 0, 0, 0)),
			new QuaternionFrame(7, new Quaternion(-0.92388, 0, 0, 0.382683)),
			new QuaternionFrame(8, new Quaternion(0.707107, 0, 0, -0.707106)),
		};
    }
}
