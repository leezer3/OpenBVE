using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.Stations;

namespace CsvRwRouteParser {
	internal partial class Parser {
		internal string ObjectPath;
		internal string SoundPath;
		internal string TrainPath;
		internal string CompatibilityFolder;
		internal static CompatabilityHacks EnabledHacks;
		internal bool SplitLineHack = true;
		internal bool AllowTrackPositionArguments = false;
		internal bool IsRW;
		internal bool IsHmmsim;

		internal Plugin Plugin;

		internal CurrentRoute CurrentRoute;
		// parse route
		internal void ParseRoute(string FileName, bool isRW, System.Text.Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, Plugin hostPlugin)
		{
			Plugin = hostPlugin;
			CurrentRoute = Plugin.CurrentRoute;
			/*
			 * Store paths for later use
			 */
			ObjectPath = objectPath;
			SoundPath = soundPath;
			TrainPath = trainPath;
			IsRW = isRW;
			if (!PreviewOnly)
			{
				for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
				{
					Plugin.CurrentHost.Plugins[i].Object?.SetObjectParser(SoundPath); //HACK: Pass out the sound folder path to those plugins which consume it
				}
			}
			freeObjCount = 0;
			railtypeCount = 0;
			Plugin.CurrentOptions.UnitOfSpeed = "km/h";
			Plugin.CurrentOptions.SpeedConversionFactor = 0.0;
			CompatibilityFolder = Plugin.FileSystem.GetDataFolder("Compatibility");
			if (!PreviewOnly)
			{
				CompatibilityObjects.LoadCompatibilityObjects(Path.CombineFile(CompatibilityFolder,"CompatibilityObjects.xml"));
			}

			RoutePatchDatabaseParser.LoadRoutePatchDatabase(ref availableRoutefilePatches);
			Plugin.CurrentOptions.ObjectDisposalMode = ObjectDisposalMode.Legacy;
			RouteData Data = new RouteData(PreviewOnly);
			
			if (!PreviewOnly)
			{
				Data.Blocks[0].Background = 0;
				Data.Blocks[0].Fog = new Fog(CurrentRoute.NoFogStart, CurrentRoute.NoFogEnd, Color24.Grey, 0);
				Data.Blocks[0].Cycle = new[] {-1};
				Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
				Data.Blocks[0].RailFreeObj = new Dictionary<int, List<FreeObj>>();
				Data.Blocks[0].GroundFreeObj = new List<FreeObj>();
				Data.Blocks[0].RailWall = new Dictionary<int, WallDike>();
				Data.Blocks[0].RailDike = new Dictionary<int, WallDike>();
				Data.Blocks[0].RailPole = new Pole[] {};
				string PoleFolder = Path.CombineDirectory(CompatibilityFolder, "Poles");
				Data.Structure.Poles = new PoleDictionary
				{
					{0, new ObjectDictionary()}, 
					{1, new ObjectDictionary()},
					{2, new ObjectDictionary()}, 
					{3, new ObjectDictionary()}
				};
				Data.Structure.Poles[0].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_1.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles[1].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_2.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles[2].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_3.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles[3].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_4.csv"), System.Text.Encoding.UTF8, false));
				
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
				Data.Backgrounds = new BackgroundDictionary();
				Data.TimetableDaytime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				Data.TimetableNighttime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				Data.Structure.WeatherObjects = new ObjectDictionary();
				Data.Structure.LightDefinitions = new Dictionary<int, LightDefinition[]>();
				if (Plugin.CurrentOptions.CurrentCompatibilitySignalSet == null) //not selected via main form
				{
					Plugin.CurrentOptions.CurrentCompatibilitySignalSet = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility"), "Signals\\Japanese.xml");
				}
				CompatibilitySignalObject.ReadCompatibilitySignalXML(Plugin.CurrentHost, Plugin.CurrentOptions.CurrentCompatibilitySignalSet, out Data.CompatibilitySignals, out CompatibilityObjects.SignalPost, out Data.SignalSpeeds);
				// game data
				CurrentRoute.Sections = new[]
				{
					new RouteManager2.SignalManager.Section(0, new[] { new SectionAspect(0, 0.0), new SectionAspect(4, double.PositiveInfinity) }, SectionType.IndexBased)
				};
				
				CurrentRoute.Sections[0].CurrentAspect = 0;
				CurrentRoute.Sections[0].StationIndex = -1;
			}
			ParseRouteForData(FileName, Encoding, ref Data, PreviewOnly);
			if (Plugin.Cancel)
			{
				Plugin.IsLoading = false;
				return;
			}
			ApplyRouteData(FileName, ref Data, PreviewOnly);
		}

