using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LibRender2.Trains;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using TrainManager.Car;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	internal class PanelXmlParser
	{
		internal Plugin Plugin;

		internal PanelXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		// constants
		private const double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double EyeDistance = 1.0;

		/// <summary>Parses a openBVE panel.xml file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="Train.TrainFolder">The on-disk path to the train</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal void ParsePanelXml(string PanelFile, TrainBase Train, int Car)
		{
			// The current XML file to load
			string FileName = PanelFile;
			if (!File.Exists(FileName))
			{
				FileName = Path.CombineFile(Train.TrainFolder, PanelFile);
			}
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);

			// Check for null
			if (CurrentXML.Root == null)
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException(FileName + " does not appear to be a valid XML file.");
			}

			IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("Panel");

			// Check this file actually contains OpenBVE panel definition elements
			if (DocumentElements == null || !DocumentElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException(FileName + " is not a valid PanelXML file.");
			}

			foreach (XElement element in DocumentElements)
			{
				ParsePanelNode(element, FileName, Train, Car, ref Train.Cars[Car].CarSections[0], 0, 0);
			}
		}

		private void ParsePanelNode(XElement Element, string FileName, TrainBase Train, int Car, ref CarSection CarSection, int GroupIndex, int OffsetLayer, double PanelResolution = 1024.0, double PanelLeft = 0.0, double PanelRight = 1024.0, double PanelTop = 0.0, double PanelBottom = 1024.0, double PanelCenterX = 0, double PanelCenterY = 512, double PanelOriginX = 0, double PanelOriginY = 512)
		{
			//Train name, used for hacks detection
			string trainName = new DirectoryInfo(Train.TrainFolder).Name.ToUpperInvariant();

			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			// initialize
			Vector2 PanelCenter = new Vector2(PanelCenterX, PanelCenterY);
			Vector2 PanelOrigin = new Vector2(PanelOriginX, PanelOriginY);

			if (GroupIndex == 0)
			{
				string PanelDaytimeImage = null;
				string PanelNighttimeImage = null;
				Color24 PanelTransparentColor = Color24.Blue;

				foreach (XElement SectionElement in Element.Elements())
				{
					string Section = SectionElement.Name.LocalName;

					switch (Section.ToLowerInvariant())
					{
						case "this":
							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "resolution":
										double pr = 0.0;
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out pr))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										if (pr > 100)
										{
											PanelResolution = pr;
										}
										else
										{
											//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
											//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
											Plugin.currentHost.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "left":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelLeft))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line" + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "right":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelRight))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										if (Plugin.CurrentOptions.EnableBveTsHacks)
										{
											switch ((int)PanelRight)
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
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelTop))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "bottom":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelBottom))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											PanelDaytimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(PanelDaytimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + PanelDaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												PanelDaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											PanelNighttimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(PanelNighttimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + PanelNighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												PanelNighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out PanelTransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}

										break;
									case "center":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out PanelCenter.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelCenter.Y))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (Plugin.CurrentOptions.EnableBveTsHacks)
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
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											break;
										}
									case "origin":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out PanelOrigin.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelOrigin.Y))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (Plugin.CurrentOptions.EnableBveTsHacks)
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
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											break;
										}
								}
							}
							break;
					}
				}

				// camera restriction
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
				Train.Cars[Car].CameraRestriction.BottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, EyeDistance);
				Train.Cars[Car].CameraRestriction.TopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, EyeDistance);
				Train.Cars[Car].DriverYaw = Math.Atan((PanelCenter.X - PanelOrigin.X) * WorldWidth / PanelResolution);
				Train.Cars[Car].DriverPitch = Math.Atan((PanelOrigin.Y - PanelCenter.Y) * WorldWidth / PanelResolution);

				// create panel
				if (PanelDaytimeImage != null)
				{
					if (!File.Exists(PanelDaytimeImage))
					{
						Plugin.currentHost.AddMessage(MessageType.Error, true, "The daytime panel bitmap could not be found in " + FileName);
					}
					else
					{
						Plugin.currentHost.RegisterTexture(PanelDaytimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out var tday, true);
						Texture tnight = null;
						if (PanelNighttimeImage != null)
						{
							if (!File.Exists(PanelNighttimeImage))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, true, "The nighttime panel bitmap could not be found in " + FileName);
							}
							else
							{
								Plugin.currentHost.RegisterTexture(PanelNighttimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tnight);
							}
						}
						Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], 0.0, 0.0, new Vector2(0.5, 0.5), OffsetLayer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, tday, tnight);
					}
				}
			}

			int currentSectionElement = 0;
			int numberOfSectionElements = Element.Elements().Count();
			double invfac = numberOfSectionElements == 0 ? 0.4 : 0.4 / numberOfSectionElements;

			foreach (XElement SectionElement in Element.Elements())
			{
				Plugin.CurrentProgress = Plugin.LastProgress + invfac * currentSectionElement;
				if ((currentSectionElement & 4) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}

				string Section = SectionElement.Name.LocalName;

				switch (SectionElement.Name.LocalName.ToLowerInvariant())
				{
					case "screen":
						if (GroupIndex == 0)
						{
							int n = 0;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "number":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out n))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (n + 1 >= CarSection.Groups.Length)
							{
								Array.Resize(ref CarSection.Groups, n + 2);
								CarSection.Groups[n + 1] = new ElementsGroup();
							}

							ParsePanelNode(SectionElement, FileName, Train, Car, ref CarSection, n + 1, Layer, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter.X, PanelCenter.Y, PanelOriginX, PanelOriginY);
						}
						break;
					case "touch":
						if(GroupIndex > 0)
						{
							Vector2 Location = new Vector2();
							Vector2 Size = new Vector2();
							int JumpScreen = GroupIndex - 1;
							List<int> SoundIndices = new List<int>();
							List<CommandEntry> CommandEntries = new List<CommandEntry>();
							CommandEntry CommandEntry = new CommandEntry();
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "location":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Location.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Location.Y))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "size":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Size.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Size.Y))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "jumpscreen":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out JumpScreen))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "soundindex":
										if (Value.Length != 0)
										{
											if (!NumberFormats.TryParseIntVb6(Value, out var SoundIndex))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												break;
											}
											SoundIndices.Add(SoundIndex);
										}
										break;
									case "command":
										{
											if (!CommandEntries.Contains(CommandEntry))
											{
												CommandEntries.Add(CommandEntry);
											}

											if (string.Compare(Value, "N/A", StringComparison.InvariantCultureIgnoreCase) == 0)
											{
												break;
											}

											int i;
											for (i = 0; i < Translations.CommandInfos.Length; i++)
											{
												if (string.Compare(Value, Translations.CommandInfos[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
												{
													break;
												}
											}
											if (i == Translations.CommandInfos.Length || Translations.CommandInfos[i].Type != Translations.CommandType.Digital)
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												break;
											}
											CommandEntry.Command = Translations.CommandInfos[i].Command;
										}
										break;
									case "commandoption":
										if (!CommandEntries.Contains(CommandEntry))
										{
											CommandEntries.Add(CommandEntry);
										}

										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out CommandEntry.Option))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "soundentries":
										if (!KeyNode.HasElements)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchSoundEntryNode(FileName, KeyNode, SoundIndices);
										break;
									case "commandentries":
										if (!KeyNode.HasElements)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchCommandEntryNode(FileName, KeyNode, CommandEntries);
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}
							CreateTouchElement(CarSection.Groups[GroupIndex], Location, Size, JumpScreen, SoundIndices.ToArray(), CommandEntries.ToArray(), new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver);
						}
						break;
					case "pilotlamp":
						{
							string Subject = "true";
							double LocationX = 0.0, LocationY = 0.0;
							string DaytimeImage = null, NighttimeImage = null;
							Color24 TransparentColor = Color24.Blue;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "subject":
										Subject = Value;
										break;
									case "location":
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(DaytimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(NighttimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}
							if (DaytimeImage == null)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday, true);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								int w = tday.Width;
								int h = tday.Height;
								int j = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, tday, tnight, Color32.White);
								string f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f + " 1 == --", false);
							}
						}
						break;
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

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "subject":
										Subject = Value;
										break;
									case "location":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "radius":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Radius == 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											Radius = 16.0;
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(DaytimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(NighttimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "color":
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "origin":
										{
											int k = Value.IndexOf(',');
											if (k >= 0)
											{
												string a = Value.Substring(0, k).TrimEnd();
												string b = Value.Substring(k + 1).TrimStart();
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out OriginX))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out OriginY))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
													OriginX = -OriginX;
												}
												OriginDefined = true;
											}
											else
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "initialangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "lastangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "naturalfreq":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out NaturalFrequency))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (NaturalFrequency < 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											NaturalFrequency = -NaturalFrequency;
										}
										break;
									case "dampingratio":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out DampingRatio))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (DampingRatio < 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											DampingRatio = -DampingRatio;
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
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
							}

							if (DaytimeImage == null)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday, true);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								if (!OriginDefined)
								{
									OriginX = 0.5 * tday.Width;
									OriginY = 0.5 * tday.Height;
								}
								double ox = OriginX / tday.Width;
								double oy = OriginY / tday.Height;
								double n = Radius == 0.0 | OriginY == 0.0 ? 1.0 : Radius / OriginY;
								double nx = n * tday.Width;
								double ny = n * tday.Height;
								int j = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX - ox * nx, LocationY - oy * ny, nx, ny, new Vector2(ox, oy), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, tday, tnight, Color);
								CarSection.Groups[GroupIndex].Elements[j].RotateZDirection = Vector3.Backward;
								CarSection.Groups[GroupIndex].Elements[j].RotateXDirection = Vector3.Right;
								CarSection.Groups[GroupIndex].Elements[j].RotateYDirection = Vector3.Cross(CarSection.Groups[GroupIndex].Elements[j].RotateZDirection, CarSection.Groups[GroupIndex].Elements[j].RotateXDirection);
								string f;
								switch (Subject.ToLowerInvariant())
								{
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
										f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
										break;
								}
								InitialAngle = InitialAngle.ToRadians();
								LastAngle = LastAngle.ToRadians();
								double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
								double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
								f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
								if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0)
								{
									CarSection.Groups[GroupIndex].Elements[j].RotateZDamping = new Damping(NaturalFrequency, DampingRatio);
								}
								CarSection.Groups[GroupIndex].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, f, false);
								if (Backstop)
								{
									CarSection.Groups[GroupIndex].Elements[j].RotateZFunction.Minimum = InitialAngle;
									CarSection.Groups[GroupIndex].Elements[j].RotateZFunction.Maximum = LastAngle;
								}
							}
						}
						break;
					case "lineargauge":
						{
							string Subject = "true";
							int Width = 0;
							Vector2 Direction = new Vector2(1, 0);
							double LocationX = 0.0, LocationY = 0.0;
							string DaytimeImage = null, NighttimeImage = null;
							double Minimum = 0.0, Maximum = 0.0;
							Color24 TransparentColor = Color24.Blue;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "subject":
										Subject = Value;
										break;
									case "location":
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "width":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Width))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "direction":
										{
											string[] s = Value.Split(',');
											if (s.Length == 2)
											{
												if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out Direction.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
													break;
												}
												if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out Direction.Y))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in  LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
													break;
												}
											}
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(DaytimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(NighttimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (DaytimeImage == null)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday, true);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								int w = tday.Width;
								int h = tday.Height;
								int j = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, tday, tnight, Color32.White);
								if (Maximum < Minimum)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Maximum value must be greater than minimum value " + Section + " in " + FileName);
									break;
								}
								string tf = Plugin.Panel2CfgParser.GetInfixFunction(Train, Subject, Minimum, Maximum, Width, tday.Width, Section + " in " + FileName);
								if (tf != String.Empty)
								{
									CarSection.Groups[GroupIndex].Elements[j].TextureShiftXDirection = Direction;
									CarSection.Groups[GroupIndex].Elements[j].TextureShiftXFunction = new FunctionScript(Plugin.currentHost, tf, false);
								}
							}
						}
						break;
					case "digitalnumber":
						{
							string Subject = "true";
							double LocationX = 0.0, LocationY = 0.0;
							string DaytimeImage = null, NighttimeImage = null;
							Color24 TransparentColor = Color24.Blue;
							double Layer = 0.0;
							int Interval = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "subject":
										Subject = Value;
										break;
									case "location":
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(DaytimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(Train.TrainFolder, Value);
											if (!File.Exists(NighttimeImage))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "interval":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Interval))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Interval <= 0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (DaytimeImage == null)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							if (Interval <= 0)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Interval is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null & Interval > 0)
							{
								Plugin.currentHost.QueryTextureDimensions(DaytimeImage, out var wday, out var hday);
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
											Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday[k]);
										}
										else if (k * Interval >= hday)
										{
											numFrames = k;
											Array.Resize(ref tday, k);
										}
										else
										{
											Plugin.currentHost.RegisterTexture(DaytimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wday, hday - (k * Interval)), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday[k]);
										}
									}
									if (NighttimeImage != null)
									{
										Plugin.currentHost.QueryTextureDimensions(NighttimeImage, out var wnight, out var hnight);
										tnight = new Texture[numFrames];
										for (int k = 0; k < numFrames; k++)
										{
											if ((k + 1) * Interval <= hnight)
											{
												Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, Interval), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight[k]);
											}
											else if (k * Interval > hnight)
											{
												tnight[k] = null;
											}
											else
											{
												Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(new TextureClipRegion(0, k * Interval, wnight, hnight - (k * Interval)), new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight[k]);
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
										int l = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX, LocationY, wday, Interval, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, tday[k], tnight[k], Color32.White, k != 0);
										if (k == 0) j = l;
									}
									string f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
									CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f, false);
								}
							}
						}
						break;
					case "digitalgauge":
						{
							string Subject = "true";
							double LocationX = 0.0, LocationY = 0.0;
							Color32 Color = Color32.Black;
							double Radius = 0.0;
							int Layer = 0;
							double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
							double Minimum = 0.0, Maximum = 1000.0;
							double Step = 0.0;
							int LineNumber = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "subject":
										Subject = Value;
										break;
									case "location":
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "radius":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Radius == 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											Radius = 16.0;
										}
										break;
									case "color":
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "initialangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											InitialAngle = InitialAngle.ToRadians();
										}
										break;
									case "lastangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											LastAngle = LastAngle.ToRadians();
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "step":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Step))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (Radius == 0.0)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Radius is required to be non-zero in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
							}
							if (Minimum == Maximum)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								Radius = 0.0;
							}
							if (Math.Abs(InitialAngle - LastAngle) > 6.28318531)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
							}
							if (Radius != 0.0)
							{
								// create element
								int j = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX - Radius, LocationY - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, null, null, Color);
								InitialAngle += Math.PI;
								LastAngle += Math.PI;
								double x0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.X;
								double y0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Y;
								double z0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[0].Coordinates.Z;
								double x1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.X;
								double y1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Y;
								double z1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[1].Coordinates.Z;
								double x2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.X;
								double y2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Y;
								double z2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[2].Coordinates.Z;
								double x3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.X;
								double y3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Y;
								double z3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh.Vertices[3].Coordinates.Z;
								double cx = 0.25 * (x0 + x1 + x2 + x3);
								double cy = 0.25 * (y0 + y1 + y2 + y3);
								double cz = 0.25 * (z0 + z1 + z2 + z3);
								VertexTemplate[] vertices = new VertexTemplate[11];
								for (int v = 0; v < 11; v++)
								{
									vertices[v] = new Vertex();
								}
								int[][] faces = new int[][]
								{
									new[] { 0, 1, 2 },
									new[] { 0, 3, 4 },
									new[] { 0, 5, 6 },
									new[] { 0, 7, 8 },
									new[] { 0, 9, 10 }
								};
								CarSection.Groups[GroupIndex].Elements[j].States[0].Prototype.Mesh = new Mesh(vertices, faces, Color);
								CarSection.Groups[GroupIndex].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDInitialAngle = InitialAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDLastAngle = LastAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDVectors = new[]
								{
									new Vector3(x0, y0, z0),
									new Vector3(x1, y1, z1),
									new Vector3(x2, y2, z2),
									new Vector3(x3, y3, z3),
									new Vector3(cx, cy, cz)
								};
								string f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
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
								CarSection.Groups[GroupIndex].Elements[j].LEDFunction = new FunctionScript(Plugin.currentHost, f, false);
							}
							else
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Radius is required to be specified in " + Section + " in " + FileName);
							}
						}
						break;
					case "timetable":
						{
							double LocationX = 0.0, LocationY = 0.0;
							double Width = 0.0, Height = 0.0;
							//We read the transparent color for the timetable from the config file, but it is never used
							//TODO: Fix or depreciate??
							Color24 TransparentColor = Color24.Blue;
							double Layer = 0.0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "location":
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out LocationX))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "width":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Width))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Width <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "height":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Height))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Height <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							// create element
							if (Width <= 0.0)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
							}
							if (Height <= 0.0)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
							}
							if (Width > 0.0 & Height > 0.0)
							{
								int j = Plugin.Panel2CfgParser.CreateElement(ref CarSection.Groups[GroupIndex], LocationX, LocationY, Width, Height, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, null, null, Color32.White);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, "timetable", false);
								Plugin.currentHost.AddObjectForCustomTimeTable(CarSection.Groups[GroupIndex].Elements[j]);
							}
						}
						break;
					case "windscreen":
					{
						Vector2 topLeft = new Vector2(PanelLeft, PanelTop);
						Vector2 bottomRight = new Vector2(PanelRight, PanelBottom);
						int numberOfDrops = 16, Layer = 0, dropSize = 16;
						WiperPosition restPosition = WiperPosition.Left, holdPosition = WiperPosition.Left;
						List<string> daytimeDropFiles, nighttimeDropFiles;
						Color24 TransparentColor = Color24.Blue;
						double wipeSpeed = 1.0, holdTime = 1.0, dropLife = 10.0;
						try
						{
							daytimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Day")).ToList();
							nighttimeDropFiles = Directory.GetFiles(Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility\\Windscreen\\Night")).ToList();
						}
						catch
						{
							break;
						}

						foreach (XElement KeyNode in SectionElement.Elements())
						{
							string Key = KeyNode.Name.LocalName;
							string Value = KeyNode.Value;
							int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;
							int k;
							switch (Key.ToLowerInvariant())
							{
								case "topleft":
									k = Value.IndexOf(',');
									if (k >= 0)
									{
										string a = Value.Substring(0, k).TrimEnd();
										string b = Value.Substring(k + 1).TrimStart();
										if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out topLeft.X))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}

										if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out topLeft.Y))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "bottomright":
									k = Value.IndexOf(',');
									if (k >= 0)
									{
										string a = Value.Substring(0, k).TrimEnd();
										string b = Value.Substring(k + 1).TrimStart();
										if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out bottomRight.X))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}

										if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out bottomRight.Y))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "numberofdrops":
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out numberOfDrops))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "NumberOfDrops is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "dropsize":
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out numberOfDrops))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DropSize is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "daytimedrops":
									daytimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
									break;
								case "nighttimedrops":
									nighttimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
									break;
								case "transparentcolor":
									if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "layer":
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "wipespeed":
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out wipeSpeed))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "wiperholdtime":
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out holdTime))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "restposition":
								case "wiperrestposition":
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											break;
									}
									break;
								case "holdposition":
								case "wiperholdposition":
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperHoldPosition is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											break;
									}
									break;
								case "droplife":
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out dropLife))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DropLife is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
							}
						}

						List<Texture> daytimeDrops = new List<Texture>(), nighttimeDrops = new List<Texture>();
						/*
						 * Ensure we have the same number of drops for day + night
						 * NOTE: If a drop is missing, we may get slightly odd effects, but can't be helped
						 * Raindrops ought to be blurry, and they're small enough anyway...
						 */
						int MD = Math.Max(daytimeDropFiles.Count, nighttimeDropFiles.Count);
						if (daytimeDropFiles.Count < MD)
						{
							while (daytimeDropFiles.Count < MD)
							{
								daytimeDropFiles.Add(string.Empty);
							}
						}

						if (nighttimeDropFiles.Count < MD)
						{
							while (nighttimeDropFiles.Count < MD)
							{
								nighttimeDropFiles.Add(string.Empty);
							}
						}

						for (int l = 0; l < daytimeDropFiles.Count; l++)
						{
							string currentDropFile = !System.IO.Path.IsPathRooted(daytimeDropFiles[l]) ? Path.CombineFile(Train.TrainFolder, daytimeDropFiles[l]) : daytimeDropFiles[l];
							if (!File.Exists(currentDropFile))
							{
								currentDropFile = Path.CombineFile(Plugin.FileSystem.DataFolder, "Compatability\\Windscreen\\Day\\Drop" + Plugin.RandomNumberGenerator.Next(1, 4) + ".png");
								TransparentColor = Color24.Blue;
							}

							Plugin.currentHost.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out var drop, true);
							daytimeDrops.Add(drop);

						}

						for (int l = 0; l < nighttimeDropFiles.Count; l++)
						{
							string currentDropFile = !System.IO.Path.IsPathRooted(nighttimeDropFiles[l]) ? Path.CombineFile(Train.TrainFolder, nighttimeDropFiles[l]) : nighttimeDropFiles[l];
							if (!File.Exists(currentDropFile))
							{
								currentDropFile = Path.CombineFile(Plugin.FileSystem.DataFolder, "Compatability\\Windscreen\\Night\\Drop" + Plugin.RandomNumberGenerator.Next(1, 4) + ".png");
								TransparentColor = Color24.Blue;
							}

							Plugin.currentHost.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out var drop, true);
							nighttimeDrops.Add(drop);
						}

						double dropInterval = (bottomRight.X - topLeft.X) / numberOfDrops;
						double currentDropX = topLeft.X;
						Train.Cars[Train.DriverCar].Windscreen = new Windscreen(numberOfDrops, dropLife, Train.Cars[Train.DriverCar]);
						Train.Cars[Train.DriverCar].Windscreen.Wipers = new WindscreenWiper(Train.Cars[Train.DriverCar].Windscreen, restPosition, holdPosition, wipeSpeed, holdTime);
						// Create drops
						for (int drop = 0; drop < numberOfDrops; drop++)
						{
							int DropTexture = Plugin.RandomNumberGenerator.Next(daytimeDrops.Count);
							double currentDropY = Plugin.RandomNumberGenerator.NextDouble() * (bottomRight.Y - topLeft.Y) + topLeft.Y;
							int panelDropIndex = Plugin.Panel2CfgParser.CreateElement(ref Train.Cars[Car].CarSections[0].Groups[0], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer * StackDistance, PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver, daytimeDrops[DropTexture], nighttimeDrops[DropTexture], Color32.White);
							string f = drop + " raindrop";
							try
							{
								Train.Cars[Car].CarSections[0].Groups[GroupIndex].Elements[panelDropIndex].StateFunction = new FunctionScript(Plugin.currentHost, f + " 1 == --", false);
							}
							catch
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid animated function provided in " + Section + " in " + FileName);
							}

							currentDropX += dropInterval;
						}
					}
					break;
				}
				currentSectionElement++;
			}
		}


		private void ParseTouchSoundEntryNode(string fileName, XElement parent, ICollection<int> indices)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

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
									if (!NumberFormats.TryParseIntVb6(value, out var index))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
									}

									indices.Add(index);
								}
								break;
						}
					}
				}
			}
		}

		private void ParseTouchCommandEntryNode(string fileName, XElement parent, ICollection<CommandEntry> entries)
		{
			foreach (XElement childNode in parent.Elements())
			{
				if (childNode.Name.LocalName.ToLowerInvariant() != "entry")
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, $"Invalid entry node {childNode.Name.LocalName} in XML node {parent.Name.LocalName} at line {((IXmlLineInfo)childNode).LineNumber}");
				}
				else
				{
					CommandEntry entry = new CommandEntry();
					System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

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

								for (i = 0; i < Translations.CommandInfos.Length; i++)
								{
									if (string.Compare(value, Translations.CommandInfos[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
									{
										break;
									}
								}

								if (i == Translations.CommandInfos.Length || Translations.CommandInfos[i].Type != Translations.CommandType.Digital)
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
								}
								else
								{
									entry.Command = Translations.CommandInfos[i].Command;
								}
								break;
							case "option":
								if (value.Any())
								{
									if (!NumberFormats.TryParseIntVb6(value, out var option))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, $"value is invalid in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
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

		internal void CreateTouchElement(ElementsGroup Group, Vector2 Location, Vector2 Size, int ScreenIndex, int[] SoundIndices, CommandEntry[] CommandEntries, Vector2 RelativeRotationCenter, double Distance, double PanelResolution, double PanelBottom, Vector2 PanelCenter, Vector3 Driver)
		{
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
			double x0 = Location.X / PanelResolution;
			double x1 = (Location.X + Size.X) / PanelResolution;
			double y0 = (PanelBottom - Location.Y) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelBottom - (Location.Y + Size.Y)) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd;
			x1 += xd;
			double yt = PanelBottom - PanelResolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (PanelCenter.Y - yt) / (PanelBottom - yt) - 0.5;
			y0 += yd;
			y1 += yd;
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
			StaticObject Object = new StaticObject(Plugin.currentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = 0;
			Object.Mesh.Materials[0].Color = Color32.White;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = null;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = EyeDistance - Distance + Driver.Z;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TouchElement
			{
				Element = new AnimatedObject(Plugin.currentHost),
				JumpScreenIndex = ScreenIndex,
				SoundIndices = SoundIndices,
				ControlIndices = new int[CommandEntries.Length]
			};
			Group.TouchElements[n].Element.States = new[] { new ObjectState() };
			Group.TouchElements[n].Element.States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
			Group.TouchElements[n].Element.States[0].Prototype = Object;
			Group.TouchElements[n].Element.CurrentState = 0;
			Group.TouchElements[n].Element.internalObject = new ObjectState(Object);
			Plugin.currentHost.CreateDynamicObject(ref Group.TouchElements[n].Element.internalObject);
			int m = Plugin.CurrentControls.Length;
			Array.Resize(ref Plugin.CurrentControls, m + CommandEntries.Length);
			for (int i = 0; i < CommandEntries.Length; i++)
			{
				Plugin.CurrentControls[m + i].Command = CommandEntries[i].Command;
				Plugin.CurrentControls[m + i].Method = ControlMethod.Touch;
				Plugin.CurrentControls[m + i].Option = CommandEntries[i].Option;
				Group.TouchElements[n].ControlIndices[i] = m + i;
			}
		}
	}
}
