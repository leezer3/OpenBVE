using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	internal static class PanelCfgParser {

		// constants
		private static double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed</remarks>
		private const double EyeDistance = 1.0;

		/// <summary>Parses a BVE1 panel.cfg file</summary>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Train">The train</param>
		internal static void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg");
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).TrimEnd();
				}
			}
			// initialize
			double FullWidth = 480, FullHeight = 440, SemiHeight = 240;
			double AspectRatio = FullWidth / FullHeight;
			double WorldWidth, WorldHeight;
			if (Screen.Width >= Screen.Height) {
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / AspectRatio;
			} else {
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance;
				WorldWidth = WorldHeight * AspectRatio;
			}
			World.CameraRestrictionBottomLeft = new Vector3(-0.5 * WorldWidth, -0.5 * WorldHeight, EyeDistance);
			World.CameraRestrictionTopRight = new Vector3(0.5 * WorldWidth, 0.5 * WorldHeight, EyeDistance);
			double WorldLeft = Train.Cars[Train.DriverCar].Driver.X - 0.5 * WorldWidth;
			double WorldTop = Train.Cars[Train.DriverCar].Driver.Y + 0.5 * WorldHeight;
			double WorldZ = Train.Cars[Train.DriverCar].Driver.Z;
			const double UpDownAngleConstant = -0.191986217719376;
			double PanelYaw = 0.0;
			double PanelPitch = UpDownAngleConstant;
			string PanelBackground = OpenBveApi.Path.CombineFile(TrainPath, "panel.bmp");
			// parse lines for panel and view
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length > 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// panel
							case "panel":
								i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									int j = Lines[i].IndexOf('=');
									if (j >= 0) {
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant()) {
											case "background":
												if (Path.ContainsInvalidChars(Value)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
												} else {
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													PanelBackground = OpenBveApi.Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(PanelBackground)) {
														Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + PanelBackground + "could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													}
												}
												break;
										}
									} i++;
								} i--; break;
								// view
							case "view":
								i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
									int j = Lines[i].IndexOf('=');
									if (j >= 0) {
										string Key = Lines[i].Substring(0, j).TrimEnd();
										string Value = Lines[i].Substring(j + 1).TrimStart();
										switch (Key.ToLowerInvariant()) {
											case "yaw":
												{
													double yaw = 0.0;
													if (Value.Length > 0 && !NumberFormats.TryParseDoubleVb6(Value, out yaw)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														yaw = 0.0;
													}
													PanelYaw = Math.Atan(yaw);
												} break;
											case "pitch":
												{
													double pitch = 0.0;
													if (Value.Length > 0 && !NumberFormats.TryParseDoubleVb6(Value, out pitch)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														pitch = 0.0;
													}
													PanelPitch = Math.Atan(pitch) + UpDownAngleConstant;
												} break;
										}
									} i++;
								} i--; break;
						}
					}
				}
			}
			Train.Cars[Train.DriverCar].DriverYaw = PanelYaw;
			Train.Cars[Train.DriverCar].DriverPitch = PanelPitch;
			// panel
			{
				if (!System.IO.File.Exists(PanelBackground)) {
					Interface.AddMessage(Interface.MessageType.Error, true, "The panel image could not be found in " + FileName);
				} else {
					Textures.Texture t;
					Textures.RegisterTexture(PanelBackground, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
					OpenBVEGame.RunInRenderThread(() =>
					{
						Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp); 
					});
					double w = (double)t.Width;
					double h = (double)t.Height;
					SemiHeight = FullHeight - h;
					CreateElement(Train, 0, SemiHeight, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
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
				if (Lines[i].Length != 0) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						string Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim();
						switch (Section.ToLowerInvariant()) {
								// pressuregauge
							case "pressuregauge":
							case "pressuremeter":
							case "pressureindicator":
							case "圧力計":
								{
									int Type = 0;
									Color32[] NeedleColor = new Color32[] { new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 255) };
									int[] NeedleType = new int[] { 0, 0 };
									double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
									string Background = null, Cover = null;
									double Angle = 0.785398163397449, Minimum = 0.0, Maximum = 1000.0;
									double UnitFactor = 1000.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "type":
												case "形態":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Type)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Type is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Type = 0;
													} else if (Type != 0 & Type != 1) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Type must be either 0 or 1 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Type = 0;
													} break;
												case "lowerneedle":
												case "lowerhand":
												case "下針":
												case "upperneedle":
												case "upperhand":
												case "上針":
													int k = Key.ToLowerInvariant() == "lowerneedle" | Key.ToLowerInvariant() == "lowerhand" | Key == "下針" ? 0 : 1;
													if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
														switch (Arguments[0].ToLowerInvariant()) {
															case "bc":
															case "ブレーキシリンダ":
																NeedleType[k] = 1; break;
															case "sap":
															case "直通管":
																NeedleType[k] = 2; break;
															case "bp":
															case "ブレーキ管":
															case "制動管":
																NeedleType[k] = 3; break;
															case "er":
															case "釣り合い空気溜め":
															case "釣り合い空気ダメ":
															case "つりあい空気溜め":
															case "ツリアイ空気ダメ":
																NeedleType[k] = 4; break;
															case "mr":
															case "元空気溜め":
															case "元空気ダメ":
																NeedleType[k] = 5; break;
															default:
																{
																	int a; if (!NumberFormats.TryParseIntVb6(Arguments[0], out a)) {
																		Interface.AddMessage(Interface.MessageType.Error, false, "Subject is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																		a = 0;
																	}
																	NeedleType[k] = a;
																} break;
														}
													}
													int r = 0, g = 0, b = 0;
													if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out r)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														r = 0;
													} else if (r < 0 | r > 255) {
														Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														r = r < 0 ? 0 : 255;
													}
													if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out g)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + Key + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														g = 0;
													} else if (g < 0 | g > 255) {
														Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														g = g < 0 ? 0 : 255;
													}
													if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out b)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + Key + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														b = 0;
													} else if (b < 0 | b > 255) {
														Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														b = b < 0 ? 0 : 255;
													}
													NeedleColor[k] = new Color32((byte)r, (byte)g, (byte)b, 255);
													break;
												case "center":
												case "中心":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterY = 0.0;
													} break;
												case "radius":
												case "半径":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 1.0;
													} break;
												case "background":
												case "背景":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Background = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Background)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Background = null;
														}
													}
													break;
												case "cover":
												case "ふた":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Cover = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Cover)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + Cover + "could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Cover = null;
														}
													}
													break;
												case "unit":
												case "単位":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
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
																Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
																break;
														}
													} break;
												case "maximum":
												case "最大":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Maximum)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "PressureValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Maximum = 1000.0;
													} break;
												case "minimum":
												case "最小":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Minimum)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "PressureValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Minimum = 0.0;
													} break;
												case "angle":
												case "角度":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Angle)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Angle = 0.785398163397449;
													} else {
														Angle *= 0.0174532925199433;
													} break;
											}
										} i++;
									} i--;
									// units
									Minimum *= UnitFactor;
									Maximum *= UnitFactor;
									// background
									if (Background != null) {
										Textures.Texture t;
										Textures.RegisterTexture(Background, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										CreateElement(Train, CenterX - 0.5 * w, CenterY + SemiHeight - 0.5 * h, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 3.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
									}
									// cover
									if (Cover != null) {
										Textures.Texture t;
										Textures.RegisterTexture(Cover, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										CreateElement(Train, CenterX - 0.5 * w, CenterY + SemiHeight - 0.5 * h, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 6.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
									}
									if (Type == 0) {
										// needles
										for (int k = 0; k < 2; k++) {
											if (NeedleType[k] != 0) {
												string Folder = Program.FileSystem.GetDataFolder("Compatibility");
												string File = OpenBveApi.Path.CombineFile(Folder, k == 0 ? "needle_pressuregauge_lower.png" : "needle_pressuregauge_upper.png");
												Textures.Texture t;
												Textures.RegisterTexture(File, out t);
												OpenBVEGame.RunInRenderThread(() =>
												{
													Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
												});
												double w = (double)t.Width;
												double h = (double)t.Height;
												int j = CreateElement(Train, CenterX - Radius * w / h, CenterY + SemiHeight - Radius, 2.0 * Radius * w / h, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - (double)(4 + k) * StackDistance, Train.Cars[Train.DriverCar].Driver, t, NeedleColor[k], false);
												Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
												Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
												Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
												double c0 = (Angle * (Maximum - Minimum) - 2.0 * Minimum * Math.PI) / (Maximum - Minimum) + Math.PI;
												double c1 = 2.0 * (Math.PI - Angle) / (Maximum - Minimum);
												string Variable = "0";
												switch (NeedleType[k]) {
														case 1: Variable = "brakecylinder"; break;
														case 2: Variable = "straightairpipe"; break;
														case 3: Variable = "brakepipe"; break;
														case 4: Variable = "equalizingreservoir"; break;
														case 5: Variable = "mainreservoir"; break;
												}
												Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma");
											}
										}
									} else if (Type == 1) {
										// leds
										if (NeedleType[1] != 0) {
											int j = CreateElement(Train, CenterX - Radius, CenterY + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 5.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, null, NeedleColor[1], false);
											double x0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.X;
											double y0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Y;
											double z0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Z;
											double x1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.X;
											double y1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Y;
											double z1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Z;
											double x2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.X;
											double y2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Y;
											double z2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Z;
											double x3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.X;
											double y3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Y;
											double z3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Z;
											double cx = 0.25 * (x0 + x1 + x2 + x3);
											double cy = 0.25 * (y0 + y1 + y2 + y3);
											double cz = 0.25 * (z0 + z1 + z2 + z3);
											World.Vertex[] vertices = new World.Vertex[11];
											int[][] faces = new int[][] {
												new int[] { 0, 1, 2 },
												new int[] { 0, 3, 4 },
												new int[] { 0, 5, 6 },
												new int[] { 0, 7, 8 },
												new int[] { 0, 9, 10 }
											};
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh = new World.Mesh(vertices, faces, NeedleColor[1]);
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDClockwiseWinding = true;
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDVectors = new Vector3[] {
												new Vector3(x0, y0, z0),
												new Vector3(x1, y1, z1),
												new Vector3(x2, y2, z2),
												new Vector3(x3, y3, z3),
												new Vector3(cx, cy, cz)
											};
											double c0 = (Angle * (Maximum - Minimum) - 2.0 * Minimum * Math.PI) / (Maximum - Minimum);
											double c1 = 2.0 * (Math.PI - Angle) / (Maximum - Minimum);
											string Variable;
											switch (NeedleType[1]) {
													case 1: Variable = "brakecylinder"; break;
													case 2: Variable = "straightairpipe"; break;
													case 3: Variable = "brakepipe"; break;
													case 4: Variable = "equalizingreservoir"; break;
													case 5: Variable = "mainreservoir"; break;
													default: Variable = "0"; break;
											}
											Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma");
										}
									}
								} break;
								// speedometer
							case "speedometer":
							case "speedindicator":
							case "速度計":
								{
									int Type = 0;
									Color32 Needle = new Color32(255, 255, 255, 255);
									bool NeedleOverridden = false;
									double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
									string Background = null, Cover = null, Atc = null;
									double Angle = 1.0471975511966, Maximum = 33.3333333333333, AtcRadius = 0.0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "type":
												case "形態":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Type)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Type = 0;
													} else if (Type != 0 & Type != 1) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Value must be either 0 or 1 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Type = 0;
													} break;
												case "background":
												case "背景":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Background = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Background)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Background = null;
														}
													}
													break;
												case "needle":
												case "hand":
												case "針":
													{
														int r = 0, g = 0, b = 0;
														if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															r = 255;
														} else if (r < 0 | r > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															r = r < 0 ? 0 : 255;
														}
														if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															g = 255;
														} else if (g < 0 | g > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															g = g < 0 ? 0 : 255;
														}
														if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															b = 255;
														} else if (b < 0 | b > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															b = b < 0 ? 0 : 255;
														}
														Needle = new Color32((byte)r, (byte)g, (byte)b, 255);
														NeedleOverridden = true;
													} break;
												case "cover":
												case "ふた":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													Cover = OpenBveApi.Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Cover)) {
														Interface.AddMessage(Interface.MessageType.Error, true, "FileName" + Cover + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Cover = null;
													} break;
												case "atc":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													Atc = OpenBveApi.Path.CombineFile(TrainPath, Value);
													if (!System.IO.File.Exists(Atc)) {
														Interface.AddMessage(Interface.MessageType.Error, true, "FileName" + Atc + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Atc = null;
													} break;
												case "atcradius":
												case "atc半径":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out AtcRadius)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														AtcRadius = 0.0;
													} break;
												case "center":
												case "中心":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterY = 0.0;
													} break;
												case "radius":
												case "半径":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 0.0;
													} break;
												case "angle":
												case "角度":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Angle)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Angle = 1.0471975511966;
													} else {
														Angle *= 0.0174532925199433;
													} break;
												case "maximum":
												case "最大":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Maximum)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "SpeedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Maximum = 33.3333333333333;
													} else {
														Maximum *= 0.277777777777778;
													} break;
											}
										} i++;
									} i--;
									if (Background != null) {
										// background/led
										Textures.Texture t;
										Textures.RegisterTexture(Background, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										CreateElement(Train, CenterX - 0.5 * w, CenterY + SemiHeight - 0.5 * h, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 3.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
									}
									if (Cover != null) {
										// cover
										Textures.Texture t;
										Textures.RegisterTexture(Cover, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										CreateElement(Train, CenterX - 0.5 * w, CenterY + SemiHeight - 0.5 * h, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 6.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
									}
									if (Atc != null) {
										// atc
										int w, h;
										Program.CurrentHost.QueryTextureDimensions(Atc, out w, out h);
										if (w > 0 & h > 0) {
											int n = w / h;
											int k = -1;
											for (int j = 0; j < n; j++) {
												double s; switch (j) {
														case 1: s = 0.0; break;
														case 2: s = 15.0; break;
														case 3: s = 25.0; break;
														case 4: s = 45.0; break;
														case 5: s = 55.0; break;
														case 6: s = 65.0; break;
														case 7: s = 75.0; break;
														case 8: s = 90.0; break;
														case 9: s = 100.0; break;
														case 10: s = 110.0; break;
														case 11: s = 120.0; break;
														default: s = -1.0; break;
												} s *= 0.277777777777778;
												double a; if (s >= 0.0) {
													a = 2.0 * s * (Math.PI - Angle) / Maximum + Angle + Math.PI;
												} else {
													a = Math.PI;
												}
												double x = CenterX - 0.5 * h + Math.Sin(a) * AtcRadius;
												double y = CenterY - 0.5 * h - Math.Cos(a) * AtcRadius + SemiHeight;
												Textures.Texture t;
												Textures.RegisterTexture(Atc, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(j * h, 0, h, h), Color24.Blue), out t);
												OpenBVEGame.RunInRenderThread(() =>
												{
													Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
												});
												if (j == 0) {
													k = CreateElement(Train, x, y, (double)h, (double)h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 4.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
												} else {
													CreateElement(Train, x, y, (double)h, (double)h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 4.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), true);
												}
											}
											Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("271 pluginstate");
										}
									}
									if (Type == 0) {
										// needle
										string Folder = Program.FileSystem.GetDataFolder("Compatibility");
										string File = OpenBveApi.Path.CombineFile(Folder, "needle_speedometer.png");
										Textures.Texture t;
										Textures.RegisterTexture(File, out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										int j = CreateElement(Train, CenterX - Radius * w / h, CenterY + SemiHeight - Radius, 2.0 * Radius * w / h, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 5.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, Needle, false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
										double c0 = Angle + Math.PI;
										double c1 = 2.0 * (Math.PI - Angle) / Maximum;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma");
									} else if (Type == 1) {
										// led
										if (!NeedleOverridden) Needle = new Color32(0, 0, 0, 255);
										int j = CreateElement(Train, CenterX - Radius, CenterY + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 5.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, null, Needle, false);
										double x0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.X;
										double y0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Y;
										double z0 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Z;
										double x1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.X;
										double y1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Y;
										double z1 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Z;
										double x2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.X;
										double y2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Y;
										double z2 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Z;
										double x3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.X;
										double y3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Y;
										double z3 = Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Z;
										double cx = 0.25 * (x0 + x1 + x2 + x3);
										double cy = 0.25 * (y0 + y1 + y2 + y3);
										double cz = 0.25 * (z0 + z1 + z2 + z3);
										World.Vertex[] vertices = new World.Vertex[11];
										int[][] faces = new int[][] {
											new int[] { 0, 1, 2 },
											new int[] { 0, 3, 4 },
											new int[] { 0, 5, 6 },
											new int[] { 0, 7, 8 },
											new int[] { 0, 9, 10 }
										};
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].States[0].Object.Mesh = new World.Mesh(vertices, faces, Needle);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDClockwiseWinding = true;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDVectors = new Vector3[] {
											new Vector3(x0, y0, z0),
											new Vector3(x1, y1, z1),
											new Vector3(x2, y2, z2),
											new Vector3(x3, y3, z3),
											new Vector3(cx, cy, cz)
										};
										double c0 = Angle;
										double c1 = 2.0 * (Math.PI - Angle) / Maximum;
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].LEDFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma");
									}
								} break;
								// digitalindicator
							case "digitalindicator":
							case "デジタル速度計":
								{
									string Number = null;
									double CornerX = 0.0, CornerY = 0.0;
									int Width = 0, Height = 0;
									double UnitFactor = 3.6;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "number":
												case "数字":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Number = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Number)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + Number + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Number = null;
														}
													}
													break;
												case "corner":
												case "左上":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerY = 0.0;
													} break;
												case "size":
												case "サイズ":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Width)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Width is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Width = 0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out Height)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Height = 0;
													} break;
												case "unit":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
														string a = Arguments[0].ToLowerInvariant();
														int Unit = 0;
														if (a == "km/h") {
															Unit = 0;
														} else if (a == "mph") {
															Unit = 1;
														} else if (a == "m/s") {
															Unit = 2;
														} else if (!NumberFormats.TryParseIntVb6(Arguments[0], out Unit)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Unit = 0;
														} else if (Unit < 0 | Unit > 2) {
															Interface.AddMessage(Interface.MessageType.Error, false, "Value must be between 0 and 2 in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Unit = 0;
														}
														if (Unit == 1) {
															UnitFactor = 2.2369362920544;
														} else if (Unit == 2) {
															UnitFactor = 1.0;
														} else {
															UnitFactor = 3.6;
														}
													} break;
											}
										} i++;
									} i--;
									if (Number == null) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Number is required to be specified in " + Section + " in " + FileName);
									}
									if (Width <= 0) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
									}
									if (Height <= 0) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
									}
									if (Number != null & Width > 0 & Height > 0) {
										int w, h;
										Program.CurrentHost.QueryTextureDimensions(Number, out w, out h);
										if (w > 0 & h > 0) {
											//Generate an error message rather than crashing if the clip region is invalid
											if (Width > w)
											{
												Width = w;
												Interface.AddMessage(Interface.MessageType.Warning, false, "Clip region width was greater than the texture width " + Section + " in " + FileName);
											}
											if (Height > h)
											{
												Height = h;
												Interface.AddMessage(Interface.MessageType.Warning, false, "Clip region height was greater than the texture height " + Section + " in " + FileName);
											}
											int n = h / Height;
											Textures.Texture[] t = new Textures.Texture[n];
											for (int j = 0; j < n; j++) {
												Textures.RegisterTexture(Number, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(w - Width, j * Height, Width, Height), Color24.Blue), out t[j]);
												//TextureManager.UseTexture(t[j], TextureManager.UseMode.Normal);
											}
											{ // hundreds
												int k = -1;
												for (int j = 0; j < n; j++) {
													if (j == 0) {
														k = CreateElement(Train, CornerX, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), false);
													} else {
														CreateElement(Train, CornerX, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), true);
													}
												}
												Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 100 >= <> 100 quotient 10 mod 10 ?");
											}
											{ // tens
												int k = -1;
												for (int j = 0; j < n; j++) {
													if (j == 0) {
														k = CreateElement(Train, CornerX + (double)Width, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), false);
													} else {
														CreateElement(Train, CornerX + (double)Width, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), true);
													}
												}
												Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 10 >= <> 10 quotient 10 mod 10 ?");
											}
											{ // ones
												int k = -1;
												for (int j = 0; j < n; j++) {
													if (j == 0) {
														k = CreateElement(Train, CornerX + 2.0 * (double)Width, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), false);
													} else {
														CreateElement(Train, CornerX + 2.0 * (double)Width, CornerY + SemiHeight, (double)Width, (double)Height, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 7.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t[j], new Color32(255, 255, 255, 255), true);
													}
												}
												Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("speedometer abs " + UnitFactor.ToString(Culture) + " * floor 10 mod");
											}
										}
									}
								} break;
								// pilotlamp
							case "pilotlamp":
							case "知らせ灯":
								{
									double CornerX = 0.0, CornerY = 0.0;
									string TurnOn = null, TurnOff = null;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "turnon":
												case "点灯":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														TurnOn = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(TurnOn)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName" + TurnOn + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															TurnOn = null;
														}
													}
													break;
												case "turnoff":
												case "消灯":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														TurnOff = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(TurnOff)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName" + TurnOff + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															TurnOff = null;
														}
													}
													break;
												case "corner":
												case "左上":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerY = 0.0;
													} break;
											}
										} i++;
									} i--;
									if (TurnOn != null & TurnOff != null) {
										Textures.Texture t0, t1;
										Textures.RegisterTexture(TurnOn, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t0);
										Textures.RegisterTexture(TurnOff, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t1);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t0, Textures.OpenGlTextureWrapMode.ClampClamp);
											Textures.LoadTexture(t1, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t0.Width;
										double h = (double)t0.Height;
										int j = CreateElement(Train, CornerX, CornerY + SemiHeight, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 2.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t0, new Color32(255, 255, 255, 255), false);
										w = (double)t1.Width;
										h = (double)t1.Height;
										CreateElement(Train, CornerX, CornerY + SemiHeight, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 2.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t1, new Color32(255, 255, 255, 255), true);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("doors 0 !=");
									}
								} break;
								// watch
							case "watch":
							case "時計":
								{
									Color32 Needle = new Color32(0, 0, 0, 255);
									double CenterX = 0.0, CenterY = 0.0, Radius = 16.0;
									string Background = null;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "background":
												case "背景":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Background = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Background)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName" + Background + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Background = null;
														}
													}
													break;
												case "needle":
												case "hand":
												case "針":
													{
														int r = 0, g = 0, b = 0;
														if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															r = 0;
														} else if (r < 0 | r > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															r = r < 0 ? 0 : 255;
														}
														if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															g = 0;
														} else if (g < 0 | g > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															g = g < 0 ? 0 : 255;
														}
														if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
															Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															b = 0;
														} else if (b < 0 | b > 255) {
															Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															b = b < 0 ? 0 : 255;
														}
														Needle = new Color32((byte)r, (byte)g, (byte)b, 255);
													} break;
												case "center":
												case "中心":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CenterX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CenterY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CenterY = 0.0;
													} break;
												case "radius":
												case "半径":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Radius = 16.0;
													} break;
											}
										} i++;
									} i--;
									if (Background != null) {
										Textures.Texture t;
										Textures.RegisterTexture(Background, new OpenBveApi.Textures.TextureParameters(null, Color24.Blue), out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										CreateElement(Train, CenterX - 0.5 * w, CenterY + SemiHeight - 0.5 * h, w, h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 3.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
									}
									string Folder = Program.FileSystem.GetDataFolder("Compatibility");
									{ // hour
										string File = OpenBveApi.Path.CombineFile(Folder, "needle_hour.png");
										Textures.Texture t;
										Textures.RegisterTexture(File, out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										int j = CreateElement(Train, CenterX - Radius * w / h, CenterY + SemiHeight - Radius, 2.0 * Radius * w / h, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 4.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, Needle, false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("time 0.000277777777777778 * floor 0.523598775598298 *");
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDamping = new ObjectManager.Damping(20.0, 0.4);
									}
									{ // minute
										string File = OpenBveApi.Path.CombineFile(Folder, "needle_minute.png");
										Textures.Texture t;
										Textures.RegisterTexture(File, out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										int j = CreateElement(Train, CenterX - Radius * w / h, CenterY + SemiHeight - Radius, 2.0 * Radius * w / h, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 5.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, Needle, false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("time 0.0166666666666667 * floor 0.10471975511966 *");
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDamping = new ObjectManager.Damping(20.0, 0.4);
									}
									{ // second
										string File = OpenBveApi.Path.CombineFile(Folder, "needle_second.png");
										Textures.Texture t;
										Textures.RegisterTexture(File, out t);
										OpenBVEGame.RunInRenderThread(() =>
										{
											Textures.LoadTexture(t, Textures.OpenGlTextureWrapMode.ClampClamp);
										});
										double w = (double)t.Width;
										double h = (double)t.Height;
										int j = CreateElement(Train, CenterX - Radius * w / h, CenterY + SemiHeight - Radius, 2.0 * Radius * w / h, 2.0 * Radius, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - 6.0 * StackDistance, Train.Cars[Train.DriverCar].Driver, t, Needle, false);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("time floor 0.10471975511966 *");
										Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDamping = new ObjectManager.Damping(20.0, 0.4);
									}
								} break;
								// brakeindicator
							case "brakeindicator":
							case "ハンドルの段表示":
								{
									double CornerX = 0.0, CornerY = 0.0;
									string Image = null;
									int Width = 0;
									i++; while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
										int j = Lines[i].IndexOf('='); if (j >= 0) {
											string Key = Lines[i].Substring(0, j).TrimEnd();
											string Value = Lines[i].Substring(j + 1).TrimStart();
											string[] Arguments = GetArguments(Value);
											switch (Key.ToLowerInvariant()) {
												case "image":
												case "画像":
													if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
													if (Path.ContainsInvalidChars(Value)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
													} else {
														Image = OpenBveApi.Path.CombineFile(TrainPath, Value);
														if (!System.IO.File.Exists(Image)) {
															Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + Image + " could not be found in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
															Image = null;
														}
													}
													break;
												case "corner":
												case "左上":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out CornerX)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerX = 0.0;
													} else if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out CornerY)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														CornerY = 0.0;
													} break;
												case "width":
												case "幅":
													if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out Width)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Width is invalid in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Width = 1;
													} else if (Width <= 0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Width is expected to be positive in " + Key + " in " + Section + " at line " + (i + 1).ToString(Culture) + " in " + FileName);
														Width = 1;
													} break;
											}
										} i++;
									} i--;
									if (Image == null) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Image is required to be specified in " + Section + " in " + FileName);
									}
									if (Width <= 0) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
									}
									if (Image != null & Width > 0) {
										int w, h;
										Program.CurrentHost.QueryTextureDimensions(Image, out w, out h);
										if (w > 0 & h > 0) {
											int n = w / Width;
											int k = -1;
											for (int j = 0; j < n; j++) {
												Textures.Texture t;
												OpenBveApi.Textures.TextureClipRegion clip = new OpenBveApi.Textures.TextureClipRegion(j * Width, 0, Width, h);
												Textures.RegisterTexture(Image, new OpenBveApi.Textures.TextureParameters(clip, Color24.Blue), out t);
												if (j == 0) {
													k = CreateElement(Train, CornerX, CornerY + SemiHeight, (double)Width, (double)h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), false);
												} else {
													CreateElement(Train, CornerX, CornerY + SemiHeight, (double)Width, (double)h, FullWidth, FullHeight, WorldLeft, WorldTop, WorldWidth, WorldHeight, WorldZ + EyeDistance - StackDistance, Train.Cars[Train.DriverCar].Driver, t, new Color32(255, 255, 255, 255), true);
												}
											}
											if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												int maxpow = Train.Specs.MaximumPowerNotch;
												int em = maxpow + 3;
												Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > " + maxpow.ToString(Culture) + " BrakeNotch + " + maxpow.ToString(Culture) + " powerNotch - ? ?");
											} else {
												if (Train.Specs.HasHoldBrake) {
													int em = Train.Specs.MaximumPowerNotch + 2 + Train.Specs.MaximumBrakeNotch;
													int maxpow = Train.Specs.MaximumPowerNotch;
													int maxpowp1 = maxpow + 1;
													Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("emergencyBrake " + em.ToString(Culture) + " holdBrake " + maxpowp1.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpowp1.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ? ?");
												} else {
													int em = Train.Specs.MaximumPowerNotch + 1 + Train.Specs.MaximumBrakeNotch;
													int maxpow = Train.Specs.MaximumPowerNotch;
													Train.Cars[Train.DriverCar].CarSections[0].Elements[k].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation("emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpow.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ?");
												}
											}
										}
									}
								} break;
						}
					}
				}
			}
		}

		// get arguments
		private static string[] GetArguments(string Expression) {
			string[] Arguments = new string[16];
			int UsedArguments = 0;
			int Start = 0;
			for (int i = 0; i < Expression.Length; i++) {
				if (Expression[i] == ',' | Expression[i] == ':') {
					if (UsedArguments >= Arguments.Length) Array.Resize<string>(ref Arguments, Arguments.Length << 1);
					Arguments[UsedArguments] = Expression.Substring(Start, i - Start).TrimStart();
					UsedArguments++; Start = i + 1;
				} else if (Expression[i] == ';') {
					if (UsedArguments >= Arguments.Length) Array.Resize<string>(ref Arguments, Arguments.Length << 1);
					Arguments[UsedArguments] = Expression.Substring(Start, i - Start).TrimStart();
					UsedArguments++; Start = Expression.Length; break;
				}
			} if (Start < Expression.Length) {
				if (UsedArguments >= Arguments.Length) Array.Resize<string>(ref Arguments, Arguments.Length << 1);
				Arguments[UsedArguments] = Expression.Substring(Start).Trim();
				UsedArguments++;
			}
			Array.Resize<string>(ref Arguments, UsedArguments);
			return Arguments;
		}

		// create element
		private static int CreateElement(TrainManager.Train Train, double Left, double Top, double Width, double Height, double FullWidth, double FullHeight, double WorldLeft, double WorldTop, double WorldWidth, double WorldHeight, double WorldZ, Vector3 Driver, Textures.Texture Texture, Color32 Color, bool AddStateToLastElement) {
			// create object
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Vector3[] v = new Vector3[4];
			double sx = 0.5 * WorldWidth * Width / FullWidth;
			double sy = 0.5 * WorldHeight * Height / FullHeight;
			v[0] = new Vector3(-sx, -sy, 0);
			v[1] = new Vector3(-sx, sy, 0);
			v[2] = new Vector3(sx, sy, 0);
			v[3] = new Vector3(sx, -sy, 0);
			World.Vertex t0 = new World.Vertex(v[0], new Vector2(0.0f, 1.0f));
			World.Vertex t1 = new World.Vertex(v[1], new Vector2(0.0f, 0.0f));
			World.Vertex t2 = new World.Vertex(v[2], new Vector2(1.0f, 0.0f));
			World.Vertex t3 = new World.Vertex(v[3], new Vector2(1.0f, 1.0f));
			Object.Mesh.Vertices = new World.Vertex[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new World.MeshFace[] { new World.MeshFace(new int[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new World.MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = Texture != null ? (byte)World.MeshMaterial.TransparentColorMask : (byte)0;
			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = new Color24(0, 0, 255);
			Object.Mesh.Materials[0].DaytimeTexture = Texture;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = WorldLeft + sx + WorldWidth * Left / FullWidth;
			o.Y = WorldTop - sy - WorldHeight * Top / FullHeight;
			o.Z = WorldZ;
			// add object
			if (AddStateToLastElement) {
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length - 1;
				int j = Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States.Length;
				Array.Resize<ObjectManager.AnimatedObjectState>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States, j + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Object = Object;
				return n;
			} else {
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length;
				Array.Resize<ObjectManager.AnimatedObject>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements, n + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n] = new ObjectManager.AnimatedObject();
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States = new ObjectManager.AnimatedObjectState[1];
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Object = Object;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].CurrentState = 0;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex = ObjectManager.CreateDynamicObject();
				ObjectManager.Objects[Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex] = Object.Clone();
				return n;
			}
		}

	}
}
