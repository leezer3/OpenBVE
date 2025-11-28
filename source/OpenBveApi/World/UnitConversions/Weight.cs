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

using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfWeight
	{
		/// <summary>Grams</summary>
		Grams,
		/// <summary>Kilograms</summary>
		Kilograms,
		/// <summary>Metric tonnes</summary>
		MetricTonnes,
		/// <summary>Imperial tons</summary>
		ImperialTons,
		/// <summary>Pounds</summary>
		Pounds,
	}

	/// <summary>Implements the weight convertor</summary>
	public class WeightConverter : UnitConverter<UnitOfWeight, double>
	{
		static WeightConverter()
		{
			BaseUnit = UnitOfWeight.Kilograms;
			RegisterConversion(UnitOfWeight.Grams, v => v * 100.0, v => v / 100.0);
			RegisterConversion(UnitOfWeight.MetricTonnes, v => v / 1000.0, v => v * 1000.0);
			RegisterConversion(UnitOfWeight.ImperialTons, v => v / 1016.05, v => v * 1016.05);
			RegisterConversion(UnitOfWeight.Pounds, v => v/ 2.205, v => v * 2.205);
			KnownUnits = new Dictionary<string, UnitOfWeight>
			{
				{"g", UnitOfWeight.Grams}, {"gram", UnitOfWeight.Grams}, {"grams", UnitOfWeight.Grams},
				{"kg", UnitOfWeight.Kilograms}, {"kilogram", UnitOfWeight.Kilograms}, {"kilograms", UnitOfWeight.Kilograms},
				/*
				 * NOTE: Unfortunately, metric and imperial tons are often confused / interchangable
				 *       They're close enough it's unlikely to matter for our purposes, but we're going to assume that
				 *       the T abbreviation refers to a metric ton for convienence.
				 */
				{"t", UnitOfWeight.MetricTonnes}, {"tonne", UnitOfWeight.MetricTonnes}, {"tonnes", UnitOfWeight.MetricTonnes},
				{"ton", UnitOfWeight.ImperialTons}, {"tons", UnitOfWeight.ImperialTons}, {"t-uk", UnitOfWeight.ImperialTons},
				{"lbs", UnitOfWeight.Pounds}, {"lb", UnitOfWeight.Pounds}, {"pounds", UnitOfWeight.Pounds}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfWeight> KnownUnits;
	}
}
