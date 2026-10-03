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

using System;
using System.IO;
using System.Linq;
using System.Text;
using Bve5_Parsing.ScenarioGrammar;
using OpenBveApi;
using OpenBveApi.Math;
using RouteManager2.Stations;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	internal static partial class Bve5ScenarioParser
	{
		/// <summary>Checks whether the given file is a BVE5 scenario</summary>
		/// <param name="fileName">The filename to check</param>
		internal static bool IsBve5(string fileName)
		{
			try
			{
				using (StreamReader reader = new StreamReader(fileName))
				{
					string firstLine = reader.ReadLine() ?? string.Empty;
					if (!firstLine.StartsWith("bvets scenario", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}

					string versionText = string.Empty;
					for (int i = 15; i < firstLine.Length; i++)
					{
						if (char.IsDigit(firstLine[i]) || firstLine[i] == '.')
						{
							versionText += firstLine[i];
						}
						else
						{
							break;
						}
					}

					if (versionText.Length == 0)
					{
						return false;
					}

					NumberFormats.TryParseDoubleVb6(versionText, out double version);
					return version <= 2.0;
				}
			}
			catch
			{
				return false;
			}
		}

		internal static void ParseScenario(string fileName, bool previewOnly)
		{
			Encoding Encoding = Text.DetermineBVE5FileEncoding(fileName);

			ScenarioGrammarParser Parser = new ScenarioGrammarParser();
			ScenarioData Data = Parser.Parse(File.ReadAllText(fileName, Encoding));

			Plugin.CurrentRoute.Comment = Data.Comment ?? string.Empty;
			Plugin.CurrentRoute.Stations = Array.Empty<RouteStation>();
			CurrentStation = 0;
			if (!string.IsNullOrEmpty(Data.Image))
			{
				Plugin.CurrentRoute.Image = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), Data.Image);
			}
			string RouteFile = string.Empty;

			if (!Data.Route.Any())
			{
				throw new Exception("The BVE5 scenario did not define a route map");
			}

			double[] RouteFileWeightTable = new double[Data.Route.Count];

			for (int i = 0; i < Data.Route.Count; i++)
			{
				RouteFileWeightTable[i] = Data.Route[i].Weight;
			}

			int RouteFileIndex = GetRandomIndex(RouteFileWeightTable);

			if (RouteFileIndex != -1)
			{
				RouteFile = Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), Data.Route[RouteFileIndex].Value);
			}

			ParseMap(RouteFile, previewOnly);
		}

		private static int GetRandomIndex(params double[] WeightTable)
		{
			double TotalWeight = 0.0;
			foreach (double weight in WeightTable)
			{
				TotalWeight += weight;
			}

			double Value = Plugin.CurrentHost.Random.NextDouble() * TotalWeight;
			double Cumulative = 0.0;
			for (int i = 0; i < WeightTable.Length; i++)
			{
				Cumulative += WeightTable[i];
				if (Value < Cumulative)
				{
					return i;
				}
			}

			return WeightTable.Length > 0 ? WeightTable.Length - 1 : -1;
		}
	}
}
