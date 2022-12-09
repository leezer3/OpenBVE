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

using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
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
				}
			}

			// somewhat inelegant, but this gives us a better XML structure as we need all the properties defined
			// FIXME: Should probably check the max against the current etc.
			steamEngine.Boiler = new Boiler(steamEngine, waterLevel, maxWaterLevel, steamPressure, maxSteamPressure, blowoffPressure, minWorkingPressure, steamGenerationRate);
			steamEngine.Boiler.LiveSteamInjector.BaseInjectionRate = liveSteamInjectionRate;
			steamEngine.Boiler.ExhaustSteamInjector.BaseInjectionRate = exhaustSteamInjectionRate;
			steamEngine.Boiler.Firebox = new Firebox(steamEngine, fireArea, maxFireArea, maxFireTemp, fireConversionRate, shovelSize);

			Train.Cars[Car].TractionModel = steamEngine;
		}
	}
}

