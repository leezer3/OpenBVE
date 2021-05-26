using System;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Sounds;
using SoundManager;

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
								MessageBox.Show("Line outside of [element] structure encountered at line " + (i + 1).ToString(Culture) + " in " + File);
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
									switch (Command.ToLowerInvariant())
									{
										case "subject":
											if (Arguments.Length == 1)
											{
												CurrentHudElements[Length - 1].Subject = Arguments[0];
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "position":
											if (Arguments.Length == 2)
											{
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x))
												{
													MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y))
												{
													MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Position.X = x;
													CurrentHudElements[Length - 1].Position.Y = y;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "alignment":
											if (Arguments.Length == 2)
											{
												int x, y;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out x))
												{
													MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out y))
												{
													MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Alignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].Alignment.Y = Math.Sign(y);
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "topleft":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "topmiddle":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "topright":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "centerleft":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "centermiddle":
											if (Arguments.Length == 2)
											{
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterMiddle.BackgroundTexture);
												}

												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase))
												{
													Program.Renderer.TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterMiddle.OverlayTexture);
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "centerright":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "bottomleft":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "bottommiddle":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "bottomright":
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
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "backcolor":
											if (Arguments.Length == 4)
											{
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r))
												{
													MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g))
												{
													MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b))
												{
													MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a))
												{
													MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].BackgroundColor = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
												}

												break;
											}
											MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											break;
										case "overlaycolor":
											if (Arguments.Length == 4)
											{
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r))
												{
													MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g))
												{
													MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b))
												{
													MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a))
												{
													MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].OverlayColor = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
												}

												break;
											}
											MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											break;
										case "textcolor":
											if (Arguments.Length == 4)
											{
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r))
												{
													MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g))
												{
													MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b))
												{
													MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a))
												{
													MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].TextColor = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
												}

												break;
											}
											MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											break;
										case "textposition":
											if (Arguments.Length == 2)
											{
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x))
												{
													MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y))
												{
													MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TextPosition.X = x;
													CurrentHudElements[Length - 1].TextPosition.Y = y;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "textalignment":
											if (Arguments.Length == 2)
											{
												int x, y;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out x))
												{
													MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out y))
												{
													MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TextAlignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].TextAlignment.Y = Math.Sign(y);
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "textsize":
											if (Arguments.Length == 1)
											{
												int s;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out s))
												{
													MessageBox.Show("SIZE is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
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
														default:
															CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.NormalFont;
															break;
													}
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "textshadow":
											if (Arguments.Length == 1)
											{
												int s;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out s))
												{
													MessageBox.Show("SHADOW is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TextShadow = s != 0;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "text":
											if (Arguments.Length == 1)
											{
												CurrentHudElements[Length - 1].Text = Arguments[0];
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "value":
											if (Arguments.Length == 1)
											{
												int n;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out n))
												{
													MessageBox.Show("VALUE1 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Value1 = n;
												}
											}
											else if (Arguments.Length == 2)
											{
												float a, b;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out a))
												{
													MessageBox.Show("VALUE1 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out b))
												{
													MessageBox.Show("VALUE2 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Value1 = a;
													CurrentHudElements[Length - 1].Value2 = b;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "transition":
											if (Arguments.Length == 1)
											{
												int n;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out n))
												{
													MessageBox.Show("TRANSITION is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].Transition = (HUD.Transition) n;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}
											break;
										case "transitionvector":
											if (Arguments.Length == 2)
											{
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x))
												{
													MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y))
												{
													MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												}
												else
												{
													CurrentHudElements[Length - 1].TransitionVector.X = x;
													CurrentHudElements[Length - 1].TransitionVector.Y = y;
												}
											}
											else
											{
												MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											}

											break;
										default:
											MessageBox.Show("Invalid command encountered at line " + (i + 1).ToString(Culture) + " in " + File);
											break;
									}
								}
								else
								{
									MessageBox.Show("Invalid statement encountered at line " + (i + 1).ToString(Culture) + " in " + File);
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
