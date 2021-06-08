using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using TrainManager.Power;
using TrainManager.Trains;

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
		private static BveAccelerationCurve[] AccelerationCurves;
		internal void Parse(string fileName, TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref UnifiedObject[] CouplerObjects, out bool[] interiorVisible)
		{
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = System.IO.Path.GetDirectoryName(fileName);
			if (System.IO.File.Exists(OpenBveApi.Path.CombineFile(currentPath, "train.dat")))
			{
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					if (Train.Cars[i].Specs.IsMotorCar)
					{
						AccelerationCurves = new BveAccelerationCurve[Train.Cars[i].Specs.AccelerationCurves.Length];
						for (int j = 0; j < Train.Cars[i].Specs.AccelerationCurves.Length; j++)
						{
							BveAccelerationCurve c = (BveAccelerationCurve)Train.Cars[i].Specs.AccelerationCurves[j];
							AccelerationCurves[j] = c.Clone(c.Multiplier);
						}
					}
				}
			}
			CarObjectsReversed = new bool[Train.Cars.Length];
			BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			interiorVisible = new bool[Train.Cars.Length];
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/*[self::Car or self::Coupler]");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "No car nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty train.xml file");
				}

				int carIndex = 0;
				//Use the index here for easy access to the car count
				for (int i = 0; i < DocumentNodes.Count; i++)
				{
					if (carIndex > Train.Cars.Length)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "WARNING: A total of " + DocumentNodes.Count + " cars were specified in XML file " + fileName + " whilst only " + Train.Cars.Length + " were specified in the train.dat file.");
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
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Unexpected extra coupler encountered in XML file " + fileName);
								continue;
							}
							foreach (XmlNode c in DocumentNodes[i].ChildNodes)
							{
								switch (c.Name.ToLowerInvariant())
								{
									case "minimum":
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "MinimumDistanceBetweenCars is invalid for coupler " + carIndex + "in XML file " + fileName);
										}
										break;
									case "maximum":
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[carIndex - 1].Coupler.MaximumDistanceBetweenCars))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "MaximumDistanceBetweenCars is invalid for coupler " + carIndex + "in XML file " + fileName);
										}
										break;
									case "object":
										if (string.IsNullOrEmpty(c.InnerText))
										{
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid object path for Coupler " + (carIndex - 1) + " in XML file " + fileName);
											break;
										}
										string f = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
										if (System.IO.File.Exists(f))
										{
											Plugin.currentHost.LoadObject(f, Encoding.Default, out CouplerObjects[carIndex - 1]);
										}
										break;
								}
							}
						}

					}
					else if (!String.IsNullOrEmpty(DocumentNodes[i].InnerText))
					{
						try
						{
							string childFile = OpenBveApi.Path.CombineFile(currentPath, DocumentNodes[i].InnerText);
							XmlDocument childXML = new XmlDocument();
							childXML.Load(childFile);
							XmlNodeList childNodes = childXML.DocumentElement.SelectNodes("/openBVE/Car");
							//We need to save and restore the current path to make relative paths within the child file work correctly
							string savedPath = currentPath;
							currentPath = System.IO.Path.GetDirectoryName(childFile);
							ParseCarNode(childNodes[0], fileName, i, ref Train, ref CarObjects, ref BogieObjects, ref interiorVisible[carIndex]);
							currentPath = savedPath;
						}
						catch
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Failed to load the child Car XML file specified in " + DocumentNodes[i].InnerText);
						}
					}
					if (i == DocumentNodes.Count && carIndex < Train.Cars.Length)
					{
						//If this is the case, the number of motor cars is the primary thing which may get confused....
						//Not a lot to be done about this until a full replacement is built for the train.dat file & we can dump it entirely
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "WARNING: The number of cars specified in the train.xml file does not match that in the train.dat- Some properties may be invalid.");
					}
					if (DocumentNodes[i].Name == "Car")
					{
						carIndex++;
					}
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
								switch (c.Name.ToLowerInvariant())
								{
									case "power":
										Train.Handles.Power.NotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.Handles.Power.NotchDescriptions.Length; j++)
										{
											Size s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Power.NotchDescriptions[j]);
											if (s.Width > Train.Handles.Power.MaxWidth)
											{
												Train.Handles.Power.MaxWidth = s.Width;
											}
										}
										break;
									case "brake":
										Train.Handles.Brake.NotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.Handles.Brake.NotchDescriptions.Length; j++)
										{
											Size s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Brake.NotchDescriptions[j]);
											if (s.Width > Train.Handles.Brake.MaxWidth)
											{
												Train.Handles.Brake.MaxWidth = s.Width;
											}
										}
										break;
									case "locobrake":
										if (Train.Handles.LocoBrake == null)
										{
											continue;
										}
										Train.Handles.LocoBrake.NotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.Handles.LocoBrake.NotchDescriptions.Length; j++)
										{
											Size s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.LocoBrake.NotchDescriptions[j]);
											if (s.Width > Train.Handles.LocoBrake.MaxWidth)
											{
												Train.Handles.LocoBrake.MaxWidth = s.Width;
											}
										}
										break;
									case "reverser":
										Train.Handles.Reverser.NotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.Handles.Reverser.NotchDescriptions.Length; j++)
										{
											Size s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Reverser.NotchDescriptions[j]);
											if (s.Width > Train.Handles.Reverser.MaxWidth)
											{
												Train.Handles.Reverser.MaxWidth = s.Width;
											}
										}
										break;
								}
							}
						}

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
