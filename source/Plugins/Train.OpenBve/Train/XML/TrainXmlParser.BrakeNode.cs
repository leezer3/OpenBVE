using System;
using System.Linq;
using System.Xml;
using Formats.OpenBve;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainManager.BrakeSystems;
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseBrakeNode(XmlNode brakeNode, string fileName, int carIndex, ref TrainBase Train)
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
			foreach (XmlNode c in brakeNode.ChildNodes)
			{
				Enum.TryParse(c.Name, true, out BrakeXMLKey key);
				switch (key)
				{
					case BrakeXMLKey.Compressor:
						Train.Cars[carIndex].CarBrake.BrakeType = BrakeType.Main; //We have a compressor so must be a main brake type
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.Rate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorRate) | compressorRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid compression rate defined for Car " + carIndex + " in XML file " + fileName);
											compressorRate = 5000.0;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.MainReservoir:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.MinimumPressure:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorMinimumPressure) | compressorMinimumPressure <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid main reservoir minumum pressure defined for Car " + carIndex + " in XML file " + fileName);
											compressorMinimumPressure = 690000.0;
										}
										break;
									case BrakeXMLKey.MaximumPressure:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorMaximumPressure) | compressorMaximumPressure <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid main reservoir maximum pressure defined for Car " + carIndex + " in XML file " + fileName);
											compressorMaximumPressure = 780000.0;
										}
										break;
									case BrakeXMLKey.Volume:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out mainReservoirVolume) | mainReservoirVolume <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid main reservoir volume defined for Car " + carIndex + " in XML file " + fileName);
											mainReservoirVolume = 0.5;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.AuxiliaryReservoir:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.ChargeRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out auxiliaryReservoirChargeRate) | auxiliaryReservoirChargeRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid auxiliary reservoir charge rate defined for Car " + carIndex + " in XML file " + fileName);
											auxiliaryReservoirChargeRate = 200000.0;
										}
										break;
									case BrakeXMLKey.Volume:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out auxiliaryReservoirVolume) | auxiliaryReservoirVolume <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid auxiliary reservoir volume defined for Car " + carIndex + " in XML file " + fileName);
											auxiliaryReservoirVolume = 0.5;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.EqualizingReservoir:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.ChargeRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirChargeRate) | equalizingReservoirChargeRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir charge rate defined for Car " + carIndex + " in XML file " + fileName);
											equalizingReservoirChargeRate = 50000.0;
										}
										break;
									case BrakeXMLKey.ServiceRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirServiceRate) | equalizingReservoirServiceRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir service rate defined for Car " + carIndex + " in XML file " + fileName);
											equalizingReservoirServiceRate = 50000.0;
										}
										break;
									case BrakeXMLKey.EmergencyRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirEmergencyRate) | equalizingReservoirEmergencyRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir emergency rate defined for Car " + carIndex + " in XML file " + fileName);
											equalizingReservoirEmergencyRate = 50000.0;
										}
										break;
									case BrakeXMLKey.Volume:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirVolume) | equalizingReservoirVolume <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir volume defined for Car " + carIndex + " in XML file " + fileName);
											equalizingReservoirVolume = 0.015;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.BrakePipe:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.NormalPressure:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeNormalPressure) | brakePipeNormalPressure <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake pipe normal pressure defined for Car " + carIndex + " in XML file " + fileName);
											brakePipeNormalPressure = 0.0;
										}
										break;
									case BrakeXMLKey.ChargeRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeChargeRate) | brakePipeChargeRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake pipe charge rate defined for Car " + carIndex + " in XML file " + fileName);
											brakePipeChargeRate = 10000000.0;
										}
										break;
									case BrakeXMLKey.ServiceRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeServiceRate) | brakePipeServiceRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake pipe service rate defined for Car " + carIndex + " in XML file " + fileName);
											brakePipeServiceRate = 1500000.0;
										}
										break;
									case BrakeXMLKey.EmergencyRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeEmergencyRate) | brakePipeEmergencyRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake pipe emergency rate defined for Car " + carIndex + " in XML file " + fileName);
											brakePipeEmergencyRate = 400000.0;
										}
										break;
									case BrakeXMLKey.Volume:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeVolume) | brakePipeVolume <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake pipe volume defined for Car " + carIndex + " in XML file " + fileName);
											brakePipeVolume = Math.Pow(0.0175 * Math.PI, 2) * Train.Cars.Length;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.StraightAirPipe:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.ServiceRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeServiceRate) | straightAirPipeServiceRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid straight air pipe service rate defined for Car " + carIndex + " in XML file " + fileName);
											 straightAirPipeServiceRate = 300000.0;
										}
										break;
									case BrakeXMLKey.EmergencyRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeEmergencyRate) | straightAirPipeEmergencyRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + carIndex + " in XML file " + fileName);
											straightAirPipeEmergencyRate = 400000.0;
										}
										break;
									case BrakeXMLKey.ReleaseRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeReleaseRate) | straightAirPipeReleaseRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + carIndex + " in XML file " + fileName);
											straightAirPipeReleaseRate = 200000.0;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.BrakeCylinder:
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								Enum.TryParse(cc.Name, true, out key);
								switch (key)
								{
									case BrakeXMLKey.ServiceMaximumPressure:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderServiceMaximumPressure) | brakeCylinderServiceMaximumPressure <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake cylinder service pressure defined for Car " + carIndex + " in XML file " + fileName);
											brakeCylinderServiceMaximumPressure = 440000.0;
										}
										break;
									case BrakeXMLKey.EmergencyMaximumPressure:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderEmergencyMaximumPressure) | brakeCylinderEmergencyMaximumPressure <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + carIndex + " in XML file " + fileName);
											brakeCylinderEmergencyMaximumPressure = 440000.0;
										}
										break;
									case BrakeXMLKey.EmergencyRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderEmergencyRate) | brakeCylinderEmergencyRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + carIndex + " in XML file " + fileName);
											brakeCylinderEmergencyRate = 300000.0;
										}
										break;
									case BrakeXMLKey.ReleaseRate:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderReleaseRate) | brakeCylinderReleaseRate <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + carIndex + " in XML file " + fileName);
											brakeCylinderReleaseRate = 200000.0;
										}
										break;
									case BrakeXMLKey.Volume:
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderVolume) | brakeCylinderVolume <= 0.0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid brake cylinder volume defined for Car " + carIndex + " in XML file " + fileName);
											brakeCylinderVolume = 0.5;
										}
										break;
								}
							}
						}
						break;
					case BrakeXMLKey.Handle:
						ParseHandleNode(c, ref Train.Handles.Brake, carIndex, Train, fileName);
						break;
					case BrakeXMLKey.LegacyPressureDistribution:
						if (c.InnerText == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							Train.Specs.AveragesPressureDistribution = true;
						}
						else
						{
							Train.Specs.AveragesPressureDistribution = false;
						}
						break;
					
				}
			}
			
			Train.Cars[carIndex].CarBrake.MainReservoir = new MainReservoir(compressorMinimumPressure, compressorMaximumPressure, 0.01, (Train.Handles.Brake is AirBrakeHandle ? 0.25 : 0.075) / Train.Cars.Length);
			Train.Cars[carIndex].CarBrake.MainReservoir.Volume = mainReservoirVolume;
			AirBrake airBrake = Train.Cars[carIndex].CarBrake as AirBrake;
			airBrake.Compressor = new Compressor(5000.0, Train.Cars[carIndex].CarBrake.MainReservoir, Train.Cars[carIndex]);
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
