using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi.Interface;
using SoundEditor.Systems;
using Path = OpenBveApi.Path;

namespace SoundEditor.Parsers.Sound
{
	internal static partial class SoundCfg
	{
		private static void ParseBve4(string fileName, Encoding encoding, Sounds result)
		{
			// parse configuration file
			CultureInfo Culture = CultureInfo.InvariantCulture;
			List<string> Lines = File.ReadAllLines(fileName, encoding).ToList();

			for (int i = Lines.Count - 1; i >= 0; i--)
			{
				/*
				 * Strip comments and remove empty resulting lines etc.
				 *
				 * This fixes an error with some NYCTA content, which has
				 * a copyright notice instead of the file header specified....
				 */
				int j = Lines[i].IndexOf(';');

				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}

				if (string.IsNullOrEmpty(Lines[i]))
				{
					Lines.RemoveAt(i);
				}
			}

			if (!Lines.Any())
			{
				Interface.AddMessage(MessageType.Error, false, string.Format("Empty sound.cfg encountered in {0}.", fileName));
			}

			if (string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
			{
				Interface.AddMessage(MessageType.Error, false, string.Format("Invalid file format encountered in {0}. The first line is expected to be \"Version 1.0\".", fileName));
			}

			for (int i = 0; i < Lines.Count; i++)
			{
				switch (Lines[i].ToLowerInvariant())
				{
					case "[run]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									if (k >= 0)
									{
										result.Run.Add(new IndexedSound
										{
											Index = k,
											FileName = b
										});
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Index must be greater or equal to zero at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[flange]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									if (k >= 0)
									{
										result.Flange.Add(new IndexedSound
										{
											Index = k,
											FileName = b
										});
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Index must be greater or equal to zero at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[motor]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									if (k >= 0)
									{
										result.Motor.Add(new IndexedSound
										{
											Index = k,
											FileName = b
										});
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Index is invalid at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[switch]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									if (k >= 0)
									{
										result.FrontSwitch.Add(new IndexedSound
										{
											Index = k,
											FileName = b
										});
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Index is invalid at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[brake]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "bc release high":
											result.Brake.BcReleaseHigh.FileName = b;
											break;
										case "bc release":
											result.Brake.BcRelease.FileName = b;
											break;
										case "bc release full":
											result.Brake.BcReleaseFull.FileName = b;
											break;
										case "emergency":
											result.Brake.Emergency.FileName = b;
											break;
										case "bp decomp":
											result.Brake.BpDecomp.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[compressor]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "attack":
											result.Compressor.Attack.FileName = b;
											break;
										case "loop":
											result.Compressor.Loop.FileName = b;
											break;
										case "release":
											result.Compressor.Release.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[suspension]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "left":
											result.Suspension.Left.FileName = b;
											break;
										case "right":
											result.Suspension.Right.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[horn]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										//PRIMARY HORN (Enter)
										case "primarystart":
											result.PrimaryHorn.Start.FileName = b;
											break;
										case "primaryend":
										case "primaryrelease":
											result.PrimaryHorn.End.FileName = b;
											break;
										case "primaryloop":
										case "primary":
											result.PrimaryHorn.Loop.FileName = b;
											break;
										//SECONDARY HORN (Numpad Enter)
										case "secondarystart":
											result.SecondaryHorn.Start.FileName = b;
											break;
										case "secondaryend":
										case "secondaryrelease":
											result.SecondaryHorn.End.FileName = b;
											break;
										case "secondaryloop":
										case "secondary":
											result.SecondaryHorn.Loop.FileName = b;
											break;
										//MUSIC HORN
										case "musicstart":
											result.MusicHorn.Start.FileName = b;
											break;
										case "musicend":
										case "musicrelease":
											result.MusicHorn.End.FileName = b;
											break;
										case "musicloop":
										case "music":
											result.MusicHorn.Loop.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[door]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "open left":
											result.Door.OpenLeft.FileName = b;
											break;
										case "open right":
											result.Door.OpenRight.FileName = b;
											break;
										case "close left":
											result.Door.CloseLeft.FileName = b;
											break;
										case "close right":
											result.Door.CloseRight.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[ats]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									int k;

									if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
									else
									{
										if (k >= 0)
										{
											result.Ats.Add(new IndexedSound
											{
												Index = k,
												FileName = b
											});
										}
										else
										{
											Interface.AddMessage(MessageType.Warning, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + fileName);
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[buzzer]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "correct":
											result.Buzzer.Correct.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[pilot lamp]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											result.PilotLamp.On.FileName = b;
											break;
										case "off":
											result.PilotLamp.Off.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[brake handle]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "apply":
											result.BrakeHandle.Apply.FileName = b;
											break;
										case "applyfast":
											result.BrakeHandle.ApplyFast.FileName = b;
											break;
										case "release":
											result.BrakeHandle.Release.FileName = b;
											break;
										case "releasefast":
											result.BrakeHandle.ReleaseFast.FileName = b;
											break;
										case "min":
											result.BrakeHandle.Min.FileName = b;
											break;
										case "max":
											result.BrakeHandle.Max.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[master controller]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "up":
											result.MasterController.Up.FileName = b;
											break;
										case "upfast":
											result.MasterController.UpFast.FileName = b;
											break;
										case "down":
											result.MasterController.Down.FileName = b;
											break;
										case "downfast":
											result.MasterController.DownFast.FileName = b;
											break;
										case "min":
											result.MasterController.Min.FileName = b;
											break;
										case "max":
											result.MasterController.Max.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[reverser]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											result.Reverser.On.FileName = b;
											break;
										case "off":
											result.Reverser.Off.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[breaker]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											result.Breaker.On.FileName = b;
											break;
										case "off":
											result.Breaker.Off.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[others]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "noise":
											result.Others.Noise.FileName = b;
											break;
										case "shoe":
											result.Others.Shoe.FileName = b;
											break;
										case "halt":
											result.Others.Halt.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[requeststop]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "stop":
											result.RequestStop.Stop.FileName = b;
											break;
										case "pass":
											result.RequestStop.Pass.FileName = b;
											break;
										case "ignored":
											result.RequestStop.Ignored.FileName = b;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, string.Format("Unsupported key {0} encountered at line {1} in file {2}", a, (i + 1).ToString(Culture), fileName));
											break;
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[touch]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, string.Format("FileName contains illegal characters or is empty at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
								}
								else
								{
									int k;

									if (!int.TryParse(a, NumberStyles.Integer, Culture, out k))
									{
										Interface.AddMessage(MessageType.Error, false, string.Format("Invalid index appeared at line {0} in file {1}", (i + 1).ToString(Culture), fileName));
									}
									else
									{
										if (k >= 0)
										{
											result.Touch.Add(new IndexedSound
											{
												Index = k,
												FileName = b
											});
										}
										else
										{
											Interface.AddMessage(MessageType.Warning, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + fileName);
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
				}
			}

		}

		private static void WriteCfg(Sounds data, string fileName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Version 1.0");

			builder.AppendLine("[Run]");

			foreach (IndexedSound sound in data.Run)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Flange]");

			foreach (IndexedSound sound in data.Flange)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Motor]");

			foreach (IndexedSound sound in data.Motor)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Switch]");

			foreach (IndexedSound sound in data.FrontSwitch)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Brake]");
			WriteKey(builder, "BC Release High", data.Brake.BcReleaseHigh);
			WriteKey(builder, "BC Release", data.Brake.BcRelease);
			WriteKey(builder, "BC Release Full", data.Brake.BcReleaseFull);
			WriteKey(builder, "Emergency", data.Brake.Emergency);
			WriteKey(builder, "BP Decomp", data.Brake.BpDecomp);

			builder.AppendLine("[Compressor]");
			WriteKey(builder, "Attack", data.Compressor.Attack);
			WriteKey(builder, "Loop", data.Compressor.Loop);
			WriteKey(builder, "Release", data.Compressor.Release);

			builder.AppendLine("[Suspension]");
			WriteKey(builder, "Left", data.Suspension.Left);
			WriteKey(builder, "Right", data.Suspension.Right);

			builder.AppendLine("[Horn]");
			WriteKey(builder, "PrimaryStart", data.PrimaryHorn.Start);
			WriteKey(builder, "PrimaryLoop", data.PrimaryHorn.Loop);
			WriteKey(builder, "PrimaryEnd", data.PrimaryHorn.End);
			WriteKey(builder, "SecondaryStart", data.SecondaryHorn.Start);
			WriteKey(builder, "SecondaryLoop", data.SecondaryHorn.Loop);
			WriteKey(builder, "SecondaryEnd", data.SecondaryHorn.End);
			WriteKey(builder, "MusicStart", data.MusicHorn.Start);
			WriteKey(builder, "MusicLoop", data.MusicHorn.Loop);
			WriteKey(builder, "MusicEnd", data.MusicHorn.End);

			builder.AppendLine("[Door]");
			WriteKey(builder, "Open Left", data.Door.OpenLeft);
			WriteKey(builder, "Close Left", data.Door.CloseLeft);
			WriteKey(builder, "Open Right", data.Door.OpenRight);
			WriteKey(builder, "Close Right", data.Door.CloseRight);

			builder.AppendLine("[Ats]");

			foreach (IndexedSound sound in data.Ats)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Buzzer]");
			WriteKey(builder, "Correct", data.Buzzer.Correct);

			builder.AppendLine("[Pilot Lamp]");
			WriteKey(builder, "On", data.PilotLamp.On);
			WriteKey(builder, "Off", data.PilotLamp.Off);

			builder.AppendLine("[Brake Handle]");
			WriteKey(builder, "Apply", data.BrakeHandle.Apply);
			WriteKey(builder, "ApplyFast", data.BrakeHandle.ApplyFast);
			WriteKey(builder, "Release", data.BrakeHandle.Release);
			WriteKey(builder, "ReleaseFast", data.BrakeHandle.ReleaseFast);
			WriteKey(builder, "Min", data.BrakeHandle.Min);
			WriteKey(builder, "Max", data.BrakeHandle.Max);

			builder.AppendLine("[Master Controller]");
			WriteKey(builder, "Up", data.MasterController.Up);
			WriteKey(builder, "UpFast", data.MasterController.UpFast);
			WriteKey(builder, "Down", data.MasterController.Down);
			WriteKey(builder, "DownFast", data.MasterController.DownFast);
			WriteKey(builder, "Min", data.MasterController.Min);
			WriteKey(builder, "Max", data.MasterController.Max);

			builder.AppendLine("[Reverser]");
			WriteKey(builder, "On", data.Reverser.On);
			WriteKey(builder, "Off", data.Reverser.Off);

			builder.AppendLine("[Breaker]");
			WriteKey(builder, "On", data.Breaker.On);
			WriteKey(builder, "Off", data.Breaker.Off);

			builder.AppendLine("[Request Stop]");
			WriteKey(builder, "Stop", data.RequestStop.Stop);
			WriteKey(builder, "Pass", data.RequestStop.Pass);
			WriteKey(builder, "Ignored", data.RequestStop.Ignored);

			builder.AppendLine("[Touch]");

			foreach (IndexedSound sound in data.Touch)
			{
				WriteKey(builder, sound.Index.ToString(), sound);
			}

			builder.AppendLine("[Others]");
			WriteKey(builder, "Noise", data.Others.Noise);
			WriteKey(builder, "Shoe", data.Others.Shoe);
			WriteKey(builder, "Halt", data.Others.Halt);

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}

		private static void WriteKey(StringBuilder builder, string key, Sound sound)
		{
			if (!string.IsNullOrEmpty(sound.FileName))
			{
				builder.AppendLine(string.Format("{0} = {1}", key, sound.FileName));
			}
		}
	}
}
