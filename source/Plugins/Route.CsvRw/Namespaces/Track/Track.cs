using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.Events;
using RouteManager2.MessageManager;
using RouteManager2.MessageManager.MessageTypes;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using RouteManager2.Tracks;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private int CurrentStation = -1;
		private int CurrentStop = -1;
		private int CurrentSection = 0;
		private bool DepartureSignalUsed = false;
		private void ParseTrackCommand(TrackCommand Command, string[] Arguments, string FileName, double[] UnitOfLength, Expression Expression, ref RouteData Data, int BlockIndex, bool PreviewOnly, bool IsRw, int RailIndex = 0)
		{
			switch (Command)
			{
				case TrackCommand.RailStart:
				case TrackCommand.Rail:
				{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (Command == TrackCommand.RailStart)
						{
							if (Data.Blocks[BlockIndex].Rails.ContainsKey(idx) && Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is required to reference a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}

						if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx))
						{
							Data.Blocks[BlockIndex].Rails.Add(idx, new Rail(2.0, 1.0));

							if (idx >= Data.Blocks[BlockIndex].RailCycles.Length)
							{
								int ol = Data.Blocks[BlockIndex].RailCycles.Length;
								Array.Resize(ref Data.Blocks[BlockIndex].RailCycles, idx + 1);
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
								if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out currentRail.RailStart.X))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									currentRail.RailStart.X = 0.0;
								}
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
								if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out currentRail.RailStart.Y))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									currentRail.RailStart.Y = 0.0;
								}
							}

							if (!currentRail.RailEnded)
							{
								currentRail.RailEnd.Y = currentRail.RailStart.Y;
							}
						}

						if (Data.Blocks[BlockIndex].RailType.Length <= idx)
						{
							Array.Resize(ref Data.Blocks[BlockIndex].RailType, idx + 1);
						}

						// Ignore the RailStructureIndex in preview mode, obviously not visible!
						if (!PreviewOnly && Arguments.Length >= 4 && Arguments[3].Length != 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[3], out int sttype))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								sttype = 0;
							}

							if (sttype < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (!Data.Structure.RailObjects.ContainsKey(sttype))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
				case TrackCommand.RailEnd:
				{
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The command " + Command + " is invalid for Rail 0 at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx < 0 || !Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx))
						{
							Data.Blocks[BlockIndex].Rails.Add(idx, new Rail(2.0, 1.0));
						}

						Rail currentRail = Data.Blocks[BlockIndex].Rails[idx];
						currentRail.RailStarted = false;
						currentRail.RailStartRefreshed = false;
						currentRail.RailEnded = true;
						currentRail.IsDriveable = false;
						
						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out currentRail.RailEnd.X))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								currentRail.RailEnd.X = 0.0;
							}
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0)
						{
							if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out currentRail.RailEnd.Y))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								currentRail.RailEnd.Y = 0.0;
							}
						}

						Data.Blocks[BlockIndex].Rails[idx] = currentRail;
					}
				}
					break;
				case TrackCommand.RailType:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						int sttype = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (sttype < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (!Data.Structure.RailObjects.ContainsKey(sttype))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex " + sttype + " references an object not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Blocks[BlockIndex].RailType.Length <= idx)
								{
									Array.Resize(ref Data.Blocks[BlockIndex].RailType, idx + 1);
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
				case TrackCommand.Accuracy:
				{
					double acc = 2.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out acc))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						acc = 2.0;
					}

					if (acc < 0.0)
					{
						acc = 0.0;
					}
					else if (acc > 4.0)
					{
						acc = 4.0;
					}

					Data.Blocks[BlockIndex].Rails[0].Accuracy = acc;
				}
					break;
				case TrackCommand.Pitch:
				{
					double p = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out p))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ValueInPermille is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						p = 0.0;
					}

					Data.Blocks[BlockIndex].Pitch = 0.001 * p;
				}
					break;
				case TrackCommand.Curve:
				{
					double radius = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						radius = 0.0;
					}

					double cant = 0.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out cant))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							cant *= Math.Sign(radius);
						}
					}
					else
					{
						cant = Math.Abs(cant) * Math.Sign(radius);
					}

					Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = radius;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = cant;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
				}
					break;
				case TrackCommand.Turn:
				{
					Data.TurnUsed = true;
					double s = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out s))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Ratio is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						s = 0.0;
					}

					Data.Blocks[BlockIndex].Turn = s;
				}
					break;
				case TrackCommand.Adhesion:
				{
					double a = 100.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out a))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100.0;
					}

					if (a < 0.0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100.0;
					}

					Data.Blocks[BlockIndex].Rails[0].AdhesionMultiplier = 0.01 * a;
				}
					break;
				case TrackCommand.Brightness:
				{
					if (!PreviewOnly)
					{
						float value = 255.0f;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[0], out value))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							value = 255.0f;
						}

						value /= 255.0f;
						if (value < 0.0f) value = 0.0f;
						if (value > 1.0f) value = 1.0f;
						int n = Data.Blocks[BlockIndex].BrightnessChanges.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].BrightnessChanges, n + 1);
						Data.Blocks[BlockIndex].BrightnessChanges[n] = new Brightness(Data.TrackPosition, value);
					}
				}
					break;
				case TrackCommand.Fog:
				{
					if (!PreviewOnly)
					{
						double start = 0.0, end = 0.0;
						int r = 128, g = 128, b = 128;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out start))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "StartingDistance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							start = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out end))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "EndingDistance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							end = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out r))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							r = 128;
						}
						
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[3], out g))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							g = 128;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[4], out b))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							b = 128;
						}

						if (start < end)
						{
							Data.Blocks[BlockIndex].Fog.Start = (float)start;
							Data.Blocks[BlockIndex].Fog.End = (float)end;
						}
						else
						{
							Data.Blocks[BlockIndex].Fog.Start = CurrentRoute.NoFogStart;
							Data.Blocks[BlockIndex].Fog.End = CurrentRoute.NoFogEnd;
						}

						Data.Blocks[BlockIndex].Fog.Color = new Color24((byte)r, (byte)g, (byte)b);
						Data.Blocks[BlockIndex].FogDefined = true;
					}
				}
					break;
				case TrackCommand.Section:
				case TrackCommand.SectionS:
				case TrackCommand.SectionP:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "At least one argument is required in " + Command + "at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								if (string.IsNullOrEmpty(Arguments[i]) || !NumberFormats.TryParseIntVb6(Arguments[i], out aspects[i]))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									aspects[i] = -1;
								}
								else if (aspects[i] < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Aspect" + i.ToString(Culture) + " is expected to be non-negative in " + Command + "at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									aspects[i] = -1;
								}
							}

							bool valueBased = Data.ValueBasedSections | Command == TrackCommand.SectionS;
							if (valueBased)
							{
								Array.Sort(aspects);
							}

							int n = Data.Blocks[BlockIndex].Sections.Length;
							Array.Resize(ref Data.Blocks[BlockIndex].Sections, n + 1);
							int departureStationIndex = -1;
							if (CurrentStation >= 0 && CurrentRoute.Stations[CurrentStation].ForceStopSignal)
							{
								if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
								{
									departureStationIndex = CurrentStation;
									DepartureSignalUsed = true;
								}
							}

							SectionType type;
							if (valueBased)
							{
								type = Command == TrackCommand.SectionP ? SectionType.PermissiveValueBased : SectionType.ValueBased;
							}
							else
							{
								type = Command == TrackCommand.SectionP ? SectionType.PermissiveIndexBased : SectionType.IndexBased;
							}
							Data.Blocks[BlockIndex].Sections[n] = new Section(Data.TrackPosition, aspects, departureStationIndex, type);


							CurrentSection++;
						}
					}
				}
					break;
				case TrackCommand.SigF:
				{
					if (!PreviewOnly)
					{
						int objidx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out objidx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SignalIndex is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							objidx = 0;
						}

						SignalObject signalObject;
						if (Data.Signals.ContainsKey(objidx))
						{
							signalObject = Data.Signals[objidx];
						}
						else if (objidx >= 0 && Data.CompatibilitySignals.Length > objidx)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "SignalIndex " + objidx + " mapped to default signal, as custom signal not loaded in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							signalObject = Data.CompatibilitySignals[objidx];
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SignalIndex " + objidx + " references a signal object not loaded in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						int section = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out section))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Section is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							section = 0;
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.SigF at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Signals.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Signals, n + 1);
						Data.Blocks[BlockIndex].Signals[n] = new Signal(Data.TrackPosition, CurrentSection + section, signalObject, new Vector2(x, y < 0.0 ? 4.8 : y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians(), true, y < 0.0);
					}
				}
					break;
				case TrackCommand.Signal:
				case TrackCommand.Sig:
				{
					if (!PreviewOnly)
					{
						int num = -2;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out num))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Aspects is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							num = -2;
						}

						if (num == 0 && IsRW)
						{
							//Aspects value of zero in RW routes produces a 2-aspect R/G signal
							num = -2;
						}

						if (num != 1 & num != -2 & num != 2 & num != -3 & num != 3 & num != -4 & num != 4 & num != -5 & num != 5 & num != 6)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Aspects has an unsupported value in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							num = num == -3 | num == -6 | num == -1 ? -num : -4;
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int[] aspects;
						int comp;
						switch (num)
						{
							case 1:
								aspects = new[] { 0, 2, 3 };
								comp = 4;
								break;
							case 2:
								aspects = new[] { 0, 2 };
								comp = 0;
								break;
							case -2:
								aspects = new[] { 0, 4 };
								comp = 1;
								break;
							case -3:
								aspects = new[] { 0, 2, 4 };
								comp = 2;
								break; //Undocumented, see https://github.com/leezer3/OpenBVE/issues/336
							case 3:
								aspects = new[] { 0, 2, 4 };
								comp = 2;
								break;
							case 4:
								aspects = new[] { 0, 1, 2, 4 };
								comp = 3;
								break;
							case -4:
								aspects = new[] { 0, 2, 3, 4 };
								comp = 4;
								break;
							case 5:
								aspects = new[] { 0, 1, 2, 3, 4 };
								comp = 5;
								break;
							case -5:
								aspects = new[] { 0, 2, 3, 4, 5 };
								comp = 6;
								break;
							case 6:
								aspects = new[] { 0, 1, 2, 3, 4, 5 };
								comp = 7;
								break;
							default:
								aspects = new[] { 0, 2 };
								comp = 0;
								break;
						}

						int n = Data.Blocks[BlockIndex].Sections.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Sections, n + 1);
						int departureStationIndex = -1;
						if (CurrentStation >= 0 && CurrentRoute.Stations[CurrentStation].ForceStopSignal)
						{
							if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
							{
								departureStationIndex = CurrentStation;
								DepartureSignalUsed = true;
							}
						}

						Data.Blocks[BlockIndex].Sections[n] = new Section(Data.TrackPosition, aspects, departureStationIndex, SectionType.ValueBased, x == 0.0);
						CurrentSection++;
						n = Data.Blocks[BlockIndex].Signals.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Signals, n + 1);
						Data.Blocks[BlockIndex].Signals[n] = new Signal(Data.TrackPosition, CurrentSection, Data.CompatibilitySignals[comp], new Vector2(x, y < 0.0 ? 4.8 : y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians(), x != 0.0, x != 0.0 & y < 0.0);
					}
				}
					break;
				case TrackCommand.Relay:
				{
					if (!PreviewOnly)
					{
						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out yaw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out pitch))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out roll))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Relay at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Signals.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Signals, n + 1);
						Data.Blocks[BlockIndex].Signals[n] = new Signal(Data.TrackPosition, CurrentSection + 1, Data.CompatibilitySignals[8], new Vector2(x, y < 0.0 ? 4.8 : y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians(), x != 0.0, x != 0.0 & y < 0.0);
					}
				}
					break;
				case TrackCommand.Destination:
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (type < -1 || type > 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is expected to be in the range of -1 to 1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int structure = 0, nextDestination = 0, previousDestination = 0, triggerOnce = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = 0;
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out nextDestination))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NextDestination is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								nextDestination = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out previousDestination))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PreviousDestination is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								previousDestination = 0;
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out triggerOnce))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								previousDestination = 0;
							}

							if (structure < -1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}
							else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}

							if (triggerOnce < 0 || triggerOnce > 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is expected to be in the range of 0 to 1 in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								triggerOnce = 0;
							}

							double x = 0.0, y = 0.0;
							double yaw = 0.0, pitch = 0.0, roll = 0.0;
							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out x))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], UnitOfLength, out y))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out yaw))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out pitch))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[9], out roll))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Destination at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roll = 0.0;
							}

							int n = Data.Blocks[BlockIndex].DestinationChanges.Length;
							Array.Resize(ref Data.Blocks[BlockIndex].DestinationChanges, n + 1);
							Data.Blocks[BlockIndex].DestinationChanges[n] = new DestinationEvent(Data.TrackPosition, type, triggerOnce != 0, structure, nextDestination, previousDestination, new Vector2(x, y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
						}
					}
				}
					break;
				case TrackCommand.Beacon:
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (type < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is expected to be non-negative in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int structure = 0, section = 0, optional = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = 0;
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out section))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Section is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								section = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out optional))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Data is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								optional = 0;
							}

							if (structure < -1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}
							else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}

							if (section == -1)
							{
								//section = (int)TrackManager.TransponderSpecialSection.NextRedSection;
							}
							else if (section < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Section is expected to be non-negative or -1 in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], UnitOfLength, out y))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out yaw))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out pitch))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out roll))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.Beacon at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roll = 0.0;
							}

							int n = Data.Blocks[BlockIndex].Transponders.Length;
							Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
							Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, type, optional, new Vector2(x, y), section, structure, false, yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
						}
					}
				}
					break;
				case TrackCommand.Transponder:
				case TrackCommand.Tr:
				{
					if (!PreviewOnly)
					{
						int type = 0, oversig = 0, work = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out oversig))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Signals is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							oversig = 0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out work))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "SwitchSystems is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							work = 0;
						}

						if (oversig < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Signals is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							oversig = 0;
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out pitch))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out roll))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, type, work, new Vector2(x, y), CurrentSection + oversig + 1, -2, true, yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
					}
				}
					break;
				case TrackCommand.ATSSn:
				{
					if (!PreviewOnly)
					{
						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, 0, 0, new Vector2(), CurrentSection + 1, -2);
					}
				}
					break;
				case TrackCommand.ATSP:
				{
					if (!PreviewOnly)
					{
						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, 3, 0, new Vector2(), CurrentSection + 1, -2);
					}
				}
					break;
				case TrackCommand.Pattern:
				{
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						double speed = 0.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							speed = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						if (type == 0)
						{
							Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, TransponderTypes.InternalAtsPTemporarySpeedLimit, speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6));
						}
						else
						{
							Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, TransponderTypes.AtsPPermanentSpeedLimit, speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6));
						}
					}
				}
					break;
				case TrackCommand.PLimit:
				{
					if (!PreviewOnly)
					{
						double speed = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out speed))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							speed = 0.0;
						}

						int n = Data.Blocks[BlockIndex].Transponders.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].Transponders, n + 1);
						Data.Blocks[BlockIndex].Transponders[n] = new Transponder(Data.TrackPosition, TransponderTypes.AtsPPermanentSpeedLimit, speed == 0.0 ? int.MaxValue : (int)Math.Round(speed * Data.UnitOfSpeed * 3.6));
					}
				}
					break;
				case TrackCommand.Limit:
				{
					double limit = 0.0;
					int direction = 0, cource = 0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out limit))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						limit = 0.0;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out direction))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						direction = 0;
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out cource))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Cource is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						cource = 0;
					}

					if (limit < 0.0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed must be positive in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					int n = Data.Blocks[BlockIndex].Limits.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Limits, n + 1);
					
					Data.Blocks[BlockIndex].Limits[n] = new Limit(Data.TrackPosition, limit == 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit, direction, cource, 0);
				}
					break;
				case TrackCommand.Stop:
				case TrackCommand.StopPos:
					if (CurrentStation == -1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "A stop without a station is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						int dir = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out dir))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dir = 0;
						}

						double backw = 5.0, forw = 5.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							backw = 5.0;
						}
						else if (backw <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be positive in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							backw = 5.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							forw = 5.0;
						}
						else if (forw <= 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be positive in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							forw = 5.0;
						}

						int cars = 0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out cars))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Cars is invalid in Track.Stop at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							cars = 0;
						}

						int n = Data.Blocks[BlockIndex].StopPositions.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].StopPositions, n + 1);
						Data.Blocks[BlockIndex].StopPositions[n] = new Stop(Data.TrackPosition, CurrentStation, dir, forw, backw, cars);
						CurrentStop = cars;
					}

					break;
				case TrackCommand.Sta:
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (Arguments[1].Length == 1 && Arguments[1][0] == '.')
						{
							/* Treat a single period as a blank space */
						}
						else if (string.Equals(Arguments[1], "D", StringComparison.OrdinalIgnoreCase))
						{
							CurrentRoute.Stations[CurrentStation].Dummy = true;
						}
						else if (!TryParseTime(Arguments[1], out arr))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (Arguments[2].StartsWith("J:", StringComparison.InvariantCultureIgnoreCase))
						{
							string[] splitString = Arguments[2].Split(':');
							for (int i = 0; i < splitString.Length; i++)
							{
								switch (i)
								{
									case 1:
										if (!NumberFormats.TryParseIntVb6(splitString[1].TrimStart(), out CurrentRoute.Stations[CurrentStation].JumpIndex))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "JumpStationIndex is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dep = -1.0;
						}
					}

					int passalarm = 0;
					if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out passalarm))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PassAlarm is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						passalarm = 0;
					}

					Direction door = Direction.Both;
					if (Arguments.Length >= 5 && Arguments[4].Length != 0)
					{
						door = FindDirection(Arguments[4], "Track.Sta", false, Expression.Line, Expression.File);
						if (door == Direction.Invalid)
						{
							door = Direction.Both;
						}
					}

					int stop = 0;
					if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[5], out stop))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}

						if (device != 0 & device != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Path.CombineFile(SoundPath, Arguments[7]);
								if (!System.IO.File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "ArrivalSound " + f + " not found in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Plugin.CurrentHost.RegisterSound(f, radius, out arrsnd);
								}
							}
						}
					}

					double halt = 15.0;
					if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out halt))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "StopDuration is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						halt = 15.0;
					}
					else if (halt < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "StopDuration is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							jam = 100.0;
						}
						else if (jam < 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							jam = 100.0;
						}
					}

					if (!PreviewOnly)
					{
						if (Arguments.Length >= 11 && Arguments[10].Length > 0)
						{
							if (Path.ContainsInvalidChars(Arguments[10]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Path.CombineFile(SoundPath, Arguments[10]);
								if (!System.IO.File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Plugin.CurrentHost.RegisterSound(f, radius, out depsnd);
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									ttidx = -1;
								}
								else if (ttidx >= Data.TimetableDaytime.Length & ttidx >= Data.TimetableNighttime.Length)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TimetableIndex references textures not loaded in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenDoor = 0.0;
						}
						else if (reopenDoor < 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenDoor = 0.0;
						}
					}

					int reopenStationLimit = 5;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 14 && Arguments[13].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[13], out reopenStationLimit))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenStationLimit = 5;
						}
						else if (reopenStationLimit < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							reopenStationLimit = 0;
						}
					}

					double interferenceInDoor = Plugin.RandomNumberGenerator.NextDouble() * 30.0;
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 15 && Arguments[14].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[14], out interferenceInDoor))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							interferenceInDoor = Plugin.RandomNumberGenerator.NextDouble() * 30.0;
						}
						else if (interferenceInDoor < 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor is expected to be non-negative in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							interferenceInDoor = 0.0;
						}
					}

					int maxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 16 && Arguments[15].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[15], out maxInterferingObjectRate))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							maxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
						}
						else if (maxInterferingObjectRate <= 0 || maxInterferingObjectRate >= 100)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is expected to be positive, less than 100 in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							maxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
						}
					}

					CurrentRoute.Stations[CurrentStation].ArrivalTime = arr;
					CurrentRoute.Stations[CurrentStation].ArrivalSoundBuffer = arrsnd;
					CurrentRoute.Stations[CurrentStation].DepartureTime = dep;
					CurrentRoute.Stations[CurrentStation].DepartureSoundBuffer = depsnd;
					CurrentRoute.Stations[CurrentStation].StopTime = halt;
					CurrentRoute.Stations[CurrentStation].ForceStopSignal = stop == 1;
					CurrentRoute.Stations[CurrentStation].OpenLeftDoors = door == Direction.Left | door == Direction.Both;
					CurrentRoute.Stations[CurrentStation].OpenRightDoors = door == Direction.Right | door == Direction.Both;
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
					CurrentRoute.Stations[CurrentStation].JumpTime = -1;
					Data.Blocks[BlockIndex].Station = CurrentStation;
					Data.Blocks[BlockIndex].StationPassAlarm = passalarm == 1;
					CurrentStop = -1;
					DepartureSignalUsed = false;
					//Detect common BVE2 / BVE4 dummy stations, missing names etc.
					if (CurrentRoute.Stations[CurrentStation].Name.Length == 0 & (CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.AllStop))
					{
						if (CurrentRoute.Stations[CurrentStation].OpenLeftDoors || CurrentRoute.Stations[CurrentStation].OpenRightDoors)
						{
							CurrentRoute.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture);
						}
						else if (CurrentRoute.Stations[CurrentStation].ForceStopSignal)
						{
							CurrentRoute.Stations[CurrentStation].Dummy = true;
						}
					}
				}
					break;
				case TrackCommand.Station:
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								arr = -1.0;
							}
						}
						else if (!TryParseTime(Arguments[1], out arr))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								dep = -1.0;
							}
						}
						else if (!TryParseTime(Arguments[2], out dep))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							dep = -1.0;
						}
					}

					int stop = 0;
					if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out stop))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						stop = 0;
					}

					int device = 0;
					if (Arguments.Length >= 5 && Arguments[4].Length > 0)
					{
						if (string.Compare(Arguments[4], "ats", StringComparison.OrdinalIgnoreCase) == 0 || (Plugin.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("ats", StringComparison.OrdinalIgnoreCase)))
						{
							device = 0;
						}
						else if (string.Compare(Arguments[4], "atc", StringComparison.OrdinalIgnoreCase) == 0 || (Plugin.CurrentOptions.EnableBveTsHacks && Arguments[4].StartsWith("atc", StringComparison.OrdinalIgnoreCase)))
						{
							device = 1;
						}
						else if (!NumberFormats.TryParseIntVb6(Arguments[4], out device))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is invalid in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							device = 0;
						}
						else if (device != 0 & device != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "System is not supported in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureSound contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Path.CombineFile(SoundPath, Arguments[5]);
								if (!System.IO.File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "DepartureSound " + f + " not found in Track.Station at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									const double radius = 30.0;
									Plugin.CurrentHost.RegisterSound(f, radius, out depsnd);
								}
							}
						}
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
					//Detect common BVE2 / BVE4 dummy stations, missing names etc.
					if (CurrentRoute.Stations[CurrentStation].Name.Length == 0 & (CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.AllStop))
					{
						//Set default name
						CurrentRoute.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture);
						if (CurrentRoute.Stations[CurrentStation].ForceStopSignal)
						{
							if (IsRW)
							{
								/*
								 * NOTE: The Track.Sta command is not valid in RW format routes (and this is what allows the
								 * door direction to be set). Let's assume that no name and a forced red signal
								 * is actualy a signalling control station
								 * e.g. Rocky Mountains Express
								 *
								 * However, the Station format command can *also* be used in a CSV route,
								 * where according to the documentation, both doors are assumed to open. Assume that in this case
								 * it was deliberate....
								 */
								CurrentRoute.Stations[CurrentStation].Dummy = true;
								CurrentRoute.Stations[CurrentStation].Name = string.Empty;
								CurrentRoute.Stations[CurrentStation].OpenLeftDoors = false;
								CurrentRoute.Stations[CurrentStation].OpenRightDoors = false;
							}
						}
					}
				}
					break;
				case TrackCommand.StationXML:
					string fn = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
					if (!System.IO.File.Exists(fn))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, true, "Station XML file " + fn + " not found in Track.StationXML at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					CurrentStation++;
					Array.Resize(ref CurrentRoute.Stations, CurrentStation + 1);
					CurrentRoute.Stations[CurrentStation] = new RouteStation();
					StopRequest sr = new StopRequest();
					sr.TrackPosition = Data.TrackPosition;
					sr.StationIndex = CurrentStation;
					CurrentRoute.Stations[CurrentStation] = StationXMLParser.ReadStationXML(CurrentRoute, fn, PreviewOnly, Data.TimetableDaytime, Data.TimetableNighttime, CurrentStation, ref Data.Blocks[BlockIndex].StationPassAlarm, ref sr);
					if (CurrentRoute.Stations[CurrentStation].Type == StationType.RequestStop)
					{
						Data.RequestStops.Add(sr);
					}

					Data.Blocks[BlockIndex].Station = CurrentStation;
					break;
				case TrackCommand.Buffer:
				{
					if (!PreviewOnly)
					{
						CurrentRoute.BufferTrackPositions.Add(new BufferStop(0, Data.TrackPosition, true));
					}
				}
					break;
				case TrackCommand.Form:
				{
					if (!PreviewOnly)
					{
						int idx1 = 0, idx2 = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (idx2 < 0 & idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be greater or equal to -2 in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx1) || !Data.Blocks[BlockIndex].Rails[idx1].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex1 could be out of range in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR && (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx2) || !Data.Blocks[BlockIndex].Rails[idx2].RailStarted))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex2 could be out of range in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							int roof = 0, pf = 0;
							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out roof))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roof = 0;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out pf))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex is invalid in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pf = 0;
							}

							if (roof != 0 & (roof < 0 || (!Data.Structure.RoofL.ContainsKey(roof) && !Data.Structure.RoofR.ContainsKey(roof))))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex " + roof + " references an object not loaded in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (pf < 0 | (!Data.Structure.FormL.ContainsKey(pf) & !Data.Structure.FormR.ContainsKey(pf)))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex " + pf + " references an object not loaded in Track.Form at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							int n = Data.Blocks[BlockIndex].Forms.Length;
							Array.Resize(ref Data.Blocks[BlockIndex].Forms, n + 1);
							Data.Blocks[BlockIndex].Forms[n] = new Form(idx1, idx2, pf, roof, Data.Structure, Expression.File);
						}
					}
				}
					break;
				case TrackCommand.Pole:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (idx >= Data.Blocks[BlockIndex].RailPole.Length)
							{
								Array.Resize(ref Data.Blocks[BlockIndex].RailPole, idx + 1);
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AdditionalRailsCovered is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									typ = 0;
								}
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0)
							{
								if (!NumberFormats.TryParseDoubleVb6(Arguments[2], out double loc))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Location is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									loc = 0.0;
								}

								Data.Blocks[BlockIndex].RailPole[idx].Location = loc;
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0)
							{
								if (!NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out double dist))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Interval is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dist = Data.BlockInterval;
								}

								Data.Blocks[BlockIndex].RailPole[idx].Interval = dist;
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0)
							{
								if (!NumberFormats.TryParseIntVb6(Arguments[4], out sttype))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex is invalid in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									sttype = 0;
								}
							}

							if (typ < 0 || !Data.Structure.Poles.ContainsKey(typ) || Data.Structure.Poles[typ] == null)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (sttype < 0 || !Data.Structure.Poles[typ].ContainsKey(sttype) || Data.Structure.Poles[typ][sttype] == null)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex " + typ + " references an object not loaded in Track.Pole at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
				case TrackCommand.PoleEnd:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailPole.Length)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing pole in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.PoleEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Data.Blocks[BlockIndex].RailPole[idx].Exists = false;
						}
					}
				}
					break;
				case TrackCommand.Wall:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						Direction dir = Direction.Invalid;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							dir = FindDirection(Arguments[1], "Track.Wall", true, Expression.Line, Expression.File);
						}

						if (dir == Direction.Invalid || dir == Direction.None)
						{
							break;
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex is invalid in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex is expected to be a non-negative integer in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (dir < 0 && !Data.Structure.WallL.ContainsKey(sttype) || dir > 0 && !Data.Structure.WallR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.WallL.ContainsKey(sttype) && !Data.Structure.WallR.ContainsKey(sttype)))
						{
							if (dir < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (dir > 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallR at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
						}
						else
						{
							if (dir == Direction.Both)
							{
								if (!Data.Structure.WallL.ContainsKey(sttype))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "LeftWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dir = Direction.Right;
								}

								if (!Data.Structure.WallR.ContainsKey(sttype))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RightWallStructureIndex " + sttype + " references an object not loaded in Track.WallBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dir = Direction.Left;
								}
							}

							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Wall at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Data.Blocks[BlockIndex].RailWall.ContainsKey(idx))
							{
								Data.Blocks[BlockIndex].RailWall[idx] = new WallDike(sttype, dir, Data.Structure.WallL, Data.Structure.WallR);
							}
							else
							{
								Data.Blocks[BlockIndex].RailWall.Add(idx, new WallDike(sttype, dir, Data.Structure.WallL, Data.Structure.WallR));
							}
						}
					}
				}
					break;
				case TrackCommand.WallEnd:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (!Data.Blocks[BlockIndex].RailWall.ContainsKey(idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing wall in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.WallEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Data.Blocks[BlockIndex].RailWall.ContainsKey(idx))
							{
								Data.Blocks[BlockIndex].RailWall[idx].Exists = false;
							}
						}
					}
				}
					break;
				case TrackCommand.Dike:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						Direction dir = Direction.Invalid;

						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							dir = FindDirection(Arguments[1], "Track.Dike", true, Expression.Line, Expression.File);
						}

						if (dir == Direction.Invalid || dir == Direction.None)
						{
							break;
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex is invalid in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex is expected to be a non-negative integer in Track.DikeL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (dir < 0 && !Data.Structure.DikeL.ContainsKey(sttype) || dir > 0 && !Data.Structure.DikeR.ContainsKey(sttype) || dir == 0 && (!Data.Structure.DikeL.ContainsKey(sttype) && !Data.Structure.DikeR.ContainsKey(sttype)))
						{
							if (dir > 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeL at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (dir < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeR at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DikeStructureIndex " + sttype + " references an object not loaded in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (dir == Direction.Both)
							{
								if (!Data.Structure.DikeL.ContainsKey(sttype))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "LeftDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dir = Direction.Right;
								}

								if (!Data.Structure.DikeR.ContainsKey(sttype))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RightDikeStructureIndex " + sttype + " references an object not loaded in Track.DikeBothSides at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									dir = Direction.Left;
								}
							}

							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.Dike at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Data.Blocks[BlockIndex].RailDike.ContainsKey(idx))
							{
								Data.Blocks[BlockIndex].RailDike[idx] = new WallDike(sttype, dir, Data.Structure.DikeL, Data.Structure.DikeR);
							}
							else
							{
								Data.Blocks[BlockIndex].RailDike.Add(idx, new WallDike(sttype, dir, Data.Structure.DikeL, Data.Structure.DikeR));
							}
						}

					}
				}
					break;
				case TrackCommand.DikeEnd:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (!Data.Blocks[BlockIndex].RailDike.ContainsKey(idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " does not reference an existing dike in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || (!Data.Blocks[BlockIndex].Rails[idx].RailStarted & !Data.Blocks[BlockIndex].Rails[idx].RailEnded))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.DikeEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Data.Blocks[BlockIndex].RailDike.ContainsKey(idx))
							{
								Data.Blocks[BlockIndex].RailDike[idx].Exists = false;
							}
						}
					}
				}
					break;
				case TrackCommand.Marker:
				case TrackCommand.TextMarker:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Marker is expected to have at least one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Path.ContainsInvalidChars(Arguments[0]))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							string f = Path.CombineFile(ObjectPath, Arguments[0]);
							if (!System.IO.File.Exists(f) && Command == TrackCommand.Marker)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in Track.Marker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (System.IO.File.Exists(f) && f.ToLowerInvariant().EndsWith(".xml"))
								{
									if (MarkerScriptParser.ReadMarkerXML(f, Data.TrackPosition, out Marker m))
									{
										Data.Markers.Add(m);
									}
									break;
								}

								double dist = Data.BlockInterval;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out dist))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Distance is invalid in Track.Marker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								AbstractMessage message;
								if (Command == TrackCommand.TextMarker)
								{
									message = new MarkerText(Arguments[0]);
									if (Arguments.Length >= 3)
									{
										switch (Arguments[2].ToLowerInvariant())
										{
											case "black":
											case "1":
												message.Color = MessageColor.Black;
												break;
											case "gray":
											case "2":
												message.Color = MessageColor.Gray;
												break;
											case "white":
											case "3":
												message.Color = MessageColor.White;
												break;
											case "red":
											case "4":
												message.Color = MessageColor.Red;
												break;
											case "orange":
											case "5":
												message.Color = MessageColor.Orange;
												break;
											case "green":
											case "6":
												message.Color = MessageColor.Green;
												break;
											case "blue":
											case "7":
												message.Color = MessageColor.Blue;
												break;
											case "magenta":
											case "8":
												message.Color = MessageColor.Magenta;
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageColor is invalid in Track.TextMarker at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
												//Default message color is set to white
												break;
										}
									}
								}
								else
								{
									Color24? transparentColor = new Color24(64, 64, 64);
									if (Plugin.CurrentOptions.EnableBveTsHacks && IsRw)
									{
										/*
										 * RW routes were only for BVE1 / BVE2
										 * This had markers in a black sidebar, so transparency should not be used here as it may produce odd results
										 */
										transparentColor = null;
									}
									Plugin.CurrentHost.RegisterTexture(f, new OpenBveApi.Textures.TextureParameters(null, transparentColor), out OpenBveApi.Textures.Texture t);
									
									message = new MarkerImage(Plugin.CurrentHost, t);
								}

								Data.Markers.Add(new Marker(start, end, message));
							}
						}
					}
				}
					break;
				case TrackCommand.Height:
				{
					if (!PreviewOnly)
					{
						double h = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out h))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Height is invalid in Track.Height at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							h = 0.0;
						}

						Data.Blocks[BlockIndex].Height = IsRW ? h + 0.3 : h;
					}
				}
					break;
				case TrackCommand.Ground:
				{
					if (!PreviewOnly)
					{
						int cytype = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out cytype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CycleIndex is invalid in Track.Ground at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CycleIndex " + cytype + " references an object not loaded in Track.Ground at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].Cycle = new[] { cytype };
							}
						}
					}
				}
					break;
				case TrackCommand.Crack:
				{
					if (!PreviewOnly)
					{
						int idx1 = 0, idx2 = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx1))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx1 = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out idx2))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx2 = 0;
						}

						int sttype = 0;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex is invalid in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (sttype < 0 || !Data.Structure.CrackL.ContainsKey(sttype) || !Data.Structure.CrackR.ContainsKey(sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex " + sttype + " references an object not loaded in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (idx1 < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (idx2 < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be non-negative in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (idx1 == idx2)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex1 is expected to be unequal to Index2 in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx1) || !Data.Blocks[BlockIndex].Rails[idx1].RailStarted)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx2) || !Data.Blocks[BlockIndex].Rails[idx2].RailStarted)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex2 " + idx2 + " could be out of range in Track.Crack at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								int n = Data.Blocks[BlockIndex].Cracks.Length;
								Array.Resize(ref Data.Blocks[BlockIndex].Cracks, n + 1);
								Data.Blocks[BlockIndex].Cracks[n] = new Crack(idx1, idx2, sttype, Expression.File);
							}
						}
					}
				}
					break;
				case TrackCommand.FreeObj:
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An insufficient number of arguments was supplied in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							return;
						}

						int idx = 0, sttype = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out sttype))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							sttype = 0;
						}

						if (idx < -1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative or -1 in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (sttype < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is expected to be non-negative in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (idx >= 0 && (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (!Data.Structure.FreeObjects.ContainsKey(sttype))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex " + sttype + " references an object not loaded in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Vector2 objectPosition = new Vector2();
								double yaw = 0.0, pitch = 0.0, roll = 0.0;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out objectPosition.X))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out objectPosition.Y))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									yaw = 0.0;
								}

								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									pitch = 0.0;
								}

								if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.FreeObj at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									roll = 0.0;
								}

								if (idx == -1)
								{

									if (!Data.IgnorePitchRoll)
									{
										Data.Blocks[BlockIndex].GroundFreeObj.Add(new FreeObj(Data.TrackPosition, sttype, objectPosition, yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians()));
									}
									else
									{
										Data.Blocks[BlockIndex].GroundFreeObj.Add(new FreeObj(Data.TrackPosition, sttype, objectPosition, yaw.ToRadians()));
									}
								}
								else
								{
									if (!Data.Blocks[BlockIndex].RailFreeObj.ContainsKey(idx))
									{
										Data.Blocks[BlockIndex].RailFreeObj.Add(idx, new List<FreeObj>());
									}

									if (!Data.IgnorePitchRoll)
									{
										Data.Blocks[BlockIndex].RailFreeObj[idx].Add(new FreeObj(Data.TrackPosition, sttype, objectPosition, yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians()));
									}
									else
									{
										Data.Blocks[BlockIndex].RailFreeObj[idx].Add(new FreeObj(Data.TrackPosition, sttype, objectPosition, yaw.ToRadians()));
									}
								}
							}
						}
					}
				}
					break;
				case TrackCommand.Back:
				case TrackCommand.Background:
				{
					if (!PreviewOnly)
					{
						int typ = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out typ))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							typ = 0;
						}

						if (typ < 0 | !Data.Backgrounds.ContainsKey(typ))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex " + typ + " references a background not loaded in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Data.Backgrounds[typ] is StaticBackground)
						{
							StaticBackground bg = Data.Backgrounds[typ] as StaticBackground;
							if (bg.Texture == null)
							{
								//There's a possibility that this was loaded via a default BVE command rather than XML
								//Thus check for the existance of the file and chuck out error if appropriate
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundIndex " + typ + " has not been loaded via Texture.Background in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].Background = typ;
								if (Plugin.CurrentOptions.EnableBveTsHacks && Data.Blocks.Count == 2 && Data.Blocks[0].Background == 0)
								{
									//The initial background for block 0 is always set to zero
									//This handles the case where background idx #0 is not used
									if (Data.Backgrounds.TryGetValue(0, out BackgroundHandle backgroundZero))
									{
										bg = backgroundZero as StaticBackground;
										if (bg.Texture == null)
										{
											Data.Blocks[0].Background = typ;
										}
									}
									else
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
				case TrackCommand.Announce:
				case TrackCommand.AnnounceAll:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateSound(ref f, SoundPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									double speed = 0.0;
									if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out speed))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										speed = 0.0;
									}

									int n = Data.Blocks[BlockIndex].SoundEvents.Length;
									Array.Resize(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
									if (Command == TrackCommand.AnnounceAll)
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, speed * Data.UnitOfSpeed, new Vector2(), 0, 0, true);
									}
									else
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, speed * Data.UnitOfSpeed);
									}

								}
							}
						}
					}
				}
					break;
				case TrackCommand.RailAnnounce:
				case TrackCommand.RailAnnounceAll:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length < 2)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 2 and 3 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int railIndex))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid rail index for " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								break;
							}
							if (Path.ContainsInvalidChars(Arguments[1]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[1] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[1];
								if (!LocateSound(ref f, SoundPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									double speed = 0.0;
									if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out speed))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										speed = 0.0;
									}
									int n = Data.Blocks[BlockIndex].SoundEvents.Length;
									Array.Resize(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
									if (Command == TrackCommand.RailAnnounceAll)
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, speed * Data.UnitOfSpeed, new Vector2(), 0, 0, true, railIndex);
									}
									else
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, speed * Data.UnitOfSpeed, new Vector2(), 0, 0, false, railIndex);
									}
								}
							}
						}
					}
				}
					break;
				case TrackCommand.Doppler:
				case TrackCommand.DopplerAll:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have between 1 and 3 arguments at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateSound(ref f, SoundPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									double x = 0.0, y = 0.0;
									if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										x = 0.0;
									}

									if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										y = 0.0;
									}

									int n = Data.Blocks[BlockIndex].SoundEvents.Length;
									Array.Resize(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
									if (Command == TrackCommand.DopplerAll)
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, -1, new Vector2(x, y), 0, 0, true);
									}
									else
									{
										Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, f, -1, new Vector2(x, y));
									}
								}
							}
						}
					}
				}
					break;
				case TrackCommand.MicSound:
				{
					if (!PreviewOnly)
					{
						double x = 0.0, y = 0.0, back = 0.0, front = 0.0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out back))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							back = 0.0;
						}
						else if (back < 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackwardTolerance is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							back = 0.0;
						}

						if (Arguments.Length >= 4 && Arguments[3].Length > 0 & !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out front))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							front = 0.0;
						}
						else if (front < 0.0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForwardTolerance is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							front = 0.0;
						}

						int n = Data.Blocks[BlockIndex].SoundEvents.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].SoundEvents, n + 1);
						Data.Blocks[BlockIndex].SoundEvents[n] = new Sound(Data.TrackPosition, string.Empty, -1, new Vector2(x, y), front, back);
					}
				}
					break;
				case TrackCommand.PreTrain:
				{
					if (!PreviewOnly)
					{
						if (Arguments.Length == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have exactly 1 argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments[0].Length > 0 & !TryParseTime(Arguments[0], out double time))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Time is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								time = 0.0;
							}

							int n = CurrentRoute.BogusPreTrainInstructions.Length;
							if (n != 0 && CurrentRoute.BogusPreTrainInstructions[n - 1].Time >= time)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Time is expected to be in ascending order between successive " + Command + " commands at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							Array.Resize(ref CurrentRoute.BogusPreTrainInstructions, n + 1);
							CurrentRoute.BogusPreTrainInstructions[n].TrackPosition = Data.TrackPosition;
							CurrentRoute.BogusPreTrainInstructions[n].Time = time;
						}
					}
				}
					break;
				case TrackCommand.PointOfInterest:
				case TrackCommand.POI:
				{
					if (!PreviewOnly)
					{
						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							idx = 0;
						}

						if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex " + idx + " references a non-existing rail in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						double x = 0.0, y = 0.0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							x = 0.0;
						}

						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							y = 0.0;
						}

						double yaw = 0.0, pitch = 0.0, roll = 0.0;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out yaw))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							yaw = 0.0;
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out pitch))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							pitch = 0.0;
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out roll))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							roll = 0.0;
						}

						string text = null;
						if (Arguments.Length >= 7 && Arguments[6].Length != 0)
						{
							text = Arguments[6];
						}

						int n = Data.Blocks[BlockIndex].PointsOfInterest.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].PointsOfInterest, n + 1);
						Data.Blocks[BlockIndex].PointsOfInterest[n] = new PointOfInterest(Data.TrackPosition, idx, text, new Vector2(x, y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
					}
				}
					break;
				case TrackCommand.HornBlow:
					if (!PreviewOnly)
					{
						int type = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out type))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "HornType is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							type = 0;
						}

						if (type < 0 || type > 2)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "HornType is expected to be in the range of -1 to 1 in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							int structure = 0, triggerOnce = 0;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = 0;
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out triggerOnce))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (structure < -1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative or -1 in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}
							else if (structure >= 0 && !Data.Structure.Beacon.ContainsKey(structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex " + structure + " references an object not loaded in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								structure = -1;
							}

							if (triggerOnce < 0 || triggerOnce > 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TriggerOnce is expected to be in the range of 0 to 1 in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								triggerOnce = 0;
							}

							double x = 0.0, y = 0.0;
							double yaw = 0.0, pitch = 0.0, roll = 0.0;
							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out x))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								x = 0.0;
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], UnitOfLength, out y))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								y = 0.0;
							}

							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Yaw is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								yaw = 0.0;
							}

							if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out pitch))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Pitch is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								pitch = 0.0;
							}

							if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out roll))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Roll is invalid in Track.HornBlow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								roll = 0.0;
							}

							int n = Data.Blocks[BlockIndex].HornBlows.Length;
							Array.Resize(ref Data.Blocks[BlockIndex].HornBlows, n + 1);
							Data.Blocks[BlockIndex].HornBlows[n] = new HornBlowEvent(Data.TrackPosition, (HornTypes)type, triggerOnce != 0, structure, new Vector2(x, y), yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
						}
					}

					break;
				case TrackCommand.Rain:
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 1 && Arguments[0].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int currentIntensity))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Intensity is invalid in Track.Rain at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].RainIntensity = currentIntensity;
							}
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[1], out int structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WeatherStructureIndex is invalid in Track.Rain at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Structure.WeatherObjects.ContainsKey(structure))
								{
									Data.Blocks[BlockIndex].WeatherObject = structure;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WeatherStructureIndex " + structure + " was not found in Track.Rain at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

							}
						}
					}

					break;
				case TrackCommand.Snow:
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 1 && Arguments[0].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int currentIntensity))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Intensity is invalid in Track.Snow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								Data.Blocks[BlockIndex].SnowIntensity = currentIntensity;
							}
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[1], out int structure))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WeatherStructureIndex is invalid in Track.Snow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Structure.WeatherObjects.ContainsKey(structure))
								{
									Data.Blocks[BlockIndex].WeatherObject = structure;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WeatherStructureIndex " + structure + " was not found in Track.Snow at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}

							}
						}
					}

					break;
				case TrackCommand.DynamicLight:
					if (!PreviewOnly)
					{
						if (Arguments.Length >= 1 && Arguments[0].Length > 0)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int currentLightSet))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DynamicLightIndex is invalid in Track.DynamicLight at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{

								int l = Data.Blocks[BlockIndex].LightingChanges.Length;
								Array.Resize(ref Data.Blocks[BlockIndex].LightingChanges, l + 1);
								Data.Blocks[BlockIndex].LightingChanges[l] = new LightingChange(Data.Blocks[BlockIndex].DynamicLightDefinition, currentLightSet);
								Data.Blocks[BlockIndex].DynamicLightDefinition = currentLightSet;
							}
						}
					}

					break;
				case TrackCommand.DirectionalLight:
					if (!PreviewOnly)
					{
						if (Plugin.CurrentRoute.DynamicLighting)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Track.DirectionalLight will be ignored");
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

						int l = Data.Blocks[BlockIndex].LightingChanges.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].LightingChanges, l + 1);
						LightDefinition ld = Data.Blocks[BlockIndex].LightDefinition;
						ld.DiffuseColor = new Color24((byte)r, (byte)g, (byte)b);
						Data.Blocks[BlockIndex].LightingChanges[l] = new LightingChange(Data.Blocks[BlockIndex].LightDefinition, ld);
						Data.Blocks[BlockIndex].LightDefinition = ld;
					}

					break;
				case TrackCommand.AmbientLight:
					if (!PreviewOnly)
					{
						if (Plugin.CurrentRoute.DynamicLighting)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Dynamic lighting is enabled- Track.AmbientLight will be ignored");
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
						
						int l = Data.Blocks[BlockIndex].LightingChanges.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].LightingChanges, l + 1);
						LightDefinition ld = Data.Blocks[BlockIndex].LightDefinition;
						ld.AmbientColor = new Color24((byte)r, (byte)g, (byte)b);
						Data.Blocks[BlockIndex].LightingChanges[l] = new LightingChange(Data.Blocks[BlockIndex].LightDefinition, ld);
						Data.Blocks[BlockIndex].LightDefinition = ld;
					}

					break;
				case TrackCommand.LightDirection:
					if (!PreviewOnly)
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
						int l = Data.Blocks[BlockIndex].LightingChanges.Length;
						Array.Resize(ref Data.Blocks[BlockIndex].LightingChanges, l + 1);
						LightDefinition ld = Data.Blocks[BlockIndex].LightDefinition;
						ld.LightPosition = new Vector3((float)-dx, (float)-dy, (float)-dz);
						Data.Blocks[BlockIndex].LightingChanges[l] = new LightingChange(Data.Blocks[BlockIndex].LightDefinition, ld);
						Data.Blocks[BlockIndex].LightDefinition = ld;
					}

					break;
				case TrackCommand.PatternObj:
					/*
					 * PatternIndex
					 * PlacementInterval
					 * BlockInterval
					 * X
					 * Y
					 * Types array
					 */
					if (!PreviewOnly)
					{
						if (!IsHmmsim)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "PatternObj command detected, but this does not appear to be a Hmmsim route");
						}

						int idx = -1;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Index is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						PatternObj patternObj = new PatternObj(RailIndex);

						if (Data.Blocks[BlockIndex].PatternObjs.ContainsKey(idx))
						{
							patternObj = Data.Blocks[BlockIndex].PatternObjs[idx];
						}

						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out patternObj.Interval))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Repetition interval is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							patternObj.Interval = 25; // try the default BVE block-length
						}

						if (patternObj.Interval <= 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Repetition interval must be non-zero in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out patternObj.Interval))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Repetition interval is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								patternObj.Interval = 25; // try the default BVE block-length
							}
						}


						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out patternObj.Position.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "X position is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out patternObj.Position.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Y position is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						Array.Resize(ref patternObj.Types, Arguments.Length - 6);
						for (int i = 6; i < Arguments.Length; i++)
						{
							if (!NumberFormats.TryParseIntVb6(Arguments[i], out patternObj.Types[i - 6]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ObjectIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								break;
							}
						}

						patternObj.LastPlacement = Data.TrackPosition;
						Data.Blocks[BlockIndex].PatternObjs[idx] = patternObj;
					}

					break;
				case TrackCommand.PatternEnd:
					if (!PreviewOnly)
					{
						if (!IsHmmsim)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "PatternEnd command detected, but this does not appear to be a Hmmsim route");
						}

						int idx = -1;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Index is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (!Data.Blocks[BlockIndex].PatternObjs.ContainsKey(idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to stop the non-existant pattern with index " + idx + " in command " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							Data.Blocks[BlockIndex].PatternObjs[idx].Ends = true;
						}
					}

					break;
				case TrackCommand.Switch:
				case TrackCommand.SwitchT:
				{
						Data.SwitchUsed = true;
						bool trailing = Command == TrackCommand.SwitchT;

						int idx = 0;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						int idx1 = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out idx1))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (idx1 < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						int initialSetting = idx;
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out initialSetting))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Initial setting for Switch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						if (initialSetting < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Initial setting for Switch is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							initialSetting = idx;
						}

						bool springReturn = Plugin.CurrentOptions.Derailments;
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !bool.TryParse(Arguments[3], out springReturn))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Spring return setting for Switch is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						string switchName = string.Empty;
						string[] trackNames = new string[]
						{
							string.Empty,
							string.Empty
						};

						if (Arguments.Length >= 5 && Arguments[4].Length > 0)
						{
							switchName = Arguments[4];
						}

						if (Arguments.Length >= 6 && Arguments[5].Length > 0)
						{
							trackNames[0] = Arguments[5];
						}

						if (Arguments.Length >= 7 && Arguments[6].Length > 0)
						{
							trackNames[1] = Arguments[6];
						}

						if (Data.Blocks[BlockIndex].Switches.Length <= idx)
						{
							Array.Resize(ref Data.Blocks[BlockIndex].Switches, idx + 1);
						}

						Data.Blocks[BlockIndex].Switches[idx] = new Switch
						{
							SecondTrack = idx1,
							InitialSetting = initialSetting,
							Trailing = trailing,
							SpringReturn = springReturn,
							Name = switchName,
							TrackNames = trackNames
						};
						if (Command == TrackCommand.Switch)
						{
							if (idx != 0)
							{
								int block = BlockIndex;
								while (Data.Blocks[block].Rails.ContainsKey(idx) && block > 0)
								{
									Data.Blocks[block].Rails[idx].IsDriveable = true;
									if (Data.Blocks[block].Rails[idx].RailEnded)
									{
										break;
									}
									block--;
								}
							}
							if (idx1 != 0)
							{
								int block = BlockIndex;
								while (Data.Blocks[block].Rails.ContainsKey(idx1) && block > 0)
								{
									Data.Blocks[block].Rails[idx1].IsDriveable = true;
									if (Data.Blocks[block].Rails[idx1].RailEnded)
									{
										break;
									}
									block--;
								}
							}
						}
						if (Command == TrackCommand.SwitchT)
						{
							if (idx != 0)
							{
								int block = BlockIndex;
								while (Data.Blocks[block].Rails.ContainsKey(idx) && block > 0)
								{
									Data.Blocks[block].Rails[idx].IsDriveable = true;
									if (Data.Blocks[block].Rails[idx].RailStarted)
									{
										break;
									}
									block--;
								}
							}
							if (idx1 != 0)
							{
								int block = BlockIndex;
								while (Data.Blocks[block].Rails.ContainsKey(idx1) && block > 0)
								{
									Data.Blocks[block].Rails[idx1].IsDriveable = true;
									if (Data.Blocks[block].Rails[idx1].RailStarted)
									{
										break;
									}
									block--;
								}
							}
						}
				}
					break;
				case TrackCommand.PlayerPath:
				{
					int idx = 0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					if (idx < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					int idx1 = 0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out idx1))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}
					
					if (idx1 < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is expected to be positive in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					string switchName = string.Empty;
					if (Arguments.Length >= 3 && Arguments[2].Length > 0)
					{
						switchName = Arguments[2];
					}

					if (idx != 0)
					{
						int block = BlockIndex;
						while (Data.Blocks[block].Rails.ContainsKey(idx) && block > 0)
						{
							Data.Blocks[block].Rails[idx].IsDriveable = true;
							if (Data.Blocks[block].Rails[idx].RailEnded)
							{
								break;
							}
							block--;
						}
					}
					if (idx1 != 0)
					{
						int block = BlockIndex;
						while (Data.Blocks[block].Rails.ContainsKey(idx1) && block > 0)
						{
							Data.Blocks[block].Rails[idx1].IsDriveable = true;
							if (Data.Blocks[block].Rails[idx1].RailEnded)
							{
								break;
							}
							block--;
						}
					}
					if (Data.Blocks[BlockIndex].Switches.Length <= idx)
					{
						Array.Resize(ref Data.Blocks[BlockIndex].Switches, idx + 1);
					}
					Data.Blocks[BlockIndex].Switches[idx] = new Switch
					{
						SecondTrack = idx1,
						FixedRoute = true,
						InitialSetting = idx1,
						Name = switchName
					};
				}
					break;
				case TrackCommand.RailLimit:
				{
					int railIndex = -1;
					double limit = 0.0;
					int direction = 0, cource = 0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out railIndex) || railIndex == -1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.RailLimit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out limit))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Limit is invalid in Track.RailLimit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out direction))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Direction is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						direction = 0;
					}

					if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out cource))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Cource is invalid in Track.Limit at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						cource = 0;
					}

					int n = Data.Blocks[BlockIndex].Limits.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Limits, n + 1);
					Data.Blocks[BlockIndex].Limits[n] = new Limit(Data.TrackPosition, limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit, direction, cource, railIndex);
					
				}
					break;
				case TrackCommand.RailBuffer:
				{
					if (!PreviewOnly)
					{
						int railIndex = -1;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out railIndex) || railIndex == -1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.RailBuffer at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							break;
						}

						int affectsAI = 0;
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out affectsAI))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Limit is invalid in Track.RailBuffer at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}

						if (affectsAI != 0 && affectsAI != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AffectsAI is invalid in Track.RailBuffer at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							affectsAI = 0;
						}

						Plugin.CurrentRoute.BufferTrackPositions.Add(new BufferStop(railIndex, Data.TrackPosition, affectsAI != 0));
					}
				}
					break;
				case TrackCommand.RailAccuracy:
				{
					double acc = 2.0;
					int railIndex = -1;

					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out railIndex) || railIndex == -1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.RailAccuracy at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out acc))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						acc = 2.0;
					}
					
					if (acc < 0.0)
					{
						acc = 0.0;
					}
					else if (acc > 4.0)
					{
						acc = 4.0;
					}
					Data.Blocks[BlockIndex].Rails[RailIndex].Accuracy = acc;
				}
					break;
				case TrackCommand.RailAdhesion:
				{
					double a = 100.0;
					int railIndex = -1;


					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out railIndex) || railIndex == -1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.RailAccuracy at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						break;
					}

					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out a))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100;
					}

					if (a < 0.0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						a = 100.0;
					}

					Data.Blocks[BlockIndex].Rails[railIndex].AdhesionMultiplier = 0.01 * a;
				}
					break;
				case TrackCommand.PowerSupply:
					{
						if (!PreviewOnly)
						{
							int idx = 0;
							if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								idx = 0;
							}

							if (idx < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								idx = 0;
							}

							PowerSupplyTypes type = PowerSupplyTypes.None;
							PowerSupplyVoltageTypes voltageType = PowerSupplyVoltageTypes.AC;
							double voltage = 25000.0;
							double amps = 1000;
							double contactHeight = 5.3;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0)
							{
								switch (Arguments[1].ToUpperInvariant().Trim(new char[] { }))
								{
									case "OHLE":
									case "1":
										type = PowerSupplyTypes.OverheadLine;
										break;
									case "THIRDRAIL":
									case "2":
										type = PowerSupplyTypes.ThirdRail;
										voltageType = PowerSupplyVoltageTypes.DC;
										voltage = 750;
										contactHeight = 0;
										amps = 3000;
										break;
									case "FOURTHRAIL":
									case "3":
										type = PowerSupplyTypes.FourthRail;
										voltageType = PowerSupplyVoltageTypes.DC;
										voltage = 750;
										contactHeight = 0;
										amps = 3000;
										break;
									default:
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid type in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										break;
								}
							}

							if (Arguments.Length >= 3 && Arguments[2].Length > 0)
							{
								switch (Arguments[2].ToUpperInvariant().Trim(new char[] { }))
								{
									case "AC":
									case "0":
										voltageType = PowerSupplyVoltageTypes.AC;
										break;
									case "DC":
									case "1":
										voltageType = PowerSupplyVoltageTypes.DC;
										break;
									default:
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid voltage type in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										break;
								}
							}

							if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out voltage))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Voltage is invalid in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out amps))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Amperage is invalid in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out contactHeight))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Contact height is invalid in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							if (!Data.Blocks[BlockIndex].Rails.ContainsKey(idx) || !Data.Blocks[BlockIndex].Rails[idx].RailStarted)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "RailIndex " + idx + " could be out of range in Track.PowerSupply at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}

							
							if (type != PowerSupplyTypes.None)
							{
								PowerSupply p = new PowerSupply(voltageType, voltage, amps, contactHeight);
								Data.Blocks[BlockIndex].Rails[idx].PowerSupplies[type] = p;
							}
						}
					}
					break;
				case TrackCommand.PowerSupplyEnd:
					{
						if (!PreviewOnly)
						{
							int idx = 0;
							if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out idx))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is invalid in Track.PowerSupplyEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								idx = 0;
							}

							if (idx < 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex is expected to be a non-negative integer in Track.PowerSupplyEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								idx = 0;
							}

							PowerSupplyTypes type = PowerSupplyTypes.None;
							if (Arguments.Length >= 2 && Arguments[1].Length > 0)
							{
								switch (Arguments[1].ToUpperInvariant().Trim(new char[] { }))
								{
									case "OHLE":
									case "1":
										type = PowerSupplyTypes.OverheadLine;
										break;
									case "THIRDRAIL":
									case "2":
										type = PowerSupplyTypes.ThirdRail;
										break;
									case "FOURTHRAIL":
									case "3":
										type = PowerSupplyTypes.FourthRail;
										break;
									default:
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid type in Track.PowerSupplyEnd at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
										break;
								}
							}

							if (type != PowerSupplyTypes.None)
							{
								Data.Blocks[BlockIndex].Rails[idx].PowerSupplies.Remove(type);
							}
						}
					}
					break;
			}
		}

	}
}
