using System;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Panels;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Panels.Bve4
{
	internal static partial class PanelCfgBve4
	{
		internal static void Parse(string fileName, out Panel panel)
		{
			panel = new Panel();

			CultureInfo culture = CultureInfo.InvariantCulture;
			string[] lines = File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName));
			string basePath = System.IO.Path.GetDirectoryName(fileName);

			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
				int j = lines[i].IndexOf(';');

				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).TrimEnd();
				}
			}

			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Any())
				{
					if (lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal))
					{
						string section = lines[i].Substring(1, lines[i].Length - 2).Trim();

						switch (section.ToLowerInvariant())
						{
							case "this":
								i++;

								while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = lines[i].IndexOf('='); if (j >= 0)
									{
										string key = lines[i].Substring(0, j).TrimEnd();
										string value = lines[i].Substring(j + 1).TrimStart();

										switch (key.ToLowerInvariant())
										{
											case "resolution":
												double pr = 0.0;

												if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out pr))
												{
													Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
												}

												if (pr > 100)
												{
													panel.This.Resolution = pr;
												}
												else
												{
													//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
													//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
													Interface.AddMessage(MessageType.Error, false, $"A panel resolution of less than 100px was given at line {(i + 1).ToString(culture)} in {fileName}");
												}
												break;
											case "left":
												if (value.Any())
												{
													double left;

													if (!NumberFormats.TryParseDoubleVb6(value, out left))
													{
														Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line{(i + 1).ToString(culture)} in {fileName}");
													}

													panel.This.Left = left;
												}
												break;
											case "right":
												if (value.Any())
												{
													double right;

													if (!NumberFormats.TryParseDoubleVb6(value, out right))
													{
														Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}

													panel.This.Right = right;
												}
												break;
											case "top":
												if (value.Any())
												{
													double top;

													if (!NumberFormats.TryParseDoubleVb6(value, out top))
													{
														Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}

													panel.This.Top = top;
												}
												break;
											case "bottom":
												if (value.Any())
												{
													double bottom;

													if (!NumberFormats.TryParseDoubleVb6(value, out bottom))
													{
														Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}

													panel.This.Bottom = bottom;
												}
												break;
											case "daytimeimage":
												if (!System.IO.Path.HasExtension(value))
												{
													value += ".bmp";
												}

												if (Path.ContainsInvalidChars(value))
												{
													Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
												}
												else
												{
													panel.This.DaytimeImage = Path.CombineFile(basePath, value);

													if (!File.Exists(panel.This.DaytimeImage))
													{
														Interface.AddMessage(MessageType.Warning, true, $"FileName {panel.This.DaytimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
													Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
												}
												else
												{
													panel.This.NighttimeImage = Path.CombineFile(basePath, value);

													if (!File.Exists(panel.This.NighttimeImage))
													{
														Interface.AddMessage(MessageType.Warning, true, $"FileName {panel.This.NighttimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
												}
												break;
											case "transparentcolor":
												if (value.Any())
												{
													Color24 transparentColor;

													if (!Color24.TryParseHexColor(value, out transparentColor))
													{
														Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}

													panel.This.TransparentColor = transparentColor;
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
																Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															panel.This.CenterX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															panel.This.CenterY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															panel.This.OriginX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															panel.This.OriginY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												}
										}
									}

									i++;
								}

								i--;
								break;
							case "pilotlamp":
								{
									PilotLampElement pilotLamp = new PilotLampElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
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
																Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															pilotLamp.LocationX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															pilotLamp.LocationY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(value))
													{
														value += ".bmp";
													}

													if (Path.ContainsInvalidChars(value))
													{
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														pilotLamp.DaytimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(pilotLamp.DaytimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {pilotLamp.DaytimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														pilotLamp.NighttimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(pilotLamp.NighttimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {pilotLamp.NighttimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "transparentcolor":
													if (value.Any())
													{
														Color24 transparentColor;

														if (!Color24.TryParseHexColor(value, out transparentColor))
														{
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														pilotLamp.Layer = layer;
													}
													break;
											}
										}

										i++;
									}

									i--;
									panel.PanelElements.Add(pilotLamp);
								}
								break;
							case "needle":
								{
									NeedleElement needle = new NeedleElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
											{
												case "subject":
													needle.Subject = Subject.StringToSubject(value, $"{section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																	Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																}

																needle.LocationX = x;
															}

															if (b.Any())
															{
																double y;

																if (!NumberFormats.TryParseDoubleVb6(b, out y))
																{
																	Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																}

																needle.LocationY = y;
															}
														}
														else
														{
															Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "radius":
													if (value.Any())
													{
														double radius;

														if (!NumberFormats.TryParseDoubleVb6(value, out radius))
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														needle.Radius = radius;

														if (needle.Radius == 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is expected to be non-zero in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														needle.DaytimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(needle.DaytimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {needle.DaytimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														needle.NighttimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(needle.NighttimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, "FileName " + needle.NighttimeImage + " could not be found in " + key + " in " + section + " at line " + (i + 1).ToString(culture) + " in " + fileName);
														}
													}
													break;
												case "color":
													if (value.Any())
													{
														Color24 color;

														if (!Color24.TryParseHexColor(value, out color))
														{
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																	Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																}

																needle.OriginX = x;
															}

															if (b.Any())
															{
																double y;

																if (!NumberFormats.TryParseDoubleVb6(b, out y))
																{
																	Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																	needle.OriginX = -needle.OriginX;
																}

																needle.OriginY = y;
															}

															needle.DefinedOrigin = true;
														}
														else
														{
															Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "initialangle":
													if (value.Any())
													{
														double initialAngle;

														if (!NumberFormats.TryParseDoubleVb6(value, out initialAngle))
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														needle.NaturalFreq = naturalFreq;

														if (needle.NaturalFreq < 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"Value is expected to be non-negative in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														needle.DampingRatio = dampingRatio;

														if (needle.DampingRatio < 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"Value is expected to be non-negative in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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

										i++;
									}

									i--;
									panel.PanelElements.Add(needle);
								}
								break;
							case "lineargauge":
								{
									LinearGaugeElement linearGauge = new LinearGaugeElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
											{
												case "subject":
													linearGauge.Subject = Subject.StringToSubject(value, $"{section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															linearGauge.LocationX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															linearGauge.LocationY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												case "minimum":
													if (value.Any())
													{
														double minimum;

														if (!NumberFormats.TryParseDoubleVb6(value, out minimum))
														{
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																Interface.AddMessage(MessageType.Error, false, $"X is invalid in LinearGauge Direction at line {(i + 1).ToString(culture)} in file {fileName}");
															}

															linearGauge.DirectionX = x;

															if (linearGauge.DirectionX < -1 || linearGauge.DirectionX > 1)
															{
																Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																linearGauge.DirectionX = 0;
															}

															if (!NumberFormats.TryParseIntVb6(s[1], out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Y is invalid in  LinearGauge Direction at line {(i + 1).ToString(culture)} in file {fileName}");
															}

															linearGauge.DirectionY = y;

															if (linearGauge.DirectionY < -1 || linearGauge.DirectionY > 1)
															{
																Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																linearGauge.DirectionY = 0;
															}
														}
														else
														{
															Interface.AddMessage(MessageType.Error, false, $"Exactly 2 arguments are expected in LinearGauge Direction at line {(i + 1).ToString(culture)} in file {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														linearGauge.DaytimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(linearGauge.DaytimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {linearGauge.DaytimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														linearGauge.NighttimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(linearGauge.NighttimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {linearGauge.NighttimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "transparentcolor":
													if (value.Any())
													{
														Color24 transparentColor;

														if (!Color24.TryParseHexColor(value, out transparentColor))
														{
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														linearGauge.Layer = layer;
													}
													break;
											}
										}

										i++;
									}

									i--;
									panel.PanelElements.Add(linearGauge);
								}
								break;
							case "digitalnumber":
								{
									DigitalNumberElement digitalNumber = new DigitalNumberElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
											{
												case "subject":
													digitalNumber.Subject = Subject.StringToSubject(value, $"{section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																Interface.AddMessage(MessageType.Error, false, $"Left is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															digitalNumber.LocationX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Top is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															digitalNumber.LocationY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(value))
													{
														value += ".bmp";
													}

													if (Path.ContainsInvalidChars(value))
													{
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + key + " in " + section + " at line " + (i + 1).ToString(culture) + " in " + fileName);
													}
													else
													{
														digitalNumber.DaytimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(digitalNumber.DaytimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {digitalNumber.DaytimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
														Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													else
													{
														digitalNumber.NighttimeImage = Path.CombineFile(basePath, value);

														if (!File.Exists(digitalNumber.NighttimeImage))
														{
															Interface.AddMessage(MessageType.Warning, true, $"FileName {digitalNumber.NighttimeImage} could not be found in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "transparentcolor":
													if (value.Any())
													{
														Color24 transparentColor;

														if (!Color24.TryParseHexColor(value, out transparentColor))
														{
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Height is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														digitalNumber.Interval = interval;

														if (digitalNumber.Interval <= 0)
														{
															Interface.AddMessage(MessageType.Error, false, $"Height is expected to be non-negative in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "layer":
													if (value.Any())
													{
														int layer;

														if (!NumberFormats.TryParseIntVb6(value, out layer))
														{
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														digitalNumber.Layer = layer;
													}
													break;
											}
										}

										i++;
									}

									i--;
									panel.PanelElements.Add(digitalNumber);
								}
								break;
							case "digitalgauge":
								{
									DigitalGaugeElement digitalGauge = new DigitalGaugeElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
											{
												case "subject":
													digitalGauge.Subject = Subject.StringToSubject(value, $"{section} at line {(i + 1).ToString(culture)} in {fileName}");
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
																Interface.AddMessage(MessageType.Error, false, $"CenterX is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															digitalGauge.LocationX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"CenterY is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															digitalGauge.LocationY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												case "radius":
													if (value.Any())
													{
														double radius;

														if (!NumberFormats.TryParseDoubleVb6(value, out radius))
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														digitalGauge.Radius = radius;

														if (digitalGauge.Radius == 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is expected to be non-zero in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"ValueInDegrees is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														digitalGauge.Layer = layer;
													}
													break;
											}
										}

										i++;
									}

									i--;
									panel.PanelElements.Add(digitalGauge);
								}
								break;
							case "timetable":
								{
									TimetableElement timetable = new TimetableElement();
									i++;

									while (i < lines.Length && !(lines[i].StartsWith("[", StringComparison.Ordinal) & lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = lines[i].IndexOf('=');

										if (j >= 0)
										{
											string key = lines[i].Substring(0, j).TrimEnd();
											string value = lines[i].Substring(j + 1).TrimStart();

											switch (key.ToLowerInvariant())
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
																Interface.AddMessage(MessageType.Error, false, $"X is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															timetable.LocationX = x;
														}

														if (b.Any())
														{
															double y;

															if (!NumberFormats.TryParseDoubleVb6(b, out y))
															{
																Interface.AddMessage(MessageType.Error, false, $"Y is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
															}

															timetable.LocationY = y;
														}
													}
													else
													{
														Interface.AddMessage(MessageType.Error, false, $"Two arguments are expected in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
													}
													break;
												case "width":
													if (value.Any())
													{
														double width;

														if (!NumberFormats.TryParseDoubleVb6(value, out width))
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														timetable.Width = width;

														if (timetable.Width <= 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is required to be positive in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "height":
													if (value.Any())
													{
														double height;

														if (!NumberFormats.TryParseDoubleVb6(value, out height))
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														timetable.Height = height;

														if (timetable.Height <= 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is required to be positive in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "transparentcolor":
													if (value.Any())
													{
														Color24 transparentColor;

														if (!Color24.TryParseHexColor(value, out transparentColor))
														{
															Interface.AddMessage(MessageType.Error, false, $"HexColor is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
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
															Interface.AddMessage(MessageType.Error, false, $"LayerIndex is invalid in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}

														timetable.Layer = layer;
													}
													break;
											}
										}

										i++;
									}

									i--;
									panel.PanelElements.Add(timetable);
								}
								break;
						}
					}
				}
			}
		}
	}
}
