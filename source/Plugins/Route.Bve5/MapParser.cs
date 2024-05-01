using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bve5_Parsing;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi.Interface;
using static Bve5_Parsing.MapGrammar.MapGrammarParser;

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
			private MapGrammarParser Parser;

			internal MapParser(string fileName, string input, bool isDisplayErrors)
			{
				FileName = fileName;
				Input = input;
				IsDisplayErrors = isDisplayErrors;
			}


			internal MapData Parse()
			{
				var Data = new MapData();
				Parser = new MapGrammarParser();

				if (Parser != null)
				{
					Data = Parser.ParseFromFile(FileName, MapGrammarParserOption.ParseIncludeSyntaxRecursively);

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


			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			RouteData RouteData;
			ConvertToBlock(FileName, PreviewOnly, RootData, out RouteData);

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ApplyRouteData(FileName, PreviewOnly, RouteData);
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
				if (Statement.ElementName != MapElementName.Track)
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
