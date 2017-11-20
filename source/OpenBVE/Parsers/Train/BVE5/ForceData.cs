using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve
{
	using System.Collections.Generic;

	partial class Bve5TrainParser
	{
		internal static void ParseForceData(string UnloadedFile, string LoadedFile, ref TrainManager.Train Train, bool Power)
		{
			if (string.IsNullOrEmpty(UnloadedFile))
			{
				throw new Exception("The BVE5 Unloaded Force file is missing.");
			}
			string fileFormat = File.ReadLines(UnloadedFile).First();
			string[] splitFormat = fileFormat.Split(':');
			if (!string.Equals(splitFormat[0], "BveTs Vehicle Performance Table 2.00", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Invalid BVE5 vehicle performance table format: " + splitFormat[0]);
			}
			System.Text.Encoding e = Encoding.UTF8;
			if (splitFormat.Length >= 2)
			{
				/*
				 * Pull out the text encoding of our file
				 */
				e = TextEncoding.ParseEncoding(splitFormat[1]);
			}
			string[] Lines = File.ReadAllLines(UnloadedFile, e);
			TrainManager.Bve5AccelerationCurve[] AccelerationCurves;
			if (Power)
			{
				AccelerationCurves = new TrainManager.Bve5AccelerationCurve[Train.Specs.MaximumPowerNotch];
			}
			else
			{
				AccelerationCurves = new TrainManager.Bve5AccelerationCurve[Train.Specs.MaximumBrakeNotch];
			}
			
			for (int i = 0; i < AccelerationCurves.Length; i++)
			{
				AccelerationCurves[i] = new TrainManager.Bve5AccelerationCurve
				{
					UnloadedAcceleration = new TrainManager.Bve5AccelerationCurveEntry[0],
					LoadedAcceleration = new TrainManager.Bve5AccelerationCurveEntry[0]
				};
			}
			for (int i = 1; i < Lines.Length; i++)
			{
				string line = Lines[i];

				int hash = line.IndexOf('#');
				if (hash > 0)
				{
					line = line.Substring(0, hash).Trim();
				}
				else if (hash == 0)
				{
					continue;
				}
				else
				{
					line = line.Trim();
				}

				string[] splitLine = line.Split(',');
				if (splitLine.Length > 1)
				{
					double speed = 0.0;
					int k = 0;
					for (int j = 0; j < splitLine.Length; j++)
					{
						if (j == 0)
						{
							if (!double.TryParse(splitLine[j].Trim(), out speed))
							{
								Interface.AddMessage(Interface.MessageType.Warning, false, "Speed was invalid at line " + i  + " of BVE5 force file " + UnloadedFile);
								break;
							}
							continue;
						}
						int l = AccelerationCurves[j - 1].UnloadedAcceleration.Length;
						Array.Resize(ref AccelerationCurves[j - 1].UnloadedAcceleration, l + 1);
						AccelerationCurves[j - 1].UnloadedAcceleration[l] = new TrainManager.Bve5AccelerationCurveEntry(speed);
						double a;
						if (!double.TryParse(splitLine[j].Trim(), out a))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "The acceleration for Notch " + j + " was invalid at line " + i + " of BVE5 force file " + UnloadedFile);
						}
						else
						{
							AccelerationCurves[j - 1].UnloadedAcceleration[l].Acceleration = GetAccelerationFigure(Train, a);
						}
						k++;
					}
					if (AccelerationCurves.Length != k)
					{
						Array.Resize(ref AccelerationCurves, k);
					}
				}
			}

			if (string.IsNullOrEmpty(LoadedFile))
			{
				throw new Exception("The BVE5 Unloaded Force file is missing.");
			}
			fileFormat = File.ReadLines(LoadedFile).First();
			splitFormat = fileFormat.Split(':');
			if (!string.Equals(splitFormat[0], "BveTs Vehicle Performance Table 2.00", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Invalid BVE5 vehicle performance table format: " + splitFormat[0]);
			}
			e = Encoding.UTF8;
			if (splitFormat.Length >= 2)
			{
				/*
				 * Pull out the text encoding of our file
				 */
				e = TextEncoding.ParseEncoding(splitFormat[1]);
			}
			Lines = File.ReadAllLines(LoadedFile, e);
			for (int i = 1; i < Lines.Length; i++)
			{
				string line = Lines[i];

				int hash = line.IndexOf('#');
				if (hash >= 0)
				{
					line = line.Substring(0, hash).Trim();
				}
				else
				{
					line = line.Trim();
				}

				string[] splitLine = line.Split(',');
				if (splitLine.Length > 1)
				{
					double speed = 0.0;
					for (int j = 0; j < splitLine.Length; j++)
					{
						if (j == 0)
						{
							if (!double.TryParse(splitLine[j].Trim(), out speed))
							{
								Interface.AddMessage(Interface.MessageType.Warning, false, "Speed was invalid at line " + i + " of BVE5 force file " + LoadedFile);
								break;
							}
							continue;
						}
						int l = AccelerationCurves[j - 1].LoadedAcceleration.Length;
						Array.Resize(ref AccelerationCurves[j - 1].LoadedAcceleration, l + 1);
						AccelerationCurves[j - 1].LoadedAcceleration[l] = new TrainManager.Bve5AccelerationCurveEntry(speed);
						double a;
						if (!double.TryParse(splitLine[j].Trim(), out a))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "The loaded acceleration for Notch " + j + " was invalid at line " + i + " of BVE5 force file " + LoadedFile);
						}
						else
						{
							AccelerationCurves[j - 1].LoadedAcceleration[l].Acceleration = GetAccelerationFigure(Train, a);
						}
					}
				}
			}

			
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					if (Power)
					{
						Train.Cars[i].Specs.AccelerationCurves = AccelerationCurves;
					}
					else
					{
						Train.Cars[i].Specs.DecelerationCurves = AccelerationCurves;
					}
				}
		}

		static double GetAccelerationFigure(TrainManager.Train Train, double Number)
		{
			double MotorCars = 0;
			double TrailerCars = 0;
			double MotorCarMass = 0.0;
			double TrailerCarMass = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (Train.Cars[i].Specs.IsMotorCar)
				{
					MotorCars++;
					MotorCarMass = Train.Cars[i].Specs.MassCurrent / 1000.0;
				}
				else
				{
					TrailerCars++;
					TrailerCarMass = Train.Cars[i].Specs.MassCurrent / 1000.0;
				}
			}
			return (Number * (MotorCars * 3.6) / (MotorCarMass * 1.1 * MotorCars + TrailerCarMass * 1.05 * TrailerCars)) / 10000;
		}
	}
}
