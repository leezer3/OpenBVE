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
using OpenBveApi.Interface;
using OpenBveApi.Motor;

namespace TrainManager.Motor
{
	public class LiveSteamInjector : AbstractComponent
	{
		/// <summary>The diameter of the injector cone</summary>
		private readonly double Diameter;

		/// <summary>The minimum boiler pressure at which the injector can operate</summary>
		private readonly double minimumBoilerPressure;

		private readonly double minimumBoilerWaterLevel;

		private readonly double maximumBoilerWaterLevel;

		public LiveSteamInjector(TractionModel engine, double coneDiameter, double minBoilerPressure, double minWaterLevel, double maxWaterLevel) : base(engine)
		{
			Diameter = coneDiameter;
			minimumBoilerPressure = minBoilerPressure;
			minimumBoilerWaterLevel = minWaterLevel;
			maximumBoilerWaterLevel = maxWaterLevel;
		}


		public override void Update(double timeElapsed)
		{
			if (Active && baseEngine.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				if (boiler.CurrentPressure < minimumBoilerPressure || boiler.WaterLevel < minimumBoilerWaterLevel ||
				    boiler.WaterLevel > maximumBoilerWaterLevel)
				{
					// Can't inject if below minimum pressure in the boiler, or above max water level
					return;
				}

				// https://web.archive.org/web/20260903094113/https://www.firgelliauto.com/blogs/mechanisms/standard-injector
				// feed water delivery rate
				// ṁw = K × At × √(2 × ρs × Ps) × (hs − hd) / (hd − hw)

				// first calculate steam mass jet flow
				// ṁs = K × At × √(2 × ρs × Ps)
				double bp = boiler.CurrentPressure * 703.0695796402; // psi to kg/m3
				double steamMassFlow = SteamTable.DischargeCoefficient * 1.96 * Math.Pow(Diameter, -5) * Math.Sqrt(2 * bp * SteamTable.GetSteamDensity(boiler.CurrentPressure));
				// apply enthalpy balance
				// ṁw = ṁs × (hs − hd) / (hd − hw)
				double waterFlow = steamMassFlow * (SteamTable.SteamEnthalpyNominal - SteamTable.InjectorWaterEnthalpyNominal) / (SteamTable.GetSteamEnthalpy(boiler.CurrentPressure) - SteamTable.InjectorWaterEnthalpyNominal);

				boiler.WaterLevel += waterFlow * timeElapsed;
				boiler.CurrentSteamVolume -= steamMassFlow * timeElapsed;
			}
		}

		public override void ControlDown(Translations.Command command)
		{
			if (command == Translations.Command.LiveSteamInjector)
			{
				Active = !Active;
			}
		}
	}

	public class ExhaustSteamInjector : AbstractComponent
	{
		/// <summary>The diameter of the injector cone</summary>
		public readonly double Diameter;

		/// <summary>The minimum boiler pressure at which the injector can operate</summary>
		private readonly double minimumBoilerPressure;

		private readonly double minimumBoilerWaterLevel;

		private readonly double maximumBoilerWaterLevel;

		public ExhaustSteamInjector(TractionModel engine, double coneDiameter, double minBoilerPressure, double minWaterLevel, double maxWaterLevel) : base(engine)
		{
			Diameter = coneDiameter;
		}


		public override void Update(double timeElapsed)
		{
			if (Active && baseEngine.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				if (boiler.CurrentPressure < minimumBoilerPressure || boiler.WaterLevel < minimumBoilerWaterLevel ||
				    boiler.WaterLevel > maximumBoilerWaterLevel || Diameter == 0)
				{
					// Can't inject if below minimum pressure in the boiler, or above max water level
					return;
				}

				// https://web.archive.org/web/20260903094113/https://www.firgelliauto.com/blogs/mechanisms/standard-injector
				// feed water delivery rate
				// ṁw = K × At × √(2 × ρs × Ps) × (hs − hd) / (hd − hw)

				// first calculate steam mass jet flow
				// ṁs = K × At × √(2 × ρs × Ps)
				double bp = boiler.CurrentPressure * 703.0695796402; // psi to kg/m3
				double steamMassFlow = SteamTable.DischargeCoefficient * 1.96 * Math.Pow(Diameter, -5) * Math.Sqrt(2 * bp * SteamTable.GetSteamDensity(boiler.CurrentPressure));
				// apply enthalpy balance
				// ṁw = ṁs × (hs − hd) / (hd − hw)
				double waterFlow = steamMassFlow * (SteamTable.SteamEnthalpyNominal - SteamTable.InjectorWaterEnthalpyNominal) / (SteamTable.GetSteamEnthalpy(boiler.CurrentPressure) - SteamTable.InjectorWaterEnthalpyNominal);

				boiler.WaterLevel += waterFlow * timeElapsed;
			}
		}

		public override void ControlDown(Translations.Command command)
		{
			if (command == Translations.Command.ExhaustSteamInjector)
			{
				Active = !Active;
			}
		}
	}
}

