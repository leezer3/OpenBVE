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
	/// <summary>Represents the valve gear and related properties</summary>
	/// <remarks>Simple Stephenson type valve gear, single main rod and cylinder slider bar</remarks>
	public class ValveGear
	{
		/// <summary>Holds a reference to the base engine</summary>
		public readonly SteamEngine Engine;
		/// <summary>The circumference of the circle described by the crank pivot point</summary>
		private readonly double crankCircumference;
		/// <summary>The current rotational position of the wheel</summary>
		public double WheelPosition;
		/// <summary>The connecting rod</summary>
		internal ValveGearRod ConnectingRod;
		/// <summary>The left crank rod</summary>
		internal ValveGearRod[] CrankRods;
		/// <summary>The location of the left valve gear pivot</summary>
		public Vector2 LeftPivotLocation;
		/// <summary>The location of the right valve gear pivot</summary>
		public Vector2 RightPivotLocation;

		public ValveGear(SteamEngine engine)
		{
			// FIXME: Values from 81xx as temp to port...
			CrankRods = new ValveGearRod[]
			{
				new ValveGearRod(0.35, 1.28, 0),
				new ValveGearRod(0.35, 1.28, 90)
			};
			ConnectingRod = new ValveGearRod(0.35, 1.28, 0);
			Engine = engine;
			crankCircumference = ConnectingRod.Radius * 2 * Math.PI;
		}

		private double previousLocation;
		private const double halfCircleDegrees = Math.PI / 180;

		internal void Update()
		{
			double distanceTravelled = Engine.Car.TrackPosition - previousLocation;
			previousLocation = Engine.Car.TrackPosition;

			double percentageRotated = (distanceTravelled / crankCircumference) * 35;
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

			if (ConnectingRod.Radius > 0)
			{
				LeftPivotLocation.X = -ConnectingRod.Radius * Math.Sin(halfCircleDegrees * turnedDegrees);
				LeftPivotLocation.Y =  ConnectingRod.Radius * Math.Cos(halfCircleDegrees * turnedDegrees);
				RightPivotLocation.X = -ConnectingRod.Radius * Math.Sin(halfCircleDegrees * (turnedDegrees + 90));
				RightPivotLocation.Y =  ConnectingRod.Radius * Math.Cos(halfCircleDegrees * (turnedDegrees + 90));
			}

			for (int i = 0; i < CrankRods.Length; i++)
			{
				CrankRods[i].Update(turnedDegrees);
			}
		}
	}
}
