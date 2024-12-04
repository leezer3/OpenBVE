// ReSharper disable CoVariantArrayConversion
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>An animation updating the position of the base matrix</summary>
    public class LinearKey : AbstractAnimation
    {
	    private readonly VectorFrame[] animationFrames;
		
		/// <summary>Creates a new linear_pos animation</summary>
		public LinearKey(string name, VectorFrame[] frames) : base(name)
	    {
		    animationFrames = frames;
		}

	    /// <summary>Updates the animation</summary>
	    public override void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
		{
			int currentFrame = animationFrames.FindCurrentFrame(animationKey, out int interpolateFrame, out double frac);
			Vector3 v = Vector3.Lerp(animationFrames[currentFrame].Vector, animationFrames[interpolateFrame].Vector, (float)frac);
		    matrix.Row3.Xyz = v;
	    }
	}
}
