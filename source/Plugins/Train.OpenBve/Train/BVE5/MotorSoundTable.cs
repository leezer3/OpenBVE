using System;
using System.Collections.Generic;
using System.IO;
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
			ParsePitchTable(motorSoundPitch, ref motorSound.MotorSoundTable);
			ParseVolumeTable(motorSoundPitch, ref motorSound.MotorSoundTable);
			ParsePitchTable(motorSoundPitch, ref motorSound.BrakeSoundTable);
			ParseVolumeTable(motorSoundPitch, ref motorSound.BrakeSoundTable);
			return motorSound;
		}

		private static void ParsePitchTable(string pitchFile, ref BVE5MotorSoundTableEntry[] soundTable)
		{
			Encoding encoding = Text.DetermineBVE5FileEncoding(pitchFile);
			string[] Lines = File.ReadAllLines(pitchFile, encoding).TrimBVE5Comments();
			string[] format = Lines[0].Split(':');
			if (!format[0].StartsWith("BveTs Motor Noise Table", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Expected a BveTs Motor Noise Table, got: " + format[0]);
			}

			Dictionary<double, double[]> setPoints = new Dictionary<double, double[]>();
			int maxPitchValues = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				if (!string.IsNullOrEmpty(Lines[i]))
				{
					string[] splitString = Lines[i].Split(',');
					double speed;
					if (!double.TryParse(splitString[0], out speed))
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid speed encountered at Line " + i + " in BveTs Motor Noise table " + pitchFile);
						continue;
					}

					if (splitString.Length > maxPitchValues)
					{
						maxPitchValues = splitString.Length;
					}

					double[] pitchValues = new double[maxPitchValues];
					for (int j = 1; j < splitString.Length; j++)
					{
						if (string.IsNullOrEmpty(splitString[j]))
						{
							pitchValues[j - 1] = 0;
						}
						if (!double.TryParse(splitString[j], out pitchValues[j - 1]))
						{
							pitchValues[j - 1] = 0;
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid pitch encountered at Line " + i + " Index " + (j - 1) + " in BveTs Motor Noise table " + pitchFile);
						}
					}
					setPoints.Add(speed, pitchValues);
				}
			}
		}

		private static void ParseVolumeTable(string volumeFile, ref BVE5MotorSoundTableEntry[] soundTable)
		{
			Encoding encoding = Text.DetermineBVE5FileEncoding(volumeFile);
			string[] Lines = File.ReadAllLines(volumeFile, encoding).TrimBVE5Comments();
			string[] format = Lines[0].Split(':');
			if (!format[0].StartsWith("BveTs Motor Noise Table", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Expected a BveTs Motor Noise Table, got: " + format[0]);
			}

			Dictionary<double, double[]> setPoints = new Dictionary<double, double[]>();

			int maxGainValues = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				if (!string.IsNullOrEmpty(Lines[i]))
				{
					string[] splitString = Lines[i].Split(',');
					double speed;
					if (!double.TryParse(splitString[0], out speed))
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid speed encountered at Line " + i + " in BveTs Motor Noise table " + volumeFile);
						continue;
					}

					if (splitString.Length > maxGainValues)
					{
						maxGainValues = splitString.Length;
					}

					double[] gainValues = new double[maxGainValues];
					for (int j = 1; j < splitString.Length; j++)
					{
						if (string.IsNullOrEmpty(splitString[j]))
						{
							gainValues[j - 1] = 0;
						}

						if (!double.TryParse(splitString[j], out gainValues[j - 1]))
						{
							gainValues[j - 1] = 0;
							Plugin.currentHost.AddMessage(MessageType.Error, false, "Invalid gain encountered at Line " + i + " Index " + (j - 1) + " in BveTs Motor Noise table " + volumeFile);
						}
					}
					setPoints.Add(speed, gainValues);
				}
			}
		}
	}
}
