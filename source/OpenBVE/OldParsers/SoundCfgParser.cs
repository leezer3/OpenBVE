using System;
using OpenBveApi;
using OpenBveApi.Math;

namespace OpenBve {
	internal static class SoundCfgParser {

		// parse sound config
		internal static void ParseSoundConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.cfg");
			if (System.IO.File.Exists(FileName)) {
				LoadBve4Sound(FileName, TrainPath, Encoding, Train);
			} else {
				LoadBve2Sound(TrainPath, Train);
			}
		}
		
		internal static void LoadDefaultPluginSounds(TrainManager.Train train, string trainFolder) {
			Vector3 position = new Vector3(train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ + 1.0);
			const double radius = 2.0;
			train.Cars[train.DriverCar].Sounds.Plugin = new TrainManager.CarSound[] {
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "ats.wav"), position, radius),
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "atscnt.wav"), position, radius),
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "ding.wav"), position, radius),
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "toats.wav"), position, radius),
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "toatc.wav"), position, radius),
				TryLoadSound(OpenBveApi.Path.CombineFile(trainFolder, "eb.wav"), position, radius)
			};
		}

		// load no sound
		internal static void LoadNoSound(TrainManager.Train train) {
			// initialize
			for (int i = 0; i < train.Cars.Length; i++) {
				train.Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
				train.Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
				train.Cars[i].Sounds.Adjust = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Air = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.AirHigh = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.AirZero = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Brake = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BrakeHandleApply = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BrakeHandleMin = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BrakeHandleMax = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BrakeHandleRelease = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.CpEnd = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.CpLoop = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.CpStart = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.DoorCloseL = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.DoorCloseR = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.DoorOpenL = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.DoorOpenR = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.EmrBrake = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
				train.Cars[i].Sounds.FlangeVolume = new double[] { };
				train.Cars[i].Sounds.Halt = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Horns = new TrainManager.Horn[] { TrainManager.Horn.Empty, TrainManager.Horn.Empty, TrainManager.Horn.Empty };
				train.Cars[i].Sounds.Loop = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PilotLampOn = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PointFrontAxle = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PointRearAxle = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.ReverserOn = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.ReverserOff = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Rub = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
				train.Cars[i].Sounds.RunVolume = new double[] { };
				train.Cars[i].Sounds.SpringL = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.SpringR = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.Plugin = new TrainManager.CarSound[] { };
			}
		}

		// load bve 2 sound
		private static void LoadBve2Sound(string TrainPath, TrainManager.Train train) {
			// set sound positions and radii
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			Vector3 center = new Vector3(0.0, 0.0, 0.0);
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 cab = new Vector3(-train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ - 0.5);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ + 1.0);
			const double large = 30.0;
			const double medium = 10.0;
			const double small = 5.0;
			const double tiny = 2.0;
			LoadNoSound(train);
			// load sounds for driver's car
			train.Cars[train.DriverCar].Sounds.Adjust = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Adjust.wav"), panel, tiny);
			train.Cars[train.DriverCar].Sounds.Brake = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Brake.wav"), center, small);
			train.Cars[train.DriverCar].Sounds.Halt = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Halt.wav"), cab, tiny);
			train.Cars[train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Klaxon0.wav"), front, large);
			train.Cars[train.DriverCar].Sounds.Horns[0].Loop = false;
			if (train.Cars[train.DriverCar].Sounds.Horns[0].Sound.Buffer == null) {
				train.Cars[train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Klaxon.wav"), front, large);
			}
			train.Cars[train.DriverCar].Sounds.Horns[1].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Klaxon1.wav"), front, large);
			train.Cars[train.DriverCar].Sounds.Horns[1].Loop = false;
			train.Cars[train.DriverCar].Sounds.Horns[2].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Klaxon2.wav"), front, medium);
			train.Cars[train.DriverCar].Sounds.Horns[2].Loop = true;
			train.Cars[train.DriverCar].Sounds.PilotLampOn = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Leave.wav"), cab, tiny);
			train.Cars[train.DriverCar].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
			// load sounds for all cars
			for (int i = 0; i < train.Cars.Length; i++) {
				Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[i].FrontAxlePosition);
				Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[i].RearAxlePosition);
				train.Cars[i].Sounds.Air = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Air.wav"), center, small);
				train.Cars[i].Sounds.AirHigh = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "AirHigh.wav"), center, small);
				train.Cars[i].Sounds.AirZero = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "AirZero.wav"), center, small);
				if (train.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
					train.Cars[i].Sounds.CpEnd = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "CpEnd.wav"), center, medium);
					train.Cars[i].Sounds.CpLoop = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "CpLoop.wav"), center, medium);
					train.Cars[i].Sounds.CpStart = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "CpStart.wav"), center, medium);
				}
				train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.BreakerResumed = false;
				train.Cars[i].Sounds.DoorCloseL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorClsL.wav"), left, small);
				train.Cars[i].Sounds.DoorCloseR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorClsR.wav"), right, small);
				if (train.Cars[i].Sounds.DoorCloseL.Buffer == null) {
					train.Cars[i].Sounds.DoorCloseL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorCls.wav"), left, small);
				}
				if (train.Cars[i].Sounds.DoorCloseR.Buffer == null) {
					train.Cars[i].Sounds.DoorCloseR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorCls.wav"), right, small);
				}
				train.Cars[i].Sounds.DoorOpenL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorOpnL.wav"), left, small);
				train.Cars[i].Sounds.DoorOpenR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorOpnR.wav"), right, small);
				if (train.Cars[i].Sounds.DoorOpenL.Buffer == null) {
					train.Cars[i].Sounds.DoorOpenL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorOpn.wav"), left, small);
				}
				if (train.Cars[i].Sounds.DoorOpenR.Buffer == null) {
					train.Cars[i].Sounds.DoorOpenR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "DoorOpn.wav"), right, small);
				}
				train.Cars[i].Sounds.EmrBrake = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "EmrBrake.wav"), center, medium);
				train.Cars[i].Sounds.Flange = TryLoadSoundArray(TrainPath, "Flange", ".wav", center, medium);
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
				train.Cars[i].Sounds.Loop = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Loop.wav"), center, medium);
				train.Cars[i].Sounds.PointFrontAxle = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Point.wav"), frontaxle, small);
				train.Cars[i].Sounds.PointRearAxle = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Point.wav"), rearaxle, small);
				train.Cars[i].Sounds.Rub = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Rub.wav"), center, medium);
				train.Cars[i].Sounds.Run = TryLoadSoundArray(TrainPath, "Run", ".wav", center, medium);
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.SpringL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "SpringL.wav"), left, small);
				train.Cars[i].Sounds.SpringR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "SpringR.wav"), right, small);
				// motor sound
				if (train.Cars[i].Specs.IsMotorCar) {
					System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
					train.Cars[i].Sounds.Motor.Position = center;
					for (int j = 0; j < train.Cars[i].Sounds.Motor.Tables.Length; j++) {
						for (int k = 0; k < train.Cars[i].Sounds.Motor.Tables[j].Entries.Length; k++) {
							int idx = train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundIndex;
							if (idx >= 0) {
								TrainManager.CarSound snd = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, "Motor" + idx.ToString(Culture) + ".wav"), center, medium);
								train.Cars[i].Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
							}
						}
					}
				}
			}
		}

		// load bve 4 sound
		private static void LoadBve4Sound(string FileName, string TrainPath, System.Text.Encoding Encoding, TrainManager.Train train) {
			// set sound positions and radii
			Vector3 center = new Vector3(0.0, 0.0, 0.0);
			Vector3 left = new Vector3(-1.3, 0.0, 0.0);
			Vector3 right = new Vector3(1.3, 0.0, 0.0);
			Vector3 front = new Vector3(0.0, 0.0, 0.5 * train.Cars[train.DriverCar].Length);
			Vector3 panel = new Vector3(train.Cars[train.DriverCar].DriverX, train.Cars[train.DriverCar].DriverY, train.Cars[train.DriverCar].DriverZ + 1.0);
			double large = 30.0;
			double medium = 10.0;
			double small = 5.0;
			double tiny = 2.0;
			LoadNoSound(train);
			// parse configuration file
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
			}
			if (Lines.Length < 1 || string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0) {
				Interface.AddMessage(Interface.MessageType.Error, false, "Invalid file format encountered in " + FileName + ". The first line is expected to be \"Version 1.0\".");
			}
			string[] MotorFiles = new string[] { };
			double invfac = Lines.Length == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				switch (Lines[i].ToLowerInvariant()) {
					case "[run]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										for (int c = 0; c < train.Cars.Length; c++) {
											int n = train.Cars[c].Sounds.Run.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref train.Cars[c].Sounds.Run, k + 1);
												for (int h = n; h < k; h++) {
													train.Cars[c].Sounds.Run[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[c].Sounds.Run[k] = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[flange]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										for (int c = 0; c < train.Cars.Length; c++) {
											int n = train.Cars[c].Sounds.Flange.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref train.Cars[c].Sounds.Flange, k + 1);
												for (int h = n; h < k; h++) {
													train.Cars[c].Sounds.Flange[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[c].Sounds.Flange[k] = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[motor]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										if (k >= MotorFiles.Length) {
											Array.Resize<string>(ref MotorFiles, k + 1);
										}
										MotorFiles[k] = OpenBveApi.Path.CombineFile(TrainPath, b);
										if (!System.IO.File.Exists(MotorFiles[k])) {
											Interface.AddMessage(Interface.MessageType.Error, true, "File " + MotorFiles[k] + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											MotorFiles[k] = null;
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[switch]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (a == "0") {
									for (int c = 0; c < train.Cars.Length; c++) {
										Vector3 frontaxle = new Vector3(0.0, 0.0, train.Cars[c].FrontAxlePosition);
										Vector3 rearaxle = new Vector3(0.0, 0.0, train.Cars[c].RearAxlePosition);
										train.Cars[c].Sounds.PointFrontAxle = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), frontaxle, small);
										train.Cars[c].Sounds.PointRearAxle = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), rearaxle, small);
									}
								} else {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported index " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} i++;
						} i--; break;
					case "[brake]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "bc release high":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.AirHigh = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, small);
											} break;
										case "bc release":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.Air = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, small);
											} break;
										case "bc release full":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.AirZero = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, small);
											} break;
										case "emergency":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.EmrBrake = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
											} break;
										case "bp decomp":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.Brake = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[compressor]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									for (int c = 0; c < train.Cars.Length; c++) {
										if (train.Cars[c].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
											switch (a.ToLowerInvariant()) {
												case "attack":
													train.Cars[c].Sounds.CpStart = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
													break;
												case "loop":
													train.Cars[c].Sounds.CpLoop = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
													break;
												case "release":
													train.Cars[c].Sounds.CpEnd = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
													break;
												default:
													Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
									}
								}
							} i++;
						} i--; break;
					case "[suspension]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "left":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.SpringL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), left, small);
											} break;
										case "right":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.SpringR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), right, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[horn]":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "primary":
											train.Cars[train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), front, large);
											train.Cars[train.DriverCar].Sounds.Horns[0].Loop = false;
											break;
										case "secondary":
											train.Cars[train.DriverCar].Sounds.Horns[1].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), front, large);
											train.Cars[train.DriverCar].Sounds.Horns[1].Loop = false;
											break;
										case "music":
											train.Cars[train.DriverCar].Sounds.Horns[2].Sound = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), front, medium);
											train.Cars[train.DriverCar].Sounds.Horns[2].Loop = true;
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[door]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "open left":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.DoorOpenL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), left, small);
											} break;
										case "open right":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.DoorOpenR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), left, small);
											} break;
										case "close left":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.DoorCloseL = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), left, small);
											} break;
										case "close right":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.DoorCloseR = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), left, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[ats]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									int k;
									if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (k >= 0) {
											int n = train.Cars[train.DriverCar].Sounds.Plugin.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref train.Cars[train.DriverCar].Sounds.Plugin, k + 1);
												for (int h = n; h < k; h++) {
													train.Cars[train.DriverCar].Sounds.Plugin[h] = TrainManager.CarSound.Empty;
												}
											}
											train.Cars[train.DriverCar].Sounds.Plugin[k] = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
										} else {
											Interface.AddMessage(Interface.MessageType.Warning, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
								}
							} i++;
						} i--; break;
					case "[buzzer]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "correct":
											train.Cars[train.DriverCar].Sounds.Adjust = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[pilot lamp]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											train.Cars[train.DriverCar].Sounds.PilotLampOn = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.PilotLampOff = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[brake handle]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "apply":
											train.Cars[train.DriverCar].Sounds.BrakeHandleApply = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "release":
											train.Cars[train.DriverCar].Sounds.BrakeHandleRelease = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "min":
											train.Cars[train.DriverCar].Sounds.BrakeHandleMin = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "max":
											train.Cars[train.DriverCar].Sounds.BrakeHandleMax = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[master controller]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "up":
											train.Cars[train.DriverCar].Sounds.MasterControllerUp = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "down":
											train.Cars[train.DriverCar].Sounds.MasterControllerDown = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "min":
											train.Cars[train.DriverCar].Sounds.MasterControllerMin = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "max":
											train.Cars[train.DriverCar].Sounds.MasterControllerMax = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[reverser]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											train.Cars[train.DriverCar].Sounds.ReverserOn = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.ReverserOff = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[breaker]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											train.Cars[train.DriverCar].Sounds.BreakerResume = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, small);
											break;
										case "off":
											train.Cars[train.DriverCar].Sounds.BreakerResumeOrInterrupt = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), panel, small);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[others]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (b.Length == 0 || Path.ContainsInvalidChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters or is empty at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "noise":
											for (int c = 0; c < train.Cars.Length; c++) {
												if (train.Cars[c].Specs.IsMotorCar | c == train.DriverCar) {
													train.Cars[c].Sounds.Loop = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
												}
											} break;
										case "shoe":
											for (int c = 0; c < train.Cars.Length; c++) {
												train.Cars[c].Sounds.Rub = TryLoadSound(OpenBveApi.Path.CombineFile(TrainPath, b), center, medium);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
				}
			}
			for (int i = 0; i < train.Cars.Length; i++) {
				train.Cars[i].Sounds.RunVolume = new double[train.Cars[i].Sounds.Run.Length];
				train.Cars[i].Sounds.FlangeVolume = new double[train.Cars[i].Sounds.Flange.Length];
			}
			// motor sound
			for (int c = 0; c < train.Cars.Length; c++) {
				if (train.Cars[c].Specs.IsMotorCar) {
					train.Cars[c].Sounds.Motor.Position = center;
					for (int i = 0; i < train.Cars[c].Sounds.Motor.Tables.Length; i++) {
						train.Cars[c].Sounds.Motor.Tables[i].Buffer = null;
						train.Cars[c].Sounds.Motor.Tables[i].Source = null;
						for (int j = 0; j < train.Cars[c].Sounds.Motor.Tables[i].Entries.Length; j++) {
							int index = train.Cars[c].Sounds.Motor.Tables[i].Entries[j].SoundIndex;
							if (index >= 0 && index < MotorFiles.Length && MotorFiles[index] != null) {
								train.Cars[c].Sounds.Motor.Tables[i].Entries[j].Buffer = Sounds.RegisterBuffer(MotorFiles[index], medium);
							}
						}
					}
				}
			}
		}

		// try load sound
		private static TrainManager.CarSound TryLoadSound(string FileName, Vector3 Position, double Radius) {
			TrainManager.CarSound s = TrainManager.CarSound.Empty;
			s.Position = Position;
			s.Source = null;
			if (FileName != null) {
				if (System.IO.File.Exists(FileName)) {
					s.Buffer = Sounds.RegisterBuffer(FileName, Radius);
				}
			}
			return s;
		}
		private static TrainManager.CarSound[] TryLoadSoundArray(string Folder, string FileStart, string FileEnd, Vector3 Position, double Radius) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			TrainManager.CarSound[] Sounds = new TrainManager.CarSound[] { };
			string[] Files = System.IO.Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++) {
				string a = System.IO.Path.GetFileName(Files[i]);
                if (a == null) return Sounds;
				if (a.Length > FileStart.Length + FileEnd.Length) {
					if (a.StartsWith(FileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(FileEnd, StringComparison.OrdinalIgnoreCase)) {
						string b = a.Substring(FileStart.Length, a.Length - FileEnd.Length - FileStart.Length);
						int n; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, Culture, out n)) {
							if (n >= 0) {
								int m = Sounds.Length;
								if (n >= m) {
									Array.Resize<TrainManager.CarSound>(ref Sounds, n + 1);
									for (int j = m; j < n; j++) {
										Sounds[j] = TrainManager.CarSound.Empty;
										Sounds[j].Source = null;
									}
								}
								Sounds[n] = TryLoadSound(Files[i], Position, Radius);
							}
						}
					}
				}
			}
			return Sounds;
		}

	}
}