//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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
using OpenBveApi;
using OpenBveApi.Motor;
using OpenBveApi.Trains;
using TrainManager.Motor;

namespace TrainManager.SteamEngine
{
	public class Cylinders : AbstractComponent
	{
		/// <summary>The diameter of the cylinder port</summary>
		public readonly double Diameter;
		/// <summary>The length of the piston stroke</summary>
		private readonly double StrokeLength;
		/// <summary>The current piston position</summary>
		/// <remarks>Expressed as a double from 0 (full reverse) to 1 (full forward)</remarks>
		public double CurrentPistonPosition;
		/// <summary>The total number of cylinders </summary>
		public double TotalNumber;
		/// <summary>The steam mass flow figure for a single cylinder</summary>
		public double SteamMassFlow;
		/// <summary>Holds the last updated track position</summary>
		private double lastTrackPosition;

		public Cylinders(TractionModel engine, double diameter, double strokeLength, double totalNumber) : base(engine)
		{
			Diameter = diameter;
			StrokeLength = strokeLength;
			TotalNumber = totalNumber;
		}

		public override void Update(double timeElapsed)
		{
			if (baseEngine.BaseCar.TractionModel.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				if (baseEngine.BaseCar.baseTrain.StationState == TrainStopState.Jumping || baseEngine.BaseCar.baseTrain.State != TrainState.Available)
				{
					// only update the last track position, do no further processing
					lastTrackPosition = baseEngine.BaseCar.FrontAxle.Follower.TrackPosition;
					return;
				}

				double distanceTravelled = baseEngine.BaseCar.FrontAxle.Follower.TrackPosition - lastTrackPosition;
				// convert to number of wheel revolutions
				double wheelCircumference = baseEngine.BaseCar.DrivingWheels[0].Radius * 2 * Math.PI;
				// total number of full strokes performed, then perform remainder on the position to give final piston pos
				double numberOfStrokes = distanceTravelled / wheelCircumference;
				CurrentPistonPosition += numberOfStrokes;

				// now calculate number of ascending strokes, where steam is being introduced
				// this assumes 0 - 0.5 is the exhaust part of the stroke
				double ascendingStrokes = Math.Max(Math.Abs(CurrentPistonPosition) / 2 - 0.5, 0);
				CurrentPistonPosition %= 1;
				// from this calculate the percentage of actual strokes introducing steam, and thus total steam time
				double steamIntroducedPercentage = ascendingStrokes / Math.Abs(numberOfStrokes);
				double steamIntroductionTime = timeElapsed * steamIntroducedPercentage;

				// ms = K × At × √(2 × ρs × Ps)
				double bp = boiler.CurrentPressure * 703.0695796402; // psi to kg/m3
				SteamMassFlow = SteamTable.DischargeCoefficient * 1.96 * Math.Pow(Diameter, -5) * Math.Sqrt(2 * bp * SteamTable.GetSteamDensity(boiler.CurrentPressure));
				boiler.CurrentSteamMass -= SteamMassFlow * steamIntroductionTime;
			}
			
		}
	}
}
