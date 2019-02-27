using System;
using System.Globalization;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.World;
using OpenBveApi.Textures;
using OpenBveApi.Objects;
using OpenBveApi.Interface;
using OpenBveApi.Trains;

namespace OpenBve {
	internal class CsvRwRouteParser {

		/// <inheritdoc />
		/// <summary>Defines a dictionary of objects</summary>
		private class ObjectDictionary : Dictionary<int, UnifiedObject>
		{
			/// <summary>Adds a new Unified Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="unifiedObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, UnifiedObject unifiedObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = unifiedObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					this.Add(key, unifiedObject);
				}
			}

			/// <summary>Adds a new Static Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="staticObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, ObjectManager.StaticObject staticObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = staticObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					this.Add(key, staticObject);
				}
			}
		}

		// structures
		private struct Rail {
			internal bool RailStart;
			internal bool RailStartRefreshed;
			internal double RailStartX;
			internal double RailStartY;
			internal bool RailEnd;
			internal double RailEndX;
			internal double RailEndY;
		}
		private struct WallDike {
			internal bool Exists;
			internal int Type;
			internal int Direction;
		}
		private struct FreeObj {
			internal double TrackPosition;
			internal int Type;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct Pole {
			internal bool Exists;
			internal int Mode;
			internal double Location;
			internal double Interval;
			internal int Type;
		}
		private struct Form {
			internal int PrimaryRail;
			internal int SecondaryRail;
			internal int FormType;
			internal int RoofType;
			internal const int SecondaryRailStub = 0;
			internal const int SecondaryRailL = -1;
			internal const int SecondaryRailR = -2;
		}
		private struct Crack {
			internal int PrimaryRail;
			internal int SecondaryRail;
			internal int Type;
		}
		private struct Signal {
			internal double TrackPosition;
			internal int Section;
			internal int SignalCompatibilityObjectIndex;
			internal int SignalObjectIndex;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal bool ShowObject;
			internal bool ShowPost;
		}
		private struct Section {
			internal double TrackPosition;
			internal int[] Aspects;
			internal int DepartureStationIndex;
			internal bool Invisible;
			internal Game.SectionType Type;
		}
		private struct Limit {
			internal double TrackPosition;
			internal double Speed;
			internal int Direction;
			internal int Cource;
		}
		private struct Stop {
			internal double TrackPosition;
			internal int Station;
			internal int Direction;
			internal double ForwardTolerance;
			internal double BackwardTolerance;
			internal int Cars;
		}
		private struct Brightness {
			internal double TrackPosition;
			internal float Value;
		}
		private struct Marker {
			internal double StartingPosition;
			internal double EndingPosition;
			internal Texture Texture;
		}
		private enum SoundType { World, TrainStatic, TrainDynamic }
		private struct Sound {
			internal double TrackPosition;
			internal Sounds.SoundBuffer SoundBuffer;
			internal SoundType Type;
			internal double X;
			internal double Y;
			internal double Radius;
			internal double Speed;
		}
		private struct Transponder {
			internal double TrackPosition;
			internal TrackManager.TransponderType Type;
			internal bool ShowDefaultObject;
			internal bool SwitchSubsystem;
			internal int BeaconStructureIndex;
			internal int OptionalInteger;
			internal double OptionalFloat;
			internal int Section;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct DestinationEvent
		{
			internal double TrackPosition;
			internal int Type;
			internal bool TriggerOnce;
			internal int BeaconStructureIndex;
			internal int NextDestination;
			internal int PreviousDestination;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}
		private struct PointOfInterest {
			internal double TrackPosition;
			internal int RailIndex;
			internal double X;
			internal double Y;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal string Text;
		}
        private struct RailCycle {
            internal int RailCycleIndex;
            internal int CurrentCycle;
        }
        private class Block {
			internal int Background;
			internal Brightness[] Brightness;
			internal Game.Fog Fog;
			internal bool FogDefined;
			internal int[] Cycle;
			internal RailCycle[] RailCycle;
			internal double Height;
			internal Rail[] Rail;
			internal int[] RailType;
			internal WallDike[] RailWall;
			internal WallDike[] RailDike;
			internal Pole[] RailPole;
			internal FreeObj[][] RailFreeObj;
			internal FreeObj[] GroundFreeObj;
			internal Form[] Form;
			internal Crack[] Crack;
			internal Signal[] Signal;
			internal Section[] Section;
			internal Limit[] Limit;
			internal Stop[] Stop;
			internal Sound[] Sound;
			internal Transponder[] Transponder;
	        internal DestinationEvent[] DestinationChanges;
			internal PointOfInterest[] PointsOfInterest;
			internal TrackManager.TrackElement CurrentTrackState;
			internal double Pitch;
			internal double Turn;
			internal int Station;
			internal bool StationPassAlarm;
			internal double Accuracy;
			internal double AdhesionMultiplier;
		}
		private struct StructureData
		{
			internal ObjectDictionary RailObjects;
			internal UnifiedObject[][] Poles;
			internal ObjectDictionary Ground;
			internal ObjectDictionary WallL;
			internal ObjectDictionary WallR;
			internal ObjectDictionary DikeL;
			internal ObjectDictionary DikeR;
			internal ObjectDictionary FormL;
			internal ObjectDictionary FormR;
			internal ObjectDictionary FormCL;
			internal ObjectDictionary FormCR;
			internal ObjectDictionary RoofL;
			internal ObjectDictionary RoofR;
			internal ObjectDictionary RoofCL;
			internal ObjectDictionary RoofCR;
			internal ObjectDictionary CrackL;
			internal ObjectDictionary CrackR;
			internal ObjectDictionary FreeObjects;
			internal ObjectDictionary Beacon;
			internal int[][] Cycle;
			internal int[][] RailCycle;
			internal int[] Run;
			internal int[] Flange;
		}
		private abstract class SignalData { }
		private class Bve4SignalData : SignalData {
			internal ObjectManager.StaticObject BaseObject;
			internal ObjectManager.StaticObject GlowObject;
			internal Texture[] SignalTextures;
			internal Texture[] GlowTextures;
		}
		private class CompatibilitySignalData : SignalData {
			internal int[] Numbers;
			internal ObjectManager.StaticObject[] Objects;
			internal CompatibilitySignalData(int[] Numbers, ObjectManager.StaticObject[] Objects) {
				this.Numbers = Numbers;
				this.Objects = Objects;
			}
		}
		private class AnimatedObjectSignalData : SignalData {
			internal ObjectManager.AnimatedObjectCollection Objects;
		}
		private struct RouteData {
			internal double TrackPosition;
			internal double BlockInterval;
			internal double UnitOfSpeed;
			internal bool AccurateObjectDisposal;
			internal bool SignedCant;
			internal bool FogTransitionMode;
			internal StructureData Structure;
			internal SignalData[] SignalData;
			internal CompatibilitySignalData[] CompatibilitySignalData;
			internal int[] TimetableDaytime;
			internal int[] TimetableNighttime;
			internal World.Background[] Backgrounds;
			internal double[] SignalSpeeds;
			internal Block[] Blocks;
			internal Marker[] Markers;
			internal int FirstUsedBlock;
		}

		// parse route
		internal static void ParseRoute(string FileName, bool IsRW, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, bool PreviewOnly) {
			// initialize data
			string CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
			RouteData Data = new RouteData();
			Data.BlockInterval = 25.0;
			Data.AccurateObjectDisposal = false;
			Data.FirstUsedBlock = -1;
			Data.Blocks = new Block[1];
			Data.Blocks[0] = new Block();
			Data.Blocks[0].Rail = new Rail[1];
			Data.Blocks[0].Rail[0].RailStart = true;
			Data.Blocks[0].RailType = new int[] { 0 };
			Data.Blocks[0].Limit = new Limit[] { };
			Data.Blocks[0].Stop = new Stop[] { };
			Data.Blocks[0].Station = -1;
			Data.Blocks[0].StationPassAlarm = false;
			Data.Blocks[0].Accuracy = 2.0;
			Data.Blocks[0].AdhesionMultiplier = 1.0;
			Data.Blocks[0].CurrentTrackState = new TrackManager.TrackElement(0.0);
            if (!PreviewOnly) {
                Data.Blocks[0].Background = 0;
                Data.Blocks[0].Brightness = new Brightness[] { };
                Data.Blocks[0].Fog.Start = Game.NoFogStart;
                Data.Blocks[0].Fog.End = Game.NoFogEnd;
                Data.Blocks[0].Fog.Color = Color24.Grey;
                Data.Blocks[0].Cycle = new int[] { -1 };
                Data.Blocks[0].RailCycle = new RailCycle[1];
                Data.Blocks[0].RailCycle[0].RailCycleIndex = -1;
				Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
				Data.Blocks[0].RailFreeObj = new FreeObj[][] { };
				Data.Blocks[0].GroundFreeObj = new FreeObj[] { };
				Data.Blocks[0].RailWall = new WallDike[] { };
				Data.Blocks[0].RailDike = new WallDike[] { };
				Data.Blocks[0].RailPole = new Pole[] { };
				Data.Blocks[0].Form = new Form[] { };
				Data.Blocks[0].Crack = new Crack[] { };
				Data.Blocks[0].Signal = new Signal[] { };
				Data.Blocks[0].Section = new Section[] { };
				Data.Blocks[0].Sound = new Sound[] { };
				Data.Blocks[0].Transponder = new Transponder[] { };
	            Data.Blocks[0].DestinationChanges = new DestinationEvent[] { };
				Data.Blocks[0].PointsOfInterest = new PointOfInterest[] { };
				Data.Markers = new Marker[] { };
				string PoleFolder = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Poles");
				Data.Structure.Poles = new UnifiedObject[][] {
					new UnifiedObject[] {
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (PoleFolder, "pole_1.csv"), Encoding, false)
					}, new UnifiedObject[] {
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (PoleFolder, "pole_2.csv"), Encoding, false)
					}, new UnifiedObject[] {
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (PoleFolder, "pole_3.csv"), Encoding, false)
					}, new UnifiedObject[] {
						ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (PoleFolder, "pole_4.csv"), Encoding, false)
					}
				};
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
				Data.Structure.Cycle = new int[][] { };
				Data.Structure.RailCycle = new int[][] { };
                Data.Structure.Run = new int[] { };
				Data.Structure.Flange = new int[] { };
				Data.Backgrounds = new World.Background[] { };
				Data.TimetableDaytime = new int[] { -1, -1, -1, -1 };
				Data.TimetableNighttime = new int[] { -1, -1, -1, -1 };
				// signals
				string SignalFolder = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				Data.SignalData = new SignalData[7];
				Data.SignalData[3] = new CompatibilitySignalData(new int[] { 0, 2, 4 }, new ObjectManager.StaticObject[] {
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_3_0.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_3_2.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_3_4.csv"), Encoding, false)
																 });
				Data.SignalData[4] = new CompatibilitySignalData(new int[] { 0, 1, 2, 4 }, new ObjectManager.StaticObject[] {
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_4_0.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_4a_1.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_4a_2.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile (SignalFolder, "signal_4a_4.csv"), Encoding, false)
																 });
				Data.SignalData[5] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4 }, new ObjectManager.StaticObject[] {
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5a_1.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), Encoding, false)
																 });
				Data.SignalData[6] = new CompatibilitySignalData(new int[] { 0, 3, 4 }, new ObjectManager.StaticObject[] {
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_0.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_3.csv"), Encoding, false),
																	ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_4.csv"), Encoding, false)
																 });
				// compatibility signals
				Data.CompatibilitySignalData = new CompatibilitySignalData[9];
				Data.CompatibilitySignalData[0] = new CompatibilitySignalData(new int[] { 0, 2 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2a_2.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[1] = new CompatibilitySignalData(new int[] { 0, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_2b_4.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[2] = new CompatibilitySignalData(new int[] { 0, 2, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_3_4.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[3] = new CompatibilitySignalData(new int[] { 0, 1, 2, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_1.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4a_4.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[4] = new CompatibilitySignalData(new int[] { 0, 2, 3, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_3.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_4b_4.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[5] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5a_1.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[6] = new CompatibilitySignalData(new int[] { 0, 2, 3, 4, 5 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_3.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5_4.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_5b_5.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[7] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4, 5 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_1.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_2.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_3.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_4.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "signal_6_5.csv"), Encoding, false)
																			  });
				Data.CompatibilitySignalData[8] = new CompatibilitySignalData(new int[] { 0, 3, 4 }, new ObjectManager.StaticObject[] {
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_0.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_3.csv"), Encoding, false),
																				ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalFolder, "repeatingsignal_4.csv"), Encoding, false)
																			  });
				// game data
				Game.Sections = new Game.Section[1];
				Game.Sections[0].Aspects = new Game.SectionAspect[] { new Game.SectionAspect(0, 0.0), new Game.SectionAspect(4, double.PositiveInfinity) };
				Game.Sections[0].CurrentAspect = 0;
				Game.Sections[0].NextSection = -1;
				Game.Sections[0].PreviousSection = -1;
				Game.Sections[0].StationIndex = -1;
				Game.Sections[0].TrackPosition = 0;
				Game.Sections[0].Trains = new TrainManager.Train[] { };
				// continue
				Data.SignalSpeeds = new double[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };
			}
			ParseRouteForData(FileName, IsRW, Encoding, TrainPath, ObjectPath, SoundPath, ref Data, PreviewOnly);
			if (Loading.Cancel) return;
			ApplyRouteData(FileName, Encoding, ref Data, PreviewOnly);
		}

		// ================================

		// parse route for data
		private class Expression {
			internal string File;
			internal string Text;
			internal int Line;
			internal int Column;
			internal double TrackPositionOffset;
		}
		private static void ParseRouteForData(string FileName, bool IsRW, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, ref RouteData Data, bool PreviewOnly) {
			// parse
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			Expression[] Expressions;
			PreprocessSplitIntoExpressions(FileName, IsRW, Lines, Encoding, out Expressions, true, 0.0);
			PreprocessChrRndSub(FileName, IsRW, ref Expressions);
			double[] UnitOfLength = new double[] { 1.0 };
			Data.UnitOfSpeed = 0.277777777777778;
			PreprocessOptions(FileName, IsRW, Encoding, Expressions, ref Data, ref UnitOfLength);
			PreprocessSortByTrackPosition(FileName, IsRW, UnitOfLength, ref Expressions);
			ParseRouteForData(FileName, IsRW, Encoding, Expressions, TrainPath, ObjectPath, SoundPath, UnitOfLength, ref Data, PreviewOnly);
			Game.RouteUnitOfLength = UnitOfLength;
		}

		// preprocess split into expressions
		private static void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, System.Text.Encoding Encoding, out Expression[] Expressions, bool AllowRwRouteDescription, double trackPositionOffset) {
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
								Level--;
								break;
							case ',':
								if (Level == 0 & !IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim();
									if (t.Length > 0 && !t.StartsWith(";")) {
										Expressions[e] = new Expression();
										Expressions[e].File = FileName;
										Expressions[e].Text = t;
										Expressions[e].Line = i + 1;
										Expressions[e].Column = c + 1;
										Expressions[e].TrackPositionOffset = trackPositionOffset;
										e++;
									}
									a = j + 1;
									c++;
								}
								break;
							case '@':
								if (Level == 0 & IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim();
									if (t.Length > 0 && !t.StartsWith(";")) {
										Expressions[e] = new Expression();
										Expressions[e].File = FileName;
										Expressions[e].Text = t;
										Expressions[e].Line = i + 1;
										Expressions[e].Column = c + 1;
										Expressions[e].TrackPositionOffset = trackPositionOffset;
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
							Expressions[e] = new Expression();
							Expressions[e].File = FileName;
							Expressions[e].Text = t;
							Expressions[e].Line = i + 1;
							Expressions[e].Column = c + 1;
							Expressions[e].TrackPositionOffset = trackPositionOffset;
							e++;
						}
					}
				}
			}
			Array.Resize<Expression>(ref Expressions, e);
		}

		// preprocess chrrndsub
		private static void PreprocessChrRndSub(string FileName, bool IsRW, ref Expression[] Expressions) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.Encoding Encoding = new System.Text.ASCIIEncoding();
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
									} else {
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
												break;
											} else if (2 * ia + 1 < args.Length) {
												if (!NumberFormats.TryParseDoubleVb6(args[2 * ia + 1], out weights[ia])) {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "A weight is invalid in " + t + Epilog);
													break;
												} else if (weights[ia] <= 0.0) {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "A weight is not positive in " + t + Epilog);
													break;
												} else {
													weightsTotal += weights[ia];
												}
											} else {
												weights[ia] = 1.0;
												weightsTotal += 1.0;
											}
										}
										if (count == 0) {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "No file was specified in " + t + Epilog);
											break;
										} else if (!continueWithNextExpression) {
											double number = Game.Generator.NextDouble() * weightsTotal;
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
											string[] lines = System.IO.File.ReadAllLines(files[chosenIndex], Encoding);
											PreprocessSplitIntoExpressions(files[chosenIndex], IsRW, lines, Encoding, out expr, false, offsets[chosenIndex] + Expressions[i].TrackPositionOffset);
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
									}
									break;
								case "$chr":
									{
										int x;
										if (NumberFormats.TryParseIntVb6(s, out x)) {
											if (x > 0 & x < 128) {
												Expressions[i].Text = Expressions[i].Text.Substring(0, j) + new string(Encoding.GetChars(new byte[] { (byte)x })) + Expressions[i].Text.Substring(h + 1);
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index does not correspond to a valid ASCII character in " + t + Epilog);
											}
										} else {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
										}
									} break;
								case "$rnd":
									{
										int m = s.IndexOf(";", StringComparison.Ordinal);
										if (m >= 0) {
											string s1 = s.Substring(0, m).TrimEnd();
											string s2 = s.Substring(m + 1).TrimStart();
											int x; if (NumberFormats.TryParseIntVb6(s1, out x)) {
												int y; if (NumberFormats.TryParseIntVb6(s2, out y)) {
													int z = x + (int)Math.Floor(Game.Generator.NextDouble() * (double)(y - x + 1));
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
		private static void PreprocessOptions(string FileName, bool IsRW, System.Text.Encoding Encoding, Expression[] Expressions, ref RouteData Data, ref double[] UnitOfLength) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Section = "";
			bool SectionAlwaysPrefix = false;
			// process expressions
			for (int j = 0; j < Expressions.Length; j++) {
				if (IsRW && Expressions[j].Text.StartsWith("[") && Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					// find equals
					int Equals = Expressions[j].Text.IndexOf('=');
					if (Equals >= 0) {
						// handle RW cycle syntax
						string t = Expressions[j].Text.Substring(0, Equals);
						if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Ground(" + t + ")";
							}
						} else if (Section.ToLowerInvariant() == "signal" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Void(" + t + ")";
							}
						}
						// convert RW style into CSV style
						Expressions[j].Text = t + " " + Expressions[j].Text.Substring(Equals + 1);
					}
					// separate command and arguments
					string Command, ArgumentSequence;
					SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, Expressions[j].File, j, true);
					// process command
					double Number;
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (!NumberCheck || !NumberFormats.TryParseDoubleVb6(Command, UnitOfLength, out Number)) {
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
											Command = null; break;
										} else if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2)) {
											Command = null; break;
										}
									} else {
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1)) {
											Command = null; break;
										}
									}
									break;
								}
							}
						}
						// process command
						if (Command != null) {
							switch (Command.ToLowerInvariant()) {
									// options
								case "options.unitoflength":
									{
										if (Arguments.Length == 0) {
											Interface.AddMessage(MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											UnitOfLength = new double[Arguments.Length];
											for (int i = 0; i < Arguments.Length; i++) {
												UnitOfLength[i] = i == Arguments.Length - 1 ? 1.0 : 0.0;
												if (Arguments[i].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[i], out UnitOfLength[i])) {
													Interface.AddMessage(MessageType.Error, false, "FactorInMeters" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													UnitOfLength[i] = i == 0 ? 1.0 : 0.0;
												} else if (UnitOfLength[i] <= 0.0) {
													Interface.AddMessage(MessageType.Error, false, "FactorInMeters" + i.ToString(Culture) + " is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													UnitOfLength[i] = i == Arguments.Length - 1 ? 1.0 : 0.0;
												}
											}
										}
									} break;
								case "options.unitofspeed":
									{
										if (Arguments.Length < 1) {
											Interface.AddMessage(MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											if (Arguments.Length > 1) {
												Interface.AddMessage(MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											if (Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Data.UnitOfSpeed)) {
												Interface.AddMessage(MessageType.Error, false, "FactorInKmph is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												Data.UnitOfSpeed = 0.277777777777778;
											} else if (Data.UnitOfSpeed <= 0.0) {
												Interface.AddMessage(MessageType.Error, false, "FactorInKmph is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												Data.UnitOfSpeed = 0.277777777777778;
											} else {
												Data.UnitOfSpeed *= 0.277777777777778;
											}
										}
									} break;
								case "options.objectvisibility":
									{
										if (Arguments.Length == 0) {
											Interface.AddMessage(MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										} else {
											if (Arguments.Length > 1) {
												Interface.AddMessage(MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
											int mode = 0;
											if (Arguments.Length >= 1 && Arguments[0].Length != 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out mode)) {
												Interface.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												mode = 0;
											} else if (mode != 0 & mode != 1) {
												Interface.AddMessage(MessageType.Error, false, "The specified Mode is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												mode = 0;
											}
											Data.AccurateObjectDisposal = mode == 1;
										}
									} break;
								case "options.enablebvetshacks":
								case "options.compatibletransparencymode":
									if (Arguments[0].Trim() == "1")
									{
										Interface.AddMessage(MessageType.Information, false, Command + " is intended to fix issues with older content, is only supported in openBVE 1.5.2+ , and has no effect in RouteViewer. Please see the documentation for further details");
									}
									break;
							}
						}
					}
				}
			}
		}

		// preprocess sort by track position
		private struct PositionedExpression {
			internal double TrackPosition;
			internal Expression Expression;
		}
		private static void PreprocessSortByTrackPosition(string FileName, bool IsRW, double[] UnitFactors, ref Expression[] Expressions) {
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
			bool fpf = false;
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
				//Finds the first non-default track position
				if(p[i].TrackPosition != -1 && fpf == false)
				{
					//We know that the withTrack section starts with the first NON -1 element
					//**APPEARS TO ALWAYS BE ZERO**
					//Subsequent declarations in the same block add 1 to this index
					//Thus, the first 'actual' user set track position is somewhere ~5 elements ahead
					//Of course the route can also start at zero, which means it could be a little further on!
					int j = i;
					while (j < p.Length)
					{
						j++;
						if (p[j].TrackPosition != p[i].TrackPosition)
						{
							//First track position found!
							Program.MinimumJumpToPositionValue = p[j].TrackPosition;
							fpf = true;
							//Break out the while loop
							break;
						}
					}
					
					
				}
			}
			
			Array.Resize<Expression>(ref e, m);
			Expressions = e;
		}

		// separate commands and arguments
		private static void SeparateCommandsAndArguments(Expression Expression, out string Command, out string ArgumentSequence, System.Globalization.CultureInfo Culture, string FileName, int LineNumber, bool RaiseErrors) {
			bool openingerror = false, closingerror = false;
			int i, fcb = 0;
			for (i = 0; i < Expression.Text.Length; i++) {
				if (Expression.Text[i] == '(')
				{
					bool found = false;
					bool stationName = false;
					bool replaced = false;
					i++;
					while (i < Expression.Text.Length)
					{
						if (Expression.Text[i] == ',' || Expression.Text[i] == ';')
						{
							//Only check parenthesis in the station name field- The comma and semi-colon are the argument separators
							stationName = true;
						}
						if (Expression.Text[i] == '(')
						{
							if (RaiseErrors & !openingerror)
							{
								if (stationName)
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " +
										Expression.Column.ToString(Culture) + " in file " + Expression.File);
									openingerror = true;
								}
								else
								{
									Expression.Text = Expression.Text.Remove(i, 1).Insert(i, "[");
									replaced = true;
								}
							}
						}
						else if (Expression.Text[i] == ')')
						{
							if (stationName == false && i != Expression.Text.Length && replaced == true)
							{
								Expression.Text = Expression.Text.Remove(i, 1).Insert(i, "]");
								continue;
							}
							found = true;
							fcb = i;
							break;
						}
						i++;
					}
					if (!found) {
						if (RaiseErrors & !closingerror) {
							Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							closingerror = true;
						}
						Expression.Text += ")";
					}
				} else if (Expression.Text[i] == ')') {
					if (RaiseErrors & !closingerror) {
						Interface.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						closingerror = true;
					}
				} else if (char.IsWhiteSpace(Expression.Text[i])) {
					if (i >= Expression.Text.Length - 1 || !char.IsWhiteSpace(Expression.Text[i + 1])) {
						break;
					}
				}
			}
			if (fcb != 0 && fcb < Expression.Text.Length - 1)
			{
				if (!Char.IsWhiteSpace(Expression.Text[fcb + 1]) && Expression.Text[fcb + 1] != '.' && Expression.Text[fcb + 1] != ';')
				{
					Expression.Text = Expression.Text.Insert(fcb + 1, " ");
					i = fcb;
				}
			}
			if (i < Expression.Text.Length) {
				// white space was found outside of parentheses
				string a = Expression.Text.Substring(0, i);
				if (a.IndexOf('(') >= 0 & a.IndexOf(')') >= 0) {
					// indices found not separated from the command by spaces
					Command = Expression.Text.Substring(0, i).TrimEnd();
					ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
					if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
						// arguments are enclosed by parentheses
						ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
					} else if (ArgumentSequence.StartsWith("(")) {
						// only opening parenthesis found
						if (RaiseErrors & !closingerror) {
							Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
					}
				} else {
					// no indices found before the space
					if (i < Expression.Text.Length - 1 && Expression.Text[i + 1] == '(') {
						// opening parenthesis follows the space
						int j = Expression.Text.IndexOf(')', i + 1);
						if (j > i + 1) {
							// closing parenthesis found
							if (j == Expression.Text.Length - 1) {
								// only closing parenthesis found at the end of the expression
								Command = Expression.Text.Substring(0, i).TrimEnd();
								ArgumentSequence = Expression.Text.Substring(i + 2, j - i - 2).Trim();
							} else {
								// detect border between indices and arguments
								bool found = false;
								Command = null; ArgumentSequence = null;
								for (int k = j + 1; k < Expression.Text.Length; k++) {
									if (char.IsWhiteSpace(Expression.Text[k])) {
										Command = Expression.Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Expression.Text.Substring(k + 1).TrimStart();
										found = true; break;
									} else if (Expression.Text[k] == '(') {
										Command = Expression.Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Expression.Text.Substring(k).TrimStart();
										found = true; break;
									}
								}
								if (!found) {
									if (RaiseErrors & !openingerror & !closingerror) {
										Interface.AddMessage(MessageType.Error, false, "Invalid syntax encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										openingerror = true;
										closingerror = true;
									}
									Command = Expression.Text;
									ArgumentSequence = "";
								}
								if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
									// arguments are enclosed by parentheses
									ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
								} else if (ArgumentSequence.StartsWith("(")) {
									// only opening parenthesis found
									if (RaiseErrors & !closingerror) {
										Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
								}
							}
						} else {
							// no closing parenthesis found
							if (RaiseErrors & !closingerror) {
								Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							Command = Expression.Text.Substring(0, i).TrimEnd();
							ArgumentSequence = Expression.Text.Substring(i + 2).TrimStart();
						}
					} else {
						// no index possible
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
						if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
							// arguments are enclosed by parentheses
							ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
						} else if (ArgumentSequence.StartsWith("(")) {
							// only opening parenthesis found
							if (RaiseErrors & !closingerror) {
								Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
						}
					}
				}
			} else {
				// no single space found
				if (Expression.Text.EndsWith(")")) {
					i = Expression.Text.LastIndexOf('(');
					if (i >= 0) {
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1, Expression.Text.Length - i - 2).Trim();
					} else {
						Command = Expression.Text;
						ArgumentSequence = "";
					}
				} else {
					i = Expression.Text.IndexOf('(');
					if (i >= 0) {
						if (RaiseErrors & !closingerror) {
							Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
					} else {
						if (RaiseErrors) {
							i = Expression.Text.IndexOf(')');
							if (i >= 0 & !closingerror) {
								Interface.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}
						Command = Expression.Text;
						ArgumentSequence = "";
					}
				}
			}
			// invalid trailing characters
			if (Command.EndsWith(";")) {
				if (RaiseErrors) {
					Interface.AddMessage(MessageType.Error, false, "Invalid trailing semicolon encountered in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
				}
				while (Command.EndsWith(";")) {
					Command = Command.Substring(0, Command.Length - 1);
				}
			}
		}

		// parse route for data
		private static void ParseRouteForData(string FileName, bool IsRW, System.Text.Encoding Encoding, Expression[] Expressions, string TrainPath, string ObjectPath, string SoundPath, double[] UnitOfLength, ref RouteData Data, bool PreviewOnly) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Section = ""; bool SectionAlwaysPrefix = false;
			int BlockIndex = 0;
			int BlocksUsed = Data.Blocks.Length;
			Game.Stations = new Game.Station[] { };
			int CurrentStation = -1;
			int CurrentStop = -1;
			bool DepartureSignalUsed = false;
			int CurrentSection = 0;
			bool ValueBasedSections = false;
			double progressFactor = Expressions.Length == 0 ? 0.3333 : 0.3333 / (double)Expressions.Length;
			// process non-track namespaces
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
					int Equals = Expressions[j].Text.IndexOf('=');
					if (Equals >= 0) {
						// handle RW cycle syntax
						string t = Expressions[j].Text.Substring(0, Equals);
						if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Ground(" + t + ")";
							}
						} else if (Section.ToLowerInvariant() == "signal" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Void(" + t + ")";
							}
						}
						// convert RW style into CSV style
						Expressions[j].Text = t + " " + Expressions[j].Text.Substring(Equals + 1);
					}
					// separate command and arguments
					string Command, ArgumentSequence;
					SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, Expressions[j].File, j, false);
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
											Command = null; break;
										} else if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid second index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null; break;
										}
									} else {
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null; break;
										}
									}
									break;
								}
							}
						}
						// process command
						if (!String.IsNullOrEmpty(Command)) {
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
								case "route.loadingscreen":
								case "route.displayspeed":
								case "route.starttime":
									if (!PreviewOnly)
									{
										Interface.AddMessage(MessageType.Information, false, "" + Command + " is only supported in OpenBVE versions 1.4.4.0 and above at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									} break;
								case "route.dynamiclight":
									if (!PreviewOnly)
									{
										Interface.AddMessage(MessageType.Information, false, "" + Command + " is not supported when using RouteViewer at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
											double[] intervals = new double[Arguments.Length];
											for (int k = 0; k < Arguments.Length; k++) {
												if (!NumberFormats.TryParseDoubleVb6(Arguments[k], out intervals[k])) {
													Interface.AddMessage(MessageType.Error, false, "Interval" + k.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
											}
											Array.Sort<double>(intervals);
											Game.PrecedingTrainTimeDeltas = intervals;
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
											} 
										}
									} break;
								case "train.destination":
									if (!PreviewOnly)
									{
										if (Arguments.Length < 1)
										{
											Interface.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
									}
									break;
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														Data.Structure.Poles[CommandIndex1][CommandIndex2] = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
														string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															UnifiedObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
														string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															UnifiedObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
														string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
															if (obj != null)
															{
																Data.Structure.RoofCL.Add(CommandIndex1, obj, "Left RoofCStructureIndex");
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
														string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f)) {
															Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
															if (obj != null)
															{
																Data.Structure.RoofCR.Add(CommandIndex1, obj, "Right RoofCStructureIndex");
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														ObjectManager.StaticObject obj = ObjectManager.LoadStaticObject(f, Encoding, false);
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
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " could not be found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														UnifiedObject obj = ObjectManager.LoadObject(f, Encoding, false);
														if (obj != null)
														{
															Data.Structure.FreeObjects.Add(CommandIndex1, obj, "FreeObject");
														}
													}
												}
											}
										}
									} break;
									// signal
								case "signal":
									{
										if (!PreviewOnly) {
											if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.SignalData.Length) {
													Array.Resize<SignalData>(ref Data.SignalData, CommandIndex1 + 1);
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
															UnifiedObject Object = ObjectManager.LoadObject(f, Encoding, false);
															if (Object is ObjectManager.AnimatedObjectCollection) {
																AnimatedObjectSignalData Signal = new AnimatedObjectSignalData();
																Signal.Objects = (ObjectManager.AnimatedObjectCollection)Object;
																Data.SignalData[CommandIndex1] = Signal;
															} else {
																Interface.AddMessage(MessageType.Error, true, "GlowFileWithoutExtension " + f + " is not a valid animated object in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															}
														}
													}
												} else {
													if (Path.ContainsInvalidChars(Arguments[0])) {
														Interface.AddMessage(MessageType.Error, false, "SignalFileWithoutExtension contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														if (Arguments.Length > 2) {
															Interface.AddMessage(MessageType.Warning, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														}
														string f = System.IO.Path.Combine(ObjectPath, Arguments[0]);
														if (!System.IO.File.Exists(f))
														{
															bool notFound = false;
															while (true)
															{
																f = Path.CombineFile(System.IO.Path.GetDirectoryName(f), System.IO.Path.GetFileName(f) + ".x");
																if (System.IO.File.Exists(f))
																{
																	break;
																}
																f = Path.CombineFile(System.IO.Path.GetDirectoryName(f), System.IO.Path.GetFileName(f) + ".csv");
																if (System.IO.File.Exists(f))
																{
																	break;
																}
																f = Path.CombineFile(System.IO.Path.GetDirectoryName(f), System.IO.Path.GetFileName(f) + ".b3d");
																if (System.IO.File.Exists(f))
																{
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
														Bve4SignalData Signal = new Bve4SignalData();
														Signal.BaseObject = ObjectManager.LoadStaticObject(f, Encoding, false);
														Signal.GlowObject = null;
														string Folder = System.IO.Path.GetDirectoryName(f);
														if (!System.IO.Directory.Exists(Folder)) {
															Interface.AddMessage(MessageType.Error, true, "The folder " + Folder + " could not be found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														} else {
															Signal.SignalTextures = LoadAllTextures(f, false);
															Signal.GlowTextures = new Texture[] { };
															if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
																if (Path.ContainsInvalidChars(Arguments[1])) {
																	Interface.AddMessage(MessageType.Error, false, "GlowFileWithoutExtension contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
																} else {
																	f = System.IO.Path.Combine(ObjectPath, Arguments[1]);
																	Signal.GlowObject = ObjectManager.LoadStaticObject(f, Encoding, false);
																	Signal.GlowTextures = LoadAllTextures(f, true);
																	if (Signal.GlowObject != null) {
																		for (int p = 0; p < Signal.GlowObject.Mesh.Materials.Length; p++) {
																			Signal.GlowObject.Mesh.Materials[p].BlendMode = MeshMaterialBlendMode.Additive;
																			Signal.GlowObject.Mesh.Materials[p].GlowAttenuationData = Glow.GetAttenuationData(200.0, GlowAttenuationMode.DivisionExponent4);
																		}
																	}
																}
															}
															Data.SignalData[CommandIndex1] = Signal;
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
														Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
														for (int k = a; k <= CommandIndex1; k++) {
															Data.Backgrounds[k] = new World.Background(null, 6, false);
														}
													}
													string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else
													{
														Textures.RegisterTexture(f, out Data.Backgrounds[CommandIndex1].Texture);
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
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false,  Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.Backgrounds.Length) {
													int a = Data.Backgrounds.Length;
													Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
													for (int k = a; k <= CommandIndex1; k++) {
														Data.Backgrounds[k] = new World.Background(null, 6, false);
													}
												}
												int x;
												if (!NumberFormats.TryParseIntVb6(Arguments[0], out x)) {
													Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (x == 0) {
													Interface.AddMessage(MessageType.Error, false, "RepetitionCount is expected to be non-zero in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													Data.Backgrounds[CommandIndex1].Repetition = x;
												}
											}
										}
									} break;
								case "texture.background.aspect":
								case "structure.back.aspect":
									{
										if (!PreviewOnly) {
											if (CommandIndex1 < 0) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false,  Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (CommandIndex1 >= Data.Backgrounds.Length) {
													int a = Data.Backgrounds.Length;
													Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
													for (int k = a; k <= CommandIndex1; k++) {
														Data.Backgrounds[k] = new World.Background(null, 6, false);
													}
												}
												int aspect;
												if (!NumberFormats.TryParseIntVb6(Arguments[0], out aspect)) {
													Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (aspect != 0 & aspect != 1) {
													Interface.AddMessage(MessageType.Error, false, "Value is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													Data.Backgrounds[CommandIndex1].KeepAspectRatio = aspect == 1;
												}
											}
										}
									} break;
									// cycle
								case "cycle.ground":
									if (!PreviewOnly) {
										if (CommandIndex1 >= Data.Structure.Cycle.Length) {
											Array.Resize<int[]>(ref Data.Structure.Cycle, CommandIndex1 + 1);
										}
										Data.Structure.Cycle[CommandIndex1] = new int[Arguments.Length];
										for (int k = 0; k < Arguments.Length; k++) {
											int ix = 0;
											if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix)) {
												Interface.AddMessage(MessageType.Error, false, "GroundStructureIndex" + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											if (ix < 0 | !Data.Structure.Ground.ContainsKey(ix)) {
												Interface.AddMessage(MessageType.Error, false, "GroundStructureIndex" + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											Data.Structure.Cycle[CommandIndex1][k] = ix;
										}
									} break;
									// rail cycle
								case "cycle.rail":
									if (!PreviewOnly)
									{
										if (CommandIndex1 >= Data.Structure.RailCycle.Length)
										{
											Array.Resize<int[]>(ref Data.Structure.RailCycle, CommandIndex1 + 1);
                                        }
										Data.Structure.RailCycle[CommandIndex1] = new int[Arguments.Length];
                                        for (int k = 0; k < Arguments.Length; k++)
										{
											int ix = 0;
											if (Arguments[k].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[k], out ix))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex" + (k + 1).ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											if (ix < 0 | !Data.Structure.RailObjects.ContainsKey(ix))
											{
												Interface.AddMessage(MessageType.Error, false, "RailStructureIndex" + (k + 1).ToString(Culture) + " is out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												ix = 0;
											}
											Data.Structure.RailCycle[CommandIndex1][k] = ix;
										}
									}
									break;
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
					int Equals = Expressions[j].Text.IndexOf('=');
					if (Equals >= 0) {
						// handle RW cycle syntax
						string t = Expressions[j].Text.Substring(0, Equals);
						if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Ground(" + t + ")";
							}
						} else if (Section.ToLowerInvariant() == "signal" & SectionAlwaysPrefix) {
							double b; if (NumberFormats.TryParseDoubleVb6(t, out b)) {
								t = ".Void(" + t + ")";
							}
						}
						// convert RW style into CSV style
						Expressions[j].Text = t + " " + Expressions[j].Text.Substring(Equals + 1);
					}
					// separate command and arguments
					string Command, ArgumentSequence;
					SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, Expressions[j].File, j, false);
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
							Data.TrackPosition = Number;
							BlockIndex = (int)Math.Floor(Number / Data.BlockInterval + 0.001);
							if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
							CreateMissingBlocks(ref Data, ref BlocksUsed, BlockIndex, PreviewOnly);
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
											Command = null; break;
										} else if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid second index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null; break;
										}
									} else {
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File + ".");
											Command = null; break;
										}
									}
									break;
								}
							}
						}
						// process command
						if (!String.IsNullOrEmpty(Command)) {
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
								case "route.ambientlight":
								case "route.directionallight":
								case "route.lightdirection":
								case "route.developerid":
								case "train.folder":
								case "train.file":
								case "train.run":
								case "train.rail":
								case "train.flange":
								case "train.timetable.day":
								case "train.timetable.night":
								case "train.velocity":
								case "train.acceleration":
								case "train.station":
								case "train.destination":
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
											if (idx < Data.Blocks[BlockIndex].Rail.Length && Data.Blocks[BlockIndex].Rail[idx].RailStart)
											{
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is required to reference a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											}
										}
										if (Data.Blocks[BlockIndex].Rail.Length <= idx)
										{
											Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rail, idx + 1);
											int ol = Data.Blocks[BlockIndex].RailCycle.Length;
											Array.Resize<RailCycle>(ref Data.Blocks[BlockIndex].RailCycle, idx + 1);
											for (int rc = ol; rc < Data.Blocks[BlockIndex].RailCycle.Length; rc++)
											{
												Data.Blocks[BlockIndex].RailCycle[rc].RailCycleIndex = -1;
											}
										}
										if (Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed)
										{
											Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
										}
										Data.Blocks[BlockIndex].Rail[idx].RailStart = true;
										Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = true;
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
												Data.Blocks[BlockIndex].Rail[idx].RailStartX = x;
											}
											if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
											{
												Data.Blocks[BlockIndex].Rail[idx].RailEndX = Data.Blocks[BlockIndex].Rail[idx].RailStartX;
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
												Data.Blocks[BlockIndex].Rail[idx].RailStartY = y;
											}
											if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
											{
												Data.Blocks[BlockIndex].Rail[idx].RailEndY = Data.Blocks[BlockIndex].Rail[idx].RailStartY;
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
												if (sttype < Data.Structure.RailCycle.Length && Data.Structure.RailCycle[sttype] != null)
												{
													Data.Blocks[BlockIndex].RailType[idx] = Data.Structure.RailCycle[sttype][0];
													Data.Blocks[BlockIndex].RailCycle[idx].RailCycleIndex = sttype;
													Data.Blocks[BlockIndex].RailCycle[idx].CurrentCycle = 0;
												}
												else
												{
													Data.Blocks[BlockIndex].RailType[idx] = sttype;
													Data.Blocks[BlockIndex].RailCycle[idx].RailCycleIndex = -1;
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
											if (idx < 0 || idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart)
											{
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												break;
											}
											if (Data.Blocks[BlockIndex].RailType.Length <= idx)
											{
												Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rail, idx + 1);
											}
											Data.Blocks[BlockIndex].Rail[idx].RailStart = false;
											Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = false;
											Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
											if (Arguments.Length >= 2 && Arguments[1].Length > 0)
											{
												double x;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
												{
													Interface.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													x = 0.0;
												}
												Data.Blocks[BlockIndex].Rail[idx].RailEndX = x;
											}
											if (Arguments.Length >= 3 && Arguments[2].Length > 0)
											{
												double y;
												if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
												{
													Interface.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													y = 0.0;
												}
												Data.Blocks[BlockIndex].Rail[idx].RailEndY = y;
											}
										}
									}
									break;
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
												if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (sttype < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (!Data.Structure.RailObjects.ContainsKey(sttype)) {
													Interface.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (Data.Blocks[BlockIndex].RailType.Length <= idx) {
														Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
														Array.Resize(ref Data.Blocks[BlockIndex].RailCycle, idx + 1);
                                                    }
                                                    if (sttype < Data.Structure.RailCycle.Length && Data.Structure.RailCycle[sttype] != null)
                                                    {
                                                        Data.Blocks[BlockIndex].RailType[idx] = Data.Structure.RailCycle[sttype][0];
                                                        Data.Blocks[BlockIndex].RailCycle[idx].RailCycleIndex = sttype;
                                                        Data.Blocks[BlockIndex].RailCycle[idx].CurrentCycle = 0;
                                                    }
                                                    else {
                                                        Data.Blocks[BlockIndex].RailType[idx] = sttype;
                                                        Data.Blocks[BlockIndex].RailCycle[idx].RailCycleIndex = -1;
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
											int n = Data.Blocks[BlockIndex].Brightness.Length;
											Array.Resize<Brightness>(ref Data.Blocks[BlockIndex].Brightness, n + 1);
											Data.Blocks[BlockIndex].Brightness[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Brightness[n].Value = value;
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
												for (int i = 0; i < Arguments.Length; i++) {
													if (!NumberFormats.TryParseIntVb6(Arguments[i], out aspects[i])) {
														Interface.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														aspects[i] = -1;
													} else if (aspects[i] < 0) {
														Interface.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														aspects[i] = -1;
													}
												}
												bool valueBased = ValueBasedSections | string.Equals(Command, "Track.SectionS", StringComparison.OrdinalIgnoreCase);
												if (valueBased) {
													Array.Sort<int>(aspects);
												}
												int n = Data.Blocks[BlockIndex].Section.Length;
												Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
												Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
												Data.Blocks[BlockIndex].Section[n].Type = valueBased ? Game.SectionType.ValueBased : Game.SectionType.IndexBased;
												Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
												if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
													if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
														Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
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
											if (objidx >= 0 & objidx < Data.SignalData.Length && Data.SignalData[objidx] != null) {
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
												int n = Data.Blocks[BlockIndex].Signal.Length;
												Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
												Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection + section;
												Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = -1;
												Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = objidx;
												Data.Blocks[BlockIndex].Signal[n].X = x;
												Data.Blocks[BlockIndex].Signal[n].Y = y < 0.0 ? 4.8 : y;
												Data.Blocks[BlockIndex].Signal[n].Yaw = 0.0174532925199433 * yaw;
												Data.Blocks[BlockIndex].Signal[n].Pitch = 0.0174532925199433 * pitch;
												Data.Blocks[BlockIndex].Signal[n].Roll = 0.0174532925199433 * roll;
												Data.Blocks[BlockIndex].Signal[n].ShowObject = true;
												Data.Blocks[BlockIndex].Signal[n].ShowPost = y < 0.0;
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
											if (num != 1 & num != -2 & num != 2 & num != 3 & num != -3 & num != -4 & num != 4 & num != -5 & num != 5 & num != 6)
											{
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
											int n = Data.Blocks[BlockIndex].Section.Length;
											Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
											Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
											Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
											Data.Blocks[BlockIndex].Section[n].Invisible = x == 0.0;
											Data.Blocks[BlockIndex].Section[n].Type = Game.SectionType.ValueBased;
											if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
												if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
													Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
													DepartureSignalUsed = true;
												}
											}
											CurrentSection++;
											n = Data.Blocks[BlockIndex].Signal.Length;
											Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
											Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection;
											Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = comp;
											Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = -1;
											Data.Blocks[BlockIndex].Signal[n].X = x;
											Data.Blocks[BlockIndex].Signal[n].Y = y < 0.0 ? 4.8 : y;
											Data.Blocks[BlockIndex].Signal[n].Yaw = 0.0174532925199433 * yaw;
											Data.Blocks[BlockIndex].Signal[n].Pitch = 0.0174532925199433 * pitch;
											Data.Blocks[BlockIndex].Signal[n].Roll = 0.0174532925199433 * roll;
											Data.Blocks[BlockIndex].Signal[n].ShowObject = x != 0.0;
											Data.Blocks[BlockIndex].Signal[n].ShowPost = x != 0.0 & y < 0.0;
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
											int n = Data.Blocks[BlockIndex].Signal.Length;
											Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
											Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection + 1;
											Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = 8;
											Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = -1;
											Data.Blocks[BlockIndex].Signal[n].X = x;
											Data.Blocks[BlockIndex].Signal[n].Y = y < 0.0 ? 4.8 : y;
											Data.Blocks[BlockIndex].Signal[n].Yaw = yaw * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signal[n].Pitch = pitch * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signal[n].Roll = roll * 0.0174532925199433;
											Data.Blocks[BlockIndex].Signal[n].ShowObject = x != 0.0;
											Data.Blocks[BlockIndex].Signal[n].ShowPost = x != 0.0 & y < 0.0;
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
												Interface.AddMessage(MessageType.Error, false, "Type is expected to be non-positive in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
													Interface.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure +" references an object not loaded in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													structure = -1;
												}
												if (section == -1) {
													section = (int)TrackManager.TransponderSpecialSection.NextRedSection;
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
												int n = Data.Blocks[BlockIndex].Transponder.Length;
												Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
												Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
												Data.Blocks[BlockIndex].Transponder[n].Type = (TrackManager.TransponderType)type;
												Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = optional;
												Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = structure;
												Data.Blocks[BlockIndex].Transponder[n].Section = section;
												Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = optional != -1;
												Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = false;
												Data.Blocks[BlockIndex].Transponder[n].X = x;
												Data.Blocks[BlockIndex].Transponder[n].Y = y;
												Data.Blocks[BlockIndex].Transponder[n].Yaw = yaw * 0.0174532925199433;
												Data.Blocks[BlockIndex].Transponder[n].Pitch = pitch * 0.0174532925199433;
												Data.Blocks[BlockIndex].Transponder[n].Roll = roll * 0.0174532925199433;
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
											int n = Data.Blocks[BlockIndex].Transponder.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
											Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponder[n].Type = (TrackManager.TransponderType)type;
											Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = work == 0;
											Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponder[n].X = x;
											Data.Blocks[BlockIndex].Transponder[n].Y = y;
											Data.Blocks[BlockIndex].Transponder[n].Yaw = yaw * 0.0174532925199433;
											Data.Blocks[BlockIndex].Transponder[n].Pitch = pitch * 0.0174532925199433;
											Data.Blocks[BlockIndex].Transponder[n].Roll = roll * 0.0174532925199433;
											if (type == 2) {
												Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = CurrentStop >= 0 ? CurrentStop : 0;
											} else {
												Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = work;
											}
											Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + oversig + 1;
										}
									} break;
								case "track.atssn":
									{
										if (!PreviewOnly) {
											int n = Data.Blocks[BlockIndex].Transponder.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
											Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.SLong;
											Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = true;
											Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = -1;
											Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + 1;
										}
									} break;
								case "track.atsp":
									{
										if (!PreviewOnly) {
											int n = Data.Blocks[BlockIndex].Transponder.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
											Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.AtsPPatternOrigin;
											Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
											Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = true;
											Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = -1;
											Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + 1;
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
											int n = Data.Blocks[BlockIndex].Transponder.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
											Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponder[n].Type = type == 1 ? TrackManager.TransponderType.AtsPPermanentSpeedRestriction : TrackManager.TransponderType.AtsPTemporarySpeedRestriction;
											Data.Blocks[BlockIndex].Transponder[n].Section = -1;
											Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponder[n].OptionalFloat = speed * Data.UnitOfSpeed;
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
											int n = Data.Blocks[BlockIndex].Transponder.Length;
											Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
											Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
											Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.AtsPPermanentSpeedRestriction;
											Data.Blocks[BlockIndex].Transponder[n].Section = -1;
											Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
											Data.Blocks[BlockIndex].Transponder[n].OptionalFloat = speed * Data.UnitOfSpeed;
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
										int n = Data.Blocks[BlockIndex].Limit.Length;
										Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limit, n + 1);
										Data.Blocks[BlockIndex].Limit[n].TrackPosition = Data.TrackPosition;
										Data.Blocks[BlockIndex].Limit[n].Speed = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
										Data.Blocks[BlockIndex].Limit[n].Direction = direction;
										Data.Blocks[BlockIndex].Limit[n].Cource = cource;
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
										int n = Data.Blocks[BlockIndex].Stop.Length;
										Array.Resize<Stop>(ref Data.Blocks[BlockIndex].Stop, n + 1);
										Data.Blocks[BlockIndex].Stop[n].TrackPosition = Data.TrackPosition;
										Data.Blocks[BlockIndex].Stop[n].Station = CurrentStation;
										Data.Blocks[BlockIndex].Stop[n].Direction = dir;
										Data.Blocks[BlockIndex].Stop[n].ForwardTolerance = forw;
										Data.Blocks[BlockIndex].Stop[n].BackwardTolerance = backw;
										Data.Blocks[BlockIndex].Stop[n].Cars = cars;
										CurrentStop = cars;
									} break;
								case "track.sta":
									{
										CurrentStation++;
										Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
										Game.Stations[CurrentStation].Name = string.Empty;
										Game.Stations[CurrentStation].StopMode = Game.StationStopMode.AllStop;
										Game.Stations[CurrentStation].StationType = Game.StationType.Normal;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
											Game.Stations[CurrentStation].Name = Arguments[0];
										}
										double arr = -1.0, dep = -1.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
											if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.AllPass;
											} else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerPass;
											} else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerPass;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerStop;
											} else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerStop;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (!Interface.TryParseTime(Arguments[1], out arr)) {
												Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												arr = -1.0;
											}
										}
										if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
											if (string.Equals(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[2], "=", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.Terminal;
											} else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.Terminal;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.ChangeEnds;
											} else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.ChangeEnds;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (!Interface.TryParseTime(Arguments[2], out dep)) {
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
														arrsnd = Sounds.RegisterBuffer(f, 30.0);
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
														depsnd = Sounds.RegisterBuffer(f, 30.0);
													}
												}
											}
										}
										int ttidx = -1, tdt = -1, tnt = -1;
										if (!PreviewOnly) {
											if (Arguments.Length >= 12 && Arguments[11].Length > 0) {
												if (!NumberFormats.TryParseIntVb6(Arguments[11], out ttidx)) {
													ttidx = -1;
												} else {
													if (ttidx < 0) {
														// if (Program.CurrentProgramType == Program.ProgramType.OpenBve) {
														// 	Interface.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														// }
														ttidx = -1;
													} else if (ttidx >= Data.TimetableDaytime.Length & ttidx >= Data.TimetableNighttime.Length) {
														// if (Program.CurrentProgramType == Program.ProgramType.OpenBve) {
														// 	Interface.AddMessage(MessageType.Error, false, "TimetableIndex references textures not loaded in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
														// }
														ttidx = -1;
													}
													tdt = ttidx >= 0 & ttidx < Data.TimetableDaytime.Length ? Data.TimetableDaytime[ttidx] : -1;
													tnt = ttidx >= 0 & ttidx < Data.TimetableNighttime.Length ? Data.TimetableNighttime[ttidx] : -1;
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
													tdt = -1;
													tnt = -1;
												}
											}
										}
										if (Game.Stations[CurrentStation].Name.Length == 0 & (Game.Stations[CurrentStation].StopMode == Game.StationStopMode.PlayerStop | Game.Stations[CurrentStation].StopMode == Game.StationStopMode.AllStop)) {
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
										Data.Blocks[BlockIndex].Station = CurrentStation;
										Data.Blocks[BlockIndex].StationPassAlarm = passalarm == 1;
										CurrentStop = -1;
										DepartureSignalUsed = false;
									} break;
								case "track.station":
									{
										CurrentStation++;
										Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
										Game.Stations[CurrentStation].Name = string.Empty;
										Game.Stations[CurrentStation].StopMode = Game.StationStopMode.AllStop;
										Game.Stations[CurrentStation].StationType = Game.StationType.Normal;
										if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
											Game.Stations[CurrentStation].Name = Arguments[0];
										}
										double arr = -1.0, dep = -1.0;
										if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
											if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.AllPass;
											} else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerPass;
											} else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerPass;
												if (!Interface.TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr)) {
													Interface.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													arr = -1.0;
												}
											} else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerStop;
											} else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StopMode = Game.StationStopMode.PlayerStop;
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
												Game.Stations[CurrentStation].StationType = Game.StationType.Terminal;
											} else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.Terminal;
												if (!Interface.TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep)) {
													Interface.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													dep = -1.0;
												}
											} else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.ChangeEnds;
											} else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase)) {
												Game.Stations[CurrentStation].StationType = Game.StationType.ChangeEnds;
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
											if (string.Compare(Arguments[4], "ats", StringComparison.OrdinalIgnoreCase) == 0) {
												device = 0;
											} else if (string.Compare(Arguments[4], "atc", StringComparison.OrdinalIgnoreCase) == 0) {
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
														depsnd = Sounds.RegisterBuffer(f, 30.0);
													}
												}
											}
										}
										if (Game.Stations[CurrentStation].Name.Length == 0 & (Game.Stations[CurrentStation].StopMode == Game.StationStopMode.PlayerStop | Game.Stations[CurrentStation].StopMode == Game.StationStopMode.AllStop)) {
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
										Game.Stations[CurrentStation].TimetableDaytimeTexture = -1;
										Game.Stations[CurrentStation].TimetableNighttimeTexture = -1;
										Game.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
										Data.Blocks[BlockIndex].Station = CurrentStation;
										Data.Blocks[BlockIndex].StationPassAlarm = false;
										CurrentStop = -1;
										DepartureSignalUsed = false;
									} break;
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
												if (idx1 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx1].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												if (idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR && (idx2 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx2].RailStart)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex2" + idx2 + " could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												if (roof != 0 & (roof < 0 || !Data.Structure.RoofL.ContainsKey(roof)|| !Data.Structure.RoofR.ContainsKey(roof))) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex " + roof + " references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (pf < 0 | !Data.Structure.FormL.ContainsKey(pf) & !Data.Structure.FormR.ContainsKey(pf)) {
														Interface.AddMessage(MessageType.Error, false, "FormStructureIndex " + pf + " references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													int n = Data.Blocks[BlockIndex].Form.Length;
													Array.Resize<Form>(ref Data.Blocks[BlockIndex].Form, n + 1);
													Data.Blocks[BlockIndex].Form[n].PrimaryRail = idx1;
													Data.Blocks[BlockIndex].Form[n].SecondaryRail = idx2;
													Data.Blocks[BlockIndex].Form[n].FormType = pf;
													Data.Blocks[BlockIndex].Form[n].RoofType = roof;
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
												if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												if (idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
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
											if (dir <= 0 && !Data.Structure.WallL.ContainsKey(sttype) ||dir >= 0 && !Data.Structure.WallR.ContainsKey(sttype)) {
												Interface.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
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
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx +" does not reference an existing wall in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex" + idx + " could be out of range in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex is expected to be a non-negative integer in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												sttype = 0;
											}
											if (dir <= 0 && !Data.Structure.DikeL.ContainsKey(sttype) || dir >= 0 && !Data.Structure.DikeR.ContainsKey(sttype))
											{
												Interface.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
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
												Interface.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing dike in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												if (idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												}
												Data.Blocks[BlockIndex].RailDike[idx].Exists = false;
											}
										}
									} break;
								case "track.marker":
									{
										if (!PreviewOnly) {
											if (Arguments.Length < 1) {
												Interface.AddMessage(MessageType.Error, false, "Track.Marker is expected to have at least one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else if (Path.ContainsInvalidChars(Arguments[0])) {
												Interface.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
												if (!System.IO.File.Exists(f)) {
													Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in Track.Marker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
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
													Textures.RegisterTexture(f, out Data.Markers[n].Texture);
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
											if (cytype < Data.Structure.Cycle.Length && Data.Structure.Cycle[cytype] != null) {
												Data.Blocks[BlockIndex].Cycle = Data.Structure.Cycle[cytype];
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
													Interface.AddMessage(MessageType.Error, false, "RailIndex1 " + idx1 + " is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (idx2 < 0) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex2 " + idx2 + " is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else if (idx1 == idx2) {
													Interface.AddMessage(MessageType.Error, false, "RailIndex1 " + idx1 + " is expected to be unequal to Index2 in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												} else {
													if (idx1 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx1].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													if (idx2 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx2].RailStart) {
														Interface.AddMessage(MessageType.Warning, false, "RailIndex2 " + idx2 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													}
													int n = Data.Blocks[BlockIndex].Crack.Length;
													Array.Resize<Crack>(ref Data.Blocks[BlockIndex].Crack, n + 1);
													Data.Blocks[BlockIndex].Crack[n].PrimaryRail = idx1;
													Data.Blocks[BlockIndex].Crack[n].SecondaryRail = idx2;
													Data.Blocks[BlockIndex].Crack[n].Type = sttype;
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
												if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart)) {
													Interface.AddMessage(MessageType.Warning, false, "RailIndex " + idx + "could be out of range in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
														int n;
														n = Data.Blocks[BlockIndex].GroundFreeObj.Length;
														Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].GroundFreeObj, n + 1);
														Data.Blocks[BlockIndex].GroundFreeObj[n].TrackPosition = Data.TrackPosition;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Type = sttype;
														Data.Blocks[BlockIndex].GroundFreeObj[n].X = x;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Y = y;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Yaw = yaw * 0.0174532925199433;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = pitch * 0.0174532925199433;
														Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = roll * 0.0174532925199433;
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
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
														Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
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
											} else if (Data.Backgrounds[typ].Texture == null) {
												Interface.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + typ + " has not been loaded via Texture.Background in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
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
													string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
														Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
													} else {
														double speed = 0.0;
														if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed)) {
															Interface.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
															speed = 0.0;
														}
														int n = Data.Blocks[BlockIndex].Sound.Length;
														Array.Resize<Sound>(ref Data.Blocks[BlockIndex].Sound, n + 1);
														Data.Blocks[BlockIndex].Sound[n].TrackPosition = Data.TrackPosition;
														Data.Blocks[BlockIndex].Sound[n].SoundBuffer = Sounds.RegisterBuffer(f, 15.0);
														Data.Blocks[BlockIndex].Sound[n].Type = speed == 0.0 ? SoundType.TrainStatic : SoundType.TrainDynamic;
														Data.Blocks[BlockIndex].Sound[n].Speed = speed * Data.UnitOfSpeed;
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
													string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[0]);
													if (!System.IO.File.Exists(f)) {
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
														const double radius = 15.0;
														int n = Data.Blocks[BlockIndex].Sound.Length;
														Array.Resize<Sound>(ref Data.Blocks[BlockIndex].Sound, n + 1);
														Data.Blocks[BlockIndex].Sound[n].TrackPosition = Data.TrackPosition;
														Data.Blocks[BlockIndex].Sound[n].SoundBuffer = Sounds.RegisterBuffer(f, radius);
														Data.Blocks[BlockIndex].Sound[n].Type = SoundType.World;
														Data.Blocks[BlockIndex].Sound[n].X = x;
														Data.Blocks[BlockIndex].Sound[n].Y = y;
														Data.Blocks[BlockIndex].Sound[n].Radius = radius;
													}
												}
											}
										}
									} break;
								case "track.pretrain":
									{
										if (!PreviewOnly) {
											if (Arguments.Length == 0) {
												Interface.AddMessage(MessageType.Error, false, Command + " is expected to have exactly 1 argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											} else {
												double time = 0.0;
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
											if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart)) {
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
			if (!PreviewOnly) {
				// timetable
				Timetable.CustomTextureIndices = new int[Data.TimetableDaytime.Length + Data.TimetableNighttime.Length];
				int n = 0;
				for (int i = 0; i < Data.TimetableDaytime.Length; i++) {
					if (Data.TimetableDaytime[i] >= 0) {
						Timetable.CustomTextureIndices[n] = Data.TimetableDaytime[i];
						n++;
					}
				}
				for (int i = 0; i < Data.TimetableNighttime.Length; i++) {
					if (Data.TimetableNighttime[i] >= 0) {
						Timetable.CustomTextureIndices[n] = Data.TimetableNighttime[i];
						n++;
					}
				}
				Array.Resize<int>(ref Timetable.CustomTextureIndices, n);
			}
			// blocks
			Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
		}

		// ================================

		// create missing blocks
		private static void CreateMissingBlocks(ref RouteData Data, ref int BlocksUsed, int ToIndex, bool PreviewOnly) {
			if (ToIndex >= BlocksUsed) {
				while (Data.Blocks.Length <= ToIndex) {
					Array.Resize<Block>(ref Data.Blocks, Data.Blocks.Length << 1);
				}
				for (int i = BlocksUsed; i <= ToIndex; i++) {
					Data.Blocks[i] = new Block();
					if (!PreviewOnly) {
						Data.Blocks[i].Background = -1;
						Data.Blocks[i].Brightness = new Brightness[] { };
						Data.Blocks[i].Fog = Data.Blocks[i - 1].Fog;
						Data.Blocks[i].FogDefined = false;
						Data.Blocks[i].Cycle = Data.Blocks[i - 1].Cycle;
                        Data.Blocks[i].RailCycle = Data.Blocks[i - 1].RailCycle;
                        Data.Blocks[i].Height = double.NaN;
					}
					Data.Blocks[i].RailType = new int[Data.Blocks[i - 1].RailType.Length];
					if (!PreviewOnly) {
						for (int j = 0; j < Data.Blocks[i].RailType.Length; j++) {
							int rc = Data.Blocks[i].RailCycle[j].RailCycleIndex;
							if (rc != -1 && Data.Structure.RailCycle[rc].Length > 1) {
								int cc = Data.Blocks[i].RailCycle[j].CurrentCycle;
								if (cc == Data.Structure.RailCycle[rc].Length - 1) {
									Data.Blocks[i].RailType[j] = Data.Structure.RailCycle[rc][0];
									Data.Blocks[i].RailCycle[j].CurrentCycle = 0;
								}
								else {
									cc++;
									Data.Blocks[i].RailType[j] = Data.Structure.RailCycle[rc][cc];
									Data.Blocks[i].RailCycle[j].CurrentCycle++;
								}
							}
							else {
								Data.Blocks[i].RailType[j] = Data.Blocks[i - 1].RailType[j];
							}
						}
					}
					Data.Blocks[i].Rail = new Rail[Data.Blocks[i - 1].Rail.Length];
					for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
						Data.Blocks[i].Rail[j].RailStart = Data.Blocks[i - 1].Rail[j].RailStart;
						Data.Blocks[i].Rail[j].RailStartX = Data.Blocks[i - 1].Rail[j].RailStartX;
						Data.Blocks[i].Rail[j].RailStartY = Data.Blocks[i - 1].Rail[j].RailStartY;
						Data.Blocks[i].Rail[j].RailStartRefreshed = false;
						Data.Blocks[i].Rail[j].RailEnd = false;
						Data.Blocks[i].Rail[j].RailEndX = Data.Blocks[i - 1].Rail[j].RailStartX;
						Data.Blocks[i].Rail[j].RailEndY = Data.Blocks[i - 1].Rail[j].RailStartY;
					}
					if (!PreviewOnly) {
						Data.Blocks[i].RailWall = new WallDike[Data.Blocks[i - 1].RailWall.Length];
						for (int j = 0; j < Data.Blocks[i].RailWall.Length; j++) {
							Data.Blocks[i].RailWall[j] = Data.Blocks[i - 1].RailWall[j];
						}
						Data.Blocks[i].RailDike = new WallDike[Data.Blocks[i - 1].RailDike.Length];
						for (int j = 0; j < Data.Blocks[i].RailDike.Length; j++) {
							Data.Blocks[i].RailDike[j] = Data.Blocks[i - 1].RailDike[j];
						}
						Data.Blocks[i].RailPole = new Pole[Data.Blocks[i - 1].RailPole.Length];
						for (int j = 0; j < Data.Blocks[i].RailPole.Length; j++) {
							Data.Blocks[i].RailPole[j] = Data.Blocks[i - 1].RailPole[j];
						}
						Data.Blocks[i].Form = new Form[] { };
						Data.Blocks[i].Crack = new Crack[] { };
						Data.Blocks[i].Signal = new Signal[] { };
						Data.Blocks[i].Section = new Section[] { };
						Data.Blocks[i].Sound = new Sound[] { };
						Data.Blocks[i].Transponder = new Transponder[] { };
						Data.Blocks[i].DestinationChanges = new DestinationEvent[] { };
						Data.Blocks[i].RailFreeObj = new FreeObj[][] { };
						Data.Blocks[i].GroundFreeObj = new FreeObj[] { };
						Data.Blocks[i].PointsOfInterest = new PointOfInterest[] { };
					}
					Data.Blocks[i].Pitch = Data.Blocks[i - 1].Pitch;
					Data.Blocks[i].Limit = new Limit[] { };
					Data.Blocks[i].Stop = new Stop[] { };
					Data.Blocks[i].Station = -1;
					Data.Blocks[i].StationPassAlarm = false;
					Data.Blocks[i].CurrentTrackState = Data.Blocks[i - 1].CurrentTrackState;
					Data.Blocks[i].Turn = 0.0;
					Data.Blocks[i].Accuracy = Data.Blocks[i - 1].Accuracy;
					Data.Blocks[i].AdhesionMultiplier = Data.Blocks[i - 1].AdhesionMultiplier;
				}
				BlocksUsed = ToIndex + 1;
			}
		}

		// get mirrored object
		private static UnifiedObject GetMirroredObject(UnifiedObject Prototype) {
			if (Prototype is ObjectManager.StaticObject) {
				ObjectManager.StaticObject s = (ObjectManager.StaticObject)Prototype;
				return GetMirroredStaticObject(s);
			} else if (Prototype is ObjectManager.AnimatedObjectCollection) {
				ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)Prototype;
				ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
				Result.Objects = new ObjectManager.AnimatedObject[a.Objects.Length];
				for (int i = 0; i < a.Objects.Length; i++) {
					Result.Objects[i] = a.Objects[i].Clone();
					for (int j = 0; j < a.Objects[i].States.Length; j++) {
						Result.Objects[i].States[j].Object = GetMirroredStaticObject(a.Objects[i].States[j].Object);
					}
					Result.Objects[i].TranslateXDirection.X *= -1.0;
					Result.Objects[i].TranslateYDirection.X *= -1.0;
					Result.Objects[i].TranslateZDirection.X *= -1.0;
					Result.Objects[i].RotateXDirection.X *= -1.0;
					Result.Objects[i].RotateYDirection.X *= -1.0;
					Result.Objects[i].RotateZDirection.X *= -1.0;
				}
				return Result;
			} else {
				return null;
			}
		}
		private static ObjectManager.StaticObject GetMirroredStaticObject(ObjectManager.StaticObject Prototype)
		{
			ObjectManager.StaticObject Result = Prototype.Clone();
			for (int i = 0; i < Result.Mesh.Vertices.Length; i++) {
				Result.Mesh.Vertices[i].Coordinates.X = -Result.Mesh.Vertices[i].Coordinates.X;
			}
			for (int i = 0; i < Result.Mesh.Faces.Length; i++) {
				for (int k = 0; k < Result.Mesh.Faces[i].Vertices.Length; k++) {
					Result.Mesh.Faces[i].Vertices[k].Normal.X = -Result.Mesh.Faces[i].Vertices[k].Normal.X;
				}
				Result.Mesh.Faces[i].Flip();
			}
			return Result;
		}

		// get transformed object
		private static ObjectManager.StaticObject GetTransformedStaticObject(ObjectManager.StaticObject Prototype, double NearDistance, double FarDistance)
		{
			ObjectManager.StaticObject Result = Prototype.Clone();
			int n = 0;
			double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
			for (int i = 0; i < Result.Mesh.Vertices.Length; i++) {
				if (n == 2) {
					x2 = Result.Mesh.Vertices[i].Coordinates.X;
				} else if (n == 3) {
					x3 = Result.Mesh.Vertices[i].Coordinates.X;
				} else if (n == 6) {
					x6 = Result.Mesh.Vertices[i].Coordinates.X;
				} else if (n == 7) {
					x7 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				n++;
				if (n == 8) {
					break;
				}
			}
			if (n >= 4) {
				int m = 0;
				for (int i = 0; i < Result.Mesh.Vertices.Length; i++) {
					if (m == 0) {
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x3;
					} else if (m == 1) {
						Result.Mesh.Vertices[i].Coordinates.X = FarDistance - x2;
						if (n < 8) {
							m = 8;
							break;
						}
					} else if (m == 4) {
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x7;
					} else if (m == 5) {
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x6;
						m = 8;
						break;
					}
					m++;
					if (m == 8) {
						break;
					}
				}
			}
			return Result;
		}

		// load all textures
		private static Texture[] LoadAllTextures(string BaseFile, bool IsGlowTexture)
		{
			string Folder = System.IO.Path.GetDirectoryName(BaseFile);
			if (Folder != null && !System.IO.Directory.Exists(Folder))
			{
				return new Texture[] { };
			}
			string Name = System.IO.Path.GetFileNameWithoutExtension(BaseFile);
			Texture[] Textures = new Texture[] { };
			if (Folder == null) return Textures;
			string[] Files = System.IO.Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++)
			{
				string a = System.IO.Path.GetFileNameWithoutExtension(Files[i]);
				if (a == null || Name == null) return Textures;
				if (a.StartsWith(Name, StringComparison.OrdinalIgnoreCase))
				{
					if (a.Length > Name.Length)
					{
						string b = a.Substring(Name.Length).TrimStart();
						int j; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out j))
						{
							if (j >= 0)
							{
								string c = System.IO.Path.GetExtension(Files[i]);
								if (c == null) return Textures;
								switch (c.ToLowerInvariant())
								{
									case ".bmp":
									case ".gif":
									case ".jpg":
									case ".jpeg":
									case ".png":
									case ".tif":
									case ".tiff":
										if (j >= Textures.Length)
										{
											int n = Textures.Length;
											Array.Resize<Texture>(ref Textures, j + 1);
											for (int k = n; k < j; k++)
											{
												Textures[k] = null;
											}
										}
										if (IsGlowTexture)
										{
											Texture texture;
											if (Program.CurrentHost.LoadTexture(Files[i], null, out texture))
											{
												if (texture.BitsPerPixel == 32)
												{
													byte[] bytes = texture.Bytes;
													InvertLightness(bytes);
													texture = new Texture(texture.Width, texture.Height, 32, bytes, texture.Palette);
												}
												Textures[j] = OpenBve.Textures.RegisterTexture(texture);
											}
										}
										else
										{
											OpenBve.Textures.RegisterTexture(Files[i], new TextureParameters(null, Color24.Black), out Textures[j]);
										}
										break;
								}
							}
						}
					}
				}
			}
			return Textures;
		}

		/// <summary>Inverts the lightness values of a texture used for a glow</summary>
		/// <param name="bytes">A byte array containing the texture to invert</param>
		private static void InvertLightness(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i += 4)
			{
				if (bytes[i] != 0 | bytes[i + 1] != 0 | bytes[i + 2] != 0)
				{
					int r = bytes[i + 0];
					int g = bytes[i + 1];
					int b = bytes[i + 2];
					if (g <= r & r <= b | b <= r & r <= g)
					{
						bytes[i + 0] = (byte)(255 + r - g - b);
						bytes[i + 1] = (byte)(255 - b);
						bytes[i + 2] = (byte)(255 - g);
					}
					else if (r <= g & g <= b | b <= g & g <= r)
					{
						bytes[i + 0] = (byte)(255 - b);
						bytes[i + 1] = (byte)(255 + g - r - b);
						bytes[i + 2] = (byte)(255 - r);
					}
					else
					{
						bytes[i + 0] = (byte)(255 - g);
						bytes[i + 1] = (byte)(255 - r);
						bytes[i + 2] = (byte)(255 + b - r - g);
					}
				}
			}
		}

		// ================================

		// get brightness
		private static double GetBrightness(ref RouteData Data, double TrackPosition) {
			double tmin = double.PositiveInfinity;
			double tmax = double.NegativeInfinity;
			double bmin = 1.0, bmax = 1.0;
			for (int i = 0; i < Data.Blocks.Length; i++) {
				for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++) {
					if (Data.Blocks[i].Brightness[j].TrackPosition <= TrackPosition) {
						tmin = Data.Blocks[i].Brightness[j].TrackPosition;
						bmin = (double)Data.Blocks[i].Brightness[j].Value;
					}
				}
			}
			for (int i = Data.Blocks.Length - 1; i >= 0; i--) {
				for (int j = Data.Blocks[i].Brightness.Length - 1; j >= 0; j--) {
					if (Data.Blocks[i].Brightness[j].TrackPosition >= TrackPosition) {
						tmax = Data.Blocks[i].Brightness[j].TrackPosition;
						bmax = (double)Data.Blocks[i].Brightness[j].Value;
					}
				}
			}
			if (tmin == double.PositiveInfinity & tmax == double.NegativeInfinity) {
				return 1.0;
			} else if (tmin == double.PositiveInfinity) {
				return (bmax - 1.0) * TrackPosition / tmax + 1.0;
			} else if (tmax == double.NegativeInfinity) {
				return bmin;
			} else if (tmin == tmax) {
				return 0.5 * (bmin + bmax);
			} else {
				double n = (TrackPosition - tmin) / (tmax - tmin);
				return (1.0 - n) * bmin + n * bmax;
			}
		}

		// apply route data
		private static void ApplyRouteData(string FileName, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly) {
			string SignalPath, LimitPath, LimitGraphicsPath, TransponderPath;
			ObjectManager.StaticObject SignalPost, LimitPostStraight, LimitPostLeft, LimitPostRight, LimitPostInfinite;
			ObjectManager.StaticObject LimitOneDigit, LimitTwoDigits, LimitThreeDigits, StopPost;
			ObjectManager.StaticObject TransponderS, TransponderSN, TransponderFalseStart, TransponderPOrigin, TransponderPStop;
			if (!PreviewOnly) {
				string CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
				// load compatibility objects
				SignalPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				SignalPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalPath, "signal_post.csv"), Encoding, false);
				LimitPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Limits");
				LimitGraphicsPath = OpenBveApi.Path.CombineDirectory(LimitPath, "Graphics");
				LimitPostStraight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_straight.csv"), Encoding, false);
				LimitPostLeft = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_left.csv"), Encoding, false);
				LimitPostRight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_right.csv"), Encoding, false);
				LimitPostInfinite = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_infinite.csv"), Encoding, false);
				LimitOneDigit = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_1_digit.csv"), Encoding, false);
				LimitTwoDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_2_digits.csv"), Encoding, false);
				LimitThreeDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_3_digits.csv"), Encoding, false);
				StopPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(CompatibilityFolder, "stop.csv"), Encoding, false);
				TransponderPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Transponders");
				TransponderS = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "s.csv"), Encoding, false);
				TransponderSN = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "sn.csv"), Encoding, false);
				TransponderFalseStart = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "falsestart.csv"), Encoding, false);
				TransponderPOrigin = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "porigin.csv"), Encoding, false);
				TransponderPStop = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "pstop.csv"), Encoding, false);
			} else {
				SignalPath = null;
				LimitPath = null;
				LimitGraphicsPath = null;
				TransponderPath = null;
				SignalPost = null;
				LimitPostStraight = null;
				LimitPostLeft = null;
				LimitPostRight = null;
				LimitPostInfinite = null;
				LimitOneDigit = null;
				LimitTwoDigits = null;
				LimitThreeDigits = null;
				StopPost = null;
				TransponderS = null;
				TransponderSN = null;
				TransponderFalseStart = null;
				TransponderPOrigin = null;
				TransponderPStop = null;
			}
			// initialize
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			int LastBlock = (int)Math.Floor((Data.TrackPosition + 600.0) / Data.BlockInterval + 0.001) + 1;
			int BlocksUsed = Data.Blocks.Length;
			CreateMissingBlocks(ref Data, ref BlocksUsed, LastBlock, PreviewOnly);
			Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
			// interpolate height
			if (!PreviewOnly) {
				int z = 0;
				for (int i = 0; i < Data.Blocks.Length; i++) {
					if (!double.IsNaN(Data.Blocks[i].Height)) {
						for (int j = i - 1; j >= 0; j--) {
							if (!double.IsNaN(Data.Blocks[j].Height)) {
								double a = Data.Blocks[j].Height;
								double b = Data.Blocks[i].Height;
								double d = (b - a) / (double)(i - j);
								for (int k = j + 1; k < i; k++) {
									a += d;
									Data.Blocks[k].Height = a;
								}
								break;
							}
						}
						z = i;
					}
				}
				for (int i = z + 1; i < Data.Blocks.Length; i++) {
					Data.Blocks[i].Height = Data.Blocks[z].Height;
				}
			}
			// background
			if (!PreviewOnly) {
				if (Data.Blocks[0].Background >= 0 & Data.Blocks[0].Background < Data.Backgrounds.Length) {
					World.CurrentBackground = Data.Backgrounds[Data.Blocks[0].Background];
				} else {
					World.CurrentBackground = new World.Background(null, 6, false);
				}
				World.TargetBackground = World.CurrentBackground;
			}
			// brightness
			int CurrentBrightnessElement = -1;
			int CurrentBrightnessEvent = -1;
			float CurrentBrightnessValue = 1.0f;
			double CurrentBrightnessTrackPosition = (double)Data.FirstUsedBlock * Data.BlockInterval;
			if (!PreviewOnly) {
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++) {
					if (Data.Blocks[i].Brightness != null && Data.Blocks[i].Brightness.Length != 0) {
						CurrentBrightnessValue = Data.Blocks[i].Brightness[0].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[0].Value;
						break;
					}
				}
			}
			// create objects and track
			Vector3 Position = Vector3.Zero;
			Vector2 Direction = new Vector2(0.0, 1.0);
			TrackManager.CurrentTrack = new TrackManager.Track();
			TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[] { };
			double CurrentSpeedLimit = double.PositiveInfinity;
			int CurrentRunIndex = 0;
			int CurrentFlangeIndex = 0;
			if (Data.FirstUsedBlock < 0) Data.FirstUsedBlock = 0;
			TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[256];
			int CurrentTrackLength = 0;
			int PreviousFogElement = -1;
			int PreviousFogEvent = -1;
			Game.Fog PreviousFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, Color24.Grey, -Data.BlockInterval);
			Game.Fog CurrentFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, Color24.Grey, 0.0);
			// process blocks
			double progressFactor = Data.Blocks.Length - Data.FirstUsedBlock == 0 ? 0.5 : 0.5 / (double)(Data.Blocks.Length - Data.FirstUsedBlock);
			for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++) {
				Loading.RouteProgress = 0.6667 + (double)(i - Data.FirstUsedBlock) * progressFactor;
				if ((i & 15) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				double StartingDistance = (double)i * Data.BlockInterval;
				double EndingDistance = StartingDistance + Data.BlockInterval;
				// normalize
				World.Normalize(ref Direction.X, ref Direction.Y);
				// track
				if (!PreviewOnly) {
					if (Data.Blocks[i].Cycle.Length == 1 && Data.Blocks[i].Cycle[0] == -1) {
						if (Data.Structure.Cycle.Length == 0 || Data.Structure.Cycle[0] == null) {
							Data.Blocks[i].Cycle = new int[] { 0 };
						} else {
							Data.Blocks[i].Cycle = Data.Structure.Cycle[0];
						}
					}
				}
				TrackManager.TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
				int n = CurrentTrackLength;
				if (n >= TrackManager.CurrentTrack.Elements.Length) {
					Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, TrackManager.CurrentTrack.Elements.Length << 1);
				}
				CurrentTrackLength++;
				TrackManager.CurrentTrack.Elements[n] = WorldTrackElement;
				TrackManager.CurrentTrack.Elements[n].WorldPosition = Position;
				TrackManager.CurrentTrack.Elements[n].WorldDirection = Vector3.GetVector3(Direction, Data.Blocks[i].Pitch);
				TrackManager.CurrentTrack.Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
				TrackManager.CurrentTrack.Elements[n].WorldUp = Vector3.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection, TrackManager.CurrentTrack.Elements[n].WorldSide);
				TrackManager.CurrentTrack.Elements[n].StartingTrackPosition = StartingDistance;
				TrackManager.CurrentTrack.Elements[n].Events = new TrackManager.GeneralEvent[] { };
				TrackManager.CurrentTrack.Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
				TrackManager.CurrentTrack.Elements[n].CsvRwAccuracyLevel = Data.Blocks[i].Accuracy;
				// background
				if (!PreviewOnly) {
					if (Data.Blocks[i].Background >= 0) {
						int typ;
						if (i == Data.FirstUsedBlock) {
							typ = Data.Blocks[i].Background;
						} else {
							typ = Data.Backgrounds.Length > 0 ? 0 : -1;
							for (int j = i - 1; j >= Data.FirstUsedBlock; j--) {
								if (Data.Blocks[j].Background >= 0) {
									typ = Data.Blocks[j].Background;
									break;
								}
							}
						}
						if (typ >= 0 & typ < Data.Backgrounds.Length) {
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BackgroundChangeEvent(0.0, Data.Backgrounds[typ], Data.Backgrounds[Data.Blocks[i].Background]);
						}
					}
				}
				// brightness
				if (!PreviewOnly) {
					for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++) {
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						double d = Data.Blocks[i].Brightness[j].TrackPosition - StartingDistance;
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BrightnessChangeEvent(d, Data.Blocks[i].Brightness[j].Value, CurrentBrightnessValue, Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition, Data.Blocks[i].Brightness[j].Value, 0.0);
						if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0) {
							TrackManager.BrightnessChangeEvent bce = (TrackManager.BrightnessChangeEvent)TrackManager.CurrentTrack.Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
							bce.NextBrightness = Data.Blocks[i].Brightness[j].Value;
							bce.NextDistance = Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition;
						}
						CurrentBrightnessElement = n;
						CurrentBrightnessEvent = m;
						CurrentBrightnessValue = Data.Blocks[i].Brightness[j].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[j].TrackPosition;
					}
				}
				// fog
				if (!PreviewOnly) {
					if (Data.FogTransitionMode) {
						if (Data.Blocks[i].FogDefined) {
							Data.Blocks[i].Fog.TrackPosition = StartingDistance;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog);
							if (PreviousFogElement >= 0 & PreviousFogEvent >= 0) {
								TrackManager.FogChangeEvent e = (TrackManager.FogChangeEvent)TrackManager.CurrentTrack.Elements[PreviousFogElement].Events[PreviousFogEvent];
								e.NextFog = Data.Blocks[i].Fog;
							} else {
								Game.PreviousFog = PreviousFog;
								Game.CurrentFog = PreviousFog;
								Game.NextFog = Data.Blocks[i].Fog;
							}
							PreviousFog = Data.Blocks[i].Fog;
							PreviousFogElement = n;
							PreviousFogEvent = m;
						}
					} else {
						Data.Blocks[i].Fog.TrackPosition = StartingDistance + Data.BlockInterval;
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, CurrentFog, Data.Blocks[i].Fog);
						PreviousFog = CurrentFog;
						CurrentFog = Data.Blocks[i].Fog;
					}
				}
				// rail sounds
				if (!PreviewOnly) {
					int j = Data.Blocks[i].RailType[0];
					int r = j < Data.Structure.Run.Length ? Data.Structure.Run[j] : 0;
					int f = j < Data.Structure.Flange.Length ? Data.Structure.Flange[j] : 0;
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.RailSoundsChangeEvent(0.0, CurrentRunIndex, CurrentFlangeIndex, r, f);
					CurrentRunIndex = r;
					CurrentFlangeIndex = f;
				}
				// point sound
				if (!PreviewOnly) {
					if (i < Data.Blocks.Length - 1) {
						bool q = false;
						for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
							if (Data.Blocks[i].Rail[j].RailStart & Data.Blocks[i + 1].Rail.Length > j) {
								bool qx = Math.Sign(Data.Blocks[i].Rail[j].RailStartX) != Math.Sign(Data.Blocks[i + 1].Rail[j].RailEndX);
								bool qy = Data.Blocks[i].Rail[j].RailStartY * Data.Blocks[i + 1].Rail[j].RailEndY <= 0.0;
								if (qx & qy) {
									q = true;
									break;
								}
							}
						}
						if (q) {
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(0.0, null, false, false, true, Vector3.Zero, 12.5);
						}
					}
				}
				// station
				if (Data.Blocks[i].Station >= 0) {
					// station
					int s = Data.Blocks[i].Station;
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.StationStartEvent(0.0, s);
					double dx, dy = 3.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors) {
						dx = -5.0;
					} else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors) {
						dx = 5.0;
					} else {
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
					// passalarm
					if (!PreviewOnly) {
						if (Data.Blocks[i].StationPassAlarm) {
							int b = i - 6;
							if (b >= 0) {
								int j = b - Data.FirstUsedBlock;
								if (j >= 0) {
									m = TrackManager.CurrentTrack.Elements[j].Events.Length;
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[j].Events, m + 1);
									TrackManager.CurrentTrack.Elements[j].Events[m] = new TrackManager.StationPassAlarmEvent(0.0);
								}
							}
						}
					}
				}
				// stop
				for (int j = 0; j < Data.Blocks[i].Stop.Length; j++) {
					int s = Data.Blocks[i].Stop[j].Station;
					int t = Game.Stations[s].Stops.Length;
					Array.Resize<Game.StationStop>(ref Game.Stations[s].Stops, t + 1);
					Game.Stations[s].Stops[t].TrackPosition = Data.Blocks[i].Stop[j].TrackPosition;
					Game.Stations[s].Stops[t].ForwardTolerance = Data.Blocks[i].Stop[j].ForwardTolerance;
					Game.Stations[s].Stops[t].BackwardTolerance = Data.Blocks[i].Stop[j].BackwardTolerance;
					Game.Stations[s].Stops[t].Cars = Data.Blocks[i].Stop[j].Cars;
					double dx, dy = 2.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors) {
						dx = -5.0;
					} else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors) {
						dx = 5.0;
					} else {
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
				}
				// limit
				for (int j = 0; j < Data.Blocks[i].Limit.Length; j++) {
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					double d = Data.Blocks[i].Limit[j].TrackPosition - StartingDistance;
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.LimitChangeEvent(d, CurrentSpeedLimit, Data.Blocks[i].Limit[j].Speed);
					CurrentSpeedLimit = Data.Blocks[i].Limit[j].Speed;
				}
				// marker
				if (!PreviewOnly) {
					for (int j = 0; j < Data.Markers.Length; j++) {
						if (Data.Markers[j].StartingPosition >= StartingDistance & Data.Markers[j].StartingPosition < EndingDistance) {
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Markers[j].StartingPosition - StartingDistance;
							Texture t = Data.Markers[j].Texture;
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerStartEvent(d, t);
						}
						if (Data.Markers[j].EndingPosition >= StartingDistance & Data.Markers[j].EndingPosition < EndingDistance) {
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Markers[j].EndingPosition - StartingDistance;
							Texture t = Data.Markers[j].Texture;
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerEndEvent(d, t);
						}
					}
				}
				// sound
				if (!PreviewOnly) {
					for (int j = 0; j < Data.Blocks[i].Sound.Length; j++) {
						if (Data.Blocks[i].Sound[j].Type == SoundType.TrainStatic | Data.Blocks[i].Sound[j].Type == SoundType.TrainDynamic) {
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Sound[j].TrackPosition - StartingDistance;
							switch (Data.Blocks[i].Sound[j].Type) {
								case SoundType.TrainStatic:
									TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundBuffer, true, true, false, Vector3.Zero, 0.0);
									break;
								case SoundType.TrainDynamic:
									TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundBuffer, false, false, true, Vector3.Zero, Data.Blocks[i].Sound[j].Speed);
									break;
							}
						}
					}
				}
				// turn
				if (Data.Blocks[i].Turn != 0.0) {
					double ag = -Math.Atan(Data.Blocks[i].Turn);
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					Direction.Rotate(cosag, sinag);
					TrackManager.CurrentTrack.Elements[n].WorldDirection.RotatePlane(cosag, sinag);
					TrackManager.CurrentTrack.Elements[n].WorldSide.RotatePlane(cosag, sinag);
					TrackManager.CurrentTrack.Elements[n].WorldUp = Vector3.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection, TrackManager.CurrentTrack.Elements[n].WorldSide);
				}
				if (Data.Blocks[i].Pitch != 0.0)
				{
					TrackManager.CurrentTrack.Elements[n].Pitch = Data.Blocks[i].Pitch;
				}
				else
				{
					TrackManager.CurrentTrack.Elements[n].Pitch = 0.0;
				}
				// curves
				double a = 0.0;
				double c = Data.BlockInterval;
				double h = 0.0;
				if (WorldTrackElement.CurveRadius != 0.0 & Data.Blocks[i].Pitch != 0.0) {
					double d = Data.BlockInterval;
					double p = Data.Blocks[i].Pitch;
					double r = WorldTrackElement.CurveRadius;
					double s = d / Math.Sqrt(1.0 + p * p);
					h = s * p;
					double b = s / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				} else if (WorldTrackElement.CurveRadius != 0.0) {
					double d = Data.BlockInterval;
					double r = WorldTrackElement.CurveRadius;
					double b = d / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				} else if (Data.Blocks[i].Pitch != 0.0) {
					double p = Data.Blocks[i].Pitch;
					double d = Data.BlockInterval;
					c = d / Math.Sqrt(1.0 + p * p);
					h = c * p;
				}
				double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
				double TrackPitch = Math.Atan(Data.Blocks[i].Pitch);
				Transformation GroundTransformation = new Transformation(TrackYaw, 0.0, 0.0);
				Transformation TrackTransformation = new Transformation(TrackYaw, TrackPitch, 0.0);
				Transformation NullTransformation = new Transformation(0.0, 0.0, 0.0);
				// ground
				if (!PreviewOnly) {
					int cb = (int)Math.Floor((double)i + 0.001);
					int ci = (cb % Data.Blocks[i].Cycle.Length + Data.Blocks[i].Cycle.Length) % Data.Blocks[i].Cycle.Length;
					int gi = Data.Blocks[i].Cycle[ci];
					if (gi >= 0 & Data.Structure.Ground.ContainsKey(gi)) {
							ObjectManager.CreateObject(Data.Structure.Ground[Data.Blocks[i].Cycle[ci]], Position + new Vector3(0.0, -Data.Blocks[i].Height, 0.0), GroundTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
					}
				}
				// ground-aligned free objects
				if (!PreviewOnly) {
					for (int j = 0; j < Data.Blocks[i].GroundFreeObj.Length; j++) {
						int sttype = Data.Blocks[i].GroundFreeObj[j].Type;
						double d = Data.Blocks[i].GroundFreeObj[j].TrackPosition - StartingDistance;
						double dx = Data.Blocks[i].GroundFreeObj[j].X;
						double dy = Data.Blocks[i].GroundFreeObj[j].Y;
						Vector3 wpos = Position + new Vector3(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx);
						double tpos = Data.Blocks[i].GroundFreeObj[j].TrackPosition;
						ObjectManager.CreateObject(Data.Structure.FreeObjects[sttype], wpos, GroundTransformation, new Transformation(Data.Blocks[i].GroundFreeObj[j].Yaw, Data.Blocks[i].GroundFreeObj[j].Pitch, Data.Blocks[i].GroundFreeObj[j].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
					}
				}
				// rail-aligned objects
				if (!PreviewOnly) {
					for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
						if (j > 0 && !Data.Blocks[i].Rail[j].RailStart) continue;
						// rail
						Vector3 pos;
						Transformation RailTransformation;
						double planar, updown;
						if (j == 0) {
							// rail 0
							pos = Position;
							planar = 0.0;
							updown = 0.0;
							RailTransformation = new Transformation(TrackTransformation, planar, updown, 0.0);
							pos = Position;
						} else {
							// rails 1-infinity
							double x = Data.Blocks[i].Rail[j].RailStartX;
							double y = Data.Blocks[i].Rail[j].RailStartY;
							Vector3 offset = new Vector3(Direction.Y * x, y, -Direction.X * x);
							pos = Position + offset;
							double dh;
							if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j) {
								// take orientation of upcoming block into account
								Vector2 Direction2 = Direction;
								Vector3 Position2 = Position;
								Position2.X += Direction.X * c;
								Position2.Y += h;
								Position2.Z += Direction.Y * c;
								if (a != 0.0) {
									Direction2.Rotate(Math.Cos(-a), Math.Sin(-a));
								}
								if (Data.Blocks[i + 1].Turn != 0.0) {
									double ag = -Math.Atan(Data.Blocks[i + 1].Turn);
									double cosag = Math.Cos(ag);
									double sinag = Math.Sin(ag);
									Direction2.Rotate(cosag, sinag);
								}
								double a2 = 0.0;
								// double c2 = Data.BlockInterval;
								// double h2 = 0.0;
								if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0 & Data.Blocks[i + 1].Pitch != 0.0) {
									double d2 = Data.BlockInterval;
									double p2 = Data.Blocks[i + 1].Pitch;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double s2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									// h2 = s2 * p2;
									double b2 = s2 / Math.Abs(r2);
									// c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									Direction2.Rotate(Math.Cos(-a2), Math.Sin(-a2));
								} else if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0) {
									double d2 = Data.BlockInterval;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double b2 = d2 / Math.Abs(r2);
									// c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									Direction2.Rotate(Math.Cos(-a2), Math.Sin(-a2));
								} else if (Data.Blocks[i + 1].Pitch != 0.0) {
									// double p2 = Data.Blocks[i + 1].Pitch;
									// double d2 = Data.BlockInterval;
									// c2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									// h2 = c2 * p2;
								}
								// double TrackYaw2 = Math.Atan2(Direction2.X, Direction2.Y);
								// double TrackPitch2 = Math.Atan(Data.Blocks[i + 1].Pitch);
								// Transformation GroundTransformation2 = new Transformation(TrackYaw2, 0.0, 0.0);
								// Transformation TrackTransformation2 = new Transformation(TrackYaw2, TrackPitch2, 0.0);
								double x2 = Data.Blocks[i + 1].Rail[j].RailEndX;
								double y2 = Data.Blocks[i + 1].Rail[j].RailEndY;
								Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);
								Vector3 pos2 = Position2 + offset2;
								Vector3 r = new Vector3(pos2.X - pos.X, pos2.Y - pos.Y, pos2.Z - pos.Z);
								r.Normalize();
								RailTransformation.Z = r;
								RailTransformation.X = new Vector3(r.Z, 0.0, -r.X);
								World.Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
								RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
								double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
								double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
								planar = Math.Atan(dx / c);
								dh = dy / c;
								updown = Math.Atan(dh);
							} else {
								planar = 0.0;
								dh = 0.0;
								updown = 0.0;
								RailTransformation = new Transformation(TrackTransformation, 0.0, 0.0, 0.0);
							}
						}
						if (Data.Structure.RailObjects.ContainsKey(Data.Blocks[i].RailType[j])) { 
							ObjectManager.CreateObject(Data.Structure.RailObjects[Data.Blocks[i].RailType[j]], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
						}
						// points of interest
						for (int k = 0; k < Data.Blocks[i].PointsOfInterest.Length; k++) {
							if (Data.Blocks[i].PointsOfInterest[k].RailIndex == j) {
								double d = Data.Blocks[i].PointsOfInterest[k].TrackPosition - StartingDistance;
								double x = Data.Blocks[i].PointsOfInterest[k].X;
								double y = Data.Blocks[i].PointsOfInterest[k].Y;
								int m = Game.PointsOfInterest.Length;
								Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, m + 1);
								Game.PointsOfInterest[m].TrackPosition = Data.Blocks[i].PointsOfInterest[k].TrackPosition;
								if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j) {
									double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
									double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
									dx = Data.Blocks[i].Rail[j].RailStartX + d / Data.BlockInterval * dx;
									dy = Data.Blocks[i].Rail[j].RailStartY + d / Data.BlockInterval * dy;
									Game.PointsOfInterest[m].TrackOffset = new Vector3(x + dx, y + dy, 0.0);
								} else {
									double dx = Data.Blocks[i].Rail[j].RailStartX;
									double dy = Data.Blocks[i].Rail[j].RailStartY;
									Game.PointsOfInterest[m].TrackOffset = new Vector3(x + dx, y + dy, 0.0);
								}
								Game.PointsOfInterest[m].TrackYaw = Data.Blocks[i].PointsOfInterest[k].Yaw + planar;
								Game.PointsOfInterest[m].TrackPitch = Data.Blocks[i].PointsOfInterest[k].Pitch + updown;
								Game.PointsOfInterest[m].TrackRoll = Data.Blocks[i].PointsOfInterest[k].Roll;
								Game.PointsOfInterest[m].Text = Data.Blocks[i].PointsOfInterest[k].Text;
							}
						}
						// poles
						if (Data.Blocks[i].RailPole.Length > j && Data.Blocks[i].RailPole[j].Exists) {
							double dz = StartingDistance / Data.Blocks[i].RailPole[j].Interval;
							dz -= Math.Floor(dz + 0.5);
							if (dz >= -0.01 & dz <= 0.01) {
								if (Data.Blocks[i].RailPole[j].Mode == 0) {
									if (Data.Blocks[i].RailPole[j].Location <= 0.0) {
										ObjectManager.CreateObject(Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									} else {
										UnifiedObject Pole = GetMirroredObject(Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type]);
										ObjectManager.CreateObject(Pole, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
								} else {
									int m = Data.Blocks[i].RailPole[j].Mode;
									double dx = -Data.Blocks[i].RailPole[j].Location * 3.8;
									double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
									Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(updown), Math.Sin(wa));
									w.Normalize();
									double sx = Direction.Y;
									double sy = 0.0;
									double sz = -Direction.X;
									Vector3 wpos = pos + new Vector3(sx * dx + w.X * dz, sy * dx + w.Y * dz, sz * dx + w.Z * dz);
									int type = Data.Blocks[i].RailPole[j].Type;
									ObjectManager.CreateObject(Data.Structure.Poles[m][type], wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
								}
							}
						}
						// walls
						if (Data.Blocks[i].RailWall.Length > j && Data.Blocks[i].RailWall[j].Exists) {
							if (Data.Blocks[i].RailWall[j].Direction <= 0) {
								ObjectManager.CreateObject(Data.Structure.WallL[Data.Blocks[i].RailWall[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
							if (Data.Blocks[i].RailWall[j].Direction >= 0) {
								ObjectManager.CreateObject(Data.Structure.WallR[Data.Blocks[i].RailWall[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
						}
						// dikes
						if (Data.Blocks[i].RailDike.Length > j && Data.Blocks[i].RailDike[j].Exists) {
							if (Data.Blocks[i].RailDike[j].Direction <= 0) {
								ObjectManager.CreateObject(Data.Structure.DikeL[Data.Blocks[i].RailDike[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
							if (Data.Blocks[i].RailDike[j].Direction >= 0) {
								ObjectManager.CreateObject(Data.Structure.DikeR[Data.Blocks[i].RailDike[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
						}
						// sounds
						if (j == 0) {
							for (int k = 0; k < Data.Blocks[i].Sound.Length; k++) {
								if (Data.Blocks[i].Sound[k].Type == SoundType.World) {
									if (Data.Blocks[i].Sound[k].SoundBuffer != null) {
										double d = Data.Blocks[i].Sound[k].TrackPosition - StartingDistance;
										double dx = Data.Blocks[i].Sound[k].X;
										double dy = Data.Blocks[i].Sound[k].Y;
										double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
										Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(updown), Math.Sin(wa));
										w.Normalize();
										Vector3 s = new Vector3(Direction.Y, 0.0, -Direction.X);
										Vector3 u = Vector3.Cross(w, s);
										Vector3 wpos = pos + new Vector3(s.X * dx + u.X * dy + w.X * d, s.Y * dx + u.Y * dy + w.Y * d, s.Z * dx + u.Z * dy + w.Z * d);
										Sounds.PlaySound(Data.Blocks[i].Sound[k].SoundBuffer, 1.0, 1.0, wpos, null, -1, true);
									}
								}
							}
						}
						// forms
						for (int k = 0; k < Data.Blocks[i].Form.Length; k++) {
							// primary rail
							if (Data.Blocks[i].Form[k].PrimaryRail == j) {
								if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailStub) {
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType)) { 
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										if (Data.Blocks[i].Form[k].RoofType > 0) {
											if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
												Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											} else {
												ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
										}
									}
								} else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailL) {
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (!Data.Structure.FormCL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0) {
										if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
										if (!Data.Structure.RoofCL.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								} else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailR) {
									if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (!Data.Structure.FormCR.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0) {
										if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
										if (!Data.Structure.RoofCR.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								} else if (Data.Blocks[i].Form[k].SecondaryRail > 0) {
									int p = Data.Blocks[i].Form[k].PrimaryRail;
									double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
									double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
									int s = Data.Blocks[i].Form[k].SecondaryRail;
									if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart) {
										Interface.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
									} else {
										double sx0 = Data.Blocks[i].Rail[s].RailStartX;
										double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
										double d0 = sx0 - px0;
										double d1 = sx1 - px1;
										if (d0 < 0.0) {
											if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
												Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											} else {
												ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (!Data.Structure.FormCL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
												Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											} else {
												ObjectManager.StaticObject FormC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], d0, d1);
												ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (Data.Blocks[i].Form[k].RoofType > 0) {
												if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												} else {
													ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
												if (!Data.Structure.RoofCL.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												} else {
													ObjectManager.StaticObject RoofC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], d0, d1);
													ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
											}
										} else if (d0 > 0.0) {
											if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
												Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											} else {
												ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (!Data.Structure.FormCR.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
												Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											} else {
												ObjectManager.StaticObject FormC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], d0, d1);
												ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (Data.Blocks[i].Form[k].RoofType > 0) {
												if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												} else {
													ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
												if (!Data.Structure.RoofCR.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
													Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												} else {
													ObjectManager.StaticObject RoofC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], d0, d1);
													ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
											}
										}
									}
								}
							}
							// secondary rail
							if (Data.Blocks[i].Form[k].SecondaryRail == j) {
								int p = Data.Blocks[i].Form[k].PrimaryRail;
								double px = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
								int s = Data.Blocks[i].Form[k].SecondaryRail;
								double sx = Data.Blocks[i].Rail[s].RailStartX;
								double d = px - sx;
								if (d < 0.0) {
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0) {
										if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								} else {
									if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType)) {
										Interface.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									} else {
										ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0) {
										if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType)) {
											Interface.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
							}
						}
						// cracks
						for (int k = 0; k < Data.Blocks[i].Crack.Length; k++) {
							if (Data.Blocks[i].Crack[k].PrimaryRail == j) {
								int p = Data.Blocks[i].Crack[k].PrimaryRail;
								double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
								double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
								int s = Data.Blocks[i].Crack[k].SecondaryRail;
								if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart) {
									Interface.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
								} else {
									double sx0 = Data.Blocks[i].Rail[s].RailStartX;
									double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
									double d0 = sx0 - px0;
									double d1 = sx1 - px1;
									if (d0 < 0.0) {
										if (!Data.Structure.CrackL.ContainsKey(Data.Blocks[i].Crack[k].Type)) {
											Interface.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.CrackL[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									} else if (d0 > 0.0) {
										if (!Data.Structure.CrackR.ContainsKey(Data.Blocks[i].Crack[k].Type)) {
											Interface.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										} else {
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.CrackR[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
							}
						}
						// free objects
						if (Data.Blocks[i].RailFreeObj.Length > j && Data.Blocks[i].RailFreeObj[j] != null) {
							for (int k = 0; k < Data.Blocks[i].RailFreeObj[j].Length; k++) {
								int sttype = Data.Blocks[i].RailFreeObj[j][k].Type;
								double dx = Data.Blocks[i].RailFreeObj[j][k].X;
								double dy = Data.Blocks[i].RailFreeObj[j][k].Y;
								double dz = Data.Blocks[i].RailFreeObj[j][k].TrackPosition - StartingDistance;
								Vector3 wpos = pos;
								wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
								wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
								wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
								double tpos = Data.Blocks[i].RailFreeObj[j][k].TrackPosition;
								ObjectManager.CreateObject(Data.Structure.FreeObjects[sttype], wpos, RailTransformation, new Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch, Data.Blocks[i].RailFreeObj[j][k].Roll), -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0);
							}
						}
						// transponder objects
						if (j == 0) {
							for (int k = 0; k < Data.Blocks[i].Transponder.Length; k++) {
								UnifiedObject obj = null;
								if (Data.Blocks[i].Transponder[k].ShowDefaultObject) {
									switch (Data.Blocks[i].Transponder[k].Type) {
											case TrackManager.TransponderType.SLong: obj = TransponderS; break;
											case TrackManager.TransponderType.SN: obj = TransponderSN; break;
											case TrackManager.TransponderType.AccidentalDeparture: obj = TransponderFalseStart; break;
											case TrackManager.TransponderType.AtsPPatternOrigin: obj = TransponderPOrigin; break;
											case TrackManager.TransponderType.AtsPImmediateStop: obj = TransponderPStop; break;
									}
								} else {
									int b = Data.Blocks[i].Transponder[k].BeaconStructureIndex;
									if (Data.Structure.Beacon.ContainsKey(b)) {
										obj = Data.Structure.Beacon[b];
									}
								}
								if (obj != null) {
									double dx = Data.Blocks[i].Transponder[k].X;
									double dy = Data.Blocks[i].Transponder[k].Y;
									double dz = Data.Blocks[i].Transponder[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Transponder[k].TrackPosition;
									if (Data.Blocks[i].Transponder[k].ShowDefaultObject) {
										double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
										ObjectManager.CreateObject(obj, wpos, RailTransformation, new Transformation(Data.Blocks[i].Transponder[k].Yaw, Data.Blocks[i].Transponder[k].Pitch, Data.Blocks[i].Transponder[k].Roll), -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
									} else {
										ObjectManager.CreateObject(obj, wpos, RailTransformation, new Transformation(Data.Blocks[i].Transponder[k].Yaw, Data.Blocks[i].Transponder[k].Pitch, Data.Blocks[i].Transponder[k].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
									}
								}
							}
							for (int k = 0; k < Data.Blocks[i].DestinationChanges.Length; k++)
							{
								UnifiedObject obj = null;
								int b = Data.Blocks[i].DestinationChanges[k].BeaconStructureIndex;
								if (b >= 0 & Data.Structure.Beacon.ContainsKey(b))
								{
									obj = Data.Structure.Beacon[b];
								}
								if (obj != null)
								{
									double dx = Data.Blocks[i].DestinationChanges[k].X;
									double dy = Data.Blocks[i].DestinationChanges[k].Y;
									double dz = Data.Blocks[i].DestinationChanges[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].DestinationChanges[k].TrackPosition;
									ObjectManager.CreateObject(obj, wpos, RailTransformation, new Transformation(Data.Blocks[i].DestinationChanges[k].Yaw, Data.Blocks[i].DestinationChanges[k].Pitch, Data.Blocks[i].DestinationChanges[k].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
								}
							}
						}
						// sections/signals/transponders
						if (j == 0) {
							// signals
							for (int k = 0; k < Data.Blocks[i].Signal.Length; k++) {
								SignalData sd;
								if (Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex >= 0) {
									sd = Data.CompatibilitySignalData[Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex];
								} else {
									sd = Data.SignalData[Data.Blocks[i].Signal[k].SignalObjectIndex];
								}
								// objects
								double dz = Data.Blocks[i].Signal[k].TrackPosition - StartingDistance;
								if (Data.Blocks[i].Signal[k].ShowPost) {
									// post
									double dx = Data.Blocks[i].Signal[k].X;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(SignalPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
								}
								if (Data.Blocks[i].Signal[k].ShowObject) {
									// signal object
									double dx = Data.Blocks[i].Signal[k].X;
									double dy = Data.Blocks[i].Signal[k].Y;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									if (sd is AnimatedObjectSignalData) {
										AnimatedObjectSignalData aosd = (AnimatedObjectSignalData)sd;
										ObjectManager.CreateObject(aosd.Objects, wpos, RailTransformation, new Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0);
									} else if (sd is CompatibilitySignalData) {
										CompatibilitySignalData csd = (CompatibilitySignalData)sd;
										if (csd.Numbers.Length != 0) {
											double brightness = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
											ObjectManager.AnimatedObjectCollection aoc = new ObjectManager.AnimatedObjectCollection();
											aoc.Objects = new ObjectManager.AnimatedObject[1];
											aoc.Objects[0] = new ObjectManager.AnimatedObject();
											aoc.Objects[0].States = new ObjectManager.AnimatedObjectState[csd.Numbers.Length];
											for (int l = 0; l < csd.Numbers.Length; l++)
											{
												aoc.Objects[0].States[l].Object = csd.Objects[l].Clone();
											}
											string expr = "";
											for (int l = 0; l < csd.Numbers.Length - 1; l++) {
												expr += "section " + csd.Numbers[l].ToString(Culture) + " <= " + l.ToString(Culture) + " ";
											}
											expr += (csd.Numbers.Length - 1).ToString(Culture);
											for (int l = 0; l < csd.Numbers.Length - 1; l++) {
												expr += " ?";
											}
											aoc.Objects[0].StateFunction = new FunctionScript(Program.CurrentHost, expr, false);
											aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Game.Generator.NextDouble();
											ObjectManager.CreateObject(aoc, wpos, RailTransformation, new Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, brightness);
										}
									} else if (sd is Bve4SignalData) {
										Bve4SignalData b4sd = (Bve4SignalData)sd;
										if (b4sd.SignalTextures.Length != 0) {
											int m = Math.Max(b4sd.SignalTextures.Length, b4sd.GlowTextures.Length);
											int zn = 0;
											for (int l = 0; l < m; l++) {
												if (l < b4sd.SignalTextures.Length && b4sd.SignalTextures[l] != null || l < b4sd.GlowTextures.Length && b4sd.GlowTextures[l] != null) {
													zn++;
												}
											}
											ObjectManager.AnimatedObjectCollection aoc = new ObjectManager.AnimatedObjectCollection();
											aoc.Objects = new ObjectManager.AnimatedObject[1];
											aoc.Objects[0] = new ObjectManager.AnimatedObject();
											aoc.Objects[0].States = new ObjectManager.AnimatedObjectState[zn];
											int zi = 0;
											string expr = "";
											for (int l = 0; l < m; l++) {
												bool qs = l < b4sd.SignalTextures.Length && b4sd.SignalTextures[l] != null;
												bool qg = l < b4sd.GlowTextures.Length && b4sd.GlowTextures[l] != null;
												if (qs & qg) {
													ObjectManager.StaticObject so = ObjectManager.CloneObject(b4sd.BaseObject, b4sd.SignalTextures[l], null);
													ObjectManager.StaticObject go = ObjectManager.CloneObject(b4sd.GlowObject, b4sd.GlowTextures[l], null);
													so.JoinObjects(go);
													aoc.Objects[0].States[zi].Object = so;
												} else if (qs) {
													ObjectManager.StaticObject so = ObjectManager.CloneObject(b4sd.BaseObject, b4sd.SignalTextures[l], null);
													aoc.Objects[0].States[zi].Object = so;
												} else if (qg) {
													ObjectManager.StaticObject go = ObjectManager.CloneObject(b4sd.GlowObject, b4sd.GlowTextures[l], null);
													aoc.Objects[0].States[zi].Object = go;
												}
												if (qs | qg) {
													if (zi < zn - 1) {
														expr += "section " + l.ToString(Culture) + " <= " + zi.ToString(Culture) + " ";
													} else {
														expr += zi.ToString(Culture);
													}
													zi++;
												}
											}
											for (int l = 0; l < zn - 1; l++) {
												expr += " ?";
											}
											aoc.Objects[0].StateFunction = new FunctionScript(Program.CurrentHost, expr, false);
											aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Game.Generator.NextDouble();
											ObjectManager.CreateObject(aoc, wpos, RailTransformation, new Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0);
										}
									}
								}
							}
							// sections
							for (int k = 0; k < Data.Blocks[i].Section.Length; k++) {
								int m = Game.Sections.Length;
								Array.Resize<Game.Section>(ref Game.Sections, m + 1);
								// create associated transponders
								for (int g = 0; g <= i; g++) {
									for (int l = 0; l < Data.Blocks[g].Transponder.Length; l++) {
										if (Data.Blocks[g].Transponder[l].Type != TrackManager.TransponderType.None & Data.Blocks[g].Transponder[l].Section == m) {
											int o = TrackManager.CurrentTrack.Elements[n - i + g].Events.Length;
											Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n - i + g].Events, o + 1);
											double dt = Data.Blocks[g].Transponder[l].TrackPosition - StartingDistance + (double)(i - g) * Data.BlockInterval;
											TrackManager.CurrentTrack.Elements[n - i + g].Events[o] = new TrackManager.TransponderEvent(dt, Data.Blocks[g].Transponder[l].Type, Data.Blocks[g].Transponder[l].SwitchSubsystem, Data.Blocks[g].Transponder[l].OptionalInteger, Data.Blocks[g].Transponder[l].OptionalFloat, m);
											Data.Blocks[g].Transponder[l].Type = TrackManager.TransponderType.None;
										}
									}
								}
								// create section
								Game.Sections[m].TrackPosition = Data.Blocks[i].Section[k].TrackPosition;
								Game.Sections[m].Aspects = new Game.SectionAspect[Data.Blocks[i].Section[k].Aspects.Length];
								for (int l = 0; l < Data.Blocks[i].Section[k].Aspects.Length; l++) {
									Game.Sections[m].Aspects[l].Number = Data.Blocks[i].Section[k].Aspects[l];
									if (Data.Blocks[i].Section[k].Aspects[l] >= 0 & Data.Blocks[i].Section[k].Aspects[l] < Data.SignalSpeeds.Length) {
										Game.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Data.Blocks[i].Section[k].Aspects[l]];
									} else {
										Game.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
									}
								}
								Game.Sections[m].Type = Data.Blocks[i].Section[k].Type;
								Game.Sections[m].CurrentAspect = -1;
								if (m > 0) {
									Game.Sections[m].PreviousSection = m - 1;
									Game.Sections[m - 1].NextSection = m;
								} else {
									Game.Sections[m].PreviousSection = -1;
								}
								Game.Sections[m].NextSection = -1;
								Game.Sections[m].StationIndex = Data.Blocks[i].Section[k].DepartureStationIndex;
								Game.Sections[m].Invisible = Data.Blocks[i].Section[k].Invisible;
								Game.Sections[m].Trains = new TrainManager.Train[] { };
								// create section change event
								double d = Data.Blocks[i].Section[k].TrackPosition - StartingDistance;
								int p = TrackManager.CurrentTrack.Elements[n].Events.Length;
								Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, p + 1);
								TrackManager.CurrentTrack.Elements[n].Events[p] = new TrackManager.SectionChangeEvent(d, m - 1, m);
							}
							// transponders introduced after corresponding sections
							for (int l = 0; l < Data.Blocks[i].Transponder.Length; l++) {
								if (Data.Blocks[i].Transponder[l].Type != TrackManager.TransponderType.None) {
									int t = Data.Blocks[i].Transponder[l].Section;
									if (t >= 0 & t < Game.Sections.Length) {
										int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
										Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
										double dt = Data.Blocks[i].Transponder[l].TrackPosition - StartingDistance;
										TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(dt, Data.Blocks[i].Transponder[l].Type, Data.Blocks[i].Transponder[l].SwitchSubsystem, Data.Blocks[i].Transponder[l].OptionalInteger, Data.Blocks[i].Transponder[l].OptionalFloat, t);
										Data.Blocks[i].Transponder[l].Type = TrackManager.TransponderType.None;
									}
								}
							}
						}
						// limit
						if (j == 0) {
							for (int k = 0; k < Data.Blocks[i].Limit.Length; k++) {
								if (Data.Blocks[i].Limit[k].Direction != 0) {
									double dx = 2.2 * (double)Data.Blocks[i].Limit[k].Direction;
									double dz = Data.Blocks[i].Limit[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Limit[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									if (Data.Blocks[i].Limit[k].Speed <= 0.0 | Data.Blocks[i].Limit[k].Speed >= 1000.0) {
										ObjectManager.CreateStaticObject(LimitPostInfinite, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
									} else {
										if (Data.Blocks[i].Limit[k].Cource < 0) {
											ObjectManager.CreateStaticObject(LimitPostLeft, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
										} else if (Data.Blocks[i].Limit[k].Cource > 0) {
											ObjectManager.CreateStaticObject(LimitPostRight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
										} else {
											ObjectManager.CreateStaticObject(LimitPostStraight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
										}
										double lim = Data.Blocks[i].Limit[k].Speed / Data.UnitOfSpeed;
										if (lim < 10.0) {
											int d0 = (int)Math.Round(lim);
											int o = ObjectManager.CreateStaticObject(LimitOneDigit, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
										} else if (lim < 100.0) {
											int d1 = (int)Math.Round(lim);
											int d0 = d1 % 10;
											d1 /= 10;
											int o = ObjectManager.CreateStaticObject(LimitTwoDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
										} else {
											int d2 = (int)Math.Round(lim);
											int d0 = d2 % 10;
											int d1 = (d2 / 10) % 10;
											d2 /= 100;
											int o = ObjectManager.CreateStaticObject(LimitThreeDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d2 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 3) {
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
										}
									}
								}
							}
						}
						// stop
						if (j == 0) {
							for (int k = 0; k < Data.Blocks[i].Stop.Length; k++) {
								if (Data.Blocks[i].Stop[k].Direction != 0) {
									double dx = 1.8 * (double)Data.Blocks[i].Stop[k].Direction;
									double dz = Data.Blocks[i].Stop[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Stop[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(StopPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b);
								}
							}
						}
					}
				}
				// finalize block
				Position.X += Direction.X * c;
				Position.Y += h;
				Position.Z += Direction.Y * c;
				if (a != 0.0) {
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				}
			}
			// orphaned transponders
			if (!PreviewOnly) {
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++) {
					for (int j = 0; j < Data.Blocks[i].Transponder.Length; j++) {
						if (Data.Blocks[i].Transponder[j].Type != TrackManager.TransponderType.None) {
							int n = i - Data.FirstUsedBlock;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Transponder[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
							int s = Data.Blocks[i].Transponder[j].Section;
							if (s >= 0) s = -1;
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(d, Data.Blocks[i].Transponder[j].Type, Data.Blocks[i].Transponder[j].SwitchSubsystem, Data.Blocks[i].Transponder[j].OptionalInteger, Data.Blocks[i].Transponder[j].OptionalFloat, s);
							Data.Blocks[i].Transponder[j].Type = TrackManager.TransponderType.None;
						}
					}
					// Destination Change Events
					for (int j = 0; j < Data.Blocks[i].DestinationChanges.Length; j++)
					{
						int n = i - Data.FirstUsedBlock;
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						double d = Data.Blocks[i].DestinationChanges[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
						//Destination events not supported in Route Viewer.....
						//TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.DestinationEvent(d, Data.Blocks[i].DestinationChanges[j].Type, Data.Blocks[i].DestinationChanges[j].NextDestination, Data.Blocks[i].DestinationChanges[j].PreviousDestination, Data.Blocks[i].DestinationChanges[j].TriggerOnce);
					}
				}
			}
			// insert station end events
			for (int i = 0; i < Game.Stations.Length; i++) {
				int j = Game.Stations[i].Stops.Length - 1;
				if (j >= 0) {
					double p = Game.Stations[i].Stops[j].TrackPosition + Game.Stations[i].Stops[j].ForwardTolerance + Data.BlockInterval;
					int k = (int)Math.Floor(p / (double)Data.BlockInterval) - Data.FirstUsedBlock;
					if (k >= 0 & k < Data.Blocks.Length) {
						double d = p - (double)(k + Data.FirstUsedBlock) * (double)Data.BlockInterval;
						int m = TrackManager.CurrentTrack.Elements[k].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[k].Events, m + 1);
						TrackManager.CurrentTrack.Elements[k].Events[m] = new TrackManager.StationEndEvent(d, i);
					}
				}
			}
			// create default point of interests
			if (Game.PointsOfInterest.Length == 0) {
				Game.PointsOfInterest = new OpenBve.Game.PointOfInterest[Game.Stations.Length];
				int n = 0;
				for (int i = 0; i < Game.Stations.Length; i++) {
					if (Game.Stations[i].Stops.Length != 0) {
						Game.PointsOfInterest[n].Text = Game.Stations[i].Name;
						Game.PointsOfInterest[n].TrackPosition = Game.Stations[i].Stops[0].TrackPosition;
						Game.PointsOfInterest[n].TrackOffset = new Vector3(0.0, 2.8, 0.0);
						if (Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors) {
							Game.PointsOfInterest[n].TrackOffset.X = -2.5;
						} else if (!Game.Stations[i].OpenLeftDoors & Game.Stations[i].OpenRightDoors) {
							Game.PointsOfInterest[n].TrackOffset.X = 2.5;
						}
						n++;
					}
				}
				Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, n);
			}
			// convert block-based cant into point-based cant
			for (int i = CurrentTrackLength - 1; i >= 1; i--) {
				if (TrackManager.CurrentTrack.Elements[i].CurveCant == 0.0) {
					TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
				} else if (TrackManager.CurrentTrack.Elements[i - 1].CurveCant != 0.0) {
					if (Math.Sign(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) == Math.Sign(TrackManager.CurrentTrack.Elements[i].CurveCant)) {
						if (Math.Abs(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) > Math.Abs(TrackManager.CurrentTrack.Elements[i].CurveCant)) {
							TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
						}
					} else {
						TrackManager.CurrentTrack.Elements[i].CurveCant = 0.5 * (TrackManager.CurrentTrack.Elements[i].CurveCant + TrackManager.CurrentTrack.Elements[i - 1].CurveCant);
					}
				}
			}
			// finalize
			Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, CurrentTrackLength);
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].Stops.Length == 0 & Game.Stations[i].StopMode != Game.StationStopMode.AllPass) {
					Interface.AddMessage(MessageType.Warning, false, "Station " + Game.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + Game.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					Game.Stations[i].StopMode = Game.StationStopMode.AllPass;
				}
				if (Game.Stations[i].StationType == Game.StationType.ChangeEnds) {
					if (i < Game.Stations.Length - 1) {
						if (Game.Stations[i + 1].StopMode != Game.StationStopMode.AllStop) {
							Interface.AddMessage(MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but the subsequent station does not expect all trains to stop in file " + FileName);
							Game.Stations[i + 1].StopMode = Game.StationStopMode.AllStop;
						}
					} else {
						Interface.AddMessage(MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but there is no subsequent station defined in file " + FileName);
						Game.Stations[i].StationType = Game.StationType.Terminal;
					}
				}
			}
			if (Game.Stations.Length != 0) {
				Game.Stations[Game.Stations.Length - 1].StationType = Game.StationType.Terminal;
			}
			if (TrackManager.CurrentTrack.Elements.Length != 0) {
				int n = TrackManager.CurrentTrack.Elements.Length - 1;
				int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
				Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
				TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TrackEndEvent(Data.BlockInterval);
			}
			if (!PreviewOnly) {
				ComputeCantTangents();
				int subdivisions = (int)Math.Floor(Data.BlockInterval / 5.0);
				if (subdivisions >= 2) {
					SmoothenOutTurns(subdivisions);
					ComputeCantTangents();
				}
			}
		}
		
		// ------------------

		// compute cant tangents
		private static void ComputeCantTangents() {
			if (TrackManager.CurrentTrack.Elements.Length == 1) {
				TrackManager.CurrentTrack.Elements[0].CurveCantTangent = 0.0;
			} else if (TrackManager.CurrentTrack.Elements.Length != 0) {
				double[] deltas = new double[TrackManager.CurrentTrack.Elements.Length - 1];
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++) {
					deltas[i] = TrackManager.CurrentTrack.Elements[i + 1].CurveCant - TrackManager.CurrentTrack.Elements[i].CurveCant;
				}
				double[] tangents = new double[TrackManager.CurrentTrack.Elements.Length];
				tangents[0] = deltas[0];
				tangents[TrackManager.CurrentTrack.Elements.Length - 1] = deltas[TrackManager.CurrentTrack.Elements.Length - 2];
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++) {
					tangents[i] = 0.5 * (deltas[i - 1] + deltas[i]);
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++) {
					if (deltas[i] == 0.0) {
						tangents[i] = 0.0;
						tangents[i + 1] = 0.0;
					} else {
						double a = tangents[i] / deltas[i];
						double b = tangents[i + 1] / deltas[i];
						if (a * a + b * b > 9.0) {
							double t = 3.0 / Math.Sqrt(a * a + b * b);
							tangents[i] = t * a * deltas[i];
							tangents[i + 1] = t * b * deltas[i];
						}
					}
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
					TrackManager.CurrentTrack.Elements[i].CurveCantTangent = tangents[i];
				}
			}
		}
		
		// ------------------
		
		
		// smoothen out turns
		private static void SmoothenOutTurns(int subdivisions) {
			if (subdivisions < 2) {
				throw new InvalidOperationException();
			}
			// subdivide track
			int length = TrackManager.CurrentTrack.Elements.Length;
			int newLength = (length - 1) * subdivisions + 1;
			double[] midpointsTrackPositions = new double[newLength];
			Vector3[] midpointsWorldPositions = new Vector3[newLength];
			Vector3[] midpointsWorldDirections = new Vector3[newLength];
			Vector3[] midpointsWorldUps = new Vector3[newLength];
			Vector3[] midpointsWorldSides = new Vector3[newLength];
			double[] midpointsCant = new double[newLength];
			for (int i = 0; i < newLength; i++) {
				int m = i % subdivisions;
				if (m != 0) {
					int q = i / subdivisions;
					TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
					double r = (double)m / (double)subdivisions;
					double p = (1.0 - r) * TrackManager.CurrentTrack.Elements[q].StartingTrackPosition + r * TrackManager.CurrentTrack.Elements[q + 1].StartingTrackPosition;
					TrackManager.UpdateTrackFollower(ref follower, -1.0, true, false);
					TrackManager.UpdateTrackFollower(ref follower, p, true, false);
					midpointsTrackPositions[i] = p;
					midpointsWorldPositions[i] = follower.WorldPosition;
					midpointsWorldDirections[i] = follower.WorldDirection;
					midpointsWorldUps[i] = follower.WorldUp;
					midpointsWorldSides[i] = follower.WorldSide;
					midpointsCant[i] = follower.CurveCant;
				}
			}
			Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, newLength);
			for (int i = length - 1; i >= 1; i--) {
				TrackManager.CurrentTrack.Elements[subdivisions * i] = TrackManager.CurrentTrack.Elements[i];
			}
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				int m = i % subdivisions;
				if (m != 0) {
					int q = i / subdivisions;
					int j = q * subdivisions;
					TrackManager.CurrentTrack.Elements[i] = TrackManager.CurrentTrack.Elements[j];
					TrackManager.CurrentTrack.Elements[i].Events = new TrackManager.GeneralEvent[] { };
					TrackManager.CurrentTrack.Elements[i].StartingTrackPosition = midpointsTrackPositions[i];
					TrackManager.CurrentTrack.Elements[i].WorldPosition = midpointsWorldPositions[i];
					TrackManager.CurrentTrack.Elements[i].WorldDirection = midpointsWorldDirections[i];
					TrackManager.CurrentTrack.Elements[i].WorldUp = midpointsWorldUps[i];
					TrackManager.CurrentTrack.Elements[i].WorldSide = midpointsWorldSides[i];
					TrackManager.CurrentTrack.Elements[i].CurveCant = midpointsCant[i];
					TrackManager.CurrentTrack.Elements[i].CurveCantTangent = 0.0;
				}
			}
			// find turns
			bool[] isTurn = new bool[TrackManager.CurrentTrack.Elements.Length];
			{
				TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++) {
					int m = i % subdivisions;
					if (m == 0) {
						double p = 0.00000001 * TrackManager.CurrentTrack.Elements[i - 1].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
						TrackManager.UpdateTrackFollower(ref follower, p, true, false);
						Vector3 d1 = TrackManager.CurrentTrack.Elements[i].WorldDirection;
						Vector3 d2 = follower.WorldDirection;
						Vector3 d = d1 - d2;
						double t = d.X * d.X + d.Z * d.Z;
						const double e = 0.0001;
						if (t > e) {
							isTurn[i] = true;
						}
					}
				}
			}
			// replace turns by curves
			double totalShortage = 0.0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				if (isTurn[i]) {
					// estimate radius
					Vector3 AP = TrackManager.CurrentTrack.Elements[i - 1].WorldPosition;
					Vector3 AS = TrackManager.CurrentTrack.Elements[i - 1].WorldSide;
					Vector3 BP = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition;
					Vector3 BS = TrackManager.CurrentTrack.Elements[i + 1].WorldSide;
					Vector3 S = AS - BS;
					double rx;
					if (S.X * S.X > 0.000001) {
						rx = (BP.X - AP.X) / S.X;
					} else {
						rx = 0.0;
					}
					double rz;
					if (S.Z * S.Z > 0.000001) {
						rz = (BP.Z - AP.Z) / S.Z;
					} else {
						rz = 0.0;
					}
					if (rx != 0.0 | rz != 0.0) {
						double r;
						if (rx != 0.0 & rz != 0.0) {
							if (Math.Sign(rx) == Math.Sign(rz)) {
								double f = rx / rz;
								if (f > -1.1 & f < -0.9 | f > 0.9 & f < 1.1) {
									r = Math.Sqrt(Math.Abs(rx * rz)) * Math.Sign(rx);
								} else {
									r = 0.0;
								}
							} else {
								r = 0.0;
							}
						} else if (rx != 0.0) {
							r = rx;
						} else {
							r = rz;
						}
						if (r * r > 1.0) {
							// apply radius
							TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
							TrackManager.CurrentTrack.Elements[i - 1].CurveRadius = r;
							double p = 0.00000001 * TrackManager.CurrentTrack.Elements[i - 1].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							TrackManager.CurrentTrack.Elements[i].CurveRadius = r;
							//TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i].CurveCant;
							//TrackManager.CurrentTrack.Elements[i].CurveCantInterpolation = TrackManager.CurrentTrack.Elements[i].CurveCantInterpolation;
							TrackManager.CurrentTrack.Elements[i].WorldPosition = follower.WorldPosition;
							TrackManager.CurrentTrack.Elements[i].WorldDirection = follower.WorldDirection;
							TrackManager.CurrentTrack.Elements[i].WorldUp = follower.WorldUp;
							TrackManager.CurrentTrack.Elements[i].WorldSide = follower.WorldSide;
							// iterate to shorten track element length
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							Vector3 d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- follower.WorldPosition;
							double bestT = d.NormSquared();
							int bestJ = 0;
							int n = 1000;
							double a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
							for (int j = 1; j < n - 1; j++) {
								TrackManager.UpdateTrackFollower(ref follower, TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								double t = d.NormSquared();
								if (t < bestT) {
									bestT = t;
									bestJ = j;
								} else {
									break;
								}
							}
							double s = (double)bestJ * a;
							for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++) {
								TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
							}
							totalShortage += s;
							// introduce turn to compensate for curve
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							Vector3 AB = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- follower.WorldPosition;
							Vector3 AC = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- TrackManager.CurrentTrack.Elements[i].WorldPosition;
							Vector3 BC = follower.WorldPosition- TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double sa = Math.Sqrt(BC.X * BC.X + BC.Z * BC.Z);
							double sb = Math.Sqrt(AC.X * AC.X + AC.Z * AC.Z);
							double sc = Math.Sqrt(AB.X * AB.X + AB.Z * AB.Z);
							double denominator = 2.0 * sa * sb;
							if (denominator != 0.0) {
								double originalAngle;
								{
									double value = (sa * sa + sb * sb - sc * sc) / denominator;
									if (value < -1.0) {
										originalAngle = Math.PI;
									} else if (value > 1.0) {
										originalAngle = 0;
									} else {
										originalAngle = Math.Acos(value);
									}
								}
								TrackManager.TrackElement originalTrackElement = TrackManager.CurrentTrack.Elements[i];
								bestT = double.MaxValue;
								bestJ = 0;
								for (int j = -1; j <= 1; j++) {
									double g = (double)j * originalAngle;
									double cosg = Math.Cos(g);
									double sing = Math.Sin(g);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									TrackManager.CurrentTrack.Elements[i].WorldDirection.Rotate(Vector3.Down, cosg, sing);
									TrackManager.CurrentTrack.Elements[i].WorldUp.Rotate(Vector3.Down, cosg, sing);
									TrackManager.CurrentTrack.Elements[i].WorldSide.Rotate(Vector3.Down, cosg, sing);
									p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
									TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
									TrackManager.UpdateTrackFollower(ref follower, p, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- follower.WorldPosition;
									double t = d.NormSquared();
									if (t < bestT) {
										bestT = t;
										bestJ = j;
									}
								}
								{
									double newAngle = (double)bestJ * originalAngle;
									double cosg = Math.Cos(newAngle);
									double sing = Math.Sin(newAngle);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									TrackManager.CurrentTrack.Elements[i].WorldDirection.Rotate(Vector3.Down, cosg, sing);
									TrackManager.CurrentTrack.Elements[i].WorldUp.Rotate(Vector3.Down, cosg, sing);
									TrackManager.CurrentTrack.Elements[i].WorldSide.Rotate(Vector3.Down, cosg, sing);
								}
								// iterate again to further shorten track element length
								p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
								TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
								TrackManager.UpdateTrackFollower(ref follower, p, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- follower.WorldPosition;
								bestT = d.NormSquared();
								bestJ = 0;
								n = 1000;
								a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
								for (int j = 1; j < n - 1; j++) {
									TrackManager.UpdateTrackFollower(ref follower, TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- follower.WorldPosition;
									double t = d.NormSquared();
									if (t < bestT) {
										bestT = t;
										bestJ = j;
									} else {
										break;
									}
								}
								s = (double)bestJ * a;
								for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++) {
									TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
								}
								totalShortage += s;
							}
							// compensate for height difference
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							Vector3 d1 = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition- TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a1 = Math.Atan(d1.Y / Math.Sqrt(d1.X * d1.X + d1.Z * d1.Z));
							Vector3 d2 = follower.WorldPosition- TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a2 = Math.Atan(d2.Y / Math.Sqrt(d2.X * d2.X + d2.Z * d2.Z));
							double b = a2 - a1;
							if (b * b > 0.00000001) {
								double cosa = Math.Cos(b);
								double sina = Math.Sin(b);
								TrackManager.CurrentTrack.Elements[i].WorldDirection.Rotate(TrackManager.CurrentTrack.Elements[i].WorldSide, cosa, sina);
								TrackManager.CurrentTrack.Elements[i].WorldUp.Rotate(TrackManager.CurrentTrack.Elements[i].WorldSide, cosa, sina);
							}
						}
					}
				}
			}
			// correct events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++) {
				double startingTrackPosition = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double endingTrackPosition = TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
					double p = startingTrackPosition + TrackManager.CurrentTrack.Elements[i].Events[j].TrackPositionDelta;
					if (p >= endingTrackPosition) {
						int len = TrackManager.CurrentTrack.Elements[i + 1].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i + 1].Events, len + 1);
						TrackManager.CurrentTrack.Elements[i + 1].Events[len] = TrackManager.CurrentTrack.Elements[i].Events[j];
						TrackManager.CurrentTrack.Elements[i + 1].Events[len].TrackPositionDelta += startingTrackPosition - endingTrackPosition;
						for (int k = j; k < TrackManager.CurrentTrack.Elements[i].Events.Length - 1; k++) {
							TrackManager.CurrentTrack.Elements[i].Events[k] = TrackManager.CurrentTrack.Elements[i].Events[k + 1];
						}
						len = TrackManager.CurrentTrack.Elements[i].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, len - 1);
						j--;
					}
				}
			}
		}

	}
}
