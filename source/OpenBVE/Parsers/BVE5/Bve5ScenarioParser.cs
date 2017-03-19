using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	partial class Bve5ScenarioParser
	{
		//Structures
		private struct Rail
		{
			internal bool RailStart;
			internal bool RailStartRefreshed;
			internal double RailStartX;
			internal double RailStartY;
			internal bool RailEnd;
			internal double RailEndX;
			internal double RailEndY;
			internal string Key;
		}

		private struct Limit
		{
			internal double TrackPosition;
			internal double Speed;
			internal int Direction;
			internal int Cource;
			internal double LastSpeed;
		}

		private struct Stop
		{
			internal double TrackPosition;
			internal int Station;
			internal int Direction;
			internal double ForwardTolerance;
			internal double BackwardTolerance;
			internal int Cars;
		}

		private struct Object
		{
			internal double TrackPosition;
			internal string Name;
			internal int Type;
			internal double X;
			internal double Y;
			internal double Z;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}

		private struct Crack
		{
			internal int PrimaryRail;
			internal int SecondaryRail;
			internal int Type;
		}

		private struct RepeaterType
		{
			internal int Type;
			internal int ObjectArrayIndex;

			public RepeaterType(RepeaterType typeToClone)
			{
				Type = typeToClone.Type;
				ObjectArrayIndex = typeToClone.ObjectArrayIndex;
			}
		}

		

		private struct Repeater
		{
			/// <summary>The name-key that this is referred to by in the routefile</summary>
			internal string Name;

			internal int Type;
			/// <summary>The objects to use</summary>
			internal int[] StructureTypes;
			/// <summary>The length of the object</summary>
			internal double Span;
			/// <summary>The distance at which the object should be repeated</summary>
			internal double RepetitionInterval;
			/// <summary>The distance at which the object was last placed</summary>
			internal double TrackPosition;

			internal int RailIndex;

			internal double X;
			internal double Y;
			internal double Z;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
		}

		private struct Signal
		{
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
		private struct Section
		{
			internal double TrackPosition;
			internal int[] Aspects;
			internal int DepartureStationIndex;
			internal bool Invisible;
			internal Game.SectionType Type;
		}

		private struct Station
		{
			string Name;
			Game.StationStopMode StopMode;
			Game.StationType StationType;
			double ArrivalTime;
			double DepartureTime;
			bool OpenLeftDoors;
			bool OpenRightDoors;
			bool PassAlarm;
			Game.SafetySystem SafetyDevice;
		}

		internal class Background
		{
			internal BackgroundManager.BackgroundHandle Handle;
			internal string Key;

			internal Background(string key, BackgroundManager.BackgroundHandle handle)
			{
				this.Key = key;
				this.Handle = handle;
			}
		}

		private struct ObjectPointer
		{
			internal string Name;
			internal string Path;
		}

		private struct Brightness
		{
			internal double TrackPosition;
			internal float Value;
		}

		private class Block
		{
			internal int Background;
			internal Brightness[] Brightness;
			internal Game.Fog Fog;
			internal bool FogDefined;
			internal int[] Cycle;
			internal double Height;
			internal Repeater[] Repeaters;
			internal Object[][] RailFreeObj;
			internal Object[] GroundFreeObj;
			internal Rail[] Rail;
			internal Crack[] Crack;
			internal int[] RailType;
			internal Signal[] Signal;
			internal Section[] Section;
			internal Limit[] Limit;
			internal Stop[] Stop;
			internal TrackSound[] RunSounds;
			internal TrackSound[] FlangeSounds;
			internal TrackManager.TrackElement CurrentTrackState;
			internal double Pitch;
			internal double Turn;
			internal int Station;
			internal bool StationPassAlarm;
			internal double Accuracy;
			internal double AdhesionMultiplier;
			internal bool JointNoise = false;
		}

		private struct TrackSound
		{
			internal double TrackPosition;
			internal int RunSoundIndex;
			internal int FlangeSoundIndex;

			internal TrackSound(double TrackPosition, int Run, int Flange)
			{
				this.TrackPosition = TrackPosition;
				this.RunSoundIndex = Run;
				this.FlangeSoundIndex = Flange;
			}
		}

		private struct StructureData
		{
			internal ObjectManager.UnifiedObject[] Objects;
			internal int[][] Cycle;
			internal int[] Run;
			internal int[] Flange;
		}

		/// <summary>An abstract signal - All signals must inherit from this class</summary>
		private abstract class SignalData { }


		/// <summary>Defines a BVE 4 standard signal:
		/// A signal has a face based mesh and glow
		/// Textures are then substituted according to the aspect
		/// </summary>
		private class CompatibilitySignalData : SignalData
		{
			internal readonly int[] Numbers;
			internal readonly ObjectManager.StaticObject[] Objects;
			internal readonly string Key;
			internal CompatibilitySignalData(int[] Numbers, ObjectManager.StaticObject[] Objects, string Key)
			{
				this.Numbers = Numbers;
				this.Objects = Objects;
				this.Key = Key;
			}
		}

		private struct RouteData
		{
			internal double TrackPosition;
			internal double BlockInterval;
			/// <summary>OpenBVE runs internally in meters per second
			/// This value is used to convert between the speed set by Options.UnitsOfSpeed and m/s
			/// </summary>
			internal double UnitOfSpeed;
			/// <summary>If this bool is set to FALSE, then objects will disappear when the block in which their command is placed is exited via by the camera
			/// Certain BVE2/4 era routes used this as an animation trick
			/// </summary>
			internal bool AccurateObjectDisposal;
			internal bool SignedCant;
			internal bool FogTransitionMode;
			internal StructureData Structure;
			internal SignalData[] SignalData;
			internal CompatibilitySignalData[] CompatibilitySignalData;
			internal Textures.Texture[] TimetableDaytime;
			internal Textures.Texture[] TimetableNighttime;
			internal Background[] Backgrounds;
			internal double[] SignalSpeeds;
			internal Block[] Blocks;
			//internal Marker[] Markers;
			internal int FirstUsedBlock;

			internal ObjectPointer[] ObjectList;
			internal Station[] StationList;

			internal int UsedObjects;
			internal float LastBrightness;
		}

		internal static void ParseRoute(string FileName, bool IsRW, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, bool PreviewOnly)
		{
			if (Encoding == null)
			{
				Encoding = Encoding.ASCII;
			}
			string[] Lines = File.ReadAllLines(FileName);
			// initialize data
			Game.UnitOfSpeed = "km/h";
			Game.SpeedConversionFactor = 0.0;
			Game.RouteInformation.RouteBriefing = null;
			//		    customLoadScreen = false;
			string CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
			RouteData Data = new RouteData
			{
				BlockInterval = 25.0,
				AccurateObjectDisposal = true,
				FirstUsedBlock = -1,
				Blocks = new Block[1]
			};
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
			Data.ObjectList = new ObjectPointer[0];
			Data.StationList = new Station[0];
			Data.LastBrightness = 1.0f;
			
			if (!PreviewOnly)
			{
				Data.Blocks[0].Background = 0;
				Data.Blocks[0].Brightness = new Brightness[] { };
				Data.Blocks[0].Fog.Start = Game.NoFogStart;
				Data.Blocks[0].Fog.End = Game.NoFogEnd;
				Data.Blocks[0].Fog.Color = new Color24(128, 128, 128);
				Data.Blocks[0].Cycle = new int[] { -1 };
				Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
				Data.Blocks[0].RailFreeObj = new Object[][] { };
				Data.Blocks[0].GroundFreeObj = new Object[] { };
				Data.Blocks[0].Crack = new Crack[] { };
				Data.Blocks[0].Signal = new Signal[] { };
				Data.Blocks[0].Section = new Section[] { };
				Data.Blocks[0].RunSounds = new TrackSound[] { new TrackSound(0.0, 0, 0) };
				Data.Structure.Cycle = new int[][] { };
				Data.Structure.Run = new int[] { };
				Data.Structure.Flange = new int[] { };
				Data.Backgrounds = new Background[] { };
				Data.TimetableDaytime = new Textures.Texture[] { null, null, null, null };
				Data.TimetableNighttime = new Textures.Texture[] { null, null, null, null };
				Data.Structure.Objects = new ObjectManager.UnifiedObject[] { };
				Data.UnitOfSpeed = 0.277777777777778;

				Data.CompatibilitySignalData = new CompatibilitySignalData[0];
				// game data
				Game.Sections = new Game.Section[1];
				Game.Sections[0].Aspects = new Game.SectionAspect[] { new Game.SectionAspect(0, 0.0), new Game.SectionAspect(4, double.PositiveInfinity) };
				Game.Sections[0].CurrentAspect = 0;
				Game.Sections[0].NextSection = -1;
				Game.Sections[0].PreviousSection = -1;
				Game.Sections[0].SignalIndices = new int[] { };
				Game.Sections[0].StationIndex = -1;
				Game.Sections[0].TrackPosition = 0;
				Game.Sections[0].Trains = new TrainManager.Train[] { };

				/*
				 * These are the speed limits for the default Japanese signal aspects, and in most cases will be overwritten
				 */
				Data.SignalSpeeds = new double[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };
			}
			string RouteFile = String.Empty;
			//Load comment
			for (int i = 0; i < Lines.Length; i++)
			{
				var key = Lines[i].Split('=');
				switch (key[0].ToLowerInvariant().Trim())
				{
					case "comment":
						Game.RouteComment = key[1].Trim();
						break;
					case "route":
						RouteFile = Path.GetDirectoryName(FileName) + "\\" + key[1].Trim();
						break;
					case "image":
						Game.RouteImage = Path.GetDirectoryName(FileName) + "\\" + key[1].Trim();
						break;
				}
			}
			ParseRouteForData(RouteFile, Encoding, TrainPath, ObjectPath, SoundPath, ref Data, PreviewOnly);
			if (Loading.Cancel) return;
			ApplyRouteData(FileName, Encoding, ref Data, PreviewOnly);
		}

		private static void ParseRouteForData(string FileName, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, ref RouteData Data, bool PreviewOnly)
		{
			if (FileName == String.Empty)
			{
				throw new Exception("The BVE5 scenario did not define a route map");
			}
			if (!System.IO.File.Exists(FileName))
			{
				throw new Exception("The BVE5 route map file was not found");
			}
			int CurrentSection = 0;
			int BlocksUsed = Data.Blocks.Length;
			Game.Stations = new Game.Station[] { };
			//Read the entire routefile into memory
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			Expression[] Expressions;
			PreprocessSplitIntoExpressions(FileName, false, Lines, out Expressions, true, 0.0);
			double[] UnitOfLength = new double[] { 1.0 };
			PreprocessOptions(FileName, Expressions, ref Data, ref UnitOfLength, PreviewOnly, Encoding);
			int BlockIndex = 0;
			for (int e = 0; e < Expressions.Length; e++)
			{
				if (Expressions[e] == null || Expressions[e].Text.StartsWith("#") || Expressions[e].Text.StartsWith("//"))
				{
					//Skip comments and blank lines
					continue;
				}

				else if (Expressions[e].Text.ToLowerInvariant().StartsWith("include"))
				{

					int idx = Expressions[e].Text.IndexOf('\'');
					if (idx > 1)
					{
						string[] Arguments = Expressions[e].Text.Substring(idx + 1, Expressions[e].Text.Length - idx - 2).Split(',');
						string fn = Arguments[0];
						if (!System.IO.File.Exists(fn))
						{
							fn = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), fn);
						}
						if (System.IO.File.Exists(fn))
						{
							Lines = System.IO.File.ReadAllLines(fn);
							Expression[] IncludedExpressions;
							PreprocessSplitIntoExpressions(fn, false, Lines, out IncludedExpressions, true, 0.0);
							PreprocessOptions(fn, IncludedExpressions, ref Data, ref UnitOfLength, PreviewOnly, Encoding);
							int el = Expressions.Length;
							Expressions[e] = null;
							Array.Resize(ref Expressions, el + IncludedExpressions.Length);
							for (int i = 0; i < IncludedExpressions.Length; i++)
							{
								Expressions[el + i] = IncludedExpressions[i];
							}
						}

					}
				}
			}
			for (int e = 0; e < Expressions.Length; e++)
			{
				if (Expressions[e] == null)
				{
					continue;
				}
				double Number;
				int n = Expressions[e].Text.IndexOf(';');
				
				if (n != -1 && NumberFormats.TryParseDoubleVb6(Expressions[e].Text.Substring(0, n), out Number))
				{
					Expressions[e].Text = Expressions[e].Text.Substring(n, Expressions[e].Text.Length - n);
					BlockIndex = (int)Math.Floor(Number / Data.BlockInterval + 0.001);
					if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
					CreateMissingBlocks(ref Data, ref BlocksUsed, BlockIndex, PreviewOnly);
				}
				else if (NumberFormats.TryParseDouble(Expressions[e].Text, UnitOfLength, out Number))
				{
					//If our expression is a number, it must represent a track position
					if (Number < 0.0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "Negative track position encountered at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					}
					else
					{
						Data.TrackPosition = Number;
						BlockIndex = (int)Math.Floor(Number / Data.BlockInterval + 0.001);
						if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
						CreateMissingBlocks(ref Data, ref BlocksUsed, BlockIndex, PreviewOnly);
					}
				}
				else
				{
					string[] Commands = Expressions[e].Text.Split(';');
					for (int c = 0; c < Commands.Length; c++)
					{
						//Our expression contains route commands
						int idx = Commands[c].IndexOf('(');
						if (idx > 1)
						{
							//Horrible hack, but we've already validated parenthesis when we split into expressions......
							string[] Arguments = Commands[c].Substring(idx + 1, Commands[c].Length - idx - 2).Split(',');
							string command = Commands[c].Substring(0, idx).ToLowerInvariant();
							if (command.StartsWith("legacy."))
							{
								command = command.Substring(7, command.Length - 7);
								ParseLegacyCommand(command, Arguments, ref Data, BlockIndex, UnitOfLength, PreviewOnly);
								continue;
							}
							if (command.StartsWith("structure[") && !PreviewOnly)
							{
								//Rough equivilant of the FreeObj command
								int ida = Commands[c].IndexOf('[');
								int idb = Commands[c].IndexOf(']');
								int idc = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1);
								string type = Commands[c].Substring(idb + 2, idc - idb - 2).ToLowerInvariant();
								//Remove the single quotes BVE5 uses to surround names
								key = key.RemoveEnclosingQuotes();
								if (!PreviewOnly)
								{
									switch (type)
									{
										case "put":
											PutStructure(key, Arguments, ref Data, BlockIndex, UnitOfLength, false);
											break;
										case "put0":
											PutStructure(key, Arguments, ref Data, BlockIndex, UnitOfLength, true);
											break;
										case "putbetween":
											//Equivilant to the Track.Crack command
											PutStructureBetween(key, Arguments, ref Data, BlockIndex, UnitOfLength);
											break;
										default:
											//Unrecognised command, so break out of the loop and continue
											continue;
									}
								}
								continue;
							}
							if (command.StartsWith("station["))
							{
								//Add station
								int ida = Commands[c].IndexOf('[');
								int idb = Commands[c].IndexOf(']');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1);
								//Remove the single quotes BVE5 uses to surround names
								key = key.RemoveEnclosingQuotes();
								PutStation(key, Arguments, ref Data, BlockIndex, UnitOfLength);
								continue;
							}
							if (command.StartsWith("repeater[") && !PreviewOnly)
							{
								//Add repeater
								int ida = Commands[c].IndexOf('[');
								int idb = Commands[c].IndexOf(']');
								int idc = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1);
								string type = Commands[c].Substring(idb + 2, idc - idb - 2).ToLowerInvariant();
								bool Start;
								bool Type2 = false;
								switch (type)
								{
									case "begin":
										Start = true;
										Type2 = false;
										break;
									case "begin0":
										Start = true;
										Type2 = true;
										break;
									case "end":
										Start = false;
										break;
									default:
										//This is an unrecognised repeater command, so we need to break out of the enclosing loop and continue
										continue;
								}
								//Remove the single quotes BVE5 uses to surround names
								key = key.RemoveEnclosingQuotes();
								if (Start)
								{
									StartRepeater(key, Arguments, ref Data, BlockIndex, UnitOfLength, Type2);
								}
								else
								{
									EndRepeater(key, ref Data, BlockIndex, UnitOfLength);
								}
								continue;
							}
							if (command.StartsWith("speedlimit.") && !PreviewOnly)
							{
								//Add speed limit
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "begin":
										double limit;
										if (NumberFormats.TryParseDoubleVb6(Arguments[0], out limit))
										{
											StartSpeedLimit(limit, ref Data, BlockIndex, UnitOfLength);
										}
										break;
									case "end":
										break;

								}
								continue;
							}
							if (command.StartsWith("section.") && !PreviewOnly)
							{
								//Starts a new signalling section
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "begin":
										StartSection(Arguments, ref Data, BlockIndex, ref CurrentSection, UnitOfLength);
										break;
									default:
										//Unsupported section statement
										break;

								}
								continue;
							}
							if (command.StartsWith("signal") && !PreviewOnly && !command.StartsWith("signal.load") &&
							    !command.StartsWith("signal.speedlimit"))
							{
								//Add signal
								int ida = Commands[c].IndexOf('[');
								int idb = Commands[c].IndexOf(']');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1);
								//Remove the single quotes BVE5 uses to surround names
								key = key.RemoveEnclosingQuotes();
								if (!PreviewOnly)
								{
									PlaceSignal(key, Arguments, ref Data, BlockIndex, CurrentSection, UnitOfLength);
								}
								continue;
							}
							if (command.StartsWith("rollingnoise.") && !PreviewOnly)
							{
								//Change the wheel noise
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "change":
										int RollingNoise;
										if (NumberFormats.TryParseIntVb6(Arguments[0], out RollingNoise))
										{
											ChangeRunSound(RollingNoise, ref Data, BlockIndex);
										}
										break;
									default:
										//Not a number
										break;

								}
								continue;
							}
							if (command.StartsWith("flangenoise.") && !PreviewOnly)
							{
								//Change the flange noise
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "change":
										int FlangeNoise;
										if (NumberFormats.TryParseIntVb6(Arguments[0], out FlangeNoise))
										{
											ChangeFlangeSound(FlangeNoise, ref Data, BlockIndex);
										}
										break;
									default:
										//Not a number
										break;

								}
								continue;
							}
							if (command.StartsWith("track[") && !PreviewOnly)
							{
								int ida = Commands[c].IndexOf('[');
								int idb = Commands[c].IndexOf(']');
								int idc = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1);
								string type = Commands[c].Substring(idb + 2, idc - idb - 2).ToLowerInvariant();
								//Remove the single quotes BVE5 uses to surround names
								key = key.RemoveEnclosingQuotes();
								double Distance;
								switch (type)
								{
									case "position":
										SecondaryTrack(key, Arguments, ref Data, BlockIndex, UnitOfLength);
										break;
									case "y.interpolate":
										//Moves the track in the Y axis....
										if (NumberFormats.TryParseDoubleVb6(Arguments[0], out Distance))
										{
											InterpolateSecondaryTrack(key, Distance, ref Data, BlockIndex, UnitOfLength, true);
										}
										break;
									case "x.interpolate":
										//Moves the track in the X axis....
										if (NumberFormats.TryParseDoubleVb6(Arguments[0], out Distance))
										{
											InterpolateSecondaryTrack(key, Distance, ref Data, BlockIndex, UnitOfLength, false);
										}
										break;
									default:
										//Not currently supported....
										break;
								}
								continue;
							}
							if (command.StartsWith("light.") && !PreviewOnly)
							{
								//Configures the route light
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "ambient":
										SetAmbientLight(Arguments);
										break;
									case "diffuse":
										SetDiffuseLight(Arguments);
										break;
									case "direction":
										SetLightDirection(Arguments);
										break;

								}
								continue;
							}
							if (command.StartsWith("curve."))
							{
								//Configures the route light
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								SetCurve(key, Arguments, ref Data, BlockIndex, UnitOfLength);
								continue;
							}
							if (command.StartsWith("background.") && !PreviewOnly)
							{
								//Changes the current background
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								SetBackground(key, Arguments, ref Data, BlockIndex, UnitOfLength);
								continue;
							}
							if (command.StartsWith("adhesion.") && !PreviewOnly)
							{
								//Changes the current background
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								SetAdhesion(Arguments, ref Data, BlockIndex, UnitOfLength);
								continue;
							}
							if (command.StartsWith("jointnoise.") && !PreviewOnly)
							{
								//Changes the current background
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								PlayJointSound(Arguments, ref Data, BlockIndex);
								continue;
							}
							if (command.StartsWith("cabilluminance.") && !PreviewOnly)
							{
								//Sets cab illumination
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "set":
										ChangeBrightness(true, Arguments, ref Data, BlockIndex);
										break;
									case "interpolate":
										ChangeBrightness(false, Arguments, ref Data, BlockIndex);
										break;
								}
								continue;
							}
						}
						continue;
					}
				}
			}
		}

		private class Expression
		{
			internal string File;
			internal string Text;
			internal int Line;
			internal int Column;
			internal double TrackPositionOffset;
		}

		private static void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, out Expression[] Expressions, bool AllowRwRouteDescription, double trackPositionOffset)
		{
			Expressions = new Expression[4096];
			int e = 0;
			
			// parse
			for (int i = 0; i < Lines.Length; i++)
			{
				{
					// count expressions
					int n = 0; int Level = 0;
					for (int j = 0; j < Lines[i].Length; j++)
					{
						switch (Lines[i][j])
						{
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ';':
								if (Level == 0) n++;
								break;
						}
					}
					// create expressions
					int m = e + n + 1;
					while (m >= Expressions.Length)
					{
						Array.Resize<Expression>(ref Expressions, Expressions.Length << 1);
					}
					Level = 0;
					int a = 0, c = 0;
					for (int j = 0; j < Lines[i].Length; j++)
					{
						switch (Lines[i][j])
						{
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ';':
								if (Level == 0 & !IsRW)
								{
									string t = Lines[i].Substring(a, j - a).Trim();
									if (t.Length > 0 && !t.StartsWith(";"))
									{
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
					if (Lines[i].Length - a > 0)
					{
						string t = Lines[i].Substring(a).Trim();
						if (t.Length > 0 && !t.StartsWith(";"))
						{
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

		private static void PreprocessOptions(string FileName, Expression[] Expressions, ref RouteData Data, ref double[] UnitOfLength, bool PreviewOnly, System.Text.Encoding Encoding)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// process expressions
			for (int j = 0; j < Expressions.Length; j++)
			{
					// separate command and arguments
					string Command, ArgumentSequence;
					SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, true);
					// process command
					double Number;
					if (!NumberFormats.TryParseDoubleVb6(Command, UnitOfLength, out Number))
					{
						// split arguments
						string[] Arguments;
						{
							int n = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++)
							{
								if (ArgumentSequence[k] == ',')
								{
									n++;
								}
							}
							Arguments = new string[n + 1];
							int a = 0, h = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++)
							{
								if (ArgumentSequence[k] == ',')
								{
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								}
							}
							if (ArgumentSequence.Length - a > 0)
							{
								Arguments[h] = ArgumentSequence.Substring(a).Trim();
								h++;
							}
							Array.Resize<string>(ref Arguments, h);
						}


						// handle indices
						if (Command != null && Command.EndsWith(")"))
						{
							for (int k = Command.Length - 2; k >= 0; k--)
							{
								if (Command[k] == '(')
								{
									string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
									Command = Command.Substring(0, k).TrimEnd();
									int h = Indices.IndexOf(",", StringComparison.Ordinal);
									int CommandIndex1;
									if (h >= 0)
									{
										string a = Indices.Substring(0, h).TrimEnd();
										string b = Indices.Substring(h + 1).TrimStart();
										if (a.Length > 0 && !NumberFormats.TryParseIntVb6(a, out CommandIndex1))
										{
											Command = null; break;
										}
										int CommandIndex2;
										if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out CommandIndex2))
										{
											Command = null;
										}
									}
									else
									{
										if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out CommandIndex1))
										{
											Command = null;
										}
									}
									break;
								}
							}
						}
						// process command
						if (Command != null)
						{
							switch (Command.ToLowerInvariant())
							{
								//BVE5 structure definition files
								case "structure.load":
									{
										if (Arguments.Length == 0)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											for (int i = 0; i < Arguments.Length; i++)
											{
												//Remove the single quotes BVE5 uses to surround names
												if (Arguments[i].StartsWith("'") && Arguments[i].EndsWith("'"))
												{
													Arguments[i] = Arguments[i].Substring(1, Arguments[i].Length - 2);
												}
												//Call the loader method
												var StructureFile = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[i]);
												Encoding enc = Bve5ScenarioParser.DetermineFileEncoding(StructureFile);
												LoadObjects(StructureFile, ref Data, enc, PreviewOnly);
											}
										}
									} break;
								//BVE5 station definition files
								case "station.load":
									{
										if (Arguments.Length == 0)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											for (int i = 0; i < Arguments.Length; i++)
											{
												//Remove the single quotes BVE5 uses to surround names
												if (Arguments[i].StartsWith("'") && Arguments[i].EndsWith("'"))
												{
													Arguments[i] = Arguments[i].Substring(1, Arguments[i].Length - 2);
												}
												var StationFile = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[i]);
												Encoding enc = Bve5ScenarioParser.DetermineFileEncoding(StationFile);
												//Call the loader method
												LoadStations(StationFile, ref Data, enc, PreviewOnly);
											}
										}
									} break;
								//BVE5 station definition files
								case "signal.load":
									{
										if (PreviewOnly)
										{
											break;
										}
										if (Arguments.Length == 0)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										else
										{
											for (int i = 0; i < Arguments.Length; i++)
											{
												//Remove the single quotes BVE5 uses to surround names
												if (Arguments[i].StartsWith("'") && Arguments[i].EndsWith("'"))
												{
													Arguments[i] = Arguments[i].Substring(1, Arguments[i].Length - 2);
												}
												var SignalFile = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[i]);
												Encoding enc = Bve5ScenarioParser.DetermineFileEncoding(SignalFile);
												//Call the loader method
												LoadSections(SignalFile, ref Data, enc);
											}
										}
									} break;
							}
						}
					}
				
			}
		}



		private static void SeparateCommandsAndArguments(Expression Expression, out string Command, out string ArgumentSequence, System.Globalization.CultureInfo Culture, bool RaiseErrors)
		{
			if (Expression.Text[Expression.Text.Length -1] == ';')
			{
				//Strip extraneous semicolon (REQUIRES FIXING)
				Expression.Text = Expression.Text.Substring(0, Expression.Text.Length - 1);
			}
			bool openingerror = false, closingerror = false;
			int i;
			for (i = 0; i < Expression.Text.Length; i++)
			{
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " +
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
							break;
						}
						i++;
					}
					if (!found)
					{
						if (RaiseErrors & !closingerror)
						{
							Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							closingerror = true;
						}
						Expression.Text += ")";
					}
				}
				else if (Expression.Text[i] == ')')
				{
					if (RaiseErrors & !closingerror)
					{
						Interface.AddMessage(Interface.MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						closingerror = true;
					}
				}
				else if (char.IsWhiteSpace(Expression.Text[i]))
				{
					if (i >= Expression.Text.Length - 1 || !char.IsWhiteSpace(Expression.Text[i + 1]))
					{
						break;
					}
				}
			}
			if (i < Expression.Text.Length)
			{
				// white space was found outside of parentheses
				string a = Expression.Text.Substring(0, i);
				if (a.IndexOf('(') >= 0 & a.IndexOf(')') >= 0)
				{
					// indices found not separated from the command by spaces
					Command = Expression.Text.Substring(0, i).TrimEnd();
					ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
					if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")"))
					{
						// arguments are enclosed by parentheses
						ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
					}
					else if (ArgumentSequence.StartsWith("("))
					{
						// only opening parenthesis found
						if (RaiseErrors & !closingerror)
						{
							Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
					}
				}
				else
				{
					// no indices found before the space
					if (i < Expression.Text.Length - 1 && Expression.Text[i + 1] == '(')
					{
						// opening parenthesis follows the space
						int j = Expression.Text.IndexOf(')', i + 1);
						if (j > i + 1)
						{
							// closing parenthesis found
							if (j == Expression.Text.Length - 1)
							{
								// only closing parenthesis found at the end of the expression
								Command = Expression.Text.Substring(0, i).TrimEnd();
								ArgumentSequence = Expression.Text.Substring(i + 2, j - i - 2).Trim();
							}
							else
							{
								// detect border between indices and arguments
								bool found = false;
								Command = null; ArgumentSequence = null;
								for (int k = j + 1; k < Expression.Text.Length; k++)
								{
									if (char.IsWhiteSpace(Expression.Text[k]))
									{
										Command = Expression.Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Expression.Text.Substring(k + 1).TrimStart();
										found = true; break;
									}
									else if (Expression.Text[k] == '(')
									{
										Command = Expression.Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Expression.Text.Substring(k).TrimStart();
										found = true; break;
									}
								}
								if (!found)
								{
									if (RaiseErrors & !openingerror & !closingerror)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid syntax encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										openingerror = true;
										closingerror = true;
									}
									Command = Expression.Text;
									ArgumentSequence = "";
								}
								if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")"))
								{
									// arguments are enclosed by parentheses
									ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
								}
								else if (ArgumentSequence.StartsWith("("))
								{
									// only opening parenthesis found
									if (RaiseErrors & !closingerror)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
								}
							}
						}
						else
						{
							// no closing parenthesis found
							if (RaiseErrors & !closingerror)
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							Command = Expression.Text.Substring(0, i).TrimEnd();
							ArgumentSequence = Expression.Text.Substring(i + 2).TrimStart();
						}
					}
					else
					{
						// no index possible
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
						if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")"))
						{
							// arguments are enclosed by parentheses
							ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
						}
						else if (ArgumentSequence.StartsWith("("))
						{
							// only opening parenthesis found
							if (RaiseErrors & !closingerror)
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
						}
					}
				}
			}
			else
			{
				// no single space found
				if (Expression.Text.EndsWith(")"))
				{
					i = Expression.Text.LastIndexOf('(');
					if (i >= 0)
					{
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1, Expression.Text.Length - i - 2).Trim();
					}
					else
					{
						Command = Expression.Text;
						ArgumentSequence = "";
					}
				}
				else
				{
					i = Expression.Text.IndexOf('(');
					if (i >= 0)
					{
						if (RaiseErrors & !closingerror)
						{
							Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						Command = Expression.Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
					}
					else
					{
						if (RaiseErrors)
						{
							i = Expression.Text.IndexOf(')');
							if (i >= 0 & !closingerror)
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}
						Command = Expression.Text;
						ArgumentSequence = "";
					}
				}
			}
			// invalid trailing characters
			if (Command.EndsWith(","))
			{
				if (RaiseErrors)
				{
					Interface.AddMessage(Interface.MessageType.Error, false, "Invalid trailing comma encountered in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
				}
				while (Command.EndsWith(","))
				{
					Command = Command.Substring(0, Command.Length - 1);
				}
			}
		}

		private static void ApplyRouteData(string FileName, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly)
		{
			string SignalPath, LimitPath, LimitGraphicsPath, TransponderPath;
			ObjectManager.StaticObject SignalPost, LimitPostStraight, LimitPostLeft, LimitPostRight, LimitPostInfinite;
			ObjectManager.StaticObject LimitOneDigit, LimitTwoDigits, LimitThreeDigits, StopPost;
			ObjectManager.StaticObject TransponderS, TransponderSN, TransponderFalseStart, TransponderPOrigin, TransponderPStop;
			if (!PreviewOnly)
			{
				string CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
				// load compatibility objects
				SignalPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				SignalPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalPath, "signal_post.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Limits");
				LimitGraphicsPath = OpenBveApi.Path.CombineDirectory(LimitPath, "Graphics");
				LimitPostStraight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_straight.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostLeft = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_left.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostRight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_right.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostInfinite = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_infinite.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitOneDigit = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_1_digit.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitTwoDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_2_digits.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitThreeDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_3_digits.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				StopPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(CompatibilityFolder, "stop.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Transponders");
				TransponderS = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "s.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderSN = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "sn.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderFalseStart = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "falsestart.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPOrigin = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "porigin.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPStop = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "pstop.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false, false);
			}
			else
			{
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
			if (!PreviewOnly)
			{
				int z = 0;
				for (int i = 0; i < Data.Blocks.Length; i++)
				{
					if (!double.IsNaN(Data.Blocks[i].Height))
					{
						for (int j = i - 1; j >= 0; j--)
						{
							if (!double.IsNaN(Data.Blocks[j].Height))
							{
								double a = Data.Blocks[j].Height;
								double b = Data.Blocks[i].Height;
								double d = (b - a) / (double)(i - j);
								for (int k = j + 1; k < i; k++)
								{
									a += d;
									Data.Blocks[k].Height = a;
								}
								break;
							}
						}
						z = i;
					}
				}
				for (int i = z + 1; i < Data.Blocks.Length; i++)
				{
					Data.Blocks[i].Height = Data.Blocks[z].Height;
				}
			}
			// background
			if (!PreviewOnly)
			{
				BackgroundManager.CurrentBackground = new BackgroundManager.StaticBackground(null, 6, false);
				BackgroundManager.TargetBackground = BackgroundManager.CurrentBackground;
			}
			// brightness
			int CurrentBrightnessElement = -1;
			int CurrentBrightnessEvent = -1;
			float CurrentBrightnessValue = 1.0f;
			double CurrentBrightnessTrackPosition = (double)Data.FirstUsedBlock * Data.BlockInterval;
			if (!PreviewOnly)
			{
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
				{
					if (Data.Blocks[i].Brightness != null && Data.Blocks[i].Brightness.Length != 0)
					{
						CurrentBrightnessValue = Data.Blocks[i].Brightness[0].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[0].Value;
						break;
					}
				}
			}
			// create objects and track
			Vector3 Position = new Vector3(0.0, 0.0, 0.0);
			Vector2 Direction = new Vector2(0.0, 1.0);
			TrackManager.CurrentTrack = new TrackManager.Track { Elements = new TrackManager.TrackElement[] { } };
			double CurrentSpeedLimit = double.PositiveInfinity;
			int CurrentRunIndex = 0;
			int CurrentFlangeIndex = 0;
			if (Data.FirstUsedBlock < 0) Data.FirstUsedBlock = 0;
			TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[256];
			int CurrentTrackLength = 0;
			int PreviousFogElement = -1;
			int PreviousFogEvent = -1;
			Game.Fog PreviousFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, new Color24(128, 128, 128), -Data.BlockInterval);
			Game.Fog CurrentFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, new Color24(128, 128, 128), 0.0);
			// process blocks
			double progressFactor = Data.Blocks.Length - Data.FirstUsedBlock == 0 ? 0.5 : 0.5 / (double)(Data.Blocks.Length - Data.FirstUsedBlock);
			for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
			{
				Loading.RouteProgress = 0.6667 + (double)(i - Data.FirstUsedBlock) * progressFactor;
				if ((i & 15) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				double StartingDistance = (double)i * Data.BlockInterval;
				double EndingDistance = StartingDistance + Data.BlockInterval;
				// normalize
				World.Normalize(ref Direction.X, ref Direction.Y);
				// track
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Cycle.Length == 1 && Data.Blocks[i].Cycle[0] == -1)
					{
						if (Data.Structure.Cycle.Length == 0 || Data.Structure.Cycle[0] == null)
						{
							Data.Blocks[i].Cycle = new int[] { 0 };
						}
						else
						{
							Data.Blocks[i].Cycle = Data.Structure.Cycle[0];
						}
					}
				}
				TrackManager.TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
				int n = CurrentTrackLength;
				if (n >= TrackManager.CurrentTrack.Elements.Length)
				{
					Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, TrackManager.CurrentTrack.Elements.Length << 1);
				}
				CurrentTrackLength++;
				TrackManager.CurrentTrack.Elements[n] = WorldTrackElement;
				TrackManager.CurrentTrack.Elements[n].WorldPosition = Position;
				TrackManager.CurrentTrack.Elements[n].WorldDirection = Vector3.GetVector3(Direction, Data.Blocks[i].Pitch);
				TrackManager.CurrentTrack.Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
				World.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection.X, TrackManager.CurrentTrack.Elements[n].WorldDirection.Y, TrackManager.CurrentTrack.Elements[n].WorldDirection.Z, TrackManager.CurrentTrack.Elements[n].WorldSide.X, TrackManager.CurrentTrack.Elements[n].WorldSide.Y, TrackManager.CurrentTrack.Elements[n].WorldSide.Z, out TrackManager.CurrentTrack.Elements[n].WorldUp.X, out TrackManager.CurrentTrack.Elements[n].WorldUp.Y, out TrackManager.CurrentTrack.Elements[n].WorldUp.Z);
				TrackManager.CurrentTrack.Elements[n].StartingTrackPosition = StartingDistance;
				TrackManager.CurrentTrack.Elements[n].Events = new TrackManager.GeneralEvent[] { };
				TrackManager.CurrentTrack.Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
				TrackManager.CurrentTrack.Elements[n].CsvRwAccuracyLevel = Data.Blocks[i].Accuracy;
				// background
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Background >= 0)
					{
						int typ;
						if (i == Data.FirstUsedBlock)
						{
							typ = Data.Blocks[i].Background;
						}
						else
						{
							typ = Data.Backgrounds.Length > 0 ? 0 : -1;
							for (int j = i - 1; j >= Data.FirstUsedBlock; j--)
							{
								if (Data.Blocks[j].Background >= 0)
								{
									typ = Data.Blocks[j].Background;
									break;
								}
							}
						}
						if (typ >= 0 & typ < Data.Backgrounds.Length)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BackgroundChangeEvent(0.0, Data.Backgrounds[typ].Handle, Data.Backgrounds[Data.Blocks[i].Background].Handle);
						}
					}
				}
				// brightness
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++)
					{
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						double d = Data.Blocks[i].Brightness[j].TrackPosition - StartingDistance;
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BrightnessChangeEvent(d, Data.Blocks[i].Brightness[j].Value, CurrentBrightnessValue, Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition, Data.Blocks[i].Brightness[j].Value, 0.0);
						if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
						{
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
				if (!PreviewOnly)
				{
					if (Data.FogTransitionMode)
					{
						if (Data.Blocks[i].FogDefined)
						{
							Data.Blocks[i].Fog.TrackPosition = StartingDistance;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog);
							if (PreviousFogElement >= 0 & PreviousFogEvent >= 0)
							{
								TrackManager.FogChangeEvent e = (TrackManager.FogChangeEvent)TrackManager.CurrentTrack.Elements[PreviousFogElement].Events[PreviousFogEvent];
								e.NextFog = Data.Blocks[i].Fog;
							}
							else
							{
								Game.PreviousFog = PreviousFog;
								Game.CurrentFog = PreviousFog;
								Game.NextFog = Data.Blocks[i].Fog;
							}
							PreviousFog = Data.Blocks[i].Fog;
							PreviousFogElement = n;
							PreviousFogEvent = m;
						}
					}
					else
					{
						Data.Blocks[i].Fog.TrackPosition = StartingDistance + Data.BlockInterval;
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, CurrentFog, Data.Blocks[i].Fog);
						PreviousFog = CurrentFog;
						CurrentFog = Data.Blocks[i].Fog;
					}
				}
				// rail sounds
				if (!PreviewOnly)
				{
					for (int k = 0; k < Data.Blocks[i].RunSounds.Length; k++)
					{
						//Get & check our variables
						int r = Data.Blocks[i].RunSounds[k].RunSoundIndex;
						int f = Data.Blocks[i].RunSounds[k].FlangeSoundIndex;
						if (r != CurrentRunIndex || f != CurrentFlangeIndex)
						{
							//If either of these differ, we first need to resize the array for the current block
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].RunSounds[k].TrackPosition - StartingDistance;
							if (d > 0.0)
							{
								d = 0.0;
							}
							//Add event
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.RailSoundsChangeEvent(d, CurrentRunIndex,CurrentFlangeIndex, r, f);
							CurrentRunIndex = r;
							CurrentFlangeIndex = f;
						}
					}
				}
				// point sound
				if (!PreviewOnly)
				{
					if (i < Data.Blocks.Length - 1)
					{
						if (Data.Blocks[i].JointNoise)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(0.0, null, false, false, true, new Vector3(0.0, 0.0, 0.0), 12.5);
						}
					}
				}
				// station
				if (Data.Blocks[i].Station >= 0)
				{
					// station
					int s = Data.Blocks[i].Station;
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.StationStartEvent(0.0, s);
					double dx, dy = 3.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
					// passalarm
					if (!PreviewOnly)
					{
						if (Data.Blocks[i].StationPassAlarm)
						{
							int b = i - 6;
							if (b >= 0)
							{
								int j = b - Data.FirstUsedBlock;
								if (j >= 0)
								{
									m = TrackManager.CurrentTrack.Elements[j].Events.Length;
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[j].Events, m + 1);
									TrackManager.CurrentTrack.Elements[j].Events[m] = new TrackManager.StationPassAlarmEvent(0.0);
								}
							}
						}
					}
				}
				// stop
				for (int j = 0; j < Data.Blocks[i].Stop.Length; j++)
				{
					int s = Data.Blocks[i].Stop[j].Station;
					int t = Game.Stations[s].Stops.Length;
					Array.Resize<Game.StationStop>(ref Game.Stations[s].Stops, t + 1);
					Game.Stations[s].Stops[t].TrackPosition = Data.Blocks[i].Stop[j].TrackPosition;
					Game.Stations[s].Stops[t].ForwardTolerance = Data.Blocks[i].Stop[j].ForwardTolerance;
					Game.Stations[s].Stops[t].BackwardTolerance = Data.Blocks[i].Stop[j].BackwardTolerance;
					Game.Stations[s].Stops[t].Cars = Data.Blocks[i].Stop[j].Cars;
					double dx, dy = 2.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
				}
				// limit
				for (int j = 0; j < Data.Blocks[i].Limit.Length; j++)
				{
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					double d = Data.Blocks[i].Limit[j].TrackPosition - StartingDistance;
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.LimitChangeEvent(d, CurrentSpeedLimit, Data.Blocks[i].Limit[j].Speed);
					CurrentSpeedLimit = Data.Blocks[i].Limit[j].Speed;
				}
				// turn
				if (Data.Blocks[i].Turn != 0.0)
				{
					double ag = -Math.Atan(Data.Blocks[i].Turn);
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					World.Rotate(ref Direction, cosag, sinag);
					World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldDirection, cosag, sinag);
					World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldSide, cosag, sinag);
					World.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection.X, TrackManager.CurrentTrack.Elements[n].WorldDirection.Y, TrackManager.CurrentTrack.Elements[n].WorldDirection.Z, TrackManager.CurrentTrack.Elements[n].WorldSide.X, TrackManager.CurrentTrack.Elements[n].WorldSide.Y, TrackManager.CurrentTrack.Elements[n].WorldSide.Z, out TrackManager.CurrentTrack.Elements[n].WorldUp.X, out TrackManager.CurrentTrack.Elements[n].WorldUp.Y, out TrackManager.CurrentTrack.Elements[n].WorldUp.Z);
				}
				//Pitch
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
				if (WorldTrackElement.CurveRadius != 0.0 & Data.Blocks[i].Pitch != 0.0)
				{
					double d = Data.BlockInterval;
					double p = Data.Blocks[i].Pitch;
					double r = WorldTrackElement.CurveRadius;
					double s = d / Math.Sqrt(1.0 + p * p);
					h = s * p;
					double b = s / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
				}
				else if (WorldTrackElement.CurveRadius != 0.0)
				{
					double d = Data.BlockInterval;
					double r = WorldTrackElement.CurveRadius;
					double b = d / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
				}
				else if (Data.Blocks[i].Pitch != 0.0)
				{
					double p = Data.Blocks[i].Pitch;
					double d = Data.BlockInterval;
					c = d / Math.Sqrt(1.0 + p * p);
					h = c * p;
				}
				double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
				double TrackPitch = Math.Atan(Data.Blocks[i].Pitch);
				World.Transformation GroundTransformation = new World.Transformation(TrackYaw, 0.0, 0.0);
				World.Transformation TrackTransformation = new World.Transformation(TrackYaw, TrackPitch, 0.0);
				World.Transformation NullTransformation = new World.Transformation(0.0, 0.0, 0.0);
				//Add repeating objects into arrays
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Repeaters == null || Data.Blocks[i].Repeaters.Length == 0)
					{
						continue;
					}
					for (int j = 0; j < Data.Blocks[i].Repeaters.Length; j++)
					{
						if (Data.Blocks[i].Repeaters[j].Name == null)
						{
							continue;
						}
						switch (Data.Blocks[i].Repeaters[j].Type)
						{
							case 0:
								int sttype = Data.Blocks[i].Repeaters[j].StructureTypes[0];
								double d = Data.Blocks[i].Repeaters[j].TrackPosition - StartingDistance;
								double dx = 0;
								double dy = 0;
								Vector3 wpos = Position + new Vector3(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx);
								double tpos = Data.Blocks[i].Repeaters[j].TrackPosition;
								ObjectManager.CreateObject(Data.Structure.Objects[sttype], wpos, GroundTransformation, new World.Transformation(0, 0, 0), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);	
								break;
							case 1:
								//The repeater follows the gradient of it's attached rail, or Rail0 if not specified, so we must add it to the rail's object array
								if (Data.Blocks[i].Repeaters[j].RepetitionInterval != 0.0 && Data.Blocks[i].Repeaters[j].RepetitionInterval != Data.BlockInterval)
								{
									int idx = Data.Blocks[i].Repeaters[j].RailIndex;
									if (idx >= Data.Blocks[i].RailFreeObj.Length)
									{
										Array.Resize<Object[]>(ref Data.Blocks[i].RailFreeObj, idx + 1);
									}
									double nextRepetition = Data.Blocks[i].Repeaters[j].TrackPosition;
									//If the repetition interval is not 0.0 then this may repeat within a block
									if (i > 0)
									{
										for (int k = 0; k < Data.Blocks[i - 1].Repeaters.Length; k++)
										{
											if (Data.Blocks[i - 1].Repeaters[k].Name == Data.Blocks[i].Repeaters[j].Name)
											{
												//We've found our repeater in the last block, so we must add the repetition interval
												nextRepetition = Data.Blocks[i - 1].Repeaters[k].TrackPosition +
												                 Data.Blocks[i - 1].Repeaters[k].RepetitionInterval;

												int ol = 0;
												if (Data.Blocks[i].RailFreeObj[idx] == null)
												{
													Data.Blocks[i].RailFreeObj[idx] = new Object[1];
													ol = 0;
												}
												else
												{
													ol = Data.Blocks[i].RailFreeObj[idx].Length;
													Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
												}
												Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = nextRepetition;
												Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
												Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
												Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
												Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
												Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
												Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
												Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll;
											}
										}
									}
									while (nextRepetition < Data.Blocks[i].Repeaters[j].TrackPosition + Data.BlockInterval)
										{
											int ol = 0;
											if (Data.Blocks[i].RailFreeObj[idx] == null)
											{
												Data.Blocks[i].RailFreeObj[idx] = new Object[1];
												ol = 0;
											}
											else
											{
												ol = Data.Blocks[i].RailFreeObj[idx].Length;
												Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
											}
											nextRepetition += Data.Blocks[i].Repeaters[j].RepetitionInterval;
											Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = nextRepetition;
											Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
											Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
											Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
											Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
											Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
											Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
											Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll;
										}
										Data.Blocks[i].Repeaters[j].TrackPosition = nextRepetition;
									
								}
								else
								{
									int idx = Data.Blocks[i].Repeaters[j].RailIndex;
									if (idx >= Data.Blocks[i].RailFreeObj.Length)
									{
										Array.Resize<Object[]>(ref Data.Blocks[i].RailFreeObj, idx + 1);
									}
									int ol = 0;
									if (Data.Blocks[i].RailFreeObj[idx] == null)
									{
										Data.Blocks[i].RailFreeObj[idx] = new Object[1];
										ol = 0;
									}
									else
									{
										ol = Data.Blocks[i].RailFreeObj[idx].Length;
										Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
									}
									Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = Data.Blocks[i].Repeaters[j].TrackPosition;
									Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
									Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
									Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
									Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
									Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
									Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
									Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll;
								}
								break;
							case 3:
								//The repeater follows the gradient & cant of it's attached rail, or Rail0 if not specified, so we must add it to the rail's object array
								double CantAngle = Math.Tan((Math.Atan(Data.Blocks[i].CurrentTrackState.CurveCant)));
								if (Data.Blocks[i].Repeaters[j].RepetitionInterval != 0.0)
								{
									int idx = Data.Blocks[i].Repeaters[j].RailIndex;
									if (idx >= Data.Blocks[i].RailFreeObj.Length)
									{
										Array.Resize<Object[]>(ref Data.Blocks[i].RailFreeObj, idx + 1);
									}
									double nextRepetition = Data.Blocks[i].Repeaters[j].TrackPosition;
									
									//If the repetition interval is not 0.0 then this may repeat within a block
									if (i > 0)
									{
										for (int k = 0; k < Data.Blocks[i - 1].Repeaters.Length; k++)
										{
											if (Data.Blocks[i - 1].Repeaters[k].Name == Data.Blocks[i].Repeaters[j].Name)
											{
												//We've found our repeater in the last block, so we must add the repetition interval
												nextRepetition = Data.Blocks[i - 1].Repeaters[k].TrackPosition +
																 Data.Blocks[i - 1].Repeaters[k].RepetitionInterval;

												int ol = 0;
												if (Data.Blocks[i].RailFreeObj[idx] == null)
												{
													Data.Blocks[i].RailFreeObj[idx] = new Object[1];
													ol = 0;
												}
												else
												{
													ol = Data.Blocks[i].RailFreeObj[idx].Length;
													Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
												}
												Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = nextRepetition;
												Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
												Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
												Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
												Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
												Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
												Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
												Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll + CantAngle;
											}
										}
									}
									while (nextRepetition < Data.Blocks[i].Repeaters[j].TrackPosition + Data.BlockInterval)
									{
										int ol = 0;
										if (Data.Blocks[i].RailFreeObj[idx] == null)
										{
											Data.Blocks[i].RailFreeObj[idx] = new Object[1];
											ol = 0;
										}
										else
										{
											ol = Data.Blocks[i].RailFreeObj[idx].Length;
											Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
										}
										nextRepetition += Data.Blocks[i].Repeaters[j].RepetitionInterval;
										Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = nextRepetition;
										Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
										Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
										Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
										Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
										Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
										Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
										Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll + CantAngle;
									}
									Data.Blocks[i].Repeaters[j].TrackPosition = nextRepetition;

								}
								else
								{
									int idx = Data.Blocks[i].Repeaters[j].RailIndex;
									if (idx >= Data.Blocks[i].RailFreeObj.Length)
									{
										Array.Resize<Object[]>(ref Data.Blocks[i].RailFreeObj, idx + 1);
									}
									int ol = 0;
									if (Data.Blocks[i].RailFreeObj[idx] == null)
									{
										Data.Blocks[i].RailFreeObj[idx] = new Object[1];
										ol = 0;
									}
									else
									{
										ol = Data.Blocks[i].RailFreeObj[idx].Length;
										Array.Resize<Object>(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
									}
									Data.Blocks[i].RailFreeObj[idx][ol].TrackPosition = Data.Blocks[i].Repeaters[j].TrackPosition;
									Data.Blocks[i].RailFreeObj[idx][ol].Type = Data.Blocks[i].Repeaters[j].StructureTypes[0];
									Data.Blocks[i].RailFreeObj[idx][ol].X = Data.Blocks[i].Repeaters[j].X;
									Data.Blocks[i].RailFreeObj[idx][ol].Y = Data.Blocks[i].Repeaters[j].Y;
									Data.Blocks[i].RailFreeObj[idx][ol].Z = Data.Blocks[i].Repeaters[j].Z;
									Data.Blocks[i].RailFreeObj[idx][ol].Yaw = Data.Blocks[i].Repeaters[j].Yaw;
									Data.Blocks[i].RailFreeObj[idx][ol].Pitch = Data.Blocks[i].Repeaters[j].Pitch;
									Data.Blocks[i].RailFreeObj[idx][ol].Roll = Data.Blocks[i].Repeaters[j].Roll + CantAngle;
								}
								break;
						}
					}
				}
				// ground-aligned free objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].GroundFreeObj.Length; j++)
					{
						int sttype = Data.Blocks[i].GroundFreeObj[j].Type;
						double d = Data.Blocks[i].GroundFreeObj[j].TrackPosition - StartingDistance + Data.Blocks[i].GroundFreeObj[j].Z;
						double dx = Data.Blocks[i].GroundFreeObj[j].X;
						double dy = Data.Blocks[i].GroundFreeObj[j].Y;
						Vector3 wpos = Position + new Vector3(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx);
						double tpos = Data.Blocks[i].GroundFreeObj[j].TrackPosition;
						ObjectManager.CreateObject(Data.Structure.Objects[sttype], wpos, GroundTransformation, new World.Transformation(Data.Blocks[i].GroundFreeObj[j].Yaw, Data.Blocks[i].GroundFreeObj[j].Pitch, Data.Blocks[i].GroundFreeObj[j].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
					}
				}
				// rail-aligned objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Rail.Length; j++)
					{
						if (j > 0 && !Data.Blocks[i].Rail[j].RailStart) continue;
						// rail
						Vector3 pos;
						World.Transformation RailTransformation;
						double planar, updown;
						if (j == 0)
						{
							// rail 0
							planar = 0.0;
							updown = 0.0;
							RailTransformation = new World.Transformation(TrackTransformation, planar, updown, 0.0);
							pos = Position;
						}
						else
						{
							// rails 1-infinity
							double x = Data.Blocks[i].Rail[j].RailStartX;
							double y = Data.Blocks[i].Rail[j].RailStartY;
							Vector3 offset = new Vector3(Direction.Y * x, y, -Direction.X * x);
							pos = Position + offset;
							double dh;
							if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j)
							{
								// take orientation of upcoming block into account
								Vector2 Direction2 = Direction;
								Vector3 Position2 = Position;
								Position2.X += Direction.X * c;
								Position2.Y += h;
								Position2.Z += Direction.Y * c;
								if (a != 0.0)
								{
									World.Rotate(ref Direction2, Math.Cos(-a), Math.Sin(-a));
								}
								if (Data.Blocks[i + 1].Turn != 0.0)
								{
									double ag = -Math.Atan(Data.Blocks[i + 1].Turn);
									double cosag = Math.Cos(ag);
									double sinag = Math.Sin(ag);
									World.Rotate(ref Direction2, cosag, sinag);
								}
								double a2 = 0.0;
								double c2 = Data.BlockInterval;
								double h2 = 0.0;
								if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0 & Data.Blocks[i + 1].Pitch != 0.0)
								{
									double d2 = Data.BlockInterval;
									double p2 = Data.Blocks[i + 1].Pitch;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double s2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									h2 = s2 * p2;
									double b2 = s2 / Math.Abs(r2);
									c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									World.Rotate(ref Direction2, Math.Cos(-a2), Math.Sin(-a2));
								}
								else if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0)
								{
									double d2 = Data.BlockInterval;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double b2 = d2 / Math.Abs(r2);
									c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									World.Rotate(ref Direction2, Math.Cos(-a2), Math.Sin(-a2));
								}
								else if (Data.Blocks[i + 1].Pitch != 0.0)
								{
									double p2 = Data.Blocks[i + 1].Pitch;
									double d2 = Data.BlockInterval;
									c2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									h2 = c2 * p2;
								}
								double x2 = Data.Blocks[i + 1].Rail[j].RailEndX;
								double y2 = Data.Blocks[i + 1].Rail[j].RailEndY;
								Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);
								Vector3 pos2 = Position2 + offset2;
								double rx = pos2.X - pos.X;
								double ry = pos2.Y - pos.Y;
								double rz = pos2.Z - pos.Z;
								World.Normalize(ref rx, ref ry, ref rz);
								RailTransformation.Z = new Vector3(rx, ry, rz);
								RailTransformation.X = new Vector3(rz, 0.0, -rx);
								World.Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
								RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
								double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
								double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
								planar = Math.Atan(dx / c);
								dh = dy / c;
								updown = Math.Atan(dh);
							}
							else
							{
								planar = 0.0;
								dh = 0.0;
								updown = 0.0;
								RailTransformation = new World.Transformation(TrackTransformation, 0.0, 0.0, 0.0);
							}
						}
						// cracks
						for (int k = 0; k < Data.Blocks[i].Crack.Length; k++)
						{
							if (Data.Blocks[i].Crack[k].PrimaryRail == j)
							{
								int p = Data.Blocks[i].Crack[k].PrimaryRail;
								double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
								double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
								int s = Data.Blocks[i].Crack[k].SecondaryRail;
								if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart)
								{
									Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
								}
								else
								{
									double sx0 = Data.Blocks[i].Rail[s].RailStartX;
									double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
									double d0 = sx0 - px0;
									double d1 = sx1 - px1;
									if (d0 < 0.0)
									{
										if (Data.Blocks[i].Crack[k].Type >= Data.Structure.Objects.Length || Data.Structure.Objects[Data.Blocks[i].Crack[k].Type] == null)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "CrackStructureIndex references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.Objects[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
									else if (d0 > 0.0)
									{
										if (Data.Blocks[i].Crack[k].Type >= Data.Structure.Objects.Length || Data.Structure.Objects[Data.Blocks[i].Crack[k].Type] == null)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "CrackStructureIndex references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.Objects[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
							}
						}
						// free objects
						if (Data.Blocks[i].RailFreeObj.Length > j && Data.Blocks[i].RailFreeObj[j] != null)
						{
							for (int k = 0; k < Data.Blocks[i].RailFreeObj[j].Length; k++)
							{
								int sttype = Data.Blocks[i].RailFreeObj[j][k].Type;
								if (sttype != -1)
								{
									double dx = Data.Blocks[i].RailFreeObj[j][k].X;
									double dy = Data.Blocks[i].RailFreeObj[j][k].Y;
									double dz = Data.Blocks[i].RailFreeObj[j][k].TrackPosition - StartingDistance + Data.Blocks[i].RailFreeObj[j][k].Z;

									Vector3 wpos = pos;
									wpos.X += dx*RailTransformation.X.X + dy*RailTransformation.Y.X + dz*RailTransformation.Z.X;
									wpos.Y += dx*RailTransformation.X.Y + dy*RailTransformation.Y.Y + dz*RailTransformation.Z.Y;
									wpos.Z += dx*RailTransformation.X.Z + dy*RailTransformation.Y.Z + dz*RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].RailFreeObj[j][k].TrackPosition;
									ObjectManager.CreateObject(Data.Structure.Objects[sttype], wpos, RailTransformation,
										new World.Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch,
											Data.Blocks[i].RailFreeObj[j][k].Roll), -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance,
										Data.BlockInterval, tpos, 1.0, false);
								}
							}
						}
						// sections/signals/transponders
						if (j == 0)
						{
							// signals
							for (int k = 0; k < Data.Blocks[i].Signal.Length; k++)
							{
								SignalData sd;
								if (Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex >= 0)
								{
									sd = Data.CompatibilitySignalData[Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex];
								}
								else
								{
									sd = Data.SignalData[Data.Blocks[i].Signal[k].SignalObjectIndex];
								}
								// objects
								double dz = Data.Blocks[i].Signal[k].TrackPosition - StartingDistance;
								if (Data.Blocks[i].Signal[k].ShowPost)
								{
									// post
									double dx = Data.Blocks[i].Signal[k].X;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(SignalPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
								}
								if (Data.Blocks[i].Signal[k].ShowObject)
								{
									// signal object
									double dx = Data.Blocks[i].Signal[k].X;
									double dy = Data.Blocks[i].Signal[k].Y;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									
									if (sd is CompatibilitySignalData)
									{
										CompatibilitySignalData csd = (CompatibilitySignalData)sd;
										if (csd.Numbers.Length != 0)
										{
											double brightness = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
											ObjectManager.AnimatedObjectCollection aoc = new ObjectManager.AnimatedObjectCollection();
											aoc.Objects = new ObjectManager.AnimatedObject[1];
											aoc.Objects[0] = new ObjectManager.AnimatedObject();
											aoc.Objects[0].States = new ObjectManager.AnimatedObjectState[csd.Numbers.Length];
											for (int l = 0; l < csd.Numbers.Length; l++)
											{
												aoc.Objects[0].States[l].Object = ObjectManager.CloneObject(csd.Objects[l]);
											}
											string expr = "";
											for (int l = 0; l < csd.Numbers.Length - 1; l++)
											{
												expr += "section " + csd.Numbers[l].ToString(Culture) + " <= " + l.ToString(Culture) + " ";
											}
											expr += (csd.Numbers.Length - 1).ToString(Culture);
											for (int l = 0; l < csd.Numbers.Length - 1; l++)
											{
												expr += " ?";
											}
											aoc.Objects[0].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(expr);
											aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Program.RandomNumberGenerator.NextDouble();
											ObjectManager.CreateObject(aoc, wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, brightness, false);
										}
									}

								}
							}
							// sections
							for (int k = 0; k < Data.Blocks[i].Section.Length; k++)
							{
								int m = Game.Sections.Length;
								Array.Resize<Game.Section>(ref Game.Sections, m + 1);
								Game.Sections[m].SignalIndices = new int[] { };

								// create section
								Game.Sections[m].TrackPosition = Data.Blocks[i].Section[k].TrackPosition;
								Game.Sections[m].Aspects = new Game.SectionAspect[Data.Blocks[i].Section[k].Aspects.Length];
								for (int l = 0; l < Data.Blocks[i].Section[k].Aspects.Length; l++)
								{
									Game.Sections[m].Aspects[l].Number = Data.Blocks[i].Section[k].Aspects[l];
									if (Data.Blocks[i].Section[k].Aspects[l] >= 0 & Data.Blocks[i].Section[k].Aspects[l] < Data.SignalSpeeds.Length)
									{
										Game.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Data.Blocks[i].Section[k].Aspects[l]];
									}
									else
									{
										Game.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
									}
								}
								Game.Sections[m].Type = Data.Blocks[i].Section[k].Type;
								Game.Sections[m].CurrentAspect = -1;
								if (m > 0)
								{
									Game.Sections[m].PreviousSection = m - 1;
									Game.Sections[m - 1].NextSection = m;
								}
								else
								{
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
							
						}
						// limit
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Limit.Length; k++)
							{
								if (Data.Blocks[i].Limit[k].Direction != 0)
								{
									double dx = 2.2 * (double)Data.Blocks[i].Limit[k].Direction;
									double dz = Data.Blocks[i].Limit[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Limit[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									if (Data.Blocks[i].Limit[k].Speed <= 0.0 | Data.Blocks[i].Limit[k].Speed >= 1000.0)
									{
										ObjectManager.CreateStaticObject(LimitPostInfinite, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
									}
									else
									{
										if (Data.Blocks[i].Limit[k].Cource < 0)
										{
											ObjectManager.CreateStaticObject(LimitPostLeft, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										else if (Data.Blocks[i].Limit[k].Cource > 0)
										{
											ObjectManager.CreateStaticObject(LimitPostRight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										else
										{
											ObjectManager.CreateStaticObject(LimitPostStraight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										double lim = Data.Blocks[i].Limit[k].Speed / Data.UnitOfSpeed;
										if (lim < 10.0)
										{
											int d0 = (int)Math.Round(lim);
											int o = ObjectManager.CreateStaticObject(LimitOneDigit, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
										}
										else if (lim < 100.0)
										{
											int d1 = (int)Math.Round(lim);
											int d0 = d1 % 10;
											d1 /= 10;
											int o = ObjectManager.CreateStaticObject(LimitTwoDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[1].DaytimeTexture);
											}
										}
										else
										{
											int d2 = (int)Math.Round(lim);
											int d0 = d2 % 10;
											int d1 = (d2 / 10) % 10;
											d2 /= 100;
											int o = ObjectManager.CreateStaticObject(LimitThreeDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d2 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[1].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 3)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[2].DaytimeTexture);
											}
										}
									}
								}
							}
						}
						/*
						// stop
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Stop.Length; k++)
							{
								if (Data.Blocks[i].Stop[k].Direction != 0)
								{
									double dx = 1.8 * (double)Data.Blocks[i].Stop[k].Direction;
									double dz = Data.Blocks[i].Stop[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Stop[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(StopPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
								}
							}
						}
						 */
					}
				}
				// finalize block
				Position.X += Direction.X * c;
				Position.Y += h;
				Position.Z += Direction.Y * c;
				if (a != 0.0)
				{
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
				}
			}
			/*
			// orphaned transponders
			if (!PreviewOnly)
			{
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
				{
					for (int j = 0; j < Data.Blocks[i].Transponder.Length; j++)
					{
						if (Data.Blocks[i].Transponder[j].Type != -1)
						{
							int n = i - Data.FirstUsedBlock;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Transponder[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
							int s = Data.Blocks[i].Transponder[j].Section;
							if (s >= 0) s = -1;
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(d, Data.Blocks[i].Transponder[j].Type, Data.Blocks[i].Transponder[j].Data, s, Data.Blocks[i].Transponder[j].ClipToFirstRedSection);
							Data.Blocks[i].Transponder[j].Type = -1;
						}
					}
				}
			}
			 */
			// insert station end events
			for (int i = 0; i < Game.Stations.Length; i++)
			{
				int j = Game.Stations[i].Stops.Length - 1;
				if (j >= 0)
				{
					double p = Game.Stations[i].Stops[j].TrackPosition + Game.Stations[i].Stops[j].ForwardTolerance + Data.BlockInterval;
					int k = (int)Math.Floor(p / (double)Data.BlockInterval) - Data.FirstUsedBlock;
					if (k >= 0 & k < Data.Blocks.Length)
					{
						double d = p - (double)(k + Data.FirstUsedBlock) * (double)Data.BlockInterval;
						int m = TrackManager.CurrentTrack.Elements[k].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[k].Events, m + 1);
						TrackManager.CurrentTrack.Elements[k].Events[m] = new TrackManager.StationEndEvent(d, i);
					}
				}
			}
			// create default point of interests
			if (Game.PointsOfInterest.Length == 0)
			{
				Game.PointsOfInterest = new OpenBve.Game.PointOfInterest[Game.Stations.Length];
				int n = 0;
				for (int i = 0; i < Game.Stations.Length; i++)
				{
					if (Game.Stations[i].Stops.Length != 0)
					{
						Game.PointsOfInterest[n].Text = Game.Stations[i].Name;
						Game.PointsOfInterest[n].TrackPosition = Game.Stations[i].Stops[0].TrackPosition;
						Game.PointsOfInterest[n].TrackOffset = new Vector3(0.0, 2.8, 0.0);
						if (Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors)
						{
							Game.PointsOfInterest[n].TrackOffset.X = -2.5;
						}
						else if (!Game.Stations[i].OpenLeftDoors & Game.Stations[i].OpenRightDoors)
						{
							Game.PointsOfInterest[n].TrackOffset.X = 2.5;
						}
						n++;
					}
				}
				Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, n);
			}
			// convert block-based cant into point-based cant
			for (int i = CurrentTrackLength - 1; i >= 1; i--)
			{
				if (TrackManager.CurrentTrack.Elements[i].CurveCant == 0.0)
				{
					TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
				}
				else if (TrackManager.CurrentTrack.Elements[i - 1].CurveCant != 0.0)
				{
					if (Math.Sign(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) == Math.Sign(TrackManager.CurrentTrack.Elements[i].CurveCant))
					{
						if (Math.Abs(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) > Math.Abs(TrackManager.CurrentTrack.Elements[i].CurveCant))
						{
							TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
						}
					}
					else
					{
						TrackManager.CurrentTrack.Elements[i].CurveCant = 0.5 * (TrackManager.CurrentTrack.Elements[i].CurveCant + TrackManager.CurrentTrack.Elements[i - 1].CurveCant);
					}
				}
			}
			// finalize
			Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, CurrentTrackLength);
			for (int i = 0; i < Game.Stations.Length; i++)
			{
				if (Game.Stations[i].Stops.Length == 0 & Game.Stations[i].StopMode != Game.StationStopMode.AllPass)
				{
					Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + Game.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					Game.Stations[i].StopMode = Game.StationStopMode.AllPass;
				}
				if (Game.Stations[i].StationType == Game.StationType.ChangeEnds)
				{
					if (i < Game.Stations.Length - 1)
					{
						if (Game.Stations[i + 1].StopMode != Game.StationStopMode.AllStop)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but the subsequent station does not expect all trains to stop in file " + FileName);
							Game.Stations[i + 1].StopMode = Game.StationStopMode.AllStop;
						}
					}
					else
					{
						Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but there is no subsequent station defined in file " + FileName);
						Game.Stations[i].StationType = Game.StationType.Terminal;
					}
				}
			}
			if (Game.Stations.Length != 0)
			{
				Game.Stations[Game.Stations.Length - 1].StationType = Game.StationType.Terminal;
			}
			if (TrackManager.CurrentTrack.Elements.Length != 0)
			{
				int n = TrackManager.CurrentTrack.Elements.Length - 1;
				int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
				Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
				TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TrackEndEvent(Data.BlockInterval);
			}
			// insert compatibility beacons
			if (!PreviewOnly)
			{
				List<TrackManager.TransponderEvent> transponders = new List<TrackManager.TransponderEvent>();
				bool atc = false;
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
				{
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
					{
						if (!atc)
						{
							if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
							{
								TrackManager.StationStartEvent station = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Atc)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 0, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 1, 0, false);
									atc = true;
								}
							}
						}
						else
						{
							if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
							{
								TrackManager.StationStartEvent station = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Ats)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 2, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 3, 0, false);
								}
							}
							else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent)
							{
								TrackManager.StationEndEvent station = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Atc)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 1, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 2, 0, false);
								}
								else if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Ats)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 3, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 0, 0, false);
									atc = false;
								}
							}
							else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent)
							{
								TrackManager.LimitChangeEvent limit = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								int speed = (int)Math.Round(Math.Min(4095.0, 3.6 * limit.NextSpeedLimit));
								int distance = Math.Min(1048575, (int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + limit.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcSpeedLimit, value, 0, false));
								}
							}
						}
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.TransponderEvent)
						{
							TrackManager.TransponderEvent transponder = TrackManager.CurrentTrack.Elements[i].Events[j] as TrackManager.TransponderEvent;
							if (transponder.Type == TrackManager.SpecialTransponderTypes.InternalAtsPTemporarySpeedLimit)
							{
								int speed = Math.Min(4095, transponder.Data);
								int distance = Math.Min(1048575, (int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + transponder.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtsPTemporarySpeedLimit, value, 0, false));
								}
							}
						}
					}
				}
				int n = TrackManager.CurrentTrack.Elements[0].Events.Length;
				Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[0].Events, n + transponders.Count);
				for (int i = 0; i < transponders.Count; i++)
				{
					TrackManager.CurrentTrack.Elements[0].Events[n + i] = transponders[i];
				}
			}
			// cant
			if (!PreviewOnly)
			{
				ComputeCantTangents();
				int subdivisions = (int)Math.Floor(Data.BlockInterval / 5.0);
				if (subdivisions >= 2)
				{
					SmoothenOutTurns(subdivisions);
					ComputeCantTangents();
				}
			}
		}

		private static void ComputeCantTangents()
		{
			if (TrackManager.CurrentTrack.Elements.Length == 1)
			{
				TrackManager.CurrentTrack.Elements[0].CurveCantTangent = 0.0;
			}
			else if (TrackManager.CurrentTrack.Elements.Length != 0)
			{
				double[] deltas = new double[TrackManager.CurrentTrack.Elements.Length - 1];
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					deltas[i] = TrackManager.CurrentTrack.Elements[i + 1].CurveCant - TrackManager.CurrentTrack.Elements[i].CurveCant;
				}
				double[] tangents = new double[TrackManager.CurrentTrack.Elements.Length];
				tangents[0] = deltas[0];
				tangents[TrackManager.CurrentTrack.Elements.Length - 1] = deltas[TrackManager.CurrentTrack.Elements.Length - 2];
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					tangents[i] = 0.5 * (deltas[i - 1] + deltas[i]);
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					if (deltas[i] == 0.0)
					{
						tangents[i] = 0.0;
						tangents[i + 1] = 0.0;
					}
					else
					{
						double a = tangents[i] / deltas[i];
						double b = tangents[i + 1] / deltas[i];
						if (a * a + b * b > 9.0)
						{
							double t = 3.0 / Math.Sqrt(a * a + b * b);
							tangents[i] = t * a * deltas[i];
							tangents[i + 1] = t * b * deltas[i];
						}
					}
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
				{
					TrackManager.CurrentTrack.Elements[i].CurveCantTangent = tangents[i];
				}
			}
		}

		// ------------------


		// smoothen out turns
		private static void SmoothenOutTurns(int subdivisions)
		{
			if (subdivisions < 2)
			{
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
			for (int i = 0; i < newLength; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
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
			for (int i = length - 1; i >= 1; i--)
			{
				TrackManager.CurrentTrack.Elements[subdivisions * i] = TrackManager.CurrentTrack.Elements[i];
			}
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
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
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					int m = i % subdivisions;
					if (m == 0)
					{
						double p = 0.00000001 * TrackManager.CurrentTrack.Elements[i - 1].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
						TrackManager.UpdateTrackFollower(ref follower, p, true, false);
						Vector3 d1 = TrackManager.CurrentTrack.Elements[i].WorldDirection;
						Vector3 d2 = follower.WorldDirection;
						Vector3 d = d1 - d2;
						double t = d.X * d.X + d.Z * d.Z;
						const double e = 0.0001;
						if (t > e)
						{
							isTurn[i] = true;
						}
					}
				}
			}
			// replace turns by curves
			double totalShortage = 0.0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				if (isTurn[i])
				{
					// estimate radius
					Vector3 AP = TrackManager.CurrentTrack.Elements[i - 1].WorldPosition;
					Vector3 AS = TrackManager.CurrentTrack.Elements[i - 1].WorldSide;
					Vector3 BP = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition;
					Vector3 BS = TrackManager.CurrentTrack.Elements[i + 1].WorldSide;
					Vector3 S = AS - BS;
					double rx;
					if (S.X * S.X > 0.000001)
					{
						rx = (BP.X - AP.X) / S.X;
					}
					else
					{
						rx = 0.0;
					}
					double rz;
					if (S.Z * S.Z > 0.000001)
					{
						rz = (BP.Z - AP.Z) / S.Z;
					}
					else
					{
						rz = 0.0;
					}
					if (rx != 0.0 | rz != 0.0)
					{
						double r;
						if (rx != 0.0 & rz != 0.0)
						{
							if (Math.Sign(rx) == Math.Sign(rz))
							{
								double f = rx / rz;
								if (f > -1.1 & f < -0.9 | f > 0.9 & f < 1.1)
								{
									r = Math.Sqrt(Math.Abs(rx * rz)) * Math.Sign(rx);
								}
								else
								{
									r = 0.0;
								}
							}
							else
							{
								r = 0.0;
							}
						}
						else if (rx != 0.0)
						{
							r = rx;
						}
						else
						{
							r = rz;
						}
						if (r * r > 1.0)
						{
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
							Vector3 d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
							double bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
							int bestJ = 0;
							int n = 1000;
							double a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
							for (int j = 1; j < n - 1; j++)
							{
								TrackManager.UpdateTrackFollower(ref follower, TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
								if (t < bestT)
								{
									bestT = t;
									bestJ = j;
								}
								else
								{
									break;
								}
							}
							double s = (double)bestJ * a;
							for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++)
							{
								TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
							}
							totalShortage += s;
							// introduce turn to compensate for curve
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							Vector3 AB = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
							Vector3 AC = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							Vector3 BC = follower.WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double sa = Math.Sqrt(BC.X * BC.X + BC.Z * BC.Z);
							double sb = Math.Sqrt(AC.X * AC.X + AC.Z * AC.Z);
							double sc = Math.Sqrt(AB.X * AB.X + AB.Z * AB.Z);
							double denominator = 2.0 * sa * sb;
							if (denominator != 0.0)
							{
								double originalAngle;
								{
									double value = (sa * sa + sb * sb - sc * sc) / denominator;
									if (value < -1.0)
									{
										originalAngle = Math.PI;
									}
									else if (value > 1.0)
									{
										originalAngle = 0;
									}
									else
									{
										originalAngle = Math.Acos(value);
									}
								}
								TrackManager.TrackElement originalTrackElement = TrackManager.CurrentTrack.Elements[i];
								bestT = double.MaxValue;
								bestJ = 0;
								for (int j = -1; j <= 1; j++)
								{
									double g = (double)j * originalAngle;
									double cosg = Math.Cos(g);
									double sing = Math.Sin(g);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection.X, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Y, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Z, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp.X, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Y, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Z, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldSide.X, ref TrackManager.CurrentTrack.Elements[i].WorldSide.Y, ref TrackManager.CurrentTrack.Elements[i].WorldSide.Z, 0.0, 1.0, 0.0, cosg, sing);
									p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
									TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
									TrackManager.UpdateTrackFollower(ref follower, p, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
								}
								{
									double newAngle = (double)bestJ * originalAngle;
									double cosg = Math.Cos(newAngle);
									double sing = Math.Sin(newAngle);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection.X, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Y, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Z, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp.X, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Y, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Z, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldSide.X, ref TrackManager.CurrentTrack.Elements[i].WorldSide.Y, ref TrackManager.CurrentTrack.Elements[i].WorldSide.Z, 0.0, 1.0, 0.0, cosg, sing);
								}
								// iterate again to further shorten track element length
								p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
								TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
								TrackManager.UpdateTrackFollower(ref follower, p, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
								bestJ = 0;
								n = 1000;
								a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
								for (int j = 1; j < n - 1; j++)
								{
									TrackManager.UpdateTrackFollower(ref follower, TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
									else
									{
										break;
									}
								}
								s = (double)bestJ * a;
								for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++)
								{
									TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
								}
								totalShortage += s;
							}
							// compensate for height difference
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							TrackManager.UpdateTrackFollower(ref follower, p - 1.0, true, false);
							TrackManager.UpdateTrackFollower(ref follower, p, true, false);
							Vector3 d1 = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a1 = Math.Atan(d1.Y / Math.Sqrt(d1.X * d1.X + d1.Z * d1.Z));
							Vector3 d2 = follower.WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a2 = Math.Atan(d2.Y / Math.Sqrt(d2.X * d2.X + d2.Z * d2.Z));
							double b = a2 - a1;
							if (b * b > 0.00000001)
							{
								double cosa = Math.Cos(b);
								double sina = Math.Sin(b);
								World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection.X, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Y, ref TrackManager.CurrentTrack.Elements[i].WorldDirection.Z, TrackManager.CurrentTrack.Elements[i].WorldSide.X, TrackManager.CurrentTrack.Elements[i].WorldSide.Y, TrackManager.CurrentTrack.Elements[i].WorldSide.Z, cosa, sina);
								World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp.X, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Y, ref TrackManager.CurrentTrack.Elements[i].WorldUp.Z, TrackManager.CurrentTrack.Elements[i].WorldSide.X, TrackManager.CurrentTrack.Elements[i].WorldSide.Y, TrackManager.CurrentTrack.Elements[i].WorldSide.Z, cosa, sina);
							}
						}
					}
				}
			}
			// correct events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
			{
				double startingTrackPosition = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double endingTrackPosition = TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
				{
					double p = startingTrackPosition + TrackManager.CurrentTrack.Elements[i].Events[j].TrackPositionDelta;
					if (p >= endingTrackPosition)
					{
						int len = TrackManager.CurrentTrack.Elements[i + 1].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i + 1].Events, len + 1);
						TrackManager.CurrentTrack.Elements[i + 1].Events[len] = TrackManager.CurrentTrack.Elements[i].Events[j];
						TrackManager.CurrentTrack.Elements[i + 1].Events[len].TrackPositionDelta += startingTrackPosition - endingTrackPosition;
						for (int k = j; k < TrackManager.CurrentTrack.Elements[i].Events.Length - 1; k++)
						{
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
