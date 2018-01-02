using System;
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
		private static int NumberOfMotorCars;
		private static int NumberOfTrailerCars;
		private static bool FrontCarIsMotorCar;
		internal static bool[] MotorCars;
		internal static int DriverCar = 0;
		private static System.Windows.Forms.Form mainForm;
		internal static void Process(Form form)
		{
			mainForm = form;
			if (!System.IO.File.Exists(FileName))
			{
				MessageBox.Show("The selected folder does not contain a valid train.dat \r\n Please retry.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			string[] Lines = System.IO.File.ReadAllLines(FileName);
			for (int i = 0; i < Lines.Length; i++)

			{
				int n = 0;
				switch (Lines[i].ToLowerInvariant())
				{
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							double a; if (NumberFormats.TryParseDoubleVb6(Lines[i], out a))
							{
								switch (n)
								{
									case 0: ConvertSoundCfg.DriverPosition.X = 0.001 * a; break;
									case 1: ConvertSoundCfg.DriverPosition.Y = 0.001 * a; break;
									case 2: ConvertSoundCfg.DriverPosition.Z = 0.001 * a; break;
									case 3: DriverCar = (int)Math.Round(a); break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							double a; if (NumberFormats.TryParseDoubleVb6(Lines[i], out a))
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
								}
							}
							i++; n++;
						}
						i--; break;

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
					int i = (int)Math.Ceiling(0.25 * (double)(NumberOfCars - 1));
					int j = (int)Math.Floor(0.75 * (double)(NumberOfCars - 1));
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
		}
	}
}
