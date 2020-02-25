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
		private static Jerk ParseJerkNode(string fileName, XElement parent)
		{
			Jerk jerk = new Jerk();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "power":
						jerk.Power = ParseJerkEntryNode(fileName, keyNode);
						break;
					case "brake":
						jerk.Brake = ParseJerkEntryNode(fileName, keyNode);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return jerk;
		}

		private static Jerk.Entry ParseJerkEntryNode(string fileName, XElement parent)
		{
			Jerk.Entry entry = new Jerk.Entry();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "up":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								entry.Up = result;
							}
						}
						break;
					case "down":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								entry.Down = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return entry;
		}
	}
}
