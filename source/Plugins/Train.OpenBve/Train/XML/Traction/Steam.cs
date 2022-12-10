//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainManager.TractionModels;
using TrainManager.TractionModels.Steam;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseSteamEngineNode(XmlNode Node, string fileName, int Car, ref TrainBase Train)
		{
			SteamEngine steamEngine = new SteamEngine(Train.Cars[Car], 10, 10);
			// boiler properties
			double waterLevel = 2000, maxWaterLevel = 3000, steamPressure = 200, maxSteamPressure = 240, blowoffPressure = 220, minWorkingPressure = 120, steamGenerationRate = 0.00152, liveSteamInjectionRate = 3.0, exhaustSteamInjectionRate = 3.0;
			// firebox properties
			double fireArea = 7, maxFireArea = 10, maxFireTemp = 1000, fireConversionRate = 0.1, shovelSize = 3;
			// cylinder chest properties
			double cylinderChestPressureLoss = 0.005, cylinderChestBasePressureUse = 0.02;
			BypassValveType bypassValveType = BypassValveType.None;
			// valve gear properties
			List<ValveGearRod> valveGearRods = new List<ValveGearRod>();
			List<ValveGearPivot> valveGearPivots = new List<ValveGearPivot>();
			foreach (XmlNode c in Node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "boiler":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "waterlevel":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out waterLevel) | waterLevel <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid starting water level defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											waterLevel = 3000.0;
										}
										break;
									case "maxwaterlevel":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxWaterLevel) | maxWaterLevel <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid max water level defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											maxWaterLevel = 3000.0;
										}
										break;
									case "steampressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out steamPressure) | steamPressure <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid starting pressure defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											steamPressure = 200.0;
										}
										break;
									case "maxsteampressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxSteamPressure) | maxSteamPressure <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid max pressure defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											maxSteamPressure = 200.0;
										}
										break;
									case "blowoffpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out blowoffPressure) | blowoffPressure <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid blowoff pressure defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											blowoffPressure = 200.0;
										}
										break;
									case "minworkingpressure":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out minWorkingPressure) | minWorkingPressure <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid minimum pressure defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											minWorkingPressure = 200.0;
										}
										break;
									case "steamgenerationrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out steamGenerationRate) | steamGenerationRate <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid minimum pressure defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											steamGenerationRate = 0.00152;
										}
										break;
									case "livesteaminjectionrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out liveSteamInjectionRate) | liveSteamInjectionRate <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid live steam injection rate defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											liveSteamInjectionRate = 3.0;
										}
										break;
									case "exhauststeaminjectionrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out exhaustSteamInjectionRate) | exhaustSteamInjectionRate <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid exhaust steam injection rate defined for Steam Engine Boiler " + Car + " in XML file " + fileName);
											exhaustSteamInjectionRate = 3.0;
										}
										break;
								}
							}
						}
						break;
					case "firebox":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "maxarea":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxFireArea) | maxFireArea <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid max fire area defined for Steam Engine Firebox " + Car + " in XML file " + fileName);
											maxFireArea = 10;
										}
										break;
									case "area":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out fireArea) | fireArea <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid starting fire area defined for Steam Engine Firebox " + Car + " in XML file " + fileName);
											fireArea = 7;
										}
										break;
									case "maxtemp":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxFireTemp) | maxFireTemp <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid max fire temp defined for Steam Engine Firebox " + Car + " in XML file " + fileName);
											maxFireTemp = 5;
										}
										break;
									case "conversionrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out fireConversionRate) | fireConversionRate <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid conversion rate defined for Steam Engine Firebox " + Car + " in XML file " + fileName);
											fireConversionRate = 5;
										}
										break;
									case "shovelsize":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out shovelSize) | shovelSize <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid shovel size defined for Steam Engine Firebox " + Car + " in XML file " + fileName);
											shovelSize = 3;
										}
										break;
								}
							}
						}
						break;
					case "cylinderchest":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "standingpressureloss":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out cylinderChestPressureLoss) | cylinderChestPressureLoss <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid standing pressure loss defined for Steam Engine Cylinder Chest " + Car + " in XML file " + fileName);
											cylinderChestPressureLoss = 0.005;
										}
										break;
									case "basepressureuse":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out cylinderChestBasePressureUse) | cylinderChestBasePressureUse <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid base pressure use defined for Steam Engine Cylinder Chest " + Car + " in XML file " + fileName);
											cylinderChestBasePressureUse = 0.005;
										}
										break;
									case "sniftingvalve":
									case "bypassvalve":
										if (!Enum.TryParse(cc.InnerText, true, out bypassValveType))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid bypass (snifting) valve type use defined for Steam Engine Cylinder Chest " + Car + " in XML file " + fileName);
										}
										break;
								}
							}
						}
						break;
					case "valvegear":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "crank":
										if (cc.ChildNodes.OfType<XmlElement>().Any())
										{
											double radius = 0, length = 0, rotationalOffset = 0;
											foreach (XmlNode ccc in c.ChildNodes)
											{
												switch (ccc.Name.ToLowerInvariant())
												{
													case "radius":
														if (!NumberFormats.TryParseDoubleVb6(ccc.InnerText, out radius) | radius <= 0.0)
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Radius for Valve Gear Crank " + valveGearRods.Count + " defined for Steam Engine " + Car + " in XML file " + fileName);
														}
														break;
													case "length":
														if (!NumberFormats.TryParseDoubleVb6(ccc.InnerText, out length) | length <= 0.0)
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Length for Valve Gear Crank " + valveGearRods.Count + " defined for Steam Engine " + Car + " in XML file " + fileName);
														}
														break;
													case "offset":
														if (!NumberFormats.TryParseDoubleVb6(ccc.InnerText, out rotationalOffset))
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Rotational Offset for Valve Gear Crank " + valveGearRods.Count + " defined for Steam Engine " + Car + " in XML file " + fileName);
														}
														break;
												}
											}
											if (radius != 0 && length != 0)
											{
												valveGearRods.Add(new ValveGearRod(steamEngine, radius, length, rotationalOffset));
											}
										}
										break;
									case "pivot":
										if (cc.ChildNodes.OfType<XmlElement>().Any())
										{
											double radius = 0, rotationalOffset = 0;
											foreach (XmlNode ccc in c.ChildNodes)
											{
												switch (ccc.Name.ToLowerInvariant())
												{
													case "radius":
														if (!NumberFormats.TryParseDoubleVb6(ccc.InnerText, out radius) | radius <= 0.0)
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Radius for Valve Gear Crank " + valveGearRods.Count + " defined for Steam Engine " + Car + " in XML file " + fileName);
														}
														break;
													case "offset":
														if (!NumberFormats.TryParseDoubleVb6(ccc.InnerText, out rotationalOffset))
														{
															Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Rotational Offset for Valve Gear Crank " + valveGearRods.Count + " defined for Steam Engine " + Car + " in XML file " + fileName);
														}
														break;
												}
											}
											if (radius != 0)
											{
												valveGearPivots.Add(new ValveGearPivot(radius, rotationalOffset));
											}
										}
										break;
								}
							}
						}
						break;
				}
			}

			// somewhat inelegant, but this gives us a better XML structure as we need all the properties defined
			// FIXME: Should probably check the max against the current etc.
			// IDEA: Is passing through constructed boiler, firebox injectors etc. better as that would allow readonly, whilst avoiding parameter bloat?
			steamEngine.Boiler = new Boiler(steamEngine, waterLevel, maxWaterLevel, steamPressure, maxSteamPressure, blowoffPressure, minWorkingPressure, steamGenerationRate);
			steamEngine.Boiler.LiveSteamInjector.BaseInjectionRate = liveSteamInjectionRate;
			steamEngine.Boiler.ExhaustSteamInjector.BaseInjectionRate = exhaustSteamInjectionRate;
			steamEngine.Boiler.Firebox = new Firebox(steamEngine, fireArea, maxFireArea, maxFireTemp, fireConversionRate, shovelSize);
			steamEngine.CylinderChest = new CylinderChest(steamEngine, cylinderChestPressureLoss, cylinderChestBasePressureUse);
			steamEngine.CylinderChest.BypassValve.Type = bypassValveType;
			steamEngine.CylinderChest.ValveGear.CrankRods = valveGearRods.ToArray();
			steamEngine.CylinderChest.ValveGear.Pivots = valveGearPivots.ToArray();
			Train.Cars[Car].TractionModel = steamEngine;
		}
	}
}

