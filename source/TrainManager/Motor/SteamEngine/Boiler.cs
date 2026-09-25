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

using OpenBveApi;
using OpenBveApi.Motor;

namespace TrainManager.Motor
{
	public class Boiler : AbstractComponent
	{
		public readonly double Length;
		/// <summary>The maximum steam generation rate of the boiler in kg / sec</summary>
		public readonly double MaxOutput;
		/// <summary>The current boiler water level in Liters</summary>
		public double WaterLevel;
		/// <summary>The current volume of steam in the boiler in kg</summary>
		public double CurrentSteamVolume;
		/// <summary>The current boiler pressure in PSI</summary>
		public double CurrentPressure;
		/// <summary>The maximum achievable pressure</summary>
		public readonly double MaxPressure;
		/// <summary>The total internal boiler volume</summary>
		/// <remarks>150 cubic feet</remarks>
		public const double Volume = 4247.53;

		public Boiler(TractionModel engine, double length, double maxPressure, double maxOutput, double startingWaterLevel, double startingPressure) : base(engine)
		{
			Length = length;
			MaxPressure = maxPressure;
			MaxOutput = maxOutput;
			WaterLevel = startingWaterLevel;
			CurrentPressure = startingPressure;
			// calculate the starting steam volume in the boiler
			double volumePerLiter = SteamTable.GetSteamVolume(CurrentPressure) * 1000;
			CurrentSteamVolume = (Volume - WaterLevel) * volumePerLiter; // kgs
		}

		public override void Update(double timeElapsed)
		{
			if (!baseEngine.Components.TryGetTypedValue(EngineComponent.Firebox, out Firebox firebox))
			{
				return;
			}
			
			double steamGenerated = MaxOutput * firebox.HeatOutput;
			// NOTE: MSTS steam simulation bug- boiler volume fixed at 150 cubic feet (See 'Manual 2.0e.doc')
			// for the minute, we'll run with that as this is the most likely to produce 'expected' results
			// 1lb / sqft = 0.00694 psi
			if (CurrentPressure < MaxPressure)
			{
				CurrentSteamVolume += steamGenerated / 150.0 * 0.00694 * 2.205;
				// drop water level (convert from pounds)
				WaterLevel -= steamGenerated;
				// boiler volume minus water volume gives the remaining steam space
				double steamAvailableVolume = Volume - WaterLevel;
				// actual used volume per kilo is therefore available remaining volume in L divide by current steam volume
				// sure I've missed something somewhere, but the 1000 multiplier gives correct result...
				double volumePerKilo = steamAvailableVolume / CurrentSteamVolume * 1000;
				// pressure for one kilo is the same for all
				CurrentPressure = SteamTable.GetPressure(volumePerKilo) * 14.5038; // convert to PSI
			}
			
		}
	}
}
