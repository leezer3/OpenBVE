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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a keyframe animated object</summary>
	public class KeyframeAnimatedObject : UnifiedObject
	{
		private readonly HostInterface currentHost;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;
		/// <summary>Contains the list of animations contained within this object</summary>
		public Dictionary<int, KeyframeAnimation> Animations;
		/// <summary>The objects within this object</summary>
		public ObjectState[] Objects;
		/// <summary>The keyframe matricies to be sent to the shader</summary>
		public KeyframeMatrix[] Matricies;
		/// <summary>The pivots</summary>
		public Dictionary<string, PivotPoint> Pivots = new Dictionary<string, PivotPoint>();
		/// <summary>Holds a reference to the car if appropriate</summary>
		public AbstractCar BaseCar;
		/// <summary>Track follower used to update bogie positions</summary>
		internal readonly TrackFollower trackFollower;
		/// <summary>The front axle world-position of the car</summary>
		internal Vector3 frontAxlePosition;
		/// <summary>The rear axle world-position of the car</summary>
		internal Vector3 rearAxlePosition;
		/// <summary>The current absolute track position of the car</summary>
		internal double currentTrackPosition;
		/// <summary>Whether the object is reversed</summary>
		internal bool IsReversed;

		/// <summary>Creates an empty KeyFrameAnimatedObject</summary>
		/// <param name="currentHost">The host application interface</param>
		public KeyframeAnimatedObject(HostInterface currentHost)
		{
			this.currentHost = currentHost;
			RefreshRate = 0;
			SecondsSinceLastUpdate = 0;
			Matricies = new KeyframeMatrix[0];
			Animations = new Dictionary<int, KeyframeAnimation>();
			trackFollower = new TrackFollower(currentHost);
		}

		/// <inheritdoc />
		public override void CreateObject(Vector3 position, Transformation worldTransformation, Transformation localTransformation, int sectionIndex, double startingDistance, double endingDistance, double trackPosition, double brightness, bool duplicateMaterials = false)
		{
			// Update animations
			for (int i = 0; i < Animations.Count; i++)
			{
				int key = Animations.ElementAt(i).Key;
				Animations[key].Update(null, IsReversed, position, trackPosition, 0); // current state not applicable, no hand-crafted functions
			}
			Transformation finalTransformation = new Transformation(localTransformation, worldTransformation);
			Matrix4D[] matriciesToShader = new Matrix4D[Matricies.Length];
			for (int i = 0; i < matriciesToShader.Length; i++)
			{
				matriciesToShader[i] = Matricies[i].Matrix;
			}

			for (int i = 0; i < Objects.Length; i++)
			{
				if (Objects[i] == null)
				{
					continue;
				}
				Objects[i].WorldPosition = position;
				Objects[i].Rotate = Matrix4D.NoTransformation;
				Objects[i].Translation = Matrix4D.NoTransformation;
				currentHost.ShowObject(Objects[i], ObjectType.Dynamic);
				Objects[i].Matricies = matriciesToShader;
			}
			int a = currentHost.AnimatedWorldObjectsUsed;
			KeyframeWorldObject currentObject = new KeyframeWorldObject(currentHost)
			{
				Object = (KeyframeAnimatedObject)Clone(),
				Position = position,
				Direction = finalTransformation.Z,
				Up = finalTransformation.Y,
				Side = finalTransformation.X,
				TrackPosition = trackPosition
			};
			currentHost.AnimatedWorldObjects[a] = currentObject;
			currentHost.AnimatedWorldObjectsUsed++;

		}

		/// <inheritdoc />
		public override void OptimizeObject(bool preserveVerticies, int threshold, bool vertexCulling)
		{
			// not required
		}

		/// <inheritdoc />
		public override UnifiedObject Clone()
		{
			KeyframeAnimatedObject clonedObject = new KeyframeAnimatedObject(currentHost);
			clonedObject.Objects = new ObjectState[Objects.Length];
			for (int i = 0; i < Objects.Length; i++)
			{
				clonedObject.Objects[i] = (ObjectState)Objects[i].Clone();
			}

			clonedObject.Matricies = new KeyframeMatrix[Matricies.Length];
			for (int i = 0; i < Matricies.Length; i++)
			{
				clonedObject.Matricies[i] = new KeyframeMatrix(clonedObject, Matricies[i].Index, Matricies[i].Name, Matricies[i]._matrix);
			}

			for (int i = 0; i < Animations.Count; i++)
			{
				clonedObject.Animations.Add(Animations.ElementAt(i).Key, Animations.ElementAt(i).Value.Clone(clonedObject));
			}

			for (int i = 0; i < Pivots.Count; i++)
			{
				clonedObject.Pivots.Add(Pivots.ElementAt(i).Key, Pivots.ElementAt(i).Value);
			}
			return clonedObject;
			
		}

		/// <inheritdoc />
		public override UnifiedObject Mirror()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override UnifiedObject Transform(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <inheritdoc />
		public override UnifiedObject TransformLeft(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <inheritdoc />
		public override UnifiedObject TransformRight(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <summary> Updates the position and state of the animated object</summary>
		/// <param name="trackPosition"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="up"></param>
		/// <param name="side"></param>
		/// <param name="timeElapsed">The time elapsed since this object was last updated</param>
		/// <param name="isTouch">Whether Animated Object belonging to TouchElement class.</param>
		/// <param name="camera"></param>
		public void Update(double trackPosition, Vector3 position, Vector3 direction, Vector3 up, Vector3 side, double timeElapsed, bool isTouch = false, dynamic camera = null)
		{
			if (BaseCar != null)
			{
				dynamic dynamicCar = BaseCar; // HACK: get the track follower onto the right track index!
				trackFollower.TrackIndex = dynamicCar.FrontAxle.Follower.TrackIndex;
				frontAxlePosition = dynamicCar.FrontAxle.Follower.WorldPosition;
				rearAxlePosition = dynamicCar.RearAxle.Follower.WorldPosition;
				currentTrackPosition = trackPosition;
			}
			
			// Update animations
			for (int i = 0; i < Animations.Count; i++)
			{
				int key = Animations.ElementAt(i).Key;
				Animations[key].Update(BaseCar, IsReversed, position, trackPosition, timeElapsed); // current state not applicable, no hand-crafted functions
			}
			Matrix4D[] matriciesToShader = new Matrix4D[Matricies.Length];
			for (int i = 0; i < matriciesToShader.Length; i++)
			{
				matriciesToShader[i] = Matricies[i].Matrix;
			}
			for (int i = 0; i < Objects.Length; i++)
			{
				if (Objects[i] == null)
				{
					continue;
				}
				Objects[i].Matricies = matriciesToShader;
				Objects[i].Translation = Matrix4D.CreateTranslation(position.X, position.Y, -position.Z);
				Matrix4D flipMatrix = Matrix4D.NoTransformation;
				if (IsReversed)
				{
					flipMatrix = Matrix4D.CreateFromAxisAngle(Vector3.Down, 3.14159);
				}
				Objects[i].Rotate = (Matrix4D)new Transformation(direction, up, side);
				Objects[i].Rotate *= flipMatrix;
				currentHost.ShowObject(Objects[i], ObjectType.Dynamic);
			}
		}

		/// <summary>Reverses the object</summary>
		public void Reverse()
		{
			IsReversed = true;

		}

		/// <summary>Converts the KeyframeAnimatedObject to a static object</summary>
		/// <remarks>This will set all animation positions to Frame 0</remarks>
        public static implicit operator StaticObject(KeyframeAnimatedObject keyframeAnimatedObject)
		{
			Matrix4D[] matricies = new Matrix4D[keyframeAnimatedObject.Matricies.Length];
			for (int i = 0; i < keyframeAnimatedObject.Animations.Count; i++)
			{
				int key = keyframeAnimatedObject.Animations.ElementAt(i).Key;
				keyframeAnimatedObject.Animations[key].Update(null, keyframeAnimatedObject.IsReversed, Vector3.Zero, 0, 0.0); // current state not applicable, no hand-crafted functions
			}

			for (int i = 0; i < matricies.Length; i++)
			{
				matricies[i] = keyframeAnimatedObject.Matricies[i].Matrix;
			}
			StaticObject staticObject = new StaticObject(keyframeAnimatedObject.currentHost);
			for (int i = 0; i < keyframeAnimatedObject.Objects.Length; i++)
			{
				staticObject.JoinObjects(keyframeAnimatedObject.Objects[i].Prototype, matricies);
			}
			return staticObject;
		}

		/// <inheritdoc />
		public override void ApplyTranslation(double x, double y, double z, bool absoluteTranslation = false)
		{
			if (absoluteTranslation)
			{
				Matricies[0]._matrix.Row3.X = x;
				Matricies[0]._matrix.Row3.Y = y;
				Matricies[0]._matrix.Row3.Z = z;
			}
			else
			{
				Matricies[0]._matrix.Row3.X += x;
				Matricies[0]._matrix.Row3.Y += y;
				Matricies[0]._matrix.Row3.Z += z;
			}
		}
	}
}