		private void ParseRouteForData(string FileName, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly) {
			//Read the entire routefile into memory
			List<string> Lines = System.IO.File.ReadAllLines(FileName, Encoding).ToList();
			PreprocessSplitIntoExpressions(FileName, Lines, out Expression[] Expressions, true);
			PreprocessChrRndSub(FileName, Encoding, ref Expressions);
			double[] UnitOfLength = { 1.0 };
			//Set units of speed initially to km/h
			//This represents 1km/h in m/s
			Data.UnitOfSpeed = 0.277777777777778;
			PreprocessOptions(Expressions, ref Data, ref UnitOfLength, PreviewOnly);
			PreprocessSortByTrackPosition(UnitOfLength, ref Expressions);
			ParseRouteForData(FileName, Encoding, Expressions, UnitOfLength, ref Data, PreviewOnly);
			CurrentRoute.UnitOfLength = UnitOfLength;
		}
		
		private int freeObjCount;
		private int missingObjectCount;
		private int railtypeCount;
		private readonly System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

		// parse route for data
		private void ParseRouteForData(string FileName, System.Text.Encoding Encoding, Expression[] Expressions, double[] UnitOfLength, ref RouteData Data, bool PreviewOnly) {
			CurrentStation = -1;
			CurrentStop = -1;
			CurrentSection = 0;
			
			string Section = ""; bool SectionAlwaysPrefix = false;
			int BlockIndex = 0;
			CurrentRoute.Tracks[0].Direction = TrackDirection.Forwards;
			CurrentRoute.Stations = new RouteStation[] { };
			double progressFactor = Expressions.Length == 0 ? 0.3333 : 0.3333 / Expressions.Length;
			// process non-track namespaces
			//Check for any special-cased fixes we might need
			CheckForAvailablePatch(FileName, ref Data, ref Expressions, PreviewOnly);
			//Apply parameters to object loaders
			if (!PreviewOnly)
			{
				for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
				{
					if (Plugin.CurrentHost.Plugins[i].Object != null)
					{
						EnabledHacks.BveTsHacks = Plugin.CurrentOptions.EnableBveTsHacks;
						EnabledHacks.BlackTransparency = true;
						Plugin.CurrentHost.Plugins[i].Object.SetCompatibilityHacks(EnabledHacks);
						//Remember that these will be ignored if not the correct plugin
						Plugin.CurrentHost.Plugins[i].Object.SetObjectParser(Plugin.CurrentOptions.CurrentXParser);
						Plugin.CurrentHost.Plugins[i].Object.SetObjectParser(Plugin.CurrentOptions.CurrentObjParser);
					}
				}
			}
			
			for (int j = 0; j < Expressions.Length; j++) {
				Plugin.CurrentProgress = j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel)
					{
						Plugin.IsLoading = false;
						return;
					}
				}
				if (Expressions[j].Text.StartsWith("[") && Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					// find equals
					if (IsRW)
					{
						Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					}
					
					// separate command and arguments
					Expressions[j].SeparateCommandsAndArguments(out string Command, out string ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.IsValidDouble(Command, UnitOfLength)) {
						// track position (ignored)
					} else {
						string[] Arguments = SplitArguments(ArgumentSequence);

						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							SectionAlwaysPrefix = false;
							Section = Arguments.Length >= 1 ? Arguments[0] : string.Empty;
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
							
							if (Command.StartsWith("structure", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.backgroundx" + Command.Substring(18, Command.Length - 20).TrimEnd();
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.backgroundaspect" + Command.Substring(18, Command.Length - 25).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.backgroundx" + Command.Substring(14, Command.Length - 16).TrimEnd();
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.backgroundaspect" + Command.Substring(14, Command.Length - 21).TrimEnd();
							} else if (Command.StartsWith("cycle", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".params", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 7).TrimEnd();
							} else if (Command.StartsWith("signal", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd();
							} else if (Command.StartsWith("train.run", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.flange", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".day.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 24).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".night.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 26).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".day", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 19).Trim();
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".night", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 21).Trim();
							} else if (Command.StartsWith("route.signal", StringComparison.OrdinalIgnoreCase) && Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))  {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd();
							} else if (Command.StartsWith("route.runinterval", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.interval" + Command.Substring(17, Command.Length - 17);
							} else if (Command.StartsWith("train.gauge", StringComparison.OrdinalIgnoreCase)) {
								Command = "route.gauge" + Command.Substring(11, Command.Length - 11);
							} else if (Command.StartsWith("texture.", StringComparison.OrdinalIgnoreCase)) {
								Command = "structure." + Command.Substring(8, Command.Length - 8);
							}
							//Needed after the initial processing to make the enum parse work
							Command = Command.Replace("timetable.day", "timetableday");
							Command = Command.Replace("timetable.night", "timetablenight");
						}

						int[] commandIndices = FindIndices(ref Command, Expressions[j]);

						// process command
						if (!string.IsNullOrEmpty(Command))
						{
							int period = Command.IndexOf('.');
							string nameSpace = string.Empty;
							if (period != -1)
							{
								nameSpace = Command.Substring(0, period).ToLowerInvariant();
								Command = Command.Substring(period + 1);

							}
							Command = Command.ToLowerInvariant();
							
							switch (nameSpace)
							{
								case "options":
									if (Enum.TryParse(Command, true, out OptionsCommand parsedOptionCommand))
									{
										ParseOptionCommand(parsedOptionCommand, Arguments, UnitOfLength, Expressions[j], ref Data, PreviewOnly);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognised command " + Command + " encountered in the Options namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "route":
									if (Enum.TryParse(Command, true, out RouteCommand parsedRouteCommand))
									{
										ParseRouteCommand(parsedRouteCommand, Arguments, commandIndices[0], FileName, UnitOfLength, Expressions[j], ref Data, PreviewOnly);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognised command " + Command + " encountered in the Route namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "train":
									if (Enum.TryParse(Command.Split(' ')[0], true, out TrainCommand parsedTrainCommand))
									{
										ParseTrainCommand(parsedTrainCommand, Arguments, commandIndices[0], Expressions[j], ref Data, PreviewOnly);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognised command " + Command + " encountered in the Train namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "structure":
								case "texture":
									if (Enum.TryParse(Command, true, out StructureCommand parsedStructureCommand))
									{
										ParseStructureCommand(parsedStructureCommand, Arguments, commandIndices, FileName, Encoding, Expressions[j], ref Data, PreviewOnly);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognised command " + Command + " encountered in the Structure namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "":
									ParseSignalCommand(Command, Arguments, commandIndices[0], Encoding, Expressions[j], ref Data, PreviewOnly);
									break;
								case "cycle":
									if (Enum.TryParse(Command, true, out CycleCommand parsedCycleCommand))
									{
										ParseCycleCommand(parsedCycleCommand, Arguments, commandIndices[0], Expressions[j], ref Data, PreviewOnly);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognised command " + Command + " encountered in the Cycle namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									break;
								case "track":
									break;
								// ReSharper disable once RedundantEmptySwitchSection
								default:
									/*
									 * This needs an unrecognised command at some stage
									 */
									break;
							}
						}
					}
				}
			}

			Data.Blocks[0].LightDefinition = new LightDefinition(Plugin.CurrentRoute.Atmosphere.AmbientLightColor, Plugin.CurrentRoute.Atmosphere.DiffuseLightColor, Plugin.CurrentRoute.Atmosphere.LightPosition, -1, -1);
			Data.Blocks[0].DynamicLightDefinition = int.MaxValue;
			// process track namespace
			for (int j = 0; j < Expressions.Length; j++) {
				Plugin.CurrentProgress = 0.3333 + j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel)
					{
						Plugin.IsLoading = false;
						return;
					}
				}
				if (Data.LineEndingFix)
				{
					if (Expressions[j].Text.EndsWith("_"))
					{
						Expressions[j].Text = Expressions[j].Text.Substring(0, Expressions[j].Text.Length - 1).Trim();
					}
				}
				if (Expressions[j].Text.StartsWith("[") && Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					if (IsRW)
					{
						Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					}
					// separate command and arguments
					Expressions[j].SeparateCommandsAndArguments(out string Command, out string ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.TryParseDouble(Command, UnitOfLength, out double currentTrackPosition)) {
						// track position
						if (ArgumentSequence.Length != 0) {
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "A track position must not contain any arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							if (AllowTrackPositionArguments)
							{
								Data.TrackPosition = currentTrackPosition;
								BlockIndex = (int)Math.Floor(currentTrackPosition / Data.BlockInterval + 0.001);
								if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
								Data.CreateMissingBlocks(BlockIndex, PreviewOnly);
							}
						} else if (currentTrackPosition < 0.0) {
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Negative track position encountered at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						} else {
							if (Plugin.CurrentOptions.EnableBveTsHacks && IsRW && currentTrackPosition == 4535545100)
							{
								//WMATA Red line has an erroneous track position causing an out of memory cascade
								currentTrackPosition = 45355;
							}
							Data.TrackPosition = currentTrackPosition;
							BlockIndex = (int)Math.Floor(currentTrackPosition / Data.BlockInterval + 0.001);
							if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
							Data.CreateMissingBlocks(BlockIndex, PreviewOnly);
						}
					} else {
						string[] Arguments = SplitArguments(ArgumentSequence);
						
						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							SectionAlwaysPrefix = false;
							Section = Arguments.Length >= 1 ? Arguments[0] : string.Empty;
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
						}
						
						// process command
						if (!string.IsNullOrEmpty(Command)) {
							int period = Command.IndexOf('.');
							string nameSpace = string.Empty;
							if (period != -1)
							{
								nameSpace = Command.Substring(0, period).ToLowerInvariant();
								Command = Command.Substring(period + 1);
							}
							if (nameSpace.StartsWith("signal", StringComparison.InvariantCultureIgnoreCase))
							{
								nameSpace = "";
							}
							Command = Command.ToLowerInvariant();

							switch (nameSpace)
							{
								case "track":
									if (Enum.TryParse(Command, true, out TrackCommand parsedCommand))
									{
										ParseTrackCommand(parsedCommand, Arguments, FileName, UnitOfLength, Expressions[j], ref Data, BlockIndex, PreviewOnly, IsRW);
									}
									else
									{
										if (IsHmmsim)
										{
											period = Command.IndexOf('.');
											string railKey = Command.Substring(0, period);
											int railIndex = Data.RailKeys.Count;
											if (Data.RailKeys.ContainsKey(railKey))
											{
												railIndex = Data.RailKeys[railKey];
											}
											else
											{
												Data.RailKeys.Add(railKey, railIndex);
											}
											Command = Command.Substring(period + 1);
											if (Enum.TryParse(Command, true, out parsedCommand))
											{
												ParseTrackCommand(parsedCommand, Arguments, FileName, UnitOfLength, Expressions[j], ref Data, BlockIndex, PreviewOnly, IsRW);
											}
											else
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Hmmsim: Unrecognised command " + Command + " encountered in the Track namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);	
											}
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "OpenBVE: Unrecognised command " + Command + " encountered in the Track namespace at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);	
										}
										
									}
									break;
								case "options":
								case "route":
								case "train":
								case "structure":
								case "texture":
								case "":
								case "cycle":
									break;
								default:
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The command " + Command + " is not supported at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									break;
							}
							
						}
					}
				}
			}
		}
	}
}
