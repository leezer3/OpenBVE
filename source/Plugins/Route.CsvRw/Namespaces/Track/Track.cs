using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.Events;
using RouteManager2.MessageManager.MessageTypes;
using RouteManager2.SignalManager;
using RouteManager2.Stations;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static int CurrentStation = -1;
		private static int CurrentStop = -1;
		private static int CurrentSection = 0;
		private static bool DepartureSignalUsed = false;
		private static void ParseTrackCommand(string Command, string[] Arguments, string FileName, double[] UnitOfLength, Expression Expression, ref RouteData Data, int BlockIndex, bool PreviewOnly)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			switch (Command)
			{
				case "railstart":
				case "rail":
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx < 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (string.Compare(Command, "railstart", StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (Data.Blocks[BlockIndex].Rails.ContainsKey(idx) && Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is required to reference a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}

						if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx))
						{
							Data.Blocks[BlockIndex].Rails.Add(idx, new Rail());

							if (idx >= Data.Blocks[BlockIndex].RailCycles.Length)
							{
								int ol = Data.Blocks[BlockIndex].RailCycles.Length;
								Array.Resize<RailCycle>(ref Data.Blocks[BlockIndex].RailCycles, idx + 1);
								for (int rc = ol; rc < Data.Blocks[BlockIndex].RailCycles.Length; rc++)
								{
									Data.Blocks[BlockIndex].RailCycles[rc].RailCycleIndex = -1;
								}
							}

						}

						Rail currentRail = Data.Blocks[BlockIndex].Rails[idx];
						if (currentRail.RailStartRefreshed)
						{
							currentRail.RailEnded = true;
						}

						currentRail.RailStarted = true;
						currentRail.RailStartRefreshed = true;
						if (Arguments.Length >= 2)
						{
							if (Arguments[1].Length > 0)
							{
								double x;
								if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									x = 0.0;
								}

								currentRail.RailStart.X = x;
							}

							if (!currentRail.RailEnded)
							{
								currentRail.RailEnd.X = currentRail.RailStart.X;
							}
						}

						if (Arguments.Length >= 3)
						{
							if (Arguments[2].Length > 0)
							{
								double y;
								if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									y = 0.0;
								}

								currentRail.RailStart.Y = y;
							}

							if (!currentRail.RailEnded)
							{
								currentRail.RailEnd.Y = currentRail.RailStart.Y;
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
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								sttype = 0;
							}

							if (sttype < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (!Data.Structure.RailObjects.ContainsKey(sttype))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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

						double cant = 0.0;
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out cant))
						{
							if (Arguments[4] != "id 0") //RouteBuilder inserts these, harmless so let's ignore
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							cant = 0.0;
						}
						else
						{
							cant *= 0.001;
						}

						currentRail.CurveCant = cant;
						Data.Blocks[BlockIndex].Rails[idx] = currentRail;
					}

					break;
				case "railend":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx < 0 || !Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx))
						{
							Data.Blocks[BlockIndex].Rails.Add(idx, new Rail());
						}

						Rail currentRail = Data.Blocks[BlockIndex].Rails[idx];
						currentRail.RailStarted = false;
						currentRail.RailStartRefreshed = false;
						currentRail.RailEnded = true;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							double x;
							if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							currentRail.RailEnd.X = x;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0)
						{
							double y;
							if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							currentRail.RailEnd.Y = y;
						}

						Data.Blocks[BlockIndex].Rails[idx] = currentRail;
					}
				}
					break;
				case "railtype":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						int sttype = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (idx < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (sttype < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (!Data.Structure.RailObjects.ContainsKey(sttype))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Blocks[BlockIndex].RailType.Length <= idx)
								{
									Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
									int ol = Data.Blocks[BlockIndex].RailCycles.Length;
									Array.Resize(ref Data.Blocks[BlockIndex].RailCycles, idx + 1);
									for (int rc = ol; rc < Data.Blocks[BlockIndex].RailCycles.Length; rc++)
									{
										Data.Blocks[BlockIndex].RailCycles[rc].RailCycleIndex = -1;
									}
								}

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
				}
					break;
				case "accuracy":
				{
					double r = 2.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out r))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						r = 2.0;
					}

					if (r < 0.0)
					{
						r = 0.0;
					}
					else if (r > 4.0)
					{
						r = 4.0;
					}

					Data.Blocks[BlockIndex].Accuracy = r;
				}
					break;
				case "pitch":
				{
					double p = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out p))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "ValueInPermille is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						p = 0.0;
					}

					Data.Blocks[BlockIndex].Pitch = 0.001 * p;
				}
					break;
				case "curve":
				{
					double radius = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						radius = 0.0;
					}

					double cant = 0.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out cant))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						cant = 0.0;
					}
					else
					{
						cant *= 0.001;
					}

					if (Data.SignedCant)
					{
						if (radius != 0.0)
						{
							cant *= (double) Math.Sign(radius);
						}
					}
					else
					{
						cant = Math.Abs(cant) * (double) Math.Sign(radius);
					}

					Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = radius;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = cant;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
				}
					break;
				case "turn":
				{
					double s = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out s))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Ratio is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						s = 0.0;
					}

					Data.Blocks[BlockIndex].Turn = s;
				}
					break;
				case "adhesion":
				{
					double a = 100.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100.0;
					}

					if (a < 0.0)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100.0;
					}

					Data.Blocks[BlockIndex].AdhesionMultiplier = 0.01 * a;
				}
					break;
				case "brightness":
				{
					if (!PreviewOnly)
					{
						float value = 255.0f;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[0], out value))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
				}
					break;
				case "fog":
				{
					if (!PreviewOnly)
					{
						double start = 0.0, end = 0.0;
						int r = 128, g = 128, b = 128;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out start))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "StartingDistance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							start = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out end))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "EndingDistance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							end = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out r))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							r = 128;
						}
						else if (r < 0 | r > 255)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							r = r < 0 ? 0 : 255;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out g))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							g = 128;
						}
						else if (g < 0 | g > 255)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							g = g < 0 ? 0 : 255;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out b))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							b = 128;
						}
						else if (b < 0 | b > 255)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							b = b < 0 ? 0 : 255;
						}

						if (start < end)
						{
							Data.Blocks[BlockIndex].Fog.Start = (float) start;
							Data.Blocks[BlockIndex].Fog.End = (float) end;
						}
						else
						{
							Data.Blocks[BlockIndex].Fog.Start = CurrentRoute.NoFogStart;
							Data.Blocks[BlockIndex].Fog.End = CurrentRoute.NoFogEnd;
						}

						Data.Blocks[BlockIndex].Fog.Color = new Color24((byte) r, (byte) g, (byte) b);
						Data.Blocks[BlockIndex].FogDefined = true;
					}
				}
					break;
				case "section":
				case "sections":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "At least one argument is required in " + Command + "at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
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
											Array.Resize(ref Arguments, i + 1);
											Array.Resize(ref aspects, i + 1);
											break;
										}

										pp++;
									}
								}
							}

							for (int i = 0; i < Arguments.Length; i++)
							{
								if (string.IsNullOrEmpty(Arguments[i]))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									aspects[i] = -1;
								}
								else if (!NumberFormats.TryParseIntVb6(Arguments[i], out aspects[i]))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									aspects[i] = -1;
								}
								else if (aspects[i] < 0)
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is expected to be non-negative in " + Command + "at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									aspects[i] = -1;
								}
							}

							bool valueBased = Data.ValueBasedSections | string.Equals(Command, "SectionS", StringComparison.OrdinalIgnoreCase);
							if (valueBased)
							{
								Array.Sort<int>(aspects);
							}

							int n = Data.Blocks[BlockIndex].Sections.Length;
							Array.Resize<Section>(ref Data.Blocks[BlockIndex].Sections, n + 1);
							Data.Blocks[BlockIndex].Sections[n].TrackPosition = Data.TrackPosition;
							Data.Blocks[BlockIndex].Sections[n].Aspects = aspects;
							Data.Blocks[BlockIndex].Sections[n].Type = valueBased ? SectionType.ValueBased : SectionType.IndexBased;
							Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = -1;
							if (CurrentStation >= 0 && CurrentRoute.Stations[CurrentStation].ForceStopSignal)
							{
								if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
								{
									Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = CurrentStation;
									DepartureSignalUsed = true;
								}
							}

							CurrentSection++;
						}
					}
				}
					break;
				case "sigf":
				{
					if (!PreviewOnly)
					{
						int objidx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out objidx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "SignalIndex is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							objidx = 0;
						}

						if (objidx >= 0 & Data.Signals.ContainsKey(objidx))
						{
							int section = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out section))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Section is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								section = 0;
							}

							double x = 0.0, y = 0.0;
							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							double yaw = 0.0, pitch = 0.0, roll = 0.0;
							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roll = 0.0;
							}

							int n = Data.Blocks[BlockIndex].Signals.Length;
							Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signals, n + 1);
							Data.Blocks[BlockIndex].Signals[n].TrackPosition = Data.TrackPosition;
							Data.Blocks[BlockIndex].Signals[n].SectionIndex = CurrentSection + section;
							Data.Blocks[BlockIndex].Signals[n].SignalCompatibilityObjectIndex = -1;
							Data.Blocks[BlockIndex].Signals[n].SignalObjectIndex = objidx;
							Data.Blocks[BlockIndex].Signals[n].Position.X = x;
							Data.Blocks[BlockIndex].Signals[n].Position.Y = y < 0.0 ? 4.8 : y;
							Data.Blocks[BlockIndex].Signals[n].Yaw = yaw.ToRadians();
							Data.Blocks[BlockIndex].Signals[n].Pitch = pitch.ToRadians();
							Data.Blocks[BlockIndex].Signals[n].Roll = roll.ToRadians();
							Data.Blocks[BlockIndex].Signals[n].ShowObject = true;
							Data.Blocks[BlockIndex].Signals[n].ShowPost = y < 0.0;
						}
						else
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "SignalIndex " + objidx + " references a signal object not loaded in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
					}
				}
					break;
				case "signal":
				case "sig":
				{
					if (!PreviewOnly)
					{
						int num = -2;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out num))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Aspects is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							num = -2;
						}

						if (num == 0 && IsRW == true)
						{
							//Aspects value of zero in RW routes produces a 2-aspect R/G signal
							num = -2;
						}

						if (num != 1 & num != -2 & num != 2 & num != -3 & num != 3 & num != -4 & num != 4 & num != -5 & num != 5 & num != 6)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Aspects has an unsupported value in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							num = num == -3 | num == -6 | num == -1 ? -num : -4;
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int[] aspects;
						int comp;
						switch (num)
						{
							case 1:
								aspects = new int[] {0, 2, 3};
								comp = 4;
								break;
							case 2:
								aspects = new int[] {0, 2};
								comp = 0;
								break;
							case -2:
								aspects = new int[] {0, 4};
								comp = 1;
								break;
							case -3:
								aspects = new int[] {0, 2, 4};
								comp = 2;
								break; //Undocumented, see https://github.com/leezer3/OpenBVE/issues/336
							case 3:
								aspects = new int[] {0, 2, 4};
								comp = 2;
								break;
							case 4:
								aspects = new int[] {0, 1, 2, 4};
								comp = 3;
								break;
							case -4:
								aspects = new int[] {0, 2, 3, 4};
								comp = 4;
								break;
							case 5:
								aspects = new int[] {0, 1, 2, 3, 4};
								comp = 5;
								break;
							case -5:
								aspects = new int[] {0, 2, 3, 4, 5};
								comp = 6;
								break;
							case 6:
								aspects = new int[] {0, 1, 2, 3, 4, 5};
								comp = 7;
								break;
							default:
								aspects = new int[] {0, 2};
								comp = 0;
								break;
						}

						int n = Data.Blocks[BlockIndex].Sections.Length;
						Array.Resize<Section>(ref Data.Blocks[BlockIndex].Sections, n + 1);
						Data.Blocks[BlockIndex].Sections[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].Sections[n].Aspects = aspects;
						Data.Blocks[BlockIndex].Sections[n].DepartureStationIndex = -1;
						Data.Blocks[BlockIndex].Sections[n].Invisible = x == 0.0;
						Data.Blocks[BlockIndex].Sections[n].Type = SectionType.ValueBased;
						if (CurrentStation >= 0 && CurrentRoute.Stations[CurrentStation].ForceStopSignal)
						{
							if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
							{
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
						Data.Blocks[BlockIndex].Signals[n].Position.X = x;
						Data.Blocks[BlockIndex].Signals[n].Position.Y = y < 0.0 ? 4.8 : y;
						Data.Blocks[BlockIndex].Signals[n].Yaw = yaw.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].Pitch = pitch.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].Roll = roll.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].ShowObject = x != 0.0;
						Data.Blocks[BlockIndex].Signals[n].ShowPost = x != 0.0 & y < 0.0;
					}
				}
					break;
				case "relay":
				{
					if (!PreviewOnly)
					{
						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out yaw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out pitch))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out roll))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Signals.Length;
						Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signals, n + 1);
						Data.Blocks[BlockIndex].Signals[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].Signals[n].SectionIndex = CurrentSection + 1;
						Data.Blocks[BlockIndex].Signals[n].SignalCompatibilityObjectIndex = 8;
						Data.Blocks[BlockIndex].Signals[n].SignalObjectIndex = -1;
						Data.Blocks[BlockIndex].Signals[n].Position.X = x;
						Data.Blocks[BlockIndex].Signals[n].Position.Y = y < 0.0 ? 4.8 : y;
						Data.Blocks[BlockIndex].Signals[n].Yaw = yaw.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].Pitch = pitch.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].Roll = roll.ToRadians();
						Data.Blocks[BlockIndex].Signals[n].ShowObject = x != 0.0;
						Data.Blocks[BlockIndex].Signals[n].ShowPost = x != 0.0 & y < 0.0;
					}
				}
					break;
				case "destination":
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (type < -1 || type > 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is expected to be in the range of -1 to 1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int structure = 0, nextDestination = 0, previousDestination = 0, triggerOnce = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = 0;
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out nextDestination))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "NextDestination is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								nextDestination = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out previousDestination))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "PreviousDestination is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								previousDestination = 0;
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out triggerOnce))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								previousDestination = 0;
							}

							if (structure < -1)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}
							else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}

							if (triggerOnce < 0 || triggerOnce > 1)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is expected to be in the range of 0 to 1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								triggerOnce = 0;
							}

							double x = 0.0, y = 0.0;
							double yaw = 0.0, pitch = 0.0, roll = 0.0;
							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out x))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], UnitOfLength, out y))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out yaw))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out pitch))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[9], out roll))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Data.Blocks[BlockIndex].DestinationChanges[n].Position.X = x;
							Data.Blocks[BlockIndex].DestinationChanges[n].Position.Y = y;
							Data.Blocks[BlockIndex].DestinationChanges[n].Yaw = yaw.ToRadians();
							Data.Blocks[BlockIndex].DestinationChanges[n].Pitch = pitch.ToRadians();
							Data.Blocks[BlockIndex].DestinationChanges[n].Roll = roll.ToRadians();
						}
					}
				}
					break;
				case "beacon":
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (type < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is expected to be non-negative in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int structure = 0, section = 0, optional = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = 0;
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out section))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Section is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								section = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out optional))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Data is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								optional = 0;
							}

							if (structure < -1)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}
							else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}

							if (section == -1)
							{
								//section = (int)TrackManager.TransponderSpecialSection.NextRedSection;
							}
							else if (section < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Section is expected to be non-negative or -1 in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								section = CurrentSection + 1;
							}
							else
							{
								section += CurrentSection;
							}

							double x = 0.0, y = 0.0;
							double yaw = 0.0, pitch = 0.0, roll = 0.0;
							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out x))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out y))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out yaw))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out pitch))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out roll))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Data.Blocks[BlockIndex].Transponders[n].Position.X = x;
							Data.Blocks[BlockIndex].Transponders[n].Position.Y = y;
							Data.Blocks[BlockIndex].Transponders[n].Yaw = yaw.ToRadians();
							Data.Blocks[BlockIndex].Transponders[n].Pitch = pitch.ToRadians();
							Data.Blocks[BlockIndex].Transponders[n].Roll = roll.ToRadians();
						}
					}
				}
					break;
				case "transponder":
				case "tr":
				{
					if (!PreviewOnly)
					{
						int type = 0, oversig = 0, work = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out oversig))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Signals is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							oversig = 0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out work))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "SwitchSystems is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							work = 0;
						}

						if (oversig < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Signals is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							oversig = 0;
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out x))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out y))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out pitch))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out roll))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].Transponders[n].Type = type;
						Data.Blocks[BlockIndex].Transponders[n].Data = work;
						Data.Blocks[BlockIndex].Transponders[n].ShowDefaultObject = true;
						Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
						Data.Blocks[BlockIndex].Transponders[n].Position.X = x;
						Data.Blocks[BlockIndex].Transponders[n].Position.Y = y;
						Data.Blocks[BlockIndex].Transponders[n].Yaw = yaw.ToRadians();
						Data.Blocks[BlockIndex].Transponders[n].Pitch = pitch.ToRadians();
						Data.Blocks[BlockIndex].Transponders[n].Roll = roll.ToRadians();
						Data.Blocks[BlockIndex].Transponders[n].SectionIndex = CurrentSection + oversig + 1;
						Data.Blocks[BlockIndex].Transponders[n].ClipToFirstRedSection = true;
					}
				}
					break;
				case "atssn":
				{
					if (!PreviewOnly)
					{
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
				}
					break;
				case "atsp":
				{
					if (!PreviewOnly)
					{
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
				}
					break;
				case "pattern":
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						double speed = 0.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							speed = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
						if (type == 0)
						{
							Data.Blocks[BlockIndex].Transponders[n].Type = (int) TransponderTypes.InternalAtsPTemporarySpeedLimit;
							Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int) Math.Round(speed * Data.UnitOfSpeed * 3.6);
						}
						else
						{
							Data.Blocks[BlockIndex].Transponders[n].Type = (int) TransponderTypes.AtsPPermanentSpeedLimit;
							Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int) Math.Round(speed * Data.UnitOfSpeed * 3.6);
						}

						Data.Blocks[BlockIndex].Transponders[n].SectionIndex = -1;
						Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
					}
				}
					break;
				case "plimit":
				{
					if (!PreviewOnly)
					{
						double speed = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out speed))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							speed = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].Transponders[n].Type = (int) TransponderTypes.AtsPPermanentSpeedLimit;
						Data.Blocks[BlockIndex].Transponders[n].Data = speed == 0.0 ? int.MaxValue : (int) Math.Round(speed * Data.UnitOfSpeed * 3.6);
						Data.Blocks[BlockIndex].Transponders[n].SectionIndex = -1;
						Data.Blocks[BlockIndex].Transponders[n].BeaconStructureIndex = -1;
					}
				}
					break;
				case "limit":
				{
					double limit = 0.0;
					int direction = 0, cource = 0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out limit))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						limit = 0.0;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out direction))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						direction = 0;
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out cource))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "Cource is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						cource = 0;
					}

					int n = Data.Blocks[BlockIndex].Limits.Length;
					Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limits, n + 1);
					Data.Blocks[BlockIndex].Limits[n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].Limits[n].Speed = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
					Data.Blocks[BlockIndex].Limits[n].Direction = direction;
					Data.Blocks[BlockIndex].Limits[n].Cource = cource;
				}
					break;
				case "stop":
					if (CurrentStation == -1)
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "A stop without a station is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int dir = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out dir))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dir = 0;
						}

						double backw = 5.0, forw = 5.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							backw = 5.0;
						}
						else if (backw <= 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be positive in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							backw = 5.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							forw = 5.0;
						}
						else if (forw <= 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be positive in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							forw = 5.0;
						}

						int cars = 0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out cars))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Cars is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							cars = 0;
						}

						int n = Data.Blocks[BlockIndex].StopPositions.Length;
						Array.Resize<Stop>(ref Data.Blocks[BlockIndex].StopPositions, n + 1);
						Data.Blocks[BlockIndex].StopPositions[n] = new Stop(Data.TrackPosition, CurrentStation, dir, forw, backw, cars);
						CurrentStop = cars;
					}

					break;
				case "sta":
				{
					CurrentStation++;
					Array.Resize(ref CurrentRoute.Stations, CurrentStation + 1);
					CurrentRoute.Stations[CurrentStation] = new RouteStation();
					if (Arguments.Length >= 1 && Arguments[0].Length > 0)
					{
						CurrentRoute.Stations[CurrentStation].Name = Arguments[0];
					}

					double arr = -1.0, dep = -1.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0)
					{
						if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.AllPass;
						}
						else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
						}
						else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
							if (!TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
						}
						else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
							if (!TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (Arguments[1].Length == 1 && Arguments[1][0] == '.')
						{
							/* Treat a single period as a blank space */
						}
						else if (!TryParseTime(Arguments[1], out arr))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							arr = -1.0;
						}
					}

					if (Arguments.Length >= 3 && (Arguments[2].Length > 0))
					{
						if (string.Equals(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[2], "=", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.Terminal;
						}
						else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.Terminal;
							if (!TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.ChangeEnds;
						}
						else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.ChangeEnds;
							if (!TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (Arguments[2].StartsWith("J:", StringComparison.InvariantCultureIgnoreCase))
						{
							string[] splitString = Arguments[2].Split(new char[] {':'});
							for (int i = 0; i < splitString.Length; i++)
							{
								switch (i)
								{
									case 1:
										if (!NumberFormats.TryParseIntVb6(splitString[1].TrimStart(), out CurrentRoute.Stations[CurrentStation].JumpIndex))
										{
											Program.CurrentHost.AddMessage(MessageType.Error, false, "JumpStationIndex is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											dep = -1.0;
										}
										else
										{
											CurrentRoute.Stations[CurrentStation].Type = StationType.Jump;
										}

										break;
									case 2:
										if (!TryParseTime(splitString[2].TrimStart(), out dep))
										{
											Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											dep = -1.0;
										}

										break;
								}
							}
						}
						else if (Arguments[2].Length == 1 && Arguments[2][0] == '.')
						{
							/* Treat a single period as a blank space */
						}
						else if (!TryParseTime(Arguments[2], out dep))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dep = -1.0;
						}
					}

					int passalarm = 0;
					if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out passalarm))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "PassAlarm is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						passalarm = 0;
					}

					int door = 0;
					bool doorboth = false;
					if (Arguments.Length >= 5 && Arguments[4].Length != 0)
					{
						switch (Arguments[4].ToUpperInvariant())
						{
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
								if (!NumberFormats.TryParseIntVb6(Arguments[4], out door))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Doors is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									door = 0;
								}

								break;
						}
					}

					int stop = 0;
					if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[5], out stop))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						stop = 0;
					}

					int device = 0;
					if (Arguments.Length >= 7 && Arguments[6].Length > 0)
					{
						if (string.Compare(Arguments[6], "ats", StringComparison.OrdinalIgnoreCase) == 0)
						{
							device = 0;
						}
						else if (string.Compare(Arguments[6], "atc", StringComparison.OrdinalIgnoreCase) == 0)
						{
							device = 1;
						}
						else if (!NumberFormats.TryParseIntVb6(Arguments[6], out device))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}

						if (device != 0 & device != 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}
					}

					OpenBveApi.Sounds.SoundHandle arrsnd = null;
					OpenBveApi.Sounds.SoundHandle depsnd = null;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 8 && Arguments[7].Length > 0)
						{
							if (Path.ContainsInvalidChars(Arguments[7]))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[7]);
								if (!System.IO.File.Exists(f))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, true, "ArrivalSound " + f + " not found in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Program.CurrentHost.RegisterSound(f, radius, out arrsnd);
								}
							}
						}
					}

					double halt = 15.0;
					if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out halt))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "StopDuration is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						halt = 15.0;
					}
					else if (halt < 5.0)
					{
						halt = 5.0;
					}

					double jam = 100.0;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[9], out jam))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							jam = 100.0;
						}
						else if (jam < 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							jam = 100.0;
						}
					}

					if (!PreviewOnly)
					{
						if (Arguments.Length >= 11 && Arguments[10].Length > 0)
						{
							if (Path.ContainsInvalidChars(Arguments[10]))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[10]);
								if (!System.IO.File.Exists(f))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Program.CurrentHost.RegisterSound(f, radius, out depsnd);
								}
							}
						}
					}

					OpenBveApi.Textures.Texture tdt = null, tnt = null;
					if (!PreviewOnly)
					{
						int ttidx;
						if (Arguments.Length >= 12 && Arguments[11].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[11], out ttidx))
							{
								ttidx = -1;
							}
							else
							{
								if (ttidx < 0)
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									ttidx = -1;
								}
								else if (ttidx >= Data.TimetableDaytime.Length & ttidx >= Data.TimetableNighttime.Length)
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex references textures not loaded in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									ttidx = -1;
								}

								tdt = ttidx >= 0 & ttidx < Data.TimetableDaytime.Length ? Data.TimetableDaytime[ttidx] : null;
								tnt = ttidx >= 0 & ttidx < Data.TimetableNighttime.Length ? Data.TimetableNighttime[ttidx] : null;
								ttidx = 0;
							}
						}
						else
						{
							ttidx = -1;
						}

						if (ttidx == -1)
						{
							if (CurrentStation > 0)
							{
								tdt = CurrentRoute.Stations[CurrentStation - 1].TimetableDaytimeTexture;
								tnt = CurrentRoute.Stations[CurrentStation - 1].TimetableNighttimeTexture;
							}
							else if (Data.TimetableDaytime.Length > 0 & Data.TimetableNighttime.Length > 0)
							{
								tdt = Data.TimetableDaytime[0];
								tnt = Data.TimetableNighttime[0];
							}
							else
							{
								tdt = null;
								tnt = null;
							}
						}
					}

					double reopenDoor = 0.0;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 13 && Arguments[12].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[12], out reopenDoor))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenDoor = 0.0;
						}
						else if (reopenDoor < 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenDoor = 0.0;
						}
					}

					int reopenStationLimit = 5;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 14 && Arguments[13].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[13], out reopenStationLimit))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenStationLimit = 5;
						}
						else if (reopenStationLimit < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenStationLimit = 0;
						}
					}

					double interferenceInDoor = Program.RandomNumberGenerator.NextDouble() * 30.0;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 15 && Arguments[14].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[14], out interferenceInDoor))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							interferenceInDoor = Program.RandomNumberGenerator.NextDouble() * 30.0;
						}
						else if (interferenceInDoor < 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							interferenceInDoor = 0.0;
						}
					}

					int maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 16 && Arguments[15].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[15], out maxInterferingObjectRate))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
						}
						else if (maxInterferingObjectRate <= 0 || maxInterferingObjectRate >= 100)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is expected to be positive, less than 100 in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							maxInterferingObjectRate = Program.RandomNumberGenerator.Next(1, 99);
						}
					}

					if (CurrentRoute.Stations[CurrentStation].Name.Length == 0 & (CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.AllStop))
					{
						CurrentRoute.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture) + ")";
					}

					CurrentRoute.Stations[CurrentStation].ArrivalTime = arr;
					CurrentRoute.Stations[CurrentStation].ArrivalSoundBuffer = arrsnd;
					CurrentRoute.Stations[CurrentStation].DepartureTime = dep;
					CurrentRoute.Stations[CurrentStation].DepartureSoundBuffer = depsnd;
					CurrentRoute.Stations[CurrentStation].StopTime = halt;
					CurrentRoute.Stations[CurrentStation].ForceStopSignal = stop == 1;
					CurrentRoute.Stations[CurrentStation].OpenLeftDoors = door < 0.0 | doorboth;
					CurrentRoute.Stations[CurrentStation].OpenRightDoors = door > 0.0 | doorboth;
					CurrentRoute.Stations[CurrentStation].SafetySystem = device == 1 ? SafetySystem.Atc : SafetySystem.Ats;
					CurrentRoute.Stations[CurrentStation].Stops = new StationStop[] { };
					CurrentRoute.Stations[CurrentStation].PassengerRatio = 0.01 * jam;
					CurrentRoute.Stations[CurrentStation].TimetableDaytimeTexture = tdt;
					CurrentRoute.Stations[CurrentStation].TimetableNighttimeTexture = tnt;
					CurrentRoute.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
					CurrentRoute.Stations[CurrentStation].ReopenDoor = 0.01 * reopenDoor;
					CurrentRoute.Stations[CurrentStation].ReopenStationLimit = reopenStationLimit;
					CurrentRoute.Stations[CurrentStation].InterferenceInDoor = interferenceInDoor;
					CurrentRoute.Stations[CurrentStation].MaxInterferingObjectRate = maxInterferingObjectRate;
					Data.Blocks[BlockIndex].Station = CurrentStation;
					Data.Blocks[BlockIndex].StationPassAlarm = passalarm == 1;
					CurrentStop = -1;
					DepartureSignalUsed = false;
				}
					break;
				case "station":
				{
					CurrentStation++;
					Array.Resize(ref CurrentRoute.Stations, CurrentStation + 1);
					CurrentRoute.Stations[CurrentStation] = new RouteStation();
					if (Arguments.Length >= 1 && Arguments[0].Length > 0)
					{
						CurrentRoute.Stations[CurrentStation].Name = Arguments[0];
					}

					double arr = -1.0, dep = -1.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0)
					{
						if (string.Equals(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.AllPass;
						}
						else if (string.Equals(Arguments[1], "B", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
						}
						else if (Arguments[1].StartsWith("B:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerPass;
							if (!TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (string.Equals(Arguments[1], "S", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
						}
						else if (Arguments[1].StartsWith("S:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.PlayerStop;
							if (!TryParseTime(Arguments[1].Substring(2).TrimStart(), out arr))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (!TryParseTime(Arguments[1], out arr))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							arr = -1.0;
						}
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0)
					{
						if (string.Equals(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[2], "=", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.Terminal;
						}
						else if (Arguments[2].StartsWith("T:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.Terminal;
							if (!TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (string.Equals(Arguments[2], "C", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.ChangeEnds;
						}
						else if (Arguments[2].StartsWith("C:", StringComparison.InvariantCultureIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Type = StationType.ChangeEnds;
							if (!TryParseTime(Arguments[2].Substring(2).TrimStart(), out dep))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (!TryParseTime(Arguments[2], out dep))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dep = -1.0;
						}
					}

					int stop = 0;
					if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out stop))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						stop = 0;
					}

					int device = 0;
					if (Arguments.Length >= 5 && Arguments[4].Length > 0)
					{
						if (string.Compare(Arguments[4], "ats", StringComparison.OrdinalIgnoreCase) == 0 || (Program.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("ats", StringComparison.OrdinalIgnoreCase)))
						{
							device = 0;
						}
						else if (string.Compare(Arguments[4], "atc", StringComparison.OrdinalIgnoreCase) == 0 || (Program.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("atc", StringComparison.OrdinalIgnoreCase)))
						{
							device = 1;
						}
						else if (!NumberFormats.TryParseIntVb6(Arguments[4], out device))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}
						else if (device != 0 & device != 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}
					}

					OpenBveApi.Sounds.SoundHandle depsnd = null;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 6 && Arguments[5].Length != 0)
						{
							if (Path.ContainsInvalidChars(Arguments[5]))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = OpenBveApi.Path.CombineFile(SoundPath, Arguments[5]);
								if (!System.IO.File.Exists(f))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Program.CurrentHost.RegisterSound(f, radius, out depsnd);
								}
							}
						}
					}

					if (CurrentRoute.Stations[CurrentStation].Name.Length == 0 & (CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.AllStop))
					{
						CurrentRoute.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture) + ")";
					}

					CurrentRoute.Stations[CurrentStation].ArrivalTime = arr;
					CurrentRoute.Stations[CurrentStation].ArrivalSoundBuffer = null;
					CurrentRoute.Stations[CurrentStation].DepartureTime = dep;
					CurrentRoute.Stations[CurrentStation].DepartureSoundBuffer = depsnd;
					CurrentRoute.Stations[CurrentStation].StopTime = 15.0;
					CurrentRoute.Stations[CurrentStation].ForceStopSignal = stop == 1;
					CurrentRoute.Stations[CurrentStation].OpenLeftDoors = true;
					CurrentRoute.Stations[CurrentStation].OpenRightDoors = true;
					CurrentRoute.Stations[CurrentStation].SafetySystem = device == 1 ? SafetySystem.Atc : SafetySystem.Ats;
					CurrentRoute.Stations[CurrentStation].Stops = new StationStop[] { };
					CurrentRoute.Stations[CurrentStation].PassengerRatio = 1.0;
					CurrentRoute.Stations[CurrentStation].TimetableDaytimeTexture = null;
					CurrentRoute.Stations[CurrentStation].TimetableNighttimeTexture = null;
					CurrentRoute.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
					CurrentRoute.Stations[CurrentStation].ReopenDoor = 0.0;
					CurrentRoute.Stations[CurrentStation].ReopenStationLimit = 0;
					CurrentRoute.Stations[CurrentStation].InterferenceInDoor = 0.0;
					CurrentRoute.Stations[CurrentStation].MaxInterferingObjectRate = 10;
					Data.Blocks[BlockIndex].Station = CurrentStation;
					Data.Blocks[BlockIndex].StationPassAlarm = false;
					CurrentStop = -1;
					DepartureSignalUsed = false;
				}
					break;
				case "stationxml":
					string fn = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
					if (!System.IO.File.Exists(fn))
					{
						Program.CurrentHost.AddMessage(MessageType.Error, true, "Station XML file " + fn + " not found in Track.StationXML at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					CurrentStation++;
					Array.Resize(ref CurrentRoute.Stations, CurrentStation + 1);
					CurrentRoute.Stations[CurrentStation] = new RouteStation();
					StopRequest sr = new StopRequest();
					sr.TrackPosition = Data.TrackPosition;
					sr.StationIndex = CurrentStation;
					CurrentRoute.Stations[CurrentStation] = StationXMLParser.ReadStationXML(fn, PreviewOnly, Data.TimetableDaytime, Data.TimetableNighttime, CurrentStation, ref Data.Blocks[BlockIndex].StationPassAlarm, ref sr);
					if (CurrentRoute.Stations[CurrentStation].Type == StationType.RequestStop)
					{
						int l = Data.RequestStops.Length;
						Array.Resize<StopRequest>(ref Data.RequestStops, l + 1);
						Data.RequestStops[l] = sr;
					}

					Data.Blocks[BlockIndex].Station = CurrentStation;
					break;
				case "buffer":
				{
					if (!PreviewOnly)
					{
						int n = CurrentRoute.BufferTrackPositions.Length;
						Array.Resize<double>(ref CurrentRoute.BufferTrackPositions, n + 1);
						CurrentRoute.BufferTrackPositions[n] = Data.TrackPosition;
					}
				}
					break;
				case "form":
				{
					if (!PreviewOnly)
					{
						int idx1 = 0, idx2 = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx1 = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							if (string.Compare(Arguments[1], "L", StringComparison.OrdinalIgnoreCase) == 0)
							{
								idx2 = Form.SecondaryRailL;
							}
							else if (string.Compare(Arguments[1], "R", StringComparison.OrdinalIgnoreCase) == 0)
							{
								idx2 = Form.SecondaryRailR;
							}
							else if (IsRW && string.Compare(Arguments[1], "9X", StringComparison.OrdinalIgnoreCase) == 0)
							{
								idx2 = int.MaxValue;
							}
							else if (!NumberFormats.TryParseIntVb6(Arguments[1], out idx2))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								idx2 = 0;
							}
						}

						if (IsRW)
						{
							if (idx2 == int.MaxValue)
							{
								idx2 = 9;
							}
							else if (idx2 == -9)
							{
								idx2 = Form.SecondaryRailL;
							}
							else if (idx2 == 9)
							{
								idx2 = Form.SecondaryRailR;
							}
						}

						if (idx1 < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (idx2 < 0 & idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be greater or equal to -2 in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx1) || !Data.Blocks[BlockIndex].Rails[idx1].RailStarted)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex1 could be out of range in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR && (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx2) || !Data.Blocks[BlockIndex].Rails[idx2].RailStarted))
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex2 could be out of range in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							int roof = 0, pf = 0;
							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out roof))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roof = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out pf))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pf = 0;
							}

							if (roof != 0 & (roof < 0 || (!Data.Structure.RoofL.ContainsKey(roof) && !Data.Structure.RoofR.ContainsKey(roof))))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex " + roof + " references an object not loaded in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (pf < 0 | (!Data.Structure.FormL.ContainsKey(pf) & !Data.Structure.FormR.ContainsKey(pf)))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex " + pf + " references an object not loaded in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
					break;
				case "pole":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (idx >= Data.Blocks[BlockIndex].RailPole.Length)
							{
								Array.Resize<Pole>(ref Data.Blocks[BlockIndex].RailPole, idx + 1);
								Data.Blocks[BlockIndex].RailPole[idx].Mode = 0;
								Data.Blocks[BlockIndex].RailPole[idx].Location = 0;
								Data.Blocks[BlockIndex].RailPole[idx].Interval = 2.0 * Data.BlockInterval;
								Data.Blocks[BlockIndex].RailPole[idx].Type = 0;
							}

							int typ = Data.Blocks[BlockIndex].RailPole[idx].Mode;
							int sttype = Data.Blocks[BlockIndex].RailPole[idx].Type;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0)
							{
								if (!NumberFormats.TryParseIntVb6(Arguments[1], out typ))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "AdditionalRailsCovered is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									typ = 0;
								}
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0)
							{
								double loc;
								if (!NumberFormats.TryParseDoubleVb6(Arguments[2], out loc))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Location is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									loc = 0.0;
								}

								Data.Blocks[BlockIndex].RailPole[idx].Location = loc;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0)
							{
								double dist;
								if (!NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out dist))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Interval is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dist = Data.BlockInterval;
								}

								Data.Blocks[BlockIndex].RailPole[idx].Interval = dist;
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0)
							{
								if (!NumberFormats.TryParseIntVb6(Arguments[4], out sttype))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									sttype = 0;
								}
							}

							if (typ < 0 || !Data.Structure.Poles.ContainsKey(typ) || Data.Structure.Poles[typ] == null)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (sttype < 0 || !Data.Structure.Poles[typ].ContainsKey(sttype) || Data.Structure.Poles[typ][sttype] == null)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].RailPole[idx].Mode = typ;
								Data.Blocks[BlockIndex].RailPole[idx].Type = sttype;
								Data.Blocks[BlockIndex].RailPole[idx].Exists = true;
							}
						}
					}
				}
					break;
				case "poleend":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailPole.Length)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing pole in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Data.Blocks[BlockIndex].RailPole[idx].Exists = false;
						}
					}
				}
					break;
				case "wall":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						int dir = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							switch (Arguments[1].ToUpperInvariant().Trim(new char[] { }))
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
										Program.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = 0;
									}

									break;
							}
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex is invalid in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex is expected to be a non-negative integer in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (dir < 0 && !Data.Structure.WallL.ContainsKey(sttype) || dir > 0 && !Data.Structure.WallR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.WallL.ContainsKey(sttype) && !Data.Structure.WallR.ContainsKey(sttype)))
						{
							if (dir < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (dir > 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallR at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}
						else
						{
							if (idx < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (dir == 0)
								{
									if (!Data.Structure.WallL.ContainsKey(sttype))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "LeftWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = 1;
									}

									if (!Data.Structure.WallR.ContainsKey(sttype))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "RightWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = -1;
									}
								}

								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
								{
									Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (idx >= Data.Blocks[BlockIndex].RailWall.Length)
								{
									Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailWall, idx + 1);
								}

								Data.Blocks[BlockIndex].RailWall[idx].Exists = true;
								Data.Blocks[BlockIndex].RailWall[idx].Type = sttype;
								Data.Blocks[BlockIndex].RailWall[idx].Direction = dir;
							}
						}
					}
				}
					break;
				case "wallend":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailWall.Length)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing wall in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Data.Blocks[BlockIndex].RailWall[idx].Exists = false;
						}
					}
				}
					break;
				case "dike":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						int dir = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							switch (Arguments[1].ToUpperInvariant().Trim(new char[] { }))
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
										Program.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = 0;
									}

									break;
							}
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex is invalid in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex is expected to be a non-negative integer in Track.DikeL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (dir < 0 && !Data.Structure.DikeL.ContainsKey(sttype) || dir > 0 && !Data.Structure.DikeR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.DikeL.ContainsKey(sttype) && !Data.Structure.DikeR.ContainsKey(sttype)))
						{
							if (dir > 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (dir < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeR at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Program.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (idx < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (dir == 0)
								{
									if (!Data.Structure.DikeL.ContainsKey(sttype))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "LeftDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = 1;
									}

									if (!Data.Structure.DikeR.ContainsKey(sttype))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "RightDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										dir = -1;
									}
								}

								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
								{
									Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (idx >= Data.Blocks[BlockIndex].RailDike.Length)
								{
									Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailDike, idx + 1);
								}

								Data.Blocks[BlockIndex].RailDike[idx].Exists = true;
								Data.Blocks[BlockIndex].RailDike[idx].Type = sttype;
								Data.Blocks[BlockIndex].RailDike[idx].Direction = dir;
							}
						}
					}
				}
					break;
				case "dikeend":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailDike.Length)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing dike in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Data.Blocks[BlockIndex].RailDike[idx].Exists = false;
						}
					}
				}
					break;
				case "marker":
				case "textmarker":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Marker is expected to have at least one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Path.ContainsInvalidChars(Arguments[0]))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							string f = OpenBveApi.Path.CombineFile(ObjectPath, Arguments[0]);
							if (!System.IO.File.Exists(f) && Command.ToLowerInvariant() == "marker")
							{
								Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in Track.Marker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
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
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out dist))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Distance is invalid in Track.Marker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dist = Data.BlockInterval;
								}

								double start, end;
								if (dist < 0.0)
								{
									start = Data.TrackPosition;
									end = Data.TrackPosition - dist;
								}
								else
								{
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
								if (Command.ToLowerInvariant() == "textmarker")
								{
									Data.Markers[n].Message = new MarkerText(Arguments[0]);
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
												Program.CurrentHost.AddMessage(MessageType.Error, false, "MessageColor is invalid in Track.TextMarker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
												//Default message color is set to white
												break;
										}
									}
								}
								else
								{
									OpenBveApi.Textures.Texture t;
									Program.CurrentHost.RegisterTexture(f, new OpenBveApi.Textures.TextureParameters(null, new Color24(64, 64, 64)), out t);
									Data.Markers[n].Message = new MarkerImage(Program.CurrentHost, t);

								}

							}
						}
					}
				}
					break;
				case "height":
				{
					if (!PreviewOnly)
					{
						double h = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out h))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Height is invalid in Track.Height at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							h = 0.0;
						}

						Data.Blocks[BlockIndex].Height = IsRW ? h + 0.3 : h;
					}
				}
					break;
				case "ground":
				{
					if (!PreviewOnly)
					{
						int cytype = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out cytype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "CycleIndex is invalid in Track.Ground at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							cytype = 0;
						}

						if (cytype < Data.Structure.Cycles.Length && Data.Structure.Cycles[cytype] != null)
						{
							Data.Blocks[BlockIndex].Cycle = Data.Structure.Cycles[cytype];
						}
						else
						{
							if (!Data.Structure.Ground.ContainsKey(cytype))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "CycleIndex " + cytype + " references an object not loaded in Track.Ground at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].Cycle = new int[] {cytype};
							}
						}
					}
				}
					break;
				case "crack":
				{
					if (!PreviewOnly)
					{
						int idx1 = 0, idx2 = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx1 = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out idx2))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx2 = 0;
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0 || !Data.Structure.CrackL.ContainsKey(sttype) || !Data.Structure.CrackR.ContainsKey(sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex " + sttype + " references an object not loaded in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (idx1 < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (idx2 < 0)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be non-negative in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (idx1 == idx2)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be unequal to Index2 in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx1) || !Data.Blocks[BlockIndex].Rails[idx1].RailStarted)
								{
									Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx2) || !Data.Blocks[BlockIndex].Rails[idx2].RailStarted)
								{
									Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex2 " + idx2 + " could be out of range in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								int n = Data.Blocks[BlockIndex].Cracks.Length;
								Array.Resize<Crack>(ref Data.Blocks[BlockIndex].Cracks, n + 1);
								Data.Blocks[BlockIndex].Cracks[n].PrimaryRail = idx1;
								Data.Blocks[BlockIndex].Cracks[n].SecondaryRail = idx2;
								Data.Blocks[BlockIndex].Cracks[n].Type = sttype;
							}
						}
					}
				}
					break;
				case "freeobj":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length < 2)
						{
							/*
							 * If no / one arguments are supplied, this previously produced FreeObject 0 dropped on either
							 * Rail 0 (no arguments) or on the rail specified by the first argument.
							 *
							 * BVE4 ignores these, and we should too.
							 */
							Program.CurrentHost.AddMessage(MessageType.Error, false, "An insufficient number of arguments was supplied in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							return;
						}

						int idx = 0, sttype = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (idx < -1)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative or -1 in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (sttype < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is expected to be non-negative in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (idx >= 0 && (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted))
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (!Data.Structure.FreeObjects.ContainsKey(sttype))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex " + sttype + " references an object not loaded in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								double x = 0.0, y = 0.0;
								double yaw = 0.0, pitch = 0.0, roll = 0.0;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									x = 0.0;
								}

								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									y = 0.0;
								}

								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									yaw = 0.0;
								}

								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									pitch = 0.0;
								}

								if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									roll = 0.0;
								}

								if (idx == -1)
								{
									int n = Data.Blocks[BlockIndex].GroundFreeObj.Length;
									Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].GroundFreeObj, n + 1);
									Data.Blocks[BlockIndex].GroundFreeObj[n].TrackPosition = Data.TrackPosition;
									Data.Blocks[BlockIndex].GroundFreeObj[n].Type = sttype;
									Data.Blocks[BlockIndex].GroundFreeObj[n].Position.X = x;
									Data.Blocks[BlockIndex].GroundFreeObj[n].Position.Y = y;
									Data.Blocks[BlockIndex].GroundFreeObj[n].Yaw = yaw.ToRadians();
									if (!Data.IgnorePitchRoll)
									{
										Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = pitch.ToRadians();
										Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = roll.ToRadians();
									}
									else
									{
										Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = 0;
										Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = 0;
									}
								}
								else
								{
									if (idx >= Data.Blocks[BlockIndex].RailFreeObj.Length)
									{
										Array.Resize<FreeObj[]>(ref Data.Blocks[BlockIndex].RailFreeObj, idx + 1);
									}

									int n;
									if (Data.Blocks[BlockIndex].RailFreeObj[idx] == null)
									{
										Data.Blocks[BlockIndex].RailFreeObj[idx] = new FreeObj[1];
										n = 0;
									}
									else
									{
										n = Data.Blocks[BlockIndex].RailFreeObj[idx].Length;
										Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].RailFreeObj[idx], n + 1);
									}

									Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
									Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
									Data.Blocks[BlockIndex].RailFreeObj[idx][n].Position.X = x;
									Data.Blocks[BlockIndex].RailFreeObj[idx][n].Position.Y = y;
									Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw.ToRadians();
									if (!Data.IgnorePitchRoll)
									{
										Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch.ToRadians();
										Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll.ToRadians();
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
				}
					break;
				case "back":
				case "background":
				{
					if (!PreviewOnly)
					{
						int typ = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out typ))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							typ = 0;
						}

						if (typ < 0 | !Data.Backgrounds.ContainsKey(typ))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex " + typ + " references a background not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Data.Backgrounds[typ] is StaticBackground)
						{
							StaticBackground b = Data.Backgrounds[typ] as StaticBackground;
							if (b.Texture == null)
							{
								//There's a possibility that this was loaded via a default BVE command rather than XML
								//Thus check for the existance of the file and chuck out error if appropriate
								Program.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex " + typ + " has not been loaded via Texture.Background in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].Background = typ;
								if (Program.CurrentOptions.EnableBveTsHacks && Data.Blocks.Length == 2 && Data.Blocks[0].Background == 0)
								{
									//The initial background for block 0 is always set to zero
									//This handles the case where background idx #0 is not used
									b = Data.Backgrounds[0] as StaticBackground;
									if (b.Texture == null)
									{
										Data.Blocks[0].Background = typ;
									}
								}
							}
						}
						else if (Data.Backgrounds[typ] is DynamicBackground)
						{
							//File existance checks should already have been made when loading the XML
							Data.Blocks[BlockIndex].Background = typ;
						}
						else
						{
							Data.Blocks[BlockIndex].Background = typ;
						}
					}
				}
					break;
				case "announce":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateSound(ref f, SoundPath))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									double speed = 0.0;
									if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										speed = 0.0;
									}

									int n = Data.Blocks[BlockIndex].SoundEvents.Length;
									Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
									Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
									const double radius = 15.0;
									Program.CurrentHost.RegisterSound(f, radius, out Data.Blocks[BlockIndex].SoundEvents[n].SoundBuffer);
									Data.Blocks[BlockIndex].SoundEvents[n].Type = speed == 0.0 ? SoundType.TrainStatic : SoundType.TrainDynamic;
									Data.Blocks[BlockIndex].SoundEvents[n].Speed = speed * Data.UnitOfSpeed;
								}
							}
						}
					}
				}
					break;
				case "doppler":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 3 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateSound(ref f, SoundPath))
								{
									Program.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									double x = 0.0, y = 0.0;
									if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										x = 0.0;
									}

									if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
									{
										Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										y = 0.0;
									}

									int n = Data.Blocks[BlockIndex].SoundEvents.Length;
									Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
									Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
									const double radius = 15.0;
									Program.CurrentHost.RegisterSound(f, radius, out Data.Blocks[BlockIndex].SoundEvents[n].SoundBuffer);
									Data.Blocks[BlockIndex].SoundEvents[n].Type = SoundType.World;
									Data.Blocks[BlockIndex].SoundEvents[n].Position.X = x;
									Data.Blocks[BlockIndex].SoundEvents[n].Position.Y = y;
									Data.Blocks[BlockIndex].SoundEvents[n].Radius = radius;
								}
							}
						}
					}
				}
					break;
				case "micsound":
				{
					if (!PreviewOnly)
					{
						double x = 0.0, y = 0.0, back = 0.0, front = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out back))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							back = 0.0;
						}
						else if (back < 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							back = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out front))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							front = 0.0;
						}
						else if (front < 0.0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							front = 0.0;
						}

						int n = Data.Blocks[BlockIndex].SoundEvents.Length;
						Array.Resize<Sound>(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
						Data.Blocks[BlockIndex].SoundEvents[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].SoundEvents[n].Type = SoundType.World;
						Data.Blocks[BlockIndex].SoundEvents[n].Position.X = x;
						Data.Blocks[BlockIndex].SoundEvents[n].Position.Y = y;
						Data.Blocks[BlockIndex].SoundEvents[n].IsMicSound = true;
						Data.Blocks[BlockIndex].SoundEvents[n].BackwardTolerance = back;
						Data.Blocks[BlockIndex].SoundEvents[n].ForwardTolerance = front;
					}
				}
					break;
				case "pretrain":
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have exactly 1 argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							double time;
							if (Arguments[0].Length > 0 & !TryParseTime(Arguments[0], out time))
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Time is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								time = 0.0;
							}

							int n = CurrentRoute.BogusPreTrainInstructions.Length;
							if (n != 0 && CurrentRoute.BogusPreTrainInstructions[n - 1].Time >= time)
							{
								Program.CurrentHost.AddMessage(MessageType.Error, false, "Time is expected to be in ascending order between successive " + Command + " commands at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Array.Resize(ref CurrentRoute.BogusPreTrainInstructions, n + 1);
							CurrentRoute.BogusPreTrainInstructions[n].TrackPosition = Data.TrackPosition;
							CurrentRoute.BogusPreTrainInstructions[n].Time = time;
						}
					}
				}
					break;
				case "pointofinterest":
				case "poi":
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx >= 0 && (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out yaw))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out pitch))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out roll))
						{
							Program.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						string text = null;
						if (Arguments.Length >= 7 && Arguments[6].Length != 0)
						{
							text = Arguments[6];
						}

						int n = Data.Blocks[BlockIndex].PointsOfInterest.Length;
						Array.Resize<PointOfInterest>(ref Data.Blocks[BlockIndex].PointsOfInterest, n + 1);
						Data.Blocks[BlockIndex].PointsOfInterest[n].TrackPosition = Data.TrackPosition;
						Data.Blocks[BlockIndex].PointsOfInterest[n].RailIndex = idx;
						Data.Blocks[BlockIndex].PointsOfInterest[n].Position.X = x;
						Data.Blocks[BlockIndex].PointsOfInterest[n].Position.Y = y;
						Data.Blocks[BlockIndex].PointsOfInterest[n].Yaw = yaw.ToRadians();
						Data.Blocks[BlockIndex].PointsOfInterest[n].Pitch = pitch.ToRadians();
						Data.Blocks[BlockIndex].PointsOfInterest[n].Roll = roll.ToRadians();
						Data.Blocks[BlockIndex].PointsOfInterest[n].Text = text;
					}
				}
					break;
			}
		}

	}
}
