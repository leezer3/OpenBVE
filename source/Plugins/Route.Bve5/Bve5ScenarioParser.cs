using System;
using System.IO;
using System.Text;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.Stations;

namespace Bve5RouteParser
{
	internal partial class Parser
	{
		internal Plugin Plugin;
		internal CurrentRoute CurrentRoute;
		internal void ParseRoute(string FileName, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, bool PreviewOnly, Plugin plugin)
		{
			CurrentRoute = Plugin.CurrentRoute;
			Plugin = plugin;
			if (Encoding == null)
			{
				Encoding = Encoding.ASCII;
			}
			string[] Lines = File.ReadAllLines(FileName);
			// initialize data
			Plugin.CurrentOptions.UnitOfSpeed = "km/h";
			Plugin.CurrentOptions.SpeedConversionFactor = 0.0;
			//		    customLoadScreen = false;
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
			Data.Blocks[0].CurrentTrackState = new TrackElement(0.0);
			Data.ObjectList = new ObjectPointer[0];
			Data.StationList = new Station[0];
			Data.LastBrightness = 1.0f;
			Data.FogTransitionMode = true;
			if (!PreviewOnly)
			{
				Data.Blocks[0].Background = 0;
				Data.Blocks[0].Brightness = new Brightness[] { };
				Data.Blocks[0].Fog = new Fog(0,1, Color24.Grey, 0, false);
				Data.Blocks[0].Cycle = new int[] { -1 };
				Data.Blocks[0].Height = 0.0;
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
				Data.TimetableDaytime = new Texture[] { null, null, null, null };
				Data.TimetableNighttime = new Texture[] { null, null, null, null };
				Data.Structure.Objects = new UnifiedObject[] { };
				Data.UnitOfSpeed = 0.277777777777778;

				Data.CompatibilitySignalData = new CompatibilitySignalData[0];
				// game data
				CurrentRoute.Sections = new[]
				{
					new RouteManager2.SignalManager.Section(0, new[] { new SectionAspect(0, 0.0), new SectionAspect(4, double.PositiveInfinity) }, SectionType.IndexBased)
				};
				
				CurrentRoute.Sections[0].CurrentAspect = 0;
				CurrentRoute.Sections[0].StationIndex = -1;

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
						CurrentRoute.Comment = key[1].Trim();
						break;
					case "route":
						RouteFile = Path.GetDirectoryName(FileName) + "\\" + key[1].Trim();
						break;
					case "image":
						CurrentRoute.Image = Path.GetDirectoryName(FileName) + "\\" + key[1].Trim();
						break;
				}
			}
			ParseRouteForData(RouteFile, Encoding, TrainPath, ObjectPath, SoundPath, ref Data, PreviewOnly);
			if (Plugin.Cancel) return;
			ApplyRouteData(FileName, Encoding, ref Data, PreviewOnly);
		}

		private void ParseRouteForData(string FileName, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, ref RouteData Data, bool PreviewOnly)
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
			CurrentRoute.Stations = new RouteStation[] { };
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
							if (command.EndsWith(".load"))
							{
								//Handled elsewhere
								continue;
							}
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
								SetBackground(key, Arguments, ref Data, BlockIndex);
								continue;
							}
							if (command.StartsWith("adhesion.") && !PreviewOnly)
							{
								//Changes the current adhesion value
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								SetAdhesion(Arguments, ref Data, BlockIndex);
								continue;
							}
							if (command.StartsWith("irregularity.") && !PreviewOnly)
							{
								//Changes the current track accuracy values
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								SetAccuracy(Arguments, ref Data, BlockIndex);
								continue;
							}
							if (command.StartsWith("jointnoise.") && !PreviewOnly)
							{
								//Changes the current point sounds index
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
							if (command.StartsWith("fog.") && !PreviewOnly)
							{
								//Sets the current fog
								int ida = Commands[c].IndexOf('.');
								int idb = Commands[c].IndexOf('(');
								string key = Commands[c].Substring(ida + 1, idb - ida - 1).ToLowerInvariant();
								switch (key)
								{
									case "interpolate":
									case "set":
										ChangeFog(Arguments, ref Data, BlockIndex);
										break;
								}
								continue;
							}
							if (!PreviewOnly)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, command);
							}

						}
						continue;
					}
				}
			}
		}

	}
}
