using System;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private class Expression
		{
			internal string File;
			internal string Text;
			internal int Line;
			internal int Column;
			internal double TrackPositionOffset;

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
						double b; if (NumberFormats.TryParseDoubleVb6(t, out b))
						{
							t = ".Ground(" + b + ")";
						}
					}
					else if (Section.ToLowerInvariant() == "signal" & SectionAlwaysPrefix)
					{
						double b; if (NumberFormats.TryParseDoubleVb6(t, out b))
						{
							t = ".Void(" + b + ")";
						}
					}
					// convert RW style into CSV style
					Text = t + " " + Text.Substring(Equals + 1);
				}
			}

			/// <summary>Separates an expression into it's consituent command and arguments</summary>
			/// <param name="Command">The command</param>
			/// <param name="ArgumentSequence">The sequence of arguments contained within the expression</param>
			/// <param name="Culture">The current culture</param>
			/// <param name="RaiseErrors">Whether errors should be raised at this point</param>
			/// <param name="IsRw">Whether this is a RW format file</param>
			/// <param name="CurrentSection">The current section being processed</param>
			internal void SeparateCommandsAndArguments(out string Command, out string ArgumentSequence, System.Globalization.CultureInfo Culture, bool RaiseErrors, bool IsRw, string CurrentSection)
			{
				bool openingerror = false, closingerror = false;
				int i, fcb = 0;
				if (Interface.CurrentOptions.EnableBveTsHacks)
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
					else if (Text.EndsWith(")height(0)", StringComparison.InvariantCultureIgnoreCase))
					{
						//Heavy Coal original RW- Fix starting station
						Text = Text.Substring(0, Text.Length - 9);
					}
					if (IsRw && CurrentSection.ToLowerInvariant() == "track")
					{
						//Removes misplaced track position indicies from the end of a command in the Track section
						int idx = Text.LastIndexOf(')');
						if (idx != -1 && idx != Text.Length)
						{
							double d;
							string s = this.Text.Substring(idx + 1, this.Text.Length - idx - 1).Trim();
							if (NumberFormats.TryParseDoubleVb6(s, out d))
							{
								this.Text = this.Text.Substring(0, idx).Trim();
							}
						}
					}
					if(IsRw && this.Text.EndsWith("))"))
					{
						int openingBrackets = Text.Count(x => x == '(');
						int closingBrackets = Text.Count(x => x == ')');
						//Remove obviously wrong double-ending brackets
						if (closingBrackets == openingBrackets + 1 && this.Text.EndsWith("))"))
						{
							this.Text = this.Text.Substring(0, this.Text.Length - 1);
						}
					}

					if (Text.StartsWith("route.comment", StringComparison.InvariantCultureIgnoreCase) && Text.IndexOf("(C)", StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						//Some BVE4 routes use this instead of the copyright symbol
						Text = Text.Replace("(C)", "©");
						Text = Text.Replace("(c)", "©");
					}
				}
				for (i = 0; i < Text.Length; i++)
				{
					if (Text[i] == '(')
					{
						bool found = false;
						bool stationName = false;
						bool replaced = false;
						i++;
						while (i < Text.Length)
						{
							if (Text[i] == ',' || Text[i] == ';')
							{
								//Only check parenthesis in the station name field- The comma and semi-colon are the argument separators
								stationName = true;
							}
							if (Text[i] == '(')
							{
								if (RaiseErrors & !openingerror)
								{
									if (stationName)
									{
										Interface.AddMessage(MessageType.Error, false, "Invalid opening parenthesis encountered at line " + Line.ToString(Culture) + ", column " +
										                                                         Column.ToString(Culture) + " in file " + File);
										openingerror = true;
									}
									else
									{
										Text = Text.Remove(i, 1).Insert(i, "[");
										replaced = true;
									}
								}
							}
							else if (Text[i] == ')')
							{
								if (stationName == false && i != Text.Length && replaced == true)
								{
									Text = Text.Remove(i, 1).Insert(i, "]");
									continue;
								}
								found = true;
								fcb = i;
								break;
							}
							i++;
						}
						if (!found)
						{
							if (RaiseErrors & !closingerror)
							{
								Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
								closingerror = true;
							}
							Text += ")";
						}
					}
					else if (Text[i] == ')')
					{
						if (RaiseErrors & !closingerror)
						{
							Interface.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							closingerror = true;
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
				if (fcb != 0 && fcb < Text.Length - 1)
				{
					if (!Char.IsWhiteSpace(Text[fcb + 1]) && Text[fcb + 1] != '.' && Text[fcb + 1] != ';')
					{
						Text = Text.Insert(fcb + 1, " ");
						i = fcb;
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
							if (RaiseErrors & !closingerror)
							{
								Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
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
									Command = null; ArgumentSequence = null;
									for (int k = j + 1; k < Text.Length; k++)
									{
										if (char.IsWhiteSpace(Text[k]))
										{
											Command = Text.Substring(0, k).TrimEnd();
											ArgumentSequence = Text.Substring(k + 1).TrimStart();
											found = true; break;
										}
										else if (Text[k] == '(')
										{
											Command = Text.Substring(0, k).TrimEnd();
											ArgumentSequence = Text.Substring(k).TrimStart();
											found = true; break;
										}
									}
									if (!found)
									{
										if (RaiseErrors & !openingerror & !closingerror)
										{
											Interface.AddMessage(MessageType.Error, false, "Invalid syntax encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
											openingerror = true;
											closingerror = true;
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
										if (RaiseErrors & !closingerror)
										{
											Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
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
									Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
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
								if (RaiseErrors & !closingerror)
								{
									Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
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
							if (RaiseErrors & !closingerror)
							{
								Interface.AddMessage(MessageType.Error, false, "Missing closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
							}
							Command = Text.Substring(0, i).TrimEnd();
							ArgumentSequence = Text.Substring(i + 1).TrimStart();
						}
						else
						{
							if (RaiseErrors)
							{
								i = Text.IndexOf(')');
								if (i >= 0 & !closingerror)
								{
									Interface.AddMessage(MessageType.Error, false, "Invalid closing parenthesis encountered at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
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
						Interface.AddMessage(MessageType.Error, false, "Invalid trailing semicolon encountered in " + Command + " at line " + Line.ToString(Culture) + ", column " + Column.ToString(Culture) + " in file " + File);
					}
					while (Command.EndsWith(";"))
					{
						Command = Command.Substring(0, Command.Length - 1);
					}
				}
			}
		}
	}
}
