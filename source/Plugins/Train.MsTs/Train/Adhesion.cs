//Simplified BSD License (BSD-2-Clause)
//
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

using OpenBve.Formats.MsTs;
using OpenBveApi.Interface;
using TrainManager.Car;
using TrainManager.Car.Systems;

namespace Train.MsTs
{
	internal class Adhesion
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;
		/// <summary>The value used in wheelslip conditions</summary>
		private readonly double WheelSlip;
		/// <summary>The value used in normal conditions</summary>
		private readonly double Normal;
		/// <summary>The value used in sanding conditions</summary>
		private readonly double Sanding;

		internal Adhesion(CarBase car, bool isSteamEngine)
		{
			baseCar = car;
			// Kuju suggested default values
			// see Eng_and_wag_file_reference_guideV2.doc
			if (isSteamEngine)
			{
				WheelSlip = 0.15;
				Normal = 0.3;
			}
			else
			{
				WheelSlip = 0.2;
				Normal = 0.4;
			}

			Sanding = 2.0;
		}

		internal Adhesion(Block block, CarBase car, bool isSteamEngine)
		{
			baseCar = car;
			try
			{
				WheelSlip = block.ReadSingle();
				Normal = block.ReadSingle();
				Sanding = block.ReadSingle();

				if (Sanding < 0.75 || Normal < 0.05 || WheelSlip < 0.05)
				{
					/*
					 * e.g. MT Class 47
					 *		If we don't apply at least 75 percent of rated power when *sanding* let alone normally
					 *		the ENG file is clearly bugged
					 */
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Discarding implausible Adheasion values");
					WheelSlip = 0.2;
					Normal = 0.4;
					Sanding = 2.0;
				}
			}
			catch
			{
				// Kuju suggested default values
				// see Eng_and_wag_file_reference_guideV2.doc
				if (isSteamEngine)
				{
					WheelSlip = 0.15;
					Normal = 0.3;
				}
				else
				{
					WheelSlip = 0.2;
					Normal = 0.4;
				}

				Sanding = 2.0;
			}
		}
		
		internal double GetWheelslipValue()
		{
			double multiplier;
			// https://www.trainsim.com/forums/forum/general-discussion/traction/68459-msts-diesel-sanding-effective
			// MSTS uses a per-axle drive force calculation, so our traction model's force is divided by the number of axles
			// whereas BVE thinks in terms of the whole car, so for a *hacky* version, we don't need the NumWheels divisor or
			// the mass, as we're not worrying about mass per axle yet...
			// Unlikely to be perfect, but doing it this way resolves massive wheelslip issues

			if (baseCar.ReAdhesionDevice is Sanders sanders && sanders.State == SandersState.Active)
			{
				// Per-axle:
				// multiplier = 0.95 * WheelSlip * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
				multiplier = 0.95 * WheelSlip * Sanding;
			}
			else
			{
				if (!baseCar.FrontAxle.CurrentWheelSlip)
				{
					// Per-axle:
					// 	multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.TrailingWheels[0].TotalNumber / baseCar.CurrentMass;
					multiplier = Normal * Sanding;
				}
				else
				{
					// Per-axle:
					// 	multiplier = Normal * Sanding * baseCar.CurrentMass / baseCar.DrivingWheels[0].TotalNumber / baseCar.CurrentMass;
					multiplier = WheelSlip * Sanding;
				}
			}

			if (baseCar.TractionModel.MaximumPossibleAcceleration == 0)
			{
				// no possible acceleration, so can't wheelslip!
				return double.MaxValue;
			}

			return baseCar.TractionModel.MaximumPossibleAcceleration * multiplier;
		}
	}
}
