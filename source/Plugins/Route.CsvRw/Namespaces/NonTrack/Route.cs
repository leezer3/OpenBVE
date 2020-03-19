using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void ParseRouteCommand(string Command, string[] Arguments, int Index, string FileName, double[] UnitOfLength, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			switch (Command)
			{
				case "comment":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						CurrentRoute.Comment = Arguments[0];
					}

					break;
				case "image":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Image = f;
						}
					}

					break;
				case "timetable":
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "" + Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Program.CurrentRoute.Information.DefaultTimetableDescription = Arguments[0];
						}
					}

					break;
				case "change":
					if (!PreviewOnly)
					{
						int change = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out change))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							change = 0;
						}
						else if (change < -1 | change > 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Mode is expected to be -1, 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							change = 0;
						}

						Program.CurrentOptions.TrainStart = (TrainStartMode) change;
					}

					break;
				case "gauge":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double a;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInMillimeters is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
				case "signal":
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							double a;
							if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Index < 0)
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "AspectIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else if (a < 0.0)
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (Index >= Data.SignalSpeeds.Length)
									{
										int n = Data.SignalSpeeds.Length;
										Array.Resize<double>(ref Data.SignalSpeeds, Index + 1);
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
				case "accelerationduetogravity":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double a;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.AccelerationDueToGravity = a;
						}
					}

					break;
				//Sets the time the game will start at
				case "starttime":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double t;
						if (!TryParseTime(Arguments[0], out t))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Arguments[0] + " does not parse to a valid time in command " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Program.CurrentRoute.InitialStationTime == -1)
							{
								Program.CurrentRoute.InitialStationTime = t;
							}
						}
					}

					break;
				//Sets the route's loading screen texture
				case "loadingscreen":
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
							Program.Renderer.Loading.SetLoadingBkg(f);
					}

					break;
				//Sets a custom unit of speed to to displayed in on-screen messages
				case "displayspeed":
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length == 1 && Arguments[0].IndexOf(',') != -1)
					{
						Arguments = Arguments[0].Split(new char[] { ',' });
					}
					if (Arguments.Length != 2)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have two arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}
					Program.CurrentOptions.UnitOfSpeed = Arguments[0];
					if (!double.TryParse(Arguments[1], NumberStyles.Float, Culture, out Program.CurrentOptions.SpeedConversionFactor))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false,"Speed conversion factor is invalid in " + Command + " at line " +Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) +" in file " + Expression.File);
						Program.CurrentOptions.UnitOfSpeed = "km/h";
					}
					break;
				//Sets the route's briefing data
				case "briefing":
					if (PreviewOnly)
					{
						return;
					}

					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(f))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Program.CurrentRoute.Information.RouteBriefing = f;
						}
					}

					break;
				case "elevation":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double a;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Height is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialElevation = a;
						}
					}

					break;
				case "temperature":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double a;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInCelsius is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= -273.15)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInCelsius is expected to be greater than to -273.15 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialAirTemperature = a + 273.15;
						}
					}

					break;
				case "pressure":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						double a;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInKPa is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (a <= 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInKPa is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							CurrentRoute.Atmosphere.InitialAirPressure = 1000.0 * a;
						}
					}

					break;
				case "ambientlight":
				{
					if (Program.Renderer.Lighting.DynamicLighting == true)
					{
						Program.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.AmbientLight will be ignored");
						break;
					}

					int r = 255, g = 255, b = 255;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (r < 0 | r > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						r = r < 0 ? 0 : 255;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (g < 0 | g > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						g = g < 0 ? 0 : 255;
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (b < 0 | b > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						b = b < 0 ? 0 : 255;
					}

					Program.Renderer.Lighting.OptionAmbientColor = new Color24((byte) r, (byte) g, (byte) b);
				}
					break;
				case "directionallight":
				{
					if (Program.Renderer.Lighting.DynamicLighting == true)
					{
						Program.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.DirectionalLight will be ignored");
						break;
					}

					int r = 255, g = 255, b = 255;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (r < 0 | r > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						r = r < 0 ? 0 : 255;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (g < 0 | g > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						g = g < 0 ? 0 : 255;
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else if (b < 0 | b > 255)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						b = b < 0 ? 0 : 255;
					}

					Program.Renderer.Lighting.OptionDiffuseColor = new Color24((byte) r, (byte) g, (byte) b);
				}
					break;
				case "lightdirection":
				{
					if (Program.Renderer.Lighting.DynamicLighting == true)
					{
						Program.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Route.LightDirection will be ignored");
						break;
					}

					double theta = 60.0, phi = -26.565051177078;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out theta))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Theta is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out phi))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Phi is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}

					theta = theta.ToRadians();
					phi = phi.ToRadians();
					double dx = Math.Cos(theta) * Math.Sin(phi);
					double dy = -Math.Sin(theta);
					double dz = Math.Cos(theta) * Math.Cos(phi);
					Program.Renderer.Lighting.OptionLightPosition = new Vector3((float) -dx, (float) -dy, (float) -dz);
				}
					break;
				case "dynamiclight":
					//Read the lighting XML file
					string path = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
					if (System.IO.File.Exists(path))
					{
						if (DynamicLightParser.ReadLightingXML(path))
						{
							Program.Renderer.Lighting.DynamicLighting = true;
						}
						else
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "The file " + path + " is not a valid dynamic lighting XML file, at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
					}
					else
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Dynamic lighting XML file not found at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}

					break;
				case "initialviewpoint":
					if (Arguments.Length < 1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int cv;
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
									Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									break;
							}
						}

						if (cv >= 0 && cv < 4)
						{
							Program.CurrentOptions.InitialViewpoint = cv;
						}
						else
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is invalid at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
					}

					break;
				case "tfoxml":
					if (!PreviewOnly)
					{
						string tfoFile = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
						if (!System.IO.File.Exists(tfoFile))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, true, "TrackFollowingObject XML file " + tfoFile + " not found in Track.TfoXML at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						int n = CurrentRoute.TrackFollowingObjects.Length;
						Array.Resize(ref CurrentRoute.TrackFollowingObjects, n + 1);
						CurrentRoute.TrackFollowingObjects[n] = Program.CurrentHost.ParseTrackFollowingObject(tfoFile, ObjectPath);
					}

					break;
			}
		}
	}
}
