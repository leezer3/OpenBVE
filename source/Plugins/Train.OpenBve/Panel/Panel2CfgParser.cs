using System;
using System.Collections.Generic;
using System.IO;
using Formats.OpenBve;
using LibRender2.Trains;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using TrainManager.Car;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	internal partial class Panel2CfgParser
	{
		internal readonly Plugin Plugin;

		internal Panel2CfgParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		// constants
		private const double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double EyeDistance = 1.0;

		double PanelResolution = 1024.0;
		double PanelLeft = 0.0, PanelRight = 1024.0;
		double PanelTop = 0.0, PanelBottom = 1024.0;
		Vector2 PanelCenter = new Vector2(0, 512);
		Vector2 PanelOrigin = new Vector2(0, 512);
		
		private string trainName;

		/// <summary>Parses a BVE2 / openBVE panel.cfg file</summary>
		/// <param name="panelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="trainPath">The on-disk path to the train</param>
		/// <param name="Car">The car to add the panel to</param>
		internal void ParsePanel2Config(string panelFile, string trainPath, CarBase Car)
		{
			//Train name, used for hacks detection
			trainName = new DirectoryInfo(trainPath).Name.ToUpperInvariant();
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string fileName = Path.CombineFile(trainPath, panelFile);
			ConfigFile<Panel2Sections, Panel2Key> cfg = new ConfigFile<Panel2Sections, Panel2Key>(fileName, Plugin.CurrentHost);

			if (cfg.ReadBlock(Panel2Sections.This, out var Block))
			{
				// NOTE: We should only be able to create a panel with main image available, however some trains seem to use a PilotLamp as the main panel image, with no texture defined in the [This] section
				// e.g. trta9000_6r
				// Many panel properties are calculated with the size of this element, so only accept this with hacks on
				if (Block.GetPath(Panel2Key.DaytimeImage, trainPath, out string panelDaytimeImage) || Plugin.CurrentOptions.EnableBveTsHacks)
				{
					Block.TryGetValue(Panel2Key.Resolution, ref PanelResolution, NumberRange.Positive);
					if (PanelResolution < 100)
					{
						//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
						//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given in " + fileName);
					}

					Block.TryGetValue(Panel2Key.Left, ref PanelLeft);
					Block.TryGetValue(Panel2Key.Right, ref PanelRight);
					Block.TryGetValue(Panel2Key.Top, ref PanelTop);
					Block.TryGetValue(Panel2Key.Bottom, ref PanelBottom);
					Color24 panelTransparentColor = Color24.Blue;
					Block.TryGetColor24(Panel2Key.TransparentColor, ref panelTransparentColor);
					Block.TryGetVector2(Panel2Key.Center, ',', ref PanelCenter);
					Block.TryGetVector2(Panel2Key.Origin, ',', ref PanelOrigin);
					ApplyGlobalHacks();
					double WorldWidth, WorldHeight;
					if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
					{
						WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
						WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
					}
					else
					{
						WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance / Plugin.Renderer.Screen.AspectRatio;
						WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
					}
					double x0 = (PanelLeft - PanelCenter.X) / PanelResolution;
					double x1 = (PanelRight - PanelCenter.X) / PanelResolution;
					double y0 = (PanelCenter.Y - PanelBottom) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
					double y1 = (PanelCenter.Y - PanelTop) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
					Car.CameraRestriction.BottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, EyeDistance);
					Car.CameraRestriction.TopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, EyeDistance);
					Car.DriverYaw = Math.Atan((PanelCenter.X - PanelOrigin.X) * WorldWidth / PanelResolution);
					Car.DriverPitch = Math.Atan((PanelOrigin.Y - PanelCenter.Y) * WorldWidth / PanelResolution);
					Plugin.CurrentHost.RegisterTexture(panelDaytimeImage, new TextureParameters(null, panelTransparentColor), out var tday, true, 20000);
					Texture tnight = null;
					if (Block.GetPath(Panel2Key.NighttimeImage, trainPath, out string panelNighttimeImage))
					{
						Plugin.CurrentHost.RegisterTexture(panelNighttimeImage, new TextureParameters(null, panelTransparentColor), out tnight, true, 20000);
					}
					CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[0], 0.0, 0.0, new Vector2(0.5, 0.5), 0.0, PanelResolution, PanelBottom, PanelCenter, Car.Driver, tday, tnight);
				}
				else
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "A [This] section was present, but the main panel image was missing or invalid in " + fileName);
					return;
				}
				Block.ReportErrors();
			}
			else
			{
				// no main panel image, so invalid
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Panel2.cfg file " + fileName + " does not contain a [This] section.");
				return;
			}

			int GroupIndex = 0;

			if (Plugin.CurrentOptions.Panel2ExtendedMode)
			{
				GroupIndex++;
				Array.Resize(ref Car.CarSections[CarSectionType.Interior].Groups, GroupIndex + 1);
				Car.CarSections[CarSectionType.Interior].Groups[GroupIndex] = new ElementsGroup();
			}

			while (cfg.RemainingSubBlocks > 0)
			{
				Block = cfg.ReadNextBlock();
				Block.GetEnumValue(Panel2Key.Subject, out Panel2Subject Subject, out int subjectIndex, out string subjectSuffix);
				string Function = string.Empty, DaytimeImage;
				Block.TryGetValue(Panel2Key.Function, ref Function);
				Color24 Color = Color24.White;
				Color24 TransparentColor = Color24.Blue;
				Block.TryGetColor24(Panel2Key.TransparentColor, ref TransparentColor);
				Block.GetValue(Panel2Key.Layer, out int Layer);
				double Radius;
				switch (Block.Key)
				{
					case Panel2Sections.PilotLamp:
						if (Block.GetPath(Panel2Key.DaytimeImage, trainPath, out DaytimeImage))
						{
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							Plugin.CurrentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out var tday, true, 20000);
							Texture tnight = null;
							if (Block.GetPath(Panel2Key.NighttimeImage, trainPath, out string NighttimeImage))
							{
								Plugin.CurrentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight);
							}
							int w = tday.Width;
							int h = tday.Height;
							int j = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X, Location.Y, w, h, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, tday, tnight, Color32.White);
							string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, subjectIndex, subjectSuffix);
							try
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].StateFunction = !string.IsNullOrEmpty(Function) ? new FunctionScript(Plugin.CurrentHost, Function, true) : new FunctionScript(Plugin.CurrentHost, f + " 1 == --", false);
							}
							catch
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
							}
						}
						break;
					case Panel2Sections.Needle:
						if (Block.GetPath(Panel2Key.DaytimeImage, trainPath, out DaytimeImage))
						{
							double InitialAngle = -120, LastAngle = 120;
							double Minimum = 0.0, Maximum = 1000.0;
							double NaturalFrequency = -1.0, DampingRatio = -1.0;
							Vector2 Origin = new Vector2(-1, -1);
							Block.TryGetValue(Panel2Key.Function, ref Function);
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							bool OriginDefined = Block.TryGetVector2(Panel2Key.Origin, ',', ref Origin);
							Block.GetValue(Panel2Key.Radius, out Radius);
							Block.TryGetValue(Panel2Key.InitialAngle, ref InitialAngle);
							InitialAngle = InitialAngle.ToRadians();
							Block.TryGetValue(Panel2Key.LastAngle, ref LastAngle);
							LastAngle = LastAngle.ToRadians();
							Block.TryGetValue(Panel2Key.Minimum, ref Minimum);
							Block.TryGetValue(Panel2Key.Maximum, ref Maximum);
							if (Block.TryGetValue(Panel2Key.NaturalFreq, ref NaturalFrequency) && NaturalFrequency < 0)
							{
								NaturalFrequency = -NaturalFrequency;
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is expected to be non-negative in [Needle] in " + panelFile);
							}
							if(Block.TryGetValue(Panel2Key.DampingRatio, ref DampingRatio) && DampingRatio < 0)
							{
								DampingRatio = -DampingRatio;
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DampingRatio is expected to be non-negative in [Needle] in " + panelFile);
							}
							Block.TryGetColor24(Panel2Key.Color, ref Color);
							Block.GetValue(Panel2Key.Backstop, out bool Backstop);
							Block.GetValue(Panel2Key.Smoothed, out bool Smoothed);
							
							Plugin.CurrentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out var tday, true, 20000);
							Texture tnight = null;
							if (Block.GetPath(Panel2Key.NighttimeImage, trainPath, out string NighttimeImage))
							{
								Plugin.CurrentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight);
							}
							if (!OriginDefined)
							{
								Origin.X = 0.5 * tday.Width;
								Origin.Y = 0.5 * tday.Height;
							}
							double ox = Origin.X / tday.Width;
							double oy = Origin.Y / tday.Height;
							double n = Radius == 0.0 | Origin.Y == 0.0 ? 1.0 : Radius / Origin.Y;
							double nx = n * tday.Width;
							double ny = n * tday.Height;
							int j = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X - ox * nx, Location.Y - oy * ny, nx, ny, new Vector2(ox, oy), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, tday, tnight, Color);
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZDirection = Vector3.Backward;
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateXDirection = Vector3.Right;
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZDirection, Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateXDirection);
							string f;
							switch (Subject)
							{
								case Panel2Subject.Hour:
									f = Smoothed ? "0.000277777777777778 time * 24 mod" : "0.000277777777777778 time * floor";
									break;
								case Panel2Subject.Min:
									f = Smoothed ? "0.0166666666666667 time * 60 mod" : "0.0166666666666667 time * floor";
									break;
								case Panel2Subject.Sec:
									f = Smoothed ? "time 60 mod" : "time floor";
									break;
								default:
									f = GetStackLanguageFromSubject(Car.baseTrain, Subject, subjectIndex, subjectSuffix);
									break;
							}

							double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
							double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
							f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
							if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0)
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZDamping = new Damping(NaturalFrequency, DampingRatio);
							}
							try
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZFunction = !string.IsNullOrEmpty(Function) ? new FunctionScript(Plugin.CurrentHost, Function, true) : new FunctionScript(Plugin.CurrentHost, f, false);
							}
							catch
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
								break;
							}
							if (Backstop)
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZFunction.Minimum = InitialAngle;
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].RotateZFunction.Maximum = LastAngle;
							}
						}
						break;
					case Panel2Sections.LinearGauge:
						if (Block.GetPath(Panel2Key.DaytimeImage, trainPath, out DaytimeImage))
						{
							Vector2 Direction = new Vector2(1, 0);
							double Minimum = 0, Maximum = 0;

							Block.TryGetValue(Panel2Key.Function, ref Function);
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							Block.TryGetValue(Panel2Key.Minimum, ref Minimum);
							Block.TryGetValue(Panel2Key.Maximum, ref Maximum);
							Block.GetValue(Panel2Key.Width, out int Width);
							Block.TryGetVector2(Panel2Key.Direction, ',', ref Direction);

							Plugin.CurrentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, TransparentColor), out var tday, true, 20000);
							Texture tnight = null;
							if (Block.GetPath(Panel2Key.NighttimeImage, trainPath, out string NighttimeImage))
							{
								Plugin.CurrentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, TransparentColor), out tnight);
							}
							int j = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X, Location.Y, tday.Width, tday.Height, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, tday, tnight, Color32.White);
							if (Maximum < Minimum)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Maximum value must be greater than minimum value " + Block.Key + " in " + fileName);
								break;
							}
							string tf = GetInfixFunction(Car.baseTrain, Subject, subjectIndex, subjectSuffix, Minimum, Maximum, Width, tday.Width);
							if (!string.IsNullOrEmpty(tf) || !string.IsNullOrEmpty(Function))
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].TextureShiftXDirection = Direction;
								try
								{
									if (!string.IsNullOrEmpty(Function))
									{
										Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].TextureShiftXFunction = new FunctionScript(Plugin.CurrentHost, Function, true);
									}
									else
									{
										Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].TextureShiftXFunction = new FunctionScript(Plugin.CurrentHost, tf, false);
									}
								}
								catch
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
								}
							}
						}
						break;
					case Panel2Sections.DigitalNumber:
						if (Block.GetPath(Panel2Key.DaytimeImage, trainPath, out DaytimeImage))
						{
							Block.TryGetValue(Panel2Key.Function, ref Function);
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							Block.GetValue(Panel2Key.Interval, out int Interval);							
							Plugin.CurrentHost.QueryTextureDimensions(DaytimeImage, out var wday, out var hday);
							if (wday > 0 & hday > 0)
							{
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
										case Panel2Subject.Power:
											if (Car.baseTrain.Handles.Power.MaximumNotch > numFrames)
											{
												numFrames = Car.baseTrain.Handles.Power.MaximumNotch;
											}
											break;
										case Panel2Subject.Brake:
											int b = Car.baseTrain.Handles.Brake.MaximumNotch + 2;
											if (Car.baseTrain.Handles.HasHoldBrake)
											{
												b++;
											}
											if (b > numFrames)
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
										Plugin.CurrentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, Interval), TransparentColor), out tday[k]);
									}
									else if (k * Interval >= hday)
									{
										numFrames = k;
										Array.Resize(ref tday, k);
									}
									else
									{
										Plugin.CurrentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, hday - (k * Interval)), TransparentColor), out tday[k]);
									}
								}
								if (Block.GetPath(Panel2Key.NighttimeImage, trainPath, out string NighttimeImage))
								{
									Plugin.CurrentHost.QueryTextureDimensions(NighttimeImage, out var wnight, out var hnight);
									tnight = new Texture[numFrames];
									for (int k = 0; k < numFrames; k++)
									{
										if ((k + 1) * Interval <= hnight)
										{
											Plugin.CurrentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, Interval), TransparentColor), out tnight[k]);
										}
										else if (k * Interval > hnight)
										{
											tnight[k] = null;
										}
										else
										{
											Plugin.CurrentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, hnight - (k * Interval)), TransparentColor), out tnight[k]);
										}
									}

								}
								else
								{
									tnight = new Texture[numFrames];
									for (int k = 0; k < numFrames; k++)
									{
										tnight[k] = null;
									}
								}

								int j = -1;
								for (int k = 0; k < tday.Length; k++)
								{
									int l = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X, Location.Y, wday, Interval, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, tday[k], tnight[k], Color32.White, k != 0);
									if (k == 0) j = l;
								}
								string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, subjectIndex, subjectSuffix);
								try
								{
									if (!string.IsNullOrEmpty(Function))
									{
										Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.CurrentHost, Function, true);
									}
									else
									{
										Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.CurrentHost, f, false);
									}
								}
								catch
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
								}

								if (Plugin.CurrentOptions.Panel2ExtendedMode)
								{
									if (wday >= Plugin.CurrentOptions.Panel2ExtendedMinSize && Interval >= Plugin.CurrentOptions.Panel2ExtendedMinSize)
									{
										if (Subject == Panel2Subject.Power)
										{
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.PowerDecrease } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.PowerIncrease } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
										}

										if (Subject == Panel2Subject.Brake)
										{
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.BrakeIncrease } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.BrakeDecrease } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
										}

										if (Subject == Panel2Subject.Reverser)
										{
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.ReverserForward } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
											Plugin.PanelXmlParser.CreateTouchElement(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], new Vector2(Location.X, Location.Y + Interval / 2.0), new Vector2(wday, Interval / 2.0), GroupIndex - 1, new int[0], new[] { new CommandEntry { Command = Translations.Command.ReverserBackward } }, new Vector2(0.5, 0.5), 0, PanelResolution, PanelBottom, PanelCenter, Car.Driver);
										}
									}
								}
							}
						}
						break;
					case Panel2Sections.DigitalGauge:
						if (Block.GetValue(Panel2Key.Radius, out Radius) && Radius != 0)
						{
							double InitialAngle = -120, LastAngle = 120;
							double Minimum = 0.0, Maximum = 1000.0;
							Block.TryGetValue(Panel2Key.Function, ref Function);
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							Block.TryGetValue(Panel2Key.InitialAngle, ref InitialAngle);
							InitialAngle = InitialAngle.ToRadians();
							Block.TryGetValue(Panel2Key.LastAngle, ref LastAngle);
							LastAngle = LastAngle.ToRadians();
							Block.TryGetValue(Panel2Key.Minimum, ref Minimum);
							Block.TryGetValue(Panel2Key.Maximum, ref Maximum);
							Block.TryGetColor24(Panel2Key.Color, ref Color);
							Block.GetValue(Panel2Key.Step, out double Step);

							if (Plugin.CurrentOptions.EnableBveTsHacks && trainName == "BOEING-737")
							{
								/*
								 * BVE4 stacks objects within layers in order
								 * If two overlapping objects are declared in the same
								 * layer in openBVE, this causes Z-fighting
								 *
								 */
								if (Subject == Panel2Subject.SAP || Subject == Panel2Subject.BP)
								{
									Layer = 4;
								}
							}

							if (Radius == 0.0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Radius is required to be non-zero in " + Block.Key + " in " + fileName);
							}
							if (Minimum == Maximum)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Block.Key + " in " + fileName);
								Radius = 0.0;
							}
							if (Math.Abs(InitialAngle - LastAngle) > 6.28318531)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Block.Key + " in " + fileName);
							}
							// create element
							int j = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X - Radius, Location.Y - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, null, null, Color);
							InitialAngle += Math.PI;
							LastAngle += Math.PI;
							double x0 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
							double y0 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
							double z0 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
							double x1 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
							double y1 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
							double z1 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
							double x2 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
							double y2 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
							double z2 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
							double x3 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
							double y3 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
							double z3 = Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
							double cx = 0.25 * (x0 + x1 + x2 + x3);
							double cy = 0.25 * (y0 + y1 + y2 + y3);
							double cz = 0.25 * (z0 + z1 + z2 + z3);
							VertexTemplate[] vertices = new VertexTemplate[11];
							for (int v = 0; v < 11; v++)
							{
								vertices[v] = new Vertex();
							}
							int[][] faces = 
							{
								new[] { 0, 1, 2 },
								new[] { 0, 3, 4 },
								new[] { 0, 5, 6 },
								new[] { 0, 7, 8 },
								new[] { 0, 9, 10 }
							};
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, Color);
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDInitialAngle = InitialAngle;
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDLastAngle = LastAngle;
							Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDVectors = new[]
							{
								new Vector3(x0, y0, z0),
								new Vector3(x1, y1, z1),
								new Vector3(x2, y2, z2),
								new Vector3(x3, y3, z3),
								new Vector3(cx, cy, cz)
							};
							string f = GetStackLanguageFromSubject(Car.baseTrain, Subject, subjectIndex, subjectSuffix);
							double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
							double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
							if (Step == 1.0)
							{
								f += " floor";
							}
							else if (Step != 0.0)
							{
								string s = (1.0 / Step).ToString(Culture);
								string t = Step.ToString(Culture);
								f += " " + s + " * floor " + t + " *";
							}
							f += " " + a1.ToString(Culture) + " " + a0.ToString(Culture) + " fma";
							try
							{
								if (!string.IsNullOrEmpty(Function))
								{
									Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDFunction = new FunctionScript(Plugin.CurrentHost, Function, true);
								}
								else
								{
									Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].LEDFunction = new FunctionScript(Plugin.CurrentHost, f, false);
								}
							}
							catch
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
							}
						}
						break;
					case Panel2Sections.Timetable:
						{
							Block.GetVector2(Panel2Key.Location, ',', out Vector2 Location);
							Block.GetValue(Panel2Key.Width, out double Width);
							Block.GetValue(Panel2Key.Height, out double Height);
							if (Width <= 0 || Height <= 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Width and Height are required to be positive in " + Block.Key + " in " + fileName);
								break;
							}

							if (Block.GetColor24(Panel2Key.TransparentColor, out _))
							{
								// The original code read this, but never used it
								// Deliberately deprecate.
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TransparentColor is not supported in " + Block.Key + " in " + fileName);
							}

							int j = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], Location.X, Location.Y, Width, Height, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, null, null, Color32.White);
							try
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.CurrentHost, "panel2timetable", false);
							}
							catch
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
							}

							Plugin.CurrentHost.AddObjectForCustomTimeTable(Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[j]);
						}
						break;
					case Panel2Sections.Windscreen:
						Vector2 topLeft = new Vector2(PanelLeft, PanelTop);
						Vector2 bottomRight = new Vector2(PanelRight, PanelBottom);
						int numberOfDrops = 16, dropSize = 16;
						double wipeSpeed = 1.0, holdTime = 1.0, dropLife = 10.0;
						WiperPosition restPosition = WiperPosition.Left, holdPosition = WiperPosition.Left;
						string[] daytimeDropFiles, nighttimeDropFiles, daytimeFlakeFiles, nighttimeFlakeFiles;
						try
						{
							daytimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Day"), "drop*.png");
							daytimeFlakeFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Day"), "flake*.png");
							nighttimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Night"), "drop*.png");
							nighttimeFlakeFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Night"), "flake*.png");
						}
						catch
						{
							break;
						}

						Block.TryGetVector2(Panel2Key.TopLeft, ',', ref topLeft);
						Block.TryGetVector2(Panel2Key.BottomRight, ',', ref bottomRight);
						Block.TryGetValue(Panel2Key.NumberOfDrops, ref numberOfDrops, NumberRange.NonNegative);
						Block.TryGetValue(Panel2Key.DropSize, ref dropSize, NumberRange.Positive);
						Block.TryGetValue(Panel2Key.DropLife, ref dropLife, NumberRange.Positive);
						Block.TryGetStringArray(Panel2Key.DaytimeDrops, ',', ref daytimeDropFiles);
						Block.TryGetStringArray(Panel2Key.NighttimeDrops, ',', ref nighttimeDropFiles);
						Block.TryGetStringArray(Panel2Key.DaytimeFlakes, ',', ref daytimeFlakeFiles);
						Block.TryGetStringArray(Panel2Key.NighttimeFlakes, ',', ref nighttimeFlakeFiles);
						Block.TryGetValue(Panel2Key.WipeSpeed, ref wipeSpeed);
						Block.TryGetValue(Panel2Key.WiperHoldTime, ref holdTime);
						if (Block.GetValue(Panel2Key.RestPosition, out string restPos))
						{
							switch (restPos.ToLowerInvariant())
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid in " + Block.Key + " in " + fileName);
									break;
							}
						}
						if (Block.GetValue(Panel2Key.HoldPosition, out string holdPos))
						{
							switch (holdPos.ToLowerInvariant())
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid in " + Block.Key + " in " + fileName);
									break;
							}
						}

						/*
						 * Ensure we have the same number of drops for day + night
						 * NOTE: If a drop is missing, we may get slightly odd effects, but can't be helped
						 * Raindrops ought to be blurry, and they're small enough anyway...
						 */
						int MD = Math.Max(daytimeDropFiles.Length, nighttimeDropFiles.Length);
						MD = Math.Max(daytimeFlakeFiles.Length, MD);
						MD = Math.Max(nighttimeFlakeFiles.Length, MD);
						if (daytimeDropFiles.Length < MD)
						{
							Array.Resize(ref daytimeDropFiles, MD);
						}

						if (daytimeFlakeFiles.Length < MD)
						{
							Array.Resize(ref daytimeFlakeFiles, MD);
						}

						if (nighttimeDropFiles.Length < MD)
						{
							Array.Resize(ref nighttimeDropFiles, MD);
						}


						if (nighttimeFlakeFiles.Length < MD)
						{
							Array.Resize(ref nighttimeFlakeFiles, MD);
						}

						List<Texture> daytimeDrops = LoadDrops(trainPath, daytimeDropFiles, TransparentColor, "drop");
						List<Texture> daytimeFlakes = LoadDrops(trainPath, daytimeFlakeFiles, TransparentColor, "flake");
						List<Texture> nighttimeDrops = LoadDrops(trainPath, nighttimeDropFiles, TransparentColor, "drop");
						List<Texture> nighttimeFlakes = LoadDrops(trainPath, nighttimeFlakeFiles, TransparentColor, "flake");

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
							int panelDropIndex = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, daytimeDrops[DropTexture], nighttimeDrops[DropTexture], Color32.White);
							int panelFlakeIndex = CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[GroupIndex], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Car.Driver, daytimeFlakes[DropTexture], nighttimeFlakes[DropTexture], Color32.White);
							string f = drop + " raindrop";
							string f2 = drop + " snowflake";
							try
							{
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[panelDropIndex].StateFunction = new FunctionScript(Plugin.CurrentHost, f + " 1 == --", false);
								Car.CarSections[CarSectionType.Interior].Groups[GroupIndex].Elements[panelFlakeIndex].StateFunction = new FunctionScript(Plugin.CurrentHost, f2 + " 1 == --", false);
							}
							catch
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Block.Key + " in " + fileName);
							}

							currentDropX += dropInterval;
						}
						break;
				}
				Block.ReportErrors();
			}
		}

		private List<Texture> LoadDrops(string TrainPath, string[] dropFiles, Color24 TransparentColor, string compatabilityString)
		{
			List<Texture> drops = new List<Texture>();
			for (int l = 0; l < dropFiles.Length; l++)
			{
				string currentDropFile = !Path.IsPathRooted(dropFiles[l]) ? Path.CombineFile(TrainPath, dropFiles[l]) : dropFiles[l];
				if (string.IsNullOrEmpty(currentDropFile) || !File.Exists(currentDropFile))
				{
					currentDropFile = Path.CombineFile(Plugin.FileSystem.DataFolder, "Compatability\\Windscreen\\Day\\" + compatabilityString + Plugin.RandomNumberGenerator.Next(1, 4) + ".png");
					TransparentColor = Color24.Blue;
				}
				Plugin.CurrentHost.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out var drop, true, 20000);
				drops.Add(drop);
			}

			return drops;
		}

		internal string GetInfixFunction(AbstractTrain Train, Panel2Subject Subject, int SubjectIndex, string SubjectSuffix, double Minimum, double Maximum, int Width, int TextureWidth)
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

			string subjectText = GetStackLanguageFromSubject(Train, Subject, SubjectIndex, SubjectSuffix);

			if (!string.IsNullOrEmpty(subjectText))
			{
				return $"{subjectText} {Maximum} < {subjectText} {Minimum} > {subjectText} {Minimum + mp} - {range} * 0 ? {ftc} ?";
			}

			return string.Empty;
		}

		/// <summary>Converts a Panel2.cfg subject to an animation function stack</summary>
		/// <param name="Train">The train</param>
		/// <param name="Subject">The subject to convert</param>
		/// <param name="SubjectIndex">The index of the ATS etc. function if applicable</param>
		/// <param name="Suffix">Optional suffix</param>
		/// <returns>The parsed animation function stack</returns>
		internal string GetStackLanguageFromSubject(AbstractTrain Train, Panel2Subject Subject, int SubjectIndex, string Suffix) 
		{
			// transform subject
			string Code;
			switch (Subject) {
				case Panel2Subject.Acc:
					Code = "acceleration";
					break;
				case Panel2Subject.Motor:
					Code = "accelerationmotor";
					break;
				case Panel2Subject.True:
					Code = "1";
					break;
				case Panel2Subject.kmph:
					Code = "speedometer abs 3.6 *";
					break;
				case Panel2Subject.mph:
					Code = "speedometer abs 2.2369362920544 *";
					break;
				case Panel2Subject.ms:
					Code = "speedometer abs";
					break;
				case Panel2Subject.LocoBrakeCylinder:
					Code = Train.DriverCar + " brakecylinderindex 0.001 *";
					break;
				case Panel2Subject.BC:
					Code = "brakecylinder 0.001 *";
					break;
				case Panel2Subject.MR:
					Code = "mainreservoir 0.001 *";
					break;
				case Panel2Subject.SAP:
					Code = "straightairpipe 0.001 *";
					break;
				case Panel2Subject.LocoBrakePipe:
					Code = Train.DriverCar + " brakepipeindex 0.001 *";
					break;
				case Panel2Subject.BP:
					Code = "brakepipe 0.001 *";
					break;
				case Panel2Subject.ER:
					Code = "equalizingreservoir 0.001 *";
					break;
				case Panel2Subject.Door:
					Code = "1 doors -";
					break;
				case Panel2Subject.DoorL:
				case Panel2Subject.DoorR:
					if (SubjectIndex < Train.NumberOfCars)
					{
						Code = SubjectIndex + (Subject == Panel2Subject.DoorL ? " leftdoorsindex ceiling" : " rightdoorsindex ceiling");
					}
					else
					{
						Plugin.CurrentHost.AddMessage("CarIndex was invalid for " + Subject);
						return "2";
					}
					break;
				case Panel2Subject.CsC:
					Code = "constSpeed";
					break;
				case Panel2Subject.Power:
					Code = "brakeNotchLinear 0 powerNotch ?";
					break;
				case Panel2Subject.LocoBrake:
					Code = "locoBrakeNotch";
					break;
				case Panel2Subject.Brake:
					Code = "brakeNotchLinear";
					break;
				case Panel2Subject.Rev:
					Code = "reverserNotch ++";
					break;
				case Panel2Subject.Hour:
					Code = "0.000277777777777778 time * 24 mod floor";
					break;
				case Panel2Subject.Min:
					Code = "0.0166666666666667 time * 60 mod floor";
					break;
				case Panel2Subject.Sec:
					Code = "time 60 mod floor";
					break;
				case Panel2Subject.ATC:
					Code = "271 pluginstate";
					break;
				case Panel2Subject.Klaxon:
				case Panel2Subject.Horn:
				case Panel2Subject.PrimaryKlaxon:
				case Panel2Subject.PrimaryHorn:
				case Panel2Subject.SecondaryKlaxon:
				case Panel2Subject.SecondaryHorn:
				case Panel2Subject.DoorButtonL:
				case Panel2Subject.DoorButtonR:
				case Panel2Subject.WiperPosition:
				case Panel2Subject.Wheelslip:
				case Panel2Subject.Sanders:
				case Panel2Subject.SandLevel:
				case Panel2Subject.RouteLimit:
					Code = Subject.ToString().ToLowerInvariant();
					break;
				default:
					Code = SubjectIndex + " " + Subject.ToString().ToLowerInvariant();
					break;
			}
			return Code + Suffix;
		}

		internal void CreateElement(ref ElementsGroup Group, double Left, double Top, Vector2 RelativeRotationCenter, double Distance, double PanelResolution, double PanelBottom, Vector2 PanelCenter, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, bool AddStateToLastElement = false)
		{
			if (DaytimeTexture != null)
			{
				CreateElement(ref Group, Left, Top, DaytimeTexture.Width, DaytimeTexture.Height, RelativeRotationCenter, Distance, PanelResolution, PanelBottom, PanelCenter, Driver, DaytimeTexture, NighttimeTexture, Color32.White, AddStateToLastElement);
			}
		}

		internal int CreateElement(ref ElementsGroup Group, double Left, double Top, double Width, double Height, Vector2 RelativeRotationCenter, double Distance, double PanelResolution, double PanelBottom, Vector2 PanelCenter, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement = false)
		{
			if (Width == 0 || Height == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Attempted to create an invalid size element");
			}
			double WorldWidth, WorldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height) {
				WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
			} else {
				WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance / Plugin.Renderer.Screen.AspectRatio;
				WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
			}
			double x0 = Left / PanelResolution;
			double x1 = (Left + Width) / PanelResolution;
			double y0 = (PanelBottom - Top) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelBottom - (Top + Height)) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd; x1 += xd;
			double yt = PanelBottom - PanelResolution / Plugin.Renderer.Screen.AspectRatio;
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
			StaticObject Object = new StaticObject(Plugin.CurrentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 0, 2, 3 }, FaceFlags.Triangles) }; //Must create as a single face like this to avoid Z-sort issues with overlapping bits
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = new MaterialFlags();
			if (DaytimeTexture != null)
			{
				Object.Mesh.Materials[0].Flags |= MaterialFlags.TransparentColor;

				if (NighttimeTexture != null)
				{
					// In BVE4 and versions of OpenBVE prior to v1.7.1.0, elements with NighttimeImage defined are rendered with lighting disabled.
					Object.Mesh.Materials[0].Flags |= MaterialFlags.DisableLighting;
				}
			}
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
				int n = Group.Elements.Length - 1;
				int j = Group.Elements[n].States.Length;
				Array.Resize(ref Group.Elements[n].States, j + 1);
				Group.Elements[n].States[j] = new ObjectState
				{
					Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z),
					Prototype = Object
				};
				return n;
			} else {
				int n = Group.Elements.Length;
				Array.Resize(ref Group.Elements, n + 1);
				Group.Elements[n] = new AnimatedObject(Plugin.CurrentHost, Object);
				Group.Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Plugin.CurrentHost.CreateDynamicObject(ref Group.Elements[n].internalObject);
				return n;
			}
		}

	}
}
