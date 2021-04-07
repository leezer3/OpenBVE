using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using Train.OpenBve.Panel2Cfg;
using TrainManager.Car;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	internal class Panel2CfgParser
	{
		internal readonly Plugin Plugin;

		internal Panel2CfgParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		

		/// <summary>Parses a BVE2 / openBVE panel.cfg file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Car">The car to add the panel to</param>
		internal void ParsePanel2Config(string PanelFile, string TrainPath, CarBase Car)
		{
			Encoding Encoding = TextEncoding.GetSystemEncodingFromFile(PanelFile);
			//Train name, used for hacks detection
			string trainName = new DirectoryInfo(TrainPath).Name.ToUpperInvariant();
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = Path.CombineFile(TrainPath, PanelFile);
			string[] Lines = File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).TrimEnd();
				}
			}
			// initialize
			
			string PanelDaytimeImage = null;
			string PanelNighttimeImage = null;
			Color24 PanelTransparentColor = Color24.Blue;
			// parse lines for panel
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						if (!Enum.TryParse(Section, true, out Section section))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised section " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
						}
						switch (section) {
							case Panel2Cfg.Section.This:
								i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									int j = Lines[i].IndexOf('='); if (j >= 0)
									{
										string key = Lines[i].Substring(0, j).TrimEnd();
										if (!Enum.TryParse(key, true, out Key Key))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key) {
											case Key.Resolution:
												double pr = 0.0;
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out pr)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												if (pr > 100)
												{
													Panel.Resolution = pr;
												}
												else
												{
													//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
													//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
													Plugin.currentHost.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												break;
											case Key.Left:
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Panel.TopLeft.X)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line" + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case Key.Right:
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Panel.BottomRight.X)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}

												if (Plugin.CurrentOptions.EnableBveTsHacks)
												{
													switch ((int)Panel.BottomRight.X)
													{
														case 1696:
															if (Panel.Resolution == 1024 && trainName == "TOQ2000CN1EXP10" || trainName == "TOQ8500CS8EXP10")
															{
																Panel.BottomRight.X = 1024;
															}
															break;
													}
												}
												break;
											case Key.Top:
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Panel.TopLeft.Y)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case Key.Bottom:
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Panel.BottomRight.Y)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case Key.DaytimeImage:
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelDaytimeImage = Path.CombineFile(TrainPath, Value);
													if (!File.Exists(PanelDaytimeImage)) {
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + PanelDaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelDaytimeImage = null;
													}
												}
												break;
											case Key.NighttimeImage:
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelNighttimeImage = Path.CombineFile(TrainPath, Value);
													if (!File.Exists(PanelNighttimeImage)) {
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + PanelNighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelNighttimeImage = null;
													}
												}
												break;
											case Key.TransparentColor:
												if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out PanelTransparentColor)) {
													Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case Key.Center:
												{
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Panel.Center.X)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Panel.Center.Y)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (Plugin.CurrentOptions.EnableBveTsHacks)
														{
															switch ((int)Panel.Center.Y)
															{
																case 180:
																	switch (trainName.ToUpperInvariant())
																	{
																		case "LT_C69_77":
																		case "LT_C69_77_V2":
																			// Broken initial zoom
																			Panel.Center.Y = 350;
																			break;
																	}
																	break;
																case 200:
																	switch (trainName.ToUpperInvariant())
																	{
																		case "HM05":
																			// Broken initial zoom
																			Panel.Center.Y = 350;
																			break;
																	}
																	break;
																case 229:
																	if (Panel.BottomRight.Y == 768 && Panel.Resolution == 1024)
																	{
																		// Martin Finken's BVE4 trams: Broken initial zoom
																		Panel.Center.Y = 350;
																	}
																	break;
																case 255:
																	if (Panel.BottomRight.Y == 1024 && Panel.Resolution == 1024)
																	{
																		switch (trainName.ToUpperInvariant())
																		{
																			case "PARIS_MF67":
																			case "PARIS_MF88":
																			case "PARIS_MP73":
																			case "PARIS_MP89":
																			case "PARIS_MP89AUTO":
																			case "LT1938":
																			case "LT1973 UNREFURB":
																				// Broken initial zoom
																				Panel.Center.Y = 350;
																				break;
																			case "LT_A60_62":
																			case "LT1972 MKII":
																				// Broken initial zoom and black patch at bottom of panel
																				Panel.Center.Y = 350;
																				Panel.BottomRight.Y = 792;
																				break;
																		}
																	}
																	break;
															}
															
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												}
											case Key.Origin:
												{
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Panel.Origin.X)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Panel.Origin.Y)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (Plugin.CurrentOptions.EnableBveTsHacks)
														{
															switch (trainName)
															{
																case "8171BETA":
																	if (Panel.Resolution == 768 && Panel.Origin.Y == 256)
																	{
																		// 81-71: Bust panel origin means a flying cab....
																		Panel.Origin.Y = 0;
																	}
																	break;
															}
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												}
										}
									} i++;
								} i--; break;
						}
					}
				}
			}
			{ // camera restriction
				double WorldWidth, WorldHeight;
				if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height) {
					WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * Panel.EyeDistance;
					WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
				} else {
					WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * Panel.EyeDistance / Plugin.Renderer.Screen.AspectRatio;
					WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
				}
				double x0 = (Panel.TopLeft.X - Panel.Center.X) / Panel.Resolution;
				double x1 = (Panel.BottomRight.X - Panel.Center.X) / Panel.Resolution;
				double y0 = (Panel.Center.Y - Panel.BottomRight.Y) / Panel.Resolution * Plugin.Renderer.Screen.AspectRatio;
				double y1 = (Panel.Center.Y - Panel.TopLeft.Y) / Panel.Resolution * Plugin.Renderer.Screen.AspectRatio;
				Car.CameraRestriction.BottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, Panel.EyeDistance);
				Car.CameraRestriction.TopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, Panel.EyeDistance);
				Car.DriverYaw = Math.Atan((Panel.Center.X - Panel.Origin.X) * WorldWidth / Panel.Resolution);
				Car.DriverPitch = Math.Atan((Panel.Origin.Y - Panel.Center.Y) * WorldWidth / Panel.Resolution);
			}
			// create panel
			if (PanelDaytimeImage != null) {
				if (!File.Exists(PanelDaytimeImage)) {
					Plugin.currentHost.AddMessage(MessageType.Error, true, "The daytime panel bitmap could not be found in " + FileName);
				} else {
					Texture tday;
					Plugin.currentHost.RegisterTexture(PanelDaytimeImage, new TextureParameters(null, PanelTransparentColor), out tday, true);
					Texture tnight = null;
					if (PanelNighttimeImage != null) {
						if (!File.Exists(PanelNighttimeImage)) {
							Plugin.currentHost.AddMessage(MessageType.Error, true, "The nighttime panel bitmap could not be found in " + FileName);
						} else {
							Plugin.currentHost.RegisterTexture(PanelNighttimeImage, new TextureParameters(null, PanelTransparentColor), out tnight, true);
						}
					}
					Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[0], 0.0, 0.0, new Vector2(0.5, 0.5), 0.0, Car.Driver, tday, tnight);
				}
			}

			int GroupIndex = 0;

			if (Plugin.CurrentOptions.Panel2ExtendedMode)
			{
				GroupIndex++;
				Array.Resize(ref Car.CarSections[0].Groups, GroupIndex + 1);
				Car.CarSections[0].Groups[GroupIndex] = new ElementsGroup(ObjectType.Overlay);
			}

			// parse lines for rest
			double invfac = Lines.Length == 0 ? 1.0 : 1.0 / Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Plugin.CurrentProgress = Plugin.CurrentProgress + invfac * i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						if (!Enum.TryParse(Section, true, out Section section))
						{
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised section " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
						}
						switch (section) {
								// pilotlamp
							case Panel2Cfg.Section.PilotLamp:
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = Color24.Blue;
									int Layer = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Subject:
													Subject = Value;
													break;
												case Key.Location:
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.DaytimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(DaytimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case Key.NighttimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(NighttimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Texture tday;
										Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out tday, true);
										Texture tnight = null;
										if (NighttimeImage != null) {
											Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight, true);
										}
										int w = tday.Width;
										int h = tday.Height;
										int j = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), Layer, Car.Driver, tday, tnight, Color32.White);
										string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, Section + " in " + FileName);
										try
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f + " 1 == --", false);
										}
										catch
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}
									}
								} break;
								// needle
							case Panel2Cfg.Section.Needle:
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color32 Color = Color32.White;
									Color24 TransparentColor = Color24.Blue;
									double OriginX = -1.0, OriginY = -1.0;
									bool OriginDefined = false;
									double Layer = 0.0, Radius = 0.0;
									double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
									double Minimum = 0.0, Maximum = 1000.0;
									double NaturalFrequency = -1.0, DampingRatio = -1.0;
									bool Backstop = false, Smoothed = false;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Subject:
													Subject = Value;
													break;
												case Key.Location:
													{
														int k = Value.IndexOf(',');
														if (k >= 0)
														{
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
																Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
																Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
														} else {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case Key.Radius:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case Key.DaytimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(DaytimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case Key.NighttimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(NighttimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case Key.Color:
													if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Origin:
													{
														int k = Value.IndexOf(',');
														if (k >= 0)
														{
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out OriginX)) {
																Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out OriginY)) {
																Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																OriginX = -OriginX;
															}
															OriginDefined = true;
														} else {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case Key.InitialAngle:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.LastAngle:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Minimum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Maximum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.NaturalFreq:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out NaturalFrequency)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (NaturalFrequency < 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														NaturalFrequency = -NaturalFrequency;
													} break;
												case Key.DampingRatio:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out DampingRatio)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (DampingRatio < 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														DampingRatio = -DampingRatio;
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Backstop:
													if (Value.Length != 0 && Value.ToLowerInvariant() == "true" || Value == "1")
													{
														Backstop = true;
													}
													break;
												case Key.Smoothed:
													if (Value.Length != 0 && Value.ToLowerInvariant() == "true" || Value == "1")
													{
														Smoothed = true;
													}
													break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null)
									{
										Texture tday;
										Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out tday, true);
										Texture tnight = null;
										if (NighttimeImage != null)
										{
											Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight, true);
										}
										if (!OriginDefined) {
											OriginX = 0.5 * tday.Width;
											OriginY = 0.5 * tday.Height;
										}
										double ox = OriginX / tday.Width;
										double oy = OriginY / tday.Height;
										double n = Radius == 0.0 | OriginY == 0.0 ? 1.0 : Radius / OriginY;
										double nx = n * tday.Width;
										double ny = n * tday.Height;
										int j = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX - ox * nx, LocationY - oy * ny, nx, ny, new Vector2(ox, oy), Layer, Car.Driver, tday, tnight, Color);
										Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZDirection = Vector3.Backward;
										Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateXDirection = Vector3.Right;
										Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZDirection, Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateXDirection);
										string f;
										switch (Subject.ToLowerInvariant()) {
											case "hour":
												f = Smoothed ? "0.000277777777777778 time * 24 mod" : "0.000277777777777778 time * floor";
												break;
											case "min":
												f = Smoothed ? "0.0166666666666667 time * 60 mod" : "0.0166666666666667 time * floor";
												break;
											case "sec":
												f = Smoothed ? "time 60 mod" : "time floor";
												break;
											default:
												f = GetStackLanguageFromSubject(Car.baseTrain, Subject, Section + " in " + FileName);
												break;
										}

										InitialAngle = InitialAngle.ToRadians();
										LastAngle = LastAngle.ToRadians();
										double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
										double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
										f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
										if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0) {
											Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZDamping = new Damping(NaturalFrequency, DampingRatio);
										}
										try
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, f, false);
										}
										catch
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}
										if (Backstop)
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZFunction.Minimum = InitialAngle;
											Car.CarSections[0].Groups[GroupIndex].Elements[j].RotateZFunction.Maximum = LastAngle;
										}
									}
								} break;
							case Panel2Cfg.Section.LinearGauge:
									{
									string Subject = "true";
									int Width = 0;
									Vector2 Direction = new Vector2(1,0);
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									double Minimum = 0.0, Maximum = 0.0;
									Color24 TransparentColor = Color24.Blue;
									int Layer = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Subject:
													Subject = Value;
													break;
												case Key.Location:
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Minimum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Maximum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Width:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Width))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
												case Key.Direction:
													{
														string[] s = Value.Split( ',');
														if (s.Length == 2)
														{
															double x, y;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in  LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																Direction = new Vector2(x, y);
															}
														}
														else
														{
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
													break;
												case Key.DaytimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(DaytimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case Key.NighttimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(NighttimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Texture tday;
										Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out tday, true);
										Texture tnight = null;
										if (NighttimeImage != null) {
											Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight, true);
										}
										int j = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX, LocationY, tday.Width, tday.Height, new Vector2(0.5, 0.5), Layer, Car.Driver, tday, tnight, Color32.White);
										if (Maximum < Minimum)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Maximum value must be greater than minimum value " + Section + " in " + FileName);
											break;
										}
										string tf = GetInfixFunction(Car.baseTrain, Subject, Minimum, Maximum, Width, tday.Width, Section + " in " + FileName);
										if (tf != String.Empty)
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].TextureShiftXDirection = Direction;
											try
											{
												Car.CarSections[0].Groups[GroupIndex].Elements[j].TextureShiftXFunction = new FunctionScript(Plugin.currentHost, tf, false);
											}
											catch
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
										}
									}
								} break;
								// digitalnumber
							case Panel2Cfg.Section.DigitalNumber:
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = Color24.Blue;
									double Layer = 0.0; int Interval = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Subject:
													Subject = Value;
													break;
												case Key.Location:
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.DaytimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(DaytimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case Key.NighttimeImage:
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!File.Exists(NighttimeImage)) {
															Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Interval:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Interval)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Interval <= 0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									if (Interval <= 0) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Interval is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null & Interval > 0) {
										int wday, hday;
										Plugin.currentHost.QueryTextureDimensions(DaytimeImage, out wday, out hday);
										if (wday > 0 & hday > 0) {
											int numFrames = hday / Interval;
											if (Plugin.CurrentOptions.EnableBveTsHacks)
											{
												/*
												 * With hacks enabled, the final frame does not necessarily need to be
												 * completely within the confines of the texture
												 * e.g. LT_C69_77
												 * https://github.com/leezer3/OpenBVE/issues/247
												 */
												switch (Subject)
												{
													case "power":
														if (Car.baseTrain.Handles.Power.MaximumNotch > numFrames)
														{
															numFrames = Car.baseTrain.Handles.Power.MaximumNotch;
														}
														break;
													case "brake":
														int b = Car.baseTrain.Handles.Brake.MaximumNotch + 2;
														if (Car.baseTrain.Handles.HasHoldBrake)
														{
															b++;
														}
														if (b > numFrames )
														{
															numFrames = b;
														}
														break;
												}
											}
											Texture[] tday = new Texture[numFrames];
											Texture[] tnight;
											for (int k = 0; k < numFrames; k++)
											{
												if ((k + 1) * Interval <= hday)
												{
													Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, Interval), TransparentColor), out tday[k], true);
												}
												else if (k * Interval >= hday)
												{
													numFrames = k;
													Array.Resize(ref tday, k);
												}
												else
												{
													Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, hday - (k * Interval)), TransparentColor), out tday[k], true);
												}
											}
											if (NighttimeImage != null) {
												int wnight, hnight;
												Plugin.currentHost.QueryTextureDimensions(NighttimeImage, out wnight, out hnight);
												tnight = new Texture[numFrames];
												for (int k = 0; k < numFrames; k++) {
													if ((k + 1) * Interval <= hnight)
													{
														Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, Interval), TransparentColor), out tnight[k], true);
													}
													else if (k * Interval > hnight)
													{
														tnight[k] = null;
													}
													else
													{
														Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, hnight - (k * Interval)), TransparentColor), out tnight[k], true);
													}
												}
												
											} else {
												tnight = new Texture[numFrames];
												for (int k = 0; k < numFrames; k++) {
													tnight[k] = null;
												}
											}
											int j = -1;
											for (int k = 0; k < tday.Length; k++) {
												int l = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX, LocationY, wday, Interval, new Vector2(0.5, 0.5), Layer, Car.Driver, tday[k], tnight[k], Color32.White, k != 0);
												if (k == 0) j = l;
											}
											string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, Section + " in " + FileName);
											try
											{
												Car.CarSections[0].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f, false);
											}
											catch
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}

											if (Plugin.CurrentOptions.Panel2ExtendedMode)
											{
												if (wday >= Plugin.CurrentOptions.Panel2ExtendedMinSize && Interval >= Plugin.CurrentOptions.Panel2ExtendedMinSize)
												{
													if (Subject == "power")
													{
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.PowerDecrease } }, new Vector2(0.5, 0.5), 0, Car.Driver);
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.PowerIncrease } }, new Vector2(0.5, 0.5), 0, Car.Driver);
													}

													if (Subject == "brake")
													{
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.BrakeIncrease } }, new Vector2(0.5, 0.5), 0, Car.Driver);
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.BrakeDecrease } }, new Vector2(0.5, 0.5), 0, Car.Driver);
													}

													if (Subject == "reverser")
													{
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.ReverserForward } }, new Vector2(0.5, 0.5), 0, Car.Driver);
														Plugin.Panel.CreateTouchElement(Car.CarSections[0].Groups[GroupIndex], new Vector2(LocationX, LocationY + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.ReverserBackward } }, new Vector2(0.5, 0.5), 0, Car.Driver);
													}
												}
											}
										}
									}
								} break;
								// digitalgauge
							case Panel2Cfg.Section.DigitalGauge:
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									Color32 Color = Color32.Black;
									double Radius = 0.0;
									int Layer = 0;
									double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
									double Minimum = 0.0, Maximum = 1000.0;
									double Step = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Subject:
													Subject = Value;
													break;
												case Key.Location:
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Radius:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case Key.Color:
													if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.InitialAngle:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														InitialAngle = InitialAngle.ToRadians();
													} break;
												case Key.LastAngle:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														LastAngle = LastAngle.ToRadians();
													} break;
												case Key.Minimum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Maximum:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Step:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Step)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;

									if (Plugin.CurrentOptions.EnableBveTsHacks && trainName == "BOEING-737")
									{
										/*
										 * BVE4 stacks objects within layers in order
										 * If two overlapping objects are declared in the same
										 * layer in openBVE, this causes Z-fighting
										 *
										 */
										if (Subject == "sap" || Subject == "bp")
										{
											Layer = 4;
										}
									}

									if (Radius == 0.0) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Radius is required to be non-zero in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Minimum == Maximum) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										Radius = 0.0;
									}
									if (Math.Abs(InitialAngle - LastAngle) > 6.28318531) {
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Radius != 0.0) {
										// create element
										int j = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX - Radius, LocationY - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), Layer, Car.Driver, null, null, Color);
										InitialAngle = InitialAngle + Math.PI;
										LastAngle = LastAngle + Math.PI;
										double x0 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
										double y0 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
										double z0 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
										double x1 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
										double y1 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
										double z1 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
										double x2 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
										double y2 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
										double z2 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
										double x3 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
										double y3 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
										double z3 = Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
										double cx = 0.25 * (x0 + x1 + x2 + x3);
										double cy = 0.25 * (y0 + y1 + y2 + y3);
										double cz = 0.25 * (z0 + z1 + z2 + z3);
										VertexTemplate[] vertices = new VertexTemplate[11];
										for (int v = 0; v < 11; v++)
										{
											vertices[v] = new Vertex();
										}
										int[][] faces = {
											new[] { 0, 1, 2 },
											new[] { 0, 3, 4 },
											new[] { 0, 5, 6 },
											new[] { 0, 7, 8 },
											new[] { 0, 9, 10 }
										};
										Car.CarSections[0].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, Color);
										Car.CarSections[0].Groups[GroupIndex].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
										Car.CarSections[0].Groups[GroupIndex].Elements[j].LEDInitialAngle = InitialAngle;
										Car.CarSections[0].Groups[GroupIndex].Elements[j].LEDLastAngle = LastAngle;
										Car.CarSections[0].Groups[GroupIndex].Elements[j].LEDVectors = new[] {
											new Vector3(x0, y0, z0),
											new Vector3(x1, y1, z1),
											new Vector3(x2, y2, z2),
											new Vector3(x3, y3, z3),
											new Vector3(cx, cy, cz)
										};
										string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, Section + " in " + FileName);
										double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
										double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
										if (Step == 1.0) {
											f += " floor";
										} else if (Step != 0.0) {
											string s = (1.0 / Step).ToString(Culture);
											string t = Step.ToString(Culture);
											f += " " + s + " * floor " + t + " *";
										}
										f += " " + a1.ToString(Culture) + " " + a0.ToString(Culture) + " fma";
										try
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].LEDFunction = new FunctionScript(Plugin.currentHost, f, false);
										}
										catch
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}
									} else {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Radius is required to be specified in " + Section + " in " + FileName);
									}
								} break;
								// timetable
							case Panel2Cfg.Section.Timetable:
								{
									double LocationX = 0.0, LocationY = 0.0;
									double Width = 0.0, Height = 0.0;
									//We read the transparent color for the timetable from the config file, but it is never used
									//TODO: Fix or depreciate??
									// ReSharper disable once NotAccessedVariable
									Color24 TransparentColor = Color24.Blue;
									double Layer = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key) {
												case Key.Location:
													int k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Width:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Width)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Width <= 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Height:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Height)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Height <= 0.0) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									// create element
									if (Width <= 0.0) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
									}
									if (Height <= 0.0) {
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
									}
									if (Width > 0.0 & Height > 0.0) {
										int j = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[GroupIndex], LocationX, LocationY, Width, Height, new Vector2(0.5, 0.5), Layer, Car.Driver, null, null, Color32.White);
										try
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, "panel2timetable", false);
										}
										catch
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}
										
										Plugin.currentHost.AddObjectForCustomTimeTable(Car.CarSections[0].Groups[GroupIndex].Elements[j]);
									}
								} break;
							case Panel2Cfg.Section.Windscreen:
								{
									i++;
									Vector2 topLeft = new Vector2(Panel.TopLeft);
									Vector2 bottomRight = new Vector2(Panel.BottomRight);
									int numberOfDrops = 16, Layer = 0, dropSize = 16;
									WiperPosition restPosition = WiperPosition.Left, holdPosition = WiperPosition.Left;
									List<string> daytimeDropFiles, nighttimeDropFiles, daytimeFlakeFiles, nighttimeFlakeFiles;
									Color24 TransparentColor = Color24.Blue;
									double wipeSpeed = 1.0, holdTime = 1.0, dropLife = 10.0;
									try
									{
										daytimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Day"), "drop*.png").ToList();
										daytimeFlakeFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Day"), "flake*.png").ToList();
										nighttimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Night"), "drop*.png").ToList();
										nighttimeFlakeFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Night"), "flake*.png").ToList();
									}
									catch
									{
										break;
									}

									while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
									{
										int j = Lines[i].IndexOf('=');
										if (j >= 0)
										{
											int k;
											string key = Lines[i].Substring(0, j).TrimEnd();
											if (!Enum.TryParse(key, true, out Key Key))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
											}
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key)
											{
												case Key.TopLeft:
													k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out topLeft.X))
														{
															Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}

														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out topLeft.Y))
														{
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													}
													else
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}

													break;
												case Key.BottomRight:
													k = Value.IndexOf(',');
													if (k >= 0)
													{
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out bottomRight.X))
														{
															Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}

														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out bottomRight.Y))
														{
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													}
													else
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}

													break;
												case Key.NumberOfDrops:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out numberOfDrops))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "NumberOfDrops is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}

													break;
												case Key.DropSize:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out dropSize))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "DropSize is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}

													break;
												case Key.DaytimeDrops:
													daytimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
													break;
												case Key.NighttimeDrops:
													nighttimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
													break;
												case Key.DaytimeFlakes:
													daytimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
													break;
												case Key.NighttimeFlakes:
													nighttimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
													break;
												case Key.TransparentColor:
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}

													break;
												case Key.Layer:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "NumberOfDrops is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
												case Key.WipeSpeed:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out wipeSpeed))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
												case Key.WiperHoldTime:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out holdTime))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
												case Key.WiperRestPosition:
													switch (Value.ToLowerInvariant())
													{
														case "0":
														case "left":
															restPosition = WiperPosition.Left;
															break;
														case "1":
														case "right":
															restPosition = WiperPosition.Right;
															break;
														default:
															Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															break;
													}
													break;
												case Key.WiperHoldPosition:
													switch (Value.ToLowerInvariant())
													{
														case "0":
														case "left":
															holdPosition = WiperPosition.Left;
															break;
														case "1":
														case "right":
															holdPosition = WiperPosition.Right;
															break;
														default:
															Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperHoldPosition is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															break;
													}
													break;
												case Key.DropLife:
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out dropLife))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "DropLife is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
											}
										}

										i++;
									}

									i--;
									/*
									 * Ensure we have the same number of drops for day + night
									 * NOTE: If a drop is missing, we may get slightly odd effects, but can't be helped
									 * Raindrops ought to be blurry, and they're small enough anyway...
									 */
									int MD = Math.Max(daytimeDropFiles.Count, nighttimeDropFiles.Count);
									MD = Math.Max(daytimeFlakeFiles.Count, MD);
									MD = Math.Max(nighttimeFlakeFiles.Count, MD);
									if (daytimeDropFiles.Count < MD)
									{
										while (daytimeDropFiles.Count < MD)
										{
											daytimeDropFiles.Add(string.Empty);
										}
									}

									if (daytimeFlakeFiles.Count < MD)
									{
										while (daytimeFlakeFiles.Count < MD)
										{
											daytimeFlakeFiles.Add(string.Empty);
										}
									}

									if (nighttimeDropFiles.Count < MD)
									{
										while (nighttimeDropFiles.Count < MD)
										{
											nighttimeDropFiles.Add(string.Empty);
										}
									}

									
									if (nighttimeFlakeFiles.Count < MD)
									{
										while (nighttimeFlakeFiles.Count < MD)
										{
											nighttimeFlakeFiles.Add(string.Empty);
										}
									}

									List<Texture> daytimeDrops = LoadDrops(TrainPath, daytimeDropFiles, TransparentColor, "drop");
									List<Texture> daytimeFlakes = LoadDrops(TrainPath, daytimeFlakeFiles, TransparentColor, "flake");
									List<Texture> nighttimeDrops = LoadDrops(TrainPath, nighttimeDropFiles, TransparentColor, "drop");
									List<Texture> nighttimeFlakes = LoadDrops(TrainPath, nighttimeFlakeFiles, TransparentColor, "flake");

									double dropInterval = (bottomRight.X - topLeft.X) / numberOfDrops;
									double currentDropX = topLeft.X;
									Car.Windscreen = new Windscreen(numberOfDrops, dropLife, Car);
									Car.Windscreen.Wipers = new WindscreenWiper(Car.Windscreen, restPosition, holdPosition, wipeSpeed, holdTime);
									// Create drops
									for (int drop = 0; drop < numberOfDrops; drop++)
									{
										int DropTexture = Plugin.RandomNumberGenerator.Next(daytimeDrops.Count);
										double currentDropY = Plugin.RandomNumberGenerator.NextDouble() * (bottomRight.Y - topLeft.Y) + topLeft.Y;
										//Create both a drop and a snowflake at the same position, the windscreen code will determine which is shown
										int panelDropIndex = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[0], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer,  Car.Driver, daytimeDrops[DropTexture], nighttimeDrops[DropTexture], Color32.White);
										int panelFlakeIndex = Plugin.Panel.CreateElement(ref Car.CarSections[0].Groups[0], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer, Car.Driver, daytimeFlakes[DropTexture], nighttimeFlakes[DropTexture], Color32.White);
										string f = drop + " raindrop";
										string f2 = drop + " snowflake";
										try
										{
											Car.CarSections[0].Groups[GroupIndex].Elements[panelDropIndex].StateFunction = new FunctionScript(Plugin.currentHost, f + " 1 == --", false);
											Car.CarSections[0].Groups[GroupIndex].Elements[panelFlakeIndex].StateFunction = new FunctionScript(Plugin.currentHost, f2 + " 1 == --", false);
										}
										catch
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										}

										currentDropX += dropInterval;
									}
								} break;
						}
					}
				}
			}
		}

		private List<Texture> LoadDrops(string TrainPath, List<string> dropFiles, Color24 TransparentColor, string compatabilityString)
		{
			List<Texture> drops = new List<Texture>();
			for (int l = 0; l < dropFiles.Count; l++)
			{
				string currentDropFile = !System.IO.Path.IsPathRooted(dropFiles[l]) ? Path.CombineFile(TrainPath, dropFiles[l]) : dropFiles[l];
				if (!File.Exists(currentDropFile))
				{
					currentDropFile = Path.CombineFile(Plugin.FileSystem.DataFolder, "Compatability\\Windscreen\\Day\\" + compatabilityString + Plugin.RandomNumberGenerator.Next(1, 4) + ".png");
					TransparentColor = Color24.Blue;
				}

				Texture drop;
				Plugin.currentHost.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out drop, true);
				drops.Add(drop);
			}

			return drops;
		}

		internal string GetInfixFunction(AbstractTrain Train, string Subject, double Minimum, double Maximum, int Width, int TextureWidth, string ErrorLocation)
		{
			double mp = 0.0;
			if (Minimum < 0)
			{
				mp = Math.Abs(Minimum);
			}
			double ftc = 1.0;
			if (Width != 0)
			{
				//If the width of the needle is not set, it will loop round to the starting position
				ftc -= (double)Width / TextureWidth;
			}
			double range = ftc / ((Maximum + mp) - (Minimum + mp));

			string subjectText = GetStackLanguageFromSubject(Train, Subject, ErrorLocation);

			if (!string.IsNullOrEmpty(subjectText))
			{
				return $"{subjectText} {Maximum} < {subjectText} {Minimum} > {subjectText} {Minimum + mp} - {range} * 0 ? {ftc} ?";
			}

			return string.Empty;
		}

		/// <summary>Converts a Panel2.cfg subject to an animation function stack</summary>
		/// <param name="Train">The train</param>
		/// <param name="Subject">The subject to convert</param>
		/// <param name="ErrorLocation">The location in the Panel2.cfg file</param>
		/// <returns>The parsed animation function stack</returns>
		internal string GetStackLanguageFromSubject(AbstractTrain Train, string Subject, string ErrorLocation) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Suffix = "";
			{
				// detect d# suffix
				int i;
				for (i = Subject.Length - 1; i >= 0; i--) {
					int a = char.ConvertToUtf32(Subject, i);
					if (a < 48 | a > 57) break;
				}
				if (i >= 0 & i < Subject.Length - 1) {
					if (Subject[i] == 'd' | Subject[i] == 'D') {
						int n;
						if (int.TryParse(Subject.Substring(i + 1), System.Globalization.NumberStyles.Integer, Culture, out n)) {
							if (n == 0) {
								Suffix = " floor 10 mod";
							} else {
								string t0 = Math.Pow(10.0, n).ToString(Culture);
								string t1 = Math.Pow(10.0, -n).ToString(Culture);
								Suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
							}
							Subject = Subject.Substring(0, i);
						}
					}
				}
			}
			// transform subject
			string Code;
			switch (Subject.ToLowerInvariant()) {
				case "acc":
					Code = "acceleration";
					break;
				case "motor":
					Code = "accelerationmotor";
					break;
				case "true":
					Code = "1";
					break;
				case "kmph":
					Code = "speedometer abs 3.6 *";
					break;
				case "mph":
					Code = "speedometer abs 2.2369362920544 *";
					break;
				case "ms":
					Code = "speedometer abs";
					break;
				case "locobrakecylinder":
					Code = Train.DriverCar + " brakecylinderindex 0.001 *";
					break;
				case "bc":
					Code = "brakecylinder 0.001 *";
					break;
				case "mr":
					Code = "mainreservoir 0.001 *";
					break;
				case "sap":
					Code = "straightairpipe 0.001 *";
					break;
				case "locobrakepipe":
					Code = Train.DriverCar + " brakepipeindex 0.001 *";
					break;
				case "bp":
					Code = "brakepipe 0.001 *";
					break;
				case "er":
					Code = "equalizingreservoir 0.001 *";
					break;
				case "door":
					Code = "1 doors -";
					break;
				case "csc":
					Code = "constSpeed";
					break;
				case "power":
					Code = "brakeNotchLinear 0 powerNotch ?";
					break;
				case "locobrake":
					Code = "locoBrakeNotch";
					break;
				case "brake":
					Code = "brakeNotchLinear";
					break;
				case "rev":
					Code = "reverserNotch ++";
					break;
				case "hour":
					Code = "0.000277777777777778 time * 24 mod floor";
					break;
				case "min":
					Code = "0.0166666666666667 time * 60 mod floor";
					break;
				case "sec":
					Code = "time 60 mod floor";
					break;
				case "atc":
					Code = "271 pluginstate";
					break;
				case "klaxon":
				case "horn":
					Code = "klaxon";
					break;
				case "primaryklaxon":
				case "primaryhorn":
					Code = "primaryklaxon";
					break;
				case "secondaryklaxon":
				case "secondaryhorn":
					Code = "secondaryklaxon";
					break;
				case "doorbuttonl":
					Code = "leftdoorbutton";
					break;
				case "doorbuttonr":
					Code = "rightdoorbutton";
					break;
				case "routelimit":
					Code = "routelimit";
					break;
				case "wiperposition":
					Code = "wiperposition";
					break;
				default:
					{
						Code = "0";
						bool unsupported = true;
						if (Subject.StartsWith("ats", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(3);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n <= 255) {
									Code = n.ToString(Culture) + " pluginstate";
									unsupported = false;
								}
							}
						} else if (Subject.StartsWith("doorl", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(5);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n < Train.NumberOfCars) {
									Code = n.ToString(Culture) + " leftdoorsindex ceiling";
									unsupported = false;
								} else {
									Code = "2";
									unsupported = false;
								}
							}
						} else if (Subject.StartsWith("doorr", StringComparison.OrdinalIgnoreCase)) {
							string a = Subject.Substring(5);
							int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n)) {
								if (n >= 0 & n < Train.NumberOfCars) {
									Code = n.ToString(Culture) + " rightdoorsindex ceiling";
									unsupported = false;
								} else {
									Code = "2";
									unsupported = false;
								}
							}
						}
						if (unsupported) {
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid subject " + Subject + " encountered in " + ErrorLocation);
						}
					} break;
			}
			return Code + Suffix;
		}

		

	}
}
