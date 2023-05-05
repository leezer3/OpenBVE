using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.World;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static Delay ParseDelayNode(string fileName, XElement parent)
		{
			Delay delay = new Delay();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "power":
						ParseDelayEntriesNode(fileName, keyNode, delay.Power);
						break;
					case "brake":
						ParseDelayEntriesNode(fileName, keyNode, delay.Brake);
						break;
					case "locobrake":
						ParseDelayEntriesNode(fileName, keyNode, delay.LocoBrake);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return delay;
		}

		private static void ParseDelayEntriesNode(string fileName, XElement parent, ICollection<Delay.Entry> entries)
		{
			entries.Clear();

			double[] up = new double[0];
			Unit.Time[] upUnits = new Unit.Time[0];
			double[] down = new double[0];
			Unit.Time[] downUnits = new Unit.Time[0];

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
							up = value.Split(',')
								.Select(x =>
								{
									double result;

									if (!NumberFormats.TryParseDoubleVb6(x, out result) || result < 0.0)
									{
										Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.Where(x => x >= 0.0)
								.ToArray();

							upUnits = keyNode.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", StringComparison.InvariantCultureIgnoreCase))?.Value.Split(',')
								.Select(x =>
								{
									Unit.Time result;

									if (!Unit.TryParse(x, true, out result))
									{
										Interface.AddMessage(MessageType.Error, false, $"Unit is invalid value in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.ToArray();
						}
						break;
					case "down":
						if (value.Any())
						{
							down = value.Split(',')
								.Select(x =>
								{
									double result;

									if (!NumberFormats.TryParseDoubleVb6(x, out result) || result < 0.0)
									{
										Interface.AddMessage(MessageType.Error, false, $"Value must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.Where(x => x >= 0.0)
								.ToArray();

							downUnits = keyNode.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", StringComparison.InvariantCultureIgnoreCase))?.Value.Split(',')
								.Select(x =>
								{
									Unit.Time result;

									if (!Unit.TryParse(x, true, out result))
									{
										Interface.AddMessage(MessageType.Error, false, $"Unit is invalid value in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									return result;
								})
								.ToArray();
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			for (int i = 0; i < Math.Max(up.Length, down.Length); i++)
			{
				Delay.Entry entry = new Delay.Entry();

				if (i < up.Length)
				{
					entry.Up = new Quantity.Time(up[i], upUnits != null && i < upUnits.Length ? upUnits[i] : Unit.Time.Second);
				}

				if (i < down.Length)
				{
					entry.Down = new Quantity.Time(down[i], downUnits != null && i < downUnits.Length ? downUnits[i] : Unit.Time.Second);
				}

				entries.Add(entry);
			}
		}
	}
}
