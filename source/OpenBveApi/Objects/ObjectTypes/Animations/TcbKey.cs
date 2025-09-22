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

// ReSharper disable CoVariantArrayConversion

using System.ServiceModel.Dispatcher;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
    /// <summary>An animation setting an absolute rotation</summary>
    public class TcbKey : AbstractAnimation
    {
	    private readonly QuaternionFrame[] animationFrames;

	    /// <summary>Creates a new tcb_key animation</summary>
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

	    /// <inheritdoc />
	    public override AbstractAnimation Clone()
	    {
		    return new TcbKey(Name, animationFrames);
	    }
	}
}
