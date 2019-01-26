using System;
using System.Collections.Generic;
using System.Globalization;
using Path = OpenBveApi.Path;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Objects;
using OpenBveApi.Interface;
using OpenBveApi.Trains;

namespace OpenBve {
	internal partial class CsvRwRouteParser {
		internal static string ObjectPath;
		internal static string SoundPath;
		internal static string TrainPath;
		internal static string CompatibilityFolder;
		internal static bool CylinderHack = false;

		// parse route
		internal static void ParseRoute(string FileName, bool IsRW, System.Text.Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly) {
			// initialize data

			/*
			 * Store paths for later use
			 */
			ObjectPath = objectPath;
			SoundPath = soundPath;
			TrainPath = trainPath;
			freeObjCount = 0;
			railtypeCount = 0;
			Game.UnitOfSpeed = "km/h";
			Game.SpeedConversionFactor = 0.0;
			Game.RouteInformation.RouteBriefing = null;
			CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
			if (!PreviewOnly)
			{
				CompatibilityObjects.LoadCompatibilityObjects(OpenBveApi.Path.CombineFile(CompatibilityFolder,"CompatibilityObjects.xml"));
			}
			RouteData Data = new RouteData
			{
				BlockInterval = 25.0,
				AccurateObjectDisposal = false,
				FirstUsedBlock = -1,
				Blocks = new Block[1]
			};
			Data.Blocks[0] = new Block();
			Data.Blocks[0].Rails = new Rail[1];
			Data.Blocks[0].Rails[0].RailStart = true;
			Data.Blocks[0].RailType = new int[] { 0 };
			Data.Blocks[0].Limits = new Limit[] { };
			Data.Blocks[0].StopPositions = new Stop[] { };
			Data.Blocks[0].Station = -1;
			Data.Blocks[0].StationPassAlarm = false;
			Data.Blocks[0].Accuracy = 2.0;
			Data.Blocks[0].AdhesionMultiplier = 1.0;
			Data.Blocks[0].CurrentTrackState = new TrackManager.TrackElement(0.0);
			if (!PreviewOnly)
			{
				Data.Blocks[0].Background = 0;
				Data.Blocks[0].BrightnessChanges = new Brightness[] {};
				Data.Blocks[0].Fog.Start = Game.NoFogStart;
				Data.Blocks[0].Fog.End = Game.NoFogEnd;
				Data.Blocks[0].Fog.Color = Color24.Grey;
				Data.Blocks[0].Cycle = new int[] {-1};
				Data.Blocks[0].RailCycles = new RailCycle[1];
				Data.Blocks[0].RailCycles[0].RailCycleIndex = -1;
				Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
				Data.Blocks[0].RailFreeObj = new FreeObj[][] {};
				Data.Blocks[0].GroundFreeObj = new FreeObj[] {};
				Data.Blocks[0].RailWall = new WallDike[] {};
				Data.Blocks[0].RailDike = new WallDike[] {};
				Data.Blocks[0].RailPole = new Pole[] {};
				Data.Blocks[0].Forms = new Form[] {};
				Data.Blocks[0].Cracks = new Crack[] {};
				Data.Blocks[0].Signals = new Signal[] {};
				Data.Blocks[0].Sections = new Section[] {};
				Data.Blocks[0].SoundEvents = new Sound[] {};
				Data.Blocks[0].Transponders = new Transponder[] {};
				Data.Blocks[0].DestinationChanges = new DestinationEvent[] {};
				Data.Blocks[0].PointsOfInterest = new PointOfInterest[] {};
				Data.Markers = new Marker[] {};
				Data.RequestStops = new StopRequest[] { };
				string PoleFolder = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Poles");
				Data.Structure.Poles = new UnifiedObject[][]
				{
					new UnifiedObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(PoleFolder, "pole_1.csv"), System.Text.Encoding.UTF8, false, false, false)
					},
					new UnifiedObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(PoleFolder, "pole_2.csv"), System.Text.Encoding.UTF8, false, false, false)
					},
					new UnifiedObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(PoleFolder, "pole_3.csv"), System.Text.Encoding.UTF8, false, false, false)
					},
					new UnifiedObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(PoleFolder, "pole_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					}
				};
				Data.Structure.RailObjects = new ObjectDictionary();
				Data.Structure.RailObjects = new ObjectDictionary();
				Data.Structure.Ground = new ObjectDictionary();
				Data.Structure.WallL = new ObjectDictionary();
				Data.Structure.WallR = new ObjectDictionary();
				Data.Structure.DikeL = new ObjectDictionary();
				Data.Structure.DikeR = new ObjectDictionary();
				Data.Structure.FormL = new ObjectDictionary();
				Data.Structure.FormR = new ObjectDictionary();
				Data.Structure.FormCL = new ObjectDictionary();
				Data.Structure.FormCR = new ObjectDictionary();
				Data.Structure.RoofL = new ObjectDictionary();
				Data.Structure.RoofR = new ObjectDictionary();
				Data.Structure.RoofCL = new ObjectDictionary();
				Data.Structure.RoofCR = new ObjectDictionary();
				Data.Structure.CrackL = new ObjectDictionary();
				Data.Structure.CrackR = new ObjectDictionary();
				Data.Structure.FreeObjects = new ObjectDictionary();
				Data.Structure.Beacon = new ObjectDictionary();
				Data.Structure.Cycles = new int[][] {};
				Data.Structure.RailCycles = new int[][] { };
				Data.Structure.Run = new int[] {};
				Data.Structure.Flange = new int[] {};
				Data.Backgrounds = new BackgroundManager.BackgroundHandle[] {};
				Data.TimetableDaytime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				Data.TimetableNighttime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				// signals
				string SignalFolder = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				Data.Signals = new SignalData[7];
				Data.Signals[3] = new CompatibilitySignalData(new int[] {0, 2, 4}, new ObjectManager.StaticObject[]
				{
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_0.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_2.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_4.csv"), System.Text.Encoding.UTF8, false, false, false)
				});
				Data.Signals[4] = new CompatibilitySignalData(new int[] {0, 1, 2, 4}, new ObjectManager.StaticObject[]
				{
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4_0.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_1.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_2.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_4.csv"), System.Text.Encoding.UTF8, false, false, false)
				});
				Data.Signals[5] = new CompatibilitySignalData(new int[] {0, 1, 2, 3, 4}, new ObjectManager.StaticObject[]
				{
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5a_1.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), System.Text.Encoding.UTF8, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), System.Text.Encoding.UTF8, false, false, false)
				});
				Data.Signals[6] = new CompatibilitySignalData(new int[] {0, 3, 4}, new ObjectManager.StaticObject[]
				{
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_0.csv"),
						Encoding, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_3.csv"),
						Encoding, false, false, false),
					ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_4.csv"),
						Encoding, false, false, false)
				});
				// compatibility signals
				Data.CompatibilitySignals = new CompatibilitySignalData[9];
				Data.CompatibilitySignals[0] = new CompatibilitySignalData(new int[] {0, 2},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2a_2.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[1] = new CompatibilitySignalData(new int[] {0, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2b_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[2] = new CompatibilitySignalData(new int[] {0, 2, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[3] = new CompatibilitySignalData(new int[] {0, 1, 2, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_1.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[4] = new CompatibilitySignalData(new int[] {0, 2, 3, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_3.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[5] = new CompatibilitySignalData(new int[] {0, 1, 2, 3, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5a_1.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[6] = new CompatibilitySignalData(new int[] {0, 2, 3, 4, 5},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5b_5.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[7] = new CompatibilitySignalData(new int[] {0, 1, 2, 3, 4, 5},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_0.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_1.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_2.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_3.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_4.csv"), System.Text.Encoding.UTF8, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_5.csv"), System.Text.Encoding.UTF8, false, false, false)
					});
				Data.CompatibilitySignals[8] = new CompatibilitySignalData(new int[] {0, 3, 4},
					new ObjectManager.StaticObject[]
					{
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_0.csv"),
							Encoding, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_3.csv"),
							Encoding, false, false, false),
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_4.csv"),
							Encoding, false, false, false)
					});
				// game data
				Game.Sections = new Game.Section[1];
				Game.Sections[0].Aspects = new Game.SectionAspect[]
				{new Game.SectionAspect(0, 0.0), new Game.SectionAspect(4, double.PositiveInfinity)};
				Game.Sections[0].CurrentAspect = 0;
				Game.Sections[0].NextSection = -1;
				Game.Sections[0].PreviousSection = -1;
				Game.Sections[0].StationIndex = -1;
				Game.Sections[0].TrackPosition = 0;
				Game.Sections[0].Trains = new TrainManager.Train[] {};

				/*
				 * These are the speed limits for the default Japanese signal aspects, and in most cases will be overwritten
				 */
				Data.SignalSpeeds = new double[]
				{0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity};
			}
			ParseRouteForData(FileName, IsRW, Encoding, ref Data, PreviewOnly);
			if (Loading.Cancel) return;
			ApplyRouteData(FileName, ref Data, PreviewOnly);

//		    if (PreviewOnly == true && customLoadScreen == false)
//		    {
//		        Renderer.TextureLogo = null;
//		    }
		}

		// ================================

		// parse route for data
		
		private static void ParseRouteForData(string FileName, bool IsRW, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly) {
			//Read the entire routefile into memory
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			Expression[] Expressions;
			PreprocessSplitIntoExpressions(FileName, IsRW, Lines, out Expressions, true, 0.0);
			PreprocessChrRndSub(FileName, IsRW, Encoding, ref Expressions);
			double[] UnitOfLength = new double[] { 1.0 };
			//Set units of speed initially to km/h
			//This represents 1km/h in m/s
			Data.UnitOfSpeed = 0.277777777777778;
			PreprocessOptions(IsRW, Expressions, ref Data, ref UnitOfLength, PreviewOnly);
			PreprocessSortByTrackPosition(IsRW, UnitOfLength, ref Expressions);
			ParseRouteForData(FileName, IsRW, Encoding, Expressions, UnitOfLength, ref Data, PreviewOnly);
			Game.RouteUnitOfLength = UnitOfLength;
		}

		// preprocess split into expressions
		private static void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, out Expression[] Expressions, bool AllowRwRouteDescription, double trackPositionOffset) {
			Expressions = new Expression[4096];
			int e = 0;
			// full-line rw comments
			if (IsRW) {
				for (int i = 0; i < Lines.Length; i++) {
					int Level = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ';':
								if (Level == 0) {
									Lines[i] = Lines[i].Substring(0, j).TrimEnd();
									j = Lines[i].Length;
								}
								break;
							case '=':
								if (Level == 0) {
									j = Lines[i].Length;
								}
								break;
						}
					}
				}
			}
			// parse
			for (int i = 0; i < Lines.Length; i++) {
				//Remove empty null characters
				//Found these in a couple of older routes, harmless but generate errors
				//Possibly caused by BVE-RR (DOS version)
				Lines[i] = Lines[i].Replace("\0", "");
				if (IsRW & AllowRwRouteDescription) {
					// ignore rw route description
					if (
						Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].IndexOf("]", StringComparison.Ordinal) > 0 |
						Lines[i].StartsWith("$")
					) {
						AllowRwRouteDescription = false;
						Game.RouteComment = Game.RouteComment.Trim();
					} else {
						if (Game.RouteComment.Length != 0) {
							Game.RouteComment += "\n";
						}
						Game.RouteComment += Lines[i];
						continue;
					}
				}
				{
					// count expressions
					int n = 0; int Level = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ',':
								if (!IsRW & Level == 0) n++;
								break;
							case '@':
								if (IsRW & Level == 0) n++;
								break;
						}
					}
					// create expressions
					int m = e + n + 1;
					while (m >= Expressions.Length) {
						Array.Resize<Expression>(ref Expressions, Expressions.Length << 1);
					}
					Level = 0;
					int a = 0, c = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								if (Interface.CurrentOptions.EnableBveTsHacks)
								{

									if (Level > 0)
									{
										//Don't decrease the level below zero, as this messes up when extra closing brackets are encountered
										Level--;
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid additional closing parenthesis encountered at line " + i + " character " + j + " in file " + FileName);
									}
								}
								else
								{
									Level--;
								}
								break;
							case ',':
								if (Level == 0 & !IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim();
									if (t.Length > 0 && !t.StartsWith(";")) {
										Expressions[e] = new Expression
										{
											File = FileName,
											Text = t,
											Line = i + 1,
											Column = c + 1,
											TrackPositionOffset = trackPositionOffset
										};
										e++;
									}
									a = j + 1;
									c++;
								}
								break;
							case '@':
								if (Level == 1 & IsRW & Interface.CurrentOptions.EnableBveTsHacks)
								{
									//BVE2 doesn't care if a bracket is unclosed, fixes various routefiles
									Level--;
								}
								else if (Level == 2 && IsRW & Interface.CurrentOptions.EnableBveTsHacks)
								{
									int k = j;
									while (k > 0)
									{
										k--;
										if (Lines[i][k] == '(')
										{
											//Opening bracket has been used instead of closing bracket, again BVE2 ignores this
											Level -= 2;
											break;
										}
										if (!char.IsWhiteSpace(Lines[i][k]))
										{
											//Bracket not found, and this isn't whitespace either, so break out
											break;
										}
									}
								}
								if (Level == 0 & IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim();
									if (t.Length > 0 && !t.StartsWith(";")) {
										Expressions[e] = new Expression
										{
											File = FileName,
											Text = t,
											Line = i + 1,
											Column = c + 1,
											TrackPositionOffset = trackPositionOffset
										};
										e++;
									}
									a = j + 1;
									c++;
								}
								break;
						}
					}
					if (Lines[i].Length - a > 0) {
						string t = Lines[i].Substring(a).Trim();
						if (t.Length > 0 && !t.StartsWith(";")) {
							Expressions[e] = new Expression
							{
								File = FileName,
								Text = t,
								Line = i + 1,
								Column = c + 1,
								TrackPositionOffset = trackPositionOffset
							};
							e++;
						}
					}
				}
			}
			Array.Resize<Expression>(ref Expressions, e);
		}

		/// <summary>This function processes the list of expressions for $Char, $Rnd, $If and $Sub directives, and evaluates them into the final expressions dataset</summary>
		private static void PreprocessChrRndSub(string FileName, bool IsRW, System.Text.Encoding Encoding, ref Expression[] Expressions) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Subs = new string[16];
			int openIfs = 0;
			for (int i = 0; i < Expressions.Length; i++) {
				string Epilog = " at line " + Expressions[i].Line.ToString(Culture) + ", column " + Expressions[i].Column.ToString(Culture) + " in file " + Expressions[i].File;
				bool continueWithNextExpression = false;
				for (int j = Expressions[i].Text.Length - 1; j >= 0; j--) {
					if (Expressions[i].Text[j] == '$') {
						int k;
						for (k = j + 1; k < Expressions[i].Text.Length; k++) {
							if (Expressions[i].Text[k] == '(') {
								break;
							} else if (Expressions[i].Text[k] == '/' | Expressions[i].Text[k] == '\\') {
								k = Expressions[i].Text.Length + 1;
								break;
							}
						}
						if (k <= Expressions[i].Text.Length) {
							string t = Expressions[i].Text.Substring(j, k - j).TrimEnd();
							int l = 1, h;
							for (h = k + 1; h < Expressions[i].Text.Length; h++) {
								switch (Expressions[i].Text[h]) {
									case '(':
										l++;
										break;
									case ')':
										l--;
										if (l < 0) {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Invalid parenthesis structure in " + t + Epilog);
										}
										break;
								}
								if (l <= 0) {
									break;
								}
							}
							if (continueWithNextExpression) {
								break;
							}
							if (l != 0) {
								Interface.AddMessage(MessageType.Error, false, "Invalid parenthesis structure in " + t + Epilog);
								continueWithNextExpression = true;
								break;
							}
							string s = Expressions[i].Text.Substring(k + 1, h - k - 1).Trim();
							switch (t.ToLowerInvariant()) {
								case "$if":
									if (j != 0) {
										Interface.AddMessage(MessageType.Error, false, "The $If directive must not appear within another statement" + Epilog);
									} else {
										double num;
										if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out num)) {
											openIfs++;
											Expressions[i].Text = string.Empty;
											if (num == 0.0) {
												/*
												 * Blank every expression until the matching $Else or $EndIf
												 * */
												i++;
												int level = 1;
												while (i < Expressions.Length) {
													if (Expressions[i].Text.StartsWith("$if", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														level++;
													} else if (Expressions[i].Text.StartsWith("$else", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														if (level == 1) {
															level--;
															break;
														}
													} else if (Expressions[i].Text.StartsWith("$endif", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														level--;
														if (level == 0) {
															openIfs--;
															break;
														}
													} else {
														Expressions[i].Text = string.Empty;
													}
													i++;
												}
												if (level != 0) {
													Interface.AddMessage(MessageType.Error, false, "$EndIf missing at the end of the file" + Epilog);
												}
											}
											continueWithNextExpression = true;
											break;
										} else {
											Interface.AddMessage(MessageType.Error, false, "The $If condition does not evaluate to a number" + Epilog);
										}
									}
									continueWithNextExpression = true;
									break;
								case "$else":
									/*
									 * Blank every expression until the matching $EndIf
									 * */
									Expressions[i].Text = string.Empty;
									if (openIfs != 0) {
										i++;
										int level = 1;
										while (i < Expressions.Length) {
											if (Expressions[i].Text.StartsWith("$if", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												level++;
											} else if (Expressions[i].Text.StartsWith("$else", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												if (level == 1) {
													Interface.AddMessage(MessageType.Error, false, "Duplicate $Else encountered" + Epilog);
												}
											} else if (Expressions[i].Text.StartsWith("$endif", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												level--;
												if (level == 0) {
													openIfs--;
													break;
												}
											} else {
												Expressions[i].Text = string.Empty;
											}
											i++;
										}
										if (level != 0) {
											Interface.AddMessage(MessageType.Error, false, "$EndIf missing at the end of the file" + Epilog);
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "$Else without matching $If encountered" + Epilog);
									}
									continueWithNextExpression = true;
									break;
								case "$endif":
									Expressions[i].Text = string.Empty;
									if (openIfs != 0) {
										openIfs--;
									} else {
										Interface.AddMessage(MessageType.Error, false, "$EndIf without matching $If encountered" + Epilog);
									}
									continueWithNextExpression = true;
									break;
								case "$include":
									if (j != 0) {
										Interface.AddMessage(MessageType.Error, false, "The $Include directive must not appear within another statement" + Epilog);
										continueWithNextExpression = true;
										break;
									}
									string[] args = s.Split(';');
									for (int ia = 0; ia < args.Length; ia++) {
										args[ia] = args[ia].Trim();
									}
									int count = (args.Length + 1) / 2;
									string[] files = new string[count];
									double[] weights = new double[count];
									double[] offsets = new double[count];
									double weightsTotal = 0.0;
									for (int ia = 0; ia < count; ia++) {
										string file;
										double offset;
										int colon = args[2 * ia].IndexOf(':');
										if (colon >= 0) {
											file = args[2 * ia].Substring(0, colon).TrimEnd();
											string value = args[2 * ia].Substring(colon + 1).TrimStart();
											if (!double.TryParse(value, NumberStyles.Float, Culture, out offset)) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "The track position offset " + value + " is invalid in " + t + Epilog);
												break;
											}
										} else {
											file = args[2 * ia];
											offset = 0.0;
										}
										files[ia] = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), file);
										offsets[ia] = offset;
										if (!System.IO.File.Exists(files[ia])) {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "The file " + file + " could not be found in " + t + Epilog);
											for (int ta = i; ta < Expressions.Length - 1; ta++)
											{
												Expressions[ta] = Expressions[ta + 1];
											}
											Array.Resize<Expression>(ref Expressions, Expressions.Length - 1);
											i--;
											break;
										}
										if (2 * ia + 1 < args.Length)
										{
											if (!NumberFormats.TryParseDoubleVb6(args[2 * ia + 1], out weights[ia])) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "A weight is invalid in " + t + Epilog);
												break;
											}
											if (weights[ia] <= 0.0) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "A weight is not positive in " + t + Epilog);
												break;
											}
											weightsTotal += weights[ia];
										}
										else {
											weights[ia] = 1.0;
											weightsTotal += 1.0;
										}
									}
									if (count == 0) {
										continueWithNextExpression = true;
										Interface.AddMessage(MessageType.Error, false, "No file was specified in " + t + Epilog);
										break;
									}
									if (!continueWithNextExpression) {
										double number = Program.RandomNumberGenerator.NextDouble() * weightsTotal;
										double value = 0.0;
										int chosenIndex = 0;
										for (int ia = 0; ia < count; ia++) {
											value += weights[ia];
											if (value > number) {
												chosenIndex = ia;
												break;
											}
										}
										Expression[] expr;
										//Get the text encoding of our $Include file
										System.Text.Encoding includeEncoding = TextEncoding.GetSystemEncodingFromFile(files[chosenIndex]);
										if (!includeEncoding.Equals(Encoding) && includeEncoding.WindowsCodePage != Encoding.WindowsCodePage)
										{
											//If the encodings do not match, add a warning
											//This is not critical, but it's a bad idea to mix and match character encodings within a routefile, as the auto-detection may sometimes be wrong
											Interface.AddMessage(MessageType.Warning, false, "The text encoding of the $Include file " + files[chosenIndex] + " does not match that of the base routefile.");
										}
										string[] lines = System.IO.File.ReadAllLines(files[chosenIndex], includeEncoding);
										PreprocessSplitIntoExpressions(files[chosenIndex], IsRW, lines, out expr, false, offsets[chosenIndex] + Expressions[i].TrackPositionOffset);
										int length = Expressions.Length;
										if (expr.Length == 0) {
											for (int ia = i; ia < Expressions.Length - 1; ia++) {
												Expressions[ia] = Expressions[ia + 1];
											}
											Array.Resize<Expression>(ref Expressions, length - 1);
										} else {
											Array.Resize<Expression>(ref Expressions, length + expr.Length - 1);
											for (int ia = Expressions.Length - 1; ia >= i + expr.Length; ia--) {
												Expressions[ia] = Expressions[ia - expr.Length + 1];
											}
											for (int ia = 0; ia < expr.Length; ia++) {
												Expressions[i + ia] = expr[ia];
											}
										}
										i--;
										continueWithNextExpression = true;
									}
									break;
								case "$chr":
								case "$chruni":
									{
										int x;
										if (NumberFormats.TryParseIntVb6(s, out x)) {
											if (x < 0)
											{
												//Must be non-negative
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index must be a non-negative character in " + t + Epilog);
											}
											else
											{
												Expressions[i].Text = Expressions[i].Text.Substring(0, j) + char.ConvertFromUtf32(x) + Expressions[i].Text.Substring(h + 1);
											}
										}
										else {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
										}
									} break;
								case "$chrascii":
								{
									int x;
									if (NumberFormats.TryParseIntVb6(s, out x))
									{
										if (x < 0 || x > 128)
										{
											//Standard ASCII characters from 0-128
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Index does not correspond to a valid ASCII character in " + t + Epilog);
										}
										else
										{
											Expressions[i].Text = Expressions[i].Text.Substring(0, j) + char.ConvertFromUtf32(x) + Expressions[i].Text.Substring(h + 1);
										}
									}
									else
									{
										continueWithNextExpression = true;
										Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
									}
								}
									break;
								case "$rnd":
									{
										int m = s.IndexOf(";", StringComparison.Ordinal);
										if (m >= 0) {
											string s1 = s.Substring(0, m).TrimEnd();
											string s2 = s.Substring(m + 1).TrimStart();
											int x; if (NumberFormats.TryParseIntVb6(s1, out x)) {
												int y; if (NumberFormats.TryParseIntVb6(s2, out y)) {
													int z = x + (int)Math.Floor(Program.RandomNumberGenerator.NextDouble() * (double)(y - x + 1));
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + z.ToString(Culture) + Expressions[i].Text.Substring(h + 1);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index2 is invalid in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index1 is invalid in " + t + Epilog);
											}
										} else {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + t + Epilog);
										}
									} break;
								case "$sub":
									{
										l = 0;
										bool f = false;
										int m;
										for (m = h + 1; m < Expressions[i].Text.Length; m++) {
											switch (Expressions[i].Text[m]) {
													case '(': l++; break;
													case ')': l--; break;
													case '=': if (l == 0) {
														f = true;
													}
													break;
												default:
													if (!char.IsWhiteSpace(Expressions[i].Text[m])) l = -1;
													break;
											}
											if (f | l < 0) break;
										}
										if (f) {
											l = 0;
											int n;
											for (n = m + 1; n < Expressions[i].Text.Length; n++) {
												switch (Expressions[i].Text[n]) {
														case '(': l++; break;
														case ')': l--; break;
												}
												if (l < 0) break;
											}
											int x;
											if (NumberFormats.TryParseIntVb6(s, out x)) {
												if (x >= 0) {
													while (x >= Subs.Length) {
														Array.Resize<string>(ref Subs, Subs.Length << 1);
													}
													Subs[x] = Expressions[i].Text.Substring(m + 1, n - m - 1).Trim();
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Expressions[i].Text.Substring(n);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index is expected to be non-negative in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
											}
										} else {
											int x;
											if (NumberFormats.TryParseIntVb6(s, out x)) {
												if (x >= 0 & x < Subs.Length && Subs[x] != null) {
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Subs[x] + Expressions[i].Text.Substring(h + 1);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index is out of range in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
											}
										}
										
									}
									break;
							}
						}
					}
					if (continueWithNextExpression) {
						break;
					}
				}
			}
			// handle comments introduced via chr, rnd, sub
			{
				int length = Expressions.Length;
				for (int i = 0; i < length; i++) {
					Expressions[i].Text = Expressions[i].Text.Trim();
					if (Expressions[i].Text.Length != 0) {
						if (Expressions[i].Text[0] == ';') {
							for (int j = i; j < length - 1; j++) {
								Expressions[j] = Expressions[j + 1];
							}
							length--;
							i--;
						}
					} else {
						for (int j = i; j < length - 1; j++) {
							Expressions[j] = Expressions[j + 1];
						}
						length--;
						i--;
					}
				}
				if (length != Expressions.Length) {
					Array.Resize<Expression>(ref Expressions, length);
				}
			}
		}

		// preprocess options
		

		// preprocess sort by track position
		private struct PositionedExpression {
			internal double TrackPosition;
			internal Expression Expression;
		}
		private static void PreprocessSortByTrackPosition(bool IsRW, double[] UnitFactors, ref Expression[] Expressions) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			PositionedExpression[] p = new PositionedExpression[Expressions.Length];
			int n = 0;
			double a = -1.0;
			bool NumberCheck = !IsRW;
			for (int i = 0; i < Expressions.Length; i++) {
				if (IsRW) {
					// only check for track positions in the railway section for RW routes
					if (Expressions[i].Text.StartsWith("[", StringComparison.Ordinal) & Expressions[i].Text.EndsWith("]", StringComparison.Ordinal)) {
						string s = Expressions[i].Text.Substring(1, Expressions[i].Text.Length - 2).Trim();
						if (string.Compare(s, "Railway", StringComparison.OrdinalIgnoreCase) == 0) {
							NumberCheck = true;
						} else {
							NumberCheck = false;
						}
					}
				}
				double x;
				if (NumberCheck && NumberFormats.TryParseDouble(Expressions[i].Text, UnitFactors, out x)) {
					x += Expressions[i].TrackPositionOffset;
					if (x >= 0.0) {
						a = x;
					} else {
						Interface.AddMessage(MessageType.Error, false, "Negative track position encountered at line " + Expressions[i].Line.ToString(Culture) + ", column " + Expressions[i].Column.ToString(Culture) + " in file " + Expressions[i].File);
					}
				} else {
					p[n].TrackPosition = a;
					p[n].Expression = Expressions[i];
					int j = n;
					n++;
					while (j > 0) {
						if (p[j].TrackPosition < p[j - 1].TrackPosition) {
							PositionedExpression t = p[j];
							p[j] = p[j - 1];
							p[j - 1] = t;
							j--;
						} else {
							break;
						}
					}
				}
			}
			a = -1.0;
			Expression[] e = new Expression[Expressions.Length];
			int m = 0;
			for (int i = 0; i < n; i++) {
				if (p[i].TrackPosition != a) {
					a = p[i].TrackPosition;
					e[m] = new Expression();
					e[m].Text = (a / UnitFactors[UnitFactors.Length - 1]).ToString(Culture);
					e[m].Line = -1;
					e[m].Column = -1;
					m++;
				}
				e[m] = p[i].Expression;
				m++;
			}
			Array.Resize<Expression>(ref e, m);
			Expressions = e;
		}

		// separate commands and arguments
		

		private static int freeObjCount = 0;
		private static int railtypeCount = 0;

		// parse route for data
		private static void ParseRouteForData(string FileName, bool IsRW, System.Text.Encoding Encoding, Expression[] Expressions, double[] UnitOfLength, ref RouteData Data, bool PreviewOnly) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Section = ""; bool SectionAlwaysPrefix = false;
			int BlockIndex = 0;
			int BlocksUsed = Data.Blocks.Length;
			Game.Stations = new Game.Station[] { };
			Data.RequestStops = new StopRequest[] { };
			int CurrentStation = -1;
			int CurrentStop = -1;
			bool DepartureSignalUsed = false;
			int CurrentSection = 0;
			bool ValueBasedSections = false;
			double progressFactor = Expressions.Length == 0 ? 0.3333 : 0.3333 / (double)Expressions.Length;
			// process non-track namespaces
			//Check for any special-cased fixes we might need
			CheckRouteSpecificFixes(FileName, ref Data, ref Expressions);
			for (int j = 0; j < Expressions.Length; j++) {
				Loading.RouteProgress = (double)j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					// find equals
					Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					// separate command and arguments
					string Command, ArgumentSequence;
					Expressions[j].SeparateCommandsAndArguments(out Command, out ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					double Number;
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.TryParseDouble(Command, UnitOfLength, out Number)) {
						// track position (ignored)
					} else {
						// split arguments
						string[] Arguments;
						{
							int n = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++) {
								if (IsRW & ArgumentSequence[k] == ',') {
									n++;
								} else if (ArgumentSequence[k] == ';') {
									n++;
								}
							}
							Arguments = new string[n + 1];
							int a = 0, h = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++) {
								if (IsRW & ArgumentSequence[k] == ',') {
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								} else if (ArgumentSequence[k] == ';') {
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								}
							}
							if (ArgumentSequence.Length - a > 0) {
								Arguments[h] = ArgumentSequence.Substring(a).Trim();
								h++;
							}
							Array.Resize<string>(ref Arguments, h);
						}
						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							if (Arguments.Length >= 1) {
								Section = Arguments[0];
								SectionAlwaysPrefix = false;
							} else {
								Section = "";
								SectionAlwaysPrefix = false;
							}
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
							if (Command.StartsWith("structure", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.x" + Command.Substring(18, Command.Length - 20).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.aspect" + Command.Substring(18, Command.Length - 25).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.x" + Command.Substring(14, Command.Length - 16).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.aspect" + Command.Substring(14, Command.Length - 21).TrimEnd();
							} else if (Command.StartsWith("cycle", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".params", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 7).TrimEnd();
							} else if (Command.StartsWith("signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("train.run", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.flange", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 24).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 26).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 19).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 21).Trim();
							} else if (Command.StartsWith("route.signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							}
						}
						// handle indices
						int CommandIndex1 = 0, CommandIndex2 = 0;
						if (Command != null && Command.EndsWith(")")) {
							for (int k = Command.Length - 2; k >= 0; k--) {
								if (Command[k] == '(') {
									string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
									Command = Command.Substring(0, k).TrimEnd();
									int h = Indices.IndexOf(";", StringComparison.Ordinal);
									if (h >= 0) {
										string a = Indices.Substring(0, h).TrimEnd();
										string b = Indices.Substring(h + 1).TrimStart();
										if (a.Length > 0 && !NumberFormats.TryParseIntVb6(a, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid first index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										} 
										if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid second index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										}
									} else {
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										}
									}
									break;
								}
							}
						}
						// process command
						if (!string.IsNullOrEmpty(Command)) {
							switch (Command.ToLowerInvariant()) {
									// options
								case "options.blocklength":
									{
										double length = 25.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out length)) {
											Interface.AddMessage(MessageType.Error, false, "Length is invalid in Options.BlockLength at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											length = 25.0;
										}
										Data.BlockInterval = length;
									} break;
								case "options.xparser":
								if(!PreviewOnly){
									int parser = 0;
									if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 3) {
										Interface.AddMessage(MessageType.Error, false, "XParser is invalid in Options.XParser at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										Interface.CurrentOptions.CurrentXParser = (Interface.XParsers)parser;
									}
								} break;
								case "options.objparser":
									if(!PreviewOnly){
										int parser = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out parser) | parser < 0 | parser > 2) {
											Interface.AddMessage(MessageType.Error, false, "ObjParser is invalid in Options.ObjParser at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											Interface.CurrentOptions.CurrentObjParser = (Interface.ObjParsers)parser;
										}
									} break;
								case "options.unitoflength":
								case "options.unitofspeed":
								case "options.objectvisibility":
									break;
								case "options.sectionbehavior":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										int a;
										if (!NumberFormats.TryParseIntVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a != 0 & a != 1) {
											Interface.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											ValueBasedSections = a == 1;
										}
									} break;
								case "options.cantbehavior":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										int a;
										if (!NumberFormats.TryParseIntVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a != 0 & a != 1) {
											Interface.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Data.SignedCant = a == 1;
										}
									} break;
								case "options.fogbehavior":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										int a;
										if (!NumberFormats.TryParseIntVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a != 0 & a != 1) {
											Interface.AddMessage(MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Data.FogTransitionMode = a == 1;
										}
									} break;
									// route
								case "route.comment":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										Game.RouteComment = Arguments[0];
									} break;
								case "route.image":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
										if (!System.IO.File.Exists(f)) {
											Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteImage = f;
										}
									} break;
								case "route.timetable":
									if (!PreviewOnly) {
										if (Arguments.Length < 1) {
											Interface.AddMessage(MessageType.Error, false, "" + Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Timetable.DefaultTimetableDescription = Arguments[0];
										}
									} break;
								case "route.change":
									if (!PreviewOnly) {
										int change = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out change)) {
											Interface.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											change = 0;
										} else if (change < -1 | change > 1) {
											Interface.AddMessage(MessageType.Error, false, "Mode is expected to be -1, 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											change = 0;
										}
										Game.TrainStart = (TrainStartMode)change;
									} break;
								case "route.gauge":
								case "train.gauge":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										double a;
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "ValueInMillimeters is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a <= 0.0) {
											Interface.AddMessage(MessageType.Error, false, "ValueInMillimeters is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteRailGauge = 0.001 * a;
										}
									} break;
								case "route.signal":
									if (!PreviewOnly) {
										if (Arguments.Length < 1) {
											Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											double a;
											if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
												Interface.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 < 0) {
													Interface.AddMessage(MessageType.Error, false, "AspectIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (a < 0.0) {
													Interface.AddMessage(MessageType.Error, false, "Speed is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 >= Data.SignalSpeeds.Length) {
														int n = Data.SignalSpeeds.Length;
														Array.Resize<double>(ref Data.SignalSpeeds, CommandIndex1 + 1);
														for (int i = n; i < CommandIndex1; i++) {
															Data.SignalSpeeds[i] = double.PositiveInfinity;
														}
													}
													Data.SignalSpeeds[CommandIndex1] = a * Data.UnitOfSpeed;
												}
											}
										}
									} break;
								case "route.runinterval":
								case "train.interval":
									{
										if (!PreviewOnly) {
											List<double> intervals = new List<double>();
											for (int k = 0; k < Arguments.Length; k++)
											{
												double o;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[k], out o)) {
													Interface.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													continue;
												}
												if (o == 0)
												{
													Interface.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " must be non-zero in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													continue;
												}
												if (o > 43200 && Interface.CurrentOptions.EnableBveTsHacks)
												{
													//Southern Blighton- Treston park has a runinterval of well over 24 hours, and there are likely others
													//Discard this
													Interface.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " is greater than 12 hours in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													continue;
												}
												intervals.Add(o);
											}
											intervals.Sort();
											if (intervals.Count > 0)
											{
												Game.PrecedingTrainTimeDeltas = intervals.ToArray();
											}
										}
									} break;
								case "train.velocity":
									{
										if (!PreviewOnly) {
											double limit = 0.0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out limit)) {
												Interface.AddMessage(MessageType.Error, false, "Speed is invalid in Train.Velocity at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												limit = 0.0;
											}
											Game.PrecedingTrainSpeedLimit = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
										}
									} break;
								case "route.accelerationduetogravity":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										double a;
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a <= 0.0) {
											Interface.AddMessage(MessageType.Error, false, "Value is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteAccelerationDueToGravity = a;
										}
									} break;
								//Sets the time the game will start at
								case "route.starttime":
									if (Arguments.Length < 1)
									{
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										double t;
										if(!Interface.TryParseTime(Arguments[0], out t))
										{
											Interface.AddMessage(MessageType.Error, false, Arguments[0] + " does not parse to a valid time in command "+ Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											if (Game.InitialStationTime == -1)
											{
												Game.InitialStationTime = t;
											}
										}
									}
									break;
								//Sets the route's loading screen texture
								case "route.loadingscreen":
									if (Arguments.Length < 1)
									{
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
									if (!System.IO.File.Exists (f))
									{
										Interface.AddMessage (MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions [j].Line.ToString (Culture) + ", column " + Expressions [j].Column.ToString (Culture) + " in file " + Expressions [j].File);
									}
									else
										Renderer.SetLoadingBkg(f);
									}
									break;
								//Sets a custom unit of speed to to displayed in on-screen messages
								case "route.displayspeed":
								   var splitArgument = Arguments[0].Split(',');
									if (splitArgument.Length != 2)
									{
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have two arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										break;
									}
									Game.UnitOfSpeed = splitArgument[0];
									if (!double.TryParse(splitArgument[1], NumberStyles.Float, Culture, out Game.SpeedConversionFactor))
									{
										Interface.AddMessage(MessageType.Error, false,"Speed conversion factor is invalid in " + Command + " at line " +Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) +" in file " + Expressions[j].File);
										Game.UnitOfSpeed = "km/h";
									}

									break;
								//Sets the route's briefing data
								case "route.briefing":
									if (Arguments.Length < 1)
									{
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
										if (!System.IO.File.Exists(f))
										{
											Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											Game.RouteInformation.RouteBriefing = f;
										}
									}
									break;
								case "route.elevation":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										double a;
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out a)) {
											Interface.AddMessage(MessageType.Error, false, "Height is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteInitialElevation = a;
										}
									} break;
								case "route.temperature":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										double a;
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "ValueInCelsius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a <= -273.15) {
											Interface.AddMessage(MessageType.Error, false, "ValueInCelsius is expected to be greater than to -273.15 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteInitialAirTemperature = a + 273.15;
										}
									} break;
								case "route.pressure":
									if (Arguments.Length < 1) {
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										double a;
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "ValueInKPa is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (a <= 0.0) {
											Interface.AddMessage(MessageType.Error, false, "ValueInKPa is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											Game.RouteInitialAirPressure = 1000.0 * a;
										}
									} break;
								case "route.ambientlight":
									{
										if (Renderer.DynamicLighting == true)
										{
											Interface.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.AmbientLight will be ignored");
											break;
										}
										int r = 255, g = 255, b = 255;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
											Interface.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (r < 0 | r > 255) {
											Interface.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											r = r < 0 ? 0 : 255;
										}
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
											Interface.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (g < 0 | g > 255) {
											Interface.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											g = g < 0 ? 0 : 255;
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
											Interface.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (b < 0 | b > 255) {
											Interface.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											b = b < 0 ? 0 : 255;
										}
										Renderer.OptionAmbientColor = new Color24((byte)r, (byte)g, (byte)b);
									} break;
								case "route.directionallight":
									{
										if (Renderer.DynamicLighting == true)
										{
											Interface.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.DirectionalLight will be ignored");
											break;
										}
										int r = 255, g = 255, b = 255;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
											Interface.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (r < 0 | r > 255) {
											Interface.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											r = r < 0 ? 0 : 255;
										}
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
											Interface.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (g < 0 | g > 255) {
											Interface.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											g = g < 0 ? 0 : 255;
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
											Interface.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else if (b < 0 | b > 255) {
											Interface.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											b = b < 0 ? 0 : 255;
										}
										Renderer.OptionDiffuseColor = new Color24((byte)r, (byte)g, (byte)b);
									}
									break;
								case "route.lightdirection":
									{
										if (Renderer.DynamicLighting == true)
										{
											Interface.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.LightDirection will be ignored");
											break;
										}
										double theta = 60.0, phi = -26.565051177078;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out theta)) {
											Interface.AddMessage(MessageType.Error, false, "Theta is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out phi)) {
											Interface.AddMessage(MessageType.Error, false, "Phi is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										theta *= 0.0174532925199433;
										phi *= 0.0174532925199433;
										double dx = Math.Cos(theta) * Math.Sin(phi);
										double dy = -Math.Sin(theta);
										double dz = Math.Cos(theta) * Math.Cos(phi);
										Renderer.OptionLightPosition = new Vector3((float)-dx, (float)-dy, (float)-dz);
									} break;
								case "route.dynamiclight":
									//Read the lighting XML file
									string path = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
									if (System.IO.File.Exists(path))
									{
										if (DynamicLightParser.ReadLightingXML(path))
										{
											Renderer.DynamicLighting = true;
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "The file " + path + " is not a valid dynamic lighting XML file, at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, "Dynamic lighting XML file not found at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "route.initialviewpoint":
									if (Arguments.Length < 1)
									{
										Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										int cv = -1;
										if (!NumberFormats.TryParseIntVb6(Arguments[0], out cv))
										{
											switch (Arguments[0].ToLowerInvariant())
											{
												case "cab":
													cv = 0;
													break;
												case "exterior":
													cv = 1;
													break;
												case "track":
													cv = 2;
													break;
												case "flyby":
													cv = 3;
													break;
												case "flybyzooming":
													cv = 4;
													break;
												default:
													cv = 0;
													Interface.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													break;
											}
										}

										if (cv >= 0 && cv < 4)
										{
											Game.InitialViewpoint = cv;
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
									}
									break;
									// train
								case "train.folder":
								case "train.file":
									{
										if (PreviewOnly) {
											if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FolderName contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													Game.TrainName = Arguments[0];
												}
											}
										}
									} break;
								case "train.run":
								case "train.rail":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailTypeIndex is out of range in "+Command+" at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												int val = 0;
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out val)) {
													Interface.AddMessage(MessageType.Error, false, "RunSoundIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													val = 0;
												}
												if (val < 0) {
													Interface.AddMessage(MessageType.Error, false, "RunSoundIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													val = 0;
												}
												if (CommandIndex1 >= Data.Structure.Run.Length) {
													Array.Resize<int>(ref Data.Structure.Run, CommandIndex1 + 1);
												}
												Data.Structure.Run[CommandIndex1] = val;
											}
										}
										else
										{
											railtypeCount++;
										}
									} break;
								case "train.flange":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailTypeIndex is out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												int val = 0;
												if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out val)) {
													Interface.AddMessage(MessageType.Error, false, "FlangeSoundIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													val = 0;
												}
												if (val < 0) {
													Interface.AddMessage(MessageType.Error, false, "FlangeSoundIndex expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													val = 0;
												}
												if (CommandIndex1 >= Data.Structure.Flange.Length) {
													Array.Resize<int>(ref Data.Structure.Flange, CommandIndex1 + 1);
												}
												Data.Structure.Flange[CommandIndex1] = val;
											}
										}
									} break;
								case "train.timetable.day":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													while (CommandIndex1 >= Data.TimetableDaytime.Length) {
														int n = Data.TimetableDaytime.Length;
														Array.Resize<OpenBveApi.Textures.Texture>(ref Data.TimetableDaytime, n << 1);
														for (int i = n; i < Data.TimetableDaytime.Length; i++) {
															Data.TimetableDaytime[i] = null;
														}
													}
													string f = OpenBveApi.Path.CombineFile(TrainPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													}
													if (System.IO.File.Exists(f)) {
														Textures.RegisterTexture(f, out Data.TimetableDaytime[CommandIndex1]);
													}
												}
											}
										}
									} break;
								case "train.timetable.night":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													while (CommandIndex1 >= Data.TimetableNighttime.Length) {
														int n = Data.TimetableNighttime.Length;
														Array.Resize<OpenBveApi.Textures.Texture>(ref Data.TimetableNighttime, n << 1);
														for (int i = n; i < Data.TimetableNighttime.Length; i++) {
															Data.TimetableNighttime[i] = null;
														}
													}
													string f = OpenBveApi.Path.CombineFile(TrainPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													}
													if (System.IO.File.Exists(f)) {
														Textures.RegisterTexture(f, out Data.TimetableNighttime[CommandIndex1]);
													}
												}
											}
										}
									} break;
								case "train.destination":
									{
										if (!PreviewOnly)
										{
											if (Arguments.Length < 1)
											{
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											else
											{
												if (!NumberFormats.TryParseIntVb6(Arguments[0], out Game.InitialDestination))
												{
													Interface.AddMessage(MessageType.Error, false, "Destination is expected to be an Integer in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
											}
										}
									}
									break;
									// structure
								case "structure.rail":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0]; 
													if(!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.RailObjects.Add(CommandIndex1, obj, "RailStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.beacon":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.Beacon.Add(CommandIndex1, obj, "BeaconStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.pole":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "AdditionalRailsCovered is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (CommandIndex2 < 0) {
												Interface.AddMessage(MessageType.Error, false, "PoleStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 >= Data.Structure.Poles.Length) {
														Array.Resize<UnifiedObject[]>(ref Data.Structure.Poles, CommandIndex1 + 1);
													}
													if (Data.Structure.Poles[CommandIndex1] == null) {
														Data.Structure.Poles[CommandIndex1] = new UnifiedObject[CommandIndex2 + 1];
													} else if (CommandIndex2 >= Data.Structure.Poles[CommandIndex1].Length) {
														Array.Resize<UnifiedObject>(ref Data.Structure.Poles[CommandIndex1], CommandIndex2 + 1);
													}
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														Data.Structure.Poles[CommandIndex1][CommandIndex2] = ObjectManager.LoadObject(f, Encoding, false, false, false);
													}
												}
											}
										}
									} break;
								case "structure.ground":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "GroundStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.Ground.Add(CommandIndex1, obj, "GroundStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.walll":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left WallStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.WallL.Add(CommandIndex1, obj, "Left WallStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.wallr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right WallStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.WallR.Add(CommandIndex1, obj, "Right WallStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.dikel":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left DikeStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.DikeL.Add(CommandIndex1, obj, "Left DikeStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.diker":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right DikeStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.DikeR.Add(CommandIndex1, obj, "Right DikeStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.forml":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left FormStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.FormL.Add(CommandIndex1, obj, "Left FormStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.formr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right FormStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.FormR.Add(CommandIndex1, obj, "Right FormStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.formcl":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left FormCStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadStaticObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.FormCL.Add(CommandIndex1, obj, "Left FormCStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.formcr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right FormCStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadStaticObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.FormCR.Add(CommandIndex1, obj, "Right FormCStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.roofl":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left RoofStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 == 0) {
														if (!IsRW)
														{
															Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														CommandIndex1 = 1;
													}
													if (CommandIndex1 < 0) {
														Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														string f = Arguments[0];
														if (!LocateObject(ref f, ObjectPath))
														{
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
															if (obj != null)
															{
																Data.Structure.RoofL.Add(CommandIndex1, obj, "Left RoofStructure");
															}
														}
													}
												}
											}
										}
									} break;
								case "structure.roofr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right RoofStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 == 0) {
														if (!IsRW)
														{
															Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														CommandIndex1 = 1;
													}
													if (CommandIndex1 < 0) {
														Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														string f = Arguments[0];
														if (!LocateObject(ref f, ObjectPath))
														{
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
															if (obj != null)
															{
																Data.Structure.RoofR.Add(CommandIndex1, obj, "Right RoofStructure");
															}
														}
													}
												}
											}
										}
									} break;
								case "structure.roofcl":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left RoofCStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 == 0) {
														if (!IsRW)
														{
															Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														CommandIndex1 = 1;
													}
													if (CommandIndex1 < 0) {
														Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														string f = Arguments[0];
														if (!LocateObject(ref f, ObjectPath))
														{
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															var obj = ObjectManager.LoadStaticObject(f, Encoding, false, false, false);
															if (obj != null)
															{
																Data.Structure.RoofCL.Add(CommandIndex1, obj, "Left RoofCStructure");
															}
														}
													}
												}
											}
										}
									} break;
								case "structure.roofcr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right RoofCStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 == 0) {
														if (!IsRW)
														{
															Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														CommandIndex1 = 1;
													}
													if (CommandIndex1 < 0) {
														Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														string f = Arguments[0];
														if (!LocateObject(ref f, ObjectPath))
														{
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															var obj = ObjectManager.LoadStaticObject(f, Encoding, false, false, false);
															if (obj != null)
															{
																Data.Structure.RoofCR.Add(CommandIndex1, obj, "Right RoofCStructure");
															}
														}
													}
												}
											}
										}
									} break;
								case "structure.crackl":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Left CrackStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadStaticObject(f, Encoding, true, false, false);
														if (obj != null)
														{
															Data.Structure.CrackL.Add(CommandIndex1, obj, "Left CrackStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.crackr":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "Right CrackStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadStaticObject(f, Encoding, true, false, false);
														if (obj != null)
														{
															Data.Structure.CrackR.Add(CommandIndex1, obj, "Right CrackStructure");
														}
													}
												}
											}
										}
									} break;
								case "structure.freeobj":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Arguments.Length < 1) {
													Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateObject(ref f, ObjectPath))
													{
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " could not be found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														var obj = ObjectManager.LoadObject(f, Encoding, false, false, false);
														if (obj != null)
														{
															Data.Structure.FreeObjects.Add(CommandIndex1, obj, "FreeObject");
														}
													}
												}
											}
										}
										else
										{
											freeObjCount++;
										}
									} break;
									// signal
								case "signal":
									{
										if (!PreviewOnly) {
											if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.Signals.Length) {
													Array.Resize<SignalData>(ref Data.Signals, CommandIndex1 + 1);
												}
												if (Arguments[0].EndsWith(".animated", StringComparison.OrdinalIgnoreCase)) {
													if (Path.ContainsInvalidChars(Arguments[0])) {
														Interface.AddMessage(MessageType.Error, false, "AnimatedObjectFile contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														if (Arguments.Length > 1) {
															Interface.AddMessage(MessageType.Warning, false, Command + " is expected to have exactly 1 argument when using animated objects at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f)) {
															Interface.AddMessage(MessageType.Error, true, "SignalFileWithoutExtension " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															UnifiedObject Object = ObjectManager.LoadObject(f, Encoding, false, false, false);
															if (Object is ObjectManager.AnimatedObjectCollection) {
																AnimatedObjectSignalData Signal = new AnimatedObjectSignalData();
																Signal.Objects = (ObjectManager.AnimatedObjectCollection)Object;
																Data.Signals[CommandIndex1] = Signal;
															} else {
																Interface.AddMessage(MessageType.Error, true, "GlowFileWithoutExtension " + f + " is not a valid animated object in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															}
														}
													}
												} else {
													if (Path.ContainsInvalidChars(Arguments[0])) {
														Interface.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													else {
														if (Arguments.Length > 2) {
															Interface.AddMessage(MessageType.Warning, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														string f = Arguments[0];
														try
														{
															LocateObject(ref f, ObjectPath);
														}
														catch
														{
															//NYCT-1 line has a comment containing SIGNAL, which is then misinterpreted by the parser here
															//Really needs commenting fixing, rather than hacks like this.....
															Interface.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not contain a valid path in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															break;
														}
														if (!System.IO.File.Exists(f) && !System.IO.Path.HasExtension(f))
														{
															string ff;
															bool notFound = false;
															while (true)
															{
																ff = Path.CombineFile(ObjectPath, f + ".x");
																if (System.IO.File.Exists(ff))
																{
																	f = ff;
																	break;
																}
																ff = Path.CombineFile(ObjectPath, f + ".csv");
																if (System.IO.File.Exists(ff))
																{
																	f = ff;
																	break;
																}
																ff = Path.CombineFile(ObjectPath, f + ".b3d");
																if (System.IO.File.Exists(ff))
																{
																	f = ff;
																	break;
																}
																Interface.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension does not exist in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																notFound = true;
																break;
															}
															if (notFound)
															{
																break;
															}
														}
														Bve4SignalData Signal = new Bve4SignalData
														{
															BaseObject = ObjectManager.LoadStaticObject(f, Encoding, false, false, false),
															GlowObject = null
														};
														string Folder = System.IO.Path.GetDirectoryName(f);
														if (!System.IO.Directory.Exists(Folder)) {
															Interface.AddMessage(MessageType.Error, true, "The folder " + Folder + " could not be found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															Signal.SignalTextures = LoadAllTextures(f, false);
															Signal.GlowTextures = new OpenBveApi.Textures.Texture[] { };
															if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
																if (Path.ContainsInvalidChars(Arguments[1])) {
																	Interface.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																} else
																{
																	f = Arguments[1];
																	bool glowFileFound = false;
																	if (!System.IO.File.Exists(f) && System.IO.Path.HasExtension(f))
																	{
																		string ext = System.IO.Path.GetExtension(f);
																		switch (ext.ToLowerInvariant())
																		{
																			case ".csv":
																			case ".b3d":
																			case ".x":
																				Interface.AddMessage(MessageType.Warning, false, "GlowFileWithoutExtension should not supply a file extension in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																				f = Path.CombineFile(ObjectPath, f);
																				glowFileFound = true;
																				break;
																			case ".animated":
																			case ".s":
																				Interface.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension must be a static object in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																				break;
																			default:
																				Interface.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																				break;
																		}
																		
																	}
																	if (!System.IO.File.Exists(f) && !System.IO.Path.HasExtension(f))
																	{
																		string ff;
																		while (true)
																		{
																			ff = Path.CombineFile(ObjectPath, f + ".x");
																			if (System.IO.File.Exists(ff))
																			{
																				f = ff;
																				glowFileFound = true;
																				break;
																			}
																			ff = Path.CombineFile(ObjectPath, f + ".csv");
																			if (System.IO.File.Exists(ff))
																			{
																				f = ff;
																				glowFileFound = true;
																				break;
																			}
																			ff = Path.CombineFile(ObjectPath, f + ".b3d");
																			if (System.IO.File.Exists(ff))
																			{
																				f = ff;
																				glowFileFound = true;
																				break;
																			}
																			Interface.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension does not exist in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																			break;
																		}
																	}
																	if (glowFileFound)
																	{
																		Signal.GlowObject = ObjectManager.LoadStaticObject(f, Encoding, false, false, false);
																		if (Signal.GlowObject != null)
																		{
																			Signal.GlowTextures = LoadAllTextures(f, true);
																			for (int p = 0; p < Signal.GlowObject.Mesh.Materials.Length; p++)
																			{
																				Signal.GlowObject.Mesh.Materials[p].BlendMode = MeshMaterialBlendMode.Additive;
																				Signal.GlowObject.Mesh.Materials[p].GlowAttenuationData = Glow.GetAttenuationData(200.0, GlowAttenuationMode.DivisionExponent4);
																			}
																		}
																	}
																}
															}
															Data.Signals[CommandIndex1] = Signal;
														}
													}
												}
											}
										}
									} break;
									// texture
								case "texture.background":
								case "structure.back":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (CommandIndex1 >= Data.Backgrounds.Length) {
														int a = Data.Backgrounds.Length;
														Array.Resize<BackgroundManager.BackgroundHandle>(ref Data.Backgrounds, CommandIndex1 + 1);
														for (int k = a; k <= CommandIndex1; k++) {
															Data.Backgrounds[k] = new BackgroundManager.StaticBackground(null, 6, false);
														}
													}
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f) && (Arguments[0].ToLowerInvariant() == "back_mt.bmp" || Arguments[0] == "back_mthigh.bmp")) {
														//Default background textures supplied with Uchibo for BVE1 / BVE2, so map to something that's not totally black
														f = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "Uchibo\\Back_Mt.png");
													}

													if (!System.IO.File.Exists(f) && Interface.CurrentOptions.EnableBveTsHacks)
													{
														if (Arguments[0].StartsWith("Midland Suburban Line", StringComparison.InvariantCultureIgnoreCase))
														{
															Arguments[0] = "Midland Suburban Line Objects" + Arguments[0].Substring(21);
															f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														}
													}
													if (!System.IO.File.Exists(f)) {														
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														if (f.ToLowerInvariant().EndsWith(".xml"))
														{
															try
															{
																BackgroundManager.BackgroundHandle h = DynamicBackgroundParser.ReadBackgroundXML(f);
																Data.Backgrounds[CommandIndex1] = h;
															}
															catch
															{
																Interface.AddMessage(MessageType.Error, true, f + " is not a valid background XML in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															}
														}
														else
														{
															if (Data.Backgrounds[CommandIndex1] is BackgroundManager.StaticBackground)
															{
																BackgroundManager.StaticBackground b = Data.Backgrounds[CommandIndex1] as BackgroundManager.StaticBackground;
																if (b != null)
																{
																	Textures.RegisterTexture(f, out b.Texture);
																}

															}
														}
													}
												}
											}
										}
									} break;
								case "texture.background.x":
								case "structure.back.x":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + CommandIndex1 + " is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false,  Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.Backgrounds.Length) {
													int a = Data.Backgrounds.Length;
													Array.Resize<BackgroundManager.BackgroundHandle>(ref Data.Backgrounds, CommandIndex1 + 1);
													for (int k = a; k <= CommandIndex1; k++) {
														Data.Backgrounds[k] = new BackgroundManager.StaticBackground(null, 6, false);
													}
												}
												int x;
												if (!NumberFormats.TryParseIntVb6(Arguments[0], out x)) {
													Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + Arguments[0] + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (x == 0) {
													Interface.AddMessage(MessageType.Error, false, "RepetitionCount is expected to be non-zero in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													BackgroundManager.StaticBackground b = Data.Backgrounds[CommandIndex1] as BackgroundManager.StaticBackground;
													if (b != null)
													{
														b.Repetition = x;
													}
												}
											}
										}
									} break;
								case "texture.background.aspect":
								case "structure.back.aspect":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + CommandIndex1 + " is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false,  Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.Backgrounds.Length) {
													int a = Data.Backgrounds.Length;
													Array.Resize<BackgroundManager.BackgroundHandle>(ref Data.Backgrounds, CommandIndex1 + 1);
													for (int k = a; k <= CommandIndex1; k++) {
														Data.Backgrounds[k] = new BackgroundManager.StaticBackground(null, 6, false);
													}
												}
												int aspect;
												if (!NumberFormats.TryParseIntVb6(Arguments[0], out aspect)) {
													Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + Arguments[0] + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (aspect != 0 & aspect != 1) {
													Interface.AddMessage(MessageType.Error, false, "Value is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													BackgroundManager.StaticBackground b = Data.Backgrounds[CommandIndex1] as BackgroundManager.StaticBackground;
													if (b != null)
													{
														b.KeepAspectRatio = aspect == 1;
													}
													
												}
											}
										}
									} break;
									// cycle
								case "cycle.ground":
									if (!PreviewOnly) {
										if (CommandIndex1 >= Data.Structure.Cycles.Length) {
											Array.Resize<int[]>(ref Data.Structure.Cycles, CommandIndex1 + 1);
										}
										Data.Structure.Cycles[CommandIndex1] = new int[Arguments.Length];
										for (int k = 0; k < Arguments.Length; k++) {
											int ix = 0;
											if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix)) {
												Interface.AddMessage(MessageType.Error, false, "GroundStructureIndex " + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											if (ix < 0 | !Data.Structure.Ground.ContainsKey(ix)) {
												Interface.AddMessage(MessageType.Error, false, "GroundStructureIndex " + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											Data.Structure.Cycles[CommandIndex1][k] = ix;
										}
									} break;
									// rail cycle
								case "cycle.rail":
									if (!PreviewOnly)
									{
										if (CommandIndex1 >= Data.Structure.RailCycles.Length)
										{
											Array.Resize<int[]>(ref Data.Structure.RailCycles, CommandIndex1 + 1);
										}
										Data.Structure.RailCycles[CommandIndex1] = new int[Arguments.Length];
										for (int k = 0; k < Arguments.Length; k++)
										{
											int ix = 0;
											if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex " + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											if (ix < 0 | !Data.Structure.RailObjects.ContainsKey(ix))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex " + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											Data.Structure.RailCycles[CommandIndex1][k] = ix;
										}
									} break;
							}
						}
					}
				}
			}
			
			// process track namespace
			for (int j = 0; j < Expressions.Length; j++) {
				Loading.RouteProgress = 0.3333 + (double)j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				if (Data.LineEndingFix)
				{
					if (Expressions[j].Text.EndsWith("_"))
					{
						Expressions[j].Text = Expressions[j].Text.Substring(0, Expressions[j].Text.Length - 1).Trim();
					}
				}
				if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					// separate command and arguments
					string Command, ArgumentSequence;
					Expressions[j].SeparateCommandsAndArguments(out Command, out ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					double Number;
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.TryParseDouble(Command, UnitOfLength, out Number)) {
						// track position
						if (ArgumentSequence.Length != 0) {
							Interface.AddMessage(MessageType.Error, false, "A track position must not contain any arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						} else if (Number < 0.0) {
							Interface.AddMessage(MessageType.Error, false, "Negative track position encountered at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						} else {
							if (Interface.CurrentOptions.EnableBveTsHacks && IsRW && Number == 4535545100)
							{
								//WMATA Red line has an erroneous track position causing an out of memory cascade
								Number = 45355;
							}
							Data.TrackPosition = Number;
							BlockIndex = (int)Math.Floor(Number / Data.BlockInterval + 0.001);
							if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
							Data.CreateMissingBlocks(ref BlocksUsed, BlockIndex, PreviewOnly);
						}
					} else {
						// split arguments
						string[] Arguments;
						{
							int n = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++) {
								if (IsRW & ArgumentSequence[k] == ',') {
									n++;
								} else if (ArgumentSequence[k] == ';') {
									n++;
								}
							}
							Arguments = new string[n + 1];
							int a = 0, h = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++) {
								if (IsRW & ArgumentSequence[k] == ',') {
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								} else if (ArgumentSequence[k] == ';') {
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								}
							}
							if (ArgumentSequence.Length - a > 0) {
								Arguments[h] = ArgumentSequence.Substring(a).Trim();
								h++;
							}
							Array.Resize<string>(ref Arguments, h);
						}
						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							if (Arguments.Length >= 1) {
								Section = Arguments[0];
								SectionAlwaysPrefix = false;
							} else {
								Section = "";
								SectionAlwaysPrefix = false;
							}
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
							if (Command.StartsWith("structure", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.x" + Command.Substring(18, Command.Length - 20).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.aspect" + Command.Substring(18, Command.Length - 25).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.x" + Command.Substring(14, Command.Length - 16).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase)) {
								Command = "texture.background.aspect" + Command.Substring(14, Command.Length - 21).TrimEnd();
							} else if (Command.StartsWith("cycle", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".params", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 7).TrimEnd();
							} else if (Command.StartsWith("signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("train.run", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.flange", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 24).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 26).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 19).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 21).Trim();
							} else if (Command.StartsWith("route.signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							}
						}
						// handle indices
						int CommandIndex1 = 0, CommandIndex2 = 0;
						if (Command != null && Command.EndsWith(")")) {
							for (int k = Command.Length - 2; k >= 0; k--) {
								if (Command[k] == '(') {
									string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
									Command = Command.Substring(0, k).TrimEnd();
									int h = Indices.IndexOf(";", StringComparison.Ordinal);
									if (h >= 0) {
										string a = Indices.Substring(0, h).TrimEnd();
										string b = Indices.Substring(h + 1).TrimStart();
										if (a.Length > 0 && !NumberFormats.TryParseIntVb6(a, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid first index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										} 
										if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid second index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										}
									} else {
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null;
										}
									}
									break;
								}
							}
						}
						// process command
						if (!string.IsNullOrEmpty(Command)) {
							switch (Command.ToLowerInvariant()) {
									// non-track
								case "options.blocklength":
								case "options.unitoflength":
								case "options.unitofspeed":
								case "options.objectvisibility":
								case "options.sectionbehavior":
								case "options.fogbehavior":
								case "options.cantbehavior":
								case "route.comment":
								case "route.image":
								case "route.timetable":
								case "route.change":
								case "route.gauge":
								case "train.gauge":
								case "route.signal":
								case "route.runinterval":
								case "train.interval":
								case "route.accelerationduetogravity":
								case "route.elevation":
								case "route.temperature":
								case "route.pressure":
								case "route.dynamiclight":
								case "route.ambientlight":
								case "route.directionallight":
								case "route.lightdirection":
								case "route.developerid":
								case "train.folder":
								case "train.file":
								case "train.destination":
								case "train.run":
								case "train.rail":
								case "train.flange":
								case "train.timetable.day":
								case "train.timetable.night":
								case "train.velocity":
								case "train.acceleration":
								case "train.station":
								case "structure.rail":
								case "structure.beacon":
								case "structure.pole":
								case "structure.ground":
								case "structure.walll":
								case "structure.wallr":
								case "structure.dikel":
								case "structure.diker":
								case "structure.forml":
								case "structure.formr":
								case "structure.formcl":
								case "structure.formcr":
								case "structure.roofl":
								case "structure.roofr":
								case "structure.roofcl":
								case "structure.roofcr":
								case "structure.crackl":
								case "structure.crackr":
								case "structure.freeobj":
								case "signal":
								case "texture.background":
								case "structure.back":
								case "structure.back.x":
								case "structure.back.aspect":
								case "texture.background.x":
								case "texture.background.aspect":
								case "cycle.ground":
								case "cycle.rail":
								case "route.loadingscreen":
								case "route.displayspeed":
									break;
									// track
								case "track.railstart":
								case "track.rail":
									if (!PreviewOnly)
									{
										int idx = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
										{
											Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											break;
										}
										if (idx < 1)
										{
											Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											break;
										}
										if (string.Compare(Command, "track.railstart", StringComparison.OrdinalIgnoreCase) == 0)
										{
											if (idx < Data.Blocks[BlockIndex].Rails.Length && Data.Blocks[BlockIndex].Rails[idx].RailStart)
											{
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is required to reference a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
										}
										if (Data.Blocks[BlockIndex].Rails.Length <= idx)
										{
											Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rails, idx + 1);
											int ol = Data.Blocks[BlockIndex].RailCycles.Length;
											Array.Resize<RailCycle>(ref Data.Blocks[BlockIndex].RailCycles, idx + 1);
											for (int rc = ol; rc < Data.Blocks[BlockIndex].RailCycles.Length; rc++)
											{
												Data.Blocks[BlockIndex].RailCycles[rc].RailCycleIndex = -1;
											}
										}
										if (Data.Blocks[BlockIndex].Rails[idx].RailStartRefreshed)
										{
											Data.Blocks[BlockIndex].Rails[idx].RailEnd = true;
										}
										Data.Blocks[BlockIndex].Rails[idx].RailStart = true;
										Data.Blocks[BlockIndex].Rails[idx].RailStartRefreshed = true;
										if (Arguments.Length >= 2)
										{
											if (Arguments[1].Length > 0)
											{
												double x;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												Data.Blocks[BlockIndex].Rails[idx].RailStartX = x;
											}
											if (!Data.Blocks[BlockIndex].Rails[idx].RailEnd)
											{
												Data.Blocks[BlockIndex].Rails[idx].RailEndX = Data.Blocks[BlockIndex].Rails[idx].RailStartX;
											}
										}
										if (Arguments.Length >= 3)
										{
											if (Arguments[2].Length > 0)
											{
												double y;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												Data.Blocks[BlockIndex].Rails[idx].RailStartY = y;
											}
											if (!Data.Blocks[BlockIndex].Rails[idx].RailEnd)
											{
												Data.Blocks[BlockIndex].Rails[idx].RailEndY = Data.Blocks[BlockIndex].Rails[idx].RailStartY;
											}
										}
										if (Data.Blocks[BlockIndex].RailType.Length <= idx)
										{
											Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
										}
										if (Arguments.Length >= 4 && Arguments[3].Length != 0)
										{
											int sttype;
											if (!NumberFormats.TryParseIntVb6(Arguments[3], out sttype))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (sttype < 0)
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											else if (!Data.Structure.RailObjects.ContainsKey(sttype))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											else
											{
												if (sttype < Data.Structure.RailCycles.Length && Data.Structure.RailCycles[sttype] != null)
												{
													Data.Blocks[BlockIndex].RailType[idx] = Data.Structure.RailCycles[sttype][0];
													Data.Blocks[BlockIndex].RailCycles[idx].RailCycleIndex = sttype;
													Data.Blocks[BlockIndex].RailCycles[idx].CurrentCycle = 0;
												}
												else
												{
													Data.Blocks[BlockIndex].RailType[idx] = sttype;
													Data.Blocks[BlockIndex].RailCycles[idx].RailCycleIndex = -1;
												}
											}
										}
									}
									break;
								case "track.railend":
									{
										if (!PreviewOnly)
										{
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
											{
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												break;
											}
											if (idx < 0 || idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart)
											{
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												break;
											}
											if (Data.Blocks[BlockIndex].RailType.Length <= idx)
											{
												Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rails, idx + 1);
											}
											Data.Blocks[BlockIndex].Rails[idx].RailStart = false;
											Data.Blocks[BlockIndex].Rails[idx].RailStartRefreshed = false;
											Data.Blocks[BlockIndex].Rails[idx].RailEnd = true;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0)
											{
												double x;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												Data.Blocks[BlockIndex].Rails[idx].RailEndX = x;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0)
											{
												double y;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												Data.Blocks[BlockIndex].Rails[idx].RailEndY = y;
											}
										}
									} break;
								case "track.railtype":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											int sttype = 0;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype)) {
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (idx < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (sttype < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (!Data.Structure.RailObjects.ContainsKey(sttype)) {
													Interface.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (Data.Blocks[BlockIndex].RailType.Length <= idx) {
														Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
														int ol = Data.Blocks[BlockIndex].RailCycles.Length;
														Array.Resize(ref Data.Blocks[BlockIndex].RailCycles, idx + 1);
														for (int rc = ol; rc < Data.Blocks[BlockIndex].RailCycles.Length; rc++)
														{
															Data.Blocks[BlockIndex].RailCycles[rc].RailCycleIndex = -1;
														}
													}
													if (sttype < Data.Structure.RailCycles.Length && Data.Structure.RailCycles[sttype] != null) {
														Data.Blocks[BlockIndex].RailType[idx] = Data.Structure.RailCycles[sttype][0];
														Data.Blocks[BlockIndex].RailCycles[idx].RailCycleIndex = sttype;
														Data.Blocks[BlockIndex].RailCycles[idx].CurrentCycle = 0;
													}
													else {
														Data.Blocks[BlockIndex].RailType[idx] = sttype;
														Data.Blocks[BlockIndex].RailCycles[idx].RailCycleIndex = -1;
													}
												}
											}
										}
									} break;
								case "track.accuracy":
									{
										double r = 2.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out r)) {
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											r = 2.0;
										}
										if (r < 0.0) {
											r = 0.0;
										} else if (r > 4.0) {
											r = 4.0;
										}
										Data.Blocks[BlockIndex].Accuracy = r;
									} break;
								case "track.pitch":
									{
										double p = 0.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out p)) {
											Interface.AddMessage(MessageType.Error, false, "ValueInPermille is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											p = 0.0;
										}
										Data.Blocks[BlockIndex].Pitch = 0.001 * p;
									} break;
								case "track.curve":
									{
										double radius = 0.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius)) {
											Interface.AddMessage(MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											radius = 0.0;
										}
										double cant = 0.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out cant)) {
											Interface.AddMessage(MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											cant = 0.0;
										} else {
											cant *= 0.001;
										}
										if (Data.SignedCant) {
											if (radius != 0.0) {
												cant *= (double)Math.Sign(radius);
											}
										} else {
											cant = Math.Abs(cant) * (double)Math.Sign(radius);
										}
										Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = radius;
										Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = cant;
										Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
									} break;
								case "track.turn":
									{
										double s = 0.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out s)) {
											Interface.AddMessage(MessageType.Error, false, "Ratio is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											s = 0.0;
										}
										Data.Blocks[BlockIndex].Turn = s;
									} break;
								case "track.adhesion":
									{
										double a = 100.0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out a)) {
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											a = 100.0;
										}
										if (a < 0.0) {
											Interface.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											a = 100.0;
										}
										Data.Blocks[BlockIndex].AdhesionMultiplier = 0.01 * a;
									} break;
								case "track.brightness":
									{
										if (!PreviewOnly) {
											float value = 255.0f;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[0], out value)) {
												Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												value = 255.0f;
											}
											value /= 255.0f;
											if (value < 0.0f) value = 0.0f;
											if (value > 1.0f) value = 1.0f;
											int n = Data.Blocks[BlockIndex].BrightnessChanges.Length;
											Array.Resize<Brightness>(ref Data.Blocks[BlockIndex].BrightnessChanges, n + 1);
											Data.Blocks[BlockIndex].BrightnessChanges[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].BrightnessChanges[n].Value = value;
										}
									} break;
								case "track.fog":
									{
										if (!PreviewOnly) {
											double start = 0.0, end = 0.0;
											int r = 128, g = 128, b = 128;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out start)) {
												Interface.AddMessage(MessageType.Error, false, "StartingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												start = 0.0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out end)) {
												Interface.AddMessage(MessageType.Error, false, "EndingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												end = 0.0;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out r)) {
												Interface.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												r = 128;
											} else if (r < 0 | r > 255) {
												Interface.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												r = r < 0 ? 0 : 255;
											}
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out g)) {
												Interface.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												g = 128;
											} else if (g < 0 | g > 255) {
												Interface.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												g = g < 0 ? 0 : 255;
											}
											if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out b)) {
												Interface.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												b = 128;
											} else if (b < 0 | b > 255) {
												Interface.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												b = b < 0 ? 0 : 255;
											}
											if (start < end) {
												Data.Blocks[BlockIndex].Fog.Start = (float)start;
												Data.Blocks[BlockIndex].Fog.End = (float)end;
											} else {
												Data.Blocks[BlockIndex].Fog.Start = Game.NoFogStart;
												Data.Blocks[BlockIndex].Fog.End = Game.NoFogEnd;
											}
											Data.Blocks[BlockIndex].Fog.Color = new Color24((byte)r, (byte)g, (byte)b);
											Data.Blocks[BlockIndex].FogDefined = true;
										}
									} break;
								case "track.section":
								case "track.sections":
									{
										if (!PreviewOnly) {
											if (Arguments.Length == 0) {
												Interface.AddMessage(MessageType.Error, false, "At least one argument is required in " + Command + "at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												int[] aspects = new int[Arguments.Length];
												for (int i = 0; i < Arguments.Length; i++)
												{
													int p = Arguments[i].IndexOf('.');
													if (p != -1)
													{
														//HACK: If we encounter a decimal followed by a non numerical character
														// we can assume that we are missing a comma and hence the section declaration has ended
														int pp = p;
														while (pp < Arguments[i].Length)
														{
															if (char.IsLetter(Arguments[i][pp]))
															{
																Arguments[i] = Arguments[i].Substring(0, p);
																Array.Resize(ref Arguments, i +1);
																Array.Resize(ref aspects, i + 1);
																break;
															}
															pp++;
														}
													}
												}
												for (int i = 0; i < Arguments.Length; i++) {
													if (string.IsNullOrEmpty(Arguments[i]))
													{
														Interface.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														aspects[i] = -1;
													} else if (!NumberFormats.TryParseIntVb6(Arguments[i], out aspects[i])) {
														Interface.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														aspects[i] = -1;
													} else if (aspects[i] < 0) {
														Interface.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is expected to be non-negative in " + Command + "at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														aspects[i] = -1;
													}
												}
												bool valueBased = ValueBasedSections | string.Equals(Command, "Track.SectionS", StringComparison.OrdinalIgnoreCase);
												if (valueBased) {
													Array.Sort<int>(aspects);
												}
												int n = Data.Blocks[BlockIndex].Sections.Length;
												Array.Resize<Section>(ref Data.Blocks[BlockIndex].Sections, n + 1);
												Data.Blocks[BlockIndex].Sections[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Sections[n].Aspects = aspects;
												Data.Blocks[BlockIndex].Sections[n].Type = valueBased ? Game.SectionType.ValueBased : Game.SectionType.IndexBased;
												Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = -1;
												if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
													if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
														Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = CurrentStation;
														DepartureSignalUsed = true;
													}
												}
												CurrentSection++;
											}
										}
									} break;
								case "track.sigf":
									{
										if (!PreviewOnly) {
											int objidx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out objidx)) {
												Interface.AddMessage(MessageType.Error, false, "SignalIndex is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												objidx = 0;
											}
											if (objidx >= 0 & objidx < Data.Signals.Length && Data.Signals[objidx] != null) {
												int section = 0;
												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out section)) {
													Interface.AddMessage(MessageType.Error, false, "Section is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													section = 0;
												}
												double x = 0.0, y = 0.0;
												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
													Interface.AddMessage(MessageType.Error, false, "X is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												double yaw = 0.0, pitch = 0.0, roll = 0.0;
												if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw)) {
													Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													yaw = 0.0;
												}
												if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch)) {
													Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													pitch = 0.0;
												}
												if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll)) {
													Interface.AddMessage(MessageType.Error, false, "Roll is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													roll = 0.0;
												}
												int n = Data.Blocks[BlockIndex].Signals.Length;
												Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signals, n + 1);
												Data.Blocks[BlockIndex].Signals[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Signals[n].SectionIndex = CurrentSection + section;
												Data.Blocks[BlockIndex].Signals[n].SignalCompatibilityObjectIndex = -1;
												Data.Blocks[BlockIndex].Signals[n].SignalObjectIndex = objidx;
												Data.Blocks[BlockIndex].Signals[n].X = x;
												Data.Blocks[BlockIndex].Signals[n].Y = y < 0.0 ? 4.8 : y;
												Data.Blocks[BlockIndex].Signals[n].Yaw = 0.0174532925199433 * yaw;
												Data.Blocks[BlockIndex].Signals[n].Pitch = 0.0174532925199433 * pitch;
												Data.Blocks[BlockIndex].Signals[n].Roll = 0.0174532925199433 * roll;
												Data.Blocks[BlockIndex].Signals[n].ShowObject = true;
												Data.Blocks[BlockIndex].Signals[n].ShowPost = y < 0.0;
											} else {
												Interface.AddMessage(MessageType.Error, false, "SignalIndex " + objidx + " references a signal object not loaded in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
										}
									} break;
								case "track.signal":
								case "track.sig":
									{
										if (!PreviewOnly) {
											int num = -2;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out num)) {
												Interface.AddMessage(MessageType.Error, false, "Aspects is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												num = -2;
											}
											if (num == 0 && IsRW == true)
											{
												//Aspects value of zero in RW routes produces a 2-aspect R/G signal
												num = -2;
											}
											if (num != 1 & num != -2 & num != 2 & num != -3 & num != 3 & num != -4 & num != 4 & num != -5 & num != 5 & num != 6) {
												Interface.AddMessage(MessageType.Error, false, "Aspects has an unsupported value in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												num = num == -3 | num == -6 | num == -1 ? -num : -4;
											}
											double x = 0.0, y = 0.0;
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
												Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												x = 0.0;
											}
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												y = 0.0;
											}
											double yaw = 0.0, pitch = 0.0, roll = 0.0;
											if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw)) {
												Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												yaw = 0.0;
											}
											if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch)) {
												Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												pitch = 0.0;
											}
											if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll)) {
												Interface.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												roll = 0.0;
											}
											int[] aspects; int comp;
											switch (num) {
													case 1: aspects = new int[] { 0, 2, 3 }; comp = 4; break;
													case 2: aspects = new int[] { 0, 2 }; comp = 0; break;
													case -2: aspects = new int[] { 0, 4 }; comp = 1; break;
													case 3: aspects = new int[] { 0, 2, 4 }; comp = 2; break;
													case 4: aspects = new int[] { 0, 1, 2, 4 }; comp = 3; break;
													case -4: aspects = new int[] { 0, 2, 3, 4 }; comp = 4; break;
													case 5: aspects = new int[] { 0, 1, 2, 3, 4 }; comp = 5; break;
													case -5: aspects = new int[] { 0, 2, 3, 4, 5 }; comp = 6; break;
													case 6: aspects = new int[] { 0, 1, 2, 3, 4, 5 }; comp = 7; break;
													default: aspects = new int[] { 0, 2 }; comp = 0; break;
											}
											int n = Data.Blocks[BlockIndex].Sections.Length;
											Array.Resize<Section>(ref Data.Blocks[BlockIndex].Sections, n + 1);
											Data.Blocks[BlockIndex].Sections[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Sections[n].Aspects = aspects;
											Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = -1;
											Data.Blocks[BlockIndex].Sections[n].Invisible = x == 0.0;
											Data.Blocks[BlockIndex].Sections[n].Type = Game.SectionType.ValueBased;
											if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
												if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
													Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = CurrentStation;
													DepartureSignalUsed = true;
												}
											}
											CurrentSection++;
											n = Data.Blocks[BlockIndex].Signals.Length;
											Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signals, n + 1);
											Data.Blocks[BlockIndex].Signals[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Signals[n].SectionIndex = CurrentSection;
											Data.Blocks[BlockIndex].Signals[n].SignalCompatibilityObjectIndex = comp;
											Data.Blocks[BlockIndex].Signals[n].SignalObjectIndex = -1;
											Data.Blocks[BlockIndex].Signals[n].X = x;
											Data.Blocks[BlockIndex].Signals[n].Y = y < 0.0 ? 4.8 : y;
											Data.Blocks[BlockIndex].Signals[n].Yaw = 0.0174532925199433 * yaw;
											Data.Blocks[BlockIndex].Signals[n].Pitch = 0.0174532925199433 * pitch;
											Data.Blocks[BlockIndex].Signals[n].Roll = 0.0174532925199433 * roll;
											Data.Blocks[BlockIndex].Signals[n].ShowObject = x != 0.0;
											Data.Blocks[BlockIndex].Signals[n].ShowPost = x != 0.0 & y < 0.0;
										}
									} break;
								case "track.relay":
									{
										if (!PreviewOnly) {
											double x = 0.0, y = 0.0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x)) {
												Interface.AddMessage(MessageType.Error, false, "X is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												x = 0.0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y)) {
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												y = 0.0;
											}
											double yaw = 0.0, pitch = 0.0, roll = 0.0;
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out yaw)) {
												Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												yaw = 0.0;
											}
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out pitch)) {
												Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												pitch = 0.0;
											}
											if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out roll)) {
												Interface.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												roll = 0.0;
											}
											int n = Data.Blocks[BlockIndex].Signals.Length;
											Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signals, n + 1);
											Data.Blocks[BlockIndex].Signals[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Signals[n].SectionIndex = CurrentSection + 1;
											Data.Blocks[BlockIndex].Signals[n].SignalCompatibilityObjectIndex = 8;
											Data.Blocks[BlockIndex].Signals[n].SignalObjectIndex = -1;
											Data.Blocks[BlockIndex].Signals[n].X = x;
											Data.Blocks[BlockIndex].Signals[n].Y = y < 0.0 ? 4.8 : y;
											Data.Blocks[BlockIndex].Signals[n].Yaw = yaw * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signals[n].Pitch = pitch * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signals[n].Roll = roll * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signals[n].ShowObject = x != 0.0;
											Data.Blocks[BlockIndex].Signals[n].ShowPost = x != 0.0 & y < 0.0;
										}
									} break;
								case "track.destination":
									{
										if (!PreviewOnly)
										{
											int type = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
											{
												Interface.AddMessage(MessageType.Error, false, "Type is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												type = 0;
											}
											if (type < -1 || type > 1)
											{
												Interface.AddMessage(MessageType.Error, false, "Type is expected to be in the range of -1 to 1 in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											else
											{
												int structure = 0, nextDestination = 0, previousDestination = 0, triggerOnce = 0;
												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
												{
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = 0;
												}
												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out nextDestination))
												{
													Interface.AddMessage(MessageType.Error, false, "NextDestination is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													nextDestination = 0;
												}
												if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out previousDestination))
												{
													Interface.AddMessage(MessageType.Error, false, "PreviousDestination is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													previousDestination = 0;
												}
												if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out triggerOnce))
												{
													Interface.AddMessage(MessageType.Error, false, "TriggerOnce is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													previousDestination = 0;
												}
												if (structure < -1)
												{
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = -1;
												}
												else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
												{
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = -1;
												}
												if (triggerOnce < 0 || triggerOnce > 1)
												{
													Interface.AddMessage(MessageType.Error, false, "TriggerOnce is expected to be in the range of 0 to 1 in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													triggerOnce = 0;
												}
												double x = 0.0, y = 0.0;
												double yaw = 0.0, pitch = 0.0, roll = 0.0;
												if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], UnitOfLength, out y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out yaw))
												{
													Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													yaw = 0.0;
												}
												if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out pitch))
												{
													Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													pitch = 0.0;
												}
												if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[9], out roll))
												{
													Interface.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Destination at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													roll = 0.0;
												}
												int n = Data.Blocks[BlockIndex].DestinationChanges.Length;
												Array.Resize<DestinationEvent>(ref Data.Blocks[BlockIndex].DestinationChanges, n + 1);
												Data.Blocks[BlockIndex].DestinationChanges[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].DestinationChanges[n].Type = type;
												Data.Blocks[BlockIndex].DestinationChanges[n].TriggerOnce = triggerOnce != 0;
												Data.Blocks[BlockIndex].DestinationChanges[n].PreviousDestination = previousDestination;
												Data.Blocks[BlockIndex].DestinationChanges[n].BeaconStructureIndex = structure;
												Data.Blocks[BlockIndex].DestinationChanges[n].NextDestination = nextDestination;
												Data.Blocks[BlockIndex].DestinationChanges[n].X = x;
												Data.Blocks[BlockIndex].DestinationChanges[n].Y = y;
												Data.Blocks[BlockIndex].DestinationChanges[n].Yaw = yaw * 0.0174532925199433;
												Data.Blocks[BlockIndex].DestinationChanges[n].Pitch = pitch * 0.0174532925199433;
												Data.Blocks[BlockIndex].DestinationChanges[n].Roll = roll * 0.0174532925199433;
											}
										}
									}
									break;
								case "track.beacon":
									{
										if (!PreviewOnly) {
											int type = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type)) {
												Interface.AddMessage(MessageType.Error, false, "Type is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												type = 0;
											}
											if (type < 0) {
												Interface.AddMessage(MessageType.Error, false, "Type is expected to be non-negative in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												int structure = 0, section = 0, optional = 0;
												if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure)) {
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = 0;
												}
												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out section)) {
													Interface.AddMessage(MessageType.Error, false, "Section is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													section = 0;
												}
												if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out optional)) {
													Interface.AddMessage(MessageType.Error, false, "Data is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													optional = 0;
												}
												if (structure < -1) {
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = -1;
												} else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure)) {
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = -1;
												}
												if (section == -1) {
													//section = (int)TrackManager.TransponderSpecialSection.NextRedSection;
												} else if (section < 0) {
													Interface.AddMessage(MessageType.Error, false, "Section is expected to be non-negative or -1 in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													section = CurrentSection + 1;
												} else {
													section += CurrentSection;
												}
												double x = 0.0, y = 0.0;
												double yaw = 0.0, pitch = 0.0, roll = 0.0;
												if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out x)) {
													Interface.AddMessage(MessageType.Error, false, "X is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out y)) {
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out yaw)) {
													Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													yaw = 0.0;
												}
												if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out pitch)) {
													Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													pitch = 0.0;
												}
												if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out roll)) {
													Interface.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													roll = 0.0;
												}
												int n = Data.Blocks[BlockIndex].Transponders.Length;
												Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
												Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Transponders[n].Type = type;
												Data.Blocks[BlockIndex].Transponders[n].Data = optional;
												Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = structure;
												Data.Blocks[BlockIndex].Transponders[n].SectionIndex = section;
												Data.Blocks[BlockIndex].Transponders[n].ShowDefaultObject = false;
												Data.Blocks[BlockIndex].Transponders[n].X = x;
												Data.Blocks[BlockIndex].Transponders[n].Y = y;
												Data.Blocks[BlockIndex].Transponders[n].Yaw = yaw * 0.0174532925199433;
												Data.Blocks[BlockIndex].Transponders[n].Pitch = pitch * 0.0174532925199433;
												Data.Blocks[BlockIndex].Transponders[n].Roll = roll * 0.0174532925199433;
											}
										}
									} break;
								case "track.transponder":
								case "track.tr":
									{
										if (!PreviewOnly) {
											int type = 0, oversig = 0, work = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type)) {
												Interface.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												type = 0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out oversig)) {
												Interface.AddMessage(MessageType.Error, false, "Signals is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												oversig = 0;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out work)) {
												Interface.AddMessage(MessageType.Error, false, "SwitchSystems is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												work = 0;
											}
											if (oversig < 0) {
												Interface.AddMessage(MessageType.Error, false, "Signals is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												oversig = 0;
											}
											double x = 0.0, y = 0.0;
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out x)) {
												Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												x = 0.0;
											}
											if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out y)) {
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												y = 0.0;
											}
											double yaw = 0.0, pitch = 0.0, roll = 0.0;
											if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw)) {
												Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												yaw = 0.0;
											}
											if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out pitch)) {
												Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												pitch = 0.0;
											}
											if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out roll)) {
												Interface.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												roll = 0.0;
											}
											int n = Data.Blocks[BlockIndex].Transponders.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
											Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponders[n].Type = type;
											Data.Blocks[BlockIndex].Transponders[n].Data = work;
											Data.Blocks[BlockIndex].Transponders[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponders[n].X = x;
											Data.Blocks[BlockIndex].Transponders[n].Y = y;
											Data.Blocks[BlockIndex].Transponders[n].Yaw = yaw * 0.0174532925199433;
											Data.Blocks[BlockIndex].Transponders[n].Pitch = pitch * 0.0174532925199433;
											Data.Blocks[BlockIndex].Transponders[n].Roll = roll * 0.0174532925199433;
											Data.Blocks[BlockIndex].Transponders[n].SectionIndex = CurrentSection + oversig + 1;
											Data.Blocks[BlockIndex].Transponders[n].ClipToFirstRedSection = true;
										}
									} break;
								case "track.atssn":
									{
										if (!PreviewOnly) {
											int n = Data.Blocks[BlockIndex].Transponders.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
											Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponders[n].Type = 0;
											Data.Blocks[BlockIndex].Transponders[n].Data = 0;
											Data.Blocks[BlockIndex].Transponders[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponders[n].SectionIndex = CurrentSection + 1;
											Data.Blocks[BlockIndex].Transponders[n].ClipToFirstRedSection = true;
										}
									} break;
								case "track.atsp":
									{
										if (!PreviewOnly) {
											int n = Data.Blocks[BlockIndex].Transponders.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
											Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponders[n].Type = 3;
											Data.Blocks[BlockIndex].Transponders[n].Data = 0;
											Data.Blocks[BlockIndex].Transponders[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponders[n].SectionIndex = CurrentSection + 1;
											Data.Blocks[BlockIndex].Transponders[n].ClipToFirstRedSection = true;
										}
									} break;
								case "track.pattern":
									{
										if (!PreviewOnly) {
											int type = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type)) {
												Interface.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												type = 0;
											}
											double speed = 0.0;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed)) {
												Interface.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												speed = 0.0;
											}
											int n = Data.Blocks[BlockIndex].Transponders.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
											Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
											if (type == 0) {
												Data.Blocks[BlockIndex].Transponders[n].Type = TrackManager.SpecialTransponderTypes.InternalAtsPTemporarySpeedLimit;
												Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6);
											} else {
												Data.Blocks[BlockIndex].Transponders[n].Type = TrackManager.SpecialTransponderTypes.AtsPPermanentSpeedLimit;
												Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6);
											}
											Data.Blocks[BlockIndex].Transponders[n].SectionIndex = -1;
											Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
										}
									} break;
								case "track.plimit":
									{
										if (!PreviewOnly) {
											double speed = 0.0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out speed)) {
												Interface.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												speed = 0.0;
											}
											int n = Data.Blocks[BlockIndex].Transponders.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
											Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponders[n].Type = TrackManager.SpecialTransponderTypes.AtsPPermanentSpeedLimit;
											Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6);
											Data.Blocks[BlockIndex].Transponders[n].SectionIndex = -1;
											Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
										}
									} break;
								case "track.limit":
									{
										double limit = 0.0;
										int direction = 0, cource = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out limit)) {
											Interface.AddMessage(MessageType.Error, false, "Speed is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											limit = 0.0;
										}
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out direction)) {
											Interface.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											direction = 0;
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out cource)) {
											Interface.AddMessage(MessageType.Error, false, "Cource is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											cource = 0;
										}
										int n = Data.Blocks[BlockIndex].Limits.Length;
										Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limits, n + 1);
										Data.Blocks[BlockIndex].Limits[n].TrackPosition = Data.TrackPosition;
										Data.Blocks[BlockIndex].Limits[n].Speed = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
										Data.Blocks[BlockIndex].Limits[n].Direction = direction;
										Data.Blocks[BlockIndex].Limits[n].Cource = cource;
									} break;
								case "track.stop":
									if (CurrentStation == -1) {
										Interface.AddMessage(MessageType.Error, false, "A stop without a station is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} else {
										int dir = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out dir)) {
											Interface.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											dir = 0;
										}
										double backw = 5.0, forw = 5.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw)) {
											Interface.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											backw = 5.0;
										} else if (backw <= 0.0) {
											Interface.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											backw = 5.0;
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw)) {
											Interface.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											forw = 5.0;
										} else if (forw <= 0.0) {
											Interface.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											forw = 5.0;
										}
										int cars = 0;
										if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out cars)) {
											Interface.AddMessage(MessageType.Error, false, "Cars is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											cars = 0;
										}
										int n = Data.Blocks[BlockIndex].StopPositions.Length;
										Array.Resize<Stop>(ref Data.Blocks[BlockIndex].StopPositions, n + 1);
										Data.Blocks[BlockIndex].StopPositions[n].TrackPosition = Data.TrackPosition;
										Data.Blocks[BlockIndex].StopPositions[n].Station = CurrentStation;
										Data.Blocks[BlockIndex].StopPositions[n].Direction = dir;
										Data.Blocks[BlockIndex].StopPositions[n].ForwardTolerance = forw;
										Data.Blocks[BlockIndex].StopPositions[n].BackwardTolerance = backw;
										Data.Blocks[BlockIndex].StopPositions[n].Cars = cars;
										CurrentStop = cars;
									} break;
								case "track.sta":
									{
										CurrentStation++;
										Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
										Game.Stations[CurrentStation] = new Game.Station();
										if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
											Game.Stations[CurrentStation].Name = Arguments[0];
										}
										double arr = -1.0, dep = -1.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
											if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.AllPass;
											} else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
											} else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
											} else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if(Arguments[1].Length == 1 && Arguments[1][0] == '.')
											{ /* Treat a single period as a blank space */ }
											else if (!Interface.TryParseTime(Arguments[1], out arr)) {
												Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												arr = -1.0;
											}
										}
										if (Arguments.Length >= 3 && (Arguments[2].Length > 0)) {
											if (string.Equals(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[2], "=", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.Terminal;
											} else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.Terminal;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.ChangeEnds;
											} else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.ChangeEnds;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if(Arguments[2].Length == 1 && Arguments[2][0] == '.')
											{ /* Treat a single period as a blank space */ }
											else if (!Interface.TryParseTime(Arguments[2], out dep)) {
												Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												dep = -1.0;
											}
										}
										int passalarm = 0;
										if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out passalarm)) {
											Interface.AddMessage(MessageType.Error, false, "PassAlarm is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											passalarm = 0;
										}
										int door = 0;
										bool doorboth = false;
										if (Arguments.Length >= 5 && Arguments[4].Length != 0) {
											switch (Arguments[4].ToUpperInvariant()) {
												case "L":
													door = -1;
													break;
												case "R":
													door = 1;
													break;
												case "N":
													door = 0;
													break;
												case "B":
													doorboth = true;
													break;
												default:
													if (!NumberFormats.TryParseIntVb6(Arguments[4], out door)) {
														Interface.AddMessage(MessageType.Error, false, "Doors is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														door = 0;
													}
													break;
											}
										}
										int stop = 0;
										if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[5], out stop)) {
											Interface.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											stop = 0;
										}
										int device = 0;
										if (Arguments.Length >= 7 && Arguments[6].Length > 0) {
											if (string.Compare(Arguments[6], "ats", StringComparison.OrdinalIgnoreCase) == 0) {
												device = 0;
											} else if (string.Compare(Arguments[6], "atc", StringComparison.OrdinalIgnoreCase) == 0) {
												device = 1;
											} else if (!NumberFormats.TryParseIntVb6(Arguments[6], out device)) {
												Interface.AddMessage(MessageType.Error, false, "System is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												device = 0;
											}
											if (device != 0 & device != 1) {
												Interface.AddMessage(MessageType.Error, false, "System is not supported in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												device = 0;
											}
										}
										Sounds.SoundBuffer arrsnd = null;
										Sounds.SoundBuffer depsnd = null;
										if (!PreviewOnly) {
											if (Arguments.Length >= 8 && Arguments[7].Length > 0) {
												if (Path.ContainsInvalidChars(Arguments[7])) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalSound contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[7]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "ArrivalSound " + f + " not found in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														const double radius = 30.0;
														arrsnd = Sounds.RegisterBuffer(f, radius);
													}
												}
											}
										}
										double halt = 15.0;
										if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out halt)) {
											Interface.AddMessage(MessageType.Error, false, "StopDuration is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											halt = 15.0;
										} else if (halt < 5.0) {
											halt = 5.0;
										}
										double jam = 100.0;
										if (!PreviewOnly) {
											if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[9], out jam)) {
												Interface.AddMessage(MessageType.Error, false, "PassengerRatio is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												jam = 100.0;
											} else if (jam < 0.0) {
												Interface.AddMessage(MessageType.Error, false, "PassengerRatio is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												jam = 100.0;
											}
										}
										if (!PreviewOnly) {
											if (Arguments.Length >= 11 && Arguments[10].Length > 0) {
												if (Path.ContainsInvalidChars(Arguments[10])) {
													Interface.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[10]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														const double radius = 30.0;
														depsnd = Sounds.RegisterBuffer(f, radius);
													}
												}
											}
										}
										OpenBveApi.Textures.Texture tdt = null, tnt = null;
										if (!PreviewOnly)
										{
											int ttidx;
											if (Arguments.Length >= 12 && Arguments[11].Length > 0) {
												if (!NumberFormats.TryParseIntVb6(Arguments[11], out ttidx)) {
													ttidx = -1;
												} else {
													if (ttidx < 0) {
														Interface.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														ttidx = -1;
													} else if (ttidx >= Data.TimetableDaytime.Length & ttidx >= Data.TimetableNighttime.Length) {
														Interface.AddMessage(MessageType.Error, false, "TimetableIndex references textures not loaded in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														ttidx = -1;
													}
													tdt = ttidx >= 0 & ttidx < Data.TimetableDaytime.Length ? Data.TimetableDaytime[ttidx] : null;
													tnt = ttidx >= 0 & ttidx < Data.TimetableNighttime.Length ? Data.TimetableNighttime[ttidx] : null;
													ttidx = 0;
												}
											} else {
												ttidx = -1;
											}
											if (ttidx == -1) {
												if (CurrentStation > 0) {
													tdt = Game.Stations[CurrentStation - 1].TimetableDaytimeTexture;
													tnt = Game.Stations[CurrentStation - 1].TimetableNighttimeTexture;
												} else if (Data.TimetableDaytime.Length > 0 & Data.TimetableNighttime.Length > 0) {
													tdt = Data.TimetableDaytime[0];
													tnt = Data.TimetableNighttime[0];
												} else {
													tdt = null;
													tnt = null;
												}
											}
										}
										double reopenDoor = 0.0;
										if (!PreviewOnly)
										{
											if (Arguments.Length >= 13 && Arguments[12].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[12], out reopenDoor)) {
												Interface.AddMessage(MessageType.Error, false, "ReopenDoor is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												reopenDoor = 0.0;
											} else if (reopenDoor < 0.0) {
												Interface.AddMessage(MessageType.Error, false, "ReopenDoor is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												reopenDoor = 0.0;
											}
										}
										int reopenStationLimit = 5;
										if(!PreviewOnly)
										{
											if (Arguments.Length >= 14 && Arguments[13].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[13], out reopenStationLimit)) {
												Interface.AddMessage(MessageType.Error, false, "ReopenStationLimit is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												reopenStationLimit = 5;
											} else if (reopenStationLimit < 0) {
												Interface.AddMessage(MessageType.Error, false, "ReopenStationLimit is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												reopenStationLimit = 0;
											}
										}
										double interferenceInDoor = Program.RandomNumberGenerator.NextDouble() * 30.0;
										if (!PreviewOnly)
										{
											if (Arguments.Length >= 15 && Arguments[14].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[14], out interferenceInDoor)) {
												Interface.AddMessage(MessageType.Error, false, "InterferenceInDoor is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												interferenceInDoor = Program.RandomNumberGenerator.NextDouble() * 30.0;
											} else if (interferenceInDoor < 0.0) {
												Interface.AddMessage(MessageType.Error, false, "InterferenceInDoor is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												interferenceInDoor = 0.0;
											}
										}
										int maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
										if (!PreviewOnly)
										{
											if (Arguments.Length >= 16 && Arguments[15].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[15], out maxInterferingObjectRate)) {
												Interface.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
											} else if (maxInterferingObjectRate <= 0 || maxInterferingObjectRate >= 100) {
												Interface.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is expected to be positive, less than 100 in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
											}
										}
										if (Game.Stations[CurrentStation].Name.Length == 0 & (Game.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | Game.Stations[CurrentStation].StopMode == StationStopMode.AllStop)) {
											Game.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture) + ")";
										}
										Game.Stations[CurrentStation].ArrivalTime = arr;
										Game.Stations[CurrentStation].ArrivalSoundBuffer = arrsnd;
										Game.Stations[CurrentStation].DepartureTime = dep;
										Game.Stations[CurrentStation].DepartureSoundBuffer = depsnd;
										Game.Stations[CurrentStation].StopTime = halt;
										Game.Stations[CurrentStation].ForceStopSignal = stop == 1;
										Game.Stations[CurrentStation].OpenLeftDoors = door < 0.0 | doorboth;
										Game.Stations[CurrentStation].OpenRightDoors = door > 0.0 | doorboth;
										Game.Stations[CurrentStation].SafetySystem = device == 1 ? Game.SafetySystem.Atc : Game.SafetySystem.Ats;
										Game.Stations[CurrentStation].Stops = new Game.StationStop[] { };
										Game.Stations[CurrentStation].PassengerRatio = 0.01 * jam;
										Game.Stations[CurrentStation].TimetableDaytimeTexture = tdt;
										Game.Stations[CurrentStation].TimetableNighttimeTexture = tnt;
										Game.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
										Game.Stations[CurrentStation].ReopenDoor = 0.01 * reopenDoor;
										Game.Stations[CurrentStation].ReopenStationLimit = reopenStationLimit;
										Game.Stations[CurrentStation].InterferenceInDoor = interferenceInDoor;
										Game.Stations[CurrentStation].MaxInterferingObjectRate = maxInterferingObjectRate;
										Data.Blocks[BlockIndex].Station = CurrentStation;
										Data.Blocks[BlockIndex].StationPassAlarm = passalarm == 1;
										CurrentStop = -1;
										DepartureSignalUsed = false;
									} break;
								case "track.station":
									{
										CurrentStation++;
										Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
										Game.Stations[CurrentStation] = new Game.Station();
										if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
											Game.Stations[CurrentStation].Name = Arguments[0];
										}
										double arr = -1.0, dep = -1.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
											if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.AllPass;
											} else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
											} else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
											} else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (!Interface.TryParseTime(Arguments[1], out arr)) {
												Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												arr = -1.0;
											}
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
											if (string.Equals(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[2], "=", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.Terminal;
											} else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.Terminal;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.ChangeEnds;
											} else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].Type = StationType.ChangeEnds;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (!Interface.TryParseTime(Arguments[2], out dep)) {
												Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												dep = -1.0;
											}
										}
										int stop = 0;
										if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out stop)) {
											Interface.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											stop = 0;
										}
										int device = 0;
										if (Arguments.Length >= 5 && Arguments[4].Length > 0) {
											if (string.Compare(Arguments[4], "ats", StringComparison.OrdinalIgnoreCase) == 0 || (Interface.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("ats", StringComparison.OrdinalIgnoreCase))) {
												device = 0;
											} else if (string.Compare(Arguments[4], "atc", StringComparison.OrdinalIgnoreCase) == 0 || (Interface.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("atc", StringComparison.OrdinalIgnoreCase))) {
												device = 1;
											} else if (!NumberFormats.TryParseIntVb6(Arguments[4], out device)) {
												Interface.AddMessage(MessageType.Error, false, "System is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												device = 0;
											} else if (device != 0 & device != 1) {
												Interface.AddMessage(MessageType.Error, false, "System is not supported in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												device = 0;
											}
										}
										Sounds.SoundBuffer depsnd = null;
										if (!PreviewOnly) {
											if (Arguments.Length >= 6 && Arguments[5].Length != 0) {
												if (Path.ContainsInvalidChars(Arguments[5])) {
													Interface.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[5]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														const double radius = 30.0;
														depsnd = Sounds.RegisterBuffer(f, radius);
													}
												}
											}
										}
										if (Game.Stations[CurrentStation].Name.Length == 0 & (Game.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | Game.Stations[CurrentStation].StopMode == StationStopMode.AllStop)) {
											Game.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture) + ")";
										}
										Game.Stations[CurrentStation].ArrivalTime = arr;
										Game.Stations[CurrentStation].ArrivalSoundBuffer = null;
										Game.Stations[CurrentStation].DepartureTime = dep;
										Game.Stations[CurrentStation].DepartureSoundBuffer = depsnd;
										Game.Stations[CurrentStation].StopTime = 15.0;
										Game.Stations[CurrentStation].ForceStopSignal = stop == 1;
										Game.Stations[CurrentStation].OpenLeftDoors = true;
										Game.Stations[CurrentStation].OpenRightDoors = true;
										Game.Stations[CurrentStation].SafetySystem = device == 1 ? Game.SafetySystem.Atc : Game.SafetySystem.Ats;
										Game.Stations[CurrentStation].Stops = new Game.StationStop[] { };
										Game.Stations[CurrentStation].PassengerRatio = 1.0;
										Game.Stations[CurrentStation].TimetableDaytimeTexture = null;
										Game.Stations[CurrentStation].TimetableNighttimeTexture = null;
										Game.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
										Game.Stations[CurrentStation].ReopenDoor = 0.0;
										Game.Stations[CurrentStation].ReopenStationLimit = 0;
										Game.Stations[CurrentStation].InterferenceInDoor = 0.0;
										Game.Stations[CurrentStation].MaxInterferingObjectRate = 10;
										Data.Blocks[BlockIndex].Station = CurrentStation;
										Data.Blocks[BlockIndex].StationPassAlarm = false;
										CurrentStop = -1;
										DepartureSignalUsed = false;
									} break;
								case "track.stationxml":
									string fn = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
									if (!System.IO.File.Exists(fn))
									{
										Interface.AddMessage(MessageType.Error, true, "Station XML file " + fn + " not found in Track.StationXML at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										break;
									}
									CurrentStation++;
									Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
									Game.Stations[CurrentStation] = new Game.Station();
									StopRequest sr = new StopRequest();
									sr.TrackPosition = Data.TrackPosition;
									sr.StationIndex = CurrentStation;
									Game.Stations[CurrentStation] = StationXMLParser.ReadStationXML(fn, PreviewOnly, Data.TimetableDaytime, Data.TimetableNighttime, CurrentStation, ref Data.Blocks[BlockIndex].StationPassAlarm, ref sr);
									if (Game.Stations[CurrentStation].Type == StationType.RequestStop)
									{
										int l = Data.RequestStops.Length;
										Array.Resize<StopRequest> (ref Data.RequestStops, l + 1);
										Data.RequestStops[l] = sr;
									}
									Data.Blocks[BlockIndex].Station = CurrentStation;
									break;
								case "track.buffer":
									{
										if (!PreviewOnly) {
											int n = Game.BufferTrackPositions.Length;
											Array.Resize<double>(ref Game.BufferTrackPositions, n + 1);
											Game.BufferTrackPositions[n] = Data.TrackPosition;
										}
									} break;
								case "track.form":
									{
										if (!PreviewOnly) {
											int idx1 = 0, idx2 = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx1 = 0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
												if (string.Compare(Arguments[1], "L", StringComparison.OrdinalIgnoreCase) == 0) {
													idx2 = Form.SecondaryRailL;
												} else if (string.Compare(Arguments[1], "R", StringComparison.OrdinalIgnoreCase) == 0) {
													idx2 = Form.SecondaryRailR;
												} else if (IsRW && string.Compare(Arguments[1], "9X", StringComparison.OrdinalIgnoreCase) == 0) {
													idx2 = int.MaxValue;
												} else if (!NumberFormats.TryParseIntVb6(Arguments[1], out idx2)) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													idx2 = 0;
												}
											}
											if (IsRW) {
												if (idx2 == int.MaxValue) {
													idx2 = 9;
												} else if (idx2 == -9) {
													idx2 = Form.SecondaryRailL;
												} else if (idx2 == 9) {
													idx2 = Form.SecondaryRailR;
												}
											}
											if (idx1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (idx2 < 0 & idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be greater or equal to -2 in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx1 >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx1].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex1 could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR && (idx2 >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx2].RailStart)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex2 could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												int roof = 0, pf = 0;
												if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out roof)) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													roof = 0;
												}
												if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out pf)) {
													Interface.AddMessage(MessageType.Error, false, "FormStructureIndex is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													pf = 0;
												}
												if (roof != 0 & (roof < 0 || !Data.Structure.RoofL.ContainsKey(roof) || !Data.Structure.RoofR.ContainsKey(roof))) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex " + roof + " references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (pf < 0 | (!Data.Structure.FormL.ContainsKey(pf) & !Data.Structure.FormR.ContainsKey(pf))) {
														Interface.AddMessage(MessageType.Error, false, "FormStructureIndex " + pf + " references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													int n = Data.Blocks[BlockIndex].Forms.Length;
													Array.Resize<Form>(ref Data.Blocks[BlockIndex].Forms, n + 1);
													Data.Blocks[BlockIndex].Forms[n].PrimaryRail = idx1;
													Data.Blocks[BlockIndex].Forms[n].SecondaryRail = idx2;
													Data.Blocks[BlockIndex].Forms[n].FormType = pf;
													Data.Blocks[BlockIndex].Forms[n].RoofType = roof;
												}
											}
										}
									} break;
								case "track.pole":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx +" could be out of range in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (idx >= Data.Blocks[BlockIndex].RailPole.Length) {
													Array.Resize<Pole>(ref Data.Blocks[BlockIndex].RailPole, idx + 1);
													Data.Blocks[BlockIndex].RailPole[idx].Mode = 0;
													Data.Blocks[BlockIndex].RailPole[idx].Location = 0;
													Data.Blocks[BlockIndex].RailPole[idx].Interval = 2.0 * Data.BlockInterval;
													Data.Blocks[BlockIndex].RailPole[idx].Type = 0;
												}
												int typ = Data.Blocks[BlockIndex].RailPole[idx].Mode;
												int sttype = Data.Blocks[BlockIndex].RailPole[idx].Type;
												if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
													if (!NumberFormats.TryParseIntVb6(Arguments[1], out typ)) {
														Interface.AddMessage(MessageType.Error, false, "AdditionalRailsCovered is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														typ = 0;
													}
												}
												if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
													double loc;
													if (!NumberFormats.TryParseDoubleVb6(Arguments[2], out loc)) {
														Interface.AddMessage(MessageType.Error, false, "Location is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														loc = 0.0;
													}
													Data.Blocks[BlockIndex].RailPole[idx].Location = loc;
												}
												if (Arguments.Length >= 4 && Arguments[3].Length > 0) {
													double dist;
													if (!NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out dist)) {
														Interface.AddMessage(MessageType.Error, false, "Interval is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														dist = Data.BlockInterval;
													}
													Data.Blocks[BlockIndex].RailPole[idx].Interval = dist;
												}
												if (Arguments.Length >= 5 && Arguments[4].Length > 0) {
													if (!NumberFormats.TryParseIntVb6(Arguments[4], out sttype)) {
														Interface.AddMessage(MessageType.Error, false, "PoleStructureIndex is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														sttype = 0;
													}
												}
												if (typ < 0 || typ >= Data.Structure.Poles.Length || Data.Structure.Poles[typ] == null) {
													Interface.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (sttype < 0 || sttype >= Data.Structure.Poles[typ].Length || Data.Structure.Poles[typ][sttype] == null) {
													Interface.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													Data.Blocks[BlockIndex].RailPole[idx].Mode = typ;
													Data.Blocks[BlockIndex].RailPole[idx].Type = sttype;
													Data.Blocks[BlockIndex].RailPole[idx].Exists = true;
												}
											}
										}
									} break;
								case "track.poleend":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.PoleEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailPole.Length) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing pole in Track.PoleEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rails.Length || (!Data.Blocks[BlockIndex].Rails[idx].RailStart & !Data.Blocks[BlockIndex].Rails[idx].RailEnd)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.PoleEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Data.Blocks[BlockIndex].RailPole[idx].Exists = false;
											}
										}
									} break;
								case "track.wall":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											int dir = 0;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
												switch (Arguments[1].ToUpperInvariant().Trim())
												{
													case "L":
													case "-1":
														dir = -1;
														break;
													case "0":
														dir = 0;
														break;
													case "R":
													case "1":
														dir = 1;
														break;
													default:
														if (!NumberFormats.TryParseIntVb6(Arguments[1], out dir))
														{
															Interface.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = 0;
														}

														break;
												}
											}
											int sttype = 0;
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype)) {
												Interface.AddMessage(MessageType.Error, false, "WallStructureIndex is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (sttype < 0) {
												Interface.AddMessage(MessageType.Error, false, "WallStructureIndex is expected to be a non-negative integer in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (dir < 0 && !Data.Structure.WallL.ContainsKey(sttype)  || dir > 0 && !Data.Structure.WallR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.WallL.ContainsKey(sttype) && !Data.Structure.WallR.ContainsKey(sttype))) {
												if (dir < 0)
												{
													Interface.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												else if (dir > 0)
												{
													Interface.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
											} else {
												if (idx < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (dir == 0)
													{
														if (!Data.Structure.WallL.ContainsKey(sttype))
														{
															Interface.AddMessage(MessageType.Error, false, "LeftWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = 1;
														}
														if (!Data.Structure.WallR.ContainsKey(sttype))
														{
															Interface.AddMessage(MessageType.Error, false, "RightWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = -1;
														}
													}
													if (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													if (idx >= Data.Blocks[BlockIndex].RailWall.Length) {
														Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailWall, idx + 1);
													}
													Data.Blocks[BlockIndex].RailWall[idx].Exists = true;
													Data.Blocks[BlockIndex].RailWall[idx].Type = sttype;
													Data.Blocks[BlockIndex].RailWall[idx].Direction = dir;
												}
											}
										}
									} break;
								case "track.wallend":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailWall.Length) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing wall in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rails.Length || (!Data.Blocks[BlockIndex].Rails[idx].RailStart & !Data.Blocks[BlockIndex].Rails[idx].RailEnd)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Data.Blocks[BlockIndex].RailWall[idx].Exists = false;
											}
										}
									} break;
								case "track.dike":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											int dir = 0;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0)
											{
												switch (Arguments[1].ToUpperInvariant().Trim())
												{
													case "L":
													case "-1":
														dir = -1;
														break;
													case "0":
														dir = 0;
														break;
													case "R":
													case "1":
														dir = 1;
														break;
													default:
														if (!NumberFormats.TryParseIntVb6(Arguments[1], out dir))
														{
															Interface.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = 0;
														}
														break;
												}
											}
											int sttype = 0;
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype)) {
												Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (sttype < 0) {
												Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex is expected to be a non-negative integer in Track.DikeL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (dir < 0 && !Data.Structure.DikeL.ContainsKey(sttype) || dir > 0 && !Data.Structure.DikeR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.DikeL.ContainsKey(sttype) && !Data.Structure.DikeR.ContainsKey(sttype))) {
												if (dir > 0)
												{
													Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												else if (dir < 0)
												{
													Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (dir == 0)
													{
														if (!Data.Structure.DikeL.ContainsKey(sttype))
														{
															Interface.AddMessage(MessageType.Error, false, "LeftDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = 1;
														}
														if (!Data.Structure.DikeR.ContainsKey(sttype))
														{
															Interface.AddMessage(MessageType.Error, false, "RightDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															dir = -1;
														}
													}
													if (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													if (idx >= Data.Blocks[BlockIndex].RailDike.Length) {
														Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailDike, idx + 1);
													}
													Data.Blocks[BlockIndex].RailDike[idx].Exists = true;
													Data.Blocks[BlockIndex].RailDike[idx].Type = sttype;
													Data.Blocks[BlockIndex].RailDike[idx].Direction = dir;
												}
											}
										}
									} break;
								case "track.dikeend":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailDike.Length) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx +" does not reference an existing dike in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rails.Length || (!Data.Blocks[BlockIndex].Rails[idx].RailStart & !Data.Blocks[BlockIndex].Rails[idx].RailEnd)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Data.Blocks[BlockIndex].RailDike[idx].Exists = false;
											}
										}
									} break;
								case "track.marker":
								case "track.textmarker":
									{
										if (!PreviewOnly)
										{
											if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, "Track.Marker is expected to have at least one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Path.ContainsInvalidChars(Arguments[0])) {
												Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
												if (!System.IO.File.Exists(f) && Command.ToLowerInvariant() == "track.marker")
												{
													Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in Track.Marker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (System.IO.File.Exists(f) && f.ToLowerInvariant().EndsWith(".xml"))
													{
														Marker m = new Marker();
														m.StartingPosition = Data.TrackPosition;
														if (MarkerScriptParser.ReadMarkerXML(f, ref m))
														{
															int nn = Data.Markers.Length;
															Array.Resize<Marker>(ref Data.Markers, nn + 1);
															Data.Markers[nn] = m;
														}

														break;
													}
													double dist = Data.BlockInterval;
													if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out dist)) {
														Interface.AddMessage(MessageType.Error, false, "Distance is invalid in Track.Marker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														dist = Data.BlockInterval;
													}
													double start, end;
													if (dist < 0.0) {
														start = Data.TrackPosition;
														end = Data.TrackPosition - dist;
													} else {
														start = Data.TrackPosition - dist;
														end = Data.TrackPosition;
													}
													if (start < 0.0) start = 0.0;
													if (end < 0.0) end = 0.0;
													if (end <= start) end = start + 0.01;
													int n = Data.Markers.Length;
													Array.Resize<Marker>(ref Data.Markers, n + 1);
													Data.Markers[n].StartingPosition = start;
													Data.Markers[n].EndingPosition = end;
													if (Command.ToLowerInvariant() == "track.textmarker")
													{
														Data.Markers[n].Message = new MessageManager.MarkerText(Arguments[0]);
														if (Arguments.Length >= 3)
														{
															switch (Arguments[2].ToLowerInvariant())
															{
																case "black":
																case "1":
																	Data.Markers[n].Message.Color = MessageColor.Black;
																	break;
																case "gray":
																case "2":
																	Data.Markers[n].Message.Color = MessageColor.Gray;
																	break;
																case "white":
																case "3":
																	Data.Markers[n].Message.Color = MessageColor.White;
																	break;
																case "red":
																case "4":
																	Data.Markers[n].Message.Color = MessageColor.Red;
																	break;
																case "orange":
																case "5":
																	Data.Markers[n].Message.Color = MessageColor.Orange;
																	break;
																case "green":
																case "6":
																	Data.Markers[n].Message.Color = MessageColor.Green;
																	break;
																case "blue":
																case "7":
																	Data.Markers[n].Message.Color = MessageColor.Blue;
																	break;
																case "magenta":
																case "8":
																	Data.Markers[n].Message.Color = MessageColor.Magenta;
																	break;
																default:
																	Interface.AddMessage(MessageType.Error, false, "MessageColor is invalid in Track.TextMarker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																	//Default message color is set to white
																	break;
															}
														}
													}
													else
													{
														OpenBveApi.Textures.Texture t;
														Textures.RegisterTexture(f, new OpenBveApi.Textures.TextureParameters(null, new Color24(64, 64, 64)), out t);
														Data.Markers[n].Message = new MessageManager.MarkerImage(t);
														
													}
													
												}
											}
										}
									} break;
								case "track.height":
									{
										if (!PreviewOnly) {
											double h = 0.0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out h)) {
												Interface.AddMessage(MessageType.Error, false, "Height is invalid in Track.Height at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												h = 0.0;
											}
											Data.Blocks[BlockIndex].Height = IsRW ? h + 0.3 : h;
										}
									} break;
								case "track.ground":
									{
										if (!PreviewOnly) {
											int cytype = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out cytype)) {
												Interface.AddMessage(MessageType.Error, false, "CycleIndex is invalid in Track.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												cytype = 0;
											}
											if (cytype < Data.Structure.Cycles.Length && Data.Structure.Cycles[cytype] != null) {
												Data.Blocks[BlockIndex].Cycle = Data.Structure.Cycles[cytype];
											} else {
												if (!Data.Structure.Ground.ContainsKey(cytype)) {
													Interface.AddMessage(MessageType.Error, false, "CycleIndex " + cytype + " references an object not loaded in Track.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													Data.Blocks[BlockIndex].Cycle = new int[] { cytype };
												}
											}
										}
									} break;
								case "track.crack":
									{
										if (!PreviewOnly) {
											int idx1 = 0, idx2 = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx1 = 0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out idx2)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx2 = 0;
											}
											int sttype = 0;
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype)) {
												Interface.AddMessage(MessageType.Error, false, "CrackStructureIndex is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (sttype < 0 || !Data.Structure.CrackL.ContainsKey(sttype) || !Data.Structure.CrackR.ContainsKey(sttype)) {
												Interface.AddMessage(MessageType.Error, false, "CrackStructureIndex " + sttype + " references an object not loaded in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx1 < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (idx2 < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (idx1 == idx2) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be unequal to Index2 in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (idx1 >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx1].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													if (idx2 >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx2].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex2 " + idx2 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													int n = Data.Blocks[BlockIndex].Cracks.Length;
													Array.Resize<Crack>(ref Data.Blocks[BlockIndex].Cracks, n + 1);
													Data.Blocks[BlockIndex].Cracks[n].PrimaryRail = idx1;
													Data.Blocks[BlockIndex].Cracks[n].SecondaryRail = idx2;
													Data.Blocks[BlockIndex].Cracks[n].Type = sttype;
												}
											}
										}
									} break;
								case "track.freeobj":
									{
										if (!PreviewOnly) {
											int idx = 0, sttype = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype)) {
												Interface.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (idx < -1) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative or -1 in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (sttype < 0) {
												Interface.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is expected to be non-negative in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (!Data.Structure.FreeObjects.ContainsKey(sttype)) {
													Interface.AddMessage(MessageType.Error, false, "FreeObjStructureIndex " + sttype + " references an object not loaded in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													double x = 0.0, y = 0.0;
													double yaw = 0.0, pitch = 0.0, roll = 0.0;
													if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
														Interface.AddMessage(MessageType.Error, false, "X is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														x = 0.0;
													}
													if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
														Interface.AddMessage(MessageType.Error, false, "Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														y = 0.0;
													}
													if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw)) {
														Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														yaw = 0.0;
													}
													if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch)) {
														Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														pitch = 0.0;
													}
													if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll)) {
														Interface.AddMessage(MessageType.Error, false, "Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														roll = 0.0;
													}
													if (idx == -1) {
														int n = Data.Blocks[BlockIndex].GroundFreeObj.Length;
														Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].GroundFreeObj, n + 1);
														Data.Blocks[BlockIndex].GroundFreeObj[n].TrackPosition = Data.TrackPosition;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Type = sttype;
														Data.Blocks[BlockIndex].GroundFreeObj[n].X = x;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Y = y;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Yaw = yaw * 0.0174532925199433;
														if (!Data.IgnorePitchRoll)
														{
															Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = pitch * 0.0174532925199433;
															Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = roll * 0.0174532925199433;
														}
														else
														{
															Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = 0;
															Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = 0;
														}
													} else {
														if (idx >= Data.Blocks[BlockIndex].RailFreeObj.Length) {
															Array.Resize<FreeObj[]>(ref Data.Blocks[BlockIndex].RailFreeObj, idx + 1);
														}
														int n;
														if (Data.Blocks[BlockIndex].RailFreeObj[idx] == null) {
															Data.Blocks[BlockIndex].RailFreeObj[idx] = new FreeObj[1];
															n = 0;
														} else {
															n = Data.Blocks[BlockIndex].RailFreeObj[idx].Length;
															Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].RailFreeObj[idx], n + 1);
														}
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw * 0.0174532925199433;
														if (!Data.IgnorePitchRoll)
														{
															Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
															Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
														}
														else
														{
															Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = 0;
															Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = 0;
														}
													}
												}
											}
										}
									} break;
								case "track.back":
								case "track.background":
									{
										if (!PreviewOnly) {
											int typ = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out typ)) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												typ = 0;
											}
											if (typ < 0 | typ >= Data.Backgrounds.Length) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + typ + " references a texture not loaded in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Data.Backgrounds[typ] is BackgroundManager.StaticBackground) {
												BackgroundManager.StaticBackground b = Data.Backgrounds[typ] as BackgroundManager.StaticBackground;
												if (b.Texture == null)
												{
													//There's a possibility that this was loaded via a default BVE command rather than XML
													//Thus check for the existance of the file and chuck out error if appropriate
													Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + typ + " has not been loaded via Texture.Background in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												else
												{
													Data.Blocks[BlockIndex].Background = typ;
													if (Interface.CurrentOptions.EnableBveTsHacks && Data.Blocks.Length == 2 && Data.Blocks[0].Background == 0)
													{
														//The initial background for block 0 is always set to zero
														//This handles the case where background idx #0 is not used
														b = Data.Backgrounds[0] as BackgroundManager.StaticBackground;
														if (b.Texture == null)
														{
															Data.Blocks[0].Background = typ;
														}
													}
												}
											} else if (Data.Backgrounds[typ] is BackgroundManager.DynamicBackground)
											{
												//File existance checks should already have been made when loading the XML
												Data.Blocks[BlockIndex].Background = typ;
											}
											else {
												//Object based backgrounds not yet implemented
												Data.Blocks[BlockIndex].Background = typ;
											}
										}
									} break;
								case "track.announce":
									{
										if (!PreviewOnly) {
											if (Arguments.Length == 0) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateSound(ref f, SoundPath)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														double speed = 0.0;
														if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed)) {
															Interface.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															speed = 0.0;
														}
														int n = Data.Blocks[BlockIndex].SoundEvents.Length;
														Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
														Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
														const double radius = 15.0;
														Data.Blocks[BlockIndex].SoundEvents[n].SoundBuffer = Sounds.RegisterBuffer(f, radius);
														Data.Blocks[BlockIndex].SoundEvents[n].Type = speed == 0.0 ? SoundType.TrainStatic : SoundType.TrainDynamic;
														Data.Blocks[BlockIndex].SoundEvents[n].Speed = speed * Data.UnitOfSpeed;
													}
												}
											}
										}
									} break;
								case "track.doppler":
									{
										if (!PreviewOnly) {
											if (Arguments.Length == 0) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 3 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (Path.ContainsInvalidChars(Arguments[0])) {
													Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													string f = Arguments[0];
													if (!LocateSound(ref f, SoundPath)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														double x = 0.0, y = 0.0;
														if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
															Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															x = 0.0;
														}
														if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
															Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															y = 0.0;
														}
														int n = Data.Blocks[BlockIndex].SoundEvents.Length;
														Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
														Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
														const double radius = 15.0;
														Data.Blocks[BlockIndex].SoundEvents[n].SoundBuffer = Sounds.RegisterBuffer(f, radius);
														Data.Blocks[BlockIndex].SoundEvents[n].Type = SoundType.World;
														Data.Blocks[BlockIndex].SoundEvents[n].X = x;
														Data.Blocks[BlockIndex].SoundEvents[n].Y = y;
														Data.Blocks[BlockIndex].SoundEvents[n].Radius = radius;
													}
												}
											}
										}
									} break;
								case "track.micsound":
									{
										if (!PreviewOnly) {
											double x = 0.0, y = 0.0, back = 0.0, front = 0.0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x)) {
												Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												x = 0.0;
											}
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y)) {
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												y = 0.0;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out back)) {
												Interface.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												back = 0.0;
											} else if (back < 0.0) {
												Interface.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												back = 0.0;
											}
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out front)) {
												Interface.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												front = 0.0;
											} else if (front < 0.0) {
												Interface.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												front = 0.0;
											}
											int n = Data.Blocks[BlockIndex].SoundEvents.Length;
											Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
											Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].SoundEvents[n].Type = SoundType.World;
											Data.Blocks[BlockIndex].SoundEvents[n].X = x;
											Data.Blocks[BlockIndex].SoundEvents[n].Y = y;
											Data.Blocks[BlockIndex].SoundEvents[n].IsMicSound = true;
											Data.Blocks[BlockIndex].SoundEvents[n].BackwardTolerance = back;
											Data.Blocks[BlockIndex].SoundEvents[n].ForwardTolerance = front;
										}
									} break;
								case "track.pretrain":
									{
										if (!PreviewOnly) {
											if (Arguments.Length == 0) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have exactly 1 argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												double time;
												if (Arguments[0].Length > 0 & !Interface.TryParseTime(Arguments[0], out time)) {
													Interface.AddMessage(MessageType.Error, false, "Time is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													time = 0.0;
												}
												int n = Game.BogusPretrainInstructions.Length;
												if (n != 0 && Game.BogusPretrainInstructions[n - 1].Time >= time) {
													Interface.AddMessage(MessageType.Error, false, "Time is expected to be in ascending order between successive " + Command + " commands at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Array.Resize<Game.BogusPretrainInstruction>(ref Game.BogusPretrainInstructions, n + 1);
												Game.BogusPretrainInstructions[n].TrackPosition = Data.TrackPosition;
												Game.BogusPretrainInstructions[n].Time = time;
											}
										}
									} break;
								case "track.pointofinterest":
								case "track.poi":
									{
										if (!PreviewOnly) {
											int idx = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx < 0) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												idx = 0;
											}
											if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rails.Length || !Data.Blocks[BlockIndex].Rails[idx].RailStart)) {
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											double x = 0.0, y = 0.0;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
												Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												x = 0.0;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
												Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												y = 0.0;
											}
											double yaw = 0.0, pitch = 0.0, roll = 0.0;
											if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out yaw)) {
												Interface.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												yaw = 0.0;
											}
											if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out pitch)) {
												Interface.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												pitch = 0.0;
											}
											if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out roll)) {
												Interface.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												roll = 0.0;
											}
											string text = null;
											if (Arguments.Length >= 7 && Arguments[6].Length != 0) {
												text = Arguments[6];
											}
											int n = Data.Blocks[BlockIndex].PointsOfInterest.Length;
											Array.Resize<PointOfInterest>(ref Data.Blocks[BlockIndex].PointsOfInterest, n + 1);
											Data.Blocks[BlockIndex].PointsOfInterest[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].PointsOfInterest[n].RailIndex = idx;
											Data.Blocks[BlockIndex].PointsOfInterest[n].X = x;
											Data.Blocks[BlockIndex].PointsOfInterest[n].Y = y;
											Data.Blocks[BlockIndex].PointsOfInterest[n].Yaw = 0.0174532925199433 * yaw;
											Data.Blocks[BlockIndex].PointsOfInterest[n].Pitch = 0.0174532925199433 * pitch;
											Data.Blocks[BlockIndex].PointsOfInterest[n].Roll = 0.0174532925199433 * roll;
											Data.Blocks[BlockIndex].PointsOfInterest[n].Text = text;
										}
									} break;
								default:
									Interface.AddMessage(MessageType.Warning, false, "The command " + Command + " is not supported at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									break;
							}
						}
					}
				}
			}
			// blocks
			Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
		}
	}
}
