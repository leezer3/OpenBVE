using System.Linq;
using System.Xml;
using OpenBveApi.Math;
using OpenBve.BrakeSystems;
using OpenBveApi.Interface;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static void ParseBrakeNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train)
		{
			double compressorRate = 5000.0, compressorMinimumPressure = 690000.0, compressorMaximumPressure = 780000.0;
			double auxiliaryReservoirChargeRate = 200000.0;
			double equalizingReservoirChargeRate = 200000.0, equalizingReservoirServiceRate = 50000.0, equalizingReservoirEmergencyRate = 250000.0;
			double brakePipeNormalPressure = 0.0, brakePipeChargeRate = 10000000.0, brakePipeServiceRate = 1500000.0, brakePipeEmergencyRate = 5000000.0;
			double straightAirPipeServiceRate = 300000.0, straightAirPipeEmergencyRate = 400000.0, straightAirPipeReleaseRate = 200000.0;
			double brakeCylinderServiceMaximumPressure = 440000.0, brakeCylinderEmergencyMaximumPressure = 440000.0, brakeCylinderEmergencyRate = 300000.0, brakeCylinderReleaseRate = 200000.0;
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					case "compressor":
						Train.Cars[Car].CarBrake.brakeType = BrakeType.Main; //We have a compressor so must be a main brake type
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "rate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorRate) | compressorRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid compression rate defined for Car " + Car + " in XML file " + fileName);
											compressorRate = 5000.0;
										}
										break;
								}
							}
						}
						break;
					case "mainreservoir":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "minimumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorMinimumPressure) | compressorMinimumPressure <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid main reservoir minumum pressure defined for Car " + Car + " in XML file " + fileName);
											compressorMinimumPressure = 690000.0;
										}
										break;
									case "maximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out compressorMaximumPressure) | compressorMaximumPressure <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid main reservoir maximum pressure defined for Car " + Car + " in XML file " + fileName);
											compressorMaximumPressure = 780000.0;
										}
										break;
								}
							}
						}
						break;
					case "auxiliaryreservoir":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "chargerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out auxiliaryReservoirChargeRate) | auxiliaryReservoirChargeRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid auxiliary reservoir charge rate defined for Car " + Car + " in XML file " + fileName);
											auxiliaryReservoirChargeRate = 200000.0;
										}
										break;
								}
							}
						}
						break;
					case "equalizingreservoir":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "chargerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirChargeRate) | equalizingReservoirChargeRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir charge rate defined for Car " + Car + " in XML file " + fileName);
											equalizingReservoirChargeRate = 50000.0;
										}
										break;
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirServiceRate) | equalizingReservoirServiceRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir service rate defined for Car " + Car + " in XML file " + fileName);
											equalizingReservoirServiceRate = 50000.0;
										}
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out equalizingReservoirEmergencyRate) | equalizingReservoirEmergencyRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid equalizing reservoir emergency rate defined for Car " + Car + " in XML file " + fileName);
											equalizingReservoirEmergencyRate = 50000.0;
										}
										break;
								}
							}
						}
						break;
					case "brakepipe":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "normalpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeNormalPressure) | brakePipeNormalPressure <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake pipe normal pressure defined for Car " + Car + " in XML file " + fileName);
											brakePipeNormalPressure = 0.0;
										}
										break;
									case "chargerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeChargeRate) | brakePipeChargeRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake pipe charge rate defined for Car " + Car + " in XML file " + fileName);
											brakePipeChargeRate = 10000000.0;
										}
										break;
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeServiceRate) | brakePipeServiceRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake pipe service rate defined for Car " + Car + " in XML file " + fileName);
											brakePipeServiceRate = 1500000.0;
										}
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakePipeEmergencyRate) | brakePipeEmergencyRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											brakePipeEmergencyRate = 400000.0;
										}
										break;
								}
							}
						}
						break;
					case "straightairpipe":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeServiceRate) | straightAirPipeServiceRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid straight air pipe service rate defined for Car " + Car + " in XML file " + fileName);
											 straightAirPipeServiceRate = 300000.0;
										}
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeEmergencyRate) | straightAirPipeEmergencyRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											straightAirPipeEmergencyRate = 400000.0;
										}
										break;
									case "releaserate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out straightAirPipeReleaseRate) | straightAirPipeReleaseRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											straightAirPipeReleaseRate = 200000.0;
										}
										break;
								}
							}
						}
						break;
					case "brakecylinder":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "servicemaximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderServiceMaximumPressure) | brakeCylinderServiceMaximumPressure <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake cylinder service pressure defined for Car " + Car + " in XML file " + fileName);
											brakeCylinderServiceMaximumPressure = 440000.0;
										}
										break;
									case "emergencymaximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderEmergencyMaximumPressure) | brakeCylinderEmergencyMaximumPressure <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + Car + " in XML file " + fileName);
											brakeCylinderEmergencyMaximumPressure = 440000.0;
										}
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderEmergencyRate) | brakeCylinderEmergencyRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + Car + " in XML file " + fileName);
											brakeCylinderEmergencyRate = 300000.0;
										}
										break;
									case "releaserate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out brakeCylinderReleaseRate) | brakeCylinderReleaseRate <= 0.0)
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + Car + " in XML file " + fileName);
											brakeCylinderReleaseRate = 200000.0;
										}
										break;
								}
							}
						}
						break;
				}
			}
			Train.Cars[Car].CarBrake.airCompressor = new Compressor(compressorRate);
			Train.Cars[Car].CarBrake.mainReservoir = new MainReservoir(compressorMinimumPressure, compressorMaximumPressure, 0.01, (Train.Handles.Brake is TrainManager.AirBrakeHandle ? 0.25 : 0.075) / Train.Cars.Length);
			Train.Cars[Car].CarBrake.equalizingReservoir = new EqualizingReservoir(equalizingReservoirServiceRate, equalizingReservoirEmergencyRate, equalizingReservoirChargeRate);
			Train.Cars[Car].CarBrake.equalizingReservoir.NormalPressure = 1.005 * brakePipeNormalPressure;
				
			Train.Cars[Car].CarBrake.brakePipe = new BrakePipe(brakePipeNormalPressure, brakePipeChargeRate, brakePipeServiceRate, brakePipeEmergencyRate, Train.Cars[0].CarBrake is ElectricCommandBrake);
			{
				double r = 200000.0 / brakeCylinderEmergencyMaximumPressure - 1.0;
				if (r < 0.1) r = 0.1;
				if (r > 1.0) r = 1.0;
				Train.Cars[Car].CarBrake.auxiliaryReservoir = new AuxiliaryReservoir(0.975 * brakePipeNormalPressure, auxiliaryReservoirChargeRate, 0.5, r);
			}
			Train.Cars[Car].CarBrake.brakeCylinder = new BrakeCylinder(brakeCylinderServiceMaximumPressure, brakeCylinderEmergencyMaximumPressure, Train.Handles.Brake is TrainManager.AirBrakeHandle ? brakeCylinderEmergencyRate : 0.3 * brakeCylinderEmergencyRate, brakeCylinderEmergencyRate, brakeCylinderReleaseRate);
			Train.Cars[Car].CarBrake.straightAirPipe = new StraightAirPipe(straightAirPipeServiceRate, straightAirPipeEmergencyRate, straightAirPipeReleaseRate);
		}
	}
}
