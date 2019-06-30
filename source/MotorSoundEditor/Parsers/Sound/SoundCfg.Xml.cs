using System;
using System.IO;
using System.Linq;
using System.Xml;
using MotorSoundEditor.Audio;
using MotorSoundEditor.Simulation.TrainManager;
using OpenBveApi.Math;

namespace MotorSoundEditor.Parsers.Sound
{
	internal static class SoundXmlParser
	{
		private static string currentPath;

		internal static bool ParseTrain(string fileName, TrainManager.Car car)
		{
			try
			{
				Parse(fileName, car);
			}
			catch
			{
				return false;
			}

			return true;
		}

		internal static void Parse(string fileName, TrainManager.Car car)
		{
			//3D center of the car
			Vector3 center = Vector3.Zero;

			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();

			//Load the marker's XML file 
			currentXML.Load(fileName);
			currentPath = Path.GetDirectoryName(fileName);

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CarSounds");

				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					//If we have no appropriate nodes specified, return false and fallback to loading the legacy Sound.cfg file
					throw new Exception("Empty sound.xml file");
				}

				foreach (XmlNode n in DocumentNodes)
				{
					if (n.ChildNodes.OfType<XmlElement>().Any())
					{
						foreach (XmlNode c in n.ChildNodes)
						{
							switch (c.Name.ToLowerInvariant())
							{
								case "motor":
									ParseMotorSoundTableNode(c, ref car.Sounds.Motor.Tables, center, SoundCfgParser.mediumRadius);
									break;
								case "run":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										break;
									}

									ParseArrayNode(c, out car.Sounds.Run, center, SoundCfgParser.mediumRadius);
									break;
							}
						}
					}
				}

				car.Sounds.RunVolume = new double[car.Sounds.Run.Length];
			}
		}

		/// <summary>Parses an XML motor table node</summary>
		/// <param name="node">The node</param>
		/// <param name="Tables">The motor sound tables to assign this node's contents to</param>
		/// <param name="Position">The default sound position</param>
		/// <param name="Radius">The default sound radius</param>
		private static void ParseMotorSoundTableNode(XmlNode node, ref TrainManager.MotorSoundTable[] Tables, Vector3 Position, double Radius)
		{
			foreach (XmlNode c in node.ChildNodes)
			{
				int idx;

				if (!NumberFormats.TryParseIntVb6(c.Name, out idx))
				{
					continue;
				}

				for (int i = 0; i < Tables.Length; i++)
				{
					Tables[i].Buffer = null;
					Tables[i].Source = null;

					for (int j = 0; j > Tables[i].Entries.Length; j++)
					{
						if (idx == Tables[i].Entries[j].SoundIndex)
						{
							ParseNode(c, out Tables[i].Entries[j].Buffer, ref Position, Radius);
						}
					}
				}
			}
		}

		/// <summary>Parses a single XML node into a sound buffer and position reference</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private static void ParseNode(XmlNode node, out Sounds.SoundBuffer Sound, ref Vector3 Position, double Radius)
		{
			string fileName = null;

			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "filename":
						try
						{
							fileName = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);

							if (!File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Sound = null;
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Sound = null;
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;

						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							z = 0.0;
						}

						Position = new Vector3(x, y, z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
						}
						break;
				}
			}

			if (fileName == null)
			{
				//No valid filename node specified
				Sound = null;
				return;
			}

			Sound = Program.Sounds.TryToLoad(fileName, Radius);
		}

		/// <summary>Parses a single XML node into a car sound</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sound">The car sound</param>
		/// <param name="Position">The default position of this sound (May be overriden by the node)</param>
		/// <param name="Radius">The default radius of this sound (May be overriden by the node)</param>
		private static void ParseNode(XmlNode node, out TrainManager.CarSound Sound, Vector3 Position, double Radius)
		{
			string fileName = null;

			foreach (XmlNode c in node.ChildNodes)
			{
				switch (c.Name.ToLowerInvariant())
				{
					case "filename":
						try
						{
							fileName = OpenBveApi.Path.CombineFile(currentPath, c.InnerText);

							if (!File.Exists(fileName))
							{
								//Valid path, but the file does not exist
								Sound = TrainManager.CarSound.Empty;
								return;
							}
						}
						catch
						{
							//Probably invalid filename characters
							Sound = TrainManager.CarSound.Empty;
							return;
						}
						break;
					case "position":
						string[] Arguments = c.InnerText.Split(',');
						double x = 0.0, y = 0.0, z = 0.0;

						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x))
						{
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y))
						{
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z))
						{
							z = 0.0;
						}

						Position = new Vector3(x, y, z);
						break;
					case "radius":
						if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
						{
						}
						break;

				}
			}

			if (fileName == null)
			{
				//No valid filename node specified
				Sound = TrainManager.CarSound.Empty;
				return;
			}

			Sound = new TrainManager.CarSound(fileName, Position, Radius);
		}

		/// <summary>Parses an XML node containing a list of sounds into a car sound array</summary>
		/// <param name="node">The node to parse</param>
		/// <param name="Sounds">The car sound array</param>
		/// <param name="Position">The default position of the sound (May be overriden by any node)</param>
		/// <param name="Radius">The default radius of the sound (May be overriden by any node)</param>
		private static void ParseArrayNode(XmlNode node, out TrainManager.CarSound[] Sounds, Vector3 Position, double Radius)
		{
			Sounds = new TrainManager.CarSound[0];

			foreach (XmlNode c in node.ChildNodes)
			{
				int idx = -1;
				if (c.Name.ToLowerInvariant() == "sound")
				{
					for (int i = 0; i < c.ChildNodes.Count; i++)
					{
						if (c.ChildNodes[i].Name.ToLowerInvariant() == "index")
						{
							if (!NumberFormats.TryParseIntVb6(c.ChildNodes[i].InnerText.ToLowerInvariant(), out idx))
							{
								return;
							}
							break;
						}
					}

					if (idx >= 0)
					{
						int l = Sounds.Length;
						Array.Resize(ref Sounds, idx + 1);

						while (l < Sounds.Length)
						{
							Sounds[l] = TrainManager.CarSound.Empty;
							l++;
						}

						ParseNode(c, out Sounds[idx], Position, Radius);
					}
				}
			}
		}
	}
}
