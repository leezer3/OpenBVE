using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
    public class KeyframeAnimation
    {
		/// <summary>The animation name</summary>
	    public readonly string Name;
		/// <summary>The base matrix before transforms are performed</summary>
	    private readonly Matrix4D baseMatrix;

		/*
	     * FRAMERATE
		 * ---------
		 *
		 * FrameRate indicates the number of frames per second (of real-time) the animation advances.
		 * 
		 * WHEELS
		 * ------
		 *
		 * An animation with the name prefixed with WHEELS reads the corresponding wheel radius from the ENG file and uses a pretty standard odometer to rotate
		 * It has 9 frames for a total of 1 revolution.
		 *
		 * We need to ignore (!) the time advance, and instead use the total wheel revolution.
		 * The ROD prefixed animations will likewise require linking to the WHEEL position.
		 * Essentially, read back through the hierarchy tree for the wheel pos. (Will require a new animation subtype)
		 *
		 * For the minute however, let's just try and animate everything at the FPS value.
		 * This will give us constantly rotating rods, wheels etc and proves the point (!)
		 *
	     * MSTS bug / feature
	     * ------------------
	     * The FrameRate number is divided by 30fps for interior views
		 * ref http://www.elvastower.com/forums/index.php?/topic/29692-animations-in-the-passenger-view-too-fast/page__p__213634
	     *
	     */

		/// <summary>The framerate</summary>
		public readonly double FrameRate;

	    /// <summary>The total number of frames in the animation</summary>
	    public readonly int FrameCount;


	    /// <summary>The controllers for the animation</summary>
	    public AbstractAnimation[] AnimationControllers;
		/// <summary>The matrix to send to the shader</summary>
	    public Matrix4D Matrix;
		/// <summary>The current animation key</summary>
	    internal double AnimationKey;

	    public KeyframeAnimation(string name, int frameCount, double frameRate, Matrix4D matrix)
	    {
			Name = name;
		    baseMatrix = matrix;
			FrameCount = frameCount;
			FrameRate = frameRate / 100;
	    }

		public void Update(AbstractTrain train, int carIndex, Vector3 position, double trackPosition, int sectionIndex, bool isPartOfTrain, double timeElapsed, int currentState)
		{
			// calculate the current keyframe for the animation
			AnimationKey += timeElapsed * FrameRate;
			AnimationKey %= FrameCount;
			// we start off with the base matrix (clone!)
			Matrix = new Matrix4D(baseMatrix);
			for (int i = 0; i < AnimationControllers.Length; i++)
			{
				if (AnimationControllers[i] != null)
				{
					// for each of the valid controllers within the animation, perform an update
					AnimationControllers[i].Update(AnimationKey, timeElapsed, ref Matrix);
				}
				
			}
		}

	}
}
