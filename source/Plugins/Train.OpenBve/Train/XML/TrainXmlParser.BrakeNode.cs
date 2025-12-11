//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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

using Formats.OpenBve;
using System;
using TrainManager.BrakeSystems;
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseBrakeNode(Block<TrainXMLSection, TrainXMLKey> block, string fileName, int carIndex, ref TrainBase Train)
		{
			double compressorRate = 5000.0, compressorMinimumPressure = 690000.0, compressorMaximumPressure = 780000.0;
			double auxiliaryReservoirChargeRate = 200000.0;
			double equalizingReservoirChargeRate = 200000.0, equalizingReservoirServiceRate = 50000.0, equalizingReservoirEmergencyRate = 250000.0;
			double brakePipeNormalPressure = 0.0, brakePipeChargeRate = 10000000.0, brakePipeServiceRate = 1500000.0, brakePipeEmergencyRate = 5000000.0;
			double brakePipeVolume = Math.Pow(0.0175 * Math.PI, 2) * (Train.Cars.Length * 1.05); // Assuming Railway Group Standards 3.5cm diameter brake pipe, 5% extra length for bends etc.
			double mainReservoirVolume = 0.5; // Organization for Co-Operation between Railways specifies 340L to 680L main reservoir capacity for EMU, so let's pick something in the middle (in m³)
			double auxiliaryReservoirVolume = 0.16; // guessed 1/3 of main reservoir volume
			double equalizingReservoirVolume = 0.015; // very small reservoir for observation, so guess at 15L
			double brakeCylinderVolume = 0.14; // 35cm diameter, 15cm stroke
			double straightAirPipeServiceRate = 300000.0, straightAirPipeEmergencyRate = 400000.0, straightAirPipeReleaseRate = 200000.0;
			double brakeCylinderServiceMaximumPressure = 440000.0, brakeCylinderEmergencyMaximumPressure = 440000.0, brakeCylinderEmergencyRate = 300000.0, brakeCylinderReleaseRate = 200000.0;
			if (block.ReadBlock(TrainXMLSection.Compressor, out Block<TrainXMLSection, TrainXMLKey> compressorBlock))
			{
				Train.Cars[carIndex].CarBrake.BrakeType = BrakeType.Main; //We have a compressor so must be a main brake type
				compressorBlock.TryGetValue(TrainXMLKey.Rate, ref compressorRate, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.MainReservoir, out Block<TrainXMLSection, TrainXMLKey> mainReservoirBlock))
			{
				mainReservoirBlock.TryGetValue(TrainXMLKey.MinimumPressure, ref compressorMinimumPressure, NumberRange.Positive);
				mainReservoirBlock.TryGetValue(TrainXMLKey.MaximumPressure, ref compressorMaximumPressure, NumberRange.Positive);
				mainReservoirBlock.TryGetValue(TrainXMLKey.Volume, ref mainReservoirVolume, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.AuxiliaryReservoir, out Block<TrainXMLSection, TrainXMLKey> auxiliaryReservoirBlock))
			{
				auxiliaryReservoirBlock.TryGetValue(TrainXMLKey.ChargeRate, ref auxiliaryReservoirChargeRate, NumberRange.Positive);
				auxiliaryReservoirBlock.TryGetValue(TrainXMLKey.Volume, ref auxiliaryReservoirVolume, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.EqualizingReservoir, out Block<TrainXMLSection, TrainXMLKey> equalizingReservoirBlock))
			{
				equalizingReservoirBlock.TryGetValue(TrainXMLKey.ChargeRate, ref equalizingReservoirChargeRate, NumberRange.Positive);
				equalizingReservoirBlock.TryGetValue(TrainXMLKey.ServiceRate, ref equalizingReservoirServiceRate, NumberRange.Positive);
				equalizingReservoirBlock.TryGetValue(TrainXMLKey.EmergencyRate, ref equalizingReservoirEmergencyRate, NumberRange.Positive);
				equalizingReservoirBlock.TryGetValue(TrainXMLKey.Volume, ref equalizingReservoirVolume, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.BrakePipe, out Block<TrainXMLSection, TrainXMLKey> brakePipeBlock))
			{
				brakePipeBlock.TryGetValue(TrainXMLKey.NormalPressure, ref brakePipeNormalPressure, NumberRange.Positive);
				brakePipeBlock.TryGetValue(TrainXMLKey.ChargeRate, ref brakePipeChargeRate, NumberRange.Positive);
				brakePipeBlock.TryGetValue(TrainXMLKey.ServiceRate, ref brakePipeServiceRate, NumberRange.Positive);
				brakePipeBlock.TryGetValue(TrainXMLKey.EmergencyRate, ref brakePipeEmergencyRate, NumberRange.Positive);
				brakePipeBlock.TryGetValue(TrainXMLKey.Volume, ref brakePipeVolume, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.StraightAirPipe, out Block<TrainXMLSection, TrainXMLKey> straightAirPipeBlock))
			{
				straightAirPipeBlock.TryGetValue(TrainXMLKey.ServiceRate, ref straightAirPipeServiceRate, NumberRange.Positive);
				straightAirPipeBlock.TryGetValue(TrainXMLKey.EmergencyRate, ref straightAirPipeEmergencyRate, NumberRange.Positive);
				straightAirPipeBlock.TryGetValue(TrainXMLKey.ReleaseRate, ref straightAirPipeReleaseRate, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.BrakeCylinder, out Block<TrainXMLSection, TrainXMLKey> brakeCylinderBlock))
			{
				brakeCylinderBlock.TryGetValue(TrainXMLKey.ServiceMaximumPressure, ref brakeCylinderServiceMaximumPressure, NumberRange.Positive);
				brakeCylinderBlock.TryGetValue(TrainXMLKey.EmergencyMaximumPressure, ref brakeCylinderEmergencyMaximumPressure, NumberRange.Positive);
				brakeCylinderBlock.TryGetValue(TrainXMLKey.ReleaseRate, ref brakeCylinderReleaseRate, NumberRange.Positive);
				brakeCylinderBlock.TryGetValue(TrainXMLKey.Volume, ref brakeCylinderVolume, NumberRange.Positive);
			}

			if (block.ReadBlock(TrainXMLSection.Handle, out Block<TrainXMLSection, TrainXMLKey> handleBlock))
			{
				ParseHandleNode(handleBlock, ref Train.Handles.Brake, carIndex, Train, fileName);
			}

			block.TryGetValue(TrainXMLKey.LegacyPressureDistribution, ref Train.Specs.AveragesPressureDistribution);
			
			Train.Cars[carIndex].CarBrake.MainReservoir = new MainReservoir(compressorMinimumPressure, compressorMaximumPressure, 0.01, (Train.Handles.Brake is AirBrakeHandle ? 0.25 : 0.075) / Train.Cars.Length);
			Train.Cars[carIndex].CarBrake.MainReservoir.Volume = mainReservoirVolume;
			AirBrake airBrake = Train.Cars[carIndex].CarBrake as AirBrake;
			airBrake.Compressor = new Compressor(compressorRate, Train.Cars[carIndex].CarBrake.MainReservoir, Train.Cars[carIndex]);
			airBrake.StraightAirPipe = new StraightAirPipe(straightAirPipeServiceRate, straightAirPipeEmergencyRate, straightAirPipeReleaseRate);
			Train.Cars[carIndex].CarBrake.EqualizingReservoir = new EqualizingReservoir(equalizingReservoirServiceRate, equalizingReservoirEmergencyRate, equalizingReservoirChargeRate);
			Train.Cars[carIndex].CarBrake.EqualizingReservoir.NormalPressure = 1.005 * brakePipeNormalPressure;
			Train.Cars[carIndex].CarBrake.EqualizingReservoir.Volume = equalizingReservoirVolume;

			Train.Cars[carIndex].CarBrake.BrakePipe = new BrakePipe(brakePipeNormalPressure, brakePipeChargeRate, brakePipeServiceRate, brakePipeEmergencyRate, Train.Cars[0].CarBrake is ElectricCommandBrake);
			Train.Cars[carIndex].CarBrake.BrakePipe.Volume = brakePipeVolume;
			{
				double r = 200000.0 / brakeCylinderEmergencyMaximumPressure - 1.0;
				if (r < 0.1) r = 0.1;
				if (r > 1.0) r = 1.0;
				Train.Cars[carIndex].CarBrake.AuxiliaryReservoir = new AuxiliaryReservoir(0.975 * brakePipeNormalPressure, auxiliaryReservoirChargeRate, 0.5, r);
				Train.Cars[carIndex].CarBrake.AuxiliaryReservoir.Volume = auxiliaryReservoirVolume;
			}
			Train.Cars[carIndex].CarBrake.BrakeCylinder = new BrakeCylinder(brakeCylinderServiceMaximumPressure, brakeCylinderEmergencyMaximumPressure, Train.Handles.Brake is AirBrakeHandle ? brakeCylinderEmergencyRate : 0.3 * brakeCylinderEmergencyRate, brakeCylinderEmergencyRate, brakeCylinderReleaseRate);
			Train.Cars[carIndex].CarBrake.BrakeCylinder.Volume = brakeCylinderVolume;
			
		}
	}
}
