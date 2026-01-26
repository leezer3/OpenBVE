using System;
using System.IO;
using Formats.OpenBve;
using LibRender2.Trains;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using TrainManager.Car;
using TrainManager.Handles;
using Path = OpenBveApi.Path;

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

		/// <summary>The panel size</summary>
		/// <remarks>PanelCfg has a fixed size measured in pixels</remarks>
		private readonly Vector2 PanelSize = new Vector2(480, 440);
		/// <summary>The world-size of the panel</summary>
		/// <remarks>The pixel size must then be translated into an on-screen object</remarks>
		private Vector2 WorldSize;
		private const double AspectRatio = 480.0 / 440.0;

		private double WorldLeft, WorldTop;
		private double SemiHeight = 240;

		/// <summary>Parses a BVE1 panel.cfg file</summary>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Car">The car to add the panel to</param>
		internal void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding, CarBase Car)
		{
			// read lines
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string fileName = Path.CombineFile(TrainPath, "panel.cfg");
			// initialize
			
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				WorldSize.X = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
				WorldSize.Y = WorldSize.X / AspectRatio;
			}
			else
			{
				WorldSize.Y = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance;
				WorldSize.X = WorldSize.Y * AspectRatio;
			}

			Car.CameraRestriction.BottomLeft = new Vector3(-0.5 * WorldSize.X, -0.5 * WorldSize.Y, EyeDistance);
			Car.CameraRestriction.TopRight = new Vector3(0.5 * WorldSize.X, 0.5 * WorldSize.Y, EyeDistance);
			WorldLeft = Car.Driver.X - 0.5 * WorldSize.X;
			WorldTop = Car.Driver.Y + 0.5 * WorldSize.Y;
			double WorldZ = Car.Driver.Z;
			const double UpDownAngleConstant = -0.191986217719376;
			// default background
			string PanelBackground = Path.CombineFile(TrainPath, "panel.bmp");

			ConfigFile<PanelSections, PanelKey> cfg = new ConfigFile<PanelSections, PanelKey>(fileName, Plugin.CurrentHost);

			cfg.ReadBlock(PanelSections.Panel, out var Block);
			if (Block != null && Block.GetPath(PanelKey.Background, TrainPath, out var panelBackground))
			{
				PanelBackground = panelBackground;
			}

			if (File.Exists(PanelBackground))
			{
				Plugin.CurrentHost.RegisterTexture(PanelBackground, new TextureParameters(null, Color24.Blue), out var panelTexture, true);
				SemiHeight = PanelSize.Y - panelTexture.Height;
				CreateElement(Car, 0, SemiHeight, panelTexture.Width, panelTexture.Height, WorldZ + EyeDistance, panelTexture, Color32.White);
			}
			

			while (cfg.RemainingSubBlocks > 0)
			{
				Block = cfg.ReadNextBlock();
				int Type;
				double Minimum = 0, Maximum = 1000, Angle;
				string Background, Cover, Unit, TextureFile;
				Vector2 Center;
				int Radius;
				switch (Block.Key)
				{
					case PanelSections.View:
						if (Block.GetValue(PanelKey.Yaw, out double yaw))
						{
							Car.DriverYaw = Math.Atan(yaw);
						}

						if (Block.GetValue(PanelKey.Pitch, out double pitch))
						{
							Car.DriverPitch = Math.Atan(pitch) + UpDownAngleConstant;
						}
						break;
					case PanelSections.PressureGauge:
						Angle = 45;
						Block.GetValue(PanelKey.Type, out Type);
						int[] NeedleType = { 0, 0 };
						Color32[] NeedleColor = { Color32.Black, Color32.Black };
						double UnitFactor = 1000;
						

						if (Type != 0 & Type != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type must be either 0 or 1 in " + Block.Key + " in " + fileName);
							Type = 0;
						}

						if (Block.GetEnumValue(PanelKey.LowerHand, out PanelSubject lowerSubject, out Color32 lowerColor))
						{
							NeedleType[0] = (int)lowerSubject;
							NeedleColor[0] = lowerColor;
						}

						if (Block.GetEnumValue(PanelKey.UpperHand, out PanelSubject upperSubject, out Color32 upperColor))
						{
							NeedleType[1] = (int)upperSubject;
							NeedleColor[1] = upperColor;
						}

						Block.GetVector2(PanelKey.Center, ',', out Center);
						Block.GetValue(PanelKey.Radius, out Radius);
						Block.GetPath(PanelKey.Background, TrainPath, out Background);
						Block.GetPath(PanelKey.Cover, TrainPath, out Cover);
						if (Block.GetValue(PanelKey.Unit, out Unit))
						{
							switch (Unit.ToLowerInvariant())
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Units are invalid in " + Block.Key + " in " + fileName);
									break;
							}
						}

						Block.TryGetValue(PanelKey.Minimum, ref Minimum);
						Block.TryGetValue(PanelKey.Maximum, ref Maximum);
						Block.TryGetValue(PanelKey.Angle, ref Angle);

						Angle = Angle.ToRadians();
						// units
						Minimum *= UnitFactor;
						Maximum *= UnitFactor;
						// background
						if (!string.IsNullOrEmpty(Background) && File.Exists(Background))
						{
							Plugin.CurrentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var pressureBackgroundTexture, true);
							CreateElement(Car, Center.X - 0.5 * pressureBackgroundTexture.Width, Center.Y + SemiHeight - 0.5 * pressureBackgroundTexture.Height, WorldZ + EyeDistance - 3.0 * StackDistance, pressureBackgroundTexture);
						}

						// cover
						if (!string.IsNullOrEmpty(Cover) && File.Exists(Cover))
						{
							Plugin.CurrentHost.RegisterTexture(Cover, new TextureParameters(null, Color24.Blue), out var pressureCoverTexture, true);
							CreateElement(Car, Center.X - 0.5 * pressureCoverTexture.Width, Center.Y + SemiHeight - 0.5 * pressureCoverTexture.Height, WorldZ + EyeDistance - 6.0 * StackDistance, pressureCoverTexture);
						}

						if (Type == 0)
						{
							// needles
							for (int k = 0; k < 2; k++)
							{
								if (NeedleType[k] != 0)
								{
									string Folder = Plugin.FileSystem.GetDataFolder("Compatibility");
									TextureFile = Path.CombineFile(Folder, k == 0 ? "needle_pressuregauge_lower.png" : "needle_pressuregauge_upper.png");
									Plugin.CurrentHost.RegisterTexture(TextureFile, TextureParameters.NoChange, out var pressureNeedleTexture, true);
									int j = CreateElement(Car, Center.X - Radius * pressureNeedleTexture.AspectRatio, Center.Y + SemiHeight - Radius, 2.0 * Radius * pressureNeedleTexture.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - (4 + k) * StackDistance, pressureNeedleTexture, NeedleColor[k]);
									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateXDirection);
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

									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.CurrentHost, Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
								}
							}
						}
						else if (Type == 1)
						{
							// leds
							if (NeedleType[1] != 0)
							{
								int j = CreateElement(Car, Center.X - Radius, Center.Y + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, null, NeedleColor[1]);
								double x0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
								double y0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
								double z0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
								double x1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
								double y1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
								double z1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
								double x2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
								double y2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
								double z2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
								double x3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
								double y3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
								double z3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
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
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, NeedleColor[1]);
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDClockwiseWinding = true;
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDVectors = new[]
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

								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDFunction = new FunctionScript(Plugin.CurrentHost, Variable + " " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
							}
						}
						break;
					case PanelSections.Speedometer:
						Color32 needleColor = Color32.White;
						bool needleColorOverridden = false;
						Angle = 60;
						
						Block.GetValue(PanelKey.Type, out Type);

						if (Type != 0 & Type != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type must be either 0 or 1 in " + Block.Key + " in " + fileName);
							Type = 0;
						}

						Block.GetPath(PanelKey.Background, TrainPath, out Background);
						Block.GetPath(PanelKey.Cover, TrainPath, out Cover);
						if (Block.GetColor24(PanelKey.Needle, out Color24 color))
						{
							needleColor = color;
							needleColorOverridden = true;
						}

						Block.GetPath(PanelKey.ATC, TrainPath, out string ATCPath);
						Block.GetValue(PanelKey.ATCRadius, out double ATCRadius);
						Block.GetVector2(PanelKey.Center, ',', out Center);
						if (Block.GetValue(PanelKey.Maximum, out Maximum))
						{
							Maximum *= 0.277777777777778; // convert to m/s
						}
						else
						{
							Maximum = 33.3333333333333; // 120km/h
						}
						Block.GetValue(PanelKey.Radius, out Radius);
						Block.TryGetValue(PanelKey.Angle, ref Angle);

						Angle = Angle.ToRadians();

						if (!string.IsNullOrEmpty(Background) && System.IO.File.Exists(Background))
						{
							// background/led
							Plugin.CurrentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var speedometerBackgroundTexture, true);
							CreateElement(Car, Center.X - 0.5 * speedometerBackgroundTexture.Width, Center.Y + SemiHeight - 0.5 * speedometerBackgroundTexture.Height, WorldZ + EyeDistance - 3.0 * StackDistance, speedometerBackgroundTexture);
						}

						if (!string.IsNullOrEmpty(Cover))
						{
							// cover
							Plugin.CurrentHost.RegisterTexture(Cover, new TextureParameters(null, Color24.Blue), out var speedometerCoverTexture, true);
							CreateElement(Car, Center.X - 0.5 * speedometerCoverTexture.Width, Center.Y + SemiHeight - 0.5 * speedometerCoverTexture.Height, WorldZ + EyeDistance - 6.0 * StackDistance, speedometerCoverTexture);
						}

						if (!string.IsNullOrEmpty(ATCPath))
						{
							// atc
							Plugin.CurrentHost.QueryTextureDimensions(ATCPath, out var atcWidth, out var atcHeight);
							if (atcWidth > 0 & atcHeight > 0)
							{
								int n = atcWidth / atcHeight;
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

									double x = Center.X - 0.5 * atcHeight + Math.Sin(a) * ATCRadius;
									double y = Center.Y - 0.5 * atcHeight - Math.Cos(a) * ATCRadius + SemiHeight;
									Plugin.CurrentHost.RegisterTexture(ATCPath, new TextureParameters(new TextureClipRegion(j * atcHeight, 0, atcHeight, atcHeight), Color24.Blue), out var ATCTexture, true);
									if (j == 0)
									{
										k = CreateElement(Car, x, y, atcHeight, atcHeight, WorldZ + EyeDistance - 4.0 * StackDistance, ATCTexture, Color32.White);
									}
									else
									{
										CreateElement(Car, x, y, atcHeight, atcHeight, WorldZ + EyeDistance - 4.0 * StackDistance, ATCTexture, Color32.White, true);
									}
								}

								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "271 pluginstate", false);
							}
						}

						if (Type == 0)
						{
							// needle
							string Folder = Plugin.FileSystem.GetDataFolder("Compatibility");
							TextureFile = Path.CombineFile(Folder, "needle_speedometer.png");
							Plugin.CurrentHost.RegisterTexture(TextureFile, TextureParameters.NoChange, out var speedometerNeedleTexture, true);
							int j = CreateElement(Car, Center.X - Radius * speedometerNeedleTexture.AspectRatio, Center.Y + SemiHeight - Radius, 2.0 * Radius * speedometerNeedleTexture.AspectRatio, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, speedometerNeedleTexture, needleColor);
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZDirection = Vector3.Backward;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateXDirection = Vector3.Right;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateXDirection);
							double c0 = Angle + Math.PI;
							double c1 = 2.0 * (Math.PI - Angle) / Maximum;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.CurrentHost, "speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
						}
						else if (Type == 1)
						{
							// led
							if (!needleColorOverridden) needleColor = Color32.Black;
							int j = CreateElement(Car, Center.X - Radius, Center.Y + SemiHeight - Radius, 2.0 * Radius, 2.0 * Radius, WorldZ + EyeDistance - 5.0 * StackDistance, null, needleColor);
							double x0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
							double y0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
							double z0 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
							double x1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
							double y1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
							double z1 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
							double x2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
							double y2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
							double z2 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
							double x3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
							double y3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
							double z3 = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
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
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, needleColor);
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDClockwiseWinding = true;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDInitialAngle = Angle - 2.0 * Math.PI;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDLastAngle = 2.0 * Math.PI - Angle;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDVectors = new[]
							{
								new Vector3(x0, y0, z0),
								new Vector3(x1, y1, z1),
								new Vector3(x2, y2, z2),
								new Vector3(x3, y3, z3),
								new Vector3(cx, cy, cz)
							};
							double c0 = Angle;
							double c1 = 2.0 * (Math.PI - Angle) / Maximum;
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[j].LEDFunction = new FunctionScript(Plugin.CurrentHost, "speedometer abs " + c1.ToString(Culture) + " " + c0.ToString(Culture) + " fma", false);
						}
						break;
					case PanelSections.DigitalIndicator:
						if (!Block.GetPath(PanelKey.Number, TrainPath, out string digitalNumber))
						{
							break;
						}

						Block.GetVector2(PanelKey.Corner, ',', out Vector2 Corner);
						Block.GetVector2(PanelKey.Size, ',', out Vector2 Size);

						int Units = 0;
						if (Block.GetValue(PanelKey.Unit, out Unit))
						{
							switch (Unit.ToLowerInvariant())
							{
								case "km/h":
									Units = 0;
									break;
								case "mph":
									Units = 1;
									break;
								case "m/s":
									Units = 2;
									break;
								default:
									if (!NumberFormats.TryParseIntVb6(Unit, out Units))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Units are invalid in " + Block.Key + " in " + fileName);
										Units = 0;
									}
									break;
							}
						}

						if (Units < 0 | Units > 2)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value must be between 0 and 2 in " + Block.Key + " in " + fileName);
							Units = 0;
						}

						if (Units == 1)
						{
							UnitFactor = 2.2369362920544;
						}
						else if (Units == 2)
						{
							UnitFactor = 1.0;
						}
						else
						{
							UnitFactor = 3.6;
						}

						if (Size.X <= 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Block.Key + " in " + fileName);
							break;
						}

						if (Size.Y <= 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Block.Key + " in " + fileName);
							break;
						}

						Plugin.CurrentHost.QueryTextureDimensions(digitalNumber, out var digitalNumberWidth, out var digitalNumberHeight);
						if (digitalNumberWidth > 0 & digitalNumberHeight > 0)
						{
							//Generate an error message rather than crashing if the clip region is invalid
							if (Size.X > digitalNumberWidth)
							{
								Size.X = digitalNumberWidth;
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Clip region width was greater than the texture width " + Block.Key + " in " + fileName);
							}

							if (Size.Y > digitalNumberHeight)
							{
								Size.X = digitalNumberHeight;
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Clip region height was greater than the texture height " + Block.Key + " in " + fileName);
							}

							int n = digitalNumberHeight / (int)Size.Y;
							Texture[] digitalNumberTextures = new Texture[n];
							for (int j = 0; j < n; j++)
							{
								Plugin.CurrentHost.RegisterTexture(digitalNumber, new TextureParameters(new TextureClipRegion(digitalNumberWidth - (int)Size.X, j * (int)Size.Y, (int)Size.X, (int)Size.Y), Color24.Blue), out digitalNumberTextures[j]);
							}

							// hundreds
							int k = -1;
							for (int j = 0; j < n; j++)
							{
								if (j == 0)
								{
									k = CreateElement(Car, Corner.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White);
								}
								else
								{
									CreateElement(Car, Corner.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White, true);
								}
							}
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 100 >= <> 100 quotient 10 mod 10 ?", false);

							// tens
							k = -1;
							for (int j = 0; j < n; j++)
							{
								if (j == 0)
								{
									k = CreateElement(Car, Corner.X + Size.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White);
								}
								else
								{
									CreateElement(Car, Corner.X + Size.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White, true);
								}
							}
							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * ~ 10 >= <> 10 quotient 10 mod 10 ?", false);

							// ones
							k = -1;
							for (int j = 0; j < n; j++)
							{
								if (j == 0)
								{
									k = CreateElement(Car, Corner.X + 2.0 * Size.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White);
								}
								else
								{
									CreateElement(Car, Corner.X + 2.0 * Size.X, Corner.Y + SemiHeight, Size.X, Size.Y, WorldZ + EyeDistance - 7.0 * StackDistance, digitalNumberTextures[j], Color32.White, true);
								}
							}

							Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "speedometer abs " + UnitFactor.ToString(Culture) + " * floor 10 mod", false);
						}
						break;
					case PanelSections.PilotLamp:
						if (!Block.GetPath(PanelKey.TurnOn, TrainPath, out string turnOnPath) || string.IsNullOrEmpty(turnOnPath))
						{
							break;
						}
						if (!Block.GetPath(PanelKey.TurnOff, TrainPath, out string turnOffPath) || string.IsNullOrEmpty(turnOffPath))
						{
							break;
						}

						Block.GetVector2(PanelKey.Corner, ',', out Corner);

						Plugin.CurrentHost.RegisterTexture(turnOnPath, new TextureParameters(null, Color24.Blue), out var t0, true);
						Plugin.CurrentHost.RegisterTexture(turnOffPath, new TextureParameters(null, Color24.Blue), out var t1, true);
						int elementIndex = CreateElement(Car, Corner.X, Corner.Y + SemiHeight, WorldZ + EyeDistance - 2.0 * StackDistance, t0);
						CreateElement(Car, Corner.X, Corner.Y + SemiHeight, WorldZ + EyeDistance - 2.0 * StackDistance, t1, true);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new FunctionScript(Plugin.CurrentHost, "doors 0 !=", false);
						break;
					case PanelSections.Watch:
						Color24 handColor = Color24.Black;
						double handRadius = 16;
						Block.GetPath(PanelKey.Background, TrainPath, out Background);
						if (Block.GetColor24(PanelKey.Needle, out color))
						{
							handColor = color;
						}

						Block.GetVector2(PanelKey.Center, ',', out Center);
						Block.TryGetValue(PanelKey.Radius, ref handRadius);

						if (!string.IsNullOrEmpty(Background))
						{
							Plugin.CurrentHost.RegisterTexture(Background, new TextureParameters(null, Color24.Blue), out var watchBackgroundTexture, true);
							CreateElement(Car, Center.X - 0.5 * watchBackgroundTexture.Width, Center.Y + SemiHeight - 0.5 * watchBackgroundTexture.Height, WorldZ + EyeDistance - 3.0 * StackDistance, watchBackgroundTexture);
						}

						string compatabilityFolder = Plugin.FileSystem.GetDataFolder("Compatibility");
						// hour
						TextureFile = Path.CombineFile(compatabilityFolder, "needle_hour.png");
						Plugin.CurrentHost.RegisterTexture(TextureFile, TextureParameters.NoChange, out var hourTexture, true);
						int handElement = CreateElement(Car, Center.X - handRadius * hourTexture.AspectRatio, Center.Y + SemiHeight - handRadius, 2.0 * handRadius * hourTexture.AspectRatio, 2.0 * handRadius, WorldZ + EyeDistance - 4.0 * StackDistance, hourTexture, handColor);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection = Vector3.Backward;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection = Vector3.Right;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZFunction = new FunctionScript(Plugin.CurrentHost, "time 0.000277777777777778 * floor 0.523598775598298 *", false);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDamping = new Damping(20.0, 0.4);
						// minute
						TextureFile = Path.CombineFile(compatabilityFolder, "needle_minute.png");
						Plugin.CurrentHost.RegisterTexture(TextureFile, TextureParameters.NoChange, out var minuteTexture, true);
						handElement = CreateElement(Car, Center.X - handRadius * minuteTexture.AspectRatio, Center.Y + SemiHeight - handRadius, 2.0 * handRadius * minuteTexture.AspectRatio, 2.0 * handRadius, WorldZ + EyeDistance - 5.0 * StackDistance, minuteTexture, handColor);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection = Vector3.Backward;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection = Vector3.Right;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZFunction = new FunctionScript(Plugin.CurrentHost, "time 0.0166666666666667 * floor 0.10471975511966 *", false);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDamping = new Damping(20.0, 0.4);
						// second
						TextureFile = Path.CombineFile(compatabilityFolder, "needle_second.png");
						Plugin.CurrentHost.RegisterTexture(TextureFile, TextureParameters.NoChange, out var secondTexture, true);
						handElement = CreateElement(Car, Center.X - handRadius * secondTexture.AspectRatio, Center.Y + SemiHeight - handRadius, 2.0 * handRadius * secondTexture.AspectRatio, 2.0 * handRadius, WorldZ + EyeDistance - 6.0 * StackDistance, secondTexture, handColor);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection = Vector3.Backward;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection = Vector3.Right;
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateXDirection);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZFunction = new FunctionScript(Plugin.CurrentHost, "time floor 0.10471975511966 *", false);
						Car.CarSections[CarSectionType.Interior].Groups[0].Elements[handElement].RotateZDamping = new Damping(20.0, 0.4);
						break;
					case PanelSections.BrakeIndicator:
						if (!Block.GetPath(PanelKey.Image, TrainPath, out string brakeIndicatorPath) || string.IsNullOrEmpty(brakeIndicatorPath))
						{
							break;
						}

						Block.GetVector2(PanelKey.Corner, ',', out Corner);
						Block.GetValue(PanelKey.Width, out int indicatorWidth);
						if (indicatorWidth <= 0)
						{
							indicatorWidth = 1;
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Width is expected to be positive in " + Block.Key + " in " + fileName);
						}

						Plugin.CurrentHost.QueryTextureDimensions(brakeIndicatorPath, out var w, out var h);
						if (w > 0 & h > 0)
						{
							int n = w / indicatorWidth;
							int k = -1;
							for (int j = 0; j < n; j++)
							{
								TextureClipRegion clip = new TextureClipRegion(j * indicatorWidth, 0, indicatorWidth, h);
								Plugin.CurrentHost.RegisterTexture(brakeIndicatorPath, new TextureParameters(clip, Color24.Blue), out var brakeIndicatorTexture);
								if (j == 0)
								{
									k = CreateElement(Car, Corner.X, Corner.Y + SemiHeight, indicatorWidth, h, WorldZ + EyeDistance - StackDistance, brakeIndicatorTexture, Color32.White);
								}
								else
								{
									CreateElement(Car, Corner.X, Corner.Y + SemiHeight,  indicatorWidth, h, WorldZ + EyeDistance - StackDistance, brakeIndicatorTexture, Color32.White, true);
								}
							}

							if (Car.baseTrain.Handles.Brake is AirBrakeHandle)
							{
								int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
								int em = maxpow + 3;
								Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > " + maxpow.ToString(Culture) + " BrakeNotch + " + maxpow.ToString(Culture) + " powerNotch - ? ?", false);
							}
							else
							{
								if (Car.baseTrain.Handles.HasHoldBrake)
								{
									int em = Car.baseTrain.Handles.Power.MaximumNotch + 2 + Car.baseTrain.Handles.Brake.MaximumNotch;
									int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
									int maxpowp1 = maxpow + 1;
									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "emergencyBrake " + em.ToString(Culture) + " holdBrake " + maxpowp1.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpowp1.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ? ?", false);
								}
								else
								{
									int em = Car.baseTrain.Handles.Power.MaximumNotch + 1 + Car.baseTrain.Handles.Brake.MaximumNotch;
									int maxpow = Car.baseTrain.Handles.Power.MaximumNotch;
									Car.CarSections[CarSectionType.Interior].Groups[0].Elements[k].StateFunction = new FunctionScript(Plugin.CurrentHost, "emergencyBrake " + em.ToString(Culture) + " brakeNotch 0 > brakeNotch " + maxpow.ToString(Culture) + " + " + maxpow.ToString(Culture) + " powerNotch - ? ?", false);
								}
							}
						}
						break;
					
				}
			}
		}

		private int CreateElement(CarBase Car, double Left, double Top, double WorldZ, Texture Texture, bool AddStateToLastElement = false)
		{
			return CreateElement(Car, Left, Top, Texture.Width, Texture.Height, WorldZ, Texture, Color32.White, AddStateToLastElement);
		}

		// create element
		private int CreateElement(CarBase Car, double Left, double Top, double Width, double Height, double WorldZ, Texture Texture, Color32 Color, bool AddStateToLastElement = false)
		{
			// create object
			StaticObject Object = new StaticObject(Plugin.CurrentHost);
			Vector3[] v = new Vector3[4];
			double sx = 0.5 * WorldSize.X * Width / PanelSize.X;
			double sy = 0.5 * WorldSize.Y * Height / PanelSize.Y;
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
			o.X = WorldLeft + sx + WorldSize.X * Left / PanelSize.X;
			o.Y = WorldTop - sy - WorldSize.Y * Top / PanelSize.Y;
			o.Z = WorldZ;
			// add object
			if (AddStateToLastElement)
			{
				int n = Car.CarSections[CarSectionType.Interior].Groups[0].Elements.Length - 1;
				int j = Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n].States.Length;
				Array.Resize(ref Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n].States, j + 1);
				Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n].States[j] = new ObjectState
				{
					Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z),
					Prototype = Object
				};
				return n;
			}
			else
			{
				int n = Car.CarSections[CarSectionType.Interior].Groups[0].Elements.Length;
				Array.Resize(ref Car.CarSections[CarSectionType.Interior].Groups[0].Elements, n + 1);
				Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n] = new AnimatedObject(Plugin.CurrentHost, Object);
				Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Plugin.CurrentHost.CreateDynamicObject(ref Car.CarSections[CarSectionType.Interior].Groups[0].Elements[n].internalObject);
				return n;
			}
		}

	}
}
