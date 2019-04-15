using System;
using System.Collections.Generic;
using System.Linq;
using OpenBve.BrakeSystems;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Interface;

namespace OpenBve
{
	class BVE4SoundParser
	{
		/// <summary>Loads the sound set for a BVE4 or openBVE sound.cfg based train</summary>
		/// <param name="Encoding">The text encoding for the sound.cfg file</param>
		/// <param name="train">The train</param>
		/// <param name="FileName">The absolute on-disk path to the sound.cfg file</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		internal static void Parse(string FileName, string trainFolder, System.Text.Encoding Encoding, TrainManager.Train train)
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
			//TODO: All radii are much too SoundCfgParser.smallRadius in external mode, but we can't change them by default.....
			

			// parse configuration file
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			List<string> Lines = System.IO.File.ReadAllLines(FileName, Encoding).ToList();
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

			if (Lines.Count == 0)
			{
				Interface.AddMessage(MessageType.Error, false, "Empty sound.cfg encountered in " + FileName + ".");
			}
			if (string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
			{
				Interface.AddMessage(MessageType.Error, false, "Invalid file format encountered in " + FileName + ". The first line is expected to be \"Version 1.0\".");
			}
			string[] MotorFiles = new string[] { };
			double invfac = Lines.Count == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Count;
			for (int i = 0; i < Lines.Count; i++)
			{
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
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
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										for (int c = 0; c < train.Cars.Length; c++)
										{
											int n = train.Cars[c].Sounds.Run.Length;
											if (k >= n)
											{
												Array.Resize<TrainManager.CarSound>(ref train.Cars[c].Sounds.Run, k + 1);
												for (int h = n; h < k; h++)
												{
													train.Cars[c].Sounds.Run[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[c].Sounds.Run[k] = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										for (int c = 0; c < train.Cars.Length; c++)
										{
											int n = train.Cars[c].Sounds.Flange.Length;
											if (k >= n)
											{
												Array.Resize<TrainManager.CarSound>(ref train.Cars[c].Sounds.Flange, k + 1);
												for (int h = n; h < k; h++)
												{
													train.Cars[c].Sounds.Flange[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[c].Sounds.Flange[k] = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									if (k >= 0)
									{
										if (k >= MotorFiles.Length)
										{
											Array.Resize<string>(ref MotorFiles, k + 1);
										}
										MotorFiles[k] = OpenBveApi.Path.CombineFile(trainFolder, b);
										if (!System.IO.File.Exists(MotorFiles[k]))
										{
											Interface.AddMessage(MessageType.Error, true, "File " + MotorFiles[k] + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											MotorFiles[k] = null;
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "Index is invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
								int runIndex;
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (NumberFormats.TryParseIntVb6(a, out runIndex))
								{

									for (int c = 0; c < train.Cars.Length; c++)
									{
										int n = train.Cars[c].FrontAxle.PointSounds.Length;
										if (runIndex >= n)
										{
											Array.Resize<TrainManager.CarSound>(ref train.Cars[c].FrontAxle.PointSounds, runIndex + 1);
											Array.Resize<TrainManager.CarSound>(ref train.Cars[c].RearAxle.PointSounds, runIndex + 1);
											for (int h = n; h < runIndex; h++)
											{
												train.Cars[c].FrontAxle.PointSounds[h] = TrainManager.CarSound.Empty;
												train.Cars[c].RearAxle.PointSounds[h] = TrainManager.CarSound.Empty;
											}
										}
										Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[c].FrontAxle.Position);
										Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[c].RearAxle.Position);
										train.Cars[c].FrontAxle.PointSounds[runIndex] = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), frontaxle, SoundCfgParser.smallRadius);
										train.Cars[c].RearAxle.PointSounds[runIndex] = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), rearaxle, SoundCfgParser.smallRadius);
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Warning, false, "Unsupported index " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "bc release high":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.AirHigh = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.smallRadius);
											}
											break;
										case "bc release":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.Air = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.smallRadius);
											}
											break;
										case "bc release full":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.AirZero = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.smallRadius);
											}
											break;
										case "emergency":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.EmrBrake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
											}
											break;
										case "bp decomp":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.Brake = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.smallRadius);
											}
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									for (int c = 0; c < train.Cars.Length; c++)
									{
										if (train.Cars[c].CarBrake.brakeType == BrakeType.Main)
										{
											switch (a.ToLowerInvariant())
											{
												case "attack":
													train.Cars[c].Sounds.CpStart = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
													break;
												case "loop":
													train.Cars[c].Sounds.CpLoop = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
													break;
												case "release":
													train.Cars[c].Sounds.CpEnd = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
													break;
												default:
													Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "left":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.SpringL = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), left, SoundCfgParser.smallRadius);
											}
											break;
										case "right":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.SpringR = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), right, SoundCfgParser.smallRadius);
											}
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										//PRIMARY HORN (Enter)
										case "primarystart":
											train.Cars[train.DriverCar].Horns[0].StartSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
											break;
										case "primaryend":
										case "primaryrelease":
											train.Cars[train.DriverCar].Horns[0].EndSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].StartEndSounds = true;
											break;
										case "primaryloop":
										case "primary":
											train.Cars[train.DriverCar].Horns[0].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[0].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[0].Loop = false;
											break;
										//SECONDARY HORN (Numpad Enter)
										case "secondarystart":
											train.Cars[train.DriverCar].Horns[1].StartSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
											break;
										case "secondaryend":
										case "secondaryrelease":
											train.Cars[train.DriverCar].Horns[1].EndSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].StartEndSounds = true;
											break;
										case "secondaryloop":
										case "secondary":
											train.Cars[train.DriverCar].Horns[1].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.largeRadius);
											train.Cars[train.DriverCar].Horns[1].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[1].Loop = false;
											break;
										//MUSIC HORN
										case "musicstart":
											train.Cars[train.DriverCar].Horns[2].StartSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius);
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
											break;
										case "musicend":
										case "musicrelease":
											train.Cars[train.DriverCar].Horns[2].EndSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius);
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].StartEndSounds = true;
											break;
										case "musicloop":
										case "music":
											train.Cars[train.DriverCar].Horns[2].LoopSound = Sounds.SoundBuffer.TryToLoad(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius);
											train.Cars[train.DriverCar].Horns[2].SoundPosition = front;
											train.Cars[train.DriverCar].Horns[2].Loop = true;
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "open left":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Doors[0].OpenSound = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), left, SoundCfgParser.smallRadius);
											}
											break;
										case "open right":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Doors[1].OpenSound = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), left, SoundCfgParser.smallRadius);
											}
											break;
										case "close left":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Doors[0].CloseSound = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), left, SoundCfgParser.smallRadius);
											}
											break;
										case "close right":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Doors[1].CloseSound = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), left, SoundCfgParser.smallRadius);
											}
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									int k;
									if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k))
									{
										Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
									else
									{
										if (k >= 0)
										{
											int n = train.Cars[train.DriverCar].Sounds.Plugin.Length;
											if (k >= n)
											{
												Array.Resize<TrainManager.CarSound>(ref train.Cars[train.DriverCar].Sounds.Plugin, k + 1);
												for (int h = n; h < k; h++)
												{
													train.Cars[train.DriverCar].Sounds.Plugin[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[train.DriverCar].Sounds.Plugin[k] = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
										}
										else
										{
											Interface.AddMessage(MessageType.Warning, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "correct":
											train.Cars[train.DriverCar].Sounds.Adjust = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											train.Cars[train.DriverCar].Sounds.PilotLampOn = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.PilotLampOff = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "apply":
											train.Cars[train.DriverCar].Sounds.BrakeHandleApply = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "applyfast":
											train.Cars[train.DriverCar].Sounds.BrakeHandleApplyFast = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "release":
											train.Cars[train.DriverCar].Sounds.BrakeHandleRelease = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "releasefast":
											train.Cars[train.DriverCar].Sounds.BrakeHandleReleaseFast = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "min":
											train.Cars[train.DriverCar].Sounds.BrakeHandleMin = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "max":
											train.Cars[train.DriverCar].Sounds.BrakeHandleMax = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "up":
											train.Cars[train.DriverCar].Sounds.MasterControllerUp = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "upfast":
											train.Cars[train.DriverCar].Sounds.MasterControllerUpFast = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "down":
											train.Cars[train.DriverCar].Sounds.MasterControllerDown = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "downfast":
											train.Cars[train.DriverCar].Sounds.MasterControllerDownFast = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "min":
											train.Cars[train.DriverCar].Sounds.MasterControllerMin = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "max":
											train.Cars[train.DriverCar].Sounds.MasterControllerMax = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											train.Cars[train.DriverCar].Sounds.ReverserOn = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.ReverserOff = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
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
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "on":
											train.Cars[train.DriverCar].Sounds.BreakerResume = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.smallRadius);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.BreakerResumeOrInterrupt = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.smallRadius);
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							}
							i++;
						}
						i--; break;
					case "[others]":
						i++; while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									switch (a.ToLowerInvariant())
									{
										case "noise":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												if (train.Cars[c].Specs.IsMotorCar | c == train.DriverCar)
												{
													train.Cars[c].Sounds.Loop = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
												}
											}
											break;
										case "shoe":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.Rub = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), center, SoundCfgParser.mediumRadius);
											}
											break;
										case "halt":
											for (int c = 0; c < train.Cars.Length; c++)
											{
												train.Cars[c].Sounds.Halt = new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, b), panel, SoundCfgParser.tinyRadius);
											}
											break;
										default:
											Interface.AddMessage(MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							}
							i++;
						}
						i--; break;
				}
			}
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
			}
			// motor sound
			for (int c = 0; c < train.Cars.Length; c++)
			{
				if (train.Cars[c].Specs.IsMotorCar)
				{
					train.Cars[c].Sounds.Motor.Position = center;
					for (int i = 0; i < train.Cars[c].Sounds.Motor.Tables.Length; i++)
					{
						train.Cars[c].Sounds.Motor.Tables[i].Buffer = null;
						train.Cars[c].Sounds.Motor.Tables[i].Source = null;
						for (int j = 0; j < train.Cars[c].Sounds.Motor.Tables[i].Entries.Length; j++)
						{
							int index = train.Cars[c].Sounds.Motor.Tables[i].Entries[j].SoundIndex;
							if (index >= 0 && index < MotorFiles.Length && MotorFiles[index] != null)
							{
								train.Cars[c].Sounds.Motor.Tables[i].Entries[j].Buffer = Sounds.RegisterBuffer(MotorFiles[index], SoundCfgParser.mediumRadius);
							}
						}
					}
				}
			}
		}
	}
}
