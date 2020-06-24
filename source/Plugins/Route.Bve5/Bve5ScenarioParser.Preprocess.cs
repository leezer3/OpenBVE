using System;
using System.IO;
using System.Text;
using OpenBveApi.Interface;
using OpenBveApi.Math;

namespace Bve5RouteParser
{
	internal partial class Parser
	{
		private void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, out Expression[] Expressions, double trackPositionOffset)
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
						Array.Resize(ref Expressions, Expressions.Length << 1);
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
			Array.Resize(ref Expressions, e);
		}

		private void PreprocessOptions(string FileName, Expression[] Expressions, ref RouteData Data, ref double[] UnitOfLength, bool PreviewOnly, System.Text.Encoding Encoding)
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
							Array.Resize(ref Arguments, h);
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
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												Encoding enc = DetermineFileEncoding(StructureFile);
												LoadObjects(StructureFile, ref Data, enc, PreviewOnly);
											}
										}
									} break;
								//BVE5 station definition files
								case "station.load":
									{
										if (Arguments.Length == 0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												Encoding enc = DetermineFileEncoding(StationFile);
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
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
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
												Encoding enc = DetermineFileEncoding(SignalFile);
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
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " +
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
							closingerror = true;
						}
						Expression.Text += ")";
					}
				}
				else if (Expression.Text[i] == ')')
				{
					if (RaiseErrors & !closingerror)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid syntax encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
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
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid trailing comma encountered in " + Command + " at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File);
				}
				while (Command.EndsWith(","))
				{
					Command = Command.Substring(0, Command.Length - 1);
				}
			}
		}
	}
}
