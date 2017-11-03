using System;
using OpenBveApi.Math;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <summary>Preprocesses the options contained within a route file</summary>
		/// <param name="IsRW">Whether the current route file is in RW format</param>
		/// <param name="Expressions">The initial list of expressions</param>
		/// <param name="Data">The finalized route data</param>
		/// <param name="UnitOfLength">The units of length conversion factor to be applied</param>
		/// <param name="PreviewOnly">Whether this is a preview only</param>
		private static void PreprocessOptions(bool IsRW, Expression[] Expressions, ref RouteData Data, ref double[] UnitOfLength, bool PreviewOnly)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Section = "";
			bool SectionAlwaysPrefix = false;
			// process expressions
			for (int j = 0; j < Expressions.Length; j++)
			{
				if (IsRW && Expressions[j].Text.StartsWith("[") && Expressions[j].Text.EndsWith("]"))
				{
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0)
					{
						Section = "Structure";
					}
					else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0)
					{
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				}
				else
				{
					Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					// separate command and arguments
					string Command, ArgumentSequence;
					Expressions[j].SeparateCommandsAndArguments(out Command, out ArgumentSequence, Culture, true, IsRW, Section);
					// process command
					double Number;
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (!NumberCheck || !NumberFormats.TryParseDoubleVb6(Command, UnitOfLength, out Number))
					{
						// split arguments
						string[] Arguments;
						{
							int n = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++)
							{
								if (IsRW & ArgumentSequence[k] == ',')
								{
									n++;
								}
								else if (ArgumentSequence[k] == ';')
								{
									n++;
								}
							}
							Arguments = new string[n + 1];
							int a = 0, h = 0;
							for (int k = 0; k < ArgumentSequence.Length; k++)
							{
								if (IsRW & ArgumentSequence[k] == ',')
								{
									Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
									a = k + 1; h++;
								}
								else if (ArgumentSequence[k] == ';')
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
						// preprocess command
						if (Command.ToLowerInvariant() == "with")
						{
							if (Arguments.Length >= 1)
							{
								Section = Arguments[0];
								SectionAlwaysPrefix = false;
							}
							else
							{
								Section = "";
								SectionAlwaysPrefix = false;
							}
							Command = null;
						}
						else
						{
							if (Command.StartsWith("."))
							{
								Command = Section + Command;
							}
							else if (SectionAlwaysPrefix)
							{
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
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
									int h = Indices.IndexOf(";", StringComparison.Ordinal);
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
								// options
								case "options.unitoflength":
								{
									if (Arguments.Length == 0)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										UnitOfLength = new double[Arguments.Length];
										for (int i = 0; i < Arguments.Length; i++)
										{
											UnitOfLength[i] = i == Arguments.Length - 1 ? 1.0 : 0.0;
											if (Arguments[i].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[i], out UnitOfLength[i]))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FactorInMeters" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												UnitOfLength[i] = i == 0 ? 1.0 : 0.0;
											}
											else if (UnitOfLength[i] <= 0.0)
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FactorInMeters" + i.ToString(Culture) + " is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
												UnitOfLength[i] = i == Arguments.Length - 1 ? 1.0 : 0.0;
											}
										}
									}
								}
									break;
								case "options.unitofspeed":
								{
									if (Arguments.Length < 1)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										if (Arguments.Length > 1)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										if (Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Data.UnitOfSpeed))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "FactorInKmph is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											Data.UnitOfSpeed = 0.277777777777778;
										}
										else if (Data.UnitOfSpeed <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "FactorInKmph is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											Data.UnitOfSpeed = 0.277777777777778;
										}
										else
										{
											Data.UnitOfSpeed *= 0.277777777777778;
										}
									}
								}
									break;
								case "options.objectvisibility":
								{
									if (Arguments.Length == 0)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										if (Arguments.Length > 1)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										int mode = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length != 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out mode))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										else if (mode != 0 & mode != 1)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "The specified Mode is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										Data.AccurateObjectDisposal = mode == 1;
									}
								}
									break;
								case "options.compatibletransparencymode":
								{
									//Whether to use fuzzy matching for BVE2 / BVE4 transparencies
									//Should be DISABLED on openBVE content
									if (PreviewOnly)
									{
										continue;
									}
									if (Arguments.Length == 0)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										if (Arguments.Length > 1)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										int mode = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length != 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out mode))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										else if (mode != 0 & mode != 1)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "The specified Mode is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										Interface.CurrentOptions.OldTransparencyMode = mode == 1;
									}
								}
									break;
								case "options.enablehacks":
								{
									//Whether to apply various hacks to fix BVE2 / BVE4 routes
									//Whilst this is harmless, it should be DISABLED on openBVE content
									//in order to ensure that all errors are correctly fixed by the developer
									if (PreviewOnly)
									{
										continue;
									}
									if (Arguments.Length == 0)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									}
									else
									{
										if (Arguments.Length > 1)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
										}
										int mode = 0;
										if (Arguments.Length >= 1 && Arguments[0].Length != 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out mode))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										else if (mode != 0 & mode != 1)
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "The specified Mode is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
											mode = 0;
										}
										Interface.CurrentOptions.EnableBveTsHacks = mode == 1;
									}
								}
									break;
							}
						}
					}
				}
			}
		}
	}
}
