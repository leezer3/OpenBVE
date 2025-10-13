using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.Math;

namespace CarXmlConvertor
{
	class ConvertTrainDat
	{
		internal static string FileName;
		internal static int NumberOfCars;
		internal static double CarLength = 20.0;
		internal static double CarWidth = 2.6;
		internal static double CarHeight = 3.6;
		internal static double MotorCarMass = 1.0;
		internal static double TrailerCarMass = 1.0;
		internal static double BrakeCylinderServiceMaximumPressure = 440000.0;
		internal static double BrakeCylinderEmergencyMaximumPressure = 440000.0;
		internal static double BrakeCylinderEmergencyRate = 300000.0;
		internal static double BrakeCylinderReleaseRate = 200000.0;
		internal static double MainReservoirMinimumPressure = 690000.0;
		internal static double MainReservoirMaximumPressure = 780000.0;
		internal static double BrakePipePressure = 0.0;
		private static int NumberOfMotorCars;
		private static int NumberOfTrailerCars;
		private static bool FrontCarIsMotorCar;
		internal static bool[] MotorCars;
		internal static int DriverCar = 0;
		internal static int BrakeType = 0;
		internal static int ReadhesionDeviceType = 0;
		internal static double DoorWidth = 1000.0;
		internal static double DoorTolerance = 0.0;
		internal static int PowerNotches = 0;
		internal static int BrakeNotches = 0;
		internal static double CenterOfGravityHeight = 1.6;
		internal static double ExposedFrontalArea = -1;
		internal static double UnexposedFrontalArea = -1;
		internal static Vector3 DriverPosition = Vector3.Zero;
		private static MainForm mainForm;
		internal static List<AccelerationCurve> AccelerationCurves = new List<AccelerationCurve>();

		internal static void Process(MainForm form)
		{
			mainForm = form;

			if (!System.IO.File.Exists(FileName))
			{
				MessageBox.Show("The selected folder does not contain a valid train.dat \r\n Please retry.", @"CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
				mainForm.terminateEarly = true;
				return;
			}
			string[] Lines = System.IO.File.ReadAllLines(FileName);

			int currentVersion = ParseFormat(Lines);

			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}
				if (Lines[i].EndsWith(","))
				{
					//File edited with MSExcel may have additional commas at the end of a line
					Lines[i] = Lines[i].TrimEnd(',');
				}
			}

