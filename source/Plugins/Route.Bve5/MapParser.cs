using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Bve5Parser;
using Bve5Parser.MapGrammar;
using Bve5Parser.MapGrammar.V1;
using Bve5Parser.MapGrammar.V2;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private const int InterpolateInterval = 5;
		private const double StationNoticeDistance = -200.0;

		internal class MapParser
		{
			private readonly string FileName;
			private readonly string Input;
			private readonly bool IsDisplayErrors;
			private Bve5Parser.MapGrammar.MapParser Parser;

			internal MapParser(string fileName, string input, bool isDisplayErrors)
			{
				FileName = fileName;
				Input = input;
				IsDisplayErrors = isDisplayErrors;
			}

			private Bve5Parser.MapGrammar.MapParser VersionCheck()
			{
				using (var reader = new StringReader(Input))
				{
					var firstLine = reader.ReadLine() ?? "";
					var b = string.Empty;

					if (!firstLine.ToLowerInvariant().StartsWith("bvets map"))
					{
						return null;
					}

					for (int i = 10; i < firstLine.Length; i++)
					{
						if (char.IsDigit(firstLine[i]) || firstLine[i] == '.')
						{
							b += firstLine[i];
						}
						else
						{
							break;
						}
					}

					double version;
					if (!b.Any() || !NumberFormats.TryParseDoubleVb6(b, out version))
					{
						return null;
					}

					if (version < 2.0)
					{
						return new MapV1Parser();
					}

					return new MapV2Parser();

				}
			}

			internal MapData Parse()
			{
				var Data = new MapData();
				Parser = VersionCheck();

				if (Parser != null)
				{
					Data = Parser.Parse(Input);

					if (IsDisplayErrors)
					{
						DisplayErrors();
					}
				}

				return Data;
			}

			private void DisplayErrors()
			{
				foreach (var error in Parser.ParserErrors.OrderBy(e => e.Line).ThenBy(e => e.Column))
				{
					switch (error.ErrorLevel)
					{
						case ParseErrorLevel.Error:
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, string.Format("[{0}:{1}] {2}: {3} in {4}", error.Line, error.Column, error.ErrorLevel, error.Message, FileName));
							break;
						case ParseErrorLevel.Warning:
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, string.Format("[{0}:{1}] {2}: {3} in {4}", error.Line, error.Column, error.ErrorLevel, error.Message, FileName));
							break;
					}
				}
			}
		}

		private static void ParseMap(string FileName, bool PreviewOnly)
		{
			if (FileName == String.Empty)
			{
				throw new Exception("The BVE5 scenario did not define a route map");
			}
			if (!File.Exists(FileName))
			{
				throw new Exception("The BVE5 route map file: " + FileName + " was not found");
			}

			System.Text.Encoding Encoding = DetermineFileEncoding(FileName);
			string InputText = File.ReadAllText(FileName, Encoding);
			MapParser Parser = new MapParser(FileName, InputText, true);
			MapData RootData = Parser.Parse();

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ParseIncludeMap(FileName, InputText, ref RootData);

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			RouteData RouteData;
			ConvertToBlock(FileName, PreviewOnly, RootData, out RouteData);

			// Debug
			//if (!PreviewOnly)
			//{
			//    int FreeObjCount = 0;
			//    foreach (var Block in RouteData.Blocks)
			//    {
			//        foreach (var FreeObj in Block.FreeObj)
			//        {
			//            if (FreeObj != null)
			//            {
			//                FreeObjCount += FreeObj.Count;
			//            }
			//        }
			//    }
			//}

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ApplyRouteData(FileName, PreviewOnly, RouteData);
		}

		private static void ParseIncludeMap(string FileName, string InputText, ref MapData Data)
		{
			bool IsAllInclude = false;
			int IncludePosition = 0;
			MapData ModData = Data.Clone();
			MapParser Parser;

			while (!IsAllInclude)
			{
				for (var i = IncludePosition; i < ModData.Statements.Count; i++)
				{
					if (ModData.Statements[i].MapElement[0] != "include")
					{
						continue;
					}

					IncludePosition = i;

					object IncludeFileName, StartIndex, StopIndex;

					ModData.Statements[i].Arguments.TryGetValue("path", out IncludeFileName);
					ModData.Statements[i].Arguments.TryGetValue("startindex", out StartIndex);
					ModData.Statements[i].Arguments.TryGetValue("stopindex", out StopIndex);

					string LeftInputText = InputText.Substring(0, Convert.ToInt32(StartIndex));
					string RightInputText = InputText.Substring(Convert.ToInt32(StopIndex) + 1, InputText.Length - (Convert.ToInt32(StopIndex) + 1));
					LeftInputText = Regex.Replace(LeftInputText, @"i\s*n\s*c\s*l\s*u\s*d\s*e\s*\z", string.Empty, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
					RightInputText = Regex.Replace(RightInputText, @"^\s*;", string.Empty, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

					IncludeFileName = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Convert.ToString(IncludeFileName));
					string IncludeText = string.Empty;

					if (File.Exists(Convert.ToString(IncludeFileName)))
					{
						System.Text.Encoding Encoding = DetermineFileEncoding(Convert.ToString(IncludeFileName));
						Parser = new MapParser(Convert.ToString(IncludeFileName), File.ReadAllText(Convert.ToString(IncludeFileName), Encoding), true);
						Parser.Parse();
						IncludeText = string.Join(Environment.NewLine, File.ReadAllLines(Convert.ToString(IncludeFileName), Encoding).Skip(1)).Trim('\x1a');
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, IncludeFileName + "is not found.");
					}

					InputText = LeftInputText + IncludeText + RightInputText;
					break;
				}

				Parser = new MapParser(string.Empty, InputText, false);
				ModData = Parser.Parse();

				for (var i = IncludePosition; i < ModData.Statements.Count; i++)
				{
					if (ModData.Statements[i].MapElement[0] != "include")
					{
						IsAllInclude = true;
					}
					else
					{
						IsAllInclude = false;
						break;
					}
				}
			}

			Data = ModData.Clone();
			Data.Statements = Data.Statements.OrderBy(Statement => Statement.Distance).ToList();
		}

		private static void ConvertToBlock(string FileName, bool PreviewOnly, MapData ParseData, out RouteData RouteData)
		{
			RouteData = new RouteData
			{
				Blocks = new List<Block>(),
				TrackKeyList = new List<string>()
			};
			// Own track
			RouteData.TrackKeyList.Add("0");

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "track")
				{
					continue;
				}

				string TrackKey = Statement.Key;
				if (!RouteData.TrackKeyList.Contains(TrackKey))
				{
					RouteData.TrackKeyList.Add(TrackKey);
				}
			}

			RouteData.FindOrAddBlock(ParseData.Statements[0].Distance);

			LoadStationList(FileName, ParseData, RouteData);
			LoadStructureList(FileName, PreviewOnly, ParseData, RouteData);
			LoadSignalList(FileName, PreviewOnly, ParseData, RouteData);
			LoadSoundList(FileName, PreviewOnly, ParseData, RouteData);
			LoadSound3DList(FileName, PreviewOnly, ParseData, RouteData);
			if (Plugin.CurrentOptions.EnableBve5TFO)
			{
				LoadOtherTrain(FileName, PreviewOnly, ParseData, RouteData);
			}

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ConvertCurve(ParseData, RouteData);
			ConvertGradient(ParseData, RouteData);
			ConvertTrack(ParseData, RouteData);
			ConvertStation(ParseData, RouteData);
			ConvertBackground(PreviewOnly, ParseData, RouteData);
			ConvertFog(PreviewOnly, ParseData, RouteData);
			ConvertIrregularity(PreviewOnly, ParseData, RouteData);
			ConvertAdhesion(PreviewOnly, ParseData, RouteData);
			ConvertJointNoise(PreviewOnly, ParseData, RouteData);

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ConfirmCurve(RouteData.Blocks);
			ConfirmGradient(RouteData.Blocks);
			ConfirmTrack(RouteData);
			ConfirmStructure(PreviewOnly, ParseData, RouteData);
			ConfirmRepeater(PreviewOnly, ParseData, RouteData);
			ConfirmSection(PreviewOnly, ParseData, RouteData);
			ConfirmSignal(PreviewOnly, ParseData, RouteData);
			ConfirmBeacon(PreviewOnly, ParseData, RouteData.Blocks);
			ConfirmSpeedLimit(PreviewOnly, ParseData, RouteData);
			ConfirmPreTrain(PreviewOnly, ParseData);
			ConfirmLight(PreviewOnly, ParseData);
			ConfirmCabIlluminance(PreviewOnly, ParseData, RouteData.Blocks);
			ConfirmIrregularity(PreviewOnly, RouteData.Blocks);
			ConfirmAdhesion(PreviewOnly, RouteData.Blocks);
			ConfirmSound(PreviewOnly, ParseData, RouteData);
			ConfirmSound3D(PreviewOnly, ParseData, RouteData);
			ConfirmRollingNoise(PreviewOnly, ParseData, RouteData);
			ConfirmFlangeNoise(PreviewOnly, ParseData, RouteData);
		}
	}
}
