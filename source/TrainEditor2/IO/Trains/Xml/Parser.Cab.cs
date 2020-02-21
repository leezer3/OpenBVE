using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
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

			CultureInfo culture = CultureInfo.InvariantCulture;
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

							if (values.Length == 3)
							{
								double x, y, z;

								if (!NumberFormats.TryParseDoubleVb6(values[0], out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[1], out y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else if (!NumberFormats.TryParseDoubleVb6(values[2], out z))
								{
									Interface.AddMessage(MessageType.Error, false, $"Z must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									cab.PositionX = x;
									cab.PositionY = y;
									cab.PositionZ = z;
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
			CameraRestriction cameraRestriction = new CameraRestriction();

			CultureInfo culture = CultureInfo.InvariantCulture;

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
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedForwards = true;
								cameraRestriction.Forwards = result;
							}
						}
						break;
					case "backwards":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedBackwards = true;
								cameraRestriction.Backwards = result;
							}
						}
						break;
					case "left":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedLeft = true;
								cameraRestriction.Left = result;
							}
						}
						break;
					case "right":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedRight = true;
								cameraRestriction.Right = result;
							}
						}
						break;
					case "up":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedUp = true;
								cameraRestriction.Up = result;
							}
						}
						break;
					case "down":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								cameraRestriction.DefinedDown = true;
								cameraRestriction.Down = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			if (cameraRestriction.DefinedForwards && cameraRestriction.DefinedBackwards && cameraRestriction.Forwards < cameraRestriction.Backwards)
			{
				Interface.AddMessage(MessageType.Error, false, $"Backwards is expected to be less than or equal to Forwards in {section} in {fileName}");

				cameraRestriction.DefinedForwards = cameraRestriction.DefinedBackwards = false;
			}

			if (cameraRestriction.DefinedLeft && cameraRestriction.DefinedRight && cameraRestriction.Right < cameraRestriction.Left)
			{
				Interface.AddMessage(MessageType.Error, false, $"Left is expected to be less than or equal to Right in {section} in {fileName}");

				cameraRestriction.DefinedLeft = cameraRestriction.DefinedRight = false;
			}

			if (cameraRestriction.DefinedUp && cameraRestriction.DefinedDown && cameraRestriction.Up < cameraRestriction.Down)
			{
				Interface.AddMessage(MessageType.Error, false, $"Down is expected to be less than or equal to Up in {section} in {fileName}");

				cameraRestriction.DefinedUp = cameraRestriction.DefinedDown = false;
			}

			return cameraRestriction;
		}
	}
}
