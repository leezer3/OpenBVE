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

namespace TrainManager.TractionModels.Steam
{
	/// <summary>Represents the valve gear and related properties</summary>
	/// <remarks>Simple Stephenson type valve gear, single main rod and cylinder slider bar</remarks>
	public class ValveGear
	{
		/// <summary>Holds a reference to the base engine</summary>
		public readonly SteamEngine Engine;
		/// <summary>The circumference of the circle described by the wheel</summary>
		public readonly double WheelCircumference;
		/// <summary>The current rotational position of the wheel</summary>
		public double WheelPosition;
		/// <summary>The crank rods</summary>
		public readonly ValveGearRod[] CrankRods;
		/// <summary>The pivots</summary>
		public readonly ValveGearPivot[] Pivots;

		public ValveGear(SteamEngine engine, double wheelCircumference, ValveGearRod[] crankRods, ValveGearPivot[] pivots)
		{
			Engine = engine;
			WheelCircumference = wheelCircumference;
			CrankRods = crankRods;
			Pivots = pivots;
		}

		private double previousLocation;

		internal void Update()
		{
			double distanceTravelled = Engine.Car.TrackPosition - previousLocation;
			previousLocation = Engine.Car.TrackPosition;

			double percentageRotated = (distanceTravelled / WheelCircumference) * 35;
			if (Math.Abs(percentageRotated) > 100)
			{
				percentageRotated = 0;
			}

			if (WheelPosition - percentageRotated <= 100 && WheelPosition - percentageRotated >= 0)
			{
				WheelPosition -= percentageRotated;
			}
			else
			{
				WheelPosition = 100 - percentageRotated;
			}

			double turnedDegrees = 3.6 * WheelPosition;

			for (int i = 0; i < CrankRods.Length; i++)
			{
				CrankRods[i].Update(turnedDegrees);
			}

			for (int i = 0; i < Pivots.Length; i++)
			{
				Pivots[i].Update(turnedDegrees);
			}
		}
	}
}