			for (int i = 0; i < Lines.Length; i++)
			{
				int n = 0;
				switch (Lines[i].ToLowerInvariant())
				{
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a))
							{
								switch (n)
								{
									case 0:
										DriverPosition.X = 0.001 * a;
										break;
									case 1:
										DriverPosition.Y = 0.001 * a;
										break;
									case 2:
										DriverPosition.Z = 0.001 * a;
										break;
									case 3:
										DriverCar = (int)Math.Round(a);
										break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a))
							{
								switch (n)
								{
									case 0:
										if (!(a <= 0.0))
										{
											MotorCarMass = a * 1000.0;
										}
										break;
									case 1:
										if (!(a <= 0.0))
										{
											NumberOfMotorCars = (int)Math.Round(a);
										}
										break;
									case 2:
										if (!(a <= 0.0))
										{
											TrailerCarMass = a * 1000.0;
										}
										break;
									case 3:
										if (!(a <= 0.0))
										{
											NumberOfTrailerCars = (int)Math.Round(a);
										}
										break;
									case 4:
										if (!(a <= 0.0))
										{
											CarLength = a;
										}
										break;
									case 5:
										FrontCarIsMotorCar = a == 1.0;
										break;
									case 6:
										if (!(a <= 0.0))
										{
											CarWidth = a;
										}
										break;
									case 7:
										if (!(a <= 0.0))
										{
											CarHeight = a;
										}
										break;
									case 8:
										CenterOfGravityHeight = a;
										break;
									case 9:
										if (a <= 0)
										{
											ExposedFrontalArea = a;
										}
										break;
									case 10:
										if (a <= 0)
										{
											UnexposedFrontalArea = a;
										}
										break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#brake":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseIntVb6(Lines[i], out int a))
							{
								switch (n)
								{
									case 0: BrakeType = a; break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#move":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a))
							{
								switch (n)
								{
									case 4: BrakeCylinderEmergencyRate = a * 1000.0; break;
									case 5: BrakeCylinderReleaseRate = a * 1000.0; break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#pressure":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal)) {
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a)) {
								switch (n) {
									case 0:
										if (a <= 0.0)
										{
											mainForm.updateLogBoxText += "BrakeCylinderServiceMaximumPressure is expected to be positive at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in " + FileName;
										} else {
											BrakeCylinderServiceMaximumPressure = a * 1000.0;
										} break;
									case 1:
										if (a <= 0.0) {
											mainForm.updateLogBoxText += "BrakeCylinderEmergencyMaximumPressure is expected to be positive at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in " + FileName;
										} else {
											BrakeCylinderEmergencyMaximumPressure = a * 1000.0;
										} break;
									case 2:
										if (a <= 0.0) {
											mainForm.updateLogBoxText += "MainReservoirMinimumPressure is expected to be positive at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in " + FileName;
										} else {
											MainReservoirMinimumPressure = a * 1000.0;
										} break;
									case 3:
										if (a <= 0.0) {
											mainForm.updateLogBoxText += "MainReservoirMaximumPressure is expected to be positive at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in " + FileName;
										} else {
											MainReservoirMaximumPressure = a * 1000.0;
										} break;
									case 4:
										if (a <= 0.0) {
											mainForm.updateLogBoxText += "BrakePipePressue is expected to be positive at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in " + FileName;
										} else {
											BrakePipePressure = a * 1000.0;
										} break;
								}
							} i++; n++;
						} i--; break;
					case "#device":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a))
							{
								switch (n)
								{
									case 5: ReadhesionDeviceType = (int) a; break;
									case 10: DoorWidth = a; break;
									case 11: DoorTolerance = a; break;
								}
							}
							i++; n++;
						}
						i--; 
						break;
					case "#acceleration":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (string.IsNullOrEmpty(Lines[i]))
							{
								i++;
								continue;
							}
							AccelerationCurve curve = new AccelerationCurve();
							string t = Lines[i] + ",";
							int m = 0;
							while (true) {
								int j = t.IndexOf(',');
								if (j == -1) break;
								string s = t.Substring(0, j).Trim();
								t = t.Substring(j + 1);
								if (NumberFormats.TryParseDoubleVb6(s, out double a)) {
									switch (m) {
										case 0:
											if (a <= 0.0) {
												
											} else {
												curve.StageZeroAcceleration = a;
											} break;
										case 1:
											if (a <= 0.0) {
											} else {
												curve.StageOneAcceleration = a;
											} break;
										case 2:
											if (a <= 0.0) {
											} else {
												curve.StageOneSpeed = a;
											} break;
										case 3:
											if (a <= 0.0) {
											} else {
												curve.StageTwoSpeed = a;
												if (curve.StageTwoSpeed < curve.StageOneSpeed) {
													curve.StageTwoSpeed = curve.StageOneSpeed;
												}
											} break;
										case 4:
											{
												if (currentVersion < 2000000) {
													if (a <= 0.0) {
														curve.StageTwoExponent = 1.0;
													} else {
														const double c = 4.439346232277577;
														curve.StageTwoExponent = 1.0 - Math.Log(a) * curve.StageTwoSpeed * c;
														if (curve.StageTwoExponent <= 0.0) {
															curve.StageTwoExponent = 1.0;
														} else if (curve.StageTwoExponent > 4.0) {
															curve.StageTwoExponent = 4.0;
														}
													}
												} else {
													curve.StageTwoExponent = a;
													if (curve.StageTwoExponent <= 0.0) {
														curve.StageTwoExponent = 1.0;
													}
												}
											} break;
									}
								} m++;
							} i++; n++;
							AccelerationCurves.Add(curve);
						} i--; break;
					case "#handle":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							if (NumberFormats.TryParseDoubleVb6(Lines[i], out double a))
							{
								switch (n)
								{
									case 1:
										if (a > 0)
										{
											PowerNotches = (int)a;
										}
										break;
									case 2:
										if (a > 0)
										{
											BrakeNotches = (int)a;
										}
										break;
								}
							}
							i++; n++;
						}
						i--; 
						break;
					default:
					{
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							i++; n++;
						}
						i--;
					}
						break;
				}
			}

			if (BrakePipePressure <= 0.0)
			{

				if (BrakeType == 2) //Automatic air brake
				{
					BrakePipePressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					if (BrakePipePressure > MainReservoirMinimumPressure)
					{
						BrakePipePressure = MainReservoirMinimumPressure;
					}
				}
				else
				{
					if (BrakeCylinderEmergencyMaximumPressure < 480000.0 & MainReservoirMinimumPressure > 500000.0)
					{
						BrakePipePressure = 490000.0;
					}
					else
					{
						BrakePipePressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					}
				}
			}

			NumberOfCars = NumberOfMotorCars + NumberOfTrailerCars;
			MotorCars = new bool[NumberOfCars];
			if (NumberOfMotorCars == 1)
			{
				if (FrontCarIsMotorCar | NumberOfTrailerCars == 0)
				{
					MotorCars[0] = true;
				}
				else
				{
					MotorCars[NumberOfCars - 1] = true;
				}
			}
			else if (NumberOfMotorCars == 2)
			{
				if (FrontCarIsMotorCar | NumberOfTrailerCars == 0)
				{
					MotorCars[0] = true;
					MotorCars[NumberOfCars - 1] = true;
				}
				else if (NumberOfTrailerCars == 1)
				{
					MotorCars[1] = true;
					MotorCars[2] = true;
				}
				else
				{
					int i = (int)Math.Ceiling(0.25 * (NumberOfCars - 1));
					int j = (int)Math.Floor(0.75 * (NumberOfCars - 1));
					MotorCars[i] = true;
					MotorCars[j] = true;
				}
			}
			else if (NumberOfMotorCars > 0)
			{
				if (FrontCarIsMotorCar)
				{
					MotorCars[0] = true;
					double t = 1.0 + (double)NumberOfTrailerCars / (double)(NumberOfMotorCars - 1);
					double r = 0.0;
					double x = 0.0;
					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= NumberOfCars) break;
						MotorCars[i] = true;
					}
				}
				else
				{
					MotorCars[1] = true;
					double t = 1.0 + (double)(NumberOfTrailerCars - 1) / (double)(NumberOfMotorCars - 1);
					double r = 0.0;
					double x = 1.0;
					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= NumberOfCars) break;
						MotorCars[i] = true;
					}
				}
			}
			ConvertSoundCfg.DriverPosition = DriverPosition;
			ConvertSoundCfg.DriverPosition.Z = 0.5 * CarLength + ConvertSoundCfg.DriverPosition.Z;
			for (int i = 0; i < AccelerationCurves.Count; i++)
			{
				AccelerationCurves[i].Multiplier = 1.0 + NumberOfTrailerCars * TrailerCarMass / (NumberOfMotorCars * MotorCarMass);
			}
		}

		private static int ParseFormat(string[] lines)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Length <= 0)
				{
					continue;
				}

				string t = lines[i].ToLowerInvariant();
				switch (t)
				{
					case "bve1200000":
						return 1200000;
					case "bve1210000":
						return 1210000;
					case "bve1220000":
						return 1220000;
					case "bve2000000":
						return 2000000;
					case "bve2060000":
						return 2060000;
				}
			}
			return 2000000;
		}

		internal class AccelerationCurve
		{
			internal double StageZeroAcceleration;
			internal double StageOneAcceleration;
			internal double StageOneSpeed;
			internal double StageTwoSpeed;
			internal double StageTwoExponent;
			internal double Multiplier = 1.0;
		}
	}
}
