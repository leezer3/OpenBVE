using System;
using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class SoundCfgParser
	{
		/// <summary>Parses the sound configuration file for a train</summary>
		/// <param name="TrainPath">The absolute on-disk path to the train's folder</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Train">The train to which to apply the new sound configuration</param>
		internal static void ParseSoundConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train)
		{
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.cfg");
			if (System.IO.File.Exists(FileName))
			{
				LoadBve4Sounds(FileName, TrainPath, Encoding, Train);
			}
			else
			{
				LoadBve2Sounds(TrainPath, Train);
			}
		}

		/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
		/// <param name="train">The train</param>
		internal static void InitializeCarSounds(TrainManager.Train train)
		{
			// initialize
			for (int i = 0; i < train.Cars.Length; i++)
			{
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
				train.Cars[i].Sounds.Horns = new TrainManager.Horn[]
				{
					new TrainManager.Horn(),
					new TrainManager.Horn(),
					new TrainManager.Horn()
				};
				train.Cars[i].Sounds.Loop = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PilotLampOn = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
				train.Cars[i].Sounds.PointFrontAxle = new TrainManager.CarSound[] { };
				train.Cars[i].Sounds.PointRearAxle = new TrainManager.CarSound[] { };
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

		/// <summary>Loads a sound file into a sound buffer</summary>
		/// <param name="FileName">The sound to load</param>
		/// <param name="Radius">The sound radius</param>
		/// <returns>The new sound buffer</returns>
		private static Sounds.SoundBuffer TryLoadSoundBuffer(string FileName, double Radius)
		{
			if (FileName != null)
			{
				if (System.IO.File.Exists(FileName))
				{
					try
					{
						return Sounds.RegisterBuffer(FileName, Radius);
					}
					catch
					{
						return null;
					}
				}
			}
			return null;
		}

		/// <summary>Attempts to load a sound file into a car-sound</summary>
		/// <param name="FileName">The sound to load</param>
		/// <param name="Position">The position that the sound is emitted from within the car</param>
		/// <param name="Radius">The sound radius</param>
		/// <returns>The new car sound, or an empty car sound if load fails</returns>
		private static TrainManager.CarSound TryLoadSound(string FileName, Vector3 Position, double Radius)
		{
			TrainManager.CarSound s = TrainManager.CarSound.Empty;
			s.Position = Position;
			s.Source = null;
			if (FileName != null)
			{
				if (System.IO.File.Exists(FileName))
				{
					s.Buffer = Sounds.RegisterBuffer(FileName, Radius);
				}
			}
			return s;
		}

		/// <summary>Attempts to load an array of sound files into a car-sound array</summary>
		/// <param name="Folder">The folder the sound files are located in</param>
		/// <param name="FileStart">The first sound file</param>
		/// <param name="FileEnd">The last sound file</param>
		/// <param name="Position">The position the sound is to be emitted from within the car</param>
		/// <param name="Radius">The sound radius</param>
		/// <returns>The new car sound array</returns>
		private static TrainManager.CarSound[] TryLoadSoundArray(string Folder, string FileStart, string FileEnd, Vector3 Position, double Radius)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			TrainManager.CarSound[] Sounds = { };
			if (!System.IO.Directory.Exists(Folder))
			{
				//Detect whether the given folder exists before attempting to load from it
				return Sounds;
			}
			string[] Files = System.IO.Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++)
			{
				string a = System.IO.Path.GetFileName(Files[i]);
				if (a == null) return Sounds;
				if (a.Length > FileStart.Length + FileEnd.Length)
				{
					if (a.StartsWith(FileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(FileEnd, StringComparison.OrdinalIgnoreCase))
					{
						string b = a.Substring(FileStart.Length, a.Length - FileEnd.Length - FileStart.Length);
						int n; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, Culture, out n))
						{
							if (n >= 0)
							{
								int m = Sounds.Length;
								if (n >= m)
								{
									Array.Resize<TrainManager.CarSound>(ref Sounds, n + 1);
									for (int j = m; j < n; j++)
									{
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
