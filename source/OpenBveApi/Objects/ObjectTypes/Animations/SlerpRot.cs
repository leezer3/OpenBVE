// ReSharper disable CoVariantArrayConversion
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A an animation rotating the base matrix</summary>
    public class SlerpRot : AbstractAnimation
    {
	    private readonly QuaternionFrame[] animationFrames;
		
		/// <summary>Creates a new slerp_rot animation</summary>
	    public SlerpRot(string name, QuaternionFrame[] frames) : base(name)
	    {
			animationFrames = frames;
		}
		
	    /// <summary>Updates the animation</summary>
	    public override void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
	    {
		    int currentFrame = animationFrames.FindCurrentFrame(animationKey, out int interpolateFrame, out double frac);
			Quaternion q = Quaternion.Slerp(animationFrames[currentFrame].Quaternion, animationFrames[interpolateFrame].Quaternion, (float)frac);
			Vector3 location = matrix.ExtractTranslation();
			Matrix4D newMatrix = Matrix4D.CreateFromQuaternion(q);
			matrix *= newMatrix;
			matrix.Row3.Xyz = location;
		}
	}
}
