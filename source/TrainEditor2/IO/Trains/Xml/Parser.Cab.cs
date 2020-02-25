using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Units;
using TrainEditor2.IO.Panels.Xml;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static Cab ParseCabNode(string fileName, XElement parent)
		{
			Cab cab = new EmbeddedCab();

			string basePath = System.IO.Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			bool isExternalCab = false;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "panel":
						if (keyNode.HasElements)
						{
							isExternalCab = false;
							cab = new EmbeddedCab();

							PanelCfgXml.Parse(fileName, keyNode, ((EmbeddedCab)cab).Panel);
						}
						else
						{
							isExternalCab = true;
							cab = new ExternalCab();

							if (value.Any())
							{
								if (Path.ContainsInvalidChars(value))
								{
									Interface.AddMessage(MessageType.Error, false, $"Value contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									string file = Path.CombineFile(basePath, value);

									if (!File.Exists(file))
									{
										Interface.AddMessage(MessageType.Warning, true, $"The {key} object {file} does not exist in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									((ExternalCab)cab).FileName = file;
								}
							}
						}
						break;
				}
			}

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "position":
						if (value.Any())
						{
							string[] values = value.Split(',');
							string[] unitValues = keyNode.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", StringComparison.InvariantCultureIgnoreCase))?.Value.Split(',');

							if (values.Length == 3)
							{
								double x, y, z;
								Unit.Length xUnit = Unit.Length.Millimeter, yUnit = Unit.Length.Millimeter, zUnit = Unit.Length.Millimeter;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out x) || unitValues != null && unitValues.Length > 0 && !Unit.TryParse(unitValues[0], true, out xUnit))
								{
									Interface.AddMessage(MessageType.Error, false, $"X must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out y) || unitValues != null && unitValues.Length > 1 && !Unit.TryParse(unitValues[1], true, out yUnit))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[2], out z) || unitValues != null && unitValues.Length > 2 && !Unit.TryParse(unitValues[2], true, out zUnit))
								{
									Interface.AddMessage(MessageType.Error, false, $"Z must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									cab.PositionX = new Quantity.Length(x, xUnit);
									cab.PositionY = new Quantity.Length(y, yUnit);
									cab.PositionZ = new Quantity.Length(z, zUnit);
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly three arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "camerarestriction":
						if (isExternalCab)
						{
							((ExternalCab)cab).CameraRestriction = ParseCameraRestrictionNode(fileName, keyNode);
						}
						else
						{
							Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in uncontrolled car at line {lineNumber.ToString(culture)} in XML file {fileName}");
						}
						break;
					case "panel":
						// Ignore
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return cab;
		}

		private static CameraRestriction ParseCameraRestrictionNode(string fileName, XElement parent)
		{
			CameraRestriction restriction = new CameraRestriction();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "forwards":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedForwards = true;
								restriction.Forwards = result;
							}
						}
						break;
					case "backwards":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedBackwards = true;
								restriction.Backwards = result;
							}
						}
						break;
					case "left":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedLeft = true;
								restriction.Left = result;
							}
						}
						break;
					case "right":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedRight = true;
								restriction.Right = result;
							}
						}
						break;
					case "up":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedUp = true;
								restriction.Up = result;
							}
						}
						break;
					case "down":
						if (value.Any())
						{
							Quantity.Length result;

							if (!Quantity.Length.TryParse(keyNode, true, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								restriction.DefinedDown = true;
								restriction.Down = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			if (restriction.DefinedForwards && restriction.DefinedBackwards && restriction.Forwards < restriction.Backwards)
			{
				Interface.AddMessage(MessageType.Error, false, $"Backwards is expected to be less than or equal to Forwards in {section} in {fileName}");

				restriction.DefinedForwards = restriction.DefinedBackwards = false;
			}

			if (restriction.DefinedLeft && restriction.DefinedRight && restriction.Right < restriction.Left)
			{
				Interface.AddMessage(MessageType.Error, false, $"Left is expected to be less than or equal to Right in {section} in {fileName}");

				restriction.DefinedLeft = restriction.DefinedRight = false;
			}

			if (restriction.DefinedUp && restriction.DefinedDown && restriction.Up < restriction.Down)
			{
				Interface.AddMessage(MessageType.Error, false, $"Down is expected to be less than or equal to Up in {section} in {fileName}");

				restriction.DefinedUp = restriction.DefinedDown = false;
			}

			return restriction;
		}
	}
}
