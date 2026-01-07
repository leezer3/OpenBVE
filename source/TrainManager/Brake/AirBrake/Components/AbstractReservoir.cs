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

namespace TrainManager.BrakeSystems
{
	/// <summary>Base class for an abstract air reservoir</summary>
	public abstract class AbstractReservoir
	{
		/// <summary>The maximum pressure in Pa</summary>
		public readonly double MaximumPressure;
		/// <summary>The minimum pressure in Pa</summary>
		public readonly double MinimumPressure;
		/// <summary>The current pressure in Pa</summary>
		public double CurrentPressure;
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The base volume of the reservoir in m³</summary>
		public double Volume;
		/// <summary>The total volume of air contained in the reservoir (at atmospheric pressure) in m³</summary>
		public double AirVolume => Volume / 101325 * CurrentPressure;

		protected AbstractReservoir(double chargeRate, double minimumPressure, double maximumPressure)
		{
			ChargeRate = chargeRate;
			MinimumPressure = minimumPressure;
			MaximumPressure = maximumPressure;
			CurrentPressure = maximumPressure;
		}

		protected AbstractReservoir(double chargeRate, double currentPressure)
		{
			ChargeRate = chargeRate;
			MinimumPressure = 0;
			MaximumPressure = double.MaxValue;
			CurrentPressure = currentPressure;
		}
	}
}
