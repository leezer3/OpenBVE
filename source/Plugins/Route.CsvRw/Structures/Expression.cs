using System;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.Interface;

namespace CsvRwRouteParser
{
	internal class Expression
	{
		internal string File;
		internal string Text;
		internal int Line;
		internal int Column;
		internal double TrackPositionOffset;

		internal Expression(string file, string text, int line, int column, double trackPositionOffset)
		{
			File = file;
			Text = text;
			Line = line;
			Column = column;
			TrackPositionOffset = trackPositionOffset;
		}

		/// <summary>Converts a RW formatted expression to CSV format</summary>
		/// <param name="Section">The current section</param>
		/// <param name="SectionAlwaysPrefix">Whether the section prefix should always be applied</param>
		internal void ConvertRwToCsv(string Section, bool SectionAlwaysPrefix)
		{
			int Equals = Text.IndexOf('=');
			if (Equals >= 0)
			{
				// handle RW cycle syntax
				string t = Text.Substring(0, Equals);
				if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix)
				{
					if (NumberFormats.TryParseDoubleVb6(t, out double b))
					{
						t = ".Ground(" + b + ")";
					}
				}
				else if (Section.ToLowerInvariant() == "signal" & SectionAlwaysPrefix)
				{
					if (NumberFormats.TryParseDoubleVb6(t, out double b))
					{
						t = ".Void(" + b + ")";
					}
				}

				// convert RW style into CSV style
				Text = t + " " + Text.Substring(Equals + 1);
			}
		}

