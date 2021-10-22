using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Motor;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class BVE4SoundParser
	{
		internal readonly Plugin Plugin;

		internal BVE4SoundParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Loads the sound set for a BVE4 or openBVE sound.cfg based train</summary>
		/// <param name="train">The train</param>
		/// <param name="FileName">The absolute on-disk path to the sound.cfg file</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		internal void Parse(string FileName, string trainFolder, TrainBase train)
		{
			//Default sound positions and radii

			//3D center of the car
			Vector3 center = Vector3.Zero;
			//Positioned to the left of the car, but centered Y & Z
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			//Positioned to the right of the car, but centered Y & Z
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			//Positioned at the front of the car, centered X and Y
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			//Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);

			//Radius at which the sound is audible at full volume, presumably in m
			//TODO: All radii are much too small in external mode, but we can't change them by default.....

			Encoding Encoding = TextEncoding.GetSystemEncodingFromFile(FileName);

			// parse configuration file
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			List<string> Lines = System.IO.File.ReadAllLines(FileName, Encoding).ToList();
			int emptyLines = 0;
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
					emptyLines++;
				}
			}

			if (Lines.Count == 0 || emptyLines == Lines.Count)
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Empty sound.cfg encountered in " + FileName + ".");
			}
			else if (string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid file format encountered in " + FileName + ". The first line is expected to be \"Version 1.0\".");
			}
			string[] MotorFiles = new string[] { };
			double invfac = Lines.Count == 0 ? 0.1 : 0.1 / Lines.Count;
			for (int i = 0; i < Lines.Count; i++)
			{
				if (string.IsNullOrEmpty(Lines[i]))
				{
					continue;
				}
				Plugin.CurrentProgress = Plugin.LastProgress + invfac * i;
				if ((i & 7) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}
				switch (Lines[i].ToLowerInvariant())
				{
					case "[run]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out var k))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										for (int c = 0; c < train.Cars.Length; c++)
										{
											if(train.Cars[c].Sounds.Run == null)
											{
												train.Cars[c].Sounds.Run = new Dictionary<int, CarSound>();
											}

											if (train.Cars[c].Sounds.Run.ContainsKey(k))
											{
												train.Cars[c].Sounds.Run[k] = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
											}
											else
											{
												train.Cars[c].Sounds.Run.Add(k, new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center));
											}
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "RunIndex must be greater than or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							}
							i++;
						}
						i--; break;
					case "[flange]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out var k))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										for (int c = 0; c < train.Cars.Length; c++)
										{
											if (train.Cars[c].Sounds.Flange.ContainsKey(k))
											{
												train.Cars[c].Sounds.Flange[k] = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
											}
											else
											{
												train.Cars[c].Sounds.Flange.Add(k, new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center));
											}
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "FlangeIndex must be greater than or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							}
							i++;
						}
						i--; break;
					case "[motor]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out var k))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										if (k >= MotorFiles.Length)
										{
											Array.Resize(ref MotorFiles, k + 1);
										}
										MotorFiles[k] = Path.CombineFile(trainFolder, b);
										if (!System.IO.File.Exists(MotorFiles[k]))
										{
											Plugin.currentHost.AddMessage(MessageType.Error, true, "File " + MotorFiles[k] + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											MotorFiles[k] = null;
										}
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "MotorIndex must be greater than or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							}
							i++;
						}
						i--; break;
					case "[switch]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (NumberFormats.TryParseIntVb6(a, out var switchIndex))
								{
									if (switchIndex < 0)
									{
										Plugin.currentHost.AddMessage(MessageType.Error, false, "SwitchIndex must be greater than or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										continue;
									}
									for (int c = 0; c < train.Cars.Length; c++)
									{
										int n = train.Cars[c].FrontAxle.PointSounds.Length;
										if (switchIndex >= n)
										{
											Array.Resize(ref train.Cars[c].FrontAxle.PointSounds, switchIndex + 1);
											Array.Resize(ref train.Cars[c].RearAxle.PointSounds, switchIndex + 1);
											for (int h = n; h < switchIndex; h++)
											{
												train.Cars[c].FrontAxle.PointSounds[h] = new CarSound();
												train.Cars[c].RearAxle.PointSounds[h] = new CarSound();
											}
										}
										Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[c].FrontAxle.Position);
										Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[c].RearAxle.Position);
										train.Cars[c].FrontAxle.PointSounds[switchIndex] = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, frontaxle);
										train.Cars[c].RearAxle.PointSounds[switchIndex] = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, rearaxle);
									}
								}
								else
								{
									Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported index " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							}
							i++;
						}
						i--; break;
					case "[brake]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "bc release high":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].CarBrake.AirHigh = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, center);
										}

										break;
									case "bc release":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].CarBrake.Air = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, center);
										}

										break;
									case "bc release full":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].CarBrake.AirZero = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, center);
										}

										break;
									case "emergency":
										train.Handles.EmergencyBrake.ApplicationSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
										break;
									case "emergencyrelease":
										train.Handles.EmergencyBrake.ReleaseSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
										break;
									case "bp decomp":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].CarBrake.Release = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, center);
										}

										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}

							}
							i++;
						}
						i--; break;
					case "[compressor]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								for (int c = 0; c < train.Cars.Length; c++)
								{
									if (train.Cars[c].CarBrake.brakeType == BrakeType.Main)
									{
										switch (a.ToLowerInvariant())
										{
											case "attack":
												train.Cars[c].CarBrake.airCompressor.StartSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
												break;
											case "loop":
												train.Cars[c].CarBrake.airCompressor.LoopSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
												break;
											case "release":
												train.Cars[c].CarBrake.airCompressor.EndSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
												break;
											default:
												Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												break;
										}
									}
								}

							}
							i++;
						}
						i--; break;
					case "[suspension]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "left":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Sounds.SpringL = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, left);
										}

										break;
									case "right":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Sounds.SpringR = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, right);
										}

										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[horn]":
						i++;
						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								try
								{
									switch (a.ToLowerInvariant())
									{
										//PRIMARY HORN (Enter)
										case "primarystart":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var primaryStart);
											train.Cars[train.DriverCar].Horns[0].StartSound = primaryStart as SoundBuffer;
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
											break;
										case "primaryend":
										case "primaryrelease":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var primaryEnd);
											train.Cars[train.DriverCar].Horns[0].EndSound = primaryEnd as SoundBuffer;
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
											break;
										case "primaryloop":
										case "primary":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var primaryLoop);
											train.Cars[train.DriverCar].Horns[0].LoopSound = primaryLoop as SoundBuffer;
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].Loop = false;
											break;
										//SECONDARY HORN (Numpad Enter)
										case "secondarystart":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var secondaryStart);
											train.Cars[train.DriverCar].Horns[1].StartSound = secondaryStart as SoundBuffer;
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
											break;
										case "secondaryend":
										case "secondaryrelease":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var secondaryEnd);
											train.Cars[train.DriverCar].Horns[1].EndSound = secondaryEnd as SoundBuffer;
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
											break;
										case "secondaryloop":
										case "secondary":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius, out var secondaryLoop);
											train.Cars[train.DriverCar].Horns[1].LoopSound = secondaryLoop as SoundBuffer;
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].Loop = false;
											break;
										//MUSIC HORN
										case "musicstart":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius, out var musicStart);
											train.Cars[train.DriverCar].Horns[2].StartSound = musicStart as SoundBuffer;
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
											break;
										case "musicend":
										case "musicrelease":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius, out var musicEnd);
											train.Cars[train.DriverCar].Horns[2].EndSound = musicEnd as SoundBuffer;
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
											break;
										case "musicloop":
										case "music":
											Plugin.currentHost.RegisterSound(Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius, out var musicLoop);
											train.Cars[train.DriverCar].Horns[2].LoopSound = musicLoop as SoundBuffer;
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].Loop = true;
											break;
										default:
											Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
								catch
								{
									Plugin.currentHost.AddMessage(MessageType.Warning, false, "FileName contains illegal characters or is empty in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							}

							i++;
						}
						i--; break;
					case "[door]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "open left":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Doors[0].OpenSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, left);
										}

										break;
									case "open right":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Doors[1].OpenSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, right);
										}

										break;
									case "close left":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Doors[0].CloseSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, left);
										}

										break;
									case "close right":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].Doors[1].CloseSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, right);
										}
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[ats]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out var k))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										int n = train.Cars[train.DriverCar].Sounds.Plugin.Length;
										if (k >= n)
										{
											Array.Resize(ref train.Cars[train.DriverCar].Sounds.Plugin, k + 1);
											for (int h = n; h < k; h++)
											{
												train.Cars[train.DriverCar].Sounds.Plugin[h] = new CarSound();
											}
										}

										train.Cars[train.DriverCar].Sounds.Plugin[k] = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
									}
									else
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Index must be greater than or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}

							}

							i++;
						}
						i--; break;
					case "[buzzer]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "correct":
										train.SafetySystems.StationAdjust.AdjustAlarm = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}

							}
							i++;
						}
						i--; break;
					case "[pilot lamp]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "on":
										train.SafetySystems.PilotLamp.OnSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "off":
										train.SafetySystems.PilotLamp.OffSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[brake handle]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "apply":
										train.Handles.Brake.Increase = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "applyfast":
										train.Handles.Brake.IncreaseFast = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "release":
										train.Handles.Brake.Decrease = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "releasefast":
										train.Handles.Brake.DecreaseFast = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "min":
										train.Handles.Brake.Min = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "max":
										train.Handles.Brake.Max = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[master controller]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "up":
										train.Handles.Power.Increase = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "upfast":
										train.Handles.Power.IncreaseFast = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "down":
										train.Handles.Power.Decrease = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "downfast":
										train.Handles.Power.DecreaseFast = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "min":
										train.Handles.Power.Min = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "max":
										train.Handles.Power.Max = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[reverser]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "on":
										train.Handles.Reverser.EngageSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "off":
										train.Handles.Reverser.ReleaseSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[breaker]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "on":
										train.Cars[train.DriverCar].Breaker.Resume = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, panel);
										break;
									case "off":
										train.Cars[train.DriverCar].Breaker.ResumeOrInterrupt = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.smallRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[others]":
						i++;
						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "noise":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											if (train.Cars[c].Specs.IsMotorCar | c == train.DriverCar)
											{
												train.Cars[c].Sounds.Loop = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
											}
										}

										break;
									case "shoe":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.Cars[c].CarBrake.Rub = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.mediumRadius, center);
										}

										break;
									case "halt":
										for (int c = 0; c < train.Cars.Length; c++)
										{
											train.SafetySystems.PassAlarm.Sound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										}

										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
					case "[windscreen]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								switch (a.ToLowerInvariant())
								{
									case "raindrop":
										train.Cars[train.DriverCar].Windscreen.DropSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "wetwipe":
										train.Cars[train.DriverCar].Windscreen.Wipers.WetWipeSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "drywipe":
										train.Cars[train.DriverCar].Windscreen.Wipers.DryWipeSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									case "switch":
										train.Cars[train.DriverCar].Windscreen.Wipers.SwitchSound = new CarSound(Plugin.currentHost, trainFolder, FileName, i, b, SoundCfgParser.tinyRadius, panel);
										break;
									default:
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										break;
								}
							}
							i++;
						}
						i--; break;
				}
			}
			// motor sound
			for (int c = 0; c < train.Cars.Length; c++)
			{
				if (train.Cars[c].Specs.IsMotorCar)
				{
					if (train.Cars[c].Sounds.Motor is BVEMotorSound motorSound)
					{
						train.Cars[c].Sounds.Motor.Position = center;
						for (int i = 0; i < motorSound.Tables.Length; i++)
						{
							motorSound.Tables[i].Buffer = null;
							motorSound.Tables[i].Source = null;
							for (int j = 0; j < motorSound.Tables[i].Entries.Length; j++)
							{
								int index = motorSound.Tables[i].Entries[j].SoundIndex;
								if (index >= 0 && index < MotorFiles.Length && MotorFiles[index] != null)
								{
									Plugin.currentHost.RegisterSound(MotorFiles[index], SoundCfgParser.mediumRadius, out var mS);
									motorSound.Tables[i].Entries[j].Buffer = mS as SoundBuffer;
								}
							}
						}
					}
					else
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Unexpected motor sound model found in car " + c);
					}
				}
			}
		}
	}
}
