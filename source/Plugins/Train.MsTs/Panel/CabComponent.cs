//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using System;
using System.Globalization;
using System.IO;
using TrainManager.Car;

namespace Train.MsTs
{
	internal class CabComponent
	{
		private CabComponentType Type = CabComponentType.None;
		private string TexturePath;
		private PanelSubject panelSubject;
		private Units Units;
		private Vector2 Position = new Vector2(0, 0);
		private Vector2 Size = new Vector2(0, 0);
		private double PivotPoint;
		private double InitialAngle;
		private double LastAngle;
		private double Maximum;
		private double Minimum;
		private int TotalFrames;
		private int HorizontalFrames;
		private int VerticalFrames;
		private bool MouseControl;
		private bool DirIncrease;
		private int LeadingZeros;
		private FrameMapping[] FrameMappings = Array.Empty<FrameMapping>();
		private readonly Vector3 PanelPosition;
		private CabComponentStyle Style;
		private Tuple<double, Color24>[] PositiveColors;
		private Tuple<double, Color24>[] NegativeColors;
		private Color24 ControlColor;
		private int Accuracy;

		internal void Parse()
		{
			if (!Enum.TryParse(myBlock.Token.ToString(), true, out Type))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS CVF Parser: Unrecognised CabViewComponent type.");
				return;
			}

			while (myBlock.Position() < myBlock.Length() - 2)
			{
				//Components in CVF files are considerably less structured, so read *any* valid block
				try
				{
					Block newBlock = myBlock.ReadSubBlock();
					ReadSubBlock(newBlock);
				}
				catch
				{
					break;
				}

			}
		}

