//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Linq;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadSoundList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Sounds = new SoundDictionary();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.SoundListPath))
			{
				return;
			}

			string soundList = ParseData.SoundListPath;

			if (!File.Exists(soundList))
			{
				soundList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), soundList);

				if (!File.Exists(soundList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Sound List file " + soundList + " was not found.");
					return;
				}
			}

			string BaseDirectory = System.IO.Path.GetDirectoryName(soundList);

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(soundList);
			string[] Lines = File.ReadAllLines(soundList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			for (int i = 1; i < Lines.Length; i++)
			{
				//Cycle through the list of sounds
				//A sound index is formatted as follows:
				// --KEY USED BY ROUTEFILE-- , --PATH TO OBJECT RELATIVE TO SOUND FILE--

				Lines[i] = Lines[i].TrimBVE5Comments();
				if (string.IsNullOrEmpty(Lines[i]))
				{
					continue;
				}

				int a = Lines[i].IndexOf(',');
				string FilePath = Lines[i].Substring(a + 1, Lines[i].Length - a - 1).Trim();

				if (string.IsNullOrEmpty(FilePath) || a == -1)
				{
					// empty object name
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "No sound file was specified for key " + Lines[i]);
					continue;
				}

				string Key = Lines[i].Substring(0, a).Trim();
				try
				{
					FilePath = Path.CombineFile(BaseDirectory, FilePath);
				}
				catch
				{
					// ignore
				}

				if (!File.Exists(FilePath))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The Sound File " + FilePath + " with key " + Key + " was not found.");
					continue;
				}

				Plugin.CurrentHost.RegisterSound(FilePath, 15.0, out OpenBveApi.Sounds.SoundHandle handle);
				RouteData.Sounds.Add(Key, handle);
			}
		}

		private static void LoadSound3DList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Sound3Ds = new SoundDictionary();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.Sound3DListPath))
			{
				return;
			}

			string sound3DList = ParseData.Sound3DListPath;

			if (!File.Exists(sound3DList))
			{
				sound3DList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), sound3DList);

				if (!File.Exists(sound3DList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Sound3D List file " + sound3DList + " was not found.");
					return;
				}
			}

			string BaseDirectory = System.IO.Path.GetDirectoryName(sound3DList);

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(sound3DList);
			string[] Lines = File.ReadAllLines(sound3DList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			for (int i = 1; i < Lines.Length; i++)
			{
				//Cycle through the list of sounds
				//A sound index is formatted as follows:
				// --KEY USED BY ROUTEFILE-- , --PATH TO OBJECT RELATIVE TO SOUND FILE-- , --OPTIONAL RADIUS--

				Lines[i] = Lines[i].TrimBVE5Comments();
				if (string.IsNullOrEmpty(Lines[i]))
				{
					continue;
				}

				int a = Lines[i].IndexOf(',');
				if (a == -1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "No sound file was specified for key " + Lines[i]);
					continue;
				}
				string[] splitLine = Lines[i].Split(',');

				string Key = splitLine[0].Trim();
				string FilePath = splitLine[1].Trim();

				double soundRadius = 15;

				if (splitLine.Length > 2)
				{
					if (!NumberFormats.TryParseDoubleVb6(splitLine[2], out soundRadius))
					{
						soundRadius = 15;
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "An invalid sound radius was specified for key " + Lines[i]);
					}
					
				}
				if (string.IsNullOrEmpty(splitLine[1]))
				{
					// empty object name
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "No sound file was specified for key " + Lines[i]);
					continue;
				}

				

				
				try
				{
					FilePath = Path.CombineFile(BaseDirectory, FilePath);
				}
				catch
				{
					// ignore
				}

				if (!File.Exists(FilePath))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The Sound3D File " + FilePath + " with key " + Key + " was not found.");
					continue;
				}

				Plugin.CurrentHost.RegisterSound(FilePath, soundRadius, out OpenBveApi.Sounds.SoundHandle handle);
				RouteData.Sound3Ds.Add(Key, handle);
			}
		}
	}
}
