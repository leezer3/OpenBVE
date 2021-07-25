﻿using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using TrainManager.Car;
using TrainManager.Handles;

namespace Train.OpenBve
{
	internal class PanelCfgParser
	{
		internal readonly Plugin Plugin;

		internal PanelCfgParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		// constants
		private const double StackDistance = 0.000001;

		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed</remarks>
		private const double EyeDistance = 1.0;

		private double WorldWidth, WorldHeight, WorldLeft, WorldTop;
		private double FullWidth = 480, FullHeight = 440, SemiHeight = 240;

		/// <summary>Parses a BVE1 panel.cfg file</summary>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Car">The car to add the panel to</param>
		internal void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding, CarBase Car)
		{
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = Path.CombineFile(TrainPath, "panel.cfg");
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++)
			{
				Lines[i] = Lines[i].Trim();
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).TrimEnd();
				}
			}
			// initialize

			double AspectRatio = FullWidth / FullHeight;

			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance;
				WorldWidth = WorldHeight * AspectRatio;
			}

			Car.CameraRestriction.BottomLeft = new Vector3(-0.5 * WorldWidth, -0.5 * WorldHeight, EyeDistance);
			Car.CameraRestriction.TopRight = new Vector3(0.5 * WorldWidth, 0.5 * WorldHeight, EyeDistance);
			WorldLeft = Car.Driver.X - 0.5 * WorldWidth;
			WorldTop = Car.Driver.Y + 0.5 * WorldHeight;
			double WorldZ = Car.Driver.Z;
			const double UpDownAngleConstant = -0.191986217719376;
			double PanelYaw = 0.0;
			double PanelPitch = UpDownAngleConstant;
			string PanelBackground = Path.CombineFile(TrainPath, "panel.bmp");
			// parse lines for panel and view
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length > 0)
				{
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
					{
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant())
						{
							// panel
							case "panel":
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant())
										{
											case "background":
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													PanelBackground = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelBackground))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + PanelBackground + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
												}

												break;
										}
									}

									i++;
								}

								i--;
								break;
							// view
							case "view":
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant())
										{
											case "yaw":
											{
												double yaw = 0.0;
												if (Value.Length > 0 && !NumberFormats.TryParseDoubleVb6(Value, out yaw))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													yaw = 0.0;
												}

												PanelYaw = Math.Atan(yaw);
											}
												break;
											case "pitch":
											{
												double pitch = 0.0;
												if (Value.Length > 0 && !NumberFormats.TryParseDoubleVb6(Value, out pitch))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													pitch = 0.0;
												}

												PanelPitch = Math.Atan(pitch) + UpDownAngleConstant;
											}
												break;
										}
									}

									i++;
								}

								i--;
								break;
						}
					}
				}
			}

			Car.DriverYaw = PanelYaw;
			Car.DriverPitch = PanelPitch;
			// panel
			{
				if (!System.IO.File.Exists(PanelBackground))
				{
					Plugin.currentHost.AddMessage(MessageType.Error, true, "The panel image could not be found in " + FileName);
				}
				else
				{
					Plugin.currentHost.RegisterTexture(PanelBackground, new TextureParameters(null, Color24.Blue), out var t, true);
					SemiHeight = FullHeight - t.Height;
					CreateElement(Car, 0, SemiHeight, t.Width, t.Height, WorldZ + EyeDistance, t, Color32.White);
				}
			}
			// parse lines for rest
			double invfac = Lines.Length == 0 ? 0.4 : 0.4 / Lines.Length;
			for (int i = 0; i < Lines.Length; i++)
			{
				Plugin.CurrentProgress = Plugin.LastProgress + invfac * i;
				if ((i & 7) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}

				if (Lines[i].Length != 0)
				{
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
					{
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant())
						{
							// pressuregauge
							case "pressuregauge":
							case "pressuremeter":
							case "pressureindicator":
							case "圧力計":
							{
								int Type = 0;
								Color32[] NeedleColor = new Color32[] {Color32.Black, Color32.Black};
								int[] NeedleType = new int[] {0, 0};
								double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
								string Background = null, Cover = null;
								double Angle = 0.785398163397449, Minimum = 0.0, Maximum = 1000.0;
								double UnitFactor = 1000.0;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "type":
											case "形態":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Type))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Type is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Type = 0;
												}
												else if (Type != 0 & Type != 1)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Type must be either 0 or 1 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Type = 0;
												}

												break;
											case "lowerneedle":
											case "lowerhand":
											case "下針":
											case "upperneedle":
											case "upperhand":
											case "上針":
												int k = Key.ToLowerInvariant() == "lowerneedle" | Key.ToLowerInvariant() == "lowerhand" | Key == "下針" ? 0 : 1;
												if (Arguments.Length >= 1 && Arguments[0].Length > 0)
												{
													switch (Arguments[0].ToLowerInvariant())
													{
														case "bc":
														case "ブレーキシリンダ":
															NeedleType[k] = 1;
															break;
														case "sap":
														case "直通管":
															NeedleType[k] = 2;
															break;
														case "bp":
														case "ブレーキ管":
														case "制動管":
															NeedleType[k] = 3;
															break;
														case "er":
														case "釣り合い空気溜め":
														case "釣り合い空気ダメ":
														case "つりあい空気溜め":
														case "ツリアイ空気ダメ":
															NeedleType[k] = 4;
															break;
														case "mr":
														case "元空気溜め":
														case "元空気ダメ":
															NeedleType[k] = 5;
															break;
														default:
														{
															if (!NumberFormats.TryParseIntVb6(Arguments[0], out var a))
															{
																Plugin.currentHost.AddMessage(MessageType.Error, false, "Subject is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																a = 0;
															}

															NeedleType[k] = a;
														}
															break;
													}
												}

												int r = 0, g = 0, b = 0;
												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out r))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = 0;
												}
												else if (r < 0 | r > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = r < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out g))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + Key + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = 0;
												}
												else if (g < 0 | g > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = g < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out b))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + Key + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = 0;
												}
												else if (b < 0 | b > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = b < 0 ? 0 : 255;
												}

												NeedleColor[k] = new Color32((byte) r, (byte) g, (byte) b, 255);
												break;
											case "center":
											case "中心":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterY = 0.0;
												}

												break;
											case "radius":
											case "半径":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Radius = 1.0;
												}

												break;
											case "background":
											case "背景":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Background = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Background))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Background = null;
													}
												}

												break;
											case "cover":
											case "ふた":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Cover = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Cover))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + Cover + "could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Cover = null;
													}
												}

												break;
											case "unit":
											case "単位":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0)
												{
													string a = Arguments[0].ToLowerInvariant();
													switch (a)
													{
														case "kpa":
														case "0":
															UnitFactor = 1000.0;
															break;
														case "1":
														case "kgf/cm2":
														case "kgf/cm^2":
														case "kg/cm2":
														case "kg/cm^2":
															UnitFactor = 98066.5;
															break;
														default:
															Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															break;
													}
												}

												break;
											case "maximum":
											case "最大":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Maximum))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "PressureValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Maximum = 1000.0;
												}

												break;
											case "minimum":
											case "最小":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Minimum))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "PressureValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Minimum = 0.0;
												}

												break;
											case "angle":
											case "角度":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Angle))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Angle = 0.785398163397449;
												}
												else
												{
													Angle = Angle.ToRadians();
												}

												break;
										}
									}

									i++;
								}

								i--;
								// units
								Minimum *= UnitFactor;
								Maximum *= UnitFactor;
								// background
								if (Background != null)
								{
									Plugin.currentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var t, true);
									CreateElement(Car, CenterX - 0.5 * t.Width, CenterY + SemiHeight - 0.5 * t.Height, WorldZ + EyeDistance - 3.0 * StackDistance, t);
								}

								// cover
								if (Cover != null)
								{
									Plugin.currentHost.RegisterTexture(Cover, new TextureParameters(null, Color24.Blue), out var t, true);
									CreateElement(Car, CenterX - 0.5 * t.Width, CenterY + SemiHeight - 0.5 * t.Height, WorldZ + EyeDistance - 6.0 * StackDistance, t);
								}

								if (Type == 0)
								{
									// needles
									for (int k = 0; k < 2; k++)
									{
										if (NeedleType[k] != 0)
										{
											string Folder = Plugin.FileSystem.GetDataFolder("Compatibility");
											string File = Path.CombineFile(Folder, k == 0 ? "needle_pressuregauge_lower.png" : "needle_pressuregauge_upper.png");
											Plugin.currentHost.RegisterTexture(File, new TextureParameters(null, null), out var t, true);
											int j = CreateElement(Car, CenterX - Radius * t.AspectRatio, CenterY + SemiHeight - Radius, 2.0 * Radius * t.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - (4 + k) * StackDistance, t, NeedleColor[k]);
											Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
											Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
											Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
											double c0 = (Angle * (Maximum - Minimum) - 2.0 * Minimum * Math.PI) / (Maximum - Minimum) + Math.PI;
											double c1 = 2.0 * (Math.PI - Angle) / (Maximum - Minimum);
											string Variable = "0";
											switch (NeedleType[k])
											{
												case 1:
													Variable = "brakecylinder";
													break;
												case 2:
													Variable = "straightairpipe";
													break;
												case 3:
													Variable = "brakepipe";
													break;
												case 4:
													Variable = "equalizingreservoir";
													break;
												case 5:
													Variable = "mainreservoir";
													break;
											}

											Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
										}
									}
								}
								else if (Type == 1)
								{
									// leds
									if (NeedleType[1] != 0)
									{
										int j = CreateElement(Car, CenterX - Radius, CenterY + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, null, NeedleColor[1]);
										double x0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
										double y0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
										double z0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
										double x1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
										double y1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
										double z1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
										double x2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
										double y2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
										double z2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
										double x3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
										double y3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
										double z3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
										double cx = 0.25 * (x0 + x1 + x2 + x3);
										double cy = 0.25 * (y0 + y1 + y2 + y3);
										double cz = 0.25 * (z0 + z1 + z2 + z3);
										VertexTemplate[] vertices = new VertexTemplate[11];
										for (int v = 0; v < 11; v++)
										{
											//The verticies are transformed by the LED function, so must be created here at zero
											vertices[v] = new Vertex();
										}

										int[][] faces =
										{
											new[] {0, 1, 2},
											new[] {0, 3, 4},
											new[] {0, 5, 6},
											new[] {0, 7, 8},
											new[] {0, 9, 10}
										};
										Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, NeedleColor[1]);
										Car.CarSections[0].Groups[0].Elements[j].LEDClockwiseWinding = true;
										Car.CarSections[0].Groups[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
										Car.CarSections[0].Groups[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
										Car.CarSections[0].Groups[0].Elements[j].LEDVectors = new[]
										{
											new Vector3(x0, y0, z0),
											new Vector3(x1, y1, z1),
											new Vector3(x2, y2, z2),
											new Vector3(x3, y3, z3),
											new Vector3(cx, cy, cz)
										};
										double c0 = (Angle * (Maximum - Minimum) - 2.0 * Minimum * Math.PI) / (Maximum - Minimum);
										double c1 = 2.0 * (Math.PI - Angle) / (Maximum - Minimum);
										string Variable;
										switch (NeedleType[1])
										{
											case 1:
												Variable = "brakecylinder";
												break;
											case 2:
												Variable = "straightairpipe";
												break;
											case 3:
												Variable = "brakepipe";
												break;
											case 4:
												Variable = "equalizingreservoir";
												break;
											case 5:
												Variable = "mainreservoir";
												break;
											default:
												Variable = "0";
												break;
										}

										Car.CarSections[0].Groups[0].Elements[j].LEDFunction = new FunctionScript(Plugin.currentHost, Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
									}
								}
							}
								break;
							// speedometer
							case "speedometer":
							case "speedindicator":
							case "速度計":
							{
								int Type = 0;
								Color32 Needle = Color32.White;
								bool NeedleOverridden = false;
								double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
								string Background = null, Cover = null, Atc = null;
								double Angle = 1.0471975511966, Maximum = 33.3333333333333, AtcRadius = 0.0;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "type":
											case "形態":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Type))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Type = 0;
												}
												else if (Type != 0 & Type != 1)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value must be either 0 or 1 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Type = 0;
												}

												break;
											case "background":
											case "背景":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Background = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Background))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Background = null;
													}
												}

												break;
											case "needle":
											case "hand":
											case "針":
											{
												int r = 0, g = 0, b = 0;
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = 255;
												}
												else if (r < 0 | r > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = r < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = 255;
												}
												else if (g < 0 | g > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = g < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = 255;
												}
												else if (b < 0 | b > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = b < 0 ? 0 : 255;
												}

												Needle = new Color32((byte) r, (byte) g, (byte) b, 255);
												NeedleOverridden = true;
											}
												break;
											case "cover":
											case "ふた":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												Cover = Path.CombineFile(TrainPath, Value);
												if (!System.IO.File.Exists(Cover))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName" + Cover + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Cover = null;
												}

												break;
											case "atc":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												Atc = Path.CombineFile(TrainPath, Value);
												if (!System.IO.File.Exists(Atc))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName" + Atc + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Atc = null;
												}

												break;
											case "atcradius":
											case "atc半径":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out AtcRadius))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													AtcRadius = 0.0;
												}

												break;
											case "center":
											case "中心":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterY = 0.0;
												}

												break;
											case "radius":
											case "半径":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Radius = 0.0;
												}

												break;
											case "angle":
											case "角度":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Angle))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Angle = 1.0471975511966;
												}
												else
												{
													Angle = Angle.ToRadians();
												}

												break;
											case "maximum":
											case "最大":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Maximum))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "SpeedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Maximum = 33.3333333333333;
												}
												else
												{
													Maximum *= 0.277777777777778;
												}

												break;
										}
									}

									i++;
								}

								i--;
								if (Background != null)
								{
									// background/led
									Plugin.currentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var t, true);
									CreateElement(Car, CenterX - 0.5 * t.Width, CenterY + SemiHeight - 0.5 * t.Height, WorldZ + EyeDistance - 3.0 * StackDistance, t);
								}

								if (Cover != null)
								{
									// cover
									Plugin.currentHost.RegisterTexture(Cover, new TextureParameters(null, Color24.Blue), out var t, true);
									CreateElement(Car, CenterX - 0.5 * t.Width, CenterY + SemiHeight - 0.5 * t.Height, WorldZ + EyeDistance - 6.0 * StackDistance, t);
								}

								if (Atc != null)
								{
									// atc
									Plugin.currentHost.QueryTextureDimensions(Atc, out var w, out var h);
									if (w > 0 & h > 0)
									{
										int n = w / h;
										int k = -1;
										for (int j = 0; j < n; j++)
										{
											double s;
											switch (j)
											{
												case 1:
													s = 0.0;
													break;
												case 2:
													s = 15.0;
													break;
												case 3:
													s = 25.0;
													break;
												case 4:
													s = 45.0;
													break;
												case 5:
													s = 55.0;
													break;
												case 6:
													s = 65.0;
													break;
												case 7:
													s = 75.0;
													break;
												case 8:
													s = 90.0;
													break;
												case 9:
													s = 100.0;
													break;
												case 10:
													s = 110.0;
													break;
												case 11:
													s = 120.0;
													break;
												default:
													s = -1.0;
													break;
											}

											s *= 0.277777777777778;
											double a;
											if (s >= 0.0)
											{
												a = 2.0 * s * (Math.PI - Angle) / Maximum + Angle + Math.PI;
											}
											else
											{
												a = Math.PI;
											}

											double x = CenterX - 0.5 * h + Math.Sin(a) * AtcRadius;
											double y = CenterY - 0.5 * h - Math.Cos(a) * AtcRadius + SemiHeight;
											Plugin.currentHost.RegisterTexture(Atc, new TextureParameters(new TextureClipRegion(j * h, 0, h, h), Color24.Blue), out var t, true);
											if (j == 0)
											{
												k = CreateElement(Car, x, y, h, h, WorldZ + EyeDistance - 4.0 * StackDistance, t, Color32.White);
											}
											else
											{
												CreateElement(Car, x, y, h, h, WorldZ + EyeDistance - 4.0 * StackDistance, t, Color32.White, true);
											}
										}

										Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "271 pluginstate", false);
									}
								}

								if (Type == 0)
								{
									// needle
									string Folder = Plugin.FileSystem.GetDataFolder("Compatibility");
									string File = Path.CombineFile(Folder, "needle_speedometer.png");
									Plugin.currentHost.RegisterTexture(File, new TextureParameters(null, null), out var t, true);
									int j = CreateElement(Car, CenterX - Radius * t.AspectRatio, CenterY + SemiHeight - Radius, 2.0 * Radius * t.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, t, Needle);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
									Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
									Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
									double c0 = Angle + Math.PI;
									double c1 = 2.0 * (Math.PI - Angle) / Maximum;
									Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, "speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
								}
								else if (Type == 1)
								{
									// led
									if (!NeedleOverridden) Needle = Color32.Black;
									int j = CreateElement(Car, CenterX - Radius, CenterY + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, null, Needle);
									double x0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
									double y0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
									double z0 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
									double x1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
									double y1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
									double z1 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
									double x2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
									double y2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
									double z2 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
									double x3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
									double y3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
									double z3 = Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
									double cx = 0.25 * (x0 + x1 + x2 + x3);
									double cy = 0.25 * (y0 + y1 + y2 + y3);
									double cz = 0.25 * (z0 + z1 + z2 + z3);
									VertexTemplate[] vertices = new VertexTemplate[11];
									for (int v = 0; v < 11; v++)
									{
										//The verticies are transformed by the LED function, so must be created here at zero
										vertices[v] = new Vertex();
									}

									int[][] faces =
									{
										new[] {0, 1, 2},
										new[] {0, 3, 4},
										new[] {0, 5, 6},
										new[] {0, 7, 8},
										new[] {0, 9, 10}
									};
									Car.CarSections[0].Groups[0].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, Needle);
									Car.CarSections[0].Groups[0].Elements[j].LEDClockwiseWinding = true;
									Car.CarSections[0].Groups[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
									Car.CarSections[0].Groups[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
									Car.CarSections[0].Groups[0].Elements[j].LEDVectors = new[]
									{
										new Vector3(x0, y0, z0),
										new Vector3(x1, y1, z1),
										new Vector3(x2, y2, z2),
										new Vector3(x3, y3, z3),
										new Vector3(cx, cy, cz)
									};
									double c0 = Angle;
									double c1 = 2.0 * (Math.PI - Angle) / Maximum;
									Car.CarSections[0].Groups[0].Elements[j].LEDFunction = new FunctionScript(Plugin.currentHost, "speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
								}
							}
								break;
							// digitalindicator
							case "digitalindicator":
							case "デジタル速度計":
							{
								string Number = null;
								double CornerX = 0.0, CornerY = 0.0;
								int Width = 0, Height = 0;
								double UnitFactor = 3.6;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "number":
											case "数字":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Number = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Number))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + Number + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Number = null;
													}
												}

												break;
											case "corner":
											case "左上":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerY = 0.0;
												}

												break;
											case "size":
											case "サイズ":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Width))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Width = 0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out Height))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Height = 0;
												}

												break;
											case "unit":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0)
												{
													string a = Arguments[0].ToLowerInvariant();
													int Unit;
													switch (a)
													{
														case "km/h":
															Unit = 0;
															break;
														case "mph":
															Unit = 1;
															break;
														case "m/s":
															Unit = 2;
															break;
														default:
															if (!NumberFormats.TryParseIntVb6(Arguments[0], out Unit))
															{
																Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																Unit = 0;
															}

															break;
													}

													if (Unit < 0 | Unit > 2)
													{
														Plugin.currentHost.AddMessage(MessageType.Error, false, "Value must be between 0 and 2 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Unit = 0;
													}

													if (Unit == 1)
													{
														UnitFactor = 2.2369362920544;
													}
													else if (Unit == 2)
													{
														UnitFactor = 1.0;
													}
													else
													{
														UnitFactor = 3.6;
													}
												}

												break;
										}
									}

									i++;
								}

								i--;
								if (Number == null)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Number is required to be specified in " + Section + " in " + FileName);
								}

								if (Width <= 0)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
								}

								if (Height <= 0)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
								}

								if (Number != null & Width > 0 & Height > 0)
								{
									Plugin.currentHost.QueryTextureDimensions(Number, out var w, out var h);
									if (w > 0 & h > 0)
									{
										//Generate an error message rather than crashing if the clip region is invalid
										if (Width > w)
										{
											Width = w;
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Clip region width was greater than the texture width " + Section + " in " + FileName);
										}

										if (Height > h)
										{
											Height = h;
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Clip region height was greater than the texture height " + Section + " in " + FileName);
										}

										int n = h / Height;
										Texture[] t = new Texture[n];
										for (int j = 0; j < n; j++)
										{
											Plugin.currentHost.RegisterTexture(Number, new TextureParameters(new TextureClipRegion(w - Width, j * Height, Width, Height), Color24.Blue), out t[j]);
										}

										{
											// hundreds
											int k = -1;
											for (int j = 0; j < n; j++)
											{
												if (j == 0)
												{
													k = CreateElement(Car, CornerX, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White);
												}
												else
												{
													CreateElement(Car, CornerX, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White, true);
												}
											}

											Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 100 >= <> 100 quotient 10 mod 10 ?", false);
										}
										{
											// tens
											int k = -1;
											for (int j = 0; j < n; j++)
											{
												if (j == 0)
												{
													k = CreateElement(Car, CornerX + Width, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White);
												}
												else
												{
													CreateElement(Car, CornerX + Width, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White, true);
												}
											}

											Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 10 >= <> 10 quotient 10 mod 10 ?", false);
										}
										{
											// ones
											int k = -1;
											for (int j = 0; j < n; j++)
											{
												if (j == 0)
												{
													k = CreateElement(Car, CornerX + 2.0 * Width, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White);
												}
												else
												{
													CreateElement(Car, CornerX + 2.0 * Width, CornerY + SemiHeight, Width, Height, WorldZ + EyeDistance - 7.0 * StackDistance, t[j], Color32.White, true);
												}
											}

											Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * floor 10 mod", false);
										}
									}
								}
							}
								break;
							// pilotlamp
							case "pilotlamp":
							case "知らせ灯":
							{
								double CornerX = 0.0, CornerY = 0.0;
								string TurnOn = null, TurnOff = null;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "turnon":
											case "点灯":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													TurnOn = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(TurnOn))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName" + TurnOn + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														TurnOn = null;
													}
												}

												break;
											case "turnoff":
											case "消灯":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													TurnOff = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(TurnOff))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName" + TurnOff + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														TurnOff = null;
													}
												}

												break;
											case "corner":
											case "左上":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerY = 0.0;
												}

												break;
										}
									}

									i++;
								}

								i--;
								if (TurnOn != null & TurnOff != null)
								{
									Plugin.currentHost.RegisterTexture(TurnOn, new TextureParameters(null, Color24.Blue), out var t0, true);
									Plugin.currentHost.RegisterTexture(TurnOff, new TextureParameters(null, Color24.Blue), out var t1, true);
									int j = CreateElement(Car, CornerX, CornerY + SemiHeight, WorldZ + EyeDistance - 2.0 * StackDistance, t0);
									CreateElement(Car, CornerX, CornerY + SemiHeight, WorldZ + EyeDistance - 2.0 * StackDistance, t1, true);
									Car.CarSections[0].Groups[0].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, "doors 0 !=", false);
								}
							}
								break;
							// watch
							case "watch":
							case "時計":
							{
								Color32 Needle = Color32.Black;
								double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
								string Background = null;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "background":
											case "背景":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Background = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Background))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName" + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Background = null;
													}
												}

												break;
											case "needle":
											case "hand":
											case "針":
											{
												int r = 0, g = 0, b = 0;
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = 0;
												}
												else if (r < 0 | r > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													r = r < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = 0;
												}
												else if (g < 0 | g > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													g = g < 0 ? 0 : 255;
												}

												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = 0;
												}
												else if (b < 0 | b > 255)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													b = b < 0 ? 0 : 255;
												}

												Needle = new Color32((byte) r, (byte) g, (byte) b, 255);
											}
												break;
											case "center":
											case "中心":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CenterY = 0.0;
												}

												break;
											case "radius":
											case "半径":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Radius = 16.0;
												}

												break;
										}
									}

									i++;
								}

								i--;
								if (Background != null)
								{
									Plugin.currentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var t, true);
									CreateElement(Car, CenterX - 0.5 * t.Width, CenterY + SemiHeight - 0.5 * t.Height, WorldZ + EyeDistance - 3.0 * StackDistance, t);
								}

								string Folder = Plugin.FileSystem.GetDataFolder("Compatibility");
								{
									// hour
									string File = Path.CombineFile(Folder, "needle_hour.png");
									Plugin.currentHost.RegisterTexture(File, new TextureParameters(null, null), out var t, true);
									int j = CreateElement(Car, CenterX - Radius * t.AspectRatio, CenterY + SemiHeight - Radius, 2.0 * Radius * t.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - 4.0 * StackDistance, t, Needle);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
									Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
									Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
									Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, "time 0.000277777777777778 * floor 0.523598775598298 *", false);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDamping = new Damping(20.0, 0.4);
								}
								{
									// minute
									string File = Path.CombineFile(Folder, "needle_minute.png");
									Plugin.currentHost.RegisterTexture(File, new TextureParameters(null, null), out var t, true);
									int j = CreateElement(Car, CenterX - Radius * t.AspectRatio, CenterY + SemiHeight - Radius, 2.0 * Radius * t.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, t, Needle);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
									Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
									Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
									Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, "time 0.0166666666666667 * floor 0.10471975511966 *", false);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDamping = new Damping(20.0, 0.4);
								}
								{
									// second
									string File = Path.CombineFile(Folder, "needle_second.png");
									Plugin.currentHost.RegisterTexture(File, new TextureParameters(null, null), out var t, true);
									int j = CreateElement(Car, CenterX - Radius * t.AspectRatio, CenterY + SemiHeight - Radius, 2.0 * Radius * t.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - 6.0 * StackDistance, t, Needle);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
									Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
									Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
									Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, "time floor 0.10471975511966 *", false);
									Car.CarSections[0].Groups[0].Elements[j].RotateZDamping = new Damping(20.0, 0.4);
								}
							}
								break;
							// brakeindicator
							case "brakeindicator":
							case "ハンドルの段表示":
							{
								double CornerX = 0.0, CornerY = 0.0;
								string Image = null;
								int Width = 0;
								i++;
								while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)))
								{
									int j = Lines[i].IndexOf('=');
									if (j >= 0)
									{
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										string[] Arguments = GetArguments(Value);
										switch (Key.ToLowerInvariant())
										{
											case "image":
											case "画像":
												if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
												if (Path.ContainsInvalidChars(Value))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												}
												else
												{
													Image = Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Image))
													{
														Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + Image + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Image = null;
													}
												}

												break;
											case "corner":
											case "左上":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerX = 0.0;
												}
												else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													CornerY = 0.0;
												}

												break;
											case "width":
											case "幅":
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Width))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Width = 1;
												}
												else if (Width <= 0)
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is expected to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													Width = 1;
												}

												break;
										}
									}

									i++;
								}

								i--;
								if (Image == null)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Image is required to be specified in " + Section + " in " + FileName);
								}

								if (Width <= 0)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
								}

								if (Image != null & Width > 0)
								{
									Plugin.currentHost.QueryTextureDimensions(Image, out var w, out var h);
									if (w > 0 & h > 0)
									{
										int n = w / Width;
										int k = -1;
										for (int j = 0; j < n; j++)
										{
											TextureClipRegion clip = new TextureClipRegion(j * Width, 0, Width, h);
											Plugin.currentHost.RegisterTexture(Image, new TextureParameters(clip, Color24.Blue), out var t);
											if (j == 0)
											{
												k = CreateElement(Car, CornerX, CornerY + SemiHeight, Width, h, WorldZ + EyeDistance - StackDistance, t, Color32.White);
											}
											else
											{
												CreateElement(Car, CornerX, CornerY + SemiHeight, Width, h, WorldZ + EyeDistance - StackDistance, t, Color32.White, true);
											}
										}

										if (Car.baseTrain.Handles.Brake is AirBrakeHandle)
										{
											int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
											int em = maxpow + 3;
											Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > " + maxpow.ToString(Culture) + " BrakeNotch + " + maxpow.ToString(Culture) + " powerNotch - ? ?", false);
										}
										else
										{
											if (Car.baseTrain.Handles.HasHoldBrake)
											{
												int em = Car.baseTrain.Handles.Power.MaximumNotch + 2 + Car.baseTrain.Handles.Brake.MaximumNotch;
												int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
												int maxpowp1 = maxpow + 1;
												Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "emergencyBrake " + em.ToString(Culture) + " holdBrake " + maxpowp1.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpowp1.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ? ?", false);
											}
											else
											{
												int em = Car.baseTrain.Handles.Power.MaximumNotch + 1 + Car.baseTrain.Handles.Brake.MaximumNotch;
												int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
												Car.CarSections[0].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.currentHost, "emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpow.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ?", false);
											}
										}
									}
								}
							}
								break;
						}
					}
				}
			}
		}

		// get arguments
		private static string[] GetArguments(string Expression)
		{
			string[] Arguments = new string[16];
			int UsedArguments = 0;
			int Start = 0;
			for (int i = 0; i < Expression.Length; i++)
			{
				if (Expression[i] == ',' | Expression[i] == ':')
				{
					if (UsedArguments >= Arguments.Length) Array.Resize(ref Arguments, Arguments.Length << 1);
					Arguments[UsedArguments] = Expression.Substring(Start, i - Start).TrimStart();
					UsedArguments++;
					Start = i + 1;
				}
				else if (Expression[i] == ';')
				{
					if (UsedArguments >= Arguments.Length) Array.Resize(ref Arguments, Arguments.Length << 1);
					Arguments[UsedArguments] = Expression.Substring(Start, i - Start).TrimStart();
					UsedArguments++;
					Start = Expression.Length;
					break;
				}
			}

			if (Start < Expression.Length)
			{
				if (UsedArguments >= Arguments.Length) Array.Resize(ref Arguments, Arguments.Length << 1);
				Arguments[UsedArguments] = Expression.Substring(Start).Trim();
				UsedArguments++;
			}

			Array.Resize(ref Arguments, UsedArguments);
			return Arguments;
		}

		private int CreateElement(CarBase Car, double Left, double Top, double WorldZ, Texture Texture, bool AddStateToLastElement = false)
		{
			return CreateElement(Car, Left, Top, Texture.Width, Texture.Height, WorldZ, Texture, Color32.White, AddStateToLastElement);
		}

		// create element
		private int CreateElement(CarBase Car, double Left, double Top, double Width, double Height, double WorldZ, Texture Texture, Color32 Color, bool AddStateToLastElement = false)
		{
			// create object
			StaticObject Object = new StaticObject(Plugin.currentHost);
			Vector3[] v = new Vector3[4];
			double sx = 0.5 * WorldWidth * Width / FullWidth;
			double sy = 0.5 * WorldHeight * Height / FullHeight;
			v[0] = new Vector3(-sx, -sy, 0);
			v[1] = new Vector3(-sx, sy, 0);
			v[2] = new Vector3(sx, sy, 0);
			v[3] = new Vector3(sx, -sy, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			Object.Mesh.Vertices = new VertexTemplate[] {t0, t1, t2, t3};
			Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 0, 2, 3 }, FaceFlags.Triangles) }; //Must create as a single face like this to avoid Z-sort issues with overlapping bits
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = new MaterialFlags();
			if (Texture != null)
			{
				Object.Mesh.Materials[0].Flags |= MaterialFlags.TransparentColor;
			}

			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = Texture;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = WorldLeft + sx + WorldWidth * Left / FullWidth;
			o.Y = WorldTop - sy - WorldHeight * Top / FullHeight;
			o.Z = WorldZ;
			// add object
			if (AddStateToLastElement)
			{
				int n = Car.CarSections[0].Groups[0].Elements.Length - 1;
				int j = Car.CarSections[0].Groups[0].Elements[n].States.Length;
				Array.Resize(ref Car.CarSections[0].Groups[0].Elements[n].States, j + 1);
				Car.CarSections[0].Groups[0].Elements[n].States[j] = new ObjectState
				{
					Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z),
					Prototype = Object
				};
				return n;
			}
			else
			{
				int n = Car.CarSections[0].Groups[0].Elements.Length;
				Array.Resize(ref Car.CarSections[0].Groups[0].Elements, n + 1);
				Car.CarSections[0].Groups[0].Elements[n] = new AnimatedObject(Plugin.currentHost);
				Car.CarSections[0].Groups[0].Elements[n].States = new[] {new ObjectState()};
				Car.CarSections[0].Groups[0].Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Car.CarSections[0].Groups[0].Elements[n].States[0].Prototype = Object;
				Car.CarSections[0].Groups[0].Elements[n].CurrentState = 0;
				Car.CarSections[0].Groups[0].Elements[n].internalObject = new ObjectState(Object);
				Plugin.currentHost.CreateDynamicObject(ref Car.CarSections[0].Groups[0].Elements[n].internalObject);
				return n;
			}
		}

	}
}
