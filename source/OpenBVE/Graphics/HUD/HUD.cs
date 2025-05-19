using System;
using System.Globalization;
using Formats.OpenBve;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using SoundManager;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve
{
	internal static partial class HUD
	{
		/// <summary>The current HUD elements to render</summary>
		internal static Element[] CurrentHudElements = { };

		/// <summary>Sound source used by the accessibility station adjust tone</summary>
		internal static SoundSource stationAdjustBeepSource;
		/// <summary>Station adjust beep sound used by accessibility</summary>
		internal static SoundBuffer stationAdjustBeep;

		/// <summary>Loads the current HUD</summary>
		internal static void LoadHUD()
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string Folder = Program.FileSystem.GetDataFolder("In-game", Interface.CurrentOptions.UserInterfaceFolder);
			string File = OpenBveApi.Path.CombineFile(Folder, "interface.cfg");
			Program.CurrentHost.RegisterSound(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("In-game"), "beep.wav"), 50, out var beep);
			stationAdjustBeep = beep as SoundBuffer;
			CurrentHudElements = new Element[16];
			int Length = 0;
			if (System.IO.File.Exists(File))
			{
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				for (int i = 0; i < Lines.Length; i++)
				{
					int j = Lines[i].IndexOf(';');
					if (j >= 0)
					{
						Lines[i] = Lines[i].Substring(0, j).Trim(new char[] { });
					}
					else
					{
						Lines[i] = Lines[i].Trim(new char[] { });
					}
					if (Lines[i].Length != 0)
					{
						if (!Lines[i].StartsWith(";", StringComparison.Ordinal))
						{
							if (Lines[i].Equals("[element]", StringComparison.OrdinalIgnoreCase))
							{
								Length++;
								if (Length > CurrentHudElements.Length)
								{
									Array.Resize(ref CurrentHudElements, CurrentHudElements.Length << 1);
								}

								CurrentHudElements[Length - 1] = new Element();
							}
							else if (Length == 0)
							{
								Interface.AddMessage(MessageType.Error, false, "Line outside of [element] structure encountered at line " + (i + 1).ToString(Culture) + " in " + File);
							}
							else
							{
								j = Lines[i].IndexOf("=", StringComparison.Ordinal);
								if (j >= 0)
								{
									string Command = Lines[i].Substring(0, j).TrimEnd();
									string[] Arguments = Lines[i].Substring(j + 1).TrimStart().Split(new[] { ','}, StringSplitOptions.None);
									for (j = 0; j < Arguments.Length; j++)
									{
										Arguments[j] = Arguments[j].Trim(new char[] { });
									}
									Enum.TryParse(Command, true, out HUDKey key);
									switch (key)
									{
										case HUDKey.Subject:
											if (Arguments.Length == 1)
											{
												if (!Enum.TryParse(Arguments[0], true, out CurrentHudElements[Length - 1].Subject))
												{
													Interface.AddMessage(MessageType.Error, false, "Unknown Subject for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
											}
											break;
										case HUDKey.Position:
											if (!Vector2.TryParse(Arguments, out CurrentHudElements[Length - 1].Position))
											{
												Interface.AddMessage(MessageType.Error, false, "Invalid Position for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case HUDKey.Alignment:
											if (Arguments.Length == 2)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out int y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Alignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].Alignment.Y = Math.Sign(y);
												}
											}
											break;
										case HUDKey.TopLeft:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopLeft.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopLeft.OverlayTexture);
												}
											}
											break;
										case HUDKey.TopMiddle:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopMiddle.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopMiddle.OverlayTexture);
												}
											}
											break;
										case HUDKey.TopRight:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopRight.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopRight.OverlayTexture);
												}
											}
											break;
										case HUDKey.CenterLeft:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterLeft.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterLeft.OverlayTexture);
												}
											}
											break;
										case HUDKey.CenterMiddle:
											if (Arguments.Length >= 1 && Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
											{
												Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterMiddle.BackgroundTexture);
											}

											if (Arguments.Length >= 2 && Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
											{
												Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterMiddle.OverlayTexture);
											}

											if (Arguments.Length >= 3 && Arguments[2].Length != 0 & !Arguments[2].Equals("null", StringComparison.OrdinalIgnoreCase))
											{
												Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[2]), out CurrentHudElements[Length - 1].CenterMiddle.WideBackgroundTexture);
											}

											if (Arguments.Length >= 4 && Arguments[3].Length != 0 & !Arguments[3].Equals("null", StringComparison.OrdinalIgnoreCase))
											{
												Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[3]), out CurrentHudElements[Length - 1].CenterMiddle.WideOverlayTexture);
											}
											break;
										case HUDKey.CenterRight:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterRight.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterRight.OverlayTexture);
												}
											}
											break;
										case HUDKey.BottomLeft:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomLeft.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomLeft.OverlayTexture);
												}
											}
											break;
										case HUDKey.BottomMiddle:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomMiddle.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomMiddle.OverlayTexture);
												}
											}
											break;
										case HUDKey.BottomRight:
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomRight.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomRight.OverlayTexture);
												}
											}
											break;
										case HUDKey.BackColor:
											if (!Color32.TryParseColor(Arguments, out CurrentHudElements[Length - 1].BackgroundColor))
											{
												Interface.AddMessage(MessageType.Error, false, "Invalid BackColor for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case HUDKey.OverlayColor:
											if (!Color32.TryParseColor(Arguments, out CurrentHudElements[Length - 1].OverlayColor))
											{
												Interface.AddMessage(MessageType.Error, false, "Invalid OverlayColor for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case HUDKey.TextColor:
											if (!Color32.TryParseColor(Arguments, out CurrentHudElements[Length - 1].TextColor))
											{
												Interface.AddMessage(MessageType.Error, false, "Invalid TextColor for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case HUDKey.TextPosition:
											if (!Vector2.TryParse(Arguments, out CurrentHudElements[Length - 1].TextPosition))
											{
												Interface.AddMessage(MessageType.Error, false, "Invalid TextPosition for HUD element " + (Length - 1) + " at Line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case HUDKey.TextAlignment:
											if (Arguments.Length == 2)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out int y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TextAlignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].TextAlignment.Y = Math.Sign(y);
												}
											}
											break;
										case HUDKey.TextSize:
											if (Arguments.Length == 1)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int s))
												{
													Interface.AddMessage(MessageType.Error, false, "SIZE is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													s += Interface.CurrentOptions.UserInterfaceScaleFactor - 1;
													switch (s)
													{
														case 0:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.VerySmallFont;
															break;
														case 1:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.SmallFont;
															break;
														case 2:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.NormalFont;
															break;
														case 3:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.LargeFont;
															break;
														case 4:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.VeryLargeFont;
															break;
														case 5:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.EvenLargerFont;
															break;
														default:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.NormalFont;
															break;
													}
												}
											}
											break;
										case HUDKey.TextShadow:
											if (Arguments.Length == 1)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int s))
												{
													Interface.AddMessage(MessageType.Error, false, "SHADOW is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TextShadow = s != 0;
												}
											}
											break;
										case HUDKey.Text:
											if (Arguments.Length == 1)
											{
												CurrentHudElements[Length - 1].Text = Arguments[0];
											}
											break;
										case HUDKey.Value:
											if (Arguments.Length == 1)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int n))
												{
													Interface.AddMessage(MessageType.Error, false, "VALUE1 is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Value1 = n;
												}
											}
											else if (Arguments.Length == 2)
											{
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out float a))
												{
													Interface.AddMessage(MessageType.Error, false, "VALUE1 is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out float b))
												{
													Interface.AddMessage(MessageType.Error, false, "VALUE2 is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Value1 = a;
													CurrentHudElements[Length - 1].Value2 = b;
												}
											}
											break;
										case HUDKey.Transition:
											if (Arguments.Length == 1)
											{
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out int n))
												{
													Interface.AddMessage(MessageType.Error, false, "TRANSITION is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Transition = (Transition) n;
												}
											}
											break;
										case HUDKey.TransitionVector:
											if (Arguments.Length == 2)
											{
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out float x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out float y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at Line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TransitionVector.X = x;
													CurrentHudElements[Length - 1].TransitionVector.Y = y;
												}
											}

											break;
										default:
											Interface.AddMessage(MessageType.Error, false, "Invalid command encountered at Line " + (i + 1).ToString(Culture) + " in " + File);
											break;
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid statement encountered at Line " + (i + 1).ToString(Culture) + " in " + File);
								}
							}
						}
					}
				}
			}

			Array.Resize(ref CurrentHudElements, Length);
		}
	}
}
