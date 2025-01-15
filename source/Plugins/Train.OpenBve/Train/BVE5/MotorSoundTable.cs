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
			SortedDictionary<double, BVE5MotorSoundTableEntry> motorSoundTable = new SortedDictionary<double, BVE5MotorSoundTableEntry>();
			SortedDictionary<double, BVE5MotorSoundTableEntry> brakeSoundTable = new SortedDictionary<double, BVE5MotorSoundTableEntry>();
			if (File.Exists(motorSoundPitch) && File.Exists(motorSoundGain))
			{
				ParsePitchTable(motorSoundPitch, ref motorSoundTable);
				ParseVolumeTable(motorSoundGain, ref motorSoundTable);
			}
			else
			{
				Plugin.CurrentHost.AddMessage("Missing BVE5 MotorSound table file.");
			}

			if (File.Exists(brakeSoundPitch) && File.Exists(brakeSoundGain))
			{
				ParsePitchTable(brakeSoundPitch, ref brakeSoundTable);
				ParseVolumeTable(brakeSoundGain, ref brakeSoundTable);
			}
			else
			{
				Plugin.CurrentHost.AddMessage("Missing BVE5 BrakeSound table file.");
			}
			
			
			Array.Resize(ref motorSound.MotorSoundTable, motorSoundTable.Count);
			for (int i = 0; i < motorSoundTable.Count; i++)
			{
				motorSound.MotorSoundTable[i] = motorSoundTable.ElementAt(i).Value;
			}
			Array.Resize(ref motorSound.BrakeSoundTable, brakeSoundTable.Count);
			for (int i = 0; i < brakeSoundTable.Count; i++)
			{
				motorSound.BrakeSoundTable[i] = brakeSoundTable.ElementAt(i).Value;
			}
			return motorSound;
		}

		private static void ParsePitchTable(string pitchFile, ref SortedDictionary<double, BVE5MotorSoundTableEntry> soundTable)
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

			for (int i = 0; i < setPoints.Count; i++)
			{
				double speed = setPoints.ElementAt(i).Key;
				if (soundTable.ContainsKey(speed))
				{
					BVE5MotorSoundTableEntry entryToUpdate = soundTable[speed];
					double[] pitchValues = setPoints.ElementAt(i).Value;
					if (pitchValues.Length > entryToUpdate.Sounds.Length)
					{
						Array.Resize(ref entryToUpdate.Sounds, pitchValues.Length);
					}
					for (int k = 0; k < pitchValues.Length; k++)
					{
						entryToUpdate.Sounds[k].Pitch = pitchValues[k];
					}
					soundTable[speed] = entryToUpdate;
				}
				else
				{
					BVE5MotorSoundTableEntry newEntry = new BVE5MotorSoundTableEntry(speed);
					double[] pitchValues = setPoints.ElementAt(i).Value;
					Array.Resize(ref newEntry.Sounds, pitchValues.Length);
					for (int k = 0; k < pitchValues.Length; k++)
					{
						newEntry.Sounds[k].Pitch = pitchValues[k];
					}
					soundTable.Add(speed, newEntry);
				}
			}
		}

		private static void ParseVolumeTable(string volumeFile, ref SortedDictionary<double, BVE5MotorSoundTableEntry> soundTable)
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

			for (int i = 0; i < setPoints.Count; i++)
			{
				double speed = setPoints.ElementAt(i).Key;
				if (soundTable.ContainsKey(speed))
				{
					BVE5MotorSoundTableEntry entryToUpdate = soundTable[speed];
					double[] gainValues = setPoints.ElementAt(i).Value;
					if (gainValues.Length > entryToUpdate.Sounds.Length)
					{
						Array.Resize(ref entryToUpdate.Sounds, gainValues.Length);
					}
					for (int k = 0; k < gainValues.Length; k++)
					{
						entryToUpdate.Sounds[k].Gain = gainValues[k];
					}
					soundTable[speed] = entryToUpdate;
				}
				else
				{
					BVE5MotorSoundTableEntry newEntry = new BVE5MotorSoundTableEntry(speed);
					double[] gainValues = setPoints.ElementAt(i).Value;
					Array.Resize(ref newEntry.Sounds, gainValues.Length);
					for (int k = 0; k < gainValues.Length; k++)
					{
						newEntry.Sounds[k].Gain = gainValues[k];
					}
					soundTable.Add(speed, newEntry);
				}
			}
		}
	}
}
