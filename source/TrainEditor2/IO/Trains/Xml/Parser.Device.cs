using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static Device ParseDeviceNode(string fileName, XElement parent)
		{
			Device device = new Device();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "ats":
						device.Ats = ParseDeviceAtsNode(fileName, keyNode);
						break;
					case "atc":
						device.Atc = ParseDeviceAtcNode(fileName, keyNode);
						break;
					case "eb":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.Eb = result;
							}
						}
						break;
					case "constspeed":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.ConstSpeed = result;
							}
						}
						break;
					case "holdbrake":
						if (value.Any())
						{
							bool result;

							if (!bool.TryParse(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a boolean in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.HoldBrake = result;
							}
						}
						break;
					case "loadcompensatingdevice":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.LoadCompensatingDevice = result;
							}
						}
						break;
					case "passalarm":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.PassAlarmModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.PassAlarm = (Device.PassAlarmModes)result;
							}
						}
						break;
					case "dooropenmode":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.DoorModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.DoorOpenMode = (Device.DoorModes)result;
							}
						}
						break;
					case "doorclosemode":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.DoorModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								device.DoorCloseMode = (Device.DoorModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return device;
		}

		private static Device.AtsModes ParseDeviceAtsNode(string fileName, XElement parent)
		{
			Device.AtsModes ats = Device.AtsModes.AtsSn;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "type":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.AtsModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								ats = (Device.AtsModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return ats;
		}

		private static Device.AtcModes ParseDeviceAtcNode(string fileName, XElement parent)
		{
			Device.AtcModes atc = Device.AtcModes.None;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "type":
						if (value.Any())
						{
							int result;

							if (!NumberFormats.TryParseIntVb6(value, out result) || result < 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else if (!Enum.IsDefined(typeof(Device.AtcModes), result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a defined number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								atc = (Device.AtcModes)result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return atc;
		}
	}
}
