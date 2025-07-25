using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Formats.OpenBve;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Panels;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Panels.Xml
{
	internal static partial class PanelCfgXml
	{
		internal static void Parse(string fileName, out Panel panel)
		{
			panel = new Panel();

			XDocument xml = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			List<XElement> panelNodes = xml.XPathSelectElements("/openBVE/Panel").ToList();

			if (!panelNodes.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"No panel nodes defined in XML file {fileName}");
				return;
			}

			foreach (XElement panelNode in panelNodes)
			{
				ParsePanelNode(fileName, panelNode, panel);
			}
		}

		private static void ParsePanelNode(string fileName, XElement parent, Panel panel)
		{
			foreach (XElement sectionNode in parent.Elements())
			{
				Enum.TryParse(sectionNode.Name.LocalName, true, out Panel2Sections section);
				switch (section)
				{
					case Panel2Sections.This:
						ParseThisNode(fileName, sectionNode, panel.This);
						break;
					case Panel2Sections.Screen:
						ParseScreenNode(fileName, sectionNode, out Screen screen);
						panel.Screens.Add(screen);
						break;
					case Panel2Sections.PilotLamp:
						ParsePilotLampElementNode(fileName, sectionNode, out PilotLampElement pilotLamp);
						panel.PanelElements.Add(pilotLamp);
						break;
					case Panel2Sections.Needle:
						ParseNeedleElementNode(fileName, sectionNode, out NeedleElement needle);
						panel.PanelElements.Add(needle);
						break;
					case Panel2Sections.DigitalNumber:
						ParseDigitalNumberElementNode(fileName, sectionNode, out DigitalNumberElement digitalNumber);
						panel.PanelElements.Add(digitalNumber);
						break;
					case Panel2Sections.DigitalGauge:
						ParseDigitalGaugeElementNode(fileName, sectionNode, out DigitalGaugeElement digitalGauge);
						panel.PanelElements.Add(digitalGauge);
						break;
					case Panel2Sections.LinearGauge:
						ParseLinearGaugeElementNode(fileName, sectionNode, out LinearGaugeElement linearGauge);
						panel.PanelElements.Add(linearGauge);
						break;
					case Panel2Sections.Timetable:
						ParseTimetableElementNode(fileName, sectionNode, out TimetableElement timetable);
						panel.PanelElements.Add(timetable);
						break;
				}
			}
		}

		private static void ParseThisNode(string fileName, XElement parent, This This)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Resolution:
						double pr = 0.0;

						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out pr))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}

						if (pr > 100)
						{
							This.Resolution = pr;
						}
						else
						{
							//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
							//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
							Interface.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given at line " + lineNumber.ToString(culture) + " in " + fileName);
						}

						break;
					case Panel2Key.Left:
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double left))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line{lineNumber.ToString(culture)} in {fileName}");
							}

							This.Left = left;
						}
						break;
					case Panel2Key.Right:
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double right))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Right = right;
						}
						break;
					case Panel2Key.Top:
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double top))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Top = top;
						}
						break;
					case Panel2Key.Bottom:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double bottom))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Bottom = bottom;
						}
						break;
					case Panel2Key.DaytimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							This.DaytimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(This.DaytimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {This.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}

						break;
					case Panel2Key.NighttimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							This.NighttimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(This.NighttimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {This.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}

						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Center:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();
								Vector2 center = This.Center;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									center.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									center.Y = y;
								}

								This.Center = center;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							break;
						}

					case Panel2Key.Origin:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								Vector2 origin = This.Origin;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									origin.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									origin.Y = y;
								}

								This.Origin = origin;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							break;
						}
				}
			}
		}

		private static void ParseScreenNode(string fileName, XElement parent, out Screen screen)
		{
			screen = new Screen();
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Number:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int number))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Number = number;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Layer = layer;
						}
						break;
					case Panel2Key.PilotLamp:
						ParsePilotLampElementNode(fileName, keyNode, out PilotLampElement pilotLamp);
						screen.PanelElements.Add(pilotLamp);
						break;
					case Panel2Key.Needle:
						ParseNeedleElementNode(fileName, keyNode, out NeedleElement needle);
						screen.PanelElements.Add(needle);
						break;
					case Panel2Key.DigitalNumber:
						ParseDigitalNumberElementNode(fileName, keyNode, out DigitalNumberElement digitalNumber);
						screen.PanelElements.Add(digitalNumber);
						break;
					case Panel2Key.DigitalGauge:
						ParseDigitalGaugeElementNode(fileName, keyNode, out DigitalGaugeElement digitalGauge);
						screen.PanelElements.Add(digitalGauge);
						break;
					case Panel2Key.LinearGauge:
						ParseLinearGaugeElementNode(fileName, keyNode, out LinearGaugeElement linearGauge);
						screen.PanelElements.Add(linearGauge);
						break;
					case Panel2Key.Timetable:
						ParseTimetableElementNode(fileName, keyNode, out TimetableElement timetable);
						screen.PanelElements.Add(timetable);
						break;
					case Panel2Key.Touch:
						ParseTouchElementNode(fileName, keyNode, screen, out TouchElement touch);
						screen.TouchElements.Add(touch);
						break;
				}
			}
		}

		private static void ParsePilotLampElementNode(string fileName, XElement parent, out PilotLampElement pilotLamp)
		{
			pilotLamp = new PilotLampElement();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);
				switch (key)
				{
					case Panel2Key.Subject:
						pilotLamp.Subject = Subject.StringToSubject(value, $"{section} in {fileName}");
						break;
					case Panel2Key.Location:
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							Vector2 location = pilotLamp.Location;
							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.X = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.Y = y;
							}

							pilotLamp.Location = location;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case Panel2Key.DaytimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							pilotLamp.DaytimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(pilotLamp.DaytimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {pilotLamp.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.NighttimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							pilotLamp.NighttimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(pilotLamp.NighttimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {pilotLamp.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							pilotLamp.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							pilotLamp.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseNeedleElementNode(string fileName, XElement parent, out NeedleElement needle)
		{
			needle = new NeedleElement();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Subject:
						needle.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case Panel2Key.Location:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								Vector2 location = needle.Location;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									location.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									location.Y = y;
								}

								needle.Location = location;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.Radius:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double radius))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Radius = radius;

							if (needle.Radius == 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is expected to be non-zero in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								needle.Radius = 16.0;
							}

							needle.DefinedRadius = true;
						}
						break;
					case Panel2Key.DaytimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							needle.DaytimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(needle.DaytimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {needle.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.NighttimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							needle.NighttimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(needle.NighttimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, "FileName " + needle.NighttimeImage + " could not be found in " + key + " in " + section + " at line " + lineNumber.ToString(culture) + " in " + fileName);
							}
						}
						break;
					case Panel2Key.Color:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Color = color;
						}
						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Origin:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								Vector2 origin = needle.Origin;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									origin.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
										origin.X = -origin.X;
									}

									origin.Y = y;
								}

								needle.Origin = origin;
								needle.DefinedOrigin = true;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.InitialAngle:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case Panel2Key.LastAngle:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.LastAngle = lastAngle.ToRadians();
						}
						break;
					case Panel2Key.Minimum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Minimum = minimum;
						}
						break;
					case Panel2Key.Maximum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Maximum = maximum;
						}
						break;
					case Panel2Key.NaturalFreq:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double naturalFreq))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.NaturalFreq = naturalFreq;

							if (needle.NaturalFreq < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is expected to be non-negative in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								needle.NaturalFreq = -needle.NaturalFreq;
							}

							needle.DefinedNaturalFreq = true;
						}
						break;
					case Panel2Key.DampingRatio:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double dampingRatio))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.DampingRatio = dampingRatio;

							if (needle.DampingRatio < 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is expected to be non-negative in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								needle.DampingRatio = -needle.DampingRatio;
							}

							needle.DefinedDampingRatio = true;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Layer = layer;
						}
						break;
					case Panel2Key.Backstop:
						if (value.Any() && value.ToLowerInvariant() == "true" || value == "1")
						{
							needle.Backstop = true;
						}
						break;
					case Panel2Key.Smoothed:
						if (value.Any() && value.ToLowerInvariant() == "true" || value == "1")
						{
							needle.Smoothed = true;
						}
						break;
				}
			}
		}

		private static void ParseDigitalNumberElementNode(string fileName, XElement parent, out DigitalNumberElement digitalNumber)
		{
			digitalNumber = new DigitalNumberElement();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Subject:
						digitalNumber.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case Panel2Key.Location:
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							Vector2 location = digitalNumber.Location;
							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.X = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.Y = y;
							}

							digitalNumber.Location = location;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case Panel2Key.DaytimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + key + " in " + section + " at line " + lineNumber.ToString(culture) + " in " + fileName);
						}
						else
						{
							digitalNumber.DaytimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(digitalNumber.DaytimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {digitalNumber.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.NighttimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							digitalNumber.NighttimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(digitalNumber.NighttimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {digitalNumber.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalNumber.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Interval:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int interval))
							{
								Interface.AddMessage(MessageType.Error, false, $"Height is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalNumber.Interval = interval;

							if (digitalNumber.Interval <= 0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Height is expected to be non-negative in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalNumber.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseDigitalGaugeElementNode(string fileName, XElement parent, out DigitalGaugeElement digitalGauge)
		{
			digitalGauge = new DigitalGaugeElement();
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Subject:
						digitalGauge.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case Panel2Key.Location:
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							Vector2 location = digitalGauge.Location;
							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.X = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.Y = y;
							}

							digitalGauge.Location = location;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case Panel2Key.Radius:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double radius))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Radius = radius;

							if (digitalGauge.Radius == 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is expected to be non-zero in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								digitalGauge.Radius = 16.0;
							}
						}
						break;
					case Panel2Key.Color:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Color = color;
						}
						break;
					case Panel2Key.InitialAngle:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case Panel2Key.LastAngle:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.LastAngle = lastAngle.ToRadians();
						}
						break;
					case Panel2Key.Minimum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Minimum = minimum;
						}
						break;
					case Panel2Key.Maximum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Maximum = maximum;
						}
						break;
					case Panel2Key.Step:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double step))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Step = step;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseLinearGaugeElementNode(string fileName, XElement parent, out LinearGaugeElement linearGauge)
		{
			linearGauge = new LinearGaugeElement();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = Path.GetDirectoryName(fileName);

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Subject:
						linearGauge.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case Panel2Key.Location:
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							Vector2 location = linearGauge.Location;
							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.X = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.Y = y;
							}

							linearGauge.Location = location;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case Panel2Key.Minimum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Minimum = minimum;
						}
						break;
					case Panel2Key.Maximum:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Maximum = maximum;
						}
						break;
					case Panel2Key.Width:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int width))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Width = width;
						}
						break;
					case Panel2Key.Direction:
						{
							string[] s = value.Split(',');

							if (s.Length == 2)
							{

								Vector2 direction = linearGauge.Direction;
								if (!NumberFormats.TryParseIntVb6(s[0], out int x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
								}

								direction.X = x;

								if (linearGauge.Direction.X < -1 || linearGauge.Direction.X > 1)
								{
									Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1 in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									direction.X = 0;
								}

								if (!NumberFormats.TryParseIntVb6(s[1], out int y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y is invalid in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
								}

								direction.Y = y;

								if (linearGauge.Direction.Y < -1 || linearGauge.Direction.Y > 1)
								{
									Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1 in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									direction.Y = 0;
								}

								linearGauge.Direction = direction;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly 2 arguments are expected in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
							}
						}
						break;
					case Panel2Key.DaytimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							linearGauge.DaytimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(linearGauge.DaytimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {linearGauge.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.NighttimeImage:
						if (!Path.HasExtension(value))
						{
							value += ".bmp";
						}

						if (Path.ContainsInvalidChars(value))
						{
							Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							linearGauge.NighttimeImage = Path.CombineFile(basePath, value);

							if (!File.Exists(linearGauge.NighttimeImage))
							{
								Interface.AddMessage(MessageType.Warning, true, $"FileName {linearGauge.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseTimetableElementNode(string fileName, XElement parent, out TimetableElement timetable)
		{
			timetable = new TimetableElement();
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Location:
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							Vector2 location = timetable.Location;
							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.X = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								location.Y = y;
							}
							
							timetable.Location = location;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case Panel2Key.Width:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double width))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.Width = width;

							if (timetable.Width <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is required to be positive in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.Height:
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double height))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.Height = height;

							if (timetable.Height <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is required to be positive in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.TransparentColor:
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.TransparentColor = transparentColor;
						}
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseTouchElementNode(string fileName, XElement parent, Screen screen, out TouchElement touch)
		{
			touch = new TouchElement(screen);
			TouchElement.CommandEntry commandEntry = new TouchElement.CommandEntry();
			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
				Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

				switch (key)
				{
					case Panel2Key.Location:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								Vector2 location = touch.Location;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									location.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									location.Y = y;
								}

								touch.Location = location;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.Size:
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								Vector2 size = touch.Size;
								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									size.X = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									size.Y = y;
								}

								touch.Size = size;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case Panel2Key.JumpScreen:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int jumpScreen))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.JumpScreen = jumpScreen;
						}
						break;
					case Panel2Key.SoundIndex:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int soundIndex))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.SoundEntries.Add(new TouchElement.SoundEntry { Index = soundIndex });
						}
						break;
					case Panel2Key.Command:
						{
							if (!touch.CommandEntries.Contains(commandEntry))
							{
								touch.CommandEntries.Add(commandEntry);
							}

							if (string.Compare(value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								break;
							}

							int i;
							Translations.CommandInfo info = new Translations.CommandInfo();

							for (i = 0; i < Translations.CommandInfos.Count; i++)
							{
								Translations.Command command = Translations.CommandInfos.ElementAt(i).Key;
								if (string.Compare(value, Translations.CommandInfos[command].Name, StringComparison.OrdinalIgnoreCase) == 0)
								{
									break;
								}
							}

							if (i == Translations.CommandInfos.Count || info.Type != Translations.CommandType.Digital)
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								commandEntry.Info = info;
							}
						}
						break;
					case Panel2Key.CommandOption:
						if (!touch.CommandEntries.Contains(commandEntry))
						{
							touch.CommandEntries.Add(commandEntry);
						}

						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int commandOption))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								commandEntry.Option = commandOption;
							}
						}
						break;
					case Panel2Key.SoundEntries:
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)keyNode).LineNumber} in XML file {fileName}");
							break;
						}

						ParseTouchElementSoundEntryNode(fileName, keyNode, touch.SoundEntries);
						break;
					case Panel2Key.CommandEntries:
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)keyNode).LineNumber} in XML file {fileName}");
							break;
						}

						ParseTouchElementCommandEntryNode(fileName, keyNode, touch.CommandEntries);
						break;
					case Panel2Key.Layer:
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.Layer = layer;
						}
						break;
				}
			}
		}

		private static void ParseTouchElementSoundEntryNode(string fileName, XElement parent, ICollection<TouchElement.SoundEntry> entries)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					TouchElement.SoundEntry entry = new TouchElement.SoundEntry();
					CultureInfo culture = CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
						Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

						switch (key)
						{
							case Panel2Key.Index:
								if (value.Any())
								{

									if (!NumberFormats.TryParseIntVb6(value, out int index))
									{
										Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									entry.Index = index;
								}
								break;
						}
					}

					entries.Add(entry);
				}
			}
		}

		private static void ParseTouchElementCommandEntryNode(string fileName, XElement parent, ICollection<TouchElement.CommandEntry> entries)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					TouchElement.CommandEntry entry = new TouchElement.CommandEntry();
					CultureInfo culture = CultureInfo.InvariantCulture;

					string section = childNode.Name.LocalName;

					foreach (XElement keyNode in childNode.Elements())
					{
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;
						Enum.TryParse(keyNode.Name.LocalName, true, out Panel2Key key);

						switch (key)
						{
							case Panel2Key.Name:
								if (string.Compare(value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									break;
								}

								int i;
								Translations.CommandInfo info = new Translations.CommandInfo();

								for (i = 0; i < Translations.CommandInfos.Count; i++)
								{
									Translations.Command command = Translations.CommandInfos.ElementAt(i).Key;
									if (string.Compare(value, Translations.CommandInfos[command].Name, StringComparison.OrdinalIgnoreCase) == 0)
									{
										break;
									}
								}

								if (i == Translations.CommandInfos.Count || info.Type != Translations.CommandType.Digital)
								{
									Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									entry.Info = info;
								}
								break;
							case Panel2Key.Option:
								if (value.Any())
								{

									if (!NumberFormats.TryParseIntVb6(value, out int option))
									{
										Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}
									else
									{
										entry.Option = option;
									}
								}
								break;
						}
					}

					entries.Add(entry);
				}
			}
		}
	}
}
