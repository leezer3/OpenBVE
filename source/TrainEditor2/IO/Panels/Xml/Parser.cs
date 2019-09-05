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
						Screen screen;
						ParseScreenNode(fileName, sectionNode, out screen);
						panel.Screens.Add(screen);
						break;
					case "pilotlamp":
						PilotLampElement pilotLamp;
						ParsePilotLampElementNode(fileName, sectionNode, out pilotLamp);
						panel.PanelElements.Add(pilotLamp);
						break;
					case "needle":
						NeedleElement needle;
						ParseNeedleElementNode(fileName, sectionNode, out needle);
						panel.PanelElements.Add(needle);
						break;
					case "digitalnumber":
						DigitalNumberElement digitalNumber;
						ParseDigitalNumberElementNode(fileName, sectionNode, out digitalNumber);
						panel.PanelElements.Add(digitalNumber);
						break;
					case "digitalgauge":
						DigitalGaugeElement digitalGauge;
						ParseDigitalGaugeElementNode(fileName, sectionNode, out digitalGauge);
						panel.PanelElements.Add(digitalGauge);
						break;
					case "lineargauge":
						LinearGaugeElement linearGauge;
						ParseLinearGaugeElementNode(fileName, sectionNode, out linearGauge);
						panel.PanelElements.Add(linearGauge);
						break;
					case "timetable":
						TimetableElement timetable;
						ParseTimetableElementNode(fileName, sectionNode, out timetable);
						panel.PanelElements.Add(timetable);
						break;
				}
			}
		}

		private static void ParseThisNode(string fileName, XElement parent, This This)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

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
							double left;

							if (!NumberFormats.TryParseDoubleVb6(value, out left))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line{lineNumber.ToString(culture)} in {fileName}");
							}

							This.Left = left;
						}
						break;
					case "right":
						if (value.Any())
						{
							double right;

							if (!NumberFormats.TryParseDoubleVb6(value, out right))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Right = right;
						}
						break;
					case "top":
						if (value.Any())
						{
							double top;

							if (!NumberFormats.TryParseDoubleVb6(value, out top))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							This.Top = top;
						}
						break;
					case "bottom":
						if (value.Any())
						{
							double bottom;

							if (!NumberFormats.TryParseDoubleVb6(value, out bottom))
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {This.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								This.DaytimeImage = string.Empty;
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {This.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								This.NighttimeImage = string.Empty;
							}
						}

						break;
					case "transparentcolor":
						if (value.Any())
						{
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.CenterX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									This.OriginX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							int number;

							if (!NumberFormats.TryParseIntVb6(value, out number))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Number = number;
						}
						break;
					case "layer":
						if (value.Any())
						{
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
							{
								Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							screen.Layer = layer;
						}
						break;
					case "pilotlamp":
						PilotLampElement pilotLamp;
						ParsePilotLampElementNode(fileName, keyNode, out pilotLamp);
						screen.PanelElements.Add(pilotLamp);
						break;
					case "needle":
						NeedleElement needle;
						ParseNeedleElementNode(fileName, keyNode, out needle);
						screen.PanelElements.Add(needle);
						break;
					case "digitalnumber":
						DigitalNumberElement digitalNumber;
						ParseDigitalNumberElementNode(fileName, keyNode, out digitalNumber);
						screen.PanelElements.Add(digitalNumber);
						break;
					case "digitalgauge":
						DigitalGaugeElement digitalGauge;
						ParseDigitalGaugeElementNode(fileName, keyNode, out digitalGauge);
						screen.PanelElements.Add(digitalGauge);
						break;
					case "lineargauge":
						LinearGaugeElement linearGauge;
						ParseLinearGaugeElementNode(fileName, keyNode, out linearGauge);
						screen.PanelElements.Add(linearGauge);
						break;
					case "timetable":
						TimetableElement timetable;
						ParseTimetableElementNode(fileName, keyNode, out timetable);
						screen.PanelElements.Add(timetable);
						break;
					case "touch":
						TouchElement touch;
						ParseTouchElementNode(fileName, keyNode, screen, out touch);
						screen.TouchElements.Add(touch);
						break;
				}
			}
		}

		private static void ParsePilotLampElementNode(string fileName, XElement parent, out PilotLampElement pilotLamp)
		{
			pilotLamp = new PilotLampElement();
			CultureInfo culture = CultureInfo.InvariantCulture;
			string basePath = System.IO.Path.GetDirectoryName(fileName);

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
								double x;

								if (!NumberFormats.TryParseDoubleVb6(a, out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								pilotLamp.LocationX = x;
							}

							if (b.Any())
							{
								double y;

								if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {pilotLamp.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								pilotLamp.DaytimeImage = string.Empty;
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {pilotLamp.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								pilotLamp.NighttimeImage = string.Empty;
							}
						}
						break;
					case "transparentcolor":
						if (value.Any())
						{
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							pilotLamp.TransparentColor = transparentColor;
						}
						break;
					case "layer":
						if (value.Any())
						{
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
			string basePath = System.IO.Path.GetDirectoryName(fileName);

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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									needle.LocationX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							double radius;

							if (!NumberFormats.TryParseDoubleVb6(value, out radius))
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {needle.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								needle.DaytimeImage = string.Empty;
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
								Interface.AddMessage(MessageType.Error, true, "FileName " + needle.NighttimeImage + " could not be found in " + key + " in " + section + " at line " + lineNumber.ToString(culture) + " in " + fileName);
								needle.NighttimeImage = string.Empty;
							}
						}
						break;
					case "color":
						if (value.Any())
						{
							Color24 color;

							if (!Color24.TryParseHexColor(value, out color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Color = color;
						}
						break;
					case "transparentcolor":
						if (value.Any())
						{
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									needle.OriginX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							double initialAngle;

							if (!NumberFormats.TryParseDoubleVb6(value, out initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case "lastangle":
						if (value.Any())
						{
							double lastAngle;

							if (!NumberFormats.TryParseDoubleVb6(value, out lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.LastAngle = lastAngle.ToRadians();
						}
						break;
					case "minimum":
						if (value.Any())
						{
							double minimum;

							if (!NumberFormats.TryParseDoubleVb6(value, out minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{
							double maximum;

							if (!NumberFormats.TryParseDoubleVb6(value, out maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							needle.Maximum = maximum;
						}
						break;
					case "naturalfreq":
						if (value.Any())
						{
							double naturalFreq;

							if (!NumberFormats.TryParseDoubleVb6(value, out naturalFreq))
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
							double dampingRatio;

							if (!NumberFormats.TryParseDoubleVb6(value, out dampingRatio))
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
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
			string basePath = System.IO.Path.GetDirectoryName(fileName);

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
								double x;

								if (!NumberFormats.TryParseDoubleVb6(a, out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalNumber.LocationX = x;
							}

							if (b.Any())
							{
								double y;

								if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {digitalNumber.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								digitalNumber.DaytimeImage = string.Empty;
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {digitalNumber.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								digitalNumber.NighttimeImage = string.Empty;
							}
						}
						break;
					case "transparentcolor":
						if (value.Any())
						{
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalNumber.TransparentColor = transparentColor;
						}
						break;
					case "interval":
						if (value.Any())
						{
							int interval;

							if (!NumberFormats.TryParseIntVb6(value, out interval))
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
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
								double x;

								if (!NumberFormats.TryParseDoubleVb6(a, out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								digitalGauge.LocationX = x;
							}

							if (b.Any())
							{
								double y;

								if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							double radius;

							if (!NumberFormats.TryParseDoubleVb6(value, out radius))
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
							Color24 color;

							if (!Color24.TryParseHexColor(value, out color))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Color = color;
						}
						break;
					case "initialangle":
						if (value.Any())
						{
							double initialAngle;

							if (!NumberFormats.TryParseDoubleVb6(value, out initialAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.InitialAngle = initialAngle.ToRadians();
						}
						break;
					case "lastangle":
						if (value.Any())
						{
							double lastAngle;

							if (!NumberFormats.TryParseDoubleVb6(value, out lastAngle))
							{
								Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.LastAngle = lastAngle.ToRadians();
						}
						break;
					case "minimum":
						if (value.Any())
						{
							double minimum;

							if (!NumberFormats.TryParseDoubleVb6(value, out minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{
							double maximum;

							if (!NumberFormats.TryParseDoubleVb6(value, out maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Maximum = maximum;
						}
						break;
					case "step":
						if (value.Any())
						{
							double step;

							if (!NumberFormats.TryParseDoubleVb6(value, out step))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							digitalGauge.Step = step;
						}
						break;
					case "layer":
						if (value.Any())
						{
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
			string basePath = System.IO.Path.GetDirectoryName(fileName);

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
								double x;

								if (!NumberFormats.TryParseDoubleVb6(a, out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								linearGauge.LocationX = x;
							}

							if (b.Any())
							{
								double y;

								if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							double minimum;

							if (!NumberFormats.TryParseDoubleVb6(value, out minimum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Minimum = minimum;
						}
						break;
					case "maximum":
						if (value.Any())
						{
							double maximum;

							if (!NumberFormats.TryParseDoubleVb6(value, out maximum))
							{
								Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.Maximum = maximum;
						}
						break;
					case "width":
						if (value.Any())
						{
							int width;

							if (!NumberFormats.TryParseIntVb6(value, out width))
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
								int x, y;

								if (!NumberFormats.TryParseIntVb6(s[0], out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in LinearGauge Direction at line {lineNumber.ToString(culture)} in file {fileName}");
								}

								linearGauge.DirectionX = x;

								if (linearGauge.DirectionX < -1 || linearGauge.DirectionX > 1)
								{
									Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									linearGauge.DirectionX = 0;
								}

								if (!NumberFormats.TryParseIntVb6(s[1], out y))
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {linearGauge.DaytimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								linearGauge.DaytimeImage = string.Empty;
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
								Interface.AddMessage(MessageType.Error, true, $"FileName {linearGauge.NighttimeImage} could not be found in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								linearGauge.NighttimeImage = string.Empty;
							}
						}
						break;
					case "transparentcolor":
						if (value.Any())
						{
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							linearGauge.TransparentColor = transparentColor;
						}
						break;
					case "layer":
						if (value.Any())
						{
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
								double x;

								if (!NumberFormats.TryParseDoubleVb6(a, out x))
								{
									Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}

								timetable.LocationX = x;
							}

							if (b.Any())
							{
								double y;

								if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							double width;

							if (!NumberFormats.TryParseDoubleVb6(value, out width))
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
							double height;

							if (!NumberFormats.TryParseDoubleVb6(value, out height))
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
							Color24 transparentColor;

							if (!Color24.TryParseHexColor(value, out transparentColor))
							{
								Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							timetable.TransparentColor = transparentColor;
						}
						break;
					case "layer":
						if (value.Any())
						{
							int layer;

							if (!NumberFormats.TryParseIntVb6(value, out layer))
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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.LocationX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
									double x;

									if (!NumberFormats.TryParseDoubleVb6(a, out x))
									{
										Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									touch.SizeX = x;
								}

								if (b.Any())
								{
									double y;

									if (!NumberFormats.TryParseDoubleVb6(b, out y))
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
							int jumpScreen;

							if (!NumberFormats.TryParseIntVb6(value, out jumpScreen))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.JumpScreen = jumpScreen;
						}
						break;
					case "soundindex":
						if (value.Any())
						{
							int soundIndex;

							if (!NumberFormats.TryParseIntVb6(value, out soundIndex))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.SoundIndex = soundIndex;
						}
						break;
					case "command":
						{
							int i;

							for (i = 0; i < Translations.CommandInfos.Length; i++)
							{
								if (string.Compare(value, Translations.CommandInfos[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
								{
									break;
								}
							}

							if (i == Translations.CommandInfos.Length || Translations.CommandInfos[i].Type != Translations.CommandType.Digital)
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								touch.CommandInfo = Translations.CommandInfos[i];
							}
						}
						break;
					case "commandoption":
						if (value.Any())
						{
							int commandOption;

							if (!NumberFormats.TryParseIntVb6(value, out commandOption))
							{
								Interface.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}

							touch.CommandOption = commandOption;
						}
						break;
				}
			}
		}
	}
}
