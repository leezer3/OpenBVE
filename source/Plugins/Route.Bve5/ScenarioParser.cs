using System;
using System.IO;
using System.Linq;
using System.Text;
using Bve5Parser.ScenarioGrammar;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		/// <summary>Checks whether the given file is a BVE5 scenario</summary>
		/// <param name="FileName">The filename to check</param>
		internal static bool IsBve5(string FileName)
		{
			using (StreamReader reader = new StreamReader(FileName))
			{
				var firstLine = reader.ReadLine() ?? "";
				string b = String.Empty;
				if (!firstLine.ToLowerInvariant().StartsWith("bvets scenario"))
				{
					return false;
				}
				for (int i = 15; i < firstLine.Length; i++)
				{
					if (Char.IsDigit(firstLine[i]) || firstLine[i] == '.')
					{
						b = b + firstLine[i];
					}
					else
					{
						break;
					}
				}
				if (b.Length > 0)
				{
					double version = 0;
					NumberFormats.TryParseDoubleVb6(b, out version);
					if (version > 2.0)
					{
						throw new Exception(version + " is not a supported BVE5 scenario version");
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		internal static void ParseScenario(string FileName, bool PreviewOnly)
		{
			Plugin.CurrentOptions.CurrentXParser = XParsers.Assimp;

			Encoding Encoding = DetermineFileEncoding(FileName);

			ScenarioParser Parser = new ScenarioParser();
			ScenarioData Data = Parser.Parse(File.ReadAllText(FileName, Encoding));

			Plugin.CurrentRoute.Comment = Data.Comment;
			if (!string.IsNullOrEmpty(Data.Image))
			{
				Plugin.CurrentRoute.Image = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Data.Image);
			}
			string RouteFile = String.Empty;

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
				RouteFile = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Data.Route[RouteFileIndex].Value);
			}

			ParseMap(RouteFile, PreviewOnly);
		}

		private static int GetRandomIndex(params double[] WeightTable)
		{
			double TotalWeight = 0.0;
			double[] ThresholdTable = new double[WeightTable.Length];

			for (int i = 0; i < WeightTable.Length; i++)
			{
				ThresholdTable[i] = TotalWeight;
				TotalWeight += WeightTable[i];
			}

			double Value = Plugin.RandomNumberGenerator.NextDouble() * TotalWeight;
			int RetIndex = -1;

			for (int i = WeightTable.Length - 1; i >= 0; i--)
			{
				if (Value >= ThresholdTable[i])
				{
					RetIndex = i;
					break;
				}
			}

			return RetIndex;
		}
	}
}
