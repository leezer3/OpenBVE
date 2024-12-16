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
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>An animation based upon the rotation of a set of wheels</summary>
    public class WheelsAnimation : AbstractAnimation
    {
	    private readonly QuaternionFrame[] animationFrames;

	    private readonly AbstractCar _car;

	    private double lastDistance;

	    /// <summary>Creates a new slerp_rot animation</summary>
	    public WheelsAnimation(string name, QuaternionFrame[] frames, AbstractCar car) : base(name)
	    {
		    animationFrames = frames;
			_car = car;
	    }

	    /// <summary>Updates the animation</summary>
	    public override void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
	    {
		    if (_car.Wheels == null)
		    {
			    return;
		    }
		    double wheelRadius;
		    switch (Name)
		    {
				case "WHEELS1":
					if (_car.Wheels.Length >= 1)
					{
						wheelRadius = _car.Wheels[0].Radius;
					}
					else
					{
						return;
					}
					break;
				case "WHEELS2":
					if (_car.Wheels.Length >= 2)
					{
						wheelRadius = _car.Wheels[1].Radius;
					}
					else
					{
						return;
					}
					break;
				case "WHEELS3":
					if (_car.Wheels.Length >= 3)
					{
						wheelRadius = _car.Wheels[2].Radius;
					}
					else
					{
						return;
					}
					break;
				case "WHEELS4":
					if (_car.Wheels.Length >= 4)
					{
						wheelRadius = _car.Wheels[3].Radius;
					}
					else
					{
						return;
					}
					break;
			    default:
					// unknown animation key- for the minute, we'll stick to the MSTS keys
				    return;
		    }

		    double distanceTravelled = _car.FrontAxle.Follower.TrackPosition - lastDistance;
		    double wheelCircumference = 2 * System.Math.PI * wheelRadius;
		    lastDistance = _car.FrontAxle.Follower.TrackPosition;
			animationKey = (distanceTravelled / wheelCircumference) * animationFrames.Length;
			animationKey %= animationFrames.Length;
		    int currentFrame = animationFrames.FindCurrentFrame(animationKey, out int interpolateFrame, out double frac);
		    Quaternion q = Quaternion.Slerp(animationFrames[currentFrame].Quaternion, animationFrames[interpolateFrame].Quaternion, (float)frac);
		    Vector3 location = matrix.ExtractTranslation();
		    Matrix4D newMatrix = Matrix4D.CreateFromQuaternion(q);
		    matrix *= newMatrix;
		    matrix.Row3.Xyz = location;
	    }
	}
}
