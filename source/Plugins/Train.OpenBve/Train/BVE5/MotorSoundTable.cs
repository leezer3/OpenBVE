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
			List<BVE5MotorSoundTableEntry> motorSoundTable = new List<BVE5MotorSoundTableEntry>();
			List<BVE5MotorSoundTableEntry> brakeSoundTable = new List<BVE5MotorSoundTableEntry>();
			ParsePitchTable(motorSoundPitch, ref motorSoundTable);
			ParseVolumeTable(motorSoundPitch, ref motorSoundTable);
			ParsePitchTable(motorSoundPitch, ref brakeSoundTable);
			ParseVolumeTable(motorSoundPitch, ref brakeSoundTable);
			// convert to array once finished as this is faster in use
			motorSound.MotorSoundTable = motorSoundTable.ToArray();
			motorSound.BrakeSoundTable = brakeSoundTable.ToArray();
			return motorSound;
		}

		private static void ParsePitchTable(string pitchFile, ref List<BVE5MotorSoundTableEntry> soundTable)
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

			for (int i = 0; i < setPoints.Count; i++)
			{
				double speed = setPoints.ElementAt(i).Key;
				BVE5MotorSoundTableEntry previousEntry = new BVE5MotorSoundTableEntry();
				for (int j = 0; j < soundTable.Count; j++)
				{
					if (soundTable[j].Speed == speed)
					{
						BVE5MotorSoundTableEntry currentEntry = soundTable[j];
						double[] pitchValues = setPoints.ElementAt(i).Value;
						if (soundTable[j].Sounds.Length < pitchValues.Length)
						{
							Array.Resize(ref currentEntry.Sounds, pitchValues.Length);
						}

						for (int k = 0; k < pitchValues.Length; k++)
						{
							currentEntry.Sounds[k].Pitch = pitchValues[k];
						}
						soundTable[j] = currentEntry;
						break;
					}
					if (soundTable[j].Speed > speed)
					{
						BVE5MotorSoundTableEntry newEntry = previousEntry;
						double[] pitchValues = setPoints.ElementAt(i).Value;
						Array.Resize(ref newEntry.Sounds, pitchValues.Length);
						for (int k = 0; k < pitchValues.Length; k++)
						{
							newEntry.Sounds[k].Pitch = pitchValues[k];
						}
						soundTable.Insert(j -1, newEntry);
						break;
					}
					previousEntry = soundTable[j];
				}
			}
		}

		private static void ParseVolumeTable(string volumeFile, ref List<BVE5MotorSoundTableEntry> soundTable)
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

			for (int i = 0; i < setPoints.Count; i++)
			{
				double speed = setPoints.ElementAt(i).Key;
				BVE5MotorSoundTableEntry previousEntry = new BVE5MotorSoundTableEntry();
				for (int j = 0; j < soundTable.Count; j++)
				{
					if (soundTable[j].Speed == speed)
					{
						BVE5MotorSoundTableEntry currentEntry = soundTable[j];
						double[] gainValues = setPoints.ElementAt(i).Value;
						if (soundTable[j].Sounds.Length < gainValues.Length)
						{
							Array.Resize(ref currentEntry.Sounds, gainValues.Length);
						}

						for (int k = 0; k < gainValues.Length; k++)
						{
							currentEntry.Sounds[k].Pitch = gainValues[k];
						}
						soundTable[j] = currentEntry;
						break;
					}
					if (soundTable[j].Speed > speed)
					{
						BVE5MotorSoundTableEntry newEntry = previousEntry;
						double[] gainValues = setPoints.ElementAt(i).Value;
						Array.Resize(ref newEntry.Sounds, gainValues.Length);
						for (int k = 0; k < gainValues.Length; k++)
						{
							newEntry.Sounds[k].Pitch = gainValues[k];
						}
						soundTable.Insert(j -1, newEntry);
						break;
					}
					previousEntry = soundTable[j];
				}
			}
		}
	}
}
