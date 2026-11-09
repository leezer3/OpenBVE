using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using TrainManager.Car;
using TrainManager.Motor;

namespace Train.OpenBve
{
	class Bve5MotorSoundTableParser
	{
		/// <summary>Parses a set of BVE5 motor sound tables</summary>
		/// <param name="car">The car</param>
		/// <param name="motorSoundPitch">The motor pitch table</param>
		/// <param name="motorSoundGain">The motor gain table</param>
		/// <param name="brakeSoundPitch">The brake pitch table</param>
		/// <param name="brakeSoundGain">The brake gain table</param>
		internal static BVE5MotorSound Parse(CarBase car, string motorSoundPitch, string motorSoundGain, string brakeSoundPitch, string brakeSoundGain)
		{
			BVE5MotorSound motorSound = new BVE5MotorSound(car);
			// temp working lists
			if (File.Exists(motorSoundPitch) && File.Exists(motorSoundGain))
			{
				ParsePitchTable(motorSoundPitch, ref motorSound.MotorPitchTable);
				ParseVolumeTable(motorSoundGain, ref motorSound.MotorGainTable);
			}
			else
			{
				Plugin.CurrentHost.AddMessage("Missing BVE5 MotorSound table file.");
			}

			if (File.Exists(brakeSoundPitch) && File.Exists(brakeSoundGain))
			{
				ParsePitchTable(brakeSoundPitch, ref motorSound.BrakePitchTable);
				ParseVolumeTable(brakeSoundGain, ref motorSound.BrakeGainTable);
			}
			else
			{
				Plugin.CurrentHost.AddMessage("Missing BVE5 BrakeSound table file.");
			}
			
			return motorSound;
		}

		private static void ParsePitchTable(string pitchFile, ref Tuple<double, double[]>[] pitchTable)
		{
			Encoding encoding = Text.DetermineBVE5FileEncoding(pitchFile);
			string[] Lines = File.ReadAllLines(pitchFile, encoding).TrimBVE5Comments();
			string[] format = Lines[0].Split(':');
			if (!format[0].StartsWith("BveTs Motor Noise Table", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Expected a BveTs Motor Noise Table, got: " + format[0]);
			}

			SortedDictionary<double, double[]> setPoints = new SortedDictionary<double, double[]>();
			int maxPitchValues = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				if (!string.IsNullOrEmpty(Lines[i]))
				{
					string[] splitString = Lines[i].TrimEnd(',', ' ').Split(',');
					if (!double.TryParse(splitString[0], out double speed))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid speed encountered at Line " + i + " in BveTs Motor Noise table " + pitchFile);
						continue;
					}

					if (splitString.Length > maxPitchValues)
					{
						maxPitchValues = splitString.Length;
					}

					double[] pitchValues = new double[maxPitchValues];
					for (int j = 1; j < splitString.Length; j++)
					{
						// default for unspecified pitch is 1.0 (original pitch)
						if (string.IsNullOrEmpty(splitString[j]))
						{
							pitchValues[j - 1] = 1;
						}
						if (!double.TryParse(splitString[j], out pitchValues[j - 1]))
						{
							pitchValues[j - 1] = 1;
							if (!string.IsNullOrEmpty(splitString[j]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid pitch encountered at Line " + i + " Index " + (j - 1) + " in BveTs Motor Noise table " + pitchFile);
							}
						}
					}
					setPoints.Add(speed, pitchValues);
				}
			}

			pitchTable = new Tuple<double, double[]>[setPoints.Count];
			for (int i = 0; i < setPoints.Count; i++)
			{
				pitchTable[i] = new Tuple<double, double[]>(setPoints.ElementAt(i).Key, setPoints.ElementAt(i).Value);
			}
		}

		private static void ParseVolumeTable(string volumeFile, ref Tuple<double, double[]>[] gainTable)
		{
			Encoding encoding = Text.DetermineBVE5FileEncoding(volumeFile);
			string[] Lines = File.ReadAllLines(volumeFile, encoding).TrimBVE5Comments();
			string[] format = Lines[0].Split(':');
			if (!format[0].StartsWith("BveTs Motor Noise Table", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Expected a BveTs Motor Noise Table, got: " + format[0]);
			}

			SortedDictionary<double, double[]> setPoints = new SortedDictionary<double, double[]>();

			int maxGainValues = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				if (!string.IsNullOrEmpty(Lines[i]))
				{
					string[] splitString = Lines[i].TrimEnd(',', ' ').Split(',');
					if (!double.TryParse(splitString[0], out double speed))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid speed encountered at Line " + i + " in BveTs Motor Noise table " + volumeFile);
						continue;
					}

					if (splitString.Length > maxGainValues)
					{
						maxGainValues = splitString.Length;
					}

					double[] gainValues = new double[maxGainValues];
					for (int j = 1; j < splitString.Length; j++)
					{
						// default for unspecified gain is 1.0 (original gain)
						if (string.IsNullOrEmpty(splitString[j]))
						{
							gainValues[j - 1] = 1;
						}

						if (!double.TryParse(splitString[j], out gainValues[j - 1]))
						{
							gainValues[j - 1] = 1;
							if(!string.IsNullOrEmpty(splitString[j]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid gain encountered at Line " + i + " Index " + (j - 1) + " in BveTs Motor Noise table " + volumeFile);
							}
						}
					}

					
					setPoints.Add(speed, gainValues);
				}
			}

			gainTable = new Tuple<double, double[]>[setPoints.Count];
			for (int i = 0; i < setPoints.Count; i++)
			{
				gainTable[i] = new Tuple<double, double[]>(setPoints.ElementAt(i).Key, setPoints.ElementAt(i).Value);
			}
		}
	}
}
