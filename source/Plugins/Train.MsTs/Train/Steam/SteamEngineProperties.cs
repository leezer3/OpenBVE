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

using OpenBveApi.Motor;
using System;
using TrainManager.Motor;
using TrainManager.SteamEngine;

namespace Train.MsTs
{
	internal class SteamEngineProperties
	{
		/* NOTE:
		 * -----
		 * steam properties in the ENG actually default to imperial units
		 * but let's use some close metric values as our defaults, just
		 * to cut down on the number of conversions
		 */
		     
		internal double MaxFireMass = 1000;
		internal double IdealFireMass = 770;
		internal double ShovelCoalMass = 0.5;
		internal double StartingFireTemp = 500;
		internal double StartingFireMass = 1000;
		internal double BlowerSteamUsage = 0.0625; // 500lbs /hr
		internal double BlowerEfficiencyModifier = 1.0;
		internal bool cylinderCocksAutomatic = false;
		internal InjectorProperties[] Injectors = new InjectorProperties[2];
		internal BoilerProperties Boiler = new BoilerProperties();
		internal CylinderProperties Cylinders = new CylinderProperties();

		internal void CreateModel(TractionModel model)
		{
			Boiler.Create(model);
			Injectors[0].Create(model, EngineComponent.SteamInjector1);
			Injectors[1].Create(model, EngineComponent.SteamInjector2);
			Cylinders.Create(model);
			model.Components.Add(EngineComponent.Blowers, new Blowers(model, BlowerSteamUsage, BlowerEfficiencyModifier));
			// ORTS engine variables allow to specify the fuel type
			model.Components.Add(EngineComponent.Firebox, new Firebox(model, FireboxFuel.Coal, MaxFireMass, ShovelCoalMass, Math.Min(MaxFireMass, StartingFireMass), IdealFireMass, Math.Max(StartingFireTemp, 500)));
		}
	}
}
