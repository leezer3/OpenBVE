using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Formats.OpenBve;
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
using Train.OpenBve.Panel2Cfg;

namespace Train.OpenBve
{
	internal class PanelXmlParser
	{
		internal Plugin Plugin;

		internal PanelXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}
		
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

		private void ParsePanelNode(XElement Element, string FileName, TrainBase Train, int Car, ref CarSection CarSection, int GroupIndex, int OffsetLayer)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			XmlBlock panelBlock = new XmlBlock(Plugin.currentHost, FileName, Element, "PanelXml");
			// initialize

			if (GroupIndex == 0)
			{
				Block mainBlock = panelBlock.ReadSubBlock(Section.This);
				if (mainBlock == null)
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "The 'This' section was not found in panel file: " + FileName);
					return;
				}
				double resolution = mainBlock.ReadDouble(Key.Resolution);
				if (resolution > 100)
				{
					Panel.Resolution = resolution;
				}

				Panel.TopLeft.X = mainBlock.ReadDouble(Key.Left);
				Panel.TopLeft.Y = mainBlock.ReadDouble(Key.Top);
				Panel.BottomRight.X = mainBlock.ReadDouble(Key.Right);
				Panel.BottomRight.Y = mainBlock.ReadDouble(Key.Bottom);
				Color24 panelTransparentColor = mainBlock.ReadColor24(Key.TransparentColor, Color24.Blue);
				Texture panelDaytimeImage = mainBlock.LoadTexture(Key.DaytimeImage, Train.TrainFolder, new TextureParameters(null, panelTransparentColor), true);
				Texture panelNighttimeImage = mainBlock.LoadTexture(Key.NighttimeImage, Train.TrainFolder, new TextureParameters(null, panelTransparentColor), true);
				Panel.Center = mainBlock.ReadVector2(Key.Center);
				Panel.Origin = mainBlock.ReadVector2(Key.Origin);

				// camera restriction
				double WorldWidth, WorldHeight;
				if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
				{
					WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * Panel.EyeDistance;
					WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
				}
				else
				{
					WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * Panel.EyeDistance / Plugin.Renderer.Screen.AspectRatio;
					WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
				}
				double x0 = (Panel.TopLeft.X - Panel.Center.X) / Panel.Resolution;
				double x1 = (Panel.BottomRight.X - Panel.Center.X) / Panel.Resolution;
				double y0 = (Panel.Center.Y - Panel.BottomRight.Y) / Panel.Resolution * Plugin.Renderer.Screen.AspectRatio;
				double y1 = (Panel.Center.Y - Panel.TopLeft.Y) / Panel.Resolution * Plugin.Renderer.Screen.AspectRatio;
				Train.Cars[Car].CameraRestriction.BottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, Panel.EyeDistance);
				Train.Cars[Car].CameraRestriction.TopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, Panel.EyeDistance);
				Train.Cars[Car].DriverYaw = Math.Atan((Panel.Center.X - Panel.Origin.X) * WorldWidth / Panel.Resolution);
				Train.Cars[Car].DriverPitch = Math.Atan((Panel.Origin.Y - Panel.Center.Y) * WorldWidth / Panel.Resolution);

				// create panel
				if (panelDaytimeImage != null)
				{
					Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], 0, 0, new Vector2(0.5, 0.5), OffsetLayer, Train.Cars[Car].Driver, panelDaytimeImage, panelNighttimeImage);
				}
				else
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "The main panel image was not found in panel file: " + FileName);
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
				if (!Enum.TryParse(SectionElement.Name.LocalName, true, out Section Section))
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised section " + SectionElement.Name.LocalName + " at line " + ((IXmlLineInfo)SectionElement).LineNumber.ToString(Culture) + " in " + FileName);
				}
				
				switch (Section)
				{
					case Section.Screen:
						if (GroupIndex == 0)
						{
							int n = 0;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Number:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out n))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
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
								CarSection.Groups[n + 1] = new ElementsGroup(ObjectType.Overlay);
							}

							ParsePanelNode(SectionElement, FileName, Train, Car, ref CarSection, n + 1, Layer);
						}
						break;
					case Section.Touch:
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
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Location:
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
									case Key.Size:
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
									case Key.JumpScreen:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out JumpScreen))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.SoundIndex:
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
									case Key.Command:
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
									case Key.CommandOption:
										if (!CommandEntries.Contains(CommandEntry))
										{
											CommandEntries.Add(CommandEntry);
										}

										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out CommandEntry.Option))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.SoundEntries:
										if (!KeyNode.HasElements)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, $"An empty list of touch sound indices was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchSoundEntryNode(FileName, KeyNode, SoundIndices);
										break;
									case Key.CommandEntries:
										if (!KeyNode.HasElements)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, $"An empty list of touch commands was defined at line {((IXmlLineInfo)KeyNode).LineNumber} in XML file {FileName}");
											break;
										}

										ParseTouchCommandEntryNode(FileName, KeyNode, CommandEntries);
										break;
									case Key.Layer:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}
							Plugin.Panel.CreateTouchElement(CarSection.Groups[GroupIndex], Location, Size, JumpScreen, SoundIndices.ToArray(), CommandEntries.ToArray(), new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver);
						}
						break;
					case Section.PilotLamp:
						{
							string Subject = "true";
							Vector2 Location = new Vector2();
							string DaytimeImage = null, NighttimeImage = null;
							Color24 TransparentColor = Color24.Blue;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Subject:
										Subject = Value;
										break;
									case Key.Location:
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
										break;
									case Key.DaytimeImage:
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
									case Key.NighttimeImage:
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
									case Key.TransparentColor:
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
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
								Plugin.currentHost.LoadTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								int w = tday.Width;
								int h = tday.Height;
								int j = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X, Location.Y, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver, tday, tnight, Color32.White);
								string f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f + " 1 == --", false);
							}
						}
						break;
					case Section.Needle:
						{
							string Subject = "true";
							Vector2 Location = new Vector2();
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
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
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
												if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Location.X))
												{
													Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
												}
												if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Location.Y))
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
									case Key.Radius:
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
									case Key.DaytimeImage:
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
									case Key.NighttimeImage:
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
									case Key.Color:
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.TransparentColor:
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Origin:
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
									case Key.InitialAngle:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.LastAngle:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Minimum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Maximum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.NaturalFreq:
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
									case Key.DampingRatio:
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
									case Key.Layer:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
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
							}

							if (DaytimeImage == null)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "DaytimeImage is required to be specified in " + Section + " in " + FileName);
							}
							// create element
							if (DaytimeImage != null)
							{
								Plugin.currentHost.LoadTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday);
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
								int j = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X - ox * nx, Location.Y - oy * ny, nx, ny, new Vector2(ox, oy), (OffsetLayer + Layer), Train.Cars[Car].Driver, tday, tnight, Color);
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
					case Section.LinearGauge:
						{
							string Subject = "true";
							int Width = 0;
							Vector2 Direction = new Vector2(1, 0);
							Vector2 Location = new Vector2();
							string DaytimeImage = null, NighttimeImage = null;
							double Minimum = 0.0, Maximum = 0.0;
							Color24 TransparentColor = Color24.Blue;
							int Layer = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Subject:
										Subject = Value;
										break;
									case Key.Location:
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
										break;
									case Key.Minimum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Maximum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Width:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Width))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Direction:
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
									case Key.DaytimeImage:
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
									case Key.NighttimeImage:
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
									case Key.TransparentColor:
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
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
								Plugin.currentHost.LoadTexture(DaytimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out var tday);
								Texture tnight = null;
								if (NighttimeImage != null)
								{
									Plugin.currentHost.RegisterTexture(NighttimeImage, new TextureParameters(null, new Color24(TransparentColor.R, TransparentColor.G, TransparentColor.B)), out tnight);
								}
								int w = tday.Width;
								int h = tday.Height;
								int j = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X, Location.Y, w, h, new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver, tday, tnight, Color32.White);
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
					case Section.DigitalNumber:
						{
							string Subject = "true";
							Vector2 Location = new Vector2();
							string DaytimeImage = null, NighttimeImage = null;
							Color24 TransparentColor = Color24.Blue;
							double Layer = 0.0;
							int Interval = 0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Subject:
										Subject = Value;
										break;
									case Key.Location:
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
										break;
									case Key.DaytimeImage:
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
									case Key.NighttimeImage:
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
									case Key.TransparentColor:
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Interval:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Interval))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Interval <= 0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Height is expected to be non-negative in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
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
										int l = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X, Location.Y, wday, Interval, new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver, tday[k], tnight[k], Color32.White, k != 0);
										if (k == 0) j = l;
									}
									string f = Plugin.Panel2CfgParser.GetStackLanguageFromSubject(Train, Subject, Section + " in " + FileName);
									CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f, false);
								}
							}
						}
						break;
					case Section.DigitalGauge:
						{
							string Subject = "true";
							Vector2 Location = new Vector2();
							Color32 Color = Color32.Black;
							double Radius = 0.0;
							int Layer = 0;
							double InitialAngle = -2.0943951023932, LastAngle = 2.0943951023932;
							double Minimum = 0.0, Maximum = 1000.0;
							double Step = 0.0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Subject:
										Subject = Value;
										break;
									case Key.Location:
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Location.X))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterX is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Location.Y))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "CenterY is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Radius:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Radius))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										if (Radius == 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is expected to be non-zero in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											Radius = 16.0;
										}
										break;
									case Key.Color:
										if (Value.Length != 0 && !Color32.TryParseHexColor(Value, out Color))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.InitialAngle:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out InitialAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											InitialAngle = InitialAngle.ToRadians();
										}
										break;
									case Key.LastAngle:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out LastAngle))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInDegrees is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else
										{
											LastAngle = LastAngle.ToRadians();
										}
										break;
									case Key.Minimum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Minimum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Maximum:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Maximum))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Step:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Step))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							if (Minimum == Maximum)
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Minimum and Maximum must not be equal in " + Section + " in " + FileName);
								Radius = 0.0;
							}
							if (Math.Abs(InitialAngle - LastAngle) > 6.28318531)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "The absolute difference between InitialAngle and LastAngle exceeds 360 degrees in " + Section + " in " + FileName);
							}
							if (Radius != 0.0)
							{
								// create element
								int j = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X - Radius, Location.Y - Radius, 2.0 * Radius, 2.0 * Radius, new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver, null, null, Color);
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
					case Section.Timetable:
					{
							Vector2 Location = new Vector2();
							double Width = 0.0, Height = 0.0;
							//We read the transparent color for the timetable from the config file, but it is never used
							//TODO: Fix or depreciate??
							// ReSharper disable once NotAccessedVariable
							Color24 TransparentColor = Color24.Blue;
							double Layer = 0.0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
								if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
								}
								string Value = KeyNode.Value;

								switch (Key)
								{
									case Key.Location:
										int k = Value.IndexOf(',');
										if (k >= 0)
										{
											string a = Value.Substring(0, k).TrimEnd();
											string b = Value.Substring(k + 1).TrimStart();
											if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out Location.X))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out Location.Y))
											{
												Plugin.currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
										}
										else
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Width:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Width))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Width <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Height:
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Height))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										else if (Height <= 0.0)
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "ValueInPixels is required to be positive in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.TransparentColor:
										if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case Key.Layer:
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
								int j = Plugin.Panel.CreateElement(ref CarSection.Groups[GroupIndex], Location.X, Location.Y, Width, Height, new Vector2(0.5, 0.5), (OffsetLayer + Layer), Train.Cars[Car].Driver, null, null, Color32.White);
								CarSection.Groups[GroupIndex].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, "timetable", false);
								Plugin.currentHost.AddObjectForCustomTimeTable(CarSection.Groups[GroupIndex].Elements[j]);
							}
						}
						break;
					case Section.Windscreen:
					{
						Vector2 topLeft = new Vector2(Panel.TopLeft);
						Vector2 bottomRight = new Vector2(Panel.BottomRight);
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
							int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;
							if (!Enum.TryParse(KeyNode.Name.LocalName, true, out Key Key))
							{
								Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised key " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
							}
							string Value = KeyNode.Value;
							int k;
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
								case Key.BottomRight:
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
								case Key.NumberOfDrops:
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out numberOfDrops))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "NumberOfDrops is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case Key.DropSize:
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out numberOfDrops))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "DropSize is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case Key.DaytimeDrops:
									daytimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
									break;
								case Key.NighttimeDrops:
									nighttimeDropFiles = Value.IndexOf(',') != -1 ? Value.Trim().Split(',').ToList() : new List<string> {Value};
									break;
								case Key.TransparentColor:
									if (Value.Length != 0 && !Color24.TryParseHexColor(Value, out TransparentColor))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "HexColor is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case Key.Layer:
									if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out Layer))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "LayerIndex is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case Key.WipeSpeed:
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out wipeSpeed))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case Key.WiperHoldTime:
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out holdTime))
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "WipeSpeed is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperRestPosition is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
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
											Plugin.currentHost.AddMessage(MessageType.Error, false, "WiperHoldPosition is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											break;
									}
									break;
								case Key.DropLife:
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

							Plugin.Renderer.TextureManager.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out var drop);
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

							Plugin.Renderer.TextureManager.RegisterTexture(currentDropFile, new TextureParameters(null, TransparentColor), out var drop);
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
							int panelDropIndex = Plugin.Panel.CreateElement(ref Train.Cars[Car].CarSections[0].Groups[0], currentDropX, currentDropY, dropSize, dropSize, new Vector2(0.5, 0.5), Layer, Train.Cars[Car].Driver, daytimeDrops[DropTexture], nighttimeDrops[DropTexture], Color32.White);
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

		
	}
}
