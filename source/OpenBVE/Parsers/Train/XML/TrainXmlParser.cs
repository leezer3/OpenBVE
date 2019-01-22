using System;
using System.Xml;
using System.Drawing;
using System.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static string currentPath;
		private static bool[] CarObjectsReversed;
		private static bool[] BogieObjectsReversed;
		private static TrainManager.BveAccelerationCurve[] AccelerationCurves;
		internal static void Parse(string fileName, TrainManager.Train Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects)
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
						AccelerationCurves = new TrainManager.BveAccelerationCurve[Train.Cars[i].Specs.AccelerationCurves.Length];
						for (int j = 0; j < Train.Cars[i].Specs.AccelerationCurves.Length; j++)
						{
							TrainManager.BveAccelerationCurve c = (TrainManager.BveAccelerationCurve)Train.Cars[i].Specs.AccelerationCurves[j];
							AccelerationCurves[j] = c.Clone(c.Multiplier);
						}
					}
				}
			}
			CarObjectsReversed = new bool[Train.Cars.Length];
			BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/Car");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Interface.AddMessage(MessageType.Error, false, "No car nodes defined in XML file " + fileName);
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty train.xml file");
				}
				//Use the index here for easy access to the car count
				for (int i = 0; i < DocumentNodes.Count; i++)
				{
					if (i > Train.Cars.Length)
					{
						Interface.AddMessage(MessageType.Warning, false, "WARNING: A total of " + DocumentNodes.Count + " cars were specified in XML file " + fileName + " whilst only " + Train.Cars.Length + " were specified in the train.dat file.");
						break;
					}
					if (DocumentNodes[i].ChildNodes.OfType<XmlElement>().Any())
					{
						ParseCarNode(DocumentNodes[i], fileName, i, ref Train, ref CarObjects, ref BogieObjects);
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
							ParseCarNode(childNodes[0], fileName, i, ref Train, ref CarObjects, ref BogieObjects);
							currentPath = savedPath;
						}
						catch
						{
							Interface.AddMessage(MessageType.Error, false, "Failed to load the child Car XML file specified in " + DocumentNodes[i].InnerText);
						}
					}
					if (i == DocumentNodes.Count && i < Train.Cars.Length)
					{
						//If this is the case, the number of motor cars is the primary thing which may get confused....
						//Not a lot to be done about this until a full replacement is built for the train.dat file & we can dump it entirely
						Interface.AddMessage(MessageType.Warning, false, "WARNING: The number of cars specified in the train.xml file does not match that in the train.dat- Some properties may be invalid.");
					}
				}
				if (Train.Cars[Train.DriverCar].CameraRestrictionMode != Camera.RestrictionMode.NotSpecified)
				{
					World.CameraRestriction = Train.Cars[Train.DriverCar].CameraRestrictionMode;
					World.UpdateViewingDistances();
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
										Train.PowerNotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.PowerNotchDescriptions.Length; j++)
										{
											Size s = Renderer.MeasureString(Fonts.NormalFont, Train.PowerNotchDescriptions[j]);
											if (s.Width > Train.MaxPowerNotchWidth)
											{
												Train.MaxPowerNotchWidth = s.Width;
											}
										}
										break;
									case "brake":
										Train.BrakeNotchDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.BrakeNotchDescriptions.Length; j++)
										{
											Size s = Renderer.MeasureString(Fonts.NormalFont, Train.BrakeNotchDescriptions[j]);
											if (s.Width > Train.MaxBrakeNotchWidth)
											{
												Train.MaxBrakeNotchWidth = s.Width;
											}
										}
										break;
									case "reverser":
										Train.ReverserDescriptions = c.InnerText.Split(';');
										for (int j = 0; j < Train.ReverserDescriptions.Length; j++)
										{
											Size s = Renderer.MeasureString(Fonts.NormalFont, Train.ReverserDescriptions[j]);
											if (s.Width > Train.MaxReverserWidth)
											{
												Train.MaxReverserWidth = s.Width;
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
							if (CarObjects[i] is ObjectManager.StaticObject)
							{
								ObjectManager.StaticObject obj = (ObjectManager.StaticObject)CarObjects[i];
								obj.ApplyScale(-1.0, 1.0, -1.0);
							}
							else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection)
							{
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++)
								{
									for (int h = 0; h < obj.Objects[j].States.Length; h++)
									{
										obj.Objects[j].States[h].Object.ApplyScale(-1.0, 1.0, -1.0);
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
								obj.ApplyScale(-1.0, 1.0, -1.0);
							}
							else if (BogieObjects[i] is ObjectManager.AnimatedObjectCollection)
							{
								ObjectManager.AnimatedObjectCollection obj = (ObjectManager.AnimatedObjectCollection)BogieObjects[i];
								for (int j = 0; j < obj.Objects.Length; j++)
								{
									for (int h = 0; h < obj.Objects[j].States.Length; h++)
									{
										obj.Objects[j].States[h].Object.ApplyScale(-1.0, 1.0, -1.0);
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
