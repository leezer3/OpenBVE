using System;
using System.Collections.Generic;
using OpenBveApi.Math;
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
		/// <param name="train">The train to which to apply the new sound configuration</param>
		internal void ParseSoundConfig(TrainBase train)
		{
			LoadDefaultATSSounds(train);
			string fileName = OpenBveApi.Path.CombineFile(train.TrainFolder, "sound.xml");
			if (System.IO.File.Exists(fileName))
			{
				if (Plugin.SoundXmlParser.ParseTrain(fileName, train))
				{
					Plugin.FileSystem.AppendToLogFile("Loading sound.xml file: " + fileName);
					return;
				}
			}
			fileName = OpenBveApi.Path.CombineFile(train.TrainFolder, "sound.cfg");
			if (System.IO.File.Exists(fileName))
			{
				Plugin.FileSystem.AppendToLogFile("Loading sound.cfg file: " + fileName);
				Plugin.BVE4SoundParser.Parse(fileName, train.TrainFolder, train);
			}
			else
			{
				Plugin.FileSystem.AppendToLogFile("Loading default BVE2 sounds.");
				Plugin.BVE2SoundParser.Parse(train);
			}
		}

		//Default sound radii
		internal const double largeRadius = 30.0;
		internal const double mediumRadius = 10.0;
		internal const double smallRadius = 5.0;
		internal const double tinyRadius = 2.0;

		/// <summary>Attempts to load an array of sound files into a car-sound dictionary</summary>
		/// <param name="folder">The folder the sound files are located in</param>
		/// <param name="fileStart">The first sound file</param>
		/// <param name="fileEnd">The last sound file</param>
		/// <param name="position">The position the sound is to be emitted from within the car</param>
		/// <param name="radius">The sound radius</param>
		/// <returns>The new car sound dictionary</returns>
		internal static Dictionary<int, CarSound> TryLoadSoundDictionary(string folder, string fileStart, string fileEnd, Vector3 position, double radius)
		{
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			Dictionary<int, CarSound> sounds = new Dictionary<int, CarSound>();
			if (!System.IO.Directory.Exists(folder))
			{
				//Detect whether the given folder exists before attempting to load from it
				return sounds;
			}
			string[] files = System.IO.Directory.GetFiles(folder);
			for (int i = 0; i < files.Length; i++)
			{
				string a = System.IO.Path.GetFileName(files[i]);
				if (a == null) return sounds;
				if (a.Length > fileStart.Length + fileEnd.Length)
				{
					if (a.StartsWith(fileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(fileEnd, StringComparison.OrdinalIgnoreCase))
					{
						string b = a.Substring(fileStart.Length, a.Length - fileEnd.Length - fileStart.Length);
						if (int.TryParse(b, System.Globalization.NumberStyles.Integer, culture, out var n))
						{
							if (sounds.ContainsKey(n))
							{
								Plugin.CurrentHost.RegisterSound(files[i], radius, out var snd);
								sounds[n] = new CarSound(snd, position);
							}
							else
							{
								Plugin.CurrentHost.RegisterSound(files[i], radius, out var snd);
								sounds.Add(n, new CarSound(snd, position));
							}
						}
					}
				}
			}
			return sounds;
		}
	}
}
