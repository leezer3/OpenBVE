using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Math;
using SoundManager;

namespace OpenBve
{
	internal static partial class SoundCfgParser
	{
		/// <summary>Parses the sound configuration file for a train</summary>
		/// <param name="TrainPath">The absolute on-disk path to the train's folder</param>
		/// <param name="Train">The train to which to apply the new sound configuration</param>
		internal static void ParseSoundConfig(string TrainPath, TrainManager.Train Train)
		{
			SoundCfgParser.LoadDefaultATSSounds(Train, TrainPath);
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.xml");
			if (System.IO.File.Exists(FileName))
			{
				if (SoundXmlParser.ParseTrain(FileName, Train))
				{
					Program.FileSystem.AppendToLogFile("Loading sound.xml file: " + FileName);
					return;
				}
			}
			FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.cfg");
			if (System.IO.File.Exists(FileName))
			{
				Program.FileSystem.AppendToLogFile("Loading sound.cfg file: " + FileName);
				BVE4SoundParser.Parse(FileName, TrainPath, Train);
			}
			else
			{
				Program.FileSystem.AppendToLogFile("Loading default BVE2 sounds.");
				BVE2SoundParser.Parse(TrainPath, Train);
			}
		}

		//Default sound radii
		internal const double largeRadius = 30.0;
		internal const double mediumRadius = 10.0;
		internal const double smallRadius = 5.0;
		internal const double tinyRadius = 2.0;

		/// <summary>Attempts to load an array of sound files into a car-sound dictionary</summary>
		/// <param name="Folder">The folder the sound files are located in</param>
		/// <param name="FileStart">The first sound file</param>
		/// <param name="FileEnd">The last sound file</param>
		/// <param name="Position">The position the sound is to be emitted from within the car</param>
		/// <param name="Radius">The sound radius</param>
		/// <returns>The new car sound dictionary</returns>
		internal static Dictionary<int, CarSound> TryLoadSoundDictionary(string Folder, string FileStart, string FileEnd, Vector3 Position, double Radius)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			Dictionary<int, CarSound> Sounds = new Dictionary<int, CarSound>();
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
						int n; 
						if (int.TryParse(b, System.Globalization.NumberStyles.Integer, Culture, out n))
						{
							if (Sounds.ContainsKey(n))
							{
								Sounds[n] = new CarSound(Program.Sounds.RegisterBuffer(Files[i], Radius), Position);
							}
							else
							{
								Sounds.Add(n, new CarSound(Program.Sounds.RegisterBuffer(Files[i], Radius), Position));
							}
						}
					}
				}
			}
			return Sounds;
		}
	}


}
