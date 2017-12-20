using System;
using System.Xml;
using System.Drawing;
using System.Linq;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static void ParsePerformanceNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train, ref ObjectManager.UnifiedObject[] CarObjects, ref ObjectManager.UnifiedObject[] BogieObjects)
		{
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					case "power":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							ParsePowerNode(c, fileName, Car, ref Train, ref CarObjects, ref BogieObjects);
						}
						else if (!String.IsNullOrEmpty(c.InnerText))
						{
							try
							{
								string childFile = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
								XmlDocument childXML = new XmlDocument();
								childXML.Load(childFile);
								XmlNodeList childNodes = childXML.DocumentElement.SelectNodes("/openBVE/Power");
								//We need to save and restore the current path to make relative paths within the child file work correctly
								string savedPath = currentPath;
								currentPath = System.IO.Path.GetDirectoryName(childFile);
								ParsePowerNode(childNodes[0], fileName, Car, ref Train, ref CarObjects, ref BogieObjects);
								currentPath = savedPath;
							}
							catch
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "Failed to load the child PowerData XML file specified in " + c.InnerText);
							}
						}
						break;
					case "brake":
						if (c.ChildNodes.OfType<XmlElement>().Any())
						{
							ParseBrakeNode(c, fileName, Car, ref Train, ref CarObjects, ref BogieObjects);
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
								ParseBrakeNode(childNodes[0], fileName, Car, ref Train, ref CarObjects, ref BogieObjects);
								currentPath = savedPath;
							}
							catch
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "Failed to load the child PowerData XML file specified in " + c.InnerText);
							}
						}
						break;
				}
			}
		}

		private static void ParsePowerNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train, ref ObjectManager.UnifiedObject[] CarObjects, ref ObjectManager.UnifiedObject[] BogieObjects)
		{
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					/*
					 * TODO: At present, the delay variables are calculated on a per-train basis
					 * This should be shifted so that an induvidual car may present it's own delay value
					 */
					case "delayup":
						Train.Specs.DelayPowerUp = new double[Train.Specs.MaximumPowerNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++)
							{
								Train.Specs.DelayPowerUp[i] = 0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Specs.DelayPowerUp[i]))
									{
										Train.Specs.DelayPowerUp[i] = 0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid delay " + c.InnerText + " specified for PowerDelayUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "delaydown":
						Train.Specs.DelayPowerDown = new double[Train.Specs.MaximumPowerNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++)
							{
								Train.Specs.DelayPowerDown[i] = 0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Specs.DelayPowerDown[i]))
									{
										Train.Specs.DelayPowerDown[i] = 0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid delay " + c.InnerText + " specified for PowerDelayUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "jerkup":
						Train.Cars[Car].Specs.JerkPowerUp = new double[Train.Specs.MaximumPowerNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++)
							{
								Train.Cars[Car].Specs.JerkPowerUp[i] = 10.0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Cars[Car].Specs.JerkPowerUp[i]))
									{
										Train.Cars[Car].Specs.JerkPowerUp[i] = 10.0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid value " + c.InnerText + " specified for PowerJerkUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "jerkdown":
						Train.Cars[Car].Specs.JerkPowerDown = new double[Train.Specs.MaximumPowerNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++)
							{
								Train.Cars[Car].Specs.JerkPowerDown[i] = 10.0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Cars[Car].Specs.JerkPowerDown[i]))
									{
										Train.Cars[Car].Specs.JerkPowerDown[i] = 10.0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid value " + c.InnerText + " specified for PowerJerkDown, Notch " + i);
									}
								}
							}
						}
						break;
				}
			}
		}

		private static void ParseBrakeNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train, ref ObjectManager.UnifiedObject[] CarObjects, ref ObjectManager.UnifiedObject[] BogieObjects)
		{
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					/*
					 * TODO: At present, the delay variables are calculated on a per-train basis
					 * This should be shifted so that an induvidual car may present it's own delay value
					 */
					case "delayup":
						Train.Specs.DelayBrakeUp = new double[Train.Specs.MaximumBrakeNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++)
							{
								Train.Specs.DelayBrakeUp[i] = 0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Specs.DelayBrakeUp[i]))
									{
										Train.Specs.DelayBrakeUp[i] = 0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid delay " + c.InnerText + " specified for PowerDelayUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "delaydown":
						Train.Specs.DelayBrakeDown = new double[Train.Specs.MaximumBrakeNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++)
							{
								Train.Specs.DelayBrakeDown[i] = 0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Specs.DelayBrakeDown[i]))
									{
										Train.Specs.DelayBrakeDown[i] = 0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid delay " + c.InnerText + " specified for PowerDelayUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "jerkup":
						Train.Cars[Car].Specs.JerkBrakeUp = new double[Train.Specs.MaximumBrakeNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++)
							{
								Train.Cars[Car].Specs.JerkBrakeUp[i] = 10.0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Cars[Car].Specs.JerkBrakeUp[i]))
									{
										Train.Cars[Car].Specs.JerkBrakeUp[i] = 10.0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid value " + c.InnerText + " specified for BrakeJerkUp, Notch " + i);
									}
								}
							}
						}
						break;
					case "jerkdown":
						Train.Cars[Car].Specs.JerkBrakeDown = new double[Train.Specs.MaximumBrakeNotch];
						if (c.InnerText.IndexOf(';') != -1)
						{
							string[] splitStrings = c.InnerText.Split(';');

							for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++)
							{
								Train.Cars[Car].Specs.JerkBrakeDown[i] = 10.0;
								if (i < splitStrings.Length)
								{
									if (!double.TryParse(splitStrings[i].Trim(), out Train.Cars[Car].Specs.JerkBrakeDown[i]))
									{
										Train.Cars[Car].Specs.JerkBrakeDown[i] = 10.0;
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid value " + c.InnerText + " specified for BrakeJerkDown, Notch " + i);
									}
								}
							}
						}
						break;
				}
			}
		}
	}
}
