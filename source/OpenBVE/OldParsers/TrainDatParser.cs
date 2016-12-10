﻿using System;
using System.Windows.Forms;

namespace OpenBve {
	internal static class TrainDatParser {

		// parse train data
		internal static void ParseTrainData(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			//Create the array using the default compatibility train.dat
			string[] Lines = {"BVE2000000","#CAR","1","1","1","0","1","1"};
			// load file
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "train.dat");
			try
			{
				Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			}
			catch
			{
			}
			if (Lines.Length == 0)
			{
				//Catch zero-length train.dat files
				MessageBox.Show("The train.dat file " + OpenBveApi.Path.CombineFile(TrainPath, "train.dat") + " is of zero length.");
				throw new Exception("The train.dat file " + OpenBveApi.Path.CombineFile(TrainPath, "train.dat") + " is of zero length.");
			}
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
			}
			bool ver1220000 = false;
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length > 0) {
					string t = Lines[i].ToLowerInvariant();
					if (t == "bve1220000") {
						ver1220000 = true;
					} else if (t != "bve2000000" & t != "openbve") {
						Interface.AddMessage(Interface.MessageType.Error, false, "The train.dat format " + Lines[0].ToLowerInvariant() + " is not supported in " + OpenBveApi.Path.CombineFile(TrainPath, "train.dat"));
					}
					break;
				}
			}
			// initialize
			Train.Cars = new TrainManager.Car[] { };
			Train.Couplers = null;
			double BrakeCylinderServiceMaximumPressure = 440000.0;
			double BrakeCylinderEmergencyMaximumPressure = 440000.0;
			double MainReservoirMinimumPressure = 690000.0;
			double MainReservoirMaximumPressure = 780000.0;
			double BrakePipePressure = 0.0;
			TrainManager.CarBrakeType BrakeType = TrainManager.CarBrakeType.ElectromagneticStraightAirBrake;
			TrainManager.EletropneumaticBrakeType ElectropneumaticType = TrainManager.EletropneumaticBrakeType.None;
			double BrakeControlSpeed = 0.0;
			double BrakeDeceleration = 0.277777777777778;
			double JerkPowerUp = 10.0;
			double JerkPowerDown = 10.0;
			double JerkBrakeUp = 10.0;
			double JerkBrakeDown = 10.0;
			double BrakeCylinderUp = 300000.0;
			double BrakeCylinderDown = 200000.0;
			double CoefficientOfStaticFriction = 0.35;
			double CoefficientOfRollingResistance = 0.0025;
			double AerodynamicDragCoefficient = 1.1;
			TrainManager.AccelerationCurve[] AccelerationCurves = new TrainManager.AccelerationCurve[] { };
			double DriverX = 0.0, DriverY = 0.0, DriverZ = 0.0;
			int DriverCar = 0;
			double MotorCarMass = 1.0, TrailerCarMass = 1.0;
			int MotorCars = 0, TrailerCars = 0;
			double CarLength = 20.0;
			double CarWidth = 2.6;
			double CarHeight = 3.6;
			double CenterOfGravityHeight = 1.6;
			double CarExposedFrontalArea = 0.6 * CarWidth * CarHeight;
			double CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
			bool FrontCarIsMotorCar = true;
			int ReAdhesionDevice = 0;
			Train.Specs.PassAlarm = TrainManager.PassAlarmType.None;
			Train.Specs.DoorOpenMode = TrainManager.DoorMode.AutomaticManualOverride;
			Train.Specs.DoorCloseMode = TrainManager.DoorMode.AutomaticManualOverride;
			TrainManager.MotorSoundTable[] Tables = new TrainManager.MotorSoundTable[4];
			for (int i = 0; i < 4; i++) {
				Tables[i].Entries = new TrainManager.MotorSoundTableEntry[16];
				for (int j = 0; j < 16; j++) {
					Tables[i].Entries[j].SoundIndex = -1;
					Tables[i].Entries[j].Pitch = 1.0f;
					Tables[i].Entries[j].Gain = 1.0f;
				}
			}
			// parse configuration
			double invfac = Lines.Length == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				int n = 0;
				switch (Lines[i].ToLowerInvariant()) {
					case "#acceleration":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							Array.Resize<TrainManager.AccelerationCurve>(ref AccelerationCurves, n + 1);
							string t = Lines[i] + ",";
							int m = 0;
							while (true) {
								int j = t.IndexOf(',');
								if (j == -1) break;
								string s = t.Substring(0, j).Trim();
								t = t.Substring(j + 1);
								double a; if (Interface.TryParseDoubleVb6(s, out a)) {
									switch (m) {
										case 0:
											if (a < 0.0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "a0 in section #ACCELERATION is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											} else {
												AccelerationCurves[n].StageZeroAcceleration = a * 0.277777777777778;
											} break;
										case 1:
											if (a < 0.0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "a1 in section #ACCELERATION is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											} else {
												AccelerationCurves[n].StageOneAcceleration = a * 0.277777777777778;
											} break;
										case 2:
											if (a < 0.0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "v1 in section #ACCELERATION is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											} else {
												AccelerationCurves[n].StageOneSpeed = a * 0.277777777777778;
											} break;
										case 3:
											if (a < 0.0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "v2 in section #ACCELERATION is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											} else {
												AccelerationCurves[n].StageTwoSpeed = a * 0.277777777777778;
												if (AccelerationCurves[n].StageTwoSpeed < AccelerationCurves[n].StageOneSpeed) {
													Interface.AddMessage(Interface.MessageType.Error, false, "v2 in section #ACCELERATION is expected to be greater than or equal to v1 at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													AccelerationCurves[n].StageTwoSpeed = AccelerationCurves[n].StageOneSpeed;
												}
											} break;
										case 4:
											{
												if (ver1220000) {
													if (a <= 0.0) {
														AccelerationCurves[n].StageTwoExponent = 1.0;
														Interface.AddMessage(Interface.MessageType.Error, false, "e in section #ACCELERATION is expected to be positive at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														const double c = 4.439346232277577;
														AccelerationCurves[n].StageTwoExponent = 1.0 - Math.Log(a) * AccelerationCurves[n].StageTwoSpeed * c;
														if (AccelerationCurves[n].StageTwoExponent <= 0.0) {
															AccelerationCurves[n].StageTwoExponent = 1.0;
														} else if (AccelerationCurves[n].StageTwoExponent > 4.0) {
															AccelerationCurves[n].StageTwoExponent = 4.0;
														}
													}
												} else {
													AccelerationCurves[n].StageTwoExponent = a;
													if (AccelerationCurves[n].StageTwoExponent <= 0.0) {
														AccelerationCurves[n].StageTwoExponent = 1.0;
													}
												}
											} break;
									}
								} m++;
							} i++; n++;
						} i--; break;
					case "#performance":
					case "#deceleration":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
									case 0:
										if (a < 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "BrakeDeceleration is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											BrakeDeceleration = a * 0.277777777777778;
										} break;
									case 1:
										if (a < 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "CoefficientOfStaticFriction is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CoefficientOfStaticFriction = a;
										} break;
									case 3:
										if (a < 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "CoefficientOfRollingResistance is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CoefficientOfRollingResistance = a;
										} break;
									case 4:
										if (a < 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "AerodynamicDragCoefficient is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											AerodynamicDragCoefficient = a;
										} break;
								}
							} i++; n++;
						} i--; break;
					case "#delay":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
										case 0: Train.Specs.DelayPowerUp = a; break;
										case 1: Train.Specs.DelayPowerDown = a; break;
										case 2: Train.Specs.DelayBrakeUp = a; break;
										case 3: Train.Specs.DelayBrakeDown = a; break;
								}
							} i++; n++;
						} i--; break;
					case "#move":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
										case 0: JerkPowerUp = 0.01 * a; break;
										case 1: JerkPowerDown = 0.01 * a; break;
										case 2: JerkBrakeUp = 0.01 * a; break;
										case 3: JerkBrakeDown = 0.01 * a; break;
										case 4: BrakeCylinderUp = 1000.0 * a; break;
										case 5: BrakeCylinderDown = 1000.0 * a; break;
								}
							} i++; n++;
						} i--; break;
					case "#brake":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
									case 0:
										{
											int b = (int)Math.Round(a);
											if (b >= 0 & b <= 2) {
												BrakeType = (TrainManager.CarBrakeType)b;
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "The setting for BrakeType is invalid at line " + (i + 1).ToString(Culture) + " in " + FileName);
												BrakeType = TrainManager.CarBrakeType.ElectromagneticStraightAirBrake;
											}
										} break;
									case 1:
										{
											int b = (int)Math.Round(a);
											if (b >= 0 & b <= 2) {
												ElectropneumaticType = (TrainManager.EletropneumaticBrakeType)b;
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "The setting for ElectropneumaticType is invalid at line " + (i + 1).ToString(Culture) + " in " + FileName);
												ElectropneumaticType = TrainManager.EletropneumaticBrakeType.None;
											}
										} break;
										case 2: BrakeControlSpeed = a * 0.277777777777778; break;
								}
							} i++; n++;
						} i--; break;
					case "#pressure":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
									case 0:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "BrakeCylinderServiceMaximumPressure is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											BrakeCylinderServiceMaximumPressure = a * 1000.0;
										} break;
									case 1:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "BrakeCylinderEmergencyMaximumPressure is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											BrakeCylinderEmergencyMaximumPressure = a * 1000.0;
										} break;
									case 2:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "MainReservoirMinimumPressure is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											MainReservoirMinimumPressure = a * 1000.0;
										} break;
									case 3:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "MainReservoirMaximumPressure is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											MainReservoirMaximumPressure = a * 1000.0;
										} break;
									case 4:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "BrakePipePressure is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											BrakePipePressure = a * 1000.0;
										} break;
								}
							} i++; n++;
						} i--; break;
					case "#handle":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							int a; if (Interface.TryParseIntVb6(Lines[i], out a)) {
								switch (n) {
										case 0: Train.Specs.SingleHandle = a == 1; break;
										case 1: Train.Specs.MaximumPowerNotch = a; break;
										case 2: Train.Specs.MaximumBrakeNotch = a; break;
										case 3: Train.Specs.PowerNotchReduceSteps = a; break;
								}
							} i++; n++;
						} i--; break;
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
										case 0: DriverX = 0.001 * a; break;
										case 1: DriverY = 0.001 * a; break;
										case 2: DriverZ = 0.001 * a; break;
										case 3: DriverCar = (int)Math.Round(a); break;
								}
							} i++; n++;
						} i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
									case 0:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "MotorCarMass is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											MotorCarMass = a * 1000.0;
										} break;
									case 1:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "NumberOfMotorCars is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											MotorCars = (int)Math.Round(a);
										} break;
										case 2: TrailerCarMass = a * 1000.0; break;
									case 3:
										if (a < 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "NumberOfTrailerCars is expected to be non-negative at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											TrailerCars = (int)Math.Round(a);
										} break;
									case 4:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "LengthOfACar is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CarLength = a;
										} break;
										case 5: FrontCarIsMotorCar = a == 1.0; break;
									case 6:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "WidthOfACar is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CarWidth = a;
											CarExposedFrontalArea = 0.65 * CarWidth * CarHeight;
											CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
										} break;
									case 7:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "HeightOfACar is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CarHeight = a;
											CarExposedFrontalArea = 0.65 * CarWidth * CarHeight;
											CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
										} break;
										case 8: CenterOfGravityHeight = a; break;
									case 9:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "ExposedFrontalArea is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CarExposedFrontalArea = a;
											CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
										} break;
									case 10:
										if (a <= 0.0) {
											Interface.AddMessage(Interface.MessageType.Error, false, "UnexposedFrontalArea is expected to be positive at line " + (i + 1).ToString(Culture) + " in " + FileName);
										} else {
											CarExposedFrontalArea = a;
										} break;
								}
							} i++; n++;
						} i--; break;
					case "#device":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							double a; if (Interface.TryParseDoubleVb6(Lines[i], out a)) {
								switch (n) {
									case 0:
										if (a == 0.0) {
											Train.Specs.DefaultSafetySystems |= TrainManager.DefaultSafetySystems.AtsSn;
										} else if (a == 1.0) {
											Train.Specs.DefaultSafetySystems |= TrainManager.DefaultSafetySystems.AtsSn;
											Train.Specs.DefaultSafetySystems |= TrainManager.DefaultSafetySystems.AtsP;
										}
										break;
									case 1:
										if (a == 1.0 | a == 2.0) {
											Train.Specs.DefaultSafetySystems |= TrainManager.DefaultSafetySystems.Atc;
										}
										break;
									case 2:
										if (a == 1.0) {
											Train.Specs.DefaultSafetySystems |= TrainManager.DefaultSafetySystems.Eb;
										}
										break;
									case 3:
										Train.Specs.HasConstSpeed = a == 1.0; break;
									case 4:
										Train.Specs.HasHoldBrake = a == 1.0; break;
									case 5:
										ReAdhesionDevice = (int)Math.Round(a); break;
									case 7:
										{
											int b = (int)Math.Round(a);
											if (b >= 0 & b <= 2) {
												Train.Specs.PassAlarm = (TrainManager.PassAlarmType)b;
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "PassAlarm is invalid at line " + (i + 1).ToString(Culture) + " in " + FileName);
											} break;
										}
									case 8:
										{
											int b = (int)Math.Round(a);
											if (b >= 0 & b <= 2) {
												Train.Specs.DoorOpenMode = (TrainManager.DoorMode)b;
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "DoorOpenMode is invalid at line " + (i + 1).ToString(Culture) + " in " + FileName);
											} break;
										}
									case 9:
										{
											int b = (int)Math.Round(a);
											if (b >= 0 & b <= 2) {
												Train.Specs.DoorCloseMode = (TrainManager.DoorMode)b;
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "DoorCloseMode is invalid at line " + (i + 1).ToString(Culture) + " in " + FileName);
											} break;
										}
								}
							} i++; n++;
						} i--; break;
					case "#motor_p1":
					case "#motor_p2":
					case "#motor_b1":
					case "#motor_b2":
						{
							int msi = 0;
							switch (Lines[i].ToLowerInvariant()) {
									case "#motor_p1": msi = TrainManager.MotorSound.MotorP1; break;
									case "#motor_p2": msi = TrainManager.MotorSound.MotorP2; break;
									case "#motor_b1": msi = TrainManager.MotorSound.MotorB1; break;
									case "#motor_b2": msi = TrainManager.MotorSound.MotorB2; break;
							} i++;
							while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
								int u = Tables[msi].Entries.Length;
								if (n >= u) {
									Array.Resize<TrainManager.MotorSoundTableEntry>(ref Tables[msi].Entries, 2 * u);
									for (int j = u; j < 2 * u; j++) {
										Tables[msi].Entries[j].SoundIndex = -1;
										Tables[msi].Entries[j].Pitch = 1.0f;
										Tables[msi].Entries[j].Gain = 1.0f;
									}
								}
								string t = Lines[i] + ","; int m = 0;
								while (true) {
									int j = t.IndexOf(',');
									if (j == -1) break;
									string s = t.Substring(0, j).Trim();
									t = t.Substring(j + 1);
									double a;
									if (Interface.TryParseDoubleVb6(s, out a)) {
										switch (m) {
											case 0:
												Tables[msi].Entries[n].SoundIndex = (int)Math.Round(a);
												break;
											case 1:
												if (a < 0.0) a = 0.0;
												Tables[msi].Entries[n].Pitch = (float)(0.01 * a);
												break;
											case 2:
												if (a < 0.0) a = 0.0;
												Tables[msi].Entries[n].Gain = (float)Math.Pow((0.0078125 * a), 0.25);
												break;
										}
									} m++;
								} i++; n++;
							}
							Array.Resize<TrainManager.MotorSoundTableEntry>(ref Tables[msi].Entries, n);
							i--;
						} break;
				}
			}
			if (TrailerCars > 0 & TrailerCarMass <= 0.0) {
				Interface.AddMessage(Interface.MessageType.Error, false, "TrailerCarMass is expected to be positive in " + FileName);
				TrailerCarMass = 1.0;
			}
			if (Train.Specs.MaximumPowerNotch <= 0) Train.Specs.MaximumPowerNotch = 8;
			if (Train.Specs.MaximumBrakeNotch <= 0) Train.Specs.MaximumBrakeNotch = 8;
			// apply data
			Train.Specs.TotalMass = (double)MotorCars * MotorCarMass + (double)TrailerCars * TrailerCarMass;
			if (MotorCars < 1) MotorCars = 1;
			if (TrailerCars < 0) TrailerCars = 0;
			int Cars = MotorCars + TrailerCars;
			Train.Cars = new TrainManager.Car[Cars];
			double DistanceBetweenTheCars = 0.3;
			if (DriverCar < 0 | DriverCar >= Cars) {
				Interface.AddMessage(Interface.MessageType.Error, false, "DriverCar must point to an existing car in " + FileName);
				DriverCar = 0;
			}
			// brake system
			double OperatingPressure;
			if (BrakePipePressure <= 0.0) {
				if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					if (OperatingPressure > MainReservoirMinimumPressure) {
						OperatingPressure = MainReservoirMinimumPressure;
					}
				} else {
					if (BrakeCylinderEmergencyMaximumPressure < 480000.0 & MainReservoirMinimumPressure > 500000.0) {
						OperatingPressure = 490000.0;
					} else {
						OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					}
				}
			} else {
				OperatingPressure = BrakePipePressure;
			}
			// acceleration curves
			double MaximumAcceleration = 0.0;
			for (int i = 0; i < Math.Min(AccelerationCurves.Length, Train.Specs.MaximumPowerNotch); i++) {
				bool errors = false;
				if (AccelerationCurves[i].StageZeroAcceleration <= 0.0) {
					AccelerationCurves[i].StageZeroAcceleration = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneAcceleration <= 0.0) {
					AccelerationCurves[i].StageOneAcceleration = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneSpeed <= 0.0) {
					AccelerationCurves[i].StageOneSpeed = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageTwoSpeed <= 0.0) {
					AccelerationCurves[i].StageTwoSpeed = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageTwoExponent <= 0.0) {
					AccelerationCurves[i].StageTwoExponent = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneSpeed > AccelerationCurves[i].StageTwoSpeed) {
					double x = 0.5 * (AccelerationCurves[i].StageOneSpeed + AccelerationCurves[i].StageTwoSpeed);
					AccelerationCurves[i].StageOneSpeed = x;
					AccelerationCurves[i].StageTwoSpeed = x;
					errors = true;
				}
				if (errors) {
					Interface.AddMessage(Interface.MessageType.Error, false, "Entry " + (i + 1).ToString(Culture) + " in the #ACCELERATION section is missing or invalid in " + FileName);
				}
				if (AccelerationCurves[i].StageZeroAcceleration > MaximumAcceleration) {
					MaximumAcceleration = AccelerationCurves[i].StageZeroAcceleration;
				}
				if (AccelerationCurves[i].StageOneAcceleration > MaximumAcceleration) {
					MaximumAcceleration = AccelerationCurves[i].StageOneAcceleration;
				}
			}
			double MotorDeceleration = Math.Sqrt(MaximumAcceleration * BrakeDeceleration);
			// apply brake-specific attributes for all cars
			for (int i = 0; i < Cars; i++) {
				Train.Cars[i].Specs.BrakeType = BrakeType;
				Train.Cars[i].Specs.ElectropneumaticType = ElectropneumaticType;
				Train.Cars[i].Specs.BrakeControlSpeed = BrakeControlSpeed;
				Train.Cars[i].Specs.BrakeDecelerationAtServiceMaximumPressure = BrakeDeceleration;
				Train.Cars[i].Specs.MotorDeceleration = MotorDeceleration;
				Train.Cars[i].Specs.AirBrake.AirCompressorEnabled = false;
				Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure = MainReservoirMinimumPressure;
				Train.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure = MainReservoirMaximumPressure;
				Train.Cars[i].Specs.AirBrake.AirCompressorRate = 5000.0;
				Train.Cars[i].Specs.AirBrake.MainReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure + (Train.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure - Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure) * Program.RandomNumberGenerator.NextDouble();
				Train.Cars[i].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient = 0.01;
				Train.Cars[i].Specs.AirBrake.MainReservoirBrakePipeCoefficient = (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake ? 0.25 : 0.075) / Cars;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure = 0.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure = 1.005 * OperatingPressure;
				/*
				 * These values are all constants, and are expressed in pascals per second
				 * Presumably, Michelle intended for them to be editable in v2.0
				 */
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirServiceRate = 50000.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirEmergencyRate = 250000.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirChargeRate = 200000.0;
				Train.Cars[i].Specs.AirBrake.BrakePipeNormalPressure = OperatingPressure;
				Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake ? 0.0 : Train.Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
				Train.Cars[i].Specs.AirBrake.BrakePipeFlowSpeed = 100000000.0;
				Train.Cars[i].Specs.AirBrake.BrakePipeChargeRate = 10000000.0;
				Train.Cars[i].Specs.AirBrake.BrakePipeServiceRate = 1500000.0;
				Train.Cars[i].Specs.AirBrake.BrakePipeEmergencyRate = 5000000.0;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure = 0.975 * OperatingPressure;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirChargeRate = 200000.0;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient = 0.5;
				{
					double r = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure / BrakeCylinderEmergencyMaximumPressure - 1.0;
					if (r < 0.1) r = 0.1;
					if (r > 1.0) r = 1.0;
					Train.Cars[i].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient = r;
				}
				Train.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = BrakeCylinderEmergencyMaximumPressure;
				Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceMaximumPressure = BrakeCylinderServiceMaximumPressure;
				Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure = BrakeCylinderEmergencyMaximumPressure;
				if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderReleaseRate = BrakeCylinderDown;
				} else {
					Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceChargeRate = 0.3 * BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderReleaseRate = BrakeCylinderDown;
				}
				Train.Cars[i].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = BrakeCylinderEmergencyMaximumPressure;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = 0.0;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeReleaseRate = 200000.0;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeServiceRate = 300000.0;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeEmergencyRate = 400000.0;
			}
			if (Train.Specs.HasHoldBrake & Train.Specs.MaximumBrakeNotch > 1) {
				Train.Specs.MaximumBrakeNotch--;
			}
			// apply train attributes
			Train.Specs.CurrentReverser.Driver = 0;
			Train.Specs.CurrentReverser.Actual = 0;
			Train.Specs.CurrentPowerNotch.Driver = 0;
			Train.Specs.CurrentPowerNotch.Safety = 0;
			Train.Specs.CurrentPowerNotch.Actual = 0;
			Train.Specs.CurrentPowerNotch.DelayedChanges = new TrainManager.HandleChange[] { };
			Train.Specs.CurrentBrakeNotch.Driver = 0;
			Train.Specs.CurrentBrakeNotch.Safety = 0;
			Train.Specs.CurrentBrakeNotch.Actual = 0;
			Train.Specs.CurrentBrakeNotch.DelayedChanges = new TrainManager.HandleChange[] { };
			Train.Specs.CurrentEmergencyBrake.ApplicationTime = double.MaxValue;
			if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
				Train.Specs.SingleHandle = false;
				Train.Specs.HasHoldBrake = false;
			}
			// starting mode
			if (Game.TrainStart == Game.TrainStartMode.ServiceBrakesAts) {
				for (int i = 0; i < Cars; i++) {
					Train.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = Train.Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
					Train.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
					Train.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure;
				}
				Train.Specs.AirBrake.Handle.Driver = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Actual = TrainManager.AirBrakeHandleState.Service;
				int notch = (int)Math.Round(0.7 * Train.Specs.MaximumBrakeNotch);
				Train.Specs.CurrentBrakeNotch.Driver = notch;
				Train.Specs.CurrentBrakeNotch.Safety = notch;
				Train.Specs.CurrentBrakeNotch.Actual = notch;
				Train.Specs.CurrentEmergencyBrake.Driver = false;
				Train.Specs.CurrentEmergencyBrake.Safety = false;
				Train.Specs.CurrentEmergencyBrake.Actual = false;
				Train.Specs.CurrentReverser.Driver = 1;
				Train.Specs.CurrentReverser.Actual = 1;
			} else if (Game.TrainStart == Game.TrainStartMode.EmergencyBrakesAts) {
				for (int i = 0; i < Cars; i++) {
					Train.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					Train.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = 0.0;
					Train.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure = 0.0;
				}
				Train.Specs.AirBrake.Handle.Driver = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Actual = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.CurrentBrakeNotch.Driver = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentBrakeNotch.Safety = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentBrakeNotch.Actual = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentEmergencyBrake.Driver = true;
				Train.Specs.CurrentEmergencyBrake.Safety = true;
				Train.Specs.CurrentEmergencyBrake.Actual = true;
			} else {
				for (int i = 0; i < Cars; i++) {
					Train.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					Train.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = 0.0;
					Train.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure = 0.0;
				}
				Train.Specs.AirBrake.Handle.Driver = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.AirBrake.Handle.Actual = TrainManager.AirBrakeHandleState.Service;
				Train.Specs.CurrentBrakeNotch.Driver = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentBrakeNotch.Safety = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentBrakeNotch.Actual = Train.Specs.MaximumBrakeNotch;
				Train.Specs.CurrentEmergencyBrake.Driver = true;
				Train.Specs.CurrentEmergencyBrake.Safety = true;
				Train.Specs.CurrentEmergencyBrake.Actual = true;
			}
			// apply other attributes for all cars
			double AxleDistance = 0.4 * CarLength;
			for (int i = 0; i < Cars; i++) {
				Train.Cars[i].CarSections = new TrainManager.CarSection[] { };
				Train.Cars[i].FrontBogie.CarSections = new TrainManager.CarSection[] { };
				Train.Cars[i].RearBogie.CarSections = new TrainManager.CarSection[] { };
				Train.Cars[i].CurrentCarSection = -1;
				TrainManager.ChangeCarSection(Train, i, -1);
				TrainManager.ChangeFrontBogieSection(Train, i, -1);
				TrainManager.ChangeRearBogieSection(Train, i, -1);
				Train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? TrackManager.EventTriggerType.FrontCarFrontAxle : TrackManager.EventTriggerType.OtherCarFrontAxle;
				Train.Cars[i].RearAxle.Follower.TriggerType = i == Cars - 1 ? TrackManager.EventTriggerType.RearCarRearAxle : TrackManager.EventTriggerType.OtherCarRearAxle;
				Train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? TrackManager.EventTriggerType.TrainFront : TrackManager.EventTriggerType.None;
				Train.Cars[i].BeaconReceiverPosition = 0.5 * CarLength;
				Train.Cars[i].FrontAxle.Follower.CarIndex = i;
				Train.Cars[i].RearAxle.Follower.CarIndex = i;
				Train.Cars[i].FrontAxlePosition = AxleDistance;
				Train.Cars[i].RearAxlePosition = -AxleDistance;
				Train.Cars[i].Specs.IsMotorCar = false;
				Train.Cars[i].Specs.JerkPowerUp = JerkPowerUp;
				Train.Cars[i].Specs.JerkPowerDown = JerkPowerDown;
				Train.Cars[i].Specs.JerkBrakeUp = JerkBrakeUp;
				Train.Cars[i].Specs.JerkBrakeDown = JerkBrakeDown;
				Train.Cars[i].Specs.CoefficientOfStaticFriction = CoefficientOfStaticFriction;
				Train.Cars[i].Specs.CoefficientOfRollingResistance = CoefficientOfRollingResistance;
				Train.Cars[i].Specs.AerodynamicDragCoefficient = AerodynamicDragCoefficient;
				Train.Cars[i].Specs.ExposedFrontalArea = CarExposedFrontalArea;
				Train.Cars[i].Specs.UnexposedFrontalArea = CarUnexposedFrontalArea;
				Train.Cars[i].Specs.Doors = new TrainManager.Door[2];
				Train.Cars[i].Specs.Doors[0].Direction = -1;
				Train.Cars[i].Specs.Doors[0].State = 0.0;
				Train.Cars[i].Specs.Doors[1].Direction = 1;
				Train.Cars[i].Specs.Doors[1].State = 0.0;
				Train.Cars[i].Specs.DoorOpenFrequency = 0.0;
				Train.Cars[i].Specs.DoorCloseFrequency = 0.0;
				Train.Cars[i].Specs.CenterOfGravityHeight = CenterOfGravityHeight;
				Train.Cars[i].Specs.HoldBrake.UpdateInterval = 0.5;
				Train.Cars[i].Specs.ConstSpeed.UpdateInterval = 0.5;
				Train.Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput = double.PositiveInfinity;
				Train.Cars[i].Width = CarWidth;
				Train.Cars[i].Height = CarHeight;
				Train.Cars[i].Length = CarLength;
				Train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[i].Specs.CenterOfGravityHeight / Train.Cars[i].Width);
			}
			// assign motor cars
			if (MotorCars == 1) {
				if (FrontCarIsMotorCar | TrailerCars == 0) {
					Train.Cars[0].Specs.IsMotorCar = true;
				} else {
					Train.Cars[Cars - 1].Specs.IsMotorCar = true;
				}
			} else if (MotorCars == 2) {
				if (FrontCarIsMotorCar | TrailerCars == 0) {
					Train.Cars[0].Specs.IsMotorCar = true;
					Train.Cars[Cars - 1].Specs.IsMotorCar = true;
				} else if (TrailerCars == 1) {
					Train.Cars[1].Specs.IsMotorCar = true;
					Train.Cars[2].Specs.IsMotorCar = true;
				} else {
					int i = (int)Math.Ceiling(0.25 * (double)(Cars - 1));
					int j = (int)Math.Floor(0.75 * (double)(Cars - 1));
					Train.Cars[i].Specs.IsMotorCar = true;
					Train.Cars[j].Specs.IsMotorCar = true;
				}
			} else if (MotorCars > 0) {
				if (FrontCarIsMotorCar) {
					Train.Cars[0].Specs.IsMotorCar = true;
					double t = 1.0 + (double)TrailerCars / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 0.0;
					while (true) {
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						Train.Cars[i].Specs.IsMotorCar = true;
					}
				} else {
					Train.Cars[1].Specs.IsMotorCar = true;
					double t = 1.0 + (double)(TrailerCars - 1) / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 1.0;
					while (true) {
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						Train.Cars[i].Specs.IsMotorCar = true;
					}
				}
			}
			// assign motor/trailer-specific settings
			for (int i = 0; i < Cars; i++) {
				if (Train.Cars[i].Specs.IsMotorCar) {
					// motor car
					Train.Cars[i].Specs.AirBrake.Type = TrainManager.AirBrakeType.Main;
					Train.Cars[i].Specs.MassEmpty = MotorCarMass;
					Train.Cars[i].Specs.MassCurrent = MotorCarMass;
					Train.Cars[i].Specs.AccelerationCurves = AccelerationCurves;
					Train.Cars[i].Specs.AccelerationCurvesMultiplier = 1.0 + TrailerCars * TrailerCarMass / (MotorCars * MotorCarMass);
					Train.Cars[i].Specs.AccelerationCurveMaximum = MaximumAcceleration;
					switch (ReAdhesionDevice) {
						case 0: // type a:
							Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 1.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 0.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 1.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 8.0;
							break;
						case 1: // type b:
							Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 0.1;
							Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 0.9935;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 4.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 1.125;
							break;
						case 2: // type c:
							Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 0.1;
							Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 0.965;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 2.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 1.5;
							break;
						case 3: // type d:
							Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 0.05;
							Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 0.935;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 0.3;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 2.0;
							break;
						default: // no readhesion device
							Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 1.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 1.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 1.0;
							Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 99.0;
							break;
					}
					// motor sound
					Train.Cars[i].Sounds.Motor.SpeedConversionFactor = 18.0;
					Train.Cars[i].Sounds.Motor.Tables = new TrainManager.MotorSoundTable[4];
					for (int j = 0; j < 4; j++) {
						Train.Cars[i].Sounds.Motor.Tables[j].Entries = new TrainManager.MotorSoundTableEntry[Tables[j].Entries.Length];
						for (int k = 0; k < Tables[j].Entries.Length; k++) {
							Train.Cars[i].Sounds.Motor.Tables[j].Entries[k] = Tables[j].Entries[k];
						}
					}
				} else {
					// trailer car
					Train.Cars[i].Specs.AirBrake.Type = Train == TrainManager.PlayerTrain & i == Train.DriverCar | BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake ? TrainManager.AirBrakeType.Main : TrainManager.AirBrakeType.Auxillary;
					Train.Cars[i].Specs.MassEmpty = TrailerCarMass;
					Train.Cars[i].Specs.MassCurrent = TrailerCarMass;
					Train.Cars[i].Specs.AccelerationCurves = new TrainManager.AccelerationCurve[] { };
					Train.Cars[i].Specs.AccelerationCurvesMultiplier = 0.0;
					Train.Cars[i].Specs.AccelerationCurveMaximum = 0.0;
					Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 0.0;
					Train.Cars[i].Sounds.Motor.SpeedConversionFactor = 18.0;
					Train.Cars[i].Sounds.Motor.Tables = new TrainManager.MotorSoundTable[4];
					for (int j = 0; j < 4; j++) {
						Train.Cars[i].Sounds.Motor.Tables[j].Entries = new TrainManager.MotorSoundTableEntry[] { };
					}
				}
			}
			// driver
			Train.DriverCar = DriverCar;
			Train.Cars[Train.DriverCar].DriverX = DriverX;
			Train.Cars[Train.DriverCar].DriverY = DriverY;
			Train.Cars[Train.DriverCar].DriverZ = 0.5 * CarLength + DriverZ;
			// couplers
			Train.Couplers = new TrainManager.Coupler[Cars - 1];
			for (int i = 0; i < Train.Couplers.Length; i++) {
				Train.Couplers[i].MinimumDistanceBetweenCars = 0.9 * DistanceBetweenTheCars;
				Train.Couplers[i].MaximumDistanceBetweenCars = 1.1 * DistanceBetweenTheCars;
			}
			// finish
			Train.Station = -1;
			Train.RouteLimits = new double[] { double.PositiveInfinity };
			Train.CurrentRouteLimit = double.PositiveInfinity;
			Train.CurrentSectionLimit = double.PositiveInfinity;
		}

	}
}