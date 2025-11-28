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

namespace OpenBveApi.Objects
{
	/// <summary>A keyframe matrix used in a KeyframeAnimatedObject</summary>
	public class KeyframeMatrix
	{
		/// <summary>Holds a reference to the container object</summary>
		private readonly KeyframeAnimatedObject containerObject;
		/// <summary>The Z-translation of this matrix within the container object</summary>
		private readonly double zTranslation;

		/// <summary>Creates a new keyframe matrix</summary>
		public KeyframeMatrix(KeyframeAnimatedObject container, int index, string name, Matrix4D matrix)
		{
			containerObject = container;
			Index = index;
			Name = name;
			zTranslation = matrix.ExtractTranslation().Z;
			_matrix = matrix;
		}

		/// <summary>The matrix index</summary>
		public readonly int Index;
		/// <summary>The matrix name</summary>
		public readonly string Name;
		/// <summary>The base matrix before transformations</summary>
		public Matrix4D _matrix;
		/// <summary>Gets the final transformed matrix</summary>
		public Matrix4D Matrix
		{
			get
			{
				Matrix4D matrix = _matrix;
				if (containerObject.Animations.ContainsKey(Index))
				{
					matrix = containerObject.Animations[Index].Matrix;
				}
				
				if (containerObject.Pivots.ContainsKey(Name) && containerObject.BaseCar != null)
				{
					// This somewhat misuses a single track follower, but it works OK
					// Y rotation is not handled (other than by the main model matrix moving)
					Vector3 v1 = containerObject.rearAxlePosition - containerObject.frontAxlePosition;
					containerObject.trackFollower.UpdateAbsolute(containerObject.currentTrackPosition + zTranslation + containerObject.Pivots[Name].FrontPoint, true, false);
					Vector3 w1 = containerObject.trackFollower.WorldPosition;
					containerObject.trackFollower.UpdateAbsolute(containerObject.currentTrackPosition + zTranslation + containerObject.Pivots[Name].RearPoint, true, false);
					Vector3 w2 = containerObject.trackFollower.WorldPosition;
					Vector3 v2 = w2 - w1;
					double a = Vector2.Dot(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));
					double b = Vector2.Determinant(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));
					double angle = System.Math.Atan2(a, b);
					angle += 1.5708; // 90 degrees
					Matrix4D.CreateFromAxisAngle(Vector3.Down, angle, out Matrix4D m);
					m *= matrix;
					return m;
				}
				return matrix;
			}
		}
	}
}
