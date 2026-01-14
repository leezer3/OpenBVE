using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Formats.OpenBve;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private readonly Plugin Plugin;

		internal TrainXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		private static string currentPath;
		private static bool[] CarObjectsReversed;
		private static bool[] BogieObjectsReversed;
		
		private static readonly char[] separatorChars = { ';', ',' };
		internal static bool[] MotorSoundXMLParsed;
		internal void Parse(string fileName, TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref UnifiedObject[] CouplerObjects, out bool[] interiorVisible)
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = Path.GetDirectoryName(fileName);
			MotorSoundXMLParsed = new bool[Train.Cars.Length];
			CarObjectsReversed = new bool[Train.Cars.Length];
			BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			interiorVisible = new bool[Train.Cars.Length];
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/DriverCar");
				if (DocumentNodes != null && DocumentNodes.Count > 0)
				{
					// Optional stuff, needs to be loaded before the car list
					for (int i = 0; i < DocumentNodes.Count; i++)
					{
						Enum.TryParse(DocumentNodes[i].Name, true, out TrainXMLKey key);
						switch (key)
						{
							case TrainXMLKey.DriverCar:
								if (!NumberFormats.TryParseIntVb6(DocumentNodes[i].InnerText, out var driverCar) || driverCar < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DriverCar is invalid in XML file " + fileName);
									break;
								}
								Train.DriverCar = driverCar;
								break;
						}
					}
				}

				DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/*[self::Car or self::Coupler]");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "No car nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty train.xml file");
				}

				int carIndex = 0;

				double perCarProgress = 0.25 / DocumentNodes.Count;
				//Use the index here for easy access to the car count
				for (int i = 0; i < DocumentNodes.Count; i++)
				{
					Plugin.CurrentProgress = Plugin.LastProgress + perCarProgress * i;
					if (carIndex > Train.Cars.Length - 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "WARNING: A total of " + DocumentNodes.Count + " cars were specified in XML file " + fileName + " whilst only " + Train.Cars.Length + " were specified in the train.dat file.");
						break;
					}
					if (DocumentNodes[i].ChildNodes.OfType<XmlElement>().Any())
					{
						if (DocumentNodes[i].Name == "Car")
						{
							ParseCarNode(DocumentNodes[i], fileName, carIndex, ref Train, ref CarObjects, ref BogieObjects, ref interiorVisible[carIndex]);
						}
						else
						{
							if (carIndex - 1 > Train.Cars.Length - 2)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected extra coupler encountered in XML file " + fileName);
								continue;
							}
							foreach (XmlNode c in DocumentNodes[i].ChildNodes)
							{
								Enum.TryParse(c.Name, true, out TrainXMLKey key);
								switch (key)
								{
									case TrainXMLKey.Minimum:
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MinimumDistanceBetweenCars is invalid for coupler " + carIndex + "in XML file " + fileName);
										}
										break;
									case TrainXMLKey.Maximum:
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[carIndex - 1].Coupler.MaximumDistanceBetweenCars))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MaximumDistanceBetweenCars is invalid for coupler " + carIndex + "in XML file " + fileName);
										}
										break;
									case TrainXMLKey.Object:
										if (string.IsNullOrEmpty(c.InnerText))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid object path for Coupler " + (carIndex - 1) + " in XML file " + fileName);
											break;
										}
										string f = Path.CombineFile(currentPath, c.InnerText);
										if (File.Exists(f))
										{
											Plugin.CurrentHost.LoadObject(f, Encoding.Default, out CouplerObjects[carIndex - 1]);
										}
										break;
									case TrainXMLKey.CanUncouple:
										NumberFormats.TryParseIntVb6(c.InnerText, out int nn);
										if (c.InnerText.ToLowerInvariant() == "false" || nn == 0)
										{
											Train.Cars[carIndex - 1].Coupler.CanUncouple = false;
										}
										break;
									case TrainXMLKey.UncouplingBehaviour:
										if (!Enum.TryParse(c.InnerText, true, out Train.Cars[carIndex -1].Coupler.UncouplingBehaviour))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid uncoupling behaviour " + c.InnerText + " in " + c.Name + " node.");
										}
										break;
									case TrainXMLKey.DriverBody:
										double shoulderHeight = 0.6;
										double headHeight = 0.1;
										foreach (XmlNode cc in c.ChildNodes)
										{
											switch (cc.Name.ToLowerInvariant())
											{
												case "shoulderheight":
													if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out shoulderHeight))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ShoulderHeight is invalid for DriverBody in XML file " + fileName);
													}
													break;
												case "headheight":
													if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out headHeight))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "HeadHeight is invalid for DriverBody in XML file " + fileName);
													}
													break;
											}
										}
										Train.DriverBody = new DriverBody(Train, shoulderHeight, headHeight);
										break;
								}
							}
						}

					}
					else if (!String.IsNullOrEmpty(DocumentNodes[i].InnerText))
					{
						try
						{
							string childFile = Path.CombineFile(currentPath, DocumentNodes[i].InnerText);
							XmlDocument childXML = new XmlDocument();
							childXML.Load(childFile);
							XmlNodeList childNodes = childXML.DocumentElement.SelectNodes("/openBVE/Car");
							//We need to save and restore the current path to make relative paths within the child file work correctly
							string savedPath = currentPath;
							currentPath = Path.GetDirectoryName(childFile);
							ParseCarNode(childNodes[0], fileName, carIndex, ref Train, ref CarObjects, ref BogieObjects, ref interiorVisible[carIndex]);
							currentPath = savedPath;
						}
						catch(Exception ex)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Failed to load the child Car XML file specified in " + DocumentNodes[i].InnerText);
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The error encountered was " + ex);
						}
					}
					if (i == DocumentNodes.Count && carIndex < Train.Cars.Length)
					{
						//If this is the case, the number of motor cars is the primary thing which may get confused....
						//Not a lot to be done about this until a full replacement is built for the train.dat file & we can dump it entirely
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "WARNING: The number of cars specified in the train.xml file does not match that in the train.dat- Some properties may be invalid.");
					}
					if (DocumentNodes[i].Name == "Car")
					{
						carIndex++;
					}
				}

				if (Train.DriverCar >= Train.Cars.Length)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid driver car defined in XML file " + fileName);
					Train.DriverCar = Train.Cars.Length - 1;
				}

				if (Train.Cars[Train.DriverCar].CameraRestrictionMode != CameraRestrictionMode.NotSpecified)
				{
					Plugin.Renderer.Camera.CurrentRestriction = Train.Cars[Train.DriverCar].CameraRestrictionMode;
				}
				DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/NotchDescriptions");
				
				if (DocumentNodes != null && DocumentNodes.Count > 0)
				{
					//Optional section
					for (int i = 0; i < DocumentNodes.Count; i++)
					{
						if (DocumentNodes[i].ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode c in DocumentNodes[i].ChildNodes)
							{
								Enum.TryParse(c.Name, true, out TrainXMLKey key);
								switch (key)
								{
									case TrainXMLKey.Power:
										Train.Handles.Power.NotchDescriptions = c.InnerText.Split(separatorChars);
										for (int j = 0; j < Train.Handles.Power.NotchDescriptions.Length; j++)
										{
											Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Power.NotchDescriptions[j]);
											if (s.X > Train.Handles.Power.MaxWidth)
											{
												Train.Handles.Power.MaxWidth = s.X;
											}
										}
										break;
									case TrainXMLKey.Brake:
										Train.Handles.Brake.NotchDescriptions = c.InnerText.Split(separatorChars);
										for (int j = 0; j < Train.Handles.Brake.NotchDescriptions.Length; j++)
										{
											Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Brake.NotchDescriptions[j]);
											if (s.X > Train.Handles.Brake.MaxWidth)
											{
												Train.Handles.Brake.MaxWidth = s.X;
											}
										}
										break;
									case TrainXMLKey.LocoBrake:
										if (Train.Handles.LocoBrake == null)
										{
											continue;
										}
										Train.Handles.LocoBrake.NotchDescriptions = c.InnerText.Split(separatorChars);
										for (int j = 0; j < Train.Handles.LocoBrake.NotchDescriptions.Length; j++)
										{
											Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.LocoBrake.NotchDescriptions[j]);
											if (s.X > Train.Handles.LocoBrake.MaxWidth)
											{
												Train.Handles.LocoBrake.MaxWidth = s.X;
											}
										}
										break;
									case TrainXMLKey.Reverser:
										Train.Handles.Reverser.NotchDescriptions = c.InnerText.Split(separatorChars);
										for (int j = 0; j < Train.Handles.Reverser.NotchDescriptions.Length; j++)
										{
											Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Reverser.NotchDescriptions[j]);
											if (s.X > Train.Handles.Reverser.MaxWidth)
											{
												Train.Handles.Reverser.MaxWidth = s.X;
											}
										}
										break;
								}
							}
						}

					}
				}
				DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/*[self::Plugin or self::HeadlightStates]");
				if (DocumentNodes != null && DocumentNodes.Count > 0)
				{
					// More optional, but needs to be loaded after the car list
					for (int i = 0; i < DocumentNodes.Count; i++)
					{
						Enum.TryParse(DocumentNodes[i].Name, true, out TrainXMLKey key);
						switch (key)
						{
							case TrainXMLKey.Plugin:
								if (DocumentNodes[i].HasChildNodes)
								{
									bool loadForAI = false;
									string pluginFile = string.Empty;
									for (int j = 0; j < DocumentNodes[i].ChildNodes.Count; j++)
									{
										Enum.TryParse(DocumentNodes[i].ChildNodes[j].Name, true, out TrainXMLKey pluginKey);
										switch (pluginKey)
										{
											case TrainXMLKey.File:
												pluginFile = DocumentNodes[i].ChildNodes[j].InnerText;
												break;
											case TrainXMLKey.LoadForAI:
												if (DocumentNodes[i].ChildNodes[j].InnerText.ToLowerInvariant() == "true" || DocumentNodes[i].InnerText == "1")
												{
													loadForAI = true;
												}
												break;
										}
									}
									pluginFile = Path.CombineFile(currentPath, pluginFile);
									if (File.Exists(pluginFile) && (loadForAI || Train.IsPlayerTrain))
									{
										if (!Train.LoadPlugin(pluginFile, currentPath))
										{
											Train.Plugin = null;
										}
									}

								}
								else
								{
									if (!Train.IsPlayerTrain)
									{
										break;
									}
									currentPath = Path.GetDirectoryName(fileName); // reset to base path
									string pluginFile = DocumentNodes[i].InnerText;
									pluginFile = Path.CombineFile(currentPath, pluginFile);
									if (File.Exists(pluginFile))
									{
										if (!Train.LoadPlugin(pluginFile, currentPath))
										{
											Train.Plugin = null;
										}
									}
								}
									
								break;
							case TrainXMLKey.HeadlightStates:
								if (!NumberFormats.TryParseIntVb6(DocumentNodes[i].InnerText, out int numStates))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumStates is invalid for HeadlightStates in XML file " + fileName);
									break;
								}

								Train.SafetySystems.Headlights = new LightSource(Train, numStates);
								break;
						}
					}
				}
				/*
				 * Add final properties and stuff
				 */
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					if (CarObjects[i] != null)
					{
						if (CarObjectsReversed[i])
						{
							// reverse axle positions
							double temp = Train.Cars[i].FrontAxle.Position;
							Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
							Train.Cars[i].RearAxle.Position = -temp;
							if (CarObjects[i] is StaticObject)
							{
								StaticObject obj = (StaticObject)CarObjects[i].Clone();
								obj.ApplyScale(-1.0, 1.0, -1.0);
								CarObjects[i] = obj;
							}
							else if (CarObjects[i] is AnimatedObjectCollection)
							{
								AnimatedObjectCollection obj = (AnimatedObjectCollection)CarObjects[i].Clone();
								obj.Reverse();
								CarObjects[i] = obj;
							}
							else
							{
								throw new NotImplementedException();
							}
						}
					}
				}

				//Check for bogie objects and reverse if necessary.....
				for (int i = 0; i < Train.Cars.Length * 2; i++)
				{
					bool IsOdd = (i % 2 != 0);
					int CarIndex = i / 2;
					if (BogieObjects[i] != null)
					{
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
							if (BogieObjects[i] is StaticObject)
							{
								StaticObject obj = (StaticObject)BogieObjects[i].Clone();
								obj.ApplyScale(-1.0, 1.0, -1.0);
								BogieObjects[i] = obj;
							}
							else if (BogieObjects[i] is AnimatedObjectCollection)
							{
								AnimatedObjectCollection obj = (AnimatedObjectCollection)BogieObjects[i].Clone();
								obj.Reverse();
								BogieObjects[i] = obj;
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
