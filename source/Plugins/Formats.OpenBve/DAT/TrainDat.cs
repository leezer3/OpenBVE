using OpenBveApi.Hosts;
using OpenBveApi.Math;
using System;
using System.Collections.Generic;
using System.IO;

namespace Formats.OpenBve
{
	/// <summary>Root block for a .CFG type file</summary>
	public class TrainDatFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		/// <summary>The format of the current file</summary>
		public readonly TrainDatFormats Format;

		public readonly int Version;
		public TrainDatFile(string myFile, HostInterface currentHost) : base(-1, default, myFile, currentHost)
		{
			string[] lines = File.ReadAllLines(myFile);
			List<string> blockLines = new List<string>();
			bool addToBlock = false;
			T1 previousSection = default(T1);
			bool formatFound = false;
			int startingLine = 0;
			Version = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				int semiColon = lines[i].IndexOf(';');
				if (semiColon != -1)
				{
					lines[i] = lines[i].Substring(0, semiColon);
				}
				lines[i] = lines[i].Trim();
				if (lines[i].Length > 0 && !formatFound)
				{
					formatFound = true;
					Format = ParseFormat(lines[i], out Version);
				}

				
				if (lines[i].Length > 0 && lines[i][0] == '#')
				{
					if (!Enum.TryParse(lines[i].Substring(1), true, out T1 currentSection))
					{
						addToBlock = false;
					}
					else
					{
						addToBlock = true;
					}

					if (blockLines.Count > 0)
					{
						subBlocks.Add(new TrainDatSection<T1, T2>(blockLines, previousSection, startingLine + 1, myFile, currentHost));
						blockLines.Clear();
					}

					startingLine = i;
					previousSection = currentSection;
				}
				else
				{
					if (addToBlock && !string.IsNullOrEmpty(lines[i]))
					{
						blockLines.Add(lines[i]);
					}
				}
			}

			if (blockLines.Count > 0)
			{
				subBlocks.Add(new TrainDatSection<T1, T2>(blockLines, previousSection, startingLine, myFile, currentHost));
			}
		}

		public class TrainDatSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
		{
			public TrainDatSection(List<string> blockLines, T1 myKey, int startingLine, string myFile, HostInterface currentHost) : base(-1, myKey, myFile, currentHost)
			{
				for (int i = 0; i < blockLines.Count; i++)
				{
					rawValues.Enqueue(new KeyValuePair<int, string>(startingLine + i, blockLines[i]));
				}
			}
		}

		/// <summary>Parse the format of the specified train.dat</summary>
			/// <param name="line">The version line of the train.dat</param>
			/// <param name="version">The version of the specified OpenBVE train.dat</param>
			private static TrainDatFormats ParseFormat(string line, out int version)
		{
			version = -1;
				switch (line.ToLowerInvariant())
				{
					case "bve1200000":
						return TrainDatFormats.BVE1200000;
					case "bve1210000":
						return TrainDatFormats.BVE1210000;
					case "bve1220000":
						return TrainDatFormats.BVE1220000;
					case "bve2000000":
						return TrainDatFormats.BVE2000000;
					case "bve2060000":
						return TrainDatFormats.BVE2060000;
					case "openbve":
						version = 0;
						return TrainDatFormats.openBVE;
					case "#acceleration":
					case "#deceleration":
					case "#delay":
					case "#move":
					case "#brake":
					case "#pressure":
					case "#handle":
					case "#cab":
					case "#car":
					case "#device":
					case "motor_p1":
					case "motor_p2":
					case "brake_p1":
					case "brake_p2":
						version = 0;
						return TrainDatFormats.MissingHeader;
					default:
						if (line.ToLowerInvariant().StartsWith("openbve"))
						{
							string tt = line.Substring(7, line.Length - 7).Trim();
							if (!NumberFormats.TryParseIntVb6(tt, out version))
							{
								version = -1;
							}
							return TrainDatFormats.openBVE;
						}

						return line.ToLowerInvariant().StartsWith("bve") ? TrainDatFormats.UnknownBVE : TrainDatFormats.Unsupported;
				}
		}
	}
}
