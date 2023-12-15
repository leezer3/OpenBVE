﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseCarNode(XmlNode Node, string fileName, int Car, ref TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref bool visibleFromInterior)
		{
			string interiorFile = string.Empty;
			ReadhesionDeviceType readhesionDevice = ReadhesionDeviceType.NotFitted;
			if (Train.Cars[0].ReAdhesionDevice is BveReAdhesionDevice device)
			{
				readhesionDevice = device.DeviceType;
			}
			bool CopyAccelerationCurves = true;
			bool exposedFrontalAreaSet = false;
			bool unexposedFrontalAreaSet = false;
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					case "camerarestriction":
						Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.Restricted3D;
						foreach (XmlNode cc in c.ChildNodes)
						{
							switch (cc.Name.ToLowerInvariant())
							{
								case "backwards":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.Z))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid backwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "forwards":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Z))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid forwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "left":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.X))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid left camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "right":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.X))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid right camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "down":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.Y))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid down camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "up":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Y))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid up camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
							}
						}
						break;
					case "brake":
						Train.Cars[Car].CarBrake.brakeType = BrakeType.Auxiliary;
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							ParseBrakeNode(c, fileName, Car, ref Train);
						}
						else if (!String.IsNullOrEmpty(c.InnerText))
						{
							try
							{
								string childFile = Path.CombineFile(currentPath, c.InnerText);
								XmlDocument childXML = new XmlDocument();
								childXML.Load(childFile);
								XmlNodeList childNodes = childXML.DocumentElement.SelectNodes("/openBVE/Brake");
								//We need to save and restore the current path to make relative paths within the child file work correctly
								string savedPath = currentPath;
								currentPath = Path.GetDirectoryName(childFile);
								ParseBrakeNode(childNodes[0], fileName, Car, ref Train);
								currentPath = savedPath;
							}
							catch(Exception ex)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Failed to load the child Brake XML file specified in " +c.InnerText);
								Plugin.currentHost.AddMessage(MessageType.Error, false, "The error encountered was " + ex);
							}
						}
						break;
					case "length":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double l) | l <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid length defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Length = l;
						break;
					case "width":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double w) | w <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid width defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Width = w;
						break;
					case "height":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double h) | h <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid height defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Height = h;
						break;
					case "motorcar":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							Train.Cars[Car].Specs.IsMotorCar = true;
							if (!CopyAccelerationCurves)
							{
								//We've already set the acceleration curves elsewhere in the XML, so don't copy the default ones
								break;
							}
							Train.Cars[Car].Specs.AccelerationCurves = new AccelerationCurve[Plugin.AccelerationCurves.Length];
							for (int i = 0; i < Plugin.AccelerationCurves.Length; i++)
							{
								Train.Cars[Car].Specs.AccelerationCurves[i] = Plugin.AccelerationCurves[i].Clone();
							}

							Train.Cars[Car].Specs.AccelerationCurveMaximum = Plugin.MaximumAcceleration;
						}
						else
						{
							Train.Cars[Car].Specs.AccelerationCurves = new AccelerationCurve[] { };
							Train.Cars[Car].Specs.IsMotorCar = false;
						}
						break;
					case "mass":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double m) | m <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid mass defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].EmptyMass = m;
						Train.Cars[Car].CargoMass = 0;
						break;
					case "centerofgravityheight":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double cg) | cg <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid CenterOfGravityHeight defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.CenterOfGravityHeight = cg;
						break;
					case "exposedfrontalarea":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double ef) | ef <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid ExposedFrontalArea defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.ExposedFrontalArea = ef;
						exposedFrontalAreaSet = true;
						break;
					case "unexposedfrontalarea":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double uf) | uf <= 0.0)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid UnexposedFrontalArea defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.UnexposedFrontalArea = uf;
						unexposedFrontalAreaSet = true;
						break;
					case "frontaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].FrontAxle.Position))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid front axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "rearaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].RearAxle.Position))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid rear axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "object":
						if (string.IsNullOrEmpty(c.InnerText))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid object path for Car " + Car + " in XML file " + fileName);
							break;
						}
						string f = Path.CombineFile(currentPath, c.InnerText);
						if (System.IO.File.Exists(f))
						{
							Plugin.currentHost.LoadObject(f, Encoding.Default, out CarObjects[Car]);
						}
						break;
					case "reversed":
						NumberFormats.TryParseIntVb6(c.InnerText, out int n);
						if (n == 1 || c.InnerText.ToLowerInvariant() == "true")
						{
							CarObjectsReversed[Car] = true;
						}
						break;
					case "loadingsway":
						NumberFormats.TryParseIntVb6(c.InnerText, out int nm);
						if (nm == 1 || c.InnerText.ToLowerInvariant() == "true")
						{
							Train.Cars[Car].EnableLoadingSway = true;
						}
						else
						{
							Train.Cars[Car].EnableLoadingSway = false;
						}
						break;
					case "frontbogie":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "frontaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].FrontBogie.FrontAxle.Position))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].FrontBogie.RearAxle.Position))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Plugin.currentHost.LoadObject(fb, Encoding.Default, out BogieObjects[Car * 2]);
										}
										break;
									case "reversed":
										int nn;
										NumberFormats.TryParseIntVb6(cc.InnerText, out nn);
										if (cc.InnerText.ToLowerInvariant() == "true" || nn == 1)
										{
											BogieObjectsReversed[Car * 2] = true;
										}
										break;
								}
							}
						}
						break;
					case "rearbogie":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "frontaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].RearBogie.FrontAxle.Position))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].RearBogie.RearAxle.Position))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Plugin.currentHost.LoadObject(fb, Encoding.Default, out BogieObjects[Car * 2 + 1]);
										}
										break;
									case "reversed":
										NumberFormats.TryParseIntVb6(cc.InnerText, out int nn);
										if (cc.InnerText.ToLowerInvariant() == "true" || nn == 1)
										{
											BogieObjectsReversed[Car * 2 + 1] = true;
										}
										break;
								}
							}
						}
						break;
					case "driverposition":
						string[] splitText = c.InnerText.Split(',');
						if (splitText.Length != 3)
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Driver position must have three arguments for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Driver = new Vector3();
						double driverZ;
						if (!NumberFormats.TryParseDoubleVb6(splitText[0], out Train.Cars[Car].Driver.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Driver position X was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[1], out Train.Cars[Car].Driver.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Driver position Y was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[2], out driverZ))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Driver position X was invalid for Car " + Car + " in XML file " + fileName);
						}
						Train.Cars[Car].Driver.Z = 0.5 * Train.Cars[Car].Length + driverZ;
						break;
					case "interiorview":
						if (!Train.IsPlayerTrain)
						{
							break;
						}
						Train.Cars[Car].HasInteriorView = true;
						Train.Cars[Car].CarSections = new CarSection[1];
						Train.Cars[Car].CarSections[0] = new CarSection(Plugin.currentHost, ObjectType.Overlay, true);

						string cv = Path.CombineFile(currentPath, c.InnerText);
						if (!System.IO.File.Exists(cv))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Interior view file was invalid for Car " + Car + " in XML file " + fileName);
							break;
						}
						interiorFile = cv;
						break;
					case "readhesiondevice":
						switch (c.InnerText.ToLowerInvariant())
						{
							case "typea":
							case "a":
								readhesionDevice = ReadhesionDeviceType.TypeA;
								break;
							case "typeb":
							case "b":
								readhesionDevice = ReadhesionDeviceType.TypeB;
								break;
							case "typec":
							case "c":
								readhesionDevice = ReadhesionDeviceType.TypeC;
								break;
							case "typed":
							case "d":
								readhesionDevice = ReadhesionDeviceType.TypeD;
								break;
							default:
								readhesionDevice = ReadhesionDeviceType.NotFitted;
								break;
						}
						break;
					case "sanders":
						SandersType type = SandersType.NotFitted;
						double rate = double.MaxValue;
						double level = 0;
						double applicationTime = 10.0;
						double activationTime = 5.0;
						int shots = int.MaxValue;
						foreach (XmlNode cc in c.ChildNodes)
						{
							switch (cc.Name.ToLowerInvariant())
							{
								case "type":
									if (!Enum.TryParse(cc.InnerText, true, out type))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sanders type was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "rate":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out rate))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sanders application rate was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "sandlevel":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out level))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sand level was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "numberofshots":
									if (!NumberFormats.TryParseIntVb6(cc.InnerText, out shots))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sand level was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "applicationtime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out applicationTime))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sanders application time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "activationtime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out activationTime))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Sanders activation time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
							}

							Train.Cars[Car].ReAdhesionDevice = new Sanders(Train.Cars[Car], type)
							{
								ApplicationTime = applicationTime,
								ActivationTime = activationTime,
								SandLevel = level,
								SandingRate = rate,
								NumberOfShots = shots
							};
						}

						break;
					case "visiblefrominterior":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							visibleFromInterior = true;
						}
						break;
					case "soundtable":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "bve5":
										string powerFreq = string.Empty, powerVol = string.Empty;
										string brakeFreq = string.Empty, brakeVol = string.Empty;
										foreach (XmlNode sc in cc.ChildNodes)
										{
											switch (sc.Name.ToLowerInvariant())
											{
												case "powerfreq":
													powerFreq = Path.CombineFile(currentPath, sc.InnerText);
													break;
												case "powervol":
													powerVol = Path.CombineFile(currentPath, sc.InnerText);
													break;
												case "brakefreq":
													brakeFreq = Path.CombineFile(currentPath, sc.InnerText);
													break;
												case "brakevol":
													brakeVol = Path.CombineFile(currentPath, sc.InnerText);
													break;
											}
										}
										Train.Cars[Car].Sounds.Motor = Bve5MotorSoundTableParser.Parse(Train.Cars[Car], powerFreq, powerVol, brakeFreq, brakeVol);
										break;
								}
							}
						}
						break;
					case "accelerationcurves":
						/*
						 * NOTE: This was initially implemented here.
						 * It has moved to being a child-node of the power node
						 * Retain this for the minute in case someone has actually used the thing (although the format is an ongoing WIP)....
						 */
						CopyAccelerationCurves = false;
						Train.Cars[Car].Specs.AccelerationCurves = ParseAccelerationNode(c, fileName);
						break;
					case "power":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "handle":
										AbstractHandle p = Train.Handles.Power; // yuck, but we can't store this as the base type due to constraints elsewhere
										ParseHandleNode(cc, ref p, Car, Train, fileName);
										break;
									case "accelerationcurves":
										CopyAccelerationCurves = false;
										Train.Cars[Car].Specs.AccelerationCurves = ParseAccelerationNode(cc, fileName);
										break;
								}
							}

						}
						break;
					case "doors":
						double doorWidth = 1.0;
						double doorTolerance = 0.0;
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "openspeed":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out double os))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid door opening speed defined for Car " + Car + " in XML file " + fileName);
										}
										else
										{
											Train.Cars[Car].Specs.DoorOpenFrequency = 1.0 / os;
										}
										break;
									case "closespeed":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out double cs))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid door opening speed defined for Car " + Car + " in XML file " + fileName);
										}
										else
										{
											Train.Cars[Car].Specs.DoorCloseFrequency = 1.0 / cs;
										}
										break;
									case "width":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out doorWidth))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid door width defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "tolerance":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out doorWidth))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid door closing tolerance defined for Car " + Car + " in XML file " + fileName);
										}
										break;
								}
							}
						}
						// XML uses meters for all units to be consistant, so convert to mm for door usage
						doorWidth *= 1000.0;
						doorTolerance *= 1000.0;
						Train.Cars[Car].Doors[0] = new Door(-1, doorWidth, doorTolerance);
						Train.Cars[Car].Doors[1] = new Door(1, doorWidth, doorTolerance);
						break;
					case "cargo":
						switch (c.InnerText.ToLowerInvariant())
						{
							case "passengers":
								Train.Cars[Car].Cargo = new Passengers(Train.Cars[Car]);
								break;
							case "freight":
								Train.Cars[Car].Cargo = new RobustFreight(Train.Cars[Car]);
								break;
							case "none":
								Train.Cars[Car].Cargo = new EmptyLoad();
								break;
						}
						break;
					case "windscreen":
						if (!Train.IsPlayerTrain)
						{
							break;
						}
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							int numDrops = 0;
							double wipeSpeed = 1.0, holdTime = 1.0, dropLife = 10.0;
							WiperPosition restPosition = WiperPosition.Left, holdPosition = WiperPosition.Left;

							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "numberofdrops":
										if (!NumberFormats.TryParseIntVb6(cc.InnerText, out numDrops))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid number of drops defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "wipespeed":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out wipeSpeed))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid wipe speed defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "holdtime":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out holdTime))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid wiper hold time defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "droplife":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out dropLife))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid drop life defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "restposition":
									case "wiperrestposition":
										switch (cc.InnerText.ToLowerInvariant())
										{
											case "0":
											case "left":
												restPosition = WiperPosition.Left;
												break;
											case "1":
											case "right":
												restPosition = WiperPosition.Right;
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid for Windscreen in Car " + Car + " in XML file " + fileName);
												break;
										}

										break;
									case "holdposition":
									case "wiperholdposition":
										switch (cc.InnerText.ToLowerInvariant())
										{
											case "0":
											case "left":
												holdPosition = WiperPosition.Left;
												break;
											case "1":
											case "right":
												holdPosition = WiperPosition.Right;
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperHoldPosition is invalid for Windscreen in Car " + Car + " in XML file " + fileName);
												break;
										}

										break;
								}
							}

							if (numDrops > 0)
							{
								Train.Cars[Car].Windscreen = new Windscreen(numDrops, dropLife, Train.Cars[Car]);
								Train.Cars[Car].Windscreen.Wipers = new WindscreenWiper(Train.Cars[Car].Windscreen, restPosition, holdPosition, wipeSpeed, holdTime);
							}
						}
						break;
				}
			}
			/*
			 * As there is no set order for XML tags to be presented in, these must be
			 * done after the end of the loop			 *
			 */
			//Assign interior view
			if (interiorFile != String.Empty)
			{
				if (interiorFile.ToLowerInvariant().EndsWith(".xml"))
				{
					XDocument CurrentXML = XDocument.Load(interiorFile, LoadOptions.SetLineInfo);

					// Check for null
					if (CurrentXML.Root == null)
					{
						// We couldn't find any valid XML, so return false
						throw new System.IO.InvalidDataException();
					}
					List<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated").ToList();
					if (DocumentElements != null && DocumentElements.Count != 0)
					{
						string t = Train.TrainFolder;
						Train.TrainFolder = currentPath;
						Plugin.PanelAnimatedXmlParser.ParsePanelAnimatedXml(interiorFile, Train, Car);
						Train.TrainFolder = t;
						if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						{
							Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
						}
						return;
					}
					DocumentElements = CurrentXML.Root.Elements("Panel").ToList();
					if (DocumentElements != null && DocumentElements.Count != 0)
					{
						string t = Train.TrainFolder;
						Train.TrainFolder = currentPath;
						Plugin.PanelXmlParser.ParsePanelXml(interiorFile, Train, Car);
						Train.TrainFolder = t;
						Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
						return;
					}
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".cfg"))
				{
					//Only supports panel2.cfg format
					Plugin.Panel2CfgParser.ParsePanel2Config(System.IO.Path.GetFileName(interiorFile), Path.GetDirectoryName(interiorFile), Train.Cars[Train.DriverCar]);
					Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".animated"))
				{
					Plugin.currentHost.LoadObject(interiorFile, Encoding.UTF8, out var currentObject);
					var a = (AnimatedObjectCollection)currentObject;
					if (a != null)
					{
						try
						{
							for (int i = 0; i < a.Objects.Length; i++)
							{
								Plugin.currentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
							}
							Train.Cars[Car].CarSections[0].Groups[0].Elements = a.Objects;
							if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
							{
								Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							}
						}
						catch
						{
							Plugin.Cancel = true;
						}
					}

				}
				else
				{
					Plugin.currentHost.AddMessage(MessageType.Warning, false, "Interior view file is not supported for Car " + Car + " in XML file " + fileName);
				}
			}

			if (Train.Cars[Car].ReAdhesionDevice == null)
			{
				// if required create default train readhesion device- May have already been setup earlier in the XML
				Train.Cars[Car].ReAdhesionDevice = new BveReAdhesionDevice(Train.Cars[Car], readhesionDevice);
			}

			//Set toppling angle and exposed areas
			Train.Cars[Car].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[Car].Specs.CenterOfGravityHeight / Train.Cars[Car].Width);
			if (!exposedFrontalAreaSet)
			{
				Train.Cars[Car].Specs.ExposedFrontalArea = 0.65 * Train.Cars[Car].Width * Train.Cars[Car].Height;
			}
			if (!unexposedFrontalAreaSet)
			{
				Train.Cars[Car].Specs.UnexposedFrontalArea = 0.2 * Train.Cars[Car].Width * Train.Cars[Car].Height;
			}
		}
	}
}
