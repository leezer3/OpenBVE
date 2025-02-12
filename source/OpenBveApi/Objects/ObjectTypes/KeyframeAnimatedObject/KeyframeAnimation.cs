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

using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>An animation using keyframes</summary>
    public class KeyframeAnimation
    {
		/// <summary>The parent object</summary>
	    internal readonly KeyframeAnimatedObject ParentObject;
		/// <summary>The parent animation</summary>
	    internal readonly string ParentAnimation;
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
		 * WHEELS AND OTHER LINKED ANIMATIONS
		 * ----------------------------------
		 *
		 * An animation with the name prefixed with WHEELS reads the corresponding wheel radius from the ENG file and uses a pretty standard odometer to rotate
		 * It has N + 1 frames for a total of 1 revolution.
		 *
		 * We need to ignore (!) the time advance, and instead use the total wheel revolution.
		 * The ROD prefixed animations will likewise require linking to the WHEEL position.
		 * Essentially, read back through the hierarchy tree for the wheel pos, and then update the animation from there.
		 *
		 * Binary ON / OFF animations should use the framerate to calculate the total transition time between the two states.
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

	    private double lastDistance;

		/// <summary>Creates a new keyframe animation</summary>
		/// <param name="parentObject">The parent object</param>
		/// <param name="parentAnimation">The parent animation</param>
		/// <param name="name">The animation name</param>
		/// <param name="frameCount">The total number of frames in the animation</param>
		/// <param name="frameRate">The framerate of the animation</param>
		/// <param name="matrix">The base matrix to be transformed</param>
		public KeyframeAnimation(KeyframeAnimatedObject parentObject, string parentAnimation, string name, int frameCount, double frameRate, Matrix4D matrix)
	    {
			ParentObject = parentObject;
			ParentAnimation = parentAnimation;
			Name = name;
		    baseMatrix = matrix;
			FrameCount = frameCount;
			FrameRate = frameRate / 100;
	    }

	    /// <summary>Updates the animation</summary>
		public void Update(AbstractTrain train, int carIndex, Vector3 position, double trackPosition, int sectionIndex, bool isPartOfTrain, double timeElapsed)
		{
			if (!string.IsNullOrEmpty(ParentAnimation) && ParentObject.Animations.ContainsKey(ParentAnimation))
			{
				// we have a parent- calculate our key from the parent key
				// n.b. ROD animations must have a parent of WHEEL
				AnimationKey = (ParentObject.Animations[ParentAnimation].AnimationKey / ParentObject.Animations[ParentAnimation].FrameCount) * FrameCount;
			}
			else
			{
				// calculate the current keyframe for the animation
				if (isPartOfTrain)
				{
					// HACK: use the train as a dynamic to allow us to pull out the car reference
					dynamic dynamicTrain = train;
					AbstractCar baseCar = dynamicTrain.Cars[carIndex];
					double wheelRadius;
					if (baseCar.Wheels != null && baseCar.Wheels.ContainsKey(Name))
					{
						wheelRadius = baseCar.Wheels[Name].Radius;
					}
					else
					{
						// unknown animation key- for the minute, we'll stick to the MSTS keys
						return;
					}

					double distanceTravelled = baseCar.FrontAxle.Follower.TrackPosition - lastDistance;
					double wheelCircumference = 2 * System.Math.PI * wheelRadius;
					lastDistance = baseCar.FrontAxle.Follower.TrackPosition;
					AnimationKey += (distanceTravelled / wheelCircumference) * FrameCount;
					AnimationKey %= FrameCount;
				}
				else
				{
					AnimationKey += timeElapsed * FrameRate;
					AnimationKey %= FrameCount;
				}
				
			}
			
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

	    /// <summary>Clones the animation</summary>
	    /// <param name="parentObject">The parent object</param>
	    /// <returns>The cloned animation</returns>
	    public KeyframeAnimation Clone(KeyframeAnimatedObject parentObject)
	    {
		    KeyframeAnimation kf = new KeyframeAnimation(parentObject, ParentAnimation, Name, FrameCount, FrameRate, Matrix);
		    kf.AnimationControllers = new AbstractAnimation[AnimationControllers.Length];
		    for (int i = 0; i < AnimationControllers.Length; i++)
		    {
			    kf.AnimationControllers[i] = AnimationControllers[i].Clone();
		    }

		    return kf;
	    }
	}
}
