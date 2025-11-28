//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


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
			// n.b. for correct interpolation must use internal frame number, not frame index for cases where the number of frames doesn't match the indicies, e.g. Scotsman valve gear
			diff = currentFrame < interpolateFrame ? Clamp((animationKey - frames[currentFrame].FrameNumber) / (frames[interpolateFrame].FrameNumber - frames[currentFrame].FrameNumber), 0, 1) : 0;
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
