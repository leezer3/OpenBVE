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
		private readonly KeyframeAnimatedObject containerObject;

		/// <summary>Creates a new keyframe matrix</summary>
		public KeyframeMatrix(KeyframeAnimatedObject container, string name, Matrix4D matrix)
		{
			containerObject = container;
			Name = name;
			_matrix = matrix;
		}

		/// <summary>The matrix name</summary>
		public readonly string Name;
		/// <summary>The base matrix before transformations</summary>
		private readonly Matrix4D _matrix;
		/// <summary>Gets the final transformed matrix</summary>
		public Matrix4D Matrix
		{
			get
			{
				if (containerObject.Animations != null && containerObject.Animations.ContainsKey(Name))
				{
					return containerObject.Animations[Name].Matrix;

				}
				return _matrix;
			}
		}
	}
}
