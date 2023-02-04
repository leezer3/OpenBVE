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
	public class CylinderChest
	{
		/// <summary>Holds a reference to the base engine</summary>
		public readonly SteamEngine Engine;
		/// <summary>The standing pressure loss through a stroke</summary>
		public readonly double StandingPressureLoss;
		/// <summary>The base pressure used by a stroke</summary>
		public readonly double BaseStrokePressure;
		/// <summary>The pressure used by a single stroke</summary>
		public double PressureUse => Engine.Car.baseTrain.Handles.Power.Ratio * Engine.Car.baseTrain.Handles.Reverser.Ratio * (StandingPressureLoss + BaseStrokePressure * Engine.Car.baseTrain.Handles.Reverser.Ratio);
		/// <summary>The cylinder cocks</summary>
		public CylinderCocks CylinderCocks;
		/// <summary>The valve gear</summary>
		public ValveGear ValveGear;
		/// <summary>The bypass valve</summary>
		public BypassValve BypassValve;

		public CylinderChest(SteamEngine engine, double standingPressureLoss, double baseStrokePressure)
		{
			Engine = engine;
			StandingPressureLoss = standingPressureLoss;
			BaseStrokePressure = baseStrokePressure;
			CylinderCocks = new CylinderCocks(Engine, 5);
			ValveGear = new ValveGear(engine, 0.35, new ValveGearRod[] {}, new ValveGearPivot[] {});
			BypassValve = new BypassValve(engine, BypassValveType.None, 5.0);
		}

		public void Update(double timeElapsed, double distanceTravelled)
		{
			double numberOfStrokes = Math.Abs(distanceTravelled) / Engine.Car.FrontAxle.WheelRadius;
			// drop the steam pressure appropriately
			Engine.Boiler.SteamPressure -= numberOfStrokes * Engine.Car.baseTrain.Handles.Reverser.Actual * PressureUse;
			CylinderCocks.Update(timeElapsed);
			ValveGear.Update();
			if (BypassValve.Type != BypassValveType.None)
			{
				BypassValve.Update(timeElapsed);
			}
		}
	}
}
