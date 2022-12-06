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
	public class ValveGearRod
	{
		/// <summary>The radius of this rod's rotation</summary>
		internal readonly double Radius;
		/// <summary>The length of this rod</summary>
		internal readonly double Length;
		/// <summary>The current angle of this rod</summary>
		public double Angle;
		/// <summary>The rotational offset</summary>
		private readonly double RotationalOffset;
		/// <summary>The current position offset of this rod</summary>
		public double Position;
		/// <summary>Whether the connected piston is currently emitting steam</summary>
		public bool CylinderSteam;

		private readonly SteamEngine Engine;

		private const double halfCircleDegrees = Math.PI / 180;

		internal void Update(double turnedDegrees)
		{
			if (Length <= 0 || Radius <= 0)
			{
				return;
			}

			double finalDegrees = turnedDegrees + RotationalOffset;
			Position = Radius * Math.Cos(halfCircleDegrees * finalDegrees) + Math.Sqrt(Math.Pow(Length, 2) - Math.Pow(Radius, 2) * Math.Pow(Math.Sin(halfCircleDegrees * finalDegrees), 2));
			Angle = Math.Asin(Radius * Math.Sin(halfCircleDegrees * finalDegrees) / Length) / 2.0;
			CylinderSteam = (finalDegrees >= 190 && finalDegrees <= 350) || Engine.CylinderChest.CylinderCocks.Open;
		}

		internal ValveGearRod(SteamEngine engine, double radius, double length, double rotationalOffset)
		{
			Engine = engine;
			Radius = radius;
			Length = length;
			RotationalOffset = rotationalOffset;
		}
	}
}
