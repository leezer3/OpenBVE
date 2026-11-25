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
	internal class FireboxProperties
	{
		/// <summary>The maximum fire mass in kg</summary>
		internal double MaxFireMass = 1000;
		/// <summary>The ideal fire mass in kg</summary>
		internal double IdealFireMass = 770;
		/// <summary>The mass of one shovel of coal in kg</summary>
		internal double ShovelCoalMass = 0.5;
		/// <summary>The starting fire temperature in C</summary>
		internal double StartingFireTemp = 500;
		/// <summary>The starting fire mass in kg</summary>
		internal double StartingFireMass = 1000;
		/// <summary>The effect multiplier drafting (blast effect) has</summary>
		internal double BlastExponent = 1.0;
		/// <summary>The drafting exponent, which actually controls fuel type</summary>
		internal double DraftingExponent;

		internal void Create(TractionModel model)
		{
			// ORTS engine variables allow to specify the fuel type
			model.Components.Add(EngineComponent.Firebox, new Firebox(model, FireboxFuel.Coal, MaxFireMass, ShovelCoalMass, BlastExponent, Math.Min(MaxFireMass, StartingFireMass), IdealFireMass, Math.Max(StartingFireTemp, 500)));
		}
	}
}
