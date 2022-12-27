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
using TrainManager.Cargo;
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
			/*
			 * --------------------------------------------------------------------------------------
			 * Fudged average numbers here at the minute, based upon a hypothetical large tender loco
			 * --------------------------------------------------------------------------------------
			 *
			 * Boiler: 
			 *			2000L starting level
			 *			3000L capacity
			 *			200psi starting pressure
			 *			240psi absolute max pressure
			 *			220psi blowoff pressure
			 *			120psi minimum working pressure
			 *			1L water ==> 4.15psi steam ==> divide by 60min for rate / min ==> divide by 60 for rate /s
			 *			3L /s injection rate (Davies & Metcalfe Monitor Type 11 injector, typically used on large tender locos)
			 */
			double waterLevel = 2000, maxWaterLevel = 3000, steamPressure = 200, maxSteamPressure = 240, blowoffPressure = 220, minWorkingPressure = 120, steamGenerationRate = 0.00152, liveSteamInjectionRate = 3.0, exhaustSteamInjectionRate = 3.0;
			/*
			 * Firebox:
			 *			7m² starting area
			 *			10m² max area
			 *			1000c max temp
			 *			0.1kg burnt per second at max size
			 *			3kg shovel size
			 */
			double fireArea = 7, maxFireArea = 10, maxFireTemp = 1000, fireConversionRate = 0.1, shovelSize = 3;
			/*
			 * Cylinder Chest
			 *			0.005psi standing pressure loss (leakage etc.)
			 *			0.2psi base stroke pressure, before reduction due to regulator / cutoff
			 *			5psi / second additional pressure loss when cylinder cocks are open with full regulator
			 */
			double cylinderChestPressureLoss = 0.005, cylinderChestBasePressureUse = 0.02, cylinderCockLeakRate = 5;
			BypassValveType bypassValveType = BypassValveType.None;
			/*
			 * Valve gear has no starting properties, too loco specific
			 */
			List<ValveGearRod> valveGearRods = new List<ValveGearRod>();
			List<ValveGearPivot> valveGearPivots = new List<ValveGearPivot>();
			double wheelCircumference = 0;
			/*
			 * Tender:
			 *		Coal capacity of 40T
			 *		Water capacity of 88,000L (~19,200 gallons)
			 *
			 */
			int tenderCar = Car;
			double fuelCapacity = 40000, fuelLoad = 40000, waterCapacity = 88000, waterLoad = 88000;
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
									case "cylindercockleakrate":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out cylinderCockLeakRate) | cylinderCockLeakRate <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid cylinder cock leak rate defined for Steam Engine Cylinder Chest " + Car + " in XML file " + fileName);
											cylinderChestBasePressureUse = 0.005;
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
									case "wheelcircumference":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out wheelCircumference) | wheelCircumference <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid wheel circumference defined for Steam Engine Valve Gear " + Car + " in XML file " + fileName);
											cylinderChestBasePressureUse = 0.005;
										}
										break;
								}
							}
						}
						break;
					case "tender":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "carindex":
										if (!NumberFormats.TryParseIntVb6(cc.InnerText, out tenderCar) | tenderCar < 0 | tenderCar > Train.Cars.Length - 1)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid tender car index defined for Steam Engine " + Car + " in XML file " + fileName);
											cylinderChestPressureLoss = 0.005;
										}
										break;
									case "fuelcapacity":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out fuelCapacity) | fuelCapacity <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Fuel Capacity defined for Steam Engine Tender " + Car + " in XML file " + fileName);
											fuelCapacity = 40000;
										}
										break;
									case "fuelload":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out fuelLoad) | fuelLoad <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Fuel Load defined for Steam Engine Tender " + Car + " in XML file " + fileName);
											fuelLoad = 40000;
										}
										break;
									case "watercapacity":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out waterCapacity) | waterCapacity <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Water Capacity defined for Steam Engine Tender " + Car + " in XML file " + fileName);
											waterCapacity = 88000;
										}
										break;
									case "waterload":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out waterLoad) | waterLoad <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Water Load defined for Steam Engine Tender " + Car + " in XML file " + fileName);
											waterLoad = 88000;
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
			steamEngine.CylinderChest.ValveGear = new ValveGear(steamEngine, wheelCircumference, valveGearRods.ToArray(), valveGearPivots.ToArray());
			steamEngine.CylinderChest.CylinderCocks = new CylinderCocks(steamEngine, cylinderCockLeakRate);
			if (fuelCapacity < fuelLoad)
			{
				fuelLoad = fuelCapacity;
			}

			if (waterCapacity < waterLoad)
			{
				waterLoad = waterCapacity;
			}
			Tender tender = new Tender(fuelLoad, fuelCapacity, waterLevel, waterLoad);
			steamEngine.Tender = tender;
			Train.Cars[tenderCar].Cargo = tender;
			Train.Cars[Car].TractionModel = steamEngine;
		}
	}
}

