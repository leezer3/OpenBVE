using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
				switch (sectionNode.Name.LocalName.ToLowerInvariant())
				{
					case "This":
						ParseThisNode(fileName, sectionNode, panel.This);
						break;
					case "screen":
						ParseScreenNode(fileName, sectionNode, out Screen screen);
						panel.Screens.Add(screen);
						break;
					case "pilotlamp":
						ParsePilotLampElementNode(fileName, sectionNode, out PilotLampElement pilotLamp);
						panel.PanelElements.Add(pilotLamp);
						break;
					case "needle":
						ParseNeedleElementNode(fileName, sectionNode, out NeedleElement needle);
						panel.PanelElements.Add(needle);
						break;
					case "digitalnumber":
						ParseDigitalNumberElementNode(fileName, sectionNode, out DigitalNumberElement digitalNumber);
						panel.PanelElements.Add(digitalNumber);
						break;
					case "digitalgauge":
						ParseDigitalGaugeElementNode(fileName, sectionNode, out DigitalGaugeElement digitalGauge);
						panel.PanelElements.Add(digitalGauge);
						break;
					case "lineargauge":
						ParseLinearGaugeElementNode(fileName, sectionNode, out LinearGaugeElement linearGauge);
						panel.PanelElements.Add(linearGauge);
						break;
					case "timetable":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "resolution":
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
					case "left":
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double left))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line{lineNumber.ToString(culture)} in {fileName}");
							}

							This.Left = left;
						}
						break;
					case "right":
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double right))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Right = right;
						}
						break;
					case "top":
						if (value.Any())
						{
							if (!NumberFormats.TryParseDoubleVb6(value, out double top))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Top = top;
						}
						break;
					case "bottom":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double bottom))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Bottom = bottom;
						}
						break;
					case "daytimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "nighttimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.TransparentColor = transparentColor;
						}
						break;
					case "center":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.CenterX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.CenterY = y;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							break;
						}

					case "origin":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.OriginX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.OriginY = y;
								}
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "number":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int number))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Number = number;
						}
						break;
					case "layer":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Layer = layer;
						}
						break;
					case "pilotlamp":
						ParsePilotLampElementNode(fileName, keyNode, out PilotLampElement pilotLamp);
						screen.PanelElements.Add(pilotLamp);
						break;
					case "needle":
						ParseNeedleElementNode(fileName, keyNode, out NeedleElement needle);
						screen.PanelElements.Add(needle);
						break;
					case "digitalnumber":
						ParseDigitalNumberElementNode(fileName, keyNode, out DigitalNumberElement digitalNumber);
						screen.PanelElements.Add(digitalNumber);
						break;
					case "digitalgauge":
						ParseDigitalGaugeElementNode(fileName, keyNode, out DigitalGaugeElement digitalGauge);
						screen.PanelElements.Add(digitalGauge);
						break;
					case "lineargauge":
						ParseLinearGaugeElementNode(fileName, keyNode, out LinearGaugeElement linearGauge);
						screen.PanelElements.Add(linearGauge);
						break;
					case "timetable":
						ParseTimetableElementNode(fileName, keyNode, out TimetableElement timetable);
						screen.PanelElements.Add(timetable);
						break;
					case "touch":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "subject":
						pilotLamp.Subject = Subject.StringToSubject(value, $"{section} in {fileName}");
						break;
					case "location":
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								pilotLamp.LocationX = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								pilotLamp.LocationY = y;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case "daytimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "nighttimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							pilotLamp.TransparentColor = transparentColor;
						}
						break;
					case "layer":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "subject":
						needle.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case "location":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									needle.LocationX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									needle.LocationY = y;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "radius":
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
					case "daytimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "nighttimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "color":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Color = color;
						}
						break;
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.TransparentColor = transparentColor;
						}
						break;
					case "origin":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									needle.OriginX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
										needle.OriginX = -needle.OriginX;
									}

									needle.OriginY = y;
								}

								needle.DefinedOrigin = true;
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "initialangle":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case "lastangle":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.LastAngle = lastAngle.ToRadians();
						}
						break;
					case "minimum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Maximum = maximum;
						}
						break;
					case "naturalfreq":
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
					case "dampingratio":
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
					case "layer":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Layer = layer;
						}
						break;
					case "backstop":
						if (value.Any() && value.ToLowerInvariant() == "true" || value == "1")
						{
							needle.Backstop = true;
						}
						break;
					case "smoothed":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "subject":
						digitalNumber.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case "location":
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalNumber.LocationX = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalNumber.LocationY = y;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case "daytimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "nighttimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalNumber.TransparentColor = transparentColor;
						}
						break;
					case "interval":
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
					case "layer":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "subject":
						digitalGauge.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case "location":
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalGauge.LocationX = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalGauge.LocationY = y;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case "radius":
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
					case "color":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Color = color;
						}
						break;
					case "initialangle":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case "lastangle":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.LastAngle = lastAngle.ToRadians();
						}
						break;
					case "minimum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Maximum = maximum;
						}
						break;
					case "step":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double step))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Step = step;
						}
						break;
					case "layer":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "subject":
						linearGauge.Subject = Subject.StringToSubject(value, $"{section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
					case "location":
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								linearGauge.LocationX = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								linearGauge.LocationY = y;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case "minimum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{

							if (!NumberFormats.TryParseDoubleVb6(value, out double maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Maximum = maximum;
						}
						break;
					case "width":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int width))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Width = width;
						}
						break;
					case "direction":
						{
							string[] s = value.Split(',');

							if (s.Length == 2)
							{

								if (!NumberFormats.TryParseIntVb6(s[0], out int x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
								}

								linearGauge.DirectionX = x;

								if (linearGauge.DirectionX < -1 || linearGauge.DirectionX > 1)
								{
									Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									linearGauge.DirectionX = 0;
								}

								if (!NumberFormats.TryParseIntVb6(s[1], out int y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y is invalid in  LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
								}

								linearGauge.DirectionY = y;

								if (linearGauge.DirectionY < -1 || linearGauge.DirectionY > 1)
								{
									Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									linearGauge.DirectionY = 0;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Exactly 2 arguments are expected in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
							}
						}
						break;
					case "daytimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "nighttimeimage":
						if (!System.IO.Path.HasExtension(value))
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
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.TransparentColor = transparentColor;
						}
						break;
					case "layer":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "location":
						int k = value.IndexOf(',');

						if (k >= 0)
						{
							string a = value.Substring(0, k).TrimEnd();
							string b = value.Substring(k + 1).TrimStart();

							if (a.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(a, out double x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								timetable.LocationX = x;
							}

							if (b.Any())
							{

								if (!NumberFormats.TryParseDoubleVb6(b, out double y))
								{
									Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								timetable.LocationY = y;
							}
						}
						else
						{
							Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case "width":
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
					case "height":
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
					case "transparentcolor":
						if (value.Any())
						{

							if (!Color24.TryParseHexColor(value, out Color24 transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.TransparentColor = transparentColor;
						}
						break;
					case "layer":
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
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (keyNode.Name.LocalName.ToLowerInvariant())
				{
					case "location":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.LocationX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.LocationY = y;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "size":
						{
							int k = value.IndexOf(',');

							if (k >= 0)
							{
								string a = value.Substring(0, k).TrimEnd();
								string b = value.Substring(k + 1).TrimStart();

								if (a.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(a, out double x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.SizeX = x;
								}

								if (b.Any())
								{

									if (!NumberFormats.TryParseDoubleVb6(b, out double y))
									{
										Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.SizeY = y;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
						}
						break;
					case "jumpscreen":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int jumpScreen))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.JumpScreen = jumpScreen;
						}
						break;
					case "soundindex":
						if (value.Any())
						{

							if (!NumberFormats.TryParseIntVb6(value, out int soundIndex))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.SoundEntries.Add(new TouchElement.SoundEntry { Index = soundIndex });
						}
						break;
					case "command":
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
					case "commandoption":
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
					case "soundentries":
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)keyNode).LineNumber} in XML file {fileName}");
							break;
						}

						ParseTouchElementSoundEntryNode(fileName, keyNode, touch.SoundEntries);
						break;
					case "commandentries":
						if (!keyNode.HasElements)
						{
							Interface.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)keyNode).LineNumber} in XML file {fileName}");
							break;
						}

						ParseTouchElementCommandEntryNode(fileName, keyNode, touch.CommandEntries);
						break;
					case "layer":
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
						string key = keyNode.Name.LocalName;
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

						switch (keyNode.Name.LocalName.ToLowerInvariant())
						{
							case "index":
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
						string key = keyNode.Name.LocalName;
						string value = keyNode.Value;
						int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

						switch (keyNode.Name.LocalName.ToLowerInvariant())
						{
							case "name":
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
							case "option":
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
