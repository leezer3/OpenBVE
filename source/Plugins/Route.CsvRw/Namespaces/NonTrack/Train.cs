using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseTrainCommand(TrainCommand Command, string[] Arguments, int Index, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case TrainCommand.Interval:
				{
					if (!PreviewOnly)
					{
						List<double> intervals = new List<double>();
						for (int k = 0; k < Arguments.Length; k++)
						{
							if (!NumberFormats.TryParseDoubleVb6(Arguments[k], out double o))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								continue;
							}

							if (o == 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " must be non-zero in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								continue;
							}

							if (Math.Abs(o) > 43200 && Plugin.CurrentOptions.EnableBveTsHacks)
							{
								//Southern Blighton- Treston park has a runinterval of well over 24 hours, and there are likely others
								//Discard this
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Interval " + k.ToString(Culture) + " is greater than 12 hours in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								continue;
							}

							if (Math.Abs(o) < 120 && Plugin.CurrentOptions.EnableBveTsHacks)
							{
								/*
								 * An AI train follows the same schedule / rules as the player train
								 * ==>
								 * x Waiting time before departure at the first station (30s to 1min is 'normal')
								 * x Time to accelerate to linespeed
								 * x Time to clear (as a minimum) the protecting signal on station exit
								 *
								 * When the runinterval is below ~2minutes, on large numbers of routes, this
								 * shows up as a train overlapping the player train (bad....)
								 */
								o = Math.Abs(o) != o ? -120 : 120;
							}

							intervals.Add(o);
						}

						intervals.Sort();
						if (intervals.Count > 0)
						{
							CurrentRoute.PrecedingTrainTimeDeltas = intervals.ToArray();
						}
					}
				}
					break;
				case TrainCommand.Velocity:
				{
					if (!PreviewOnly)
					{
						double limit = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out limit))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in Train.Velocity at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							limit = 0.0;
						}

						Plugin.CurrentOptions.PrecedingTrainSpeedLimit = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
					}
				}
					break;
				case TrainCommand.Folder:
				case TrainCommand.File:
				{
					if (Plugin.CurrentOptions.EnableBveTsHacks && Arguments.Length > 0)
					{
						// BVE seems to allow a relative path to the train from the routefile...
						if (Arguments[0].StartsWith(@"..\..\..\BVE\Train\", StringComparison.InvariantCultureIgnoreCase))
						{
							Arguments[0] = Arguments[0].Substring(19);
						}

						if (Arguments[0].StartsWith(@"..\..\mackoy\BVE4\Train\"))
						{
							Arguments[0] = Arguments[0].Substring(24);
						}
					}
					if (PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FolderName contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Plugin.CurrentOptions.TrainName = Arguments[0];
							}
						}
					}
				}
					break;
				case TrainCommand.DownloadLocation:
				{
					if (Arguments.Length > 0)
					{
						if (Arguments[0].StartsWith("www."))
						{
							Arguments[0] = "http://" + Arguments[0];
						}
						if (Uri.TryCreate(Arguments[0], UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp  || uriResult.Scheme == Uri.UriSchemeHttps) && uriResult.Host.Replace("www.", "").Split('.').Length > 1 && uriResult.HostNameType == UriHostNameType.Dns && uriResult.Host.Length > uriResult.Host.LastIndexOf(".", StringComparison.InvariantCulture) + 1 && 100 >= Arguments[0].Length)
						{
								// Why not save to the log...
								Plugin.FileSystem.AppendToLogFile("INFO: Route default train download location " + Arguments[0]);
								Plugin.CurrentOptions.TrainDownloadLocation = Arguments[0];
						}
					}
				}
					break;
				case TrainCommand.Run:
				case TrainCommand.Rail:
				{
					if (!PreviewOnly)
					{
						if (Index < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailTypeIndex is out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int val = 0;
							if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out val))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RunSoundIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								val = 0;
							}

							if (val < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RunSoundIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								val = 0;
							}

							if (Index >= Data.Structure.Run.Length)
							{
								Array.Resize(ref Data.Structure.Run, Index + 1);
							}

							Data.Structure.Run[Index] = val;
						}
					}
				}
					break;
				case TrainCommand.Flange:
				{
					if (!PreviewOnly)
					{
						if (Index < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailTypeIndex is out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int val = 0;
							if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out val))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FlangeSoundIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								val = 0;
							}

							if (val < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FlangeSoundIndex expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								val = 0;
							}

							if (Index >= Data.Structure.Flange.Length)
							{
								Array.Resize(ref Data.Structure.Flange, Index + 1);
							}

							Data.Structure.Flange[Index] = val;
						}
					}
				}
					break;
				case TrainCommand.TimetableDay:
				{
					if (!PreviewOnly)
					{
						if (Index < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								while (Index >= Data.TimetableDaytime.Length)
								{
									int n = Data.TimetableDaytime.Length;
									Array.Resize(ref Data.TimetableDaytime, n << 1);
									for (int i = n; i < Data.TimetableDaytime.Length; i++)
									{
										Data.TimetableDaytime[i] = null;
									}
								}

								string f = string.Empty;
								if (!string.IsNullOrEmpty(TrainPath))
								{
									f = Path.CombineFile(TrainPath, Arguments[0]);
								}
									
								if (!System.IO.File.Exists(f))
								{
									f = Path.CombineFile(ObjectPath, Arguments[0]);
								}

								if (System.IO.File.Exists(f))
								{
									Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out Data.TimetableDaytime[Index]);
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DaytimeTimetable " + Index + " with FileName " + Arguments[0] + " was not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
							}
						}
					}
				}
					break;
				case TrainCommand.TimetableNight:
				{
					if (!PreviewOnly)
					{
						if (Index < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								while (Index >= Data.TimetableNighttime.Length)
								{
									int n = Data.TimetableNighttime.Length;
									Array.Resize(ref Data.TimetableNighttime, n << 1);
									for (int i = n; i < Data.TimetableNighttime.Length; i++)
									{
										Data.TimetableNighttime[i] = null;
									}
								}

								string f = string.Empty;
								if(!string.IsNullOrEmpty(TrainPath))
								{
									f = Path.CombineFile(TrainPath, Arguments[0]);
								}

								if (!System.IO.File.Exists(f))
								{
									f = Path.CombineFile(ObjectPath, Arguments[0]);
								}

								if (System.IO.File.Exists(f))
								{
									Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out Data.TimetableNighttime[Index]);
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DaytimeTimetable " + Index + " with FileName " + Arguments[0] + " was not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
							}
						}
					}
				}
					break;
				case TrainCommand.Destination:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[0], out Plugin.CurrentOptions.InitialDestination))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Destination is expected to be an Integer in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}
					}
				}
					break;
			}
		}
	}
}
