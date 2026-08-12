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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bve5_Parsing;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.Climate;
using static Bve5_Parsing.MapGrammar.MapGrammarParser;

namespace Route.Bve5
{
	internal static partial class Bve5ScenarioParser
	{
		private const int InterpolateInterval = 5;
		private const double StationNoticeDistance = -200.0;

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
				MapData Data = new MapData();
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

			Stopwatch fileTimer = Stopwatch.StartNew();
			MapParser Parser = new MapParser(FileName, true);
			MapData RootData = Parser.Parse();
			fileTimer.Stop();

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;


			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			Stopwatch parseTimer = Stopwatch.StartNew();
			ConvertToBlock(FileName, PreviewOnly, RootData, out RouteData RouteData);
			parseTimer.Stop();
			Plugin.CurrentHost.PluginParseTime = parseTimer.ElapsedMilliseconds;

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			Stopwatch applyTimer = Stopwatch.StartNew();
			ApplyRouteData(FileName, PreviewOnly, RouteData);
			applyTimer.Stop();
			Plugin.CurrentHost.PluginApplyTime = applyTimer.ElapsedMilliseconds;

			Plugin.FileSystem.AppendToLogFile("Bve5: Map file parse: " + fileTimer.ElapsedMilliseconds + " ms, block conversion: " + parseTimer.ElapsedMilliseconds + " ms, route apply: " + applyTimer.ElapsedMilliseconds + " ms");
			Plugin.FileSystem.AppendToLogFile("Bve5: Blocks: " + RouteData.Blocks.Count + ", Track elements: " + GetTrackElementCount());
		}

		private static int GetTrackElementCount()
		{
			int count = 0;
			for (int k = 0; k < Plugin.CurrentRoute.Tracks.Count; k++)
			{
				var elements = Plugin.CurrentRoute.Tracks[k].Elements;
				if (elements == null)
				{
					continue;
				}
				for (int i = 0; i < elements.Length; i++)
				{
					if (elements[i].Events != null)
					{
						count++;
					}
				}
			}
			return count;
		}

		private static void LogPhase(string name, Stopwatch timer)
		{
			timer.Stop();
			Plugin.FileSystem.AppendToLogFile("Bve5 phase: " + name + ": " + timer.ElapsedMilliseconds + " ms");
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

			Stopwatch phaseTimer = Stopwatch.StartNew();
			foreach (string path in ParseData.StationListPaths)
			{
				LoadStationList(FileName, path, RouteData);
			}
			LogPhase("LoadStationList", phaseTimer);

			phaseTimer = Stopwatch.StartNew();
			foreach (string path in ParseData.StructureListPaths)
			{
				LoadStructureList(FileName, PreviewOnly, path, RouteData);
			}
			LogPhase("LoadStructureList", phaseTimer);
			
			phaseTimer = Stopwatch.StartNew();
			foreach (string path in ParseData.SignalListPaths)
			{
				LoadSignalList(FileName, PreviewOnly, path, RouteData);
			}
			LogPhase("LoadSignalList", phaseTimer);

			phaseTimer = Stopwatch.StartNew();
			foreach (string path in ParseData.SoundListPaths)
			{
				LoadSoundList(FileName, PreviewOnly, path, RouteData);
			}
			LogPhase("LoadSoundList", phaseTimer);

			phaseTimer = Stopwatch.StartNew();
			foreach (string path in ParseData.Sound3DListPaths)
			{
				LoadSound3DList(FileName, PreviewOnly, path, RouteData);
			}
			LogPhase("LoadSound3DList", phaseTimer);
			
			phaseTimer = Stopwatch.StartNew();
			if (Plugin.CurrentOptions.EnableBve5ScriptedTrain)
			{
				LoadScriptedTrain(FileName, PreviewOnly, ParseData, RouteData);
			}
			LogPhase("LoadScriptedTrain", phaseTimer);

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;
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

			phaseTimer = Stopwatch.StartNew();
			ConvertData(ParseData, RouteData, PreviewOnly);
			LogPhase("ConvertData", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConvertTrack(ParseData, RouteData);
			LogPhase("ConvertTrack", phaseTimer);
			
			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			phaseTimer = Stopwatch.StartNew();
			ConfirmCurve(RouteData.Blocks);
			LogPhase("ConfirmCurve", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmGradient(RouteData.Blocks);
			LogPhase("ConfirmGradient", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmTrack(RouteData);
			LogPhase("ConfirmTrack", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmStructure(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmStructure", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmRepeater(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmRepeater", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmSection(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmSection", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmSignal(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmSignal", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmBeacon(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmBeacon", phaseTimer);
			// these require looping through existing blocks, so need to be here at the minute
			phaseTimer = Stopwatch.StartNew();
			ConfirmIrregularity(PreviewOnly, RouteData);
			LogPhase("ConfirmIrregularity", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmAdhesion(PreviewOnly, RouteData);
			LogPhase("ConfirmAdhesion", phaseTimer);
			phaseTimer = Stopwatch.StartNew();
			ConfirmFlangeNoise(PreviewOnly, ParseData, RouteData);
			LogPhase("ConfirmFlangeNoise", phaseTimer);
		}

		private static void ConvertData(MapData parseData, RouteData routeData, bool previewOnly)
		{
			for (int i = 0; i < parseData.Statements.Count; i++)
			{
				switch(parseData.Statements[i].ElementName)
				{
					case MapElementName.Curve:
						ConvertCurve(parseData.Statements[i], routeData);
						break;
					case MapElementName.Gradient:
						ConvertGradient(parseData.Statements[i], routeData);
						break;
					case MapElementName.Legacy:
						switch (parseData.Statements[i].FunctionName)
						{
							case MapFunctionName.Curve:
							case MapFunctionName.Turn:
								ConvertCurve(parseData.Statements[i], routeData);
								break;
							case MapFunctionName.Pitch:
								ConvertGradient(parseData.Statements[i], routeData);
								break;
							case MapFunctionName.Fog:
								if (!previewOnly)
								{
									ConvertFog(parseData.Statements[i], routeData);
								}
								break;
						}
						break;
					case MapElementName.Station:
						ConvertStation(parseData.Statements[i], routeData);
						break;
					case MapElementName.Background:
						if (!previewOnly)
						{
							ConvertBackground(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.Fog:
						if (!previewOnly)
						{
							ConvertFog(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.Irregularity:
						if (!previewOnly)
						{
							ConvertIrregularity(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.Adhesion:
						if (!previewOnly)
						{
							ConvertAdhesion(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.JointNoise:
						if (!previewOnly)
						{
							ConvertJointNoise(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.Pretrain:
						if (!previewOnly)
						{
							ConfirmPreTrain(parseData.Statements[i]);
						}
						break;
					case MapElementName.Light:
						if (!previewOnly)
						{
							ConfirmLight(parseData.Statements[i]);
						}
						break;
					case MapElementName.Sound:
						if (!previewOnly)
						{
							ConfirmSound(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.Sound3d:
						if (!previewOnly)
						{
							ConfirmSound3D(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.SpeedLimit:
						if (!previewOnly)
						{
							ConfirmSpeedLimit(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.CabIlluminance:
						if (!previewOnly)
						{
							ConfirmCabIlluminance(parseData.Statements[i], routeData);
						}
						break;
					case MapElementName.RollingNoise:
						if (!previewOnly)
						{
							ConfirmRollingNoise(parseData.Statements[i], routeData);
						}
						break;
				}
			}
		}
	}
}