		internal void Create(ref CarBase currentCar, int componentLayer)
		{
			if (!File.Exists(TexturePath) && Type != CabComponentType.Digital && Type != CabComponentType.DigitalClock)
			{
				return;
			}
			if (FrameMappings.Length < 2 && TotalFrames > 1)
			{
				// e.g. Acela power handle has 25 frames for total power value of 100% but no mappings specified
				FrameMappings = new FrameMapping[TotalFrames];
				// frame 0 is always mapping value 0
				for (int i = 1; i < TotalFrames; i++)
				{
					FrameMappings[i].MappingValue = (i + 1.0) / TotalFrames;
					FrameMappings[i].FrameKey = i;
				}

			}

			//Create element
			const double rW = 1024.0 / 640.0;
			const double rH = 768.0 / 480.0;
			int wday, hday;
			int elementIndex;
			string f;
			CultureInfo culture = CultureInfo.InvariantCulture;
			switch (Type)
			{
				case CabComponentType.Dial:
					Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(null, null), out Texture tday, true);
					// correct angle position if appropriate
					if (!DirIncrease && InitialAngle > LastAngle)
					{
						InitialAngle = -(365 - InitialAngle);
					}

					//Get final position from the 640px panel (Yuck...)
					Position.X *= rW;
					Position.Y *= rH;
					Size.X *= rW;
					Size.Y *= rH;
					PivotPoint *= rH;
					elementIndex = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], Position, Size, new Vector2((0.5 * Size.X) / (tday.Width * rW), PivotPoint / (tday.Height * rH)), componentLayer * CabviewFileParser.StackDistance, PanelPosition, tday, null, new Color32(255, 255, 255, 255));
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateXDirection = DirIncrease ? new Vector3(1.0, 0.0, 0.0) : new Vector3(-1.0, 0.0, 0.0);
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateYDirection = Vector3.Cross(currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateZDirection, currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateXDirection);
					f = CabviewFileParser.GetStackLanguageFromSubject(currentCar.baseTrain, panelSubject, Units);
					InitialAngle = InitialAngle.ToRadians();
					LastAngle = LastAngle.ToRadians();
					double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
					double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
					f += " " + a1.ToString(culture) + " * " + a0.ToString(culture) + " +";
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateZFunction = new FunctionScript(Plugin.CurrentHost, f, false);
					// backstop by default e.g. ammeter when using dynamic brakes
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateZFunction.Minimum = InitialAngle;
					currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].RotateZFunction.Maximum = LastAngle;
					break;
				case CabComponentType.Lever:
					/*
					 * TODO:
					 * Need to revisit the actual position versus frame with MSTS content.
					 *
					 * How this works is that MSTS *only* has percentage based handles
					 * However, it's entirely possible to assign a notch set in the ENG file
					 *
					 * Our displayed notch value is >= the appropriate percentage
					 * For a lot of stuff, notched is 'better' but similarly, percentage based is
					 * 'better' for other stuff
					 *
					 * Need to figure out if there's a way to determine which to use.
					 * Check ORTS too...
					 *
					 */
					Position.X *= rW;
					Position.Y *= rH;
					Size.X *= rW;
					Size.Y *= rH;
					Plugin.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
					if (wday > 0 && hday > 0)
					{
						Texture[] textures = new Texture[TotalFrames];
						int row = 0;
						int column = 0;
						int frameWidth = wday / HorizontalFrames;
						int frameHeight = hday / VerticalFrames;
						for (int k = 0; k < TotalFrames; k++)
						{
							Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
							if (column < HorizontalFrames - 1)
							{
								column++;
							}
							else
							{
								column = 0;
								row++;
							}
						}

						elementIndex = -1;
						for (int k = 0; k < textures.Length; k++)
						{
								
							int l = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], Position, Size, new Vector2(0.5, 0.5), componentLayer * CabviewFileParser.StackDistance, PanelPosition, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
							if (k == 0) elementIndex = l;
						}

						f = CabviewFileParser.GetStackLanguageFromSubject(currentCar.baseTrain, panelSubject, Units);
						switch (panelSubject)
						{
							case PanelSubject.Engine_Brake:
							case PanelSubject.Throttle:
							case PanelSubject.Train_Brake:
							case PanelSubject.Gears:
							case PanelSubject.Blower:
								currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject, FrameMappings);
								break;
							default:
								currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new FunctionScript(Plugin.CurrentHost, f, false);
								break;
						}

					}
					break;
				case CabComponentType.TriState:
				case CabComponentType.TwoState:
				case CabComponentType.MultiStateDisplay:
				case CabComponentType.CombinedControl:
					Position.X *= rW;
					Position.Y *= rH;
					Size.X *= rW;
					Size.Y *= rH;
					Plugin.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
					if (wday > 0 && hday > 0)
					{
						Texture[] textures = new Texture[TotalFrames];
						int row = 0;
						int column = 0;
						int frameWidth = wday / HorizontalFrames;
						int frameHeight = hday / VerticalFrames;
						for (int k = 0; k < TotalFrames; k++)
						{
							Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
							if (column < HorizontalFrames - 1)
							{
								column++;
							}
							else
							{
								column = 0;
								row++;
							}
						}

						elementIndex = -1;
						for (int k = 0; k < textures.Length; k++)
						{
							int l = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], Position, Size, new Vector2(0.5, 0.5), componentLayer * CabviewFileParser.StackDistance, PanelPosition, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
							if (k == 0) elementIndex = l;
						}

						f = CabviewFileParser.GetStackLanguageFromSubject(currentCar.baseTrain, panelSubject, Units);
						switch (panelSubject)
						{
							case PanelSubject.Direction:
							case PanelSubject.Direction_Display:
							case PanelSubject.Overspeed:
							case PanelSubject.Sanders:
							case PanelSubject.Sanding:
							case PanelSubject.Wheelslip:
							case PanelSubject.Alerter_Display:
							case PanelSubject.Penalty_App:
							case PanelSubject.Throttle_Display:
							case PanelSubject.CP_Handle:
							case PanelSubject.CPH_Display:
							case PanelSubject.Friction_Braking:
							case PanelSubject.Cyl_Cocks:
							case PanelSubject.Blower:
								currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject, FrameMappings);
								break;
							default:
								currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new FunctionScript(Plugin.CurrentHost, f, false);
								break;
						}


					}
					break;
				case CabComponentType.Digital:
					Position.X *= rW;
					Position.Y *= rH;

					Color24 textColor = PositiveColors[0].Item2;

					Texture[] frameTextures = new Texture[11];
					TexturePath = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility"), "numbers.png"); // arial 9.5pt
					Plugin.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);

					for (int i = 0; i < 10; i++)
					{
						Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(0, i * 24, 16, 24), null), out frameTextures[i], true);
					}

					Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(0, 0, 16, 24), null), out frameTextures[10], true); // repeated zero [check vice MSTS]

					int numMaxDigits = (int)Math.Floor(Math.Log10(Maximum) + 1);
					int numMinDigits = (int)Math.Floor(Math.Log10(Minimum) + 1);

					int totalDigits = Math.Max(numMinDigits, numMaxDigits) + LeadingZeros;
					elementIndex = -1;
					double digitWidth = Size.X / totalDigits;
					for (int currentDigit = 0; currentDigit < totalDigits; currentDigit++)
					{
						for (int k = 0; k < frameTextures.Length; k++)
						{
							int l = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], new Vector2(Position.X + Size.X - (digitWidth * (currentDigit + 1)), Position.Y), new Vector2(digitWidth * rW, Size.Y * rH), new Vector2(0.5, 0.5), componentLayer * CabviewFileParser.StackDistance, PanelPosition, frameTextures[k], null, textColor, k != 0);
							if (k == 0) elementIndex = l;
						}

						// build color arrays and mappings
						currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].Colors = new Color24[NegativeColors.Length + PositiveColors.Length];
						FrameMappings = new FrameMapping[PositiveColors.Length + NegativeColors.Length];
						for (int i = 0; i < NegativeColors.Length; i++)
						{
							FrameMappings[i].MappingValue = NegativeColors[i].Item1;
							FrameMappings[i].FrameKey = i;
							currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].Colors[i] = NegativeColors[i].Item2;
						}

						for (int i = 0; i < PositiveColors.Length; i++)
						{
							FrameMappings[i + NegativeColors.Length].MappingValue = PositiveColors[i].Item1;
							FrameMappings[i + NegativeColors.Length].FrameKey = i + NegativeColors.Length;
							currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].Colors[i + NegativeColors.Length] = PositiveColors[i].Item2;
						}

						// create color and digit functions
						currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject, Units, currentDigit);
						currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].ColorFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject, Units, FrameMappings);
					}
					break;
				case CabComponentType.CabSignalDisplay:
					TotalFrames = 8;
					HorizontalFrames = 4;
					VerticalFrames = 2;
					Position.X *= rW;
					Position.Y *= rH;
					Size.X *= rW;
					Size.Y *= rH;
					Plugin.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
					if (wday > 0 && hday > 0)
					{
						Texture[] textures = new Texture[8];
						// 4 h-frames, 2 v-frames
						int row = 0;
						int column = 0;
						int frameWidth = wday / HorizontalFrames;
						int frameHeight = hday / VerticalFrames;
						for (int k = 0; k < TotalFrames; k++)
						{
							Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
							if (column < HorizontalFrames - 1)
							{
								column++;
							}
							else
							{
								column = 0;
								row++;
							}
						}

						elementIndex = -1;
						for (int k = 0; k < textures.Length; k++)
						{
							int l = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], Position, Size, new Vector2(0.5, 0.5), componentLayer * CabviewFileParser.StackDistance, PanelPosition, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
							if (k == 0) elementIndex = l;
						}

						currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject);
					}
					break;
				case CabComponentType.DigitalClock:
					Position.X *= rW;
					Position.Y *= rH;
					totalDigits = Accuracy == 1 ? 8 : 5; // with or without secs
					elementIndex = -1;
					digitWidth = Size.X / totalDigits;
					textColor = ControlColor;

					frameTextures = new Texture[12];
					TexturePath = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Plugin.FileSystem.DataFolder, "Compatibility"), "numbers.png"); // arial 9.5pt
					Plugin.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);

					for (int i = 0; i < 10; i++)
					{
						Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(0, i * 24, 16, 24), null), out frameTextures[i], true);
					}
					Plugin.CurrentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(0, 240, 16, 16), null), out frameTextures[11], true);

					for (int currentDigit = 0; currentDigit < totalDigits; currentDigit++)
					{
						for (int k = 0; k < frameTextures.Length; k++)
						{
							int l = CabviewFileParser.CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], new Vector2(Position.X + Size.X - (digitWidth * (currentDigit + 1)), Position.Y), new Vector2(digitWidth * rW, Size.Y * rH), new Vector2(0.5, 0.5), componentLayer * CabviewFileParser.StackDistance, PanelPosition, frameTextures[k], null, textColor, k != 0);
							if (k == 0) elementIndex = l;
						}

						// create digit functions
						currentCar.CarSections[CarSectionType.Interior].Groups[0].Elements[elementIndex].StateFunction = new CvfAnimation(Plugin.CurrentHost, panelSubject, Style, Accuracy == 1 ? currentDigit : currentDigit + 3);
					}
					break;
			}
		}

		private void ReadSubBlock(Block block)
		{
			switch (block.Token)
			{
				case KujuTokenID.Pivot:
					PivotPoint = block.ReadSingle();
					break;
				case KujuTokenID.Units:
					Units = block.ReadEnumValue(default(Units));
					break;
				case KujuTokenID.ScalePos:
					InitialAngle = block.ReadSingle();
					LastAngle = block.ReadSingle();
					break;
				case KujuTokenID.ScaleRange:
					Minimum = block.ReadSingle();
					Maximum = block.ReadSingle();
					break;
				case KujuTokenID.States:
					//Contains sub-blocks with Style and SwitchVal types
					TotalFrames = block.ReadInt16();
					HorizontalFrames = block.ReadInt16();
					VerticalFrames = block.ReadInt16();
					FrameMappings = new FrameMapping[TotalFrames];
					for (int i = 0; i < TotalFrames; i++)
					{
						FrameMappings[i].FrameKey = i;
						Block stateBlock = block.ReadSubBlock(KujuTokenID.State);
						while (stateBlock.Length() - stateBlock.Position() > 3)
						{
							Block subBlock = stateBlock.ReadSubBlock(new[] { KujuTokenID.Style, KujuTokenID.SwitchVal });
							if (subBlock.Token == KujuTokenID.SwitchVal)
							{
								FrameMappings[i].MappingValue = subBlock.ReadSingle();
								break;
							}
						}
						

					}
					break;
				case KujuTokenID.DirIncrease:
					// rotates Clockwise (0) or AntiClockwise (1)
					DirIncrease = block.ReadBool();
					break;
				case KujuTokenID.Orientation:
					//Flip?
					block.Skip((int)block.Length());
					break;
				case KujuTokenID.NumValues:
				case KujuTokenID.NumPositions:
					int numValues = block.ReadInt16();
					if (FrameMappings.Length < numValues)
					{
						Array.Resize(ref FrameMappings, numValues);
					}

					for (int i = 0; i < numValues; i++)
					{
						if (block.Token == KujuTokenID.NumValues)
						{
							FrameMappings[i].MappingValue = block.ReadSingle();
						}
						else
						{
							FrameMappings[i].FrameKey = block.ReadInt16();
						}
					}

					break;
				case KujuTokenID.NumFrames:
					TotalFrames = block.ReadInt16();
					HorizontalFrames = block.ReadInt16();
					VerticalFrames = block.ReadInt16();
					break;
				case KujuTokenID.MouseControl:
					MouseControl = block.ReadBool();
					break;
				case KujuTokenID.Style:
					Style = block.ReadEnumValue(default(CabComponentStyle));
					break;
				case KujuTokenID.Graphic:
					string s = block.ReadString();
					if (!string.IsNullOrEmpty(s))
					{
						try
						{
							TexturePath = OpenBveApi.Path.CombineFile(CabviewFileParser.CurrentFolder, s);
						}
						catch
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS CVF Parser: The texture path contains invalid characters in CabComponent " + Type);
						}

						if (!File.Exists(TexturePath))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS CVF Parser: The texture file " + s + " was not found in CabComponent " + Type);
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS CVF Parser: A texture file was not specified in CabComponent " + Type);
					}
					break;
				case KujuTokenID.Position:
					Position.X = block.ReadSingle();
					Position.Y = block.ReadSingle();
					Size.X = block.ReadSingle();
					Size.Y = block.ReadSingle();
					break;
				case KujuTokenID.Type:
					panelSubject = block.ReadEnumValue(default(PanelSubject));
					break;
				case KujuTokenID.LeadingZeros:
					LeadingZeros = block.ReadInt16();
					break;
				case KujuTokenID.PositiveColour:
					int numColors = block.ReadInt16();
					if (numColors == 0)
					{
						PositiveColors = new[]
						{
							new Tuple<double, Color24>(0, Color24.White)

						};
						if (block.Length() - block.Position() > 3)
						{
							Block subBlock = block.ReadSubBlock(KujuTokenID.ControlColour);
							PositiveColors[0] = new Tuple<double, Color24>(0, new Color24((byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16()));
						}
					}
					else
					{
						PositiveColors = new Tuple<double, Color24>[numColors];
						double value = 0;
						for (int i = 0; i < numColors; i++)
						{
							Block subBlock = block.ReadSubBlock(KujuTokenID.ControlColour);
							Color24 color = new Color24((byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16());
							PositiveColors[i] = new Tuple<double, Color24>(value, color);
							if (i < numColors - 1)
							{
								subBlock = block.ReadSubBlock(KujuTokenID.SwitchVal);
								value = subBlock.ReadSingle();
							}
						}
					}

					break;
				case KujuTokenID.NegativeColour:
					numColors = block.ReadInt16();
					if (numColors == 0)
					{
						NegativeColors = new[]
						{
							new Tuple<double, Color24>(double.NegativeInfinity, Color24.White)

						};
						if (block.Length() - block.Position() > 3)
						{
							Block subBlock = block.ReadSubBlock(KujuTokenID.ControlColour);
							NegativeColors[0] = new Tuple<double, Color24>(double.NegativeInfinity, new Color24((byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16()));
						}
					}
					else
					{
						NegativeColors = new Tuple<double, Color24>[numColors];
						double value = double.NegativeInfinity;
						for (int i = 0; i < numColors; i++)
						{
							Block subBlock = block.ReadSubBlock(KujuTokenID.ControlColour);
							Color24 color = new Color24((byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16(), (byte)subBlock.ReadInt16());
							NegativeColors[i] = new Tuple<double, Color24>(value, color);
							if (i < numColors - 1)
							{
								subBlock = block.ReadSubBlock(KujuTokenID.SwitchVal);
								value = subBlock.ReadSingle();
							}
						}
					}
					break;
				case KujuTokenID.ControlColour:
					ControlColor = new Color24((byte)block.ReadInt16(), (byte)block.ReadInt16(), (byte)block.ReadInt16());
					break;
				case KujuTokenID.Accuracy:
					Accuracy = block.ReadInt16();
					break;
			}
		}

		internal CabComponent(Block block, Vector3 panelPosition)
		{
			myBlock = block;
			PanelPosition = panelPosition;
		}

		private readonly Block myBlock;
	}
}
