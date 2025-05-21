using System;
using System.Globalization;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi.Trains;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseRouteCommand(RouteCommand Command, string[] Arguments, int Index, string FileName, double[] UnitOfLength, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case RouteCommand.DeveloperID:
					//Unused by OpenBVE
					break;
				case RouteCommand.Comment:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						CurrentRoute.Comment = Arguments[0];
					}

					break;
				case RouteCommand.Image:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Image = f;
						}
					}

					break;
				case RouteCommand.TimeTable:
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "" + Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Information.DefaultTimetableDescription = Arguments[0];
						}
					}

					break;
				case RouteCommand.Change:
					if (!PreviewOnly)
					{
						int change = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out change))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							change = 0;
						}
						else if (change < -1 | change > 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be -1, 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							change = 0;
						}

						Plugin.CurrentOptions.TrainStart = (TrainStartMode) change;
					}

					break;
				case RouteCommand.Gauge:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out double a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInMillimeters is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							for (int tt = 0; tt < CurrentRoute.Tracks.Count; tt++)
							{
								int t = CurrentRoute.Tracks.ElementAt(tt).Key;
								CurrentRoute.Tracks[t].RailGauge = 0.001 * a;
							}
						}
					}
					break;
				case RouteCommand.Signal:
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out double a))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Index < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AspectIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else if (a < 0.0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Index >= Data.SignalSpeeds.Length)
									{
										int n = Data.SignalSpeeds.Length;
										Array.Resize(ref Data.SignalSpeeds, Index + 1);
										for (int i = n; i < Index; i++)
										{
											Data.SignalSpeeds[i] = double.PositiveInfinity;
										}
									}

									Data.SignalSpeeds[Index] = a * Data.UnitOfSpeed;
								}
							}
						}
					}
					break;
				case RouteCommand.AccelerationDueToGravity:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out double a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.AccelerationDueToGravity = a;
						}
					}
					break;
				case RouteCommand.StartTime:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!TryParseTime(Arguments[0], out double t))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Arguments[0] + " does not parse to a valid time in command " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (CurrentRoute.InitialStationTime == -1)
							{
								CurrentRoute.InitialStationTime = t;
							}
						}
					}
					break;
				case RouteCommand.LoadingScreen:
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Texture t = new Texture(f, TextureParameters.NoChange, Plugin.CurrentHost);
							CurrentRoute.Information.LoadingScreenBackground = t;
						}
					}

					break;
				case RouteCommand.DisplaySpeed:
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length == 1 && Arguments[0].IndexOf(',') != -1)
					{
						Arguments = Arguments[0].Split(',');
					}
					if (Arguments.Length != 2)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have two arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}
					Plugin.CurrentOptions.UnitOfSpeed = Arguments[0];
					if (!double.TryParse(Arguments[1], NumberStyles.Float, Culture, out Plugin.CurrentOptions.SpeedConversionFactor))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false,"Speed conversion factor is invalid in " + Command + " at line " +Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) +" in file " + Expression.File);
						Plugin.CurrentOptions.UnitOfSpeed = "km/h";
					}
					break;
				case RouteCommand.Briefing:
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Information.RouteBriefing = f;
						}
					}
					break;
				case RouteCommand.Elevation:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out double a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialElevation = a;
						}
					}
					break;
				case RouteCommand.Temperature:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out double a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInCelsius is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= -273.15)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInCelsius is expected to be greater than -273.15 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a >= 100.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInCelsius is expected to be less than 100.0 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialAirTemperature = a + 273.15;
						}
					}
					break;
				case RouteCommand.Pressure:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out double a))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInKPa is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInKPa is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a >= 120.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInKPa is expected to be less than 120.0 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialAirPressure = 1000.0 * a;
						}
					}
					break;
				case RouteCommand.AmbientLight:
				{
					if (Plugin.CurrentRoute.DynamicLighting)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.AmbientLight will be ignored");
						break;
					}

					int r = 255, g = 255, b = 255;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					Plugin.CurrentRoute.Atmosphere.AmbientLightColor = new Color24((byte) r, (byte) g, (byte) b);
				}
					break;
				case RouteCommand.DirectionalLight:
				{
					if (Plugin.CurrentRoute.DynamicLighting)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.DirectionalLight will be ignored");
						break;
					}

					int r = 255, g = 255, b = 255;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					
					Plugin. CurrentRoute.Atmosphere.DiffuseLightColor = new Color24((byte) r, (byte) g, (byte) b);
				}
					break;
				case RouteCommand.LightDirection:
				{
					if (Plugin.CurrentRoute.DynamicLighting)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.LightDirection will be ignored");
						break;
					}

					double theta = 60.0, phi = -26.565051177078;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out theta))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Theta is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out phi))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Phi is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}

					theta = theta.ToRadians();
					phi = phi.ToRadians();
					double dx = Math.Cos(theta) * Math.Sin(phi);
					double dy = -Math.Sin(theta);
					double dz = Math.Cos(theta) * Math.Cos(phi);
					Plugin.CurrentRoute.Atmosphere.LightPosition = new Vector3((float) -dx, (float) -dy, (float) -dz);
				}
					break;
				case RouteCommand.DynamicLight:
					if (PreviewOnly)
					{
						break;
					}
					//Read the lighting XML file
					string path = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
					if (System.IO.File.Exists(path))
					{
						if (DynamicLightParser.ReadLightingXML(path, out Plugin.CurrentRoute.LightDefinitions))
						{
							Plugin.CurrentRoute.DynamicLighting = true;
							Data.Structure.LightDefinitions.Add(int.MaxValue, Plugin.CurrentRoute.LightDefinitions);
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The file " + path + " is not a valid dynamic lighting XML file, at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Dynamic lighting XML file not found at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					break;
				case RouteCommand.InitialViewPoint:
					if (Arguments.Length < 1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (!NumberFormats.TryParseIntVb6(Arguments[0], out int cv))
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									break;
							}
						}

						if (cv >= 0 && cv <= 4)
						{
							Plugin.CurrentOptions.InitialViewpoint = cv;
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
					}
					break;
				case RouteCommand.TfoXML:
					if (!PreviewOnly)
					{
						string tfoFile = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(tfoFile))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "TrackFollowingObject XML file " + tfoFile + " not found in Track.TfoXML at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}
						Data.ScriptedTrainFiles.Add(tfoFile);
					}
					break;
			}
		}
	}
}
