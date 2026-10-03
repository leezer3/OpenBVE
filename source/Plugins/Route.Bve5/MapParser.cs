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
using Bve5_Parsing;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.Climate;
using static Bve5_Parsing.MapGrammar.MapGrammarParser;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	internal static partial class Bve5ScenarioParser
	{
		private const int InterpolateInterval = 5;
		private const double StationNoticeDistance = -200.0;

		// Looks for a list file next to the scenario, and reports it if it is missing
		private static string FindComponentListFile(string FileName, string ListPath, string Kind)
		{
			if (File.Exists(ListPath))
			{
				return ListPath;
			}

			ListPath = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), ListPath);
			if (File.Exists(ListPath))
			{
				return ListPath;
			}

			Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BVE5: " + Kind + " file " + ListPath + " was not found.");
			return null;
		}

		// Gives the UI thread a chance to notice a cancel request
		private static bool CheckForCancel()
		{
			System.Threading.Thread.Sleep(1);
			return plugin.Cancel;
		}

		internal class MapParser
		{
			private readonly string FileName;
			private readonly bool IsDisplayErrors;
			private MapGrammarParser Parser;

			internal MapParser(string fileName, bool isDisplayErrors)
			{
				FileName = fileName;
				IsDisplayErrors = isDisplayErrors;
			}


			internal MapData Parse()
			{
				Parser = new MapGrammarParser();
				MapData Data = Parser.ParseFromFile(FileName, MapGrammarParserOption.ParseIncludeSyntaxRecursively);

				if (IsDisplayErrors)
				{
					DisplayErrors();
				}

				return Data;
			}

			private void DisplayErrors()
			{
				foreach (ParseError error in Parser.ParserErrors.OrderBy(e => e.Line).ThenBy(e => e.Column))
				{
					// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
					switch (error.ErrorLevel)
					{
						case ParseErrorLevel.Error:
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"[{error.Line}:{error.Column}] {error.ErrorLevel}: {error.Message} in {FileName}");
							break;
						case ParseErrorLevel.Warning:
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, $"[{error.Line}:{error.Column}] {error.ErrorLevel}: {error.Message} in {FileName}");
							break;
					}
				}
			}
		}

		private static void ParseMap(string FileName, bool PreviewOnly)
		{
			if (string.IsNullOrEmpty(FileName))
			{
				throw new Exception("The BVE5 scenario did not define a route map");
			}
			if (!File.Exists(FileName))
			{
				throw new Exception("The BVE5 route map file: " + FileName + " was not found");
			}

			MapParser Parser = new MapParser(FileName, true);
			MapData RootData = Parser.Parse();

			if (CheckForCancel()) return;


			if (CheckForCancel()) return;

			ConvertToBlock(FileName, PreviewOnly, RootData, out RouteData RouteData);

			if (CheckForCancel()) return;

			ApplyRouteData(FileName, PreviewOnly, RouteData);
		}

		private static void ConvertToBlock(string FileName, bool PreviewOnly, MapData ParseData, out RouteData RouteData)
		{
			RouteData = new RouteData(ParseData.TrackKeys);
			RouteData.TryAddBlock(0);
			RouteData.Blocks[0].Fog = new Fog(0, 1, Color24.Grey, 0, false);
			
			if (ParseData.StationListPaths.Count == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BVE5: No Station List file was specified.");
				return;
			}

			foreach (string path in ParseData.StationListPaths)
			{
				LoadStationList(FileName, path, RouteData);
			}

			foreach (string path in ParseData.StructureListPaths)
			{
				LoadStructureList(FileName, PreviewOnly, path, RouteData);
			}
			
			foreach (string path in ParseData.SignalListPaths)
			{
				LoadSignalList(FileName, PreviewOnly, path, RouteData);
			}

			foreach (string path in ParseData.SoundListPaths)
			{
				LoadSoundList(FileName, PreviewOnly, path, RouteData);
			}

			foreach (string path in ParseData.Sound3DListPaths)
			{
				LoadSound3DList(FileName, PreviewOnly, path, RouteData);
			}
			
			if (Plugin.CurrentOptions.EnableBve5ScriptedTrain)
			{
				LoadScriptedTrain(FileName, PreviewOnly, ParseData, RouteData);
			}

			if (CheckForCancel()) return;
			RouteData.Backgrounds = new ObjectDictionary();

			/*
			 * NOTE:
			 * Looping through the statement list multiple times is horrifically slow
			 * The slowness comes from somewhere within the BVE5 parsing library (to investigate)
			 *
			 * Track code currently requires a complete re-work to sort out properly so that only one loop
			 * is needed
			 *
			 */

			ConvertData(ParseData, RouteData, PreviewOnly);
			ConvertTrack(ParseData, RouteData);
			
			if (CheckForCancel()) return;

			ConfirmCurve(RouteData.Blocks);
			ConfirmGradient(RouteData.Blocks);
			ConfirmTrack(RouteData);
			ConfirmStructure(PreviewOnly, ParseData, RouteData);
			ConfirmRepeater(PreviewOnly, ParseData, RouteData);
			ConfirmSection(PreviewOnly, ParseData, RouteData);
			ConfirmSignal(PreviewOnly, ParseData, RouteData);
			ConfirmBeacon(PreviewOnly, ParseData, RouteData);
			// these require looping through existing blocks, so need to be here at the minute
			ConfirmIrregularity(PreviewOnly, RouteData);
			ConfirmAdhesion(PreviewOnly, RouteData);
			ConfirmFlangeNoise(PreviewOnly, ParseData, RouteData);
		}

		private static void ConvertData(MapData parseData, RouteData routeData, bool previewOnly)
		{
			foreach (Statement statement in parseData.Statements)
			{
				switch (statement.ElementName)
				{
					case MapElementName.Curve:
						ConvertCurve(statement, routeData);
						break;
					case MapElementName.Gradient:
						ConvertGradient(statement, routeData);
						break;
					case MapElementName.Legacy:
						switch (statement.FunctionName)
						{
							case MapFunctionName.Curve:
							case MapFunctionName.Turn:
								ConvertCurve(statement, routeData);
								break;
							case MapFunctionName.Pitch:
								ConvertGradient(statement, routeData);
								break;
							case MapFunctionName.Fog:
								if (!previewOnly)
								{
									ConvertFog(statement, routeData);
								}
								break;
						}
						break;
					case MapElementName.Station:
						ConvertStation(statement, routeData);
						break;
					default:
						// Everything else is only needed for the full parse
						if (previewOnly)
						{
							break;
						}
						switch (statement.ElementName)
						{
							case MapElementName.Background:
								ConvertBackground(statement, routeData);
								break;
							case MapElementName.Fog:
								ConvertFog(statement, routeData);
								break;
							case MapElementName.Irregularity:
								ConvertIrregularity(statement, routeData);
								break;
							case MapElementName.Adhesion:
								ConvertAdhesion(statement, routeData);
								break;
							case MapElementName.JointNoise:
								ConvertJointNoise(statement, routeData);
								break;
							case MapElementName.Pretrain:
								ConfirmPreTrain(statement);
								break;
							case MapElementName.Light:
								ConfirmLight(statement);
								break;
							case MapElementName.Sound:
								ConfirmSound(statement, routeData);
								break;
							case MapElementName.Sound3d:
								ConfirmSound3D(statement, routeData);
								break;
							case MapElementName.SpeedLimit:
								ConfirmSpeedLimit(statement, routeData);
								break;
							case MapElementName.CabIlluminance:
								ConfirmCabIlluminance(statement, routeData);
								break;
							case MapElementName.RollingNoise:
								ConfirmRollingNoise(statement, routeData);
								break;
						}
						break;
				}
			}
		}
	}
}
