using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve
{
	partial class Bve5TrainParser
	{
		private struct Bve5TrainData
		{
			internal string ParametersFile;
			internal string PanelFile;
			internal string SoundConfigFile;
			internal string ATSPlugin;
			internal string MotorSoundFile;
			internal PerformanceData PowerData;
			internal PerformanceData BrakeData;
		}

		private struct PerformanceData
		{
			internal string ParametersFile;
			internal string ForceFile;
			internal string MaxForceFile;
			internal string CurrentFile;
			internal string MaxCurrentFile;
			internal string NoLoadCurrentFile;
		}

		static Bve5TrainData TrainData = new Bve5TrainData();

		internal static void ParseTrainData(string FileName, TrainManager.Train Train)
		{
			
			string fileFolder = System.IO.Path.GetDirectoryName(FileName);
			string fileFormat = File.ReadLines(FileName).First();
			string[] splitFormat = fileFormat.Split(':');
			if (!string.Equals(splitFormat[0], "BveTs Vehicle 1.00", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Invalid BVE5 vehicle format: " + splitFormat[0]);
			}
			System.Text.Encoding e = Encoding.UTF8;
			if (splitFormat.Length >= 2)
			{
				/*
				 * Pull out the text encoding of our file
				 */
				e = TextEncoding.ParseEncoding(splitFormat[1]);
			}
			string[] Lines = File.ReadAllLines(FileName, e);
			for (int i = 1; i < Lines.Length; i++)
			{
				int semicolon = Lines[i].IndexOf(';');
				if (semicolon >= 0)
				{
					Lines[i] = Lines[i].Substring(0, semicolon).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}
				string[] splitLine = Lines[i].Split('=');
				if (splitLine.Length != 2)
				{
					//Hacky, but deal with this better later
					continue;
				}
				string f;
				switch (splitLine[0].ToLowerInvariant().Trim())
				{
					case "performancecurve":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						string folder = System.IO.Path.GetDirectoryName(f);
						if (File.Exists(f))
						{
							//The performance curve file doesn't allow specification of the encoding
							//Attempt to get it from the file
							Encoding fe = TextEncoding.GetSystemEncodingFromFile(f);
							string[] performanceLines = File.ReadAllLines(f, fe);
							string section = string.Empty;
							for (int j = 0; j < performanceLines.Length; j++)
							{
								string line = performanceLines[j];
								semicolon = line.IndexOf(';');
								if (semicolon >= 0)
								{
									line = line.Substring(0, semicolon).Trim();
								}
								else
								{
									line = line.Trim();
								}
								if (line.Length > 0 && line[0] == '[' & line[line.Length - 1] == ']')
								{
									section = line.Substring(1, line.Length - 2).ToLowerInvariant();
								}
								else
								{
									int equals = line.IndexOf('=');
									if (equals >= 0)
									{
										string key = line.Substring(0, @equals).Trim().ToLowerInvariant();
										string value = line.Substring(@equals + 1).Trim();
										switch (key)
										{
											case "params":
												if (section == "power")
												{
													TrainData.PowerData.ParametersFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.ParametersFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
											case "force":
												if (section == "power")
												{
													TrainData.PowerData.ForceFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.ForceFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
											case "maxforce":
												if (section == "power")
												{
													TrainData.PowerData.MaxForceFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.MaxForceFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
											case "current":
												if (section == "power")
												{
													TrainData.PowerData.CurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.CurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
											case "maxcurrent":
												if (section == "power")
												{
													TrainData.PowerData.MaxCurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.MaxCurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
											case "noloadcurrent":
												if (section == "power")
												{
													TrainData.PowerData.NoLoadCurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												else
												{
													TrainData.BrakeData.NoLoadCurrentFile = OpenBveApi.Path.CombineFile(folder, value);
												}
												break;
										}
									}
								}
							}
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "Performance curve table file " + f + " was not found.");
						}
						break;
					case "parameters":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						if (File.Exists(f))
						{
							TrainData.ParametersFile = f;
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "Parameters file " + f + " was not found.");
						}
						break;
					case "panel":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						if (File.Exists(f))
						{
							TrainData.PanelFile = f;
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "Panel file " + f + " was not found.");
						}
						break;
					case "sound":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						if (File.Exists(f))
						{
							TrainData.SoundConfigFile= f;
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "Sound configuration file " + f + " was not found.");
						}
						break;
					case "ats":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						if (File.Exists(f))
						{
							TrainData.ATSPlugin = f;
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "ATS Plugin DLL " + f + " was not found.");
						}
						break;
					case "motornoise":
						f = OpenBveApi.Path.CombineFile(fileFolder, splitLine[1].Trim());
						if (File.Exists(f))
						{
							TrainData.MotorSoundFile = f;
						}
						else
						{
							Interface.AddMessage(Interface.MessageType.Error, true, "Motor sound file " + f + " was not found.");
						}
						break;
				}
			}
			/*
			 * We have now collected the component files for our train, so load them in order
			 */
			ParseTrainParameters(TrainData.ParametersFile, ref Train);
			ParseNotchParameters(ref Train, true);
			ParseNotchParameters(ref Train, false);
			ParseForceData(TrainData.PowerData.ForceFile, TrainData.PowerData.MaxForceFile, ref Train, true);
			ParseForceData(TrainData.BrakeData.ForceFile, TrainData.BrakeData.MaxForceFile, ref Train, false);
			ParseAmmeter(ref Train);
			Train.InitializeCarSounds();
			BVE5SoundParser.Parse(TrainData.SoundConfigFile, Encoding.UTF8, Train);
			PanelTxtParser.ParsePanelTxt(TrainData.PanelFile, Encoding.UTF8, Train);
			Train.Cars[Train.DriverCar].HasInteriorView = true;
			Train.Cars[Train.DriverCar].CameraRestrictionMode = World.CameraRestrictionMode.On;
			World.CameraRestriction = World.CameraRestrictionMode.On;
			Train.Specs.CurrentAverageAcceleration = 0.0;
		}
	}
}
