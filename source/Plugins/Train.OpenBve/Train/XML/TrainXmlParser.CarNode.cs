using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LibRender2.Trains;
using LibRender2.Smoke;
using OpenBveApi;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.World;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseCarNode(XmlNode Node, string fileName, int Car, ref TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref bool visibleFromInterior)
		{
			string interiorFile = string.Empty;
			Vector3 interiorDirection = Vector3.Zero;
			ReadhesionDeviceType readhesionDevice = ReadhesionDeviceType.NotFitted;
			if (Train.Cars[0].ReAdhesionDevice is BveReAdhesionDevice device)
			{
				readhesionDevice = device.DeviceType;
			}

			bool copyAccelerationCurves = true;
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
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid backwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "forwards":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Z))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid forwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "left":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.X))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid left camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "right":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.X))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid right camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "down":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.Y))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid down camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "up":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Y))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid up camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
							}
						}
						break;
					case "brake":
						Train.Cars[Car].CarBrake.BrakeType = BrakeType.Auxiliary;
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Failed to load the child Brake XML file specified in " +c.InnerText);
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The error encountered was " + ex);
							}
						}
						break;
					case "length":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double l) | l <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid length defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Length = l;
						break;
					case "width":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double w) | w <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid width defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Width = w;
						break;
					case "height":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double h) | h <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid height defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Height = h;
						break;
					case "motorcar":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							if (!copyAccelerationCurves)
							{
								//We've already set the acceleration curves elsewhere in the XML, so don't copy the default ones
								break;
							}
							AccelerationCurve[] finalAccelerationCurves = new AccelerationCurve[Plugin.AccelerationCurves.Length];
							for (int i = 0; i < Plugin.AccelerationCurves.Length; i++)
							{
								finalAccelerationCurves[i] = Plugin.AccelerationCurves[i].Clone(1.0);
							}

							Train.Cars[Car].TractionModel = new BVEMotorCar(Train.Cars[Car], finalAccelerationCurves);
							Train.Cars[Car].TractionModel.MaximumPossibleAcceleration = Plugin.MaximumAcceleration;
						}
						else
						{
							Train.Cars[Car].TractionModel = new BVETrailerCar(Train.Cars[Car]);
						}
						break;
					case "mass":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double m) | m <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid mass defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].EmptyMass = m;
						Train.Cars[Car].CargoMass = 0;
						break;
					case "centerofgravityheight":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double cg) | cg <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid CenterOfGravityHeight defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.CenterOfGravityHeight = cg;
						break;
					case "exposedfrontalarea":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double ef) | ef <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid ExposedFrontalArea defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.ExposedFrontalArea = ef;
						exposedFrontalAreaSet = true;
						break;
					case "unexposedfrontalarea":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out double uf) | uf <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid UnexposedFrontalArea defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.UnexposedFrontalArea = uf;
						unexposedFrontalAreaSet = true;
						break;
					case "frontaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].FrontAxle.Position))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid front axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "rearaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].RearAxle.Position))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid rear axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "object":
						if (string.IsNullOrEmpty(c.InnerText))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid object path for Car " + Car + " in XML file " + fileName);
							break;
						}
						string f = Path.CombineFile(currentPath, c.InnerText);
						if (System.IO.File.Exists(f))
						{
							Plugin.CurrentHost.LoadObject(f, Encoding.Default, out CarObjects[Car]);
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
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].FrontBogie.RearAxle.Position))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid front bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Plugin.CurrentHost.LoadObject(fb, Encoding.Default, out BogieObjects[Car * 2]);
										}
										break;
									case "reversed":
										NumberFormats.TryParseIntVb6(cc.InnerText, out int nn);
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
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].RearBogie.RearAxle.Position))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid rear bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Plugin.CurrentHost.LoadObject(fb, Encoding.Default, out BogieObjects[Car * 2 + 1]);
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
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Driver position must have three arguments for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Driver = new Vector3();
						if (!NumberFormats.TryParseDoubleVb6(splitText[0], out Train.Cars[Car].Driver.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Driver position X was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[1], out Train.Cars[Car].Driver.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Driver position Y was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[2], out double driverZ))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Driver position Z was invalid for Car " + Car + " in XML file " + fileName);
						}
						Train.Cars[Car].Driver.Z = 0.5 * Train.Cars[Car].Length + driverZ;
						break;
					case "interiorview":
						if (!Train.IsPlayerTrain)
						{
							break;
						}
						Train.Cars[Car].HasInteriorView = true;
						Train.Cars[Car].CarSections = new Dictionary<CarSectionType, CarSection>();
						Train.Cars[Car].CarSections.Add(CarSectionType.Interior,new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true));

						string cv = Path.CombineFile(currentPath, c.InnerText);
						if (!System.IO.File.Exists(cv))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior view file was invalid for Car " + Car + " in XML file " + fileName);
							break;
						}
						interiorFile = cv;
						break;
					case "interiordirection":
						splitText = c.InnerText.Split(',');
						if (splitText.Length != 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior direction must have three arguments for Car " + Car + " in XML file " + fileName);
							break;
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[0], out interiorDirection.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior direction X was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[1], out interiorDirection.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior direction Y was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[2], out interiorDirection.Z))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior direction Z was invalid for Car " + Car + " in XML file " + fileName);
						}
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
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sanders type was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "rate":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out rate))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sanders application rate was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "sandlevel":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out level))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sand level was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "numberofshots":
									if (!NumberFormats.TryParseIntVb6(cc.InnerText, out shots))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sand level was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "applicationtime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out applicationTime))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sanders application time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "activationtime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out activationTime))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sanders activation time was invalid for Car " + Car + " in XML file " + fileName);
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
					case "driversupervisiondevice":
					case "dsd":
						SafetySystemType driverSupervisionType = SafetySystemType.None;
						DriverSupervisionDeviceMode driverSupervisionMode = DriverSupervisionDeviceMode.Power;
						SafetySystemTriggerMode triggerMode = SafetySystemTriggerMode.Always;
						double alarmTime = 0;
						double interventionTime = 0;
						double requiredStopTime = 0;
						bool loopingAlarm = false, loopingAlert = false;
						foreach (XmlNode cc in c.ChildNodes)
						{
							switch (cc.Name.ToLowerInvariant())
							{
								case "alarmtime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out alarmTime))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice activation time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "interventiontime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out interventionTime))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice activation time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "type":
									if (!Enum.TryParse(cc.InnerText, true, out driverSupervisionType))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice type was invalid for Car " + Car + " in XML file " + fileName);
									} 
									break; 
								case "requiredstoptime":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out requiredStopTime))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice required stop time was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "loopingalert":
									if (cc.InnerText.ToLowerInvariant() == "1" || cc.InnerText.ToLowerInvariant() == "true")
									{
										loopingAlert = true;
									}
									break;
								case "loopingalarm":
									if (cc.InnerText.ToLowerInvariant() == "1" || cc.InnerText.ToLowerInvariant() == "true")
									{
										loopingAlarm = true;
									}
									break;
								case "mode":
									if (!Enum.TryParse(cc.InnerText, true, out driverSupervisionMode))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice mode was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "triggermode":
									if (!Enum.TryParse(cc.InnerText, true, out triggerMode))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "DriverSupervisionDevice trigger mode was invalid for Car " + Car + " in XML file " + fileName);
									}
									break;
							}
						}

						if (alarmTime == 0)
						{
							alarmTime = interventionTime;
						}

						DriverSupervisionDevice dsd = new DriverSupervisionDevice(Train.Cars[Car], driverSupervisionType, driverSupervisionMode, triggerMode, alarmTime, interventionTime, requiredStopTime)
						{
							LoopingAlarm = loopingAlarm,
							LoopingAlert = loopingAlert
						};
						Train.Cars[Car].SafetySystems.Add(SafetySystem.DriverSupervisionDevice, dsd);
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
										Train.Cars[Car].TractionModel.MotorSounds = Bve5MotorSoundTableParser.Parse(Train.Cars[Car], powerFreq, powerVol, brakeFreq, brakeVol);
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
						copyAccelerationCurves = false;
						AccelerationCurve[] curves = ParseAccelerationNode(c, fileName);
						if (Train.Cars[Car].TractionModel is BVEMotorCar || Train.Cars[Car].TractionModel is BVETrailerCar)
						{
							AbstractMotorSound motor = Train.Cars[Car].TractionModel.MotorSounds;
							Train.Cars[Car].TractionModel = new BVEMotorCar(Train.Cars[Car], curves);
							Train.Cars[Car].TractionModel.MotorSounds = motor;
						}
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
										copyAccelerationCurves = false;
										curves = ParseAccelerationNode(cc, fileName);
										if (Train.Cars[Car].TractionModel is BVEMotorCar || Train.Cars[Car].TractionModel is BVETrailerCar)
										{
											AbstractMotorSound motor = Train.Cars[Car].TractionModel.MotorSounds;
											Train.Cars[Car].TractionModel = new BVEMotorCar(Train.Cars[Car], curves);
											Train.Cars[Car].TractionModel.MotorSounds = motor;
										}
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
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid door opening speed defined for Car " + Car + " in XML file " + fileName);
										}
										else
										{
											Train.Cars[Car].Specs.DoorOpenFrequency = 1.0 / os;
										}
										break;
									case "closespeed":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out double cs))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid door opening speed defined for Car " + Car + " in XML file " + fileName);
										}
										else
										{
											Train.Cars[Car].Specs.DoorCloseFrequency = 1.0 / cs;
										}
										break;
									case "width":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out doorWidth))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid door width defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "tolerance":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out doorWidth))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid door closing tolerance defined for Car " + Car + " in XML file " + fileName);
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
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid number of drops defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "wipespeed":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out wipeSpeed))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid wipe speed defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "holdtime":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out holdTime))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid wiper hold time defined for Windscreen in Car " + Car + " in XML file " + fileName);
										}

										break;
									case "droplife":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out dropLife))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid drop life defined for Windscreen in Car " + Car + " in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid for Windscreen in Car " + Car + " in XML file " + fileName);
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
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WiperHoldPosition is invalid for Windscreen in Car " + Car + " in XML file " + fileName);
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
					case "dieselengine":
						foreach (XmlNode cc in c.ChildNodes)
						{
							double idleRPM = 0, minRPM = 0, maxRPM = 0, rpmChangeUpRate = 0, rpmChangeDownRate = 0, idleFuelUse = 0, maxPowerFuelUse = 0, fuelCapacity = 0;
							double maxAmps = 0, maxRegenAmps = 0;
							bool isRegenerative = false;
							switch (cc.Name.ToLowerInvariant())
							{
								case "idlerpm":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out idleRPM))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid idle RPM defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "minrpm":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out minRPM))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid minimum RPM defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "maxrpm":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxRPM))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid maximum RPM defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "rpmchangerate":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out rpmChangeUpRate))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid RPM change rate defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									rpmChangeDownRate = rpmChangeUpRate;
									break;
								case "rpmchangeuprate":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out rpmChangeUpRate))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid RPM change Up rate defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "rpmchangedownrate":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out rpmChangeDownRate))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid RPM change Down rate defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "idlefueluse":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out idleFuelUse))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid idle fuel use rate defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "maxpowerfueluse":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxPowerFuelUse))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid max power fuel use rate defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "fuelcapacity":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out fuelCapacity))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid fuel capacity defined for DieselEngine in Car " + Car + " in XML file " + fileName);
									}
									break;
								case "tractionmotor":
								case "regenerativetractionmotor":
									foreach (XmlNode ccc in cc.ChildNodes)
									{
										switch (ccc.Name.ToLowerInvariant())
										{
											case "maxamps":
												if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxAmps))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid fuel capacity defined for DieselEngine in Car " + Car + " in XML file " + fileName);
												}
												break;
											case "maxregenerativeamps":
												if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxAmps))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid fuel capacity defined for DieselEngine in Car " + Car + " in XML file " + fileName);
												}
												break;
										}
									}

									if (cc.Name.ToLowerInvariant() == "regenerativetractionmotor")
									{
										isRegenerative = true;
									}
									break;
							}

							Train.Cars[Car].TractionModel = new DieselEngine(Train.Cars[Car], Plugin.AccelerationCurves, idleRPM, minRPM, maxRPM, rpmChangeUpRate, rpmChangeDownRate, idleFuelUse, maxPowerFuelUse);
							Train.Cars[Car].TractionModel.FuelTank = new FuelTank(fuelCapacity, 0, fuelCapacity);
							if (isRegenerative)
							{
								Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(Train.Cars[Car].TractionModel, maxAmps));
							}
							else
							{
								Train.Cars[Car].TractionModel.Components.Add(EngineComponent.RegenerativeTractionMotor, new RegenerativeTractionMotor(Train.Cars[Car].TractionModel, maxAmps, maxRegenAmps));
							}
						}
						break;
					case "electricengine":
						foreach (XmlNode cc in c.ChildNodes)
						{
							bool hasPantograph = false;
							double maxAmps = 0, maxRegenAmps = 0;
							bool isRegenerative = false;
							switch (cc.Name.ToLowerInvariant())
							{
								case "collectorshoe":
								case "pantograph":
									if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
									{
										hasPantograph = true;
									}
									break;
								case "tractionmotor":
								case "regenerativetractionmotor":
									foreach (XmlNode ccc in cc.ChildNodes)
									{
										switch (ccc.Name.ToLowerInvariant())
										{
											case "maxamps":
												if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxAmps))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid fuel capacity defined for DieselEngine in Car " + Car + " in XML file " + fileName);
												}
												break;
											case "maxregenerativeamps":
												if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maxAmps))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid fuel capacity defined for DieselEngine in Car " + Car + " in XML file " + fileName);
												}
												break;
										}
									}

									if (cc.Name.ToLowerInvariant() == "regenerativetractionmotor")
									{
										isRegenerative = true;
									}
									break;
							}
							Train.Cars[Car].TractionModel = new ElectricEngine(Train.Cars[Car], Plugin.AccelerationCurves);
							if (hasPantograph)
							{
								Train.Cars[Car].TractionModel.Components.Add(EngineComponent.Pantograph, new Pantograph(Train.Cars[Car].TractionModel));
							}

							if (isRegenerative)
							{
								Train.Cars[Car].TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(Train.Cars[Car].TractionModel, maxAmps));
							}
							else
							{
								Train.Cars[Car].TractionModel.Components.Add(EngineComponent.RegenerativeTractionMotor, new RegenerativeTractionMotor(Train.Cars[Car].TractionModel, maxAmps, maxRegenAmps));
							}
						}
						break;
					case "particlesource":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							Vector3 emitterLocation = Vector3.Zero;
							Vector3 initialMotion = Vector3.Down;
							string expression = "enginepower[" + Car + "]";
							double maximumSize = 0.2;
							double maximumGrownSize = 1.0;
							double maximumLifeSpan = 15;
							Texture particleTexture = null;
							bool emitsAtIdle = true;
							foreach (XmlNode cc in c.ChildNodes)
							{
								switch (cc.Name.ToLowerInvariant())
								{
									case "location":
										splitText = cc.InnerText.Split(',');
										if (!NumberFormats.TryParseDoubleVb6(splitText[0], out emitterLocation.X))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location X was invalid for Car " + Car + " in XML file " + fileName);
										}
										if (!NumberFormats.TryParseDoubleVb6(splitText[1], out emitterLocation.Y))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location Y was invalid for Car " + Car + " in XML file " + fileName);
										}
										if (!NumberFormats.TryParseDoubleVb6(splitText[2], out emitterLocation.Z))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location Z was invalid for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "maximumsize":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maximumSize))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid initial maximum size defined for particle emitter in Car " + Car + " in XML file " + fileName);
										}
										break;
									case "maximumgrownsize":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maximumGrownSize))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid maximum grown size defined for particle emitter in Car " + Car + " in XML file " + fileName);
										}
										break;
									case "initialdirection":
										splitText = cc.InnerText.Split(',');
										if (!NumberFormats.TryParseDoubleVb6(splitText[0], out initialMotion.X))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location X was invalid for Car " + Car + " in XML file " + fileName);
										}
										if (!NumberFormats.TryParseDoubleVb6(splitText[1], out initialMotion.Y))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location Y was invalid for Car " + Car + " in XML file " + fileName);
										}
										if (!NumberFormats.TryParseDoubleVb6(splitText[2], out initialMotion.Z))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Particle emitter location Z was invalid for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "texture":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid particle emitter texture for Car " + Car + " in XML file " + fileName);
											break;
										}
										string st = Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(st))
										{
											Plugin.CurrentHost.RegisterTexture(st, TextureParameters.NoChange, out particleTexture);
										}
										break;
									case "maximumlifespan":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out maximumLifeSpan))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid initial maximum lifespan defined for particle emitter in Car " + Car + " in XML file " + fileName);
										}
										break;
									case "function":
										if (!string.IsNullOrEmpty(cc.InnerText))
										{
											expression = cc.InnerText;
										}
										break;
									case "emitsatidle":
										if (c.InnerText.ToLowerInvariant() == "0" || c.InnerText.ToLowerInvariant() == "false")
										{
											emitsAtIdle = false;
										}
										break;
								}
							}
							ParticleSource particleSource = new ParticleSource(Plugin.Renderer, Train.Cars[Car], emitterLocation, maximumSize, maximumGrownSize, initialMotion, maximumLifeSpan);
							particleSource.EmitsAtIdle = emitsAtIdle;
							particleSource.Controller = new FunctionScript(Plugin.CurrentHost, expression, true);
							particleSource.ParticleTexture = particleTexture;
							Train.Cars[Car].ParticleSources.Add(particleSource);
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
				Transformation viewTransformation = new Transformation(interiorDirection.X.ToRadians(), interiorDirection.Y.ToRadians(), interiorDirection.Z.ToRadians());
				Train.Cars[Car].CarSections[CarSectionType.Interior].ViewDirection = viewTransformation;
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
					Plugin.CurrentHost.LoadObject(interiorFile, Encoding.UTF8, out var currentObject);
					var a = (AnimatedObjectCollection)currentObject;
					if (a != null)
					{
						try
						{
							for (int i = 0; i < a.Objects.Length; i++)
							{
								Plugin.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
							}
							Train.Cars[Car].CarSections[CarSectionType.Interior].Groups[0].Elements = a.Objects;
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
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Interior view file is not supported for Car " + Car + " in XML file " + fileName);
				}
			}

			if (!Train.Cars[Car].TractionModel.ProvidesPower && Train.Cars[Car].ParticleSources.Count > 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Car " + Car + " has a particle source assigned, but is not a motor car in XML file " + fileName);
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
