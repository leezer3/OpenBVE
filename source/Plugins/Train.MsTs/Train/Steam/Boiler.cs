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
using OpenBveApi.Motor;
using TrainManager.Motor;

namespace Train.MsTs
{
	internal class BoilerProperties
	{
		/// <summary>The length of the boiler in M</summary>
		internal double Length = 5;
		/// <summary>The max steam output of the boier in lb/h</summary>
		internal double MaxOutput = 32500; // no default specified in techdocs
		/// <summary>The max pressure of the boiler in PSI</summary>
		internal double MaxPressure = 180; // no default specified in techdocs
		/// <summary>The trigger point for the safety valve, above MaxPressure</summary>
		internal double SafetyValvePressureDifference = 5;
		/// <summary>The steam expelled by the safety valve in lb/h</summary>
		internal double SafetyValveSteamUsage = 8000;
		/// <summary>The starting water level in L</summary>
		internal double StartingWater;
		/// <summary>The starting pressure in PSI</summary>
		internal double StartingPressure;

		internal void Create(TractionModel model)
		{
			/* assume that the safety valve must always be able to dump at least 2x more than the boiler
			 *
			 * Note that both of these were probably limited to 20,000lb/h in default MSTS
			 * (MSTS-Dampflokomotive.de.doc)
			 *
			 * Probably a calculation slip somewhere too- generation / usage figures seem to be out by a factor of 100 at the minute
			 */

			SafetyValveSteamUsage = Math.Max(MaxOutput * 2, SafetyValveSteamUsage);
			model.Components.Add(EngineComponent.Boiler, new Boiler(model, Length, MaxPressure, MaxOutput / 2.205 / 3600 / 100, StartingWater, StartingPressure));
			model.Components.Add(EngineComponent.SafetyValve, new SafetyValve(model, MaxPressure + SafetyValvePressureDifference, MaxPressure - SafetyValvePressureDifference, SafetyValveSteamUsage / 2.205 / 3600 / 100));
		}
	}
}
