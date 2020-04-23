using System.Xml;
using OpenBveApi.Math;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using OpenBve.BrakeSystems;
using OpenBve.Parsers.Panel;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using OpenBveApi.Interface;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static void ParseCarNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects)
		{
			string interiorFile = string.Empty;
			TrainManager.ReadhesionDeviceType readhesionDevice = Train.Cars[0].Specs.ReAdhesionDevice.DeviceType;
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
										Interface.AddMessage(MessageType.Warning, false, "Invalid backwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "forwards":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Z))
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid forwards camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "left":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.X))
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid left camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "right":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.X))
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid right camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "down":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.BottomLeft.Y))
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid down camera restriction defined for Car " + Car + " in XML file " + fileName);
									}
									break;
								case "up":
									if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].CameraRestriction.TopRight.Y))
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid up camera restriction defined for Car " + Car + " in XML file " + fileName);
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
								string childFile = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
								XmlDocument childXML = new XmlDocument();
								childXML.Load(childFile);
								XmlNodeList childNodes = childXML.DocumentElement.SelectNodes("/openBVE/Brake");
								//We need to save and restore the current path to make relative paths within the child file work correctly
								string savedPath = currentPath;
								currentPath = System.IO.Path.GetDirectoryName(childFile);
								ParseBrakeNode(childNodes[0], fileName, Car, ref Train);
								currentPath = savedPath;
							}
							catch
							{
								Interface.AddMessage(MessageType.Error, false, "Failed to load the child Brake XML file specified in " +c.InnerText);
							}
						}
						break;
					case "length":
						double l;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out l) | l <= 0.0)
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid length defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Length = l;
						break;
					case "width":
						double w;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out w) | w <= 0.0)
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid width defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Width = w;
						break;
					case "height":
						double h;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out h) | h <= 0.0)
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid height defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Height = h;
						break;
					case "motorcar":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							Train.Cars[Car].Specs.IsMotorCar = true;
							Train.Cars[Car].Specs.AccelerationCurves = new TrainManager.AccelerationCurve[AccelerationCurves.Length];
							for (int i = 0; i < AccelerationCurves.Length; i++)
							{
								Train.Cars[Car].Specs.AccelerationCurves[i] = AccelerationCurves[i].Clone(AccelerationCurves[i].Multiplier);
							}
						}
						else
						{
							Train.Cars[Car].Specs.AccelerationCurves = new TrainManager.AccelerationCurve[] { };
							Train.Cars[Car].Specs.IsMotorCar = false;
						}
						break;
					case "mass":
						double m;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out m) | m <= 0.0)
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid mass defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].EmptyMass = m;
						Train.Cars[Car].CargoMass = 0;
						break;
					case "frontaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].FrontAxle.Position))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid front axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "rearaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].RearAxle.Position))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid rear axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "object":
						if (string.IsNullOrEmpty(c.InnerText))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid object path for Car " + Car + " in XML file " + fileName);
							break;
						}
						string f = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
						if (System.IO.File.Exists(f))
						{
							Program.CurrentHost.LoadObject(f, System.Text.Encoding.Default, out CarObjects[Car]);
						}
						break;
					case "reversed":
						int n;
						NumberFormats.TryParseIntVb6(c.InnerText, out n);
						if (n == 1 || c.InnerText.ToLowerInvariant() == "true")
						{
							CarObjectsReversed[Car] = true;
						}
						break;
					case "loadingsway":
						int nm;
						NumberFormats.TryParseIntVb6(c.InnerText, out nm);
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
											Interface.AddMessage(MessageType.Warning, false, "Invalid front bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].FrontBogie.RearAxle.Position))
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid front bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid front bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Program.CurrentHost.LoadObject(fb, System.Text.Encoding.Default, out BogieObjects[Car * 2]);
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
											Interface.AddMessage(MessageType.Warning, false, "Invalid rear bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].RearBogie.RearAxle.Position))
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid rear bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(cc.InnerText))
										{
											Interface.AddMessage(MessageType.Warning, false, "Invalid rear bogie object path for Car " + Car + " in XML file " + fileName);
											break;
										}
										string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											Program.CurrentHost.LoadObject(fb, System.Text.Encoding.Default, out BogieObjects[Car * 2 + 1]);
										}
										break;
									case "reversed":
										int nn;
										NumberFormats.TryParseIntVb6(cc.InnerText, out nn);
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
						string[] splitText = c.InnerText.Split(new char[] { ',' });
						if (splitText.Length != 3)
						{
							Interface.AddMessage(MessageType.Warning, false, "Driver position must have three arguments for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Driver = new Vector3();
						double driverZ;
						if (!NumberFormats.TryParseDoubleVb6(splitText[0], out Train.Cars[Car].Driver.X))
						{
							Interface.AddMessage(MessageType.Warning, false, "Driver position X was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[1], out Train.Cars[Car].Driver.Y))
						{
							Interface.AddMessage(MessageType.Warning, false, "Driver position Y was invalid for Car " + Car + " in XML file " + fileName);
						}
						if (!NumberFormats.TryParseDoubleVb6(splitText[2], out driverZ))
						{
							Interface.AddMessage(MessageType.Warning, false, "Driver position X was invalid for Car " + Car + " in XML file " + fileName);
						}
						Train.Cars[Car].Driver.Z = 0.5 * Train.Cars[Car].Length + driverZ;
						break;
					case "interiorview":
						if (!Train.IsPlayerTrain)
						{
							break;
						}
						Train.Cars[Car].HasInteriorView = true;
						Train.Cars[Car].CarSections = new TrainManager.CarSection[1];
						Train.Cars[Car].CarSections[0] = new TrainManager.CarSection
						{
							Groups = new TrainManager.ElementsGroup[1]
						};
						Train.Cars[Car].CarSections[0].Groups[0] = new TrainManager.ElementsGroup
						{
							Elements = new AnimatedObject[] { },
							Overlay = true
						};
						
						string cv = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
						if (!System.IO.File.Exists(cv))
						{
							Interface.AddMessage(MessageType.Warning, false, "Interior view file was invalid for Car " + Car + " in XML file " + fileName);
							break;
						}
						interiorFile = cv;
						break;
					case "readhesiondevice":
						switch (c.InnerText.ToLowerInvariant())
						{
							case "typea":
							case "a":
								readhesionDevice = TrainManager.ReadhesionDeviceType.TypeA;
								break;
							case "typeb":
							case "b":
								readhesionDevice = TrainManager.ReadhesionDeviceType.TypeB;
								break;
							case "typec":
							case "c":
								readhesionDevice = TrainManager.ReadhesionDeviceType.TypeC;
								break;
							case "typed":
							case "d":
								readhesionDevice = TrainManager.ReadhesionDeviceType.TypeD;
								break;
							default:
								readhesionDevice = TrainManager.ReadhesionDeviceType.NotFitted;
								break;
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
					IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated");
					if (DocumentElements != null && DocumentElements.Count() != 0)
					{
						PanelAnimatedXmlParser.ParsePanelAnimatedXml(interiorFile, currentPath, Train, Car);
						if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						{
							Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
						}
						return;
					}
					DocumentElements = CurrentXML.Root.Elements("Panel");
					if (DocumentElements != null  && DocumentElements.Count() != 0)
					{
						PanelXmlParser.ParsePanelXml(interiorFile, currentPath, Train, Car);
						Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
						return;
					}
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".cfg"))
				{
					//Only supports panel2.cfg format
					Panel2CfgParser.ParsePanel2Config(System.IO.Path.GetFileName(interiorFile), System.IO.Path.GetDirectoryName(interiorFile), Train, Car);
					Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.On;
				}
				else if (interiorFile.ToLowerInvariant().EndsWith(".animated"))
				{
					
					UnifiedObject currentObject;
					Program.CurrentHost.LoadObject(interiorFile, Encoding.UTF8, out currentObject);
					var a = currentObject as AnimatedObjectCollection;
					if (a != null)
					{
						try
						{
							for (int i = 0; i < a.Objects.Length; i++)
							{
								Program.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
							}
							Train.Cars[Car].CarSections[0].Groups[0].Elements = a.Objects;
							if (Train.Cars[Car].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
							{
								Train.Cars[Car].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							}
						}
						catch
						{
							Program.RestartArguments = " ";
							Loading.Cancel = true;
						}
					}

				}
				else
				{
					Interface.AddMessage(MessageType.Warning, false, "Interior view file is not supported for Car " + Car + " in XML file " + fileName);
				}
			}
			Train.Cars[Car].Specs.ReAdhesionDevice = new TrainManager.CarReAdhesionDevice(Train.Cars[Car], readhesionDevice);
		}
	}
}
