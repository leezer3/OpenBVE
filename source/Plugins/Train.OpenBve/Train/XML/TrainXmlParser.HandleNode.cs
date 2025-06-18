using System;
using System.Linq;
using System.Xml;
using Formats.OpenBve;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseHandleNode(XmlNode c, ref AbstractHandle Handle, int Car, TrainBase Train, string fileName)
		{
			if (c.ChildNodes.OfType<XmlElement>().Any())
			{
				foreach (XmlNode cc in c.ChildNodes)
				{
					Enum.TryParse(cc.Name, true, out HandleXMLKey key);
					switch (key)
					{
						case HandleXMLKey.Notches:
							if (Car != Train.DriverCar)
							{
								// only valid on driver car
								break;
							}

							if (Handle is AirBrakeHandle)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unable to define a number of notches for an AirBrake handle for Car " + Car + " in XML file " + fileName);
								break;
							}
							
							if (!NumberFormats.TryParseIntVb6(cc.InnerText, out int numberOfNotches) | numberOfNotches < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid number of handle notches defined for Car " + Car + " in XML file " + fileName);
							}

							// remember to increase the max driver notch too
							Handle.MaximumDriverNotch += numberOfNotches - Handle.MaximumNotch;
							Handle.MaximumNotch = numberOfNotches;
							break;
						case HandleXMLKey.SpringTime:
							if (Car != Train.DriverCar)
							{
								// only valid on driver car
								break;
							}

							if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Handle.SpringTime) | Handle.SpringTime <= 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid handle spring time defined for Car " + Car + " in XML file " + fileName);
								Handle.SpringTime = 0;
								Handle.SpringType = SpringType.Unsprung;
							}

							break;
						case HandleXMLKey.SpringType:
							if (!Enum.TryParse(cc.InnerText, true, out Handle.SpringType))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid handle spring type defined for Car " + Car + " in XML file " + fileName);
								Handle.SpringTime = 0;
								Handle.SpringType = SpringType.Unsprung;
							}

							break;
						case HandleXMLKey.MaxSprungNotch:
							if (Car != Train.DriverCar)
							{
								// only valid on driver car
								break;
							}

							if (!NumberFormats.TryParseIntVb6(cc.InnerText, out int maxSpring) | maxSpring > Handle.MaximumNotch)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid maximum handle spring value defined for Car " + Car + " in XML file " + fileName);
							}

							Handle.MaxSpring = maxSpring;
							break;
					}
				}
			}
		}
	}
}
