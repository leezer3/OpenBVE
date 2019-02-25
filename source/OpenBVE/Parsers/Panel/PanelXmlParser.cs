using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using Path = OpenBveApi.Path;

namespace OpenBve.Parsers.Panel
{
	internal static class PanelXmlParser
	{
		// constants
		private static double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double EyeDistance = 1.0;

		/// <summary>Parses a openBVE panel.xml file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="TrainPath">The on-disk path to the train</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal static void ParsePanelXml(string PanelFile, string TrainPath, TrainManager.Train Train, int Car)
		{
			// The current XML file to load
			string FileName = PanelFile;
			if (!File.Exists(FileName))
			{
				FileName = Path.CombineFile(TrainPath, PanelFile);
			}
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);

			// Check for null
			if (CurrentXML.Root == null)
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("Panel");

			// Check this file actually contains OpenBVE panel definition elements
			if (DocumentElements == null || !DocumentElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			foreach (XElement element in DocumentElements)
			{
				ParsePanelNode(element, FileName, TrainPath, Train, Car, Train.Cars[Car].CarSections[0], 0, 0);
			}
		}

		private static void ParsePanelNode(XElement Element, string FileName, string TrainPath, TrainManager.Train Train, int Car, TrainManager.CarSection CarSection, int GroupIndex, int OffsetLayer, double PanelResolution = 1024.0, double PanelLeft = 0.0, double PanelRight = 1024.0, double PanelTop = 0.0, double PanelBottom = 1024.0, double PanelCenterX = 0, double PanelCenterY = 512, double PanelOriginX = 0, double PanelOriginY = 512)
		{
			//Train name, used for hacks detection
			string trainName = new System.IO.DirectoryInfo(TrainPath).Name.ToUpperInvariant();

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
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										if (pr > 100)
										{
											PanelResolution = pr;
										}
										else
										{
											//Parsing very low numbers (Probable typos) for the panel resolution causes some very funky graphical bugs
											//Cap the minimum panel resolution at 100px wide (BVE1 panels are 480px wide, so this is probably a safe minimum)
											Interface.AddMessage(MessageType.Error, false, "A panel resolution of less than 100px was given at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "left":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelLeft))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line" + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "right":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelRight))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										if (Interface.CurrentOptions.EnableBveTsHacks)
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
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "bottom":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out PanelBottom))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											PanelDaytimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(PanelDaytimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + PanelDaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												PanelDaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											PanelNighttimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(PanelNighttimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + PanelNighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												PanelNighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out PanelTransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelCenter.Y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out PanelOrigin.Y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
				if (Screen.Width >= Screen.Height)
				{
					WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
					WorldHeight = WorldWidth / World.AspectRatio;
				}
				else
				{
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

				// create panel
				if (PanelDaytimeImage != null)
				{
					if (!System.IO.File.Exists(PanelDaytimeImage))
					{
						Interface.AddMessage(MessageType.Error, true, "The daytime panel bitmap could not be found in " + FileName);
					}
					else
					{
						Texture tday;
						Textures.RegisterTexture(PanelDaytimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tday);
						Texture tnight = null;
						if (PanelNighttimeImage != null)
						{
							if (!System.IO.File.Exists(PanelNighttimeImage))
							{
								Interface.AddMessage(MessageType.Error, true, "The nighttime panel bitmap could not be found in " + FileName);
							}
							else
							{
								Textures.RegisterTexture(PanelNighttimeImage, new TextureParameters(null, new Color24(PanelTransparentColor.R, PanelTransparentColor.G, PanelTransparentColor.B)), out tnight);
							}
						}
						OpenBVEGame.RunInRenderThread(() =>
						{
							Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
							//Textures.LoadTexture(tnight, OpenGlTextureWrapMode.ClampClamp);
						});
						Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], 0.0, 0.0, tday.Width, tday.Height, new Vector2(0.5, 0.5), OffsetLayer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, Color32.White, false);
					}
				}
			}

