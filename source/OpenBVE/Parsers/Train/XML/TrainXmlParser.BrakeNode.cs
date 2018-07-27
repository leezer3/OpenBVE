using System.Linq;
using System.Xml;
using OpenBveApi.Math;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static void ParseBrakeNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train)
		{
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					case "compressor":
						Train.Cars[Car].Specs.AirBrake.Type = TrainManager.AirBrakeType.Main; //We have a compressor so must be a main brake type
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "rate":
										double r;
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid compression rate defined for Car " + Car + " in XML file " + fileName);
											r = 5000.0;
										}
										Train.Cars[Car].Specs.AirBrake.AirCompressorRate = r;
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
								double r;
								switch (cc.Name.ToLowerInvariant())
								{
									case "minimumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid main reservoir minumum pressure defined for Car " + Car + " in XML file " + fileName);
											r = 690000.0;
										}
										Train.Cars[Car].Specs.AirBrake.AirCompressorMinimumPressure = r;
										break;
									case "maximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid main reservoir maximum pressure defined for Car " + Car + " in XML file " + fileName);
											r = 780000.0;
										}
										Train.Cars[Car].Specs.AirBrake.AirCompressorMaximumPressure = r;
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
										double r;
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid auxiliary reservoir charge rate defined for Car " + Car + " in XML file " + fileName);
											r = 200000.0;
										}
										Train.Cars[Car].Specs.AirBrake.AuxillaryReservoirChargeRate = r;
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
								double r;
								switch (cc.Name.ToLowerInvariant())
								{
									case "chargerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid equalizing reservoir charge rate defined for Car " + Car + " in XML file " + fileName);
											r = 50000.0;
										}
										Train.Cars[Car].Specs.AirBrake.EqualizingReservoirChargeRate = r;
										break;
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid equalizing reservoir service rate defined for Car " + Car + " in XML file " + fileName);
											r = 50000.0;
										}
										Train.Cars[Car].Specs.AirBrake.EqualizingReservoirServiceRate = r;
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid equalizing reservoir emergency rate defined for Car " + Car + " in XML file " + fileName);
											r = 50000.0;
										}
										Train.Cars[Car].Specs.AirBrake.EqualizingReservoirServiceRate = r;
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
								double r;
								switch (cc.Name.ToLowerInvariant())
								{
									case "normalpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake pipe normal pressure defined for Car " + Car + " in XML file " + fileName);
											r = 0.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakePipeNormalPressure = r;
										break;
									case "chargerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake pipe charge rate defined for Car " + Car + " in XML file " + fileName);
											r = 10000000.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakePipeChargeRate = r;
										break;
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake pipe service rate defined for Car " + Car + " in XML file " + fileName);
											r = 1500000.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakePipeServiceRate = r;
										break;
									case "emergencyrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											r = 400000.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakePipeEmergencyRate = r;
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
								double r;
								switch (cc.Name.ToLowerInvariant())
								{
									case "servicerate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid straight air pipe service rate defined for Car " + Car + " in XML file " + fileName);
											r = 300000.0;
										}
										Train.Cars[Car].Specs.AirBrake.StraightAirPipeServiceRate = r;
										break;
									case "emergencyrate":
										
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											r = 400000.0;
										}
										Train.Cars[Car].Specs.AirBrake.StraightAirPipeEmergencyRate = r;
										break;
									case "releaserate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid straight air pipe emergency rate defined for Car " + Car + " in XML file " + fileName);
											r = 200000.0;
										}
										Train.Cars[Car].Specs.AirBrake.StraightAirPipeReleaseRate = r;
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
								double r;
								switch (cc.Name.ToLowerInvariant())
								{
									case "servicemaximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake cylinder service pressure defined for Car " + Car + " in XML file " + fileName);
											r = 440000.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakeCylinderServiceMaximumPressure = r;
										break;
									case "emergencymaximumpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out r) | r <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid brake cylinder emergency pressure defined for Car " + Car + " in XML file " + fileName);
											r = 440000.0;
										}
										Train.Cars[Car].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure = r;
										break;
								}
							}
						}
						break;
				}
			}
		}
	}
}
