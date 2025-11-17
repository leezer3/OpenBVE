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
	public enum UnitOfLength
	{
		/// <summary>Millimeters</summary>
		Millimeter,
		/// <summary>Centimeters</summary>
		Centimeter,
		/// <summary>Meters</summary>
		Meter,
		/// <summary>Inches</summary>
		Inches,
		/// <summary>Feet</summary>
		Feet,
	}

	/// <summary>Implements the length convertor</summary>
	public class LengthConverter : UnitConverter<UnitOfLength, double>
	{
		static LengthConverter()
		{
			BaseUnit = UnitOfLength.Meter;
			RegisterConversion(UnitOfLength.Millimeter, v => v * 1000, v => v / 1000);
			RegisterConversion(UnitOfLength.Centimeter, v => v * 100, v => v / 100);
			RegisterConversion(UnitOfLength.Inches, v => v * 39.37, v => v / 39.37);
			RegisterConversion(UnitOfLength.Feet, v => v * 3.281, v => v / 3.281);
			KnownUnits = new Dictionary<string, UnitOfLength>
			{
				// standard units
				{"mm", UnitOfLength.Millimeter}, {"millimeter", UnitOfLength.Millimeter}, {"millimeters", UnitOfLength.Millimeter},
				{"cm", UnitOfLength.Centimeter}, {"centimeter", UnitOfLength.Centimeter}, {"centimeters", UnitOfLength.Centimeter},
				{"m", UnitOfLength.Meter}, {"meter", UnitOfLength.Meter}, {"meters", UnitOfLength.Meter},
				{"in", UnitOfLength.Inches}, {"ins", UnitOfLength.Inches}, {"inch", UnitOfLength.Inches}, {"inches", UnitOfLength.Inches},
				{"ft", UnitOfLength.Feet}, {"foot", UnitOfLength.Feet}, {"feet", UnitOfLength.Feet},
				// special-cases

				// typo in dash9.eng
				{"in2", UnitOfLength.Inches} 
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfLength> KnownUnits;
	}
}
