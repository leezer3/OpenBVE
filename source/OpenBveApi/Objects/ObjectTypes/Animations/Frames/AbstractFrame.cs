namespace OpenBveApi.Objects
{
    /// <summary>An abstract animation frame</summary>
    public abstract class AbstractFrame
    {
	    /// <summary>The frame number within the animation</summary>
	    public readonly int FrameNumber;

	    /// <summary>Creates a new abstract frame</summary>
	    /// <param name="frameNumber">The frame number</param>
	    protected internal AbstractFrame(int frameNumber)
	    {
			FrameNumber = frameNumber;
	    }
    }

	/// <summary>Extension methods for use with frames</summary>
    internal static class FrameExtensions
    {
	    /// <summary>Finds the current frame in a frame array</summary>
	    /// <param name="frames">The frame array</param>
	    /// <param name="animationKey">The animation key</param>
	    /// <param name="interpolateFrame">The frame interpolation is performed to</param>
	    /// <param name="diff">The difference value to be used in interpolation</param>
	    /// <returns>The index of the current frame</returns>
	    public static int FindCurrentFrame(this AbstractFrame[] frames, double animationKey, out int interpolateFrame, out double diff)
	    {
		    int currentFrame = 0;
		    for (int i = 0; i < frames.Length; i++)
		    {
			    if (frames[i].FrameNumber <= animationKey)
			    {
				    currentFrame = i;
			    }
			    else if (frames[i].FrameNumber > animationKey)
			    {
				    break;
			    }
		    }

		    interpolateFrame = currentFrame + 1 < frames.Length ? currentFrame + 1 : currentFrame;
			diff = currentFrame < interpolateFrame ? Clamp((animationKey - currentFrame) / (interpolateFrame - currentFrame), 0, 1) : 0;
		    return currentFrame;
	    }

	    private static double Clamp(double value, double min, double max)
	    {
		    value = value > max ? max : value;
		    value = value < min ? min : value;
		    return value;
	    }
	}
}
