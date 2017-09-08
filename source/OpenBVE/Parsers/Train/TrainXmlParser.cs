using System;
using System.Xml;
using OpenBveApi.Math;

namespace OpenBve.Parsers.Train
{
	class TrainXmlParser
	{
		internal static string currentPath;
		internal static void Parse(string fileName, TrainManager.Train Train, ref ObjectManager.UnifiedObject[] CarObjects, ref ObjectManager.UnifiedObject[] BogieObjects)
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = System.IO.Path.GetDirectoryName(fileName);
			bool[] CarObjectsReversed = new bool[Train.Cars.Length];
			bool[] BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/Car");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Interface.AddMessage(Interface.MessageType.Error, false, "No car nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty train.xml file");
				}
				//Use the index here for easy access to the car count
				for (int i = 0; i < DocumentNodes.Count; i++)
				{
					if (i > Train.Cars.Length)
					{
						Interface.AddMessage(Interface.MessageType.Warning, false, "WARNING: A total of " + DocumentNodes.Count + " cars were specified in XML file " + fileName + " whilst only " + Train.Cars.Length + " were specified in the train.dat file.");
						break;
					}
					if (DocumentNodes[i].HasChildNodes)
					{
						foreach (XmlNode c in DocumentNodes[i].ChildNodes)
						{
							//Note: Don't use the short-circuiting operator, as otherwise we need another if
							switch (c.Name.ToLowerInvariant())
							{
								case "length":
									double l;
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out l) | l <= 0.0)
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid length defined for Car " + i + " in XML file " + fileName);
										break;
									}
									Train.Cars[i].Length = l;
									break;
								case "width":
									double w;
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out w) | w <= 0.0)
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid width defined for Car " + i + " in XML file " + fileName);
										break;
									}
									Train.Cars[i].Width = w;
									break;
								case "height":
									double h;
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out h) | h <= 0.0)
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid height defined for Car " + i + " in XML file " + fileName);
										break;
									}
									Train.Cars[i].Height = h;
									break;
								case "motorcar":
									if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
									{
										Train.Cars[i].Specs.IsMotorCar = true;
									}
									else
									{
										Train.Cars[i].Specs.IsMotorCar = false;
									}
									break;
								case "mass":
									double m;
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out m) | m <= 0.0)
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid mass defined for Car " + i + " in XML file " + fileName);
										break;
									}
									Train.Cars[i].Specs.MassEmpty = m;
									Train.Cars[i].Specs.MassCurrent = m;
									break;
								case "frontaxle":
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[i].FrontAxle.Position))
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front axle position defined for Car " + i + " in XML file " + fileName);
									}
									break;
								case "rearaxle":
									if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[i].RearAxle.Position))
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear axle position defined for Car " + i + " in XML file " + fileName);
									}
									break;
								case "object":
									string f = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
									if (System.IO.File.Exists(f))
									{
										CarObjects[i] = ObjectManager.LoadObject(f, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
									}
									break;
								case "reversed":
									if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
									{
										CarObjectsReversed[i] = true;
									}
									break;
								case "frontbogie":
									if (c.HasChildNodes)
									{
										foreach (XmlNode cc in c.ChildNodes)
										{
											switch (cc.Name.ToLowerInvariant())
											{
												case "frontaxle":
													if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[i].FrontBogie.FrontAxle.Position))
													{
														Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front bogie, front axle position defined for Car " + i + " in XML file " + fileName);
													}
													break;
												case "rearaxle":
													if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[i].FrontBogie.RearAxle.Position))
													{
														Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front bogie, rear axle position defined for Car " + i + " in XML file " + fileName);
													}
													break;
												case "object":
													string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
													if (System.IO.File.Exists(fb))
													{
														BogieObjects[i * 2] = ObjectManager.LoadObject(fb, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
													}
													break;
												case "reversed":
													BogieObjectsReversed[i * 2] = true;
													break;
											}
										}
									}
									break;
								case "rearbogie":
									if (c.HasChildNodes)
									{
										foreach (XmlNode cc in c.ChildNodes)
										{
											switch (cc.Name.ToLowerInvariant())
											{
												case "frontaxle":
													if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[i].RearBogie.FrontAxle.Position))
													{
														Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear bogie, front axle position defined for Car " + i + " in XML file " + fileName);
													}
													break;
												case "rearaxle":
													if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[i].RearBogie.RearAxle.Position))
													{
														Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear bogie, rear axle position defined for Car " + i + " in XML file " + fileName);
													}
													break;
												case "object":
													string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
													if (System.IO.File.Exists(fb))
													{
														BogieObjects[i * 2 + 1] = ObjectManager.LoadObject(fb, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
													}
													break;
												case "reversed":
													BogieObjectsReversed[i * 2 + 1] = true;
													break;
											}
										}
									}
									break;
							}
						}
					}
					if (i == DocumentNodes.Count && i < Train.Cars.Length)
					{
						//If this is the case, the number of motor cars is the primary thing which may get confused....
						//Not a lot to be done about this until a full replacement is built for the train.dat file & we can dump it entirely
						Interface.AddMessage(Interface.MessageType.Warning, false, "WARNING: The number of cars specified in the train.xml file does not match that in the train.dat- Some properties may be invalid.");
					}
				}

				for (int i = 0; i < Train.Cars.Length; i++)
				{
					if (CarObjects[i] != null)
					{
						if (CarObjectsReversed[i])
						{
							{
								// reverse axle positions
								double temp = Train.Cars[i].FrontAxle.Position;
								Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
								Train.Cars[i].RearAxle.Position = -temp;
							}
							if (CarObjects[i] is ObjectManager.StaticObject)
							{
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)CarObjects[i];
								CsvB3dObjectParser.ApplyScale(obj, -1.0, 1.0, -1.0);
							}
							else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection)
							{
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++)
								{
									for (int h = 0; h < obj.Objects[j].States.Length; h++)
									{
										CsvB3dObjectParser.ApplyScale(obj.Objects[j].States[h].Object, -1.0, 1.0, -1.0);
										obj.Objects[j].States[h].Position.X *= -1.0;
										obj.Objects[j].States[h].Position.Z *= -1.0;
									}
									obj.Objects[j].TranslateXDirection.X *= -1.0;
									obj.Objects[j].TranslateXDirection.Z *= -1.0;
									obj.Objects[j].TranslateYDirection.X *= -1.0;
									obj.Objects[j].TranslateYDirection.Z *= -1.0;
									obj.Objects[j].TranslateZDirection.X *= -1.0;
									obj.Objects[j].TranslateZDirection.Z *= -1.0;
								}
							}
							else
							{
								throw new NotImplementedException();
							}
						}
					}
				}

				//Check for bogie objects and reverse if necessary.....
				int bogieObjects = 0;
				for (int i = 0; i < Train.Cars.Length * 2; i++)
				{
					bool IsOdd = (i % 2 != 0);
					int CarIndex = i / 2;
					if (BogieObjects[i] != null)
					{
						bogieObjects++;
						if (BogieObjectsReversed[i])
						{
							{
								// reverse axle positions
								if (IsOdd)
								{
									double temp = Train.Cars[CarIndex].FrontBogie.FrontAxle.Position;
									Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = -Train.Cars[CarIndex].FrontBogie.RearAxle.Position;
									Train.Cars[CarIndex].FrontBogie.RearAxle.Position = -temp;
								}
								else
								{
									double temp = Train.Cars[CarIndex].RearBogie.FrontAxle.Position;
									Train.Cars[CarIndex].RearBogie.FrontAxle.Position = -Train.Cars[CarIndex].RearBogie.RearAxle.Position;
									Train.Cars[CarIndex].RearBogie.RearAxle.Position = -temp;
								}
							}
							if (BogieObjects[i] is ObjectManager.StaticObject)
							{
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)BogieObjects[i];
								CsvB3dObjectParser.ApplyScale(obj, -1.0, 1.0, -1.0);
							}
							else if (BogieObjects[i] is ObjectManager.AnimatedObjectCollection)
							{
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)BogieObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++)
								{
									for (int h = 0; h < obj.Objects[j].States.Length; h++)
									{
										CsvB3dObjectParser.ApplyScale(obj.Objects[j].States[h].Object, -1.0, 1.0, -1.0);
										obj.Objects[j].States[h].Position.X *= -1.0;
										obj.Objects[j].States[h].Position.Z *= -1.0;
									}
									obj.Objects[j].TranslateXDirection.X *= -1.0;
									obj.Objects[j].TranslateXDirection.Z *= -1.0;
									obj.Objects[j].TranslateYDirection.X *= -1.0;
									obj.Objects[j].TranslateYDirection.Z *= -1.0;
									obj.Objects[j].TranslateZDirection.X *= -1.0;
									obj.Objects[j].TranslateZDirection.Z *= -1.0;
								}
							}
							else
							{
								throw new NotImplementedException();
							}
						}
					}
				}
			}

		}
	}
}
