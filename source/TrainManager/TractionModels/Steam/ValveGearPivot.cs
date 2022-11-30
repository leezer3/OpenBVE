//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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
using OpenBveApi.Math;

namespace TrainManager.TractionModels.Steam
{
	public class ValveGearPivot
	{
		/// <summary>The position on the wheel of this pivot</summary>
		public Vector2 Position;
		/// <summary>The rotational offset</summary>
		private readonly double RotationalOffset;
		/// <summary>The radius of the pivot point</summary>
		internal readonly double Radius;

		private const double halfCircleDegrees = Math.PI / 180;

		internal ValveGearPivot(double radius, double rotationalOffset)
		{
			Radius = radius;
			RotationalOffset = rotationalOffset;
		}

		internal void Update(double turnedDegrees)
		{
			if (Radius <= 0)
			{
				return;
			}
			Position.X = -Radius * Math.Sin(halfCircleDegrees * (turnedDegrees + RotationalOffset));
			Position.Y =  Radius * Math.Cos(halfCircleDegrees * (turnedDegrees + RotationalOffset));
		}
	}
}
