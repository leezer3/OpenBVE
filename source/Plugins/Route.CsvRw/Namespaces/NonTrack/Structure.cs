using System;
using System.IO;
using System.Text;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using Path = OpenBveApi.Path;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private void ParseStructureCommand(StructureCommand Command, string[] Arguments, int[] commandIndices, string FileName, Encoding Encoding, Expression Expression, ref RouteData Data, bool PreviewOnly)
		{
			switch (Command)
			{
				case StructureCommand.Rail:
				{
					if (commandIndices[0] < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Path.ContainsInvalidChars(Arguments[0]))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							string f = Arguments[0];
							if (!LocateObject(ref f, ObjectPath))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								missingObjectCount++;
							}
							else
							{
								if (!PreviewOnly)
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.RailObjects.Add(commandIndices[0], obj, "RailStructure");
									}
								}
								else
								{
									railtypeCount++;
								}
							}
						}
					}

				}
				break;
				case StructureCommand.Beacon:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BeaconStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Path.CombineFile(ObjectPath, Arguments[0]);
								if (!File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.Beacon.Add(commandIndices[0], obj, "BeaconStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.Pole:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AdditionalRailsCovered is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (commandIndices[1] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PoleStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{

								if (!Data.Structure.Poles.ContainsKey(commandIndices[0]))
								{
									Data.Structure.Poles.Add(commandIndices[0], new ObjectDictionary());
								}

								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									bool overwriteDefault = commandIndices[1] >= 0 && commandIndices[1] >= 3;
									Data.Structure.Poles[commandIndices[0]].Add(commandIndices[1], obj, overwriteDefault);
								}
							}
						}
					}
				}
					break;
				case StructureCommand.Ground:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "GroundStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.Ground.Add(commandIndices[0], obj, "GroundStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.WallL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left WallStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.WallL.Add(commandIndices[0], obj, "Left WallStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.WallR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right WallStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.WallR.Add(commandIndices[0], obj, "Right WallStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.DikeL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left DikeStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.DikeL.Add(commandIndices[0], obj, "Left DikeStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.DikeR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right DikeStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.DikeR.Add(commandIndices[0], obj, "Right DikeStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.FormL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left FormStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.FormL.Add(commandIndices[0], obj, "Left FormStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.FormR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right FormStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.FormR.Add(commandIndices[0], obj, "Right FormStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.FormCL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left FormCStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadStaticObject(f, Encoding, false, out StaticObject obj);
									if (obj != null)
									{
										Data.Structure.FormCL.Add(commandIndices[0], obj, "Left FormCStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.FormCR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right FormCStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadStaticObject(f, Encoding, false, out StaticObject obj);
									if (obj != null)
									{
										Data.Structure.FormCR.Add(commandIndices[0], obj, "Right FormCStructure");
									}
								}
							}
						}
					}
				}
					break;
				/*
				 * NOTE:
				 * -----
				 * Mackoy's documentation states the following:
				 * RoofIdxStType: Roof structure index (0: None)
				 *
				 * Unfortunately inconsistant with null objects for rails
				 */
				case StructureCommand.RoofL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left RoofStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (commandIndices[0] == 0)
								{
									if (!IsRW)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									commandIndices[0] = 1;
								}

								if (commandIndices[0] < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									string f = Arguments[0];
									if (!LocateObject(ref f, ObjectPath))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
										if (obj != null)
										{
											Data.Structure.RoofL.Add(commandIndices[0], obj, "Left RoofStructure");
										}
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.RoofR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right RoofStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (commandIndices[0] == 0)
								{
									if (!IsRW)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									commandIndices[0] = 1;
								}

								if (commandIndices[0] < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									string f = Arguments[0];
									if (!LocateObject(ref f, ObjectPath))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
										if (obj != null)
										{
											Data.Structure.RoofR.Add(commandIndices[0], obj, "Right RoofStructure");
										}
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.RoofCL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left RoofCStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (commandIndices[0] == 0)
								{
									if (!IsRW)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									commandIndices[0] = 1;
								}

								if (commandIndices[0] < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									string f = Arguments[0];
									if (!LocateObject(ref f, ObjectPath))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Plugin.CurrentHost.LoadStaticObject(f, Encoding, false, out StaticObject obj);
										if (obj != null)
										{
											Data.Structure.RoofCL.Add(commandIndices[0], obj, "Left RoofCStructure");
										}
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.RoofCR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right RoofCStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (commandIndices[0] == 0)
								{
									if (!IsRW)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex was omitted or is 0 in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}

									commandIndices[0] = 1;
								}

								if (commandIndices[0] < 0)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex is expected to be non-negative in " + Command + " argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									string f = Arguments[0];
									if (!LocateObject(ref f, ObjectPath))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
									}
									else
									{
										Plugin.CurrentHost.LoadStaticObject(f, Encoding, false, out StaticObject obj);
										if (obj != null)
										{
											Data.Structure.RoofCR.Add(commandIndices[0], obj, "Right RoofCStructure");
										}
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.CrackL:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Left CrackStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadStaticObject(f, Encoding, true, out StaticObject obj);
									if (obj != null)
									{
										Data.Structure.CrackL.Add(commandIndices[0], obj, "Left CrackStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.CrackR:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Right CrackStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Arguments[0];
								if (!LocateObject(ref f, ObjectPath))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadStaticObject(f, Encoding, true, out StaticObject obj);
									if (obj != null)
									{
										Data.Structure.CrackR.Add(commandIndices[0], obj, "Right CrackStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.Object:
				case StructureCommand.FreeObj:
				{
					if (Command == StructureCommand.Object)
					{
						IsHmmsim = true;
						Plugin.CurrentOptions.ObjectDisposalMode = ObjectDisposalMode.Accurate;
					}
					if (commandIndices[0] < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FreeObjStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Path.ContainsInvalidChars(Arguments[0]))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							string f = Arguments[0];
							if (!LocateObject(ref f, ObjectPath))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " could not be found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								missingObjectCount++;
							}
							else
							{
								if (!PreviewOnly)
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.FreeObjects.Add(commandIndices[0], obj, "FreeObject");
									}
								}
								else
								{
									freeObjCount++;
								}
							}
						}
					}
				}
				break;
				case StructureCommand.Background:
				case StructureCommand.Back:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundTextureIndex is expected to be non-negative at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								if (!Data.Backgrounds.ContainsKey(commandIndices[0]))
								{
									Data.Backgrounds.Add(commandIndices[0], new StaticBackground(null, 6, false, Plugin.CurrentOptions.ViewingDistance));
								}

								string f = Path.CombineFile(ObjectPath, Arguments[0]);
								if (!File.Exists(f) && (Arguments[0].ToLowerInvariant() == "back_mt.bmp" || Arguments[0].ToLowerInvariant() == "back_mthigh.bmp" || Arguments[0].ToLowerInvariant() == "bg_fine.bmp"))
								{
									//Default background textures supplied with Uchibo for BVE1 / BVE2, so map to something that's not totally black
									f = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility"), "Uchibo\\Back_Mt.png");
								}

								if (!File.Exists(f) && Plugin.CurrentOptions.EnableBveTsHacks)
								{
									if (Arguments[0].StartsWith("Midland Suburban Line", StringComparison.InvariantCultureIgnoreCase))
									{
										Arguments[0] = "Midland Suburban Line Objects" + Arguments[0].Substring(21);
										f = Path.CombineFile(ObjectPath, Arguments[0]);
									}
									else if (commandIndices[0] == 0)
									{
										// Background zero is defined but missing- Map to generic replacement, as otherwise some stuff will be completely blank
										f = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility"), "Uchibo\\Back_Mt.png");
									}
								}

								if (!File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									if (f.ToLowerInvariant().EndsWith(".xml"))
									{
										try
										{
											BackgroundHandle h = DynamicBackgroundParser.ReadBackgroundXML(f);
											Data.Backgrounds[commandIndices[0]] = h;
										}
										catch(Exception ex)
										{
											if (ex is XmlException)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, true, f + " contains malformed XML in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											}
											else
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, true, f + " is not a valid background XML in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
											}
													
										}
									}
									else
									{
										if (Data.Backgrounds[commandIndices[0]] is StaticBackground b)
										{
											Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out b.Texture);
										}
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.BackgroundX:
				case StructureCommand.BackX:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + commandIndices[0] + " is expected to be non-negative at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Backgrounds.ContainsKey(commandIndices[0]))
							{
								Data.Backgrounds.Add(commandIndices[0], new StaticBackground(null, 6, false, Plugin.CurrentOptions.ViewingDistance));
							}

							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int x))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + Arguments[0] + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (x == 0)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RepetitionCount is expected to be non-zero in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Backgrounds[commandIndices[0]] is StaticBackground b)
								{
									b.Repetition = x;
								}
							}
						}
					}
				}
					break;
				case StructureCommand.BackgroundAspect:
				case StructureCommand.BackAspect:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + commandIndices[0] + " is expected to be non-negative at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else if (Arguments.Length < 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (!Data.Backgrounds.ContainsKey(commandIndices[0]))
							{
								Data.Backgrounds.Add(commandIndices[0], new StaticBackground(null, 6, false, Plugin.CurrentOptions.ViewingDistance));
							}

							if (!NumberFormats.TryParseIntVb6(Arguments[0], out int aspect))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BackgroundTextureIndex " + Arguments[0] + " is invalid in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (aspect != 0 & aspect != 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Value is expected to be either 0 or 1 in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								if (Data.Backgrounds[commandIndices[0]] is StaticBackground b)
								{
									b.KeepAspectRatio = aspect == 1;
								}

							}
						}
					}
				}
					break;
				case StructureCommand.Weather:
				{
					if (!PreviewOnly)
					{
						if (commandIndices[0] < 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "WeatherStructureIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
						}
						else
						{
							if (Arguments.Length < 1)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, Command + " is expected to have one argument at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else if (Path.ContainsInvalidChars(Arguments[0]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FileName " + Arguments[0] + " contains illegal characters in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							}
							else
							{
								string f = Path.CombineFile(ObjectPath, Arguments[0]);
								if (!File.Exists(f))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
								}
								else
								{
									Plugin.CurrentHost.LoadObject(f, Encoding, out UnifiedObject obj);
									if (obj != null)
									{
										Data.Structure.WeatherObjects.Add(commandIndices[0], obj, "RainStructure");
									}
								}
							}
						}
					}
				}
					break;
				case StructureCommand.DynamicLight:
					if (commandIndices[0] < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DynamicLightIndex is expected to be non-negative in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
					}
					else
					{
						/*
						 * Read the dynamic lighting file
						 * We'll try first relative to the routefile (as per Route.DynamicLight)
						 * and if not found there relative to the object path
						 */
						string path = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
						if (!File.Exists(path))
						{
							path = Path.CombineFile(Path.GetDirectoryName(ObjectPath), Arguments[0]);
						}
						if (File.Exists(path))
						{
							if (DynamicLightParser.ReadLightingXML(path, out LightDefinition[] newLightDefinition))
							{
								if (Data.Structure.LightDefinitions.ContainsKey(commandIndices[0]))
								{
									Data.Structure.LightDefinitions[commandIndices[0]] = newLightDefinition;
								}
								else
								{
									Data.Structure.LightDefinitions.Add(commandIndices[0], newLightDefinition);
								}
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
					}
					break;
			}
		}
	}
}