			int currentSectionElement = 0;
			int numberOfSectionElements = Element.Elements().Count();
			double invfac = numberOfSectionElements == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)numberOfSectionElements;

			foreach (XElement SectionElement in Element.Elements())
			{
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)currentSectionElement;
				if ((currentSectionElement & 4) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
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
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (n + 1 >= CarSection.Groups.Length)
							{
								Array.Resize(ref CarSection.Groups, n + 2);
								CarSection.Groups[n + 1] = new TrainManager.ElementsGroup
								{
									Elements = new ObjectManager.AnimatedObject[] { },
									Overlay = true
								};
							}

							ParsePanelNode(SectionElement, FileName, TrainPath, Train, Car, CarSection, n + 1, Layer, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter.X, PanelCenter.Y, PanelOriginX, PanelOriginY);
						}
						break;
					case "touch":
						if(GroupIndex > 0)
						{
							Vector2 Location = new Vector2();
							Vector2 Size = new Vector2();
							int JumpScreen = GroupIndex - 1;
							int SoundIndex = -1;
							Translations.Command Command = Translations.Command.None;
							int CommandOption = 0;

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
													Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Location.Y))
												{
													Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
													Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Size.Y))
												{
													Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "jumpscreen":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out JumpScreen))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "soundindex":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out SoundIndex))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "command":
										{
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
												Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											else
											{
												Command = Translations.CommandInfos[i].Command;
											}
										}
										break;
									case "commandoption":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out CommandOption))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}
							CreateTouchElement(CarSection.Groups[GroupIndex], Location, Size, JumpScreen, SoundIndex, Command, CommandOption, new Vector2(0.5, 0.5), PanelResolution, PanelBottom, PanelCenter, Train.Cars[Car].Driver);
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
												Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(DaytimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(NighttimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}
							if (DaytimeImage == null)
							{
								Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Texture tday;
								Textures.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Textures.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								OpenBVEGame.RunInRenderThread(() =>
								{
									Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
								});
								int w = tday.Width;
								int h = tday.Height;
								int j = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, Color32.White, false);
								string f = Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Program.CurrentHost, f + " 1 == --", false);
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
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

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
													Interface.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
												{
													Interface.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "radius":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Radius == 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											Radius = 16.0;
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(DaytimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(NighttimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "color":
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out OriginY))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
													OriginX = -OriginX;
												}
												OriginDefined = true;
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										break;
									case "initialangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "lastangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "naturalfreq":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out NaturalFrequency))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (NaturalFrequency < 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											NaturalFrequency = -NaturalFrequency;
										}
										break;
									case "dampingratio":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out DampingRatio))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (DampingRatio < 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											DampingRatio = -DampingRatio;
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
								if (!OriginDefined)
								{
									OriginX = 0.5 * w;
									OriginY = 0.5 * h;
								}
								double ox = OriginX / w;
								double oy = OriginY / h;
								double n = Radius == 0.0 | OriginY == 0.0 ? 1.0 : Radius / OriginY;
								double nx = n * w;
								double ny = n * h;
								int j = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX - ox * nx, LocationY - oy * ny, nx, ny, new Vector2(ox, oy), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, Color, false);
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
										f = Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
										break;
								}
								//Convert angles from degrees to radians
								InitialAngle *= 0.0174532925199433;
								LastAngle *= 0.0174532925199433;
								double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
								double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
								f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
								if (NaturalFrequency >= 0.0 & DampingRatio >= 0.0)
								{
									CarSection.Groups[GroupIndex].Elements[j].RotateZDamping = new Damping(NaturalFrequency, DampingRatio);
								}
								CarSection.Groups[GroupIndex].Elements[j].RotateZFunction = new FunctionScript(Program.CurrentHost, f, false);
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
												Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "width":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Width))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
													Interface.AddMessage(MessageType.Error, false, "X is invalid in LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
												}
												else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in  LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
												}
												else
												{
													Direction = new Vector2(x, y);
												}
											}
											else
											{
												Interface.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in LinearGauge Direction at line " + LineNumber.ToString(Culture) + " in file " + FileName);
											}
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(DaytimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(NighttimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (DaytimeImage == null)
							{
								Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Texture tday;
								Textures.RegisterTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tday);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Textures.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								OpenBVEGame.RunInRenderThread(() =>
								{
									Textures.LoadTexture(tday, OpenGlTextureWrapMode.ClampClamp);
								});
								int w = tday.Width;
								int h = tday.Height;
								int j = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX, LocationY, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday, tnight, Color32.White, false);
								if (Maximum < Minimum)
								{
									Interface.AddMessage(MessageType.Error, false, "Maximum value must be greater than minimum value " + Section + " in " + FileName);
									break;
								}
								string tf = Panel2CfgParser.GetInfixFunction(Train, Subject, Minimum, Maximum, Width, tday.Width, Section + " in " + FileName);
								if (tf != String.Empty)
								{
									CarSection.Groups[GroupIndex].Elements[j].TextureShiftXDirection = Direction;
									CarSection.Groups[GroupIndex].Elements[j].TextureShiftXFunction = new FunctionScript(Program.CurrentHost, tf, true);
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
												Interface.AddMessage(MessageType.Error, false, "Left is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Interface.AddMessage(MessageType.Error, false, "Top is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "daytimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											DaytimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(DaytimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + DaytimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												DaytimeImage = null;
											}
										}
										break;
									case "nighttimeimage":
										if (!System.IO.Path.HasExtension(Value)) Value += ".bmp";
										if (Path.ContainsInvalidChars(Value))
										{
											Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											NighttimeImage = Path.CombineFile(TrainPath, Value);
											if (!System.IO.File.Exists(NighttimeImage))
											{
												Interface.AddMessage(MessageType.Error, true, "FileName " + NighttimeImage + " could not be found in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												NighttimeImage = null;
											}
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "interval":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Interval))
										{
											Interface.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Interval <= 0)
										{
											Interface.AddMessage(MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (DaytimeImage == null)
							{
								Interface.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							if (Interval <= 0)
							{
								Interface.AddMessage(MessageType.Error, false, "Interval is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null & Interval > 0)
							{
								int wday, hday;
								Program.CurrentHost.QueryTextureDimensions(DaytimeImage, out wday, out hday);
								if (wday > 0 & hday > 0)
								{
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
									if (NighttimeImage != null)
									{
										int wnight, hnight;
										Program.CurrentHost.QueryTextureDimensions(NighttimeImage, out wnight, out hnight);
										tnight = new Texture[numFrames];
										for (int k = 0; k < numFrames; k++)
										{
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
										int l = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX, LocationY, (double)wday, (double)Interval, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, tday[k], tnight[k], Color32.White, k != 0);
										if (k == 0) j = l;
									}
									string f = Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
									CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Program.CurrentHost, f, false);
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
												Interface.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Interface.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "radius":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Radius == 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											Radius = 16.0;
										}
										break;
									case "color":
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "initialangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											InitialAngle *= 0.0174532925199433;
										}
										break;
									case "lastangle":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											LastAngle *= 0.0174532925199433;
										}
										break;
									case "minimum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "maximum":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "step":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Step))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (Radius == 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, "Radius is required to be non-zero in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
							}
							if (Minimum == Maximum)
							{
								Interface.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								Radius = 0.0;
							}
							if (Math.Abs(InitialAngle - LastAngle) > 6.28318531)
							{
								Interface.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
							}
							if (Radius != 0.0)
							{
								// create element
								int j = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX - Radius, LocationY - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, null, null, Color, false);
								InitialAngle = InitialAngle + Math.PI;
								LastAngle = LastAngle + Math.PI;
								double x0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.X;
								double y0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Y;
								double z0 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[0].Coordinates.Z;
								double x1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.X;
								double y1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Y;
								double z1 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[1].Coordinates.Z;
								double x2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.X;
								double y2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Y;
								double z2 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[2].Coordinates.Z;
								double x3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.X;
								double y3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Y;
								double z3 = CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh.Vertices[3].Coordinates.Z;
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
									new int[] { 0, 1, 2 },
									new int[] { 0, 3, 4 },
									new int[] { 0, 5, 6 },
									new int[] { 0, 7, 8 },
									new int[] { 0, 9, 10 }
								};
								CarSection.Groups[GroupIndex].Elements[j].States[0].Object.Mesh = new Mesh(vertices, faces, Color);
								CarSection.Groups[GroupIndex].Elements[j].LEDClockwiseWinding = InitialAngle <= LastAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDInitialAngle = InitialAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDLastAngle = LastAngle;
								CarSection.Groups[GroupIndex].Elements[j].LEDVectors = new Vector3[]
								{
									new Vector3(x0, y0, z0),
									new Vector3(x1, y1, z1),
									new Vector3(x2, y2, z2),
									new Vector3(x3, y3, z3),
									new Vector3(cx, cy, cz)
								};
								string f = Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
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
								CarSection.Groups[GroupIndex].Elements[j].LEDFunction = new FunctionScript(Program.CurrentHost, f, false);
							}
							else
							{
								Interface.AddMessage(MessageType.Error, false, "Radius is required to be specified in " + Section + " in " + FileName);
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
												Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out LocationY))
											{
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "width":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Width))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Width <= 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "height":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Height))
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Height <= 0.0)
										{
											Interface.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "transparentcolor":
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Interface.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "layer":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Interface.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							// create element
							if (Width <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, "Width is required to be specified in " + Section + " in " + FileName);
							}
							if (Height <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, "Height is required to be specified in " + Section + " in " + FileName);
							}
							if (Width > 0.0 & Height > 0.0)
							{
								int j = Panel2CfgParser.CreateElement(CarSection.Groups[GroupIndex], LocationX, LocationY, Width, Height, new Vector2(0.5, 0.5), (OffsetLayer + Layer) * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelCenter, PanelOrigin, Train.Cars[Car].Driver, null, null, Color32.White, false);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Program.CurrentHost, "timetable", false);
								Timetable.AddObjectForCustomTimetable(CarSection.Groups[GroupIndex].Elements[j]);
							}
						}
						break;
				}

				currentSectionElement++;
			}
		}

		internal static void CreateTouchElement(TrainManager.ElementsGroup Group, Vector2 Location, Vector2 Size, int ScreenIndex, int SoundIndex, Translations.Command Command, int CommandOption, Vector2 RelativeRotationCenter, double PanelResolution, double PanelBottom, Vector2 PanelCenter, Vector3 Driver)
		{
			double WorldWidth, WorldHeight;
			if (Screen.Width >= Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / World.AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
				WorldWidth = WorldHeight * World.AspectRatio;
			}
			double x0 = Location.X / PanelResolution;
			double x1 = (Location.X + Size.X) / PanelResolution;
			double y0 = (PanelBottom - Location.Y) / PanelResolution * World.AspectRatio;
			double y1 = (PanelBottom - (Location.Y + Size.Y)) / PanelResolution * World.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd;
			x1 += xd;
			double yt = PanelBottom - PanelResolution / World.AspectRatio;
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
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new MeshFace[] { new MeshFace(new int[] { 0, 1, 2, 3 }) };
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
			o.Z = EyeDistance + Driver.Z;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TrainManager.TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TrainManager.TouchElement
			{
				Element = new ObjectManager.AnimatedObject(),
				JumpScreenIndex = ScreenIndex,
				SoundIndex = SoundIndex,
				Command = Command,
				CommandOption = CommandOption
			};
			Group.TouchElements[n].Element.States = new ObjectManager.AnimatedObjectState[1];
			Group.TouchElements[n].Element.States[0].Position = o;
			Group.TouchElements[n].Element.States[0].Object = Object;
			Group.TouchElements[n].Element.CurrentState = 0;
			Group.TouchElements[n].Element.ObjectIndex = ObjectManager.CreateDynamicObject();
			ObjectManager.Objects[Group.TouchElements[n].Element.ObjectIndex] = Object.Clone();
			int m = Interface.CurrentControls.Length;
			Array.Resize(ref Interface.CurrentControls, m + 1);
			Interface.CurrentControls[m].Command = Command;
			Interface.CurrentControls[m].Method = Interface.ControlMethod.Touch;
			Interface.CurrentControls[m].Option = CommandOption;
		}
	}
}
