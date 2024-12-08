using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	public class WheelRotation : AbstractAnimation
    {
	    public WheelRotation(string name) : base(name)
	    {
	    }
		
	    public override void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
	    {
		}
    }
}
