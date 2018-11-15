using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static class Panel2CfgParser {

		// constants
		private static double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double EyeDistance = 1.0;

		/// <summary>Parses a BVE2 / openBVE panel.cfg file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal static void ParsePanel2Config(string PanelFile, string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train, int Car)
		{
			//Train name, used for hacks detection
			string trainName = new System.IO.DirectoryInfo(TrainPath).Name.ToUpperInvariant();
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = Path.CombineFile(TrainPath, PanelFile);
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).TrimEnd();
				}
			}
			// initialize
			double PanelResolution = 1024.0;
			double PanelLeft = 0.0, PanelRight = 1024.0;
			double PanelTop = 0.0, PanelBottom = 1024.0;
			Vector2 PanelCenter = new Vector2(0, 512);
			Vector2 PanelOrigin = new Vector2(0, 512);
			double PanelBitmapWidth = 1024.0, PanelBitmapHeight = 1024.0;
			string PanelDaytimeImage = null;
			string PanelNighttimeImage = null;
			Color24 PanelTransparentColor = Color24.Blue;
			// parse lines for panel
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// panel
							case "this":
								i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									int j = Lines[i].IndexOf('='); if (j >= 0) {
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant()) {
											case "resolution":
												double pr = 0.0;
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out pr)) {
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												if (pr > 100)
												{
													PanelResolution = pr;
												}
												else
												{
													//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
													//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
													Interface.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												break;
											case "left":
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelLeft)) {
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line" + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "right":
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelRight)) {
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}

												if (Interface.CurrentOptions.EnableBveTsHacks)
												{
													switch ((int) PanelRight)
													{
														case 1696:
															if (PanelResolution == 1024 && trainName == "TOQ2000CN1EXP10" || trainName == "TOQ8500CS8EXP10")
															{
																PanelRight = 1024;
															}
															break;
													}
												}
												break;
											case "top":
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelTop)) {
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "bottom":
												if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelBottom)) {
													Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "daytimeimage":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value)) {
													Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelDaytimeImage = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelDaytimeImage)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + PanelDaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelDaytimeImage = null;
													}
												}
												break;
											case "nighttimeimage":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value)) {
													Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													PanelNighttimeImage = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelNighttimeImage)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + PanelNighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														PanelNighttimeImage = null;
													}
												}
												break;
											case "transparentcolor":
												if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out PanelTransparentColor)) {
													Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} break;
											case "center":
												{
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out PanelCenter.X)) {
															Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelCenter.Y)) {
															Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (Interface.CurrentOptions.EnableBveTsHacks)
														{
															switch ((int)PanelCenter.Y)
															{
																case 180:
																	switch (trainName.ToUpperInvariant())
																	{
																		case "LT_C69_77":
																		case "LT_C69_77_V2":
																			// Broken initial zoom
																			PanelCenter.Y = 350;
																			break;
																	}
																	break;
																case 200:
																	switch (trainName.ToUpperInvariant())
																	{
																		case "HM05":
																			// Broken initial zoom
																			PanelCenter.Y = 350;
																			break;
																	}
																	break;
																case 229:
																	if (PanelBottom == 768 && PanelResolution == 1024)
																	{
																		// Martin Finken's BVE4 trams: Broken initial zoom
																		PanelCenter.Y = 350;
																	}
																	break;
																case 255:
																	if (PanelBottom == 1024 && PanelResolution == 1024)
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
																				PanelCenter.Y = 350;
																				break;
																			case "LT_A60_62":
																			case "LT1972 MKII":
																				// Broken initial zoom and black patch at bottom of panel
																				PanelCenter.Y = 350;
																				PanelBottom = 792;
																				break;
																		}
																	}
																	break;
															}
															
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												}
											case "origin":
												{
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out PanelOrigin.X)) {
															Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelOrigin.Y)) {
															Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (Interface.CurrentOptions.EnableBveTsHacks)
														{
															switch (trainName)
															{
																case "8171BETA":
																	if (PanelResolution == 768 && PanelOrigin.Y == 256)
																	{
																		// 81-71: Bust panel origin means a flying cab....
																		PanelOrigin.Y = 0;
																	}
																	break;
															}
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
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
				if (Screen.Width >= Screen.Height) {
					WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
					WorldHeight = WorldWidth / World.AspectRatio;
				} else {
					WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
					WorldWidth = WorldHeight * World.AspectRatio;
				}
				double x0 = (PanelLeft - PanelCenter.X) / PanelResolution;
				double x1 = (PanelRight - PanelCenter.X) / PanelResolution;
				double y0 = (PanelCenter.Y - PanelBottom) / PanelResolution * World.AspectRatio;
				double y1 = (PanelCenter.Y - PanelTop) / PanelResolution * World.AspectRatio;
				World.CameraRestrictionBottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, EyeDistance);
				World.CameraRestrictionTopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, EyeDistance);
				Train.Cars[Car].DriverYaw = Math.Atan((PanelCenter.X - PanelOrigin.X) * WorldWidth / PanelResolution);
				Train.Cars[Car].DriverPitch = Math.Atan((PanelOrigin.Y - PanelCenter.Y) * WorldWidth / PanelResolution);
			}
			// create panel
			if (PanelDaytimeImage != null) {
				if (!System.IO.File.Exists(PanelDaytimeImage)) {
					Interface.AddMessage(MessageType.Error, true, "The daytime panel bitmap could not be found in " + FileName);
				} else {
					Texture tday;
					Textures.RegisterTexture(PanelDaytimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tday);
					Texture tnight = null;
					if (PanelNighttimeImage != null) {
						if (!System.IO.File.Exists(PanelNighttimeImage)) {
							Interface.AddMessage(MessageType.Error, true, "The nighttime panel bitmap could not be found in " + FileName);
						} else {
							Textures.RegisterTexture(PanelNighttimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tnight);
						}
					}
					OpenBVEGame.RunInRenderThread(() =>
					{
						Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
						//Textures.LoadTexture(tnight, OpenGlTextureWrapMode.ClampClamp);
					});
					PanelBitmapWidth = (double)tday.Width;
					PanelBitmapHeight = (double)tday.Height;
					CreateElement(Train.Cars[Car].CarSections[0], 0.0, 0.0, PanelBitmapWidth, PanelBitmapHeight, new Vector2(0.5, 0.5), 0.0, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, new Color32(255, 255, 255, 255), false);
				}
			}
			// parse lines for rest
			double invfac = Lines.Length == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// pilotlamp
							case "pilotlamp":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = Color24.Blue;
									int Layer = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "transparentcolor":
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Texture tday;
										Textures.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
										Texture tnight = null;
										if (NighttimeImage != null) {
											Textures.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
										}
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
										});
										int w = tday.Width;
										int h = tday.Height;
										int j = CreateElement(Train.Cars[Car].CarSections[0], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, new Color32(255, 255, 255, 255), false);
										string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
										Train.Cars[Car].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f + " 1 == --");
									}
								} break;
								// needle
							case "needle":
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
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													{
														int k = Value.IndexOf(',');
														if (k >= 0) {
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
																Interface.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
																Interface.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
														} else {
															Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case "radius":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "color":
													if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "transparentcolor":
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "origin":
													{
														int k = Value.IndexOf(',');
														if (k >= 0) {
															string a = Value.Substring(0, k).TrimEnd();
															string b = Value.Substring(k + 1).TrimStart();
															if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out OriginX)) {
																Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															}
															if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out OriginY)) {
																Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																OriginX = -OriginX;
															}
															OriginDefined = true;
														} else {
															Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} break;
												case "initialangle":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "lastangle":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "minimum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "maximum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "naturalfreq":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out NaturalFrequency)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (NaturalFrequency < 0.0) {
														Interface.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														NaturalFrequency = -NaturalFrequency;
													} break;
												case "dampingratio":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out DampingRatio)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (DampingRatio < 0.0) {
														Interface.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														DampingRatio = -DampingRatio;
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "backstop":
													if (Value.Length != 0 && Value.ToLowerInvariant() == "true" || Value == "1")
													{
														Backstop = true;
													}
													break;
												case "smoothed":
													if (Value.Length != 0 && Value.ToLowerInvariant() == "true" || Value == "1")
													{
														Smoothed = true;
													}
													break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null)
									{
										Texture tday;
										Textures.RegisterTexture(DaytimeImage,
											new TextureParameters(null,
												new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
										Texture tnight = null;
										if (NighttimeImage != null)
										{
											Textures.RegisterTexture(NighttimeImage,
												new TextureParameters(null,
													new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
										}
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)tday.Width;
										double h = (double)tday.Height;
										if (!OriginDefined) {
											OriginX = 0.5 * w;
											OriginY = 0.5 * h;
										}
										double ox = OriginX / w;
										double oy = OriginY / h;
										double n = Radius == 0.0 | OriginY == 0.0 ? 1.0 : Radius / OriginY;
										double nx = n * w;
										double ny = n * h;
										int j = CreateElement(Train.Cars[Car].CarSections[0], LocationX - ox * nx, LocationY - oy * ny, nx, ny, new Vector2(ox, oy), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, Color, false);
										Train.Cars[Car].CarSections[0].Elements[j].RotateZDirection = Vector3.Backward;
										Train.Cars[Car].CarSections[0].Elements[j].RotateXDirection = Vector3.Right;
										Train.Cars[Car].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Car].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Car].CarSections[0].Elements[j].RotateXDirection);
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
												f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
												break;
										}
										//Convert angles from degrees to radians
										InitialAngle *= 0.0174532925199433;
										LastAngle *= 0.0174532925199433;
										double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
										double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
										f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
										if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0) {
											Train.Cars[Car].CarSections[0].Elements[j].RotateZDamping = new Damping(NaturalFrequency, DampingRatio);
										}
										Train.Cars[Car].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
										if (Backstop)
										{
											Train.Cars[Car].CarSections[0].Elements[j].RotateZFunction.Minimum = InitialAngle;
											Train.Cars[Car].CarSections[0].Elements[j].RotateZFunction.Maximum = LastAngle;
										}
									}
								} break;
							case "lineargauge":
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
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "minimum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
													{
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "maximum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
													{
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "width":
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Width))
													{
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
													break;
												case "direction":
													{
														string[] s = Value.Split(',');
														if (s.Length == 2)
														{
															double x, y;
															if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x))
															{
																Interface.AddMessage(MessageType.Error, false, "X is invalid in LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
															{
																Interface.AddMessage(MessageType.Error, false, "Y is invalid in  LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
															}
															else
															{
																Direction = new Vector2(x, y);
															}
														}
														else
														{
															Interface.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in LinearGauge Direction at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
													break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "transparentcolor":
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null) {
										Texture tday;
										Textures.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
										Texture tnight = null;
										if (NighttimeImage != null) {
											Textures.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
										}
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
										});
										int w = tday.Width;
										int h = tday.Height;
										int j = CreateElement(Train.Cars[Car].CarSections[0], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, new Color32(255, 255, 255, 255), false);
										if (Maximum < Minimum)
										{
											Interface.AddMessage(MessageType.Error, false, "Maximum value must be greater than minimum value " + Section + " in " + FileName);
											break;
										}
										string tf = GetInfixFunction(Train, Subject, Minimum, Maximum, Width, tday.Width, Section + " in " + FileName);
										if (tf != String.Empty)
										{
											Train.Cars[Car].CarSections[0].Elements[j].TextureShiftXDirection = Direction;
											Train.Cars[Car].CarSections[0].Elements[j].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(tf);
										}
									}
								} break;
								// digitalnumber
							case "digitalnumber":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									string DaytimeImage = null, NighttimeImage = null;
									Color24 TransparentColor = Color24.Blue;
									double Layer = 0.0; int Interval = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "daytimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														DaytimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(DaytimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															DaytimeImage = null;
														}
													}
													break;
												case "nighttimeimage":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														NighttimeImage = Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(NighttimeImage)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															NighttimeImage = null;
														}
													}
													break;
												case "transparentcolor":
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "interval":
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Interval)) {
														Interface.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Interval <= 0) {
														Interface.AddMessage(MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (DaytimeImage == null) {
										Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
									}
									if (Interval <= 0) {
										Interface.AddMessage(MessageType.Error, false, "Interval is required to be specified in " + Section + " in " + FileName);
									}
									// create element
									if (DaytimeImage != null & Interval > 0) {
										int wday, hday;
										Program.CurrentHost.QueryTextureDimensions(DaytimeImage, out wday, out hday);
										if (wday > 0 & hday > 0) {
											int numFrames = hday / Interval;
											if (Interface.CurrentOptions.EnableBveTsHacks)
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
														if (Train.Handles.Power.MaximumNotch > numFrames)
														{
															numFrames = Train.Handles.Power.MaximumNotch;
														}
														break;
													case "brake":
														int b = Train.Handles.Brake.MaximumNotch + 2;
														if (Train.Handles.HasHoldBrake)
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
													Textures.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday[k]);
												}
												else if (k * Interval >= hday)
												{
													numFrames = k;
													Array.Resize(ref tday, k);
												}
												else
												{
													Textures.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, hday - (k * Interval)), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday[k]);
												}
											}
											if (NighttimeImage != null) {
												int wnight, hnight;
												Program.CurrentHost.QueryTextureDimensions(NighttimeImage, out wnight, out hnight);
												tnight = new Texture[numFrames];
												for (int k = 0; k < numFrames; k++) {
													if ((k + 1) * Interval <= hnight)
													{
														Textures.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight[k]);
													}
													else if (k * Interval > hnight)
													{
														tnight[k] = null;
													}
													else
													{
														Textures.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, hnight - (k * Interval)), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight[k]);
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
												int l = CreateElement(Train.Cars[Car].CarSections[0], LocationX, LocationY, (double)wday, (double)Interval, new Vector2(0.5, 0.5), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday[k], tnight[k], new Color32(255, 255, 255, 255), k != 0);
												if (k == 0) j = l;
											}
											string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
											Train.Cars[Car].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
										}
									}
								} break;
								// digitalgauge
							case "digitalgauge":
								{
									string Subject = "true";
									double LocationX = 0.0, LocationY = 0.0;
									Color32 Color = new Color32(0, 0, 0, 255);
									double Radius = 0.0;
									int Layer = 0;
									double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
									double Minimum = 0.0, Maximum = 1000.0;
									double Step = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "subject":
													Subject = Value;
													break;
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Interface.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Interface.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "radius":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Radius == 0.0) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
												case "color":
													if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "initialangle":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														InitialAngle *= 0.0174532925199433;
													} break;
												case "lastangle":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														LastAngle *= 0.0174532925199433;
													} break;
												case "minimum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "maximum":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "step":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Step)) {
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									if (Radius == 0.0) {
										Interface.AddMessage(MessageType.Error, false, "Radius is required to be non-zero in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Minimum == Maximum) {
										Interface.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
										Radius = 0.0;
									}
									if (Math.Abs(InitialAngle - LastAngle) > 6.28318531) {
										Interface.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
									}
									if (Radius != 0.0) {
										// create element
										int j = CreateElement(Train.Cars[Car].CarSections[0], LocationX - Radius, LocationY - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, null, null, Color, false);
										InitialAngle = InitialAngle + Math.PI;
										LastAngle = LastAngle + Math.PI;
										double x0 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.X;
										double y0 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Y;
										double z0 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Z;
										double x1 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.X;
										double y1 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Y;
										double z1 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Z;
										double x2 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.X;
										double y2 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Y;
										double z2 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Z;
										double x3 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.X;
										double y3 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Y;
										double z3 = Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Z;
										double cx = 0.25 * (x0 + x1 + x2 + x3);
										double cy = 0.25 * (y0 + y1 + y2 + y3);
										double cz = 0.25 * (z0 + z1 + z2 + z3);
										VertexTemplate[] vertices = new VertexTemplate[11];
										for (int v = 0; v < 11; v++)
										{
											vertices[v] = new Vertex();
										}
										int[][] faces = new int[][] {
											new int[] { 0, 1, 2 },
											new int[] { 0, 3, 4 },
											new int[] { 0, 5, 6 },
											new int[] { 0, 7, 8 },
											new int[] { 0, 9, 10 }
										};
										Train.Cars[Car].CarSections[0].Elements[j].States[0].Object.Mesh = new World.Mesh(vertices, faces, Color);
										Train.Cars[Car].CarSections[0].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
										Train.Cars[Car].CarSections[0].Elements[j].LEDInitialAngle = InitialAngle;
										Train.Cars[Car].CarSections[0].Elements[j].LEDLastAngle = LastAngle;
										Train.Cars[Car].CarSections[0].Elements[j].LEDVectors = new Vector3[] {
											new Vector3(x0, y0, z0),
											new Vector3(x1, y1, z1),
											new Vector3(x2, y2, z2),
											new Vector3(x3, y3, z3),
											new Vector3(cx, cy, cz)
										};
										string f = GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
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
										Train.Cars[Car].CarSections[0].Elements[j].LEDFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
									} else {
										Interface.AddMessage(MessageType.Error, false, "Radius is required to be specified in " + Section + " in " + FileName);
									}
								} break;
								// timetable
							case "timetable":
								{
									double LocationX = 0.0, LocationY = 0.0;
									double Width = 0.0, Height = 0.0;
									//We read the transparent color for the timetable from the config file, but it is never used
									//TODO: Fix or depreciate??
									Color24 TransparentColor = new Color24(0, 0, 255);
									double Layer = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('=');
										if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											switch (Key.ToLowerInvariant()) {
												case "location":
													int k = Value.IndexOf(',');
													if (k >= 0) {
														string a = Value.Substring(0, k).TrimEnd();
														string b = Value.Substring(k + 1).TrimStart();
														if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX)) {
															Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
														if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY)) {
															Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														}
													} else {
														Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "width":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Width)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Width <= 0.0) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "height":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Height)) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else if (Height <= 0.0) {
														Interface.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "transparentcolor":
													if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor)) {
														Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
												case "layer":
													if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer)) {
														Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} break;
											}
										} i++;
									} i--;
									// create element
									if (Width <= 0.0) {
										Interface.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
									}
									if (Height <= 0.0) {
										Interface.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
									}
									if (Width > 0.0 & Height > 0.0) {
										int j = CreateElement(Train.Cars[Car].CarSections[0], LocationX, LocationY, Width, Height, new Vector2(0.5, 0.5), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, null, null, new Color32(255, 255, 255, 255), false);
										Train.Cars[Car].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("timetable");
										Timetable.AddObjectForCustomTimetable(Train.Cars[Car].CarSections[0].Elements[j]);
									}
								} break;
						}
					}
				}
			}
		}

		private static string GetInfixFunction(TrainManager.Train Train, string Subject, double Minimum, double Maximum, int Width, int TextureWidth, string ErrorLocation)
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
				ftc -= (double) Width/TextureWidth;
			}
			double range = ftc / ((Maximum + mp) - (Minimum + mp));
			switch(Subject.ToLowerInvariant())
			{
				case "acc":
					return "if[acceleration < " + Maximum + ", if[acceleration > " + Minimum + ", acceleration " + " * " + range + ", 0]," + ftc + "]";
				case "motor":
					return "if[accelerationmotor < " + Maximum + ", if[Speed > " + Minimum + ", accelerationmotor " + " * " + range + ", 0]," + ftc + "]";
				case "true":
					return "0.0";
				case "kmph":
					Maximum /= 3.6;
					return "if[Speed < " + Maximum + ", if[Speed > " + Minimum + ", (Speed * 3.6) " + " * " + range + ", 0]," + ftc + "]";
				case "mph":
					Maximum /= 2.23694;
					return "if[Speed < " + Maximum + ", if[Speed >  " + Minimum + ", (Speed * 2.23694) " + " * " + range + ", 0]," + ftc + "]";
				case "ms":
					return "if[Speed < " + Maximum + ", if[Speed >  " + Minimum + ", Speed " + " * " + range + ", 0]," + ftc + "]";
				case "bc":
					Maximum /= 0.001;
					return "if[BrakeCylinder < " + Maximum + ", if[BrakeCylinder >  " + Minimum + ", (BrakeCylinder * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "locobrakecylinder":
					Maximum /= 0.001;
					return "if[BrakeCylinder["+ Train.DriverCar + "] < " + Maximum + ", if[BrakeCylinder["+ Train.DriverCar + "] >  " + Minimum + ", (BrakeCylinder["+ Train.DriverCar + "] * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "mr":
					Maximum /= 0.001;
					return "if[MainReservoir < " + Maximum + ", if[MainReservoir >  " + Minimum + ", (MainReservoir * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "sap":
					Maximum /= 0.001;
					return "if[StraightAirPipe < " + Maximum + ", if[StraightAirPipe >  " + Minimum + ", (StraightAirPipe * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "bp":
					Maximum /= 0.001;
					return "if[BrakePipe < " + Maximum + ", if[BrakePipe >  " + Minimum + ", (BrakePipe * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "locobrakepipe":
					Maximum /= 0.001;
					return "if[BrakePipe["+ Train.DriverCar + "] < " + Maximum + ", if[BrakePipe["+ Train.DriverCar + "] >  " + Minimum + ", (BrakePipe["+ Train.DriverCar + "] * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "er":
					Maximum /= 0.001;
					return "if[EqualizingReservoir < " + Maximum + ", if[EqualizingReservoir >  " + Minimum + ", (EqualizingReservoir * .001) " + " * " + range + ", 0]," + ftc + "]";
				case "doors":
					return "if[Doors < " + Maximum + ", if[Doors >  " + Minimum + ", Doors " + " * " + range + ", 0]," + ftc + "]";
				case "doorbuttonl":
				case "doorbuttonleft":
					return "if[leftdoorbuttom < " + Maximum + ", if[leftdoorbutton >  " + Minimum + ", leftdoorbutton " + " * " + range + ", 0]," + ftc + "]";
				case "doorbuttonr":
				case "doorbuttonright":
					return "if[rightdoorbuttom < " + Maximum + ", if[rightdoorbutton >  " + Minimum + ", rightdoorbutton " + " * " + range + ", 0]," + ftc + "]";
				case "power":
					return "if[PowerNotch < " + Maximum + ", if[PowerNotch >  " + Minimum + ", PowerNotch " + " * " + range + ", 0]," + ftc + "]";
				case "locobrake":
					return "if[LocoBrakeNotch < " + Maximum + ", if[LocoBrakeNotch >  " + Minimum + ", LocoBrakeNotch " + " * " + range + ", 0]," + ftc + "]";
				case "brake":
					return "if[BrakeNotch < " + Maximum + ", if[BrakeNotch >  " + Minimum + ", BrakeNotch " + " * " + range + ", 0]," + ftc + "]";
				case "rev":
					return "if[ReverserNotch < " + Maximum + ", if[ReverserNotch >  " + Minimum + ", (ReverserNotch + 1) " + " * " + range + ", 0]," + ftc + "]";
				case "hour":
					return range / 24 + " * floor[mod[time * 0.000277777777777778, 24]]";
				case "min":
					return range / 60 + " * floor[time * 0.0166666666666667]";
				case "sec":
					return range / 60 + " * floor[time]";
				default:
					if (Subject.StartsWith("doorl", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(5);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n < Train.Cars.Length)
							{
								string di = "leftDoors[" + n + "]";
								return "if[" + di + " < " + Maximum + ", if["+ di+ " >  " + Minimum + ", "+ di + " * " + range + ", 0]," + ftc + "]";
							}
							else
							{
								return String.Empty;
							}
						}
					}
					else if (Subject.StartsWith("doorr", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(5);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n < Train.Cars.Length)
							{
								string di = "rightDoors[" + n + "]";
								return "if[" + di + " < " + Maximum + ", if[" + di + " >  " + Minimum + ", " + di + " * " + range + ", 0]," + ftc + "]";
							}
							else
							{
								return String.Empty;
							}
						}
					}
					if (Subject.StartsWith("ats", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(3);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n <= 255)
							{
								return "if[PluginState[" + n + "] < " + Maximum + ", if[PluginState[" + n + "] > 0, PluginState[" + n + "] " + " * " + range + ", 0]," + ftc + "]";
							}
						}
					}
					break;

			}
			Interface.AddMessage(MessageType.Error, false, "Invalid subject " + Subject + " encountered in " + ErrorLocation);
			return String.Empty;
		}

		/// <summary>Converts a Panel2.cfg subject to an animation function stack</summary>
		/// <param name="Train">The train</param>
		/// <param name="Subject">The subject to convert</param>
		/// <param name="ErrorLocation">The location in the Panel2.cfg file</param>
		/// <returns>The parsed animation function stack</returns>
		private static string GetStackLanguageFromSubject(TrainManager.Train Train, string Subject, string ErrorLocation) {
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
								string t0 = Math.Pow(10.0, (double)n).ToString(Culture);
								string t1 = Math.Pow(10.0, (double)-n).ToString(Culture);
								Suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
							}
							Subject = Subject.Substring(0, i);
							i--;
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
					Code = Train.DriverCar + "brakepipeindex 0.001 *";
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
								if (n >= 0 & n < Train.Cars.Length) {
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
								if (n >= 0 & n < Train.Cars.Length) {
									Code = n.ToString(Culture) + " rightdoorsindex ceiling";
									unsupported = false;
								} else {
									Code = "2";
									unsupported = false;
								}
							}
						}
						if (unsupported) {
							Interface.AddMessage(MessageType.Error, false, "Invalid subject " + Subject + " encountered in " + ErrorLocation);
						}
					} break;
			}
			return Code + Suffix;
		}

		

		private static int CreateElement(TrainManager.CarSection Section, double Left, double Top, double Width, double Height, Vector2 RelativeRotationCenter, double Distance, double PanelResolution, double PanelLeft, double PanelRight, double PanelTop, double PanelBottom, double PanelBitmapWidth, double PanelBitmapHeight, Vector2 PanelCenter, Vector2 PanelOrigin, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement) {
			double WorldWidth, WorldHeight;
			if (Screen.Width >= Screen.Height) {
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / World.AspectRatio;
			} else {
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
				WorldWidth = WorldHeight * World.AspectRatio;
			}
			double x0 = Left / PanelResolution;
			double x1 = (Left + Width) / PanelResolution;
			double y0 = (PanelBottom - Top) / PanelResolution * World.AspectRatio;
			double y1 = (PanelBottom - (Top + Height)) / PanelResolution * World.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd; x1 += xd;
			double yt = PanelBottom - PanelResolution / World.AspectRatio;
			double yd = (PanelCenter.Y - yt) / (PanelBottom - yt) - 0.5;
			y0 += yd; y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenter.X) + x1 * RelativeRotationCenter.X;
			double ym = y0 * (1.0 - RelativeRotationCenter.Y) + y1 * RelativeRotationCenter.Y;
			Vector3[] v = new Vector3[4];
			v[0] = new Vector3(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3(x1 - xm, y1 - ym, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new World.MeshFace[] { new World.MeshFace(new int[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new World.MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = (byte)(DaytimeTexture != null ? World.MeshMaterial.TransparentColorMask : 0);
			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			Object.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = EyeDistance - Distance + Driver.Z;
			// add object
			if (AddStateToLastElement) {
				int n = Section.Elements.Length - 1;
				int j = Section.Elements[n].States.Length;
				Array.Resize<ObjectManager.AnimatedObjectState>(ref Section.Elements[n].States, j + 1);
				Section.Elements[n].States[j].Position = o;
				Section.Elements[n].States[j].Object = Object;
				return n;
			} else {
				int n = Section.Elements.Length;
				Array.Resize<ObjectManager.AnimatedObject>(ref Section.Elements, n + 1);
				Section.Elements[n] = new ObjectManager.AnimatedObject();
				Section.Elements[n].States = new ObjectManager.AnimatedObjectState[1];
				Section.Elements[n].States[0].Position = o;
				Section.Elements[n].States[0].Object = Object;
				Section.Elements[n].CurrentState = 0;
				Section.Elements[n].ObjectIndex = ObjectManager.CreateDynamicObject();
				ObjectManager.Objects[Section.Elements[n].ObjectIndex] = Object.Clone();
				return n;
			}
		}

	}
}
