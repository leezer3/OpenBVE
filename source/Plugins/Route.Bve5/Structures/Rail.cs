//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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

namespace Route.Bve5
{
	internal class Rail
	{
		/// <summary>The position of the rail within the block</summary>
		internal Vector2 Position;
		/// <summary>The absolute cant value</summary>
		internal double CurveCant;
		/// <summary>The horizontal radius</summary>
		internal double RadiusH;
		/// <summary>The vertical radius</summary>
		internal double RadiusV;
		/// <summary>Whether interpolation of the curve values has started</summary>
		internal bool CurveInterpolateStart;
		/// <summary>Whether interpolation of the curve values has ended</summary>
		internal bool CurveInterpolateEnd;
		/// <summary>Whether transition between two curve values has started</summary>
		internal bool CurveTransitionStart;
		/// <summary>Whether transition between two curve values has ended</summary>
		internal bool CurveTransitionEnd;
		/// <summary>Whether interpolation is applied in the X-axis</summary>
		internal bool InterpolateX;
		/// <summary>Whether interpolation is applied in the Y-axis</summary>
		internal bool InterpolateY;
	}
}
