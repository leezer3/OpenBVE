using System;
using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using SoundManager;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal partial class SoundCfgParser
	{
		internal readonly Plugin Plugin;

		internal SoundCfgParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Parses the sound configuration file for a train</summary>
		/// <param name="Train">The train to which to apply the new sound configuration</param>
		internal void ParseSoundConfig(TrainBase Train)
		{
			LoadDefaultATSSounds(Train);
			string FileName = OpenBveApi.Path.CombineFile(Train.TrainFolder, "sound.xml");
			if (System.IO.File.Exists(FileName))
			{
				if (Plugin.SoundXmlParser.ParseTrain(FileName, Train))
				{
					Plugin.FileSystem.AppendToLogFile("Loading sound.xml file: " + FileName);
					return;
				}
			}
			FileName = OpenBveApi.Path.CombineFile(Train.TrainFolder, "sound.cfg");
			if (System.IO.File.Exists(FileName))
			{
				Plugin.FileSystem.AppendToLogFile("Loading sound.cfg file: " + FileName);
				Plugin.BVE4SoundParser.Parse(FileName, Train.TrainFolder, Train);
			}
			else
			{
				Plugin.FileSystem.AppendToLogFile("Loading default BVE2 sounds.");
				Plugin.BVE2SoundParser.Parse(Train);
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
								SoundHandle snd;
								Plugin.currentHost.RegisterSound(Files[i], Radius, out snd);
								Sounds[n] = new CarSound(snd, Position);
							}
							else
							{
								SoundHandle snd;
								Plugin.currentHost.RegisterSound(Files[i], Radius, out snd);
								Sounds.Add(n, new CarSound(snd, Position));
							}
						}
					}
				}
			}
			return Sounds;
		}
	}
}