		/// <summary>Separates an expression into it's constituent command and arguments</summary>
		/// <param name="Command">The command</param>
		/// <param name="ArgumentSequence">The sequence of arguments contained within the expression</param>
		/// <param name="Culture">The current culture</param>
		/// <param name="RaiseErrors">Whether errors should be raised at this point</param>
		/// <param name="IsRw">Whether this is a RW format file</param>
		/// <param name="CurrentSection">The current section being processed</param>
		internal void SeparateCommandsAndArguments(out string Command, out string ArgumentSequence, System.Globalization.CultureInfo Culture, bool RaiseErrors, bool IsRw, string CurrentSection)
		{
			bool openingError = false, closingError = false;
			int i, firstClosingBracket = 0;
			if (Plugin.CurrentOptions.EnableBveTsHacks)
			{
				if (Text.StartsWith("Train. ", StringComparison.InvariantCultureIgnoreCase))
				{
					//HACK: Some Chinese routes seem to have used a space between Train. and the rest of the command
					//e.g. Taipei Metro. BVE4/ 2 accept this......
					Text = "Train." + Text.Substring(7, Text.Length - 7);
				}
				else if (Text.StartsWith("Texture. Background", StringComparison.InvariantCultureIgnoreCase))
				{
					//Same hack as above, found in Minobu route for BVE2
					Text = "Texture.Background" + Text.Substring(19, Text.Length - 19);
				}
				else if (Text.StartsWith("Structure. ", StringComparison.InvariantCultureIgnoreCase))
				{
					//Another variant, this time from JR 内房Line
					Text = "Structure." + Text.Substring(11, Text.Length - 11);
				}
				else if (Text.EndsWith(")height(0)", StringComparison.InvariantCultureIgnoreCase))
				{
					//Heavy Coal original RW- Fix starting station
					Text = Text.Substring(0, Text.Length - 9);
				}
				else if (Text.StartsWith(".Sta (貨)", StringComparison.OrdinalIgnoreCase))
				{
					// 普通播州赤穂行 - brackets in station name
					Text = ".Sta [貨]"+ Text.Substring(8);
				}

				if (IsRw && CurrentSection.ToLowerInvariant() == "track")
				{
					//Removes misplaced track position indicies from the end of a command in the Track section
					int idx = Text.LastIndexOf(')');
					if (idx != -1 && idx != Text.Length)
					{
						string s = Text.Substring(idx + 1, Text.Length - idx - 1).Trim();
						if (NumberFormats.TryParseDoubleVb6(s, out double _))
						{
							Text = Text.Substring(0, idx).Trim();
						}
					}
				}

				if (IsRw && Text.EndsWith("))"))
				{
					int openingBrackets = Text.Count(x => x == '(');
					int closingBrackets = Text.Count(x => x == ')');
					//Remove obviously wrong double-ending brackets
					if (closingBrackets == openingBrackets + 1 && Text.EndsWith("))"))
					{
						Text = Text.Substring(0, Text.Length - 1);
					}
				}

				if (Text.StartsWith("route.comment", StringComparison.InvariantCultureIgnoreCase) && Text.IndexOf("(C)", StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					//Some BVE4 routes use this instead of the copyright symbol
					Text = Text.Replace("(C)", "©");
					Text = Text.Replace("(c)", "©");
				}

				if(IsRw && Parser.EnabledHacks.AggressiveRwBrackets)
				{
					//Attempts to aggressively discard *anything* encountered after a closing bracket
					int c = Text.IndexOf(')');
					while (c > Text.Length)
					{
						if (Text[c] == '=')
						{
							break;
						}

						if (!char.IsWhiteSpace(Text[c]))
						{
							Text = Text.Substring(c);
							break;
						}
						c++;
					}
					
				}
			}

			for (i = 0; i < Text.Length; i++)
			{
				if (Text[i] == '(')
				{
					bool found = false;
					int argumentIndex = 0;
					i++;
					while (i < Text.Length)
					{
						if (Text[i] == ',' || Text[i] == ';')
						{
							//Only check parenthesis in the station name field- The comma and semi-colon are the argument separators
							argumentIndex++;
						}

						if (Text[i] == '(')
						{
							if (RaiseErrors & !openingError)
							{
								switch (argumentIndex)
								{
									case 0:
										if (Text.StartsWith("sta", StringComparison.InvariantCultureIgnoreCase))
										{
											Text = Text.Remove(i, 1).Insert(i, "[");
											break;
										}
										if (Text.StartsWith(".marker", StringComparison.InvariantCultureIgnoreCase) || Text.StartsWith(".announce", StringComparison.InvariantCultureIgnoreCase) ||
										    Text.IndexOf(".Load", StringComparison.InvariantCultureIgnoreCase) != -1)
										{
											/*
											 * HACK: In filenames, temp replace with an invalid but known character
											 *
											 * Opening parenthesis are fortunately simpler than closing, see notes below.
											 */
											if (Text.Substring(i - 5, 5).ToLowerInvariant() != ".load")
											{
												Text = Text.Remove(i, 1).Insert(i, "<");
											}
											break;
										}
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Line.ToString(Culture) + ", column " +
										                                                        Column.ToString(Culture) + " in file " + File);
										openingError = true;
										break;
									case 5: //arrival sound
									case 10: //departure sound
										if (Text.StartsWith("sta", StringComparison.InvariantCultureIgnoreCase))
										{
											Text = Text.Remove(i, 1).Insert(i, "<");
											break;
										}
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Line.ToString(Culture) + ", column " +
										                                                        Column.ToString(Culture) + " in file " + File);
										openingError = true;
										break;
									default:
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Line.ToString(Culture) + ", column " +
										                                                        Column.ToString(Culture) + " in file " + File);
										openingError = true;
										break;
								}
							}
						}
						else if (Text[i] == ')')
						{
							if (i == Text.Length - 1)
							{
								found = true;
								firstClosingBracket = i;
								break;
							}
							switch (argumentIndex)
							{
								case 0:
									if (Text.StartsWith("sta", StringComparison.InvariantCultureIgnoreCase) && i != Text.Length)
									{
										Text = Text.Remove(i, 1).Insert(i, "]");
										continue;
									}
									if (Text.StartsWith(".timetable", StringComparison.InvariantCultureIgnoreCase) || Text.StartsWith(".marker", StringComparison.InvariantCultureIgnoreCase) || Text.StartsWith(".announce", StringComparison.InvariantCultureIgnoreCase) || Text.IndexOf(".Load", StringComparison.InvariantCultureIgnoreCase) != -1)
									{
										if (Text.Substring(i + 1, 5).ToLowerInvariant() == ".load" || Text.Substring(i + 1, 9).ToLowerInvariant() == ".day.load" || Text.Substring(i + 1, 11).ToLowerInvariant() == ".night.load")
										{
											found = true;
											firstClosingBracket = i;
											goto breakout;
										}

										if (Text.IndexOf('<') == -1)
										{
											i++;
											continue;
										}
										
										/*
										 * HACK: In filenames, temp replace with an invalid but known character
										 *
										 * Note that this is a PITA in object folder names when the creator has used the alternate .Load() format as this contains far more brackets
										 * e.g.
										 * .Rail(0).Load(kcrmosr(2009)\rail\c0.csv)
										 * We must keep the first and last closing parenthesis intact here
										 */
										Text = Text.Remove(i, 1).Insert(i, ">");
										continue;
									}
									break;
								case 5: //arrival sound
								case 10: //departure sound
									if (Text.StartsWith("sta", StringComparison.InvariantCultureIgnoreCase) && i != Text.Length)
									{
										Text = Text.Remove(i, 1).Insert(i, ">");
										continue;
									}
									break;
							}
							found = true;
							firstClosingBracket = i;
							break;
						}
						i++;
					}

					breakout:

					if (!found)
					{
						if (RaiseErrors & !closingError)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							closingError = true;
						}

						Text += ")";
					}
				}
				else if (Text[i] == ')')
				{
					if (RaiseErrors & !closingError)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
						closingError = true;
					}
				}
				else if (char.IsWhiteSpace(Text[i]))
				{
					if (i >= Text.Length - 1 || !char.IsWhiteSpace(Text[i + 1]))
					{
						break;
					}
				}

			}

			if (firstClosingBracket != 0 && firstClosingBracket < Text.Length - 1)
			{
				if (!char.IsWhiteSpace(Text[firstClosingBracket + 1]) && Text[firstClosingBracket + 1] != '.' && Text[firstClosingBracket + 1] != ';')
				{
					Text = Text.Insert(firstClosingBracket + 1, " ");
					i = firstClosingBracket;
				}
			}

			if (i < Text.Length)
			{
				// white space was found outside of parentheses
				string a = Text.Substring(0, i);
				if (a.IndexOf('(') >= 0 & a.IndexOf(')') >= 0)
				{
					// indices found not separated from the command by spaces
					Command = Text.Substring(0, i).TrimEnd();
					ArgumentSequence = Text.Substring(i + 1).TrimStart();
					if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")"))
					{
						// arguments are enclosed by parentheses
						ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
					}
					else if (ArgumentSequence.StartsWith("("))
					{
						// only opening parenthesis found
						if (RaiseErrors & !closingError)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
						}

						ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
					}
				}
				else
				{
					// no indices found before the space
					if (i < Text.Length - 1 && Text[i + 1] == '(')
					{
						// opening parenthesis follows the space
						int j = Text.IndexOf(')', i + 1);
						if (j > i + 1)
						{
							// closing parenthesis found
							if (j == Text.Length - 1)
							{
								// only closing parenthesis found at the end of the expression
								Command = Text.Substring(0, i).TrimEnd();
								ArgumentSequence = Text.Substring(i + 2, j - i - 2).Trim();
							}
							else
							{
								// detect border between indices and arguments
								bool found = false;
								Command = null;
								ArgumentSequence = null;
								for (int k = j + 1; k < Text.Length; k++)
								{
									if (char.IsWhiteSpace(Text[k]))
									{
										Command = Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Text.Substring(k + 1).TrimStart();
										found = true;
										break;
									}

									if (Text[k] == '(')
									{
										Command = Text.Substring(0, k).TrimEnd();
										ArgumentSequence = Text.Substring(k).TrimStart();
										found = true;
										break;
									}
								}

								if (!found)
								{
									if (RaiseErrors & !openingError & !closingError)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid syntax encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
										closingError = true;
									}

									Command = Text;
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
									if (RaiseErrors & !closingError)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
									}

									ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
								}
							}
						}
						else
						{
							// no closing parenthesis found
							if (RaiseErrors & !closingError)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							}

							Command = Text.Substring(0, i).TrimEnd();
							ArgumentSequence = Text.Substring(i + 2).TrimStart();
						}
					}
					else
					{
						// no index possible
						Command = Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Text.Substring(i + 1).TrimStart();
						if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")"))
						{
							// arguments are enclosed by parentheses
							ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
						}
						else if (ArgumentSequence.StartsWith("("))
						{
							// only opening parenthesis found
							if (RaiseErrors & !closingError)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							}

							ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
						}
					}
				}
			}
			else
			{
				// no single space found
				if (Text.EndsWith(")"))
				{
					i = Text.LastIndexOf('(');
					if (i >= 0)
					{
						Command = Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Text.Substring(i + 1, Text.Length - i - 2).Trim();
						if (Text.StartsWith("sta", StringComparison.InvariantCultureIgnoreCase) || Text.StartsWith(".marker", StringComparison.InvariantCultureIgnoreCase) || Text.StartsWith(".announce", StringComparison.InvariantCultureIgnoreCase) || Text.IndexOf(".Load", StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							// put back any temp removed brackets
							ArgumentSequence = ArgumentSequence.Replace('<', '(');
							ArgumentSequence = ArgumentSequence.Replace('>', ')');
							if (ArgumentSequence.EndsWith(")"))
							{
								ArgumentSequence = ArgumentSequence.TrimEnd(')');
							}
						}
					}
					else
					{
						Command = Text;
						ArgumentSequence = "";
					}
				}
				else
				{
					i = Text.IndexOf('(');
					if (i >= 0)
					{
						if (RaiseErrors & !closingError)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
						}

						Command = Text.Substring(0, i).TrimEnd();
						ArgumentSequence = Text.Substring(i + 1).TrimStart();
					}
					else
					{
						if (RaiseErrors)
						{
							i = Text.IndexOf(')');
							if (i >= 0 & !closingError)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							}
						}

						Command = Text;
						ArgumentSequence = "";
					}
				}
			}

			// invalid trailing characters
			if (Command.EndsWith(";"))
			{
				if (RaiseErrors)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid trailing semicolon encountered in " + Command + " at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
				}
				Command = Command.TrimEnd(';');
			}
		}
	}
}
