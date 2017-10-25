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
			internal double HorizontalRadius;
			internal double VerticalRadius;
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
			internal RailTransformationTypes BaseTransformation;
		}

		/// <summary>Defines the different types of base transform which may be applied to an object
		/// 
		/// NOTE:
		/// Rotation is applied separately to the result of the base transform
		/// </summary>
		private enum RailTransformationTypes
		{
			/// <summary>Flat within the world</summary>
			Flat = 0,
			/// <summary>Follows the pitch of it's rail</summary>
			FollowsPitch = 1,
			/// <summary>Follows the cant of it's rail</summary>
			FollowsCant = 2,
			/// <summary>Follows both the pitch and cant of it's rail</summary>
			FollowsBoth = 3,
			/// <summary>Follows the pitch and cant of rail 0</summary>
			FollowsFirstRail = 4

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
			internal bool BeginInterpolation = false;
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
			PreprocessSplitIntoExpressions(FileName, false, Lines, out Expressions, 0.0);
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
							PreprocessSplitIntoExpressions(fn, false, Lines, out IncludedExpressions, 0.0);
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
								key = key.RemoveEnclosingQuotes().ToLowerInvariant();
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
								double Radius;
								switch (type)
								{
									case "position":
										//This moves the track directly, using no interpolation (e.g. as per prior versions of BVETS)
										SecondaryTrack(key, Arguments, ref Data, BlockIndex, UnitOfLength);
										break;
									case "y.interpolate":
										//Moves the track in the Y axis, using an interpolated curve
										//If no radius is supplied, uses the radius for this track in the previous segment
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out Distance))
										{
											break;
										}
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius))
										{
											break;
										}
										InterpolateSecondaryTrack(key, Distance, ref Data, BlockIndex, UnitOfLength, true);
										break;
									case "x.interpolate":
										//Moves the track in the Y axis, using an interpolated curve
										//If no radius is supplied, uses the radius for this track in the previous segment
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out Distance))
										{
											break;
										}
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out Radius))
										{
											break;
										}
										InterpolateSecondaryTrack(key, Distance, ref Data, BlockIndex, UnitOfLength, false);
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
							if (!PreviewOnly)
							{
								Interface.AddMessage(Interface.MessageType.Warning, false, command);
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

		private static void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, out Expression[] Expressions, double trackPositionOffset)
		{
			Expressions = new Expression[4096];
			int e = 0;
			
			// parse
			for (int i = 0; i < Lines.Length; i++)
			{
				int cm = Lines[i].IndexOf('#');
				if (cm != -1)
				{
					Lines[i] = Lines[i].Substring(0, cm);
				}
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
					follower.Update(-1.0, true, false);
					follower.Update(p, true, false);
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
						follower.Update(p, true, false);
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
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							TrackManager.CurrentTrack.Elements[i].CurveRadius = r;
							//TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i].CurveCant;
							//TrackManager.CurrentTrack.Elements[i].CurveCantInterpolation = TrackManager.CurrentTrack.Elements[i].CurveCantInterpolation;
							TrackManager.CurrentTrack.Elements[i].WorldPosition = follower.WorldPosition;
							TrackManager.CurrentTrack.Elements[i].WorldDirection = follower.WorldDirection;
							TrackManager.CurrentTrack.Elements[i].WorldUp = follower.WorldUp;
							TrackManager.CurrentTrack.Elements[i].WorldSide = follower.WorldSide;
							// iterate to shorten track element length
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							Vector3 d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
							double bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
							int bestJ = 0;
							int n = 1000;
							double a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
							for (int j = 1; j < n - 1; j++)
							{
								follower.Update(TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
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
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
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
									follower.Update(p - 1.0, true, false);
									follower.Update(p, true, false);
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
								follower.Update(p - 1.0, true, false);
								follower.Update(p, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
								bestJ = 0;
								n = 1000;
								a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
								for (int j = 1; j < n - 1; j++)
								{
									follower.Update(TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
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
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
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
