using System.Xml;
using OpenBveApi.Math;
using System.Linq;

namespace OpenBve.Parsers.Train
{
	partial class TrainXmlParser
	{
		private static void ParseCarNode(XmlNode Node, string fileName, int Car, ref TrainManager.Train Train, ref ObjectManager.UnifiedObject[] CarObjects, ref ObjectManager.UnifiedObject[] BogieObjects)
		{
			foreach (XmlNode c in Node.ChildNodes)
			{
				//Note: Don't use the short-circuiting operator, as otherwise we need another if
				switch (c.Name.ToLowerInvariant())
				{
					case "length":
						double l;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out l) | l <= 0.0)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid length defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Length = l;
						break;
					case "width":
						double w;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out w) | w <= 0.0)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid width defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Width = w;
						break;
					case "height":
						double h;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out h) | h <= 0.0)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid height defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Height = h;
						break;
					case "motorcar":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							Train.Cars[Car].Specs.IsMotorCar = true;
						}
						else
						{
							Train.Cars[Car].Specs.IsMotorCar = false;
						}
						break;
					case "mass":
						double m;
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out m) | m <= 0.0)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid mass defined for Car " + Car + " in XML file " + fileName);
							break;
						}
						Train.Cars[Car].Specs.MassEmpty = m;
						Train.Cars[Car].Specs.MassCurrent = m;
						break;
					case "frontaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].FrontAxle.Position))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "rearaxle":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Train.Cars[Car].RearAxle.Position))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear axle position defined for Car " + Car + " in XML file " + fileName);
						}
						break;
					case "object":
						string f = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);
						if (System.IO.File.Exists(f))
						{
							CarObjects[Car] = ObjectManager.LoadObject(f, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
						}
						break;
					case "reversed":
						if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
						{
							CarObjectsReversed[Car] = true;
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
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].FrontBogie.RearAxle.Position))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid front bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											BogieObjects[Car * 2] = ObjectManager.LoadObject(fb, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
										}
										break;
									case "reversed":
										BogieObjectsReversed[Car * 2] = true;
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
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear bogie, front axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "rearaxle":
										if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Train.Cars[Car].RearBogie.RearAxle.Position))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid rear bogie, rear axle position defined for Car " + Car + " in XML file " + fileName);
										}
										break;
									case "object":
										string fb = OpenBveApi.Path.CombineFile(currentPath, cc.InnerText);
										if (System.IO.File.Exists(fb))
										{
											BogieObjects[Car * 2 + 1] = ObjectManager.LoadObject(fb, System.Text.Encoding.Default, ObjectManager.ObjectLoadMode.Normal, false, false, false);
										}
										break;
									case "reversed":
										BogieObjectsReversed[Car * 2 + 1] = true;
										break;
								}
							}
						}
						break;
				}
			}
		}
	}
}
