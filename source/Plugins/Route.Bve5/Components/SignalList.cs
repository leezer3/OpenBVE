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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadSignalList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.SignalObjects = new List<SignalData>();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.SignalListPath))
			{
				return;
			}

			string signalList = ParseData.SignalListPath;

			if (!File.Exists(signalList))
			{
				signalList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), signalList);

				if (!File.Exists(signalList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Signal List file " + signalList + " was not found.");
					return;
				}
			}

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(signalList);
			string[] Lines = File.ReadAllLines(signalList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			for (int currentLine = 1; currentLine < Lines.Length; currentLine++)
			{

				Lines[currentLine] = Lines[currentLine].TrimBVE5Comments();
				if (string.IsNullOrEmpty(Lines[currentLine]))
				{
					continue;
				}

				string[] splitLine = Lines[currentLine].Split(',');
				string Key = string.Empty;
				List<int> Numbers = new List<int>();
				List<string> ObjectKeys = new List<string>();

				for (int i = 0; i < splitLine.Length; i++)
				{
					switch (i)
					{
						case 0:
							Key = splitLine[i];
							break;
						default:
							if (!string.IsNullOrEmpty(splitLine[i]))
							{
								Numbers.Add(i - 1);
								ObjectKeys.Add(splitLine[i]);
							}

							break;
					}
				}

				if (string.IsNullOrEmpty(Key) || Key[0] == ';')
				{
					// Commented line
					continue;
				}

				List<StaticObject> Objects = new List<StaticObject>();
				foreach (var ObjectKey in ObjectKeys)
				{
					RouteData.Objects.TryGetValue(ObjectKey, out UnifiedObject Object);
					if (Object != null)
					{
						Objects.Add((StaticObject)Object);
					}
					else
					{
						Objects.Add(new StaticObject(Plugin.CurrentHost));
					}
				}

				if (!string.IsNullOrEmpty(Key))
				{
					RouteData.SignalObjects.Add(new SignalData
					{
						Key = Key,
						Numbers = Numbers.ToArray(),
						BaseObjects = Objects.ToArray()
					});
				}
				else
				{
					if (RouteData.SignalObjects.Any())
					{
						RouteData.SignalObjects.Last().GlowObjects = Objects.ToArray();
					}
				}
			}
		}
	}
}
