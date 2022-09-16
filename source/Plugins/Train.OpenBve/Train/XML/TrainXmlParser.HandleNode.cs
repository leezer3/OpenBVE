using System;
using System.Linq;
using System.Xml;
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
					switch (cc.Name.ToLowerInvariant())
					{
						case "notches":
							Regulator regulator = Handle as Regulator;
							if (Car != Train.DriverCar || regulator != null)
							{
								// only valid on driver car, non percentage based handles
								break;
							}

							if (Handle is AirBrakeHandle)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unable to define a number of notches for an AirBrake handle for Car " + Car + " in XML file " + fileName);
								break;
							}

							int numberOfNotches;
							if (!NumberFormats.TryParseIntVb6(cc.InnerText, out numberOfNotches) | numberOfNotches <= 0)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid number of handle notches defined for Car " + Car + " in XML file " + fileName);
								break;
							}

							// remember to increase the max driver notch too
							Handle.MaximumDriverNotch += numberOfNotches - Handle.MaximumNotch;
							Handle.MaximumNotch = numberOfNotches;
							break;
						case "springtime":
							if (Car != Train.DriverCar)
							{
								// only valid on driver car
								break;
							}

							if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out Handle.SpringTime) | Handle.SpringTime <= 0)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid handle spring time defined for Car " + Car + " in XML file " + fileName);
								Handle.SpringTime = 0;
								Handle.SpringType = SpringType.Unsprung;
							}

							break;
						case "springtype":
							if (!Enum.TryParse(cc.InnerText, true, out Handle.SpringType))
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid handle spring type defined for Car " + Car + " in XML file " + fileName);
								Handle.SpringTime = 0;
								Handle.SpringType = SpringType.Unsprung;
							}

							break;
						case "maxsprungnotch":
							if (Car != Train.DriverCar)
							{
								// only valid on driver car
								break;
							}

							int maxSpring;
							if (!NumberFormats.TryParseIntVb6(cc.InnerText, out maxSpring) | maxSpring > Handle.MaximumNotch)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid maximum handle spring value defined for Car " + Car + " in XML file " + fileName);
							}

							Handle.MaxSpring = maxSpring;
							break;
					}
				}
			}
		}
	}
}
