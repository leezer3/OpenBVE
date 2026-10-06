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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
					RecoverIncludesWithoutDetectableEncoding(Data);

					if (IsDisplayErrors)
					{
						DisplayErrors();
					}
				}

				return Data;
			}

			/// <summary>Pulls the file path out of an unknown-encoding error.</summary>
			private static readonly Regex unknownEncodingPath = new Regex(@"File path：(.+?)）", RegexOptions.Compiled);

			/// <summary>Includes fixed up below; their original errors stay hidden.</summary>
			private readonly HashSet<string> recoveredIncludePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			/// <summary>
			/// Re-reads includes the library found but couldn't decode (no BOM, no :encoding suffix).
			/// Otherwise the whole include and its statements are lost.
			/// </summary>
			private void RecoverIncludesWithoutDetectableEncoding(MapData Data)
			{
				// Assume the route shares its top-level encoding; real BVE5 falls back to Shift-JIS.
				Encoding routeEncoding = GetDeclaredMapEncoding(FileName) ?? Encoding.GetEncoding("shift_jis");
				HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { FileName };
				Queue<string> pending = new Queue<string>();
				foreach (string path in FindUndecodableIncludes(Parser.ParserErrors))
				{
					if (visited.Add(path))
					{
						pending.Enqueue(path);
					}
				}

				while (pending.Count != 0)
				{
					if (plugin.Cancel) return;
					string path = pending.Dequeue();
					string text;
					try
					{
						text = ReadWithFallbackEncoding(path, routeEncoding);
					}
					catch
					{
						continue; // keep the original error
					}

					MapGrammarParser subParser = new MapGrammarParser();
					MapData subData;
					try
					{
						subData = subParser.Parse(text, path, MapGrammarParserOption.ParseIncludeSyntaxRecursively);
					}
					catch
					{
						continue;
					}
					Data.AddIncludeData(subData);
					recoveredIncludePaths.Add(path);

					foreach (ParseError error in subParser.ParserErrors)
					{
						Match match = unknownEncodingPath.Match(error.Message);
						if (match.Success)
						{
							if (visited.Add(match.Groups[1].Value))
							{
								pending.Enqueue(match.Groups[1].Value);
							}
						}
						else
						{
							Plugin.CurrentHost.AddMessage(error.ErrorLevel == ParseErrorLevel.Error ? MessageType.Error : MessageType.Warning, false,
								$"[{error.Line}:{error.Column}] {error.ErrorLevel}: {error.Message} in {path}");
						}
					}
				}
			}

			private static IEnumerable<string> FindUndecodableIncludes(IEnumerable<ParseError> errors)
			{
				foreach (ParseError error in errors)
				{
					Match match = unknownEncodingPath.Match(error.Message);
					if (match.Success)
					{
						yield return match.Groups[1].Value;
					}
				}
			}

			/// <summary>Header-declared encoding of a map file, or null.</summary>
			private static Encoding GetDeclaredMapEncoding(string fileName)
			{
				try
				{
					string firstLine;
					using (StreamReader reader = new StreamReader(fileName, Encoding.ASCII))
					{
						firstLine = reader.ReadLine();
					}
					if (firstLine == null)
					{
						return null;
					}
					string[] header = firstLine.Split(':');
					if (header.Length == 1)
					{
						return null;
					}
					return Encoding.GetEncoding(header[1].Split(',')[0].ToLowerInvariant().Trim());
				}
				catch
				{
					return null;
				}
			}

			/// <summary>Reads a file, honouring a BOM, else strict fallback encoding.</summary>
			private static string ReadWithFallbackEncoding(string path, Encoding fallback)
			{
				byte[] bytes = File.ReadAllBytes(path);
				if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
				{
					return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
				}
				if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
				{
					return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
				}
				if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
				{
					return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
				}
				Encoding strict = Encoding.GetEncoding(fallback.WebName, EncoderExceptionFallback.ExceptionFallback, DecoderExceptionFallback.ExceptionFallback);
				return strict.GetString(bytes);
			}

			private void DisplayErrors()
			{
				foreach (ParseError error in Parser.ParserErrors.OrderBy(e => e.Line).ThenBy(e => e.Column))
				{
					Match match = unknownEncodingPath.Match(error.Message);
					if (match.Success && recoveredIncludePaths.Contains(match.Groups[1].Value))
					{
						continue; // already recovered above
					}
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

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;


			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

			ConvertToBlock(FileName, PreviewOnly, RootData, out RouteData RouteData);

			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

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

			ConvertData(ParseData, RouteData, PreviewOnly);
			ConvertTrack(ParseData, RouteData);
			
			System.Threading.Thread.Sleep(1);
			if (plugin.Cancel) return;

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
