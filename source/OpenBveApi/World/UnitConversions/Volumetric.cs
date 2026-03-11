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
	/// <summary>Available units of volumetric</summary>
	public enum VolumetricUnit
	{
		/// <summary>Pounds per hour</summary>
		LbPerHour,
		/// <summary>Pounds per hour</summary>
		LbPerMinute,
		/// <summary>Kilograms per hour</summary>
		KgPerHour,
		/// <summary>Kilograms per minute</summary>
		KgPerMinute,
	}

	/// <summary>Implements the volume convertor</summary>
	public class VolumetricConvertor : UnitConverter<VolumetricUnit, double>
	{
		static VolumetricConvertor()
		{
			BaseUnit = VolumetricUnit.LbPerHour;
			RegisterConversion(VolumetricUnit.LbPerMinute, v => v / 60, v => v * 60);
			RegisterConversion(VolumetricUnit.KgPerHour, v => v / 60 / 2.205, v => v * 60 * 2.205);
			RegisterConversion(VolumetricUnit.KgPerMinute, v => v / 2.205, v => v * 2.205);
			KnownUnits = new Dictionary<string, VolumetricUnit>
			{
				{ "lb/h", VolumetricUnit.LbPerHour }, {"lb/m", VolumetricUnit.LbPerMinute}, { "lbh", VolumetricUnit.LbPerHour }, {"lbm", VolumetricUnit.LbPerMinute}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, VolumetricUnit> KnownUnits;
	}
}
