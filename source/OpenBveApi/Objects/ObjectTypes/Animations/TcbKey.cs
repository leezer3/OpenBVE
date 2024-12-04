// ReSharper disable CoVariantArrayConversion
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
    /// <summary>An animation setting an absolute rotation</summary>
    public class TcbKey : AbstractAnimation
    {
	    private readonly QuaternionFrame[] animationFrames;

	    /// <summary>Creates a new slerp_rot animation</summary>
	    public TcbKey(string name, QuaternionFrame[] frames) : base(name)
	    {
		    animationFrames = frames;
	    }

	    /// <summary>Updates the animation</summary>
	    public override void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
	    {
		    int currentFrame = animationFrames.FindCurrentFrame(animationKey, out int interpolateFrame, out double frac);
			Quaternion q = Quaternion.Slerp(animationFrames[currentFrame].Quaternion, animationFrames[interpolateFrame].Quaternion, (float)frac);
		    Vector3 location = matrix.ExtractTranslation();
		    matrix = Matrix4D.CreateFromQuaternion(q);
		    matrix.Row3.Xyz = location;
	    }
	}
}
