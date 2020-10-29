using System;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenTK.Input;
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
												double pr = NumberFormats.ParseDouble(value, key, section, i, fileName);
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
													panel.This.Left = NumberFormats.ParseDouble(value, key, section, i, fileName);
												}
												break;
											case "right":
												if (value.Any())
												{
													panel.This.Right = NumberFormats.ParseDouble(value, key, section, i, fileName);
												}
												break;
											case "top":
												if (value.Any())
												{
													panel.This.Top = NumberFormats.ParseDouble(value, key, section, i, fileName);
												}
												break;
											case "bottom":
												if (value.Any())
												{
													panel.This.Bottom = NumberFormats.ParseDouble(value, key, section, i, fileName);
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
															panel.This.CenterX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															panel.This.CenterY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
															panel.This.OriginX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															panel.This.OriginY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
															pilotLamp.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															pilotLamp.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														pilotLamp.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
																needle.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
															}

															if (b.Any())
															{
																needle.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														needle.Radius = NumberFormats.ParseDouble(value, key, section, i, fileName);

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
																needle.OriginX = NumberFormats.ParseDouble(a, key, section, i, fileName);
															}

															if (b.Any())
															{
																needle.OriginY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														needle.InitialAngle = NumberFormats.ParseDouble(value, key, section, i, fileName).ToRadians();
													}
													break;
												case "lastangle":
													if (value.Any())
													{
														needle.LastAngle = NumberFormats.ParseDouble(value, key, section, i, fileName).ToRadians();
													}
													break;
												case "minimum":
													if (value.Any())
													{
														needle.Minimum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "maximum":
													if (value.Any())
													{
														needle.Maximum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "naturalfreq":
													if (value.Any())
													{
														needle.NaturalFreq = NumberFormats.ParseDouble(value, key, section, i, fileName);

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
														needle.DampingRatio = NumberFormats.ParseDouble(value, key, section, i, fileName);

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
														needle.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
															linearGauge.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															linearGauge.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														linearGauge.Minimum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "maximum":
													if (value.Any())
													{
														linearGauge.Maximum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "width":
													if (value.Any())
													{
														linearGauge.Width = NumberFormats.ParseInt(value, key, section, i, fileName);
													}
													break;
												case "direction":
													{
														string[] s = value.Split(',');

														if (s.Length == 2)
														{
															linearGauge.DirectionX = NumberFormats.ParseInt(s[0], key, section, i, fileName);

															if (linearGauge.DirectionX < -1 || linearGauge.DirectionX > 1)
															{
																Interface.AddMessage(MessageType.Error, false, $"Value is expected to be -1, 0 or 1  in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
																linearGauge.DirectionX = 0;
															}

															linearGauge.DirectionY = NumberFormats.ParseInt(s[1], key, section, i, fileName);

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
														linearGauge.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
															digitalNumber.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															digitalNumber.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														digitalNumber.Interval = NumberFormats.ParseInt(value, key, section, i, fileName);

														if (digitalNumber.Interval <= 0)
														{
															Interface.AddMessage(MessageType.Error, false, $"Height is expected to be non-negative in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "layer":
													if (value.Any())
													{
														digitalNumber.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
															digitalGauge.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															digitalGauge.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														digitalGauge.Radius = NumberFormats.ParseDouble(value, key, section, i, fileName);

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
														digitalGauge.InitialAngle = NumberFormats.ParseDouble(value, key, section, i, fileName).ToRadians();
													}
													break;
												case "lastangle":
													if (value.Any())
													{
														digitalGauge.LastAngle = NumberFormats.ParseDouble(value, key, section, i, fileName).ToRadians();
													}
													break;
												case "minimum":
													if (value.Any())
													{
														digitalGauge.Minimum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "maximum":
													if (value.Any())
													{
														digitalGauge.Maximum = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "step":
													if (value.Any())
													{
														digitalGauge.Step = NumberFormats.ParseDouble(value, key, section, i, fileName);
													}
													break;
												case "layer":
													if (value.Any())
													{
														digitalGauge.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
															timetable.LocationX = NumberFormats.ParseDouble(a, key, section, i, fileName);
														}

														if (b.Any())
														{
															timetable.LocationY = NumberFormats.ParseDouble(b, key, section, i, fileName);
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
														timetable.Width = NumberFormats.ParseDouble(value, key, section, i, fileName);

														if (timetable.Width <= 0.0)
														{
															Interface.AddMessage(MessageType.Error, false, $"ValueInPixels is required to be positive in {key} in {section} at line {(i + 1).ToString(culture)} in {fileName}");
														}
													}
													break;
												case "height":
													if (value.Any())
													{
														timetable.Height = NumberFormats.ParseDouble(value, key, section, i, fileName);

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
														timetable.Layer = NumberFormats.ParseInt(value, key, section, i, fileName);
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
