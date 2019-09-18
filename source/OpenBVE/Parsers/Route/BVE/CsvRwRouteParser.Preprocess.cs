using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void PreprocessSplitIntoExpressions(string FileName, bool IsRW, string[] Lines, out Expression[] Expressions, bool AllowRwRouteDescription, double trackPositionOffset) {
			Expressions = new Expression[4096];
			int e = 0;
			// full-line rw comments
			if (IsRW) {
				for (int i = 0; i < Lines.Length; i++) {
					int Level = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ';':
								if (Level == 0)
								{
									Lines[i] = Lines[i].Substring(0, j).TrimEnd(new char[] { });
									j = Lines[i].Length;
								}
								break;
							case '=':
								if (Level == 0) {
									j = Lines[i].Length;
								}
								break;
						}
					}
				}
			}
			// parse
			for (int i = 0; i < Lines.Length; i++) {
				//Remove empty null characters
				//Found these in a couple of older routes, harmless but generate errors
				//Possibly caused by BVE-RR (DOS version)
				Lines[i] = Lines[i].Replace("\0", "");
				if (IsRW & AllowRwRouteDescription) {
					// ignore rw route description
					if (
						Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].IndexOf("]", StringComparison.Ordinal) > 0 |
						Lines[i].StartsWith("$")
					) {
						AllowRwRouteDescription = false;
						Game.RouteComment = Game.RouteComment.Trim(new char[] { });
					} else {
						if (Game.RouteComment.Length != 0) {
							Game.RouteComment += "\n";
						}
						Game.RouteComment += Lines[i];
						continue;
					}
				}
				{
					// count expressions
					int n = 0; int Level = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								Level--;
								break;
							case ',':
								if (!IsRW & Level == 0) n++;
								break;
							case '@':
								if (IsRW & Level == 0) n++;
								break;
						}
					}
					// create expressions
					int m = e + n + 1;
					while (m >= Expressions.Length) {
						Array.Resize<Expression>(ref Expressions, Expressions.Length << 1);
					}
					Level = 0;
					int a = 0, c = 0;
					for (int j = 0; j < Lines[i].Length; j++) {
						switch (Lines[i][j]) {
							case '(':
								Level++;
								break;
							case ')':
								if (Interface.CurrentOptions.EnableBveTsHacks)
								{

									if (Level > 0)
									{
										//Don't decrease the level below zero, as this messes up when extra closing brackets are encountered
										Level--;
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, "Invalid additional closing parenthesis encountered at line " + i + " character " + j + " in file " + FileName);
									}
								}
								else
								{
									Level--;
								}
								break;
							case ',':
								if (Level == 0 & !IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim(new char[] { });
									if (t.Length > 0 && !t.StartsWith(";")) {
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
							case '@':
								if (Level == 1 & IsRW & Interface.CurrentOptions.EnableBveTsHacks)
								{
									//BVE2 doesn't care if a bracket is unclosed, fixes various routefiles
									Level--;
								}
								else if (Level == 2 && IsRW & Interface.CurrentOptions.EnableBveTsHacks)
								{
									int k = j;
									while (k > 0)
									{
										k--;
										if (Lines[i][k] == '(')
										{
											//Opening bracket has been used instead of closing bracket, again BVE2 ignores this
											Level -= 2;
											break;
										}
										if (!char.IsWhiteSpace(Lines[i][k]))
										{
											//Bracket not found, and this isn't whitespace either, so break out
											break;
										}
									}
								}
								if (Level == 0 & IsRW) {
									string t = Lines[i].Substring(a, j - a).Trim(new char[] { });
									if (t.Length > 0 && !t.StartsWith(";")) {
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
					if (Lines[i].Length - a > 0) {
						string t = Lines[i].Substring(a).Trim(new char[] { });
						if (t.Length > 0 && !t.StartsWith(";")) {
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
			Array.Resize<Expression>(ref Expressions, e);
		}

		/// <summary>This function processes the list of expressions for $Char, $Rnd, $If and $Sub directives, and evaluates them into the final expressions dataset</summary>
		private static void PreprocessChrRndSub(string FileName, bool IsRW, System.Text.Encoding Encoding, ref Expression[] Expressions) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Subs = new string[16];
			int openIfs = 0;
			for (int i = 0; i < Expressions.Length; i++) {
				string Epilog = " at line " + Expressions[i].Line.ToString(Culture) + ", column " + Expressions[i].Column.ToString(Culture) + " in file " + Expressions[i].File;
				bool continueWithNextExpression = false;
				for (int j = Expressions[i].Text.Length - 1; j >= 0; j--) {
					if (Expressions[i].Text[j] == '$') {
						int k;
						for (k = j + 1; k < Expressions[i].Text.Length; k++) {
							if (Expressions[i].Text[k] == '(') {
								break;
							} else if (Expressions[i].Text[k] == '/' | Expressions[i].Text[k] == '\\') {
								k = Expressions[i].Text.Length + 1;
								break;
							}
						}
						if (k <= Expressions[i].Text.Length)
						{
							string t = Expressions[i].Text.Substring(j, k - j).TrimEnd(new char[] { });
							int l = 1, h;
							for (h = k + 1; h < Expressions[i].Text.Length; h++) {
								switch (Expressions[i].Text[h]) {
									case '(':
										l++;
										break;
									case ')':
										l--;
										if (l < 0) {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Invalid parenthesis structure in " + t + Epilog);
										}
										break;
								}
								if (l <= 0) {
									break;
								}
							}
							if (continueWithNextExpression) {
								break;
							}
							if (l != 0) {
								Interface.AddMessage(MessageType.Error, false, "Invalid parenthesis structure in " + t + Epilog);
								break;
							}
							string s = Expressions[i].Text.Substring(k + 1, h - k - 1).Trim(new char[] { });
							switch (t.ToLowerInvariant()) {
								case "$if":
									if (j != 0) {
										Interface.AddMessage(MessageType.Error, false, "The $If directive must not appear within another statement" + Epilog);
									} else {
										double num;
										if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out num)) {
											openIfs++;
											Expressions[i].Text = string.Empty;
											if (num == 0.0) {
												/*
												 * Blank every expression until the matching $Else or $EndIf
												 * */
												i++;
												int level = 1;
												while (i < Expressions.Length) {
													if (Expressions[i].Text.StartsWith("$if", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														level++;
													} else if (Expressions[i].Text.StartsWith("$else", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														if (level == 1) {
															level--;
															break;
														}
													} else if (Expressions[i].Text.StartsWith("$endif", StringComparison.OrdinalIgnoreCase)) {
														Expressions[i].Text = string.Empty;
														level--;
														if (level == 0) {
															openIfs--;
															break;
														}
													} else {
														Expressions[i].Text = string.Empty;
													}
													i++;
												}
												if (level != 0) {
													Interface.AddMessage(MessageType.Error, false, "$EndIf missing at the end of the file" + Epilog);
												}
											}
											continueWithNextExpression = true;
											break;
										} else {
											Interface.AddMessage(MessageType.Error, false, "The $If condition does not evaluate to a number" + Epilog);
										}
									}
									continueWithNextExpression = true;
									break;
								case "$else":
									/*
									 * Blank every expression until the matching $EndIf
									 * */
									Expressions[i].Text = string.Empty;
									if (openIfs != 0) {
										i++;
										int level = 1;
										while (i < Expressions.Length) {
											if (Expressions[i].Text.StartsWith("$if", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												level++;
											} else if (Expressions[i].Text.StartsWith("$else", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												if (level == 1) {
													Interface.AddMessage(MessageType.Error, false, "Duplicate $Else encountered" + Epilog);
												}
											} else if (Expressions[i].Text.StartsWith("$endif", StringComparison.OrdinalIgnoreCase)) {
												Expressions[i].Text = string.Empty;
												level--;
												if (level == 0) {
													openIfs--;
													break;
												}
											} else {
												Expressions[i].Text = string.Empty;
											}
											i++;
										}
										if (level != 0) {
											Interface.AddMessage(MessageType.Error, false, "$EndIf missing at the end of the file" + Epilog);
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "$Else without matching $If encountered" + Epilog);
									}
									continueWithNextExpression = true;
									break;
								case "$endif":
									Expressions[i].Text = string.Empty;
									if (openIfs != 0) {
										openIfs--;
									} else {
										Interface.AddMessage(MessageType.Error, false, "$EndIf without matching $If encountered" + Epilog);
									}
									continueWithNextExpression = true;
									break;
								case "$include":
									if (j != 0) {
										Interface.AddMessage(MessageType.Error, false, "The $Include directive must not appear within another statement" + Epilog);
										continueWithNextExpression = true;
										break;
									}
									string[] args = s.Split(new char[] { ';' });
									for (int ia = 0; ia < args.Length; ia++) {
										args[ia] = args[ia].Trim(new char[] { });
									}
									int count = (args.Length + 1) / 2;
									string[] files = new string[count];
									double[] weights = new double[count];
									double[] offsets = new double[count];
									double weightsTotal = 0.0;
									for (int ia = 0; ia < count; ia++) {
										string file;
										double offset;
										int colon = args[2 * ia].IndexOf(':');
										if (colon >= 0)
										{
											file = args[2 * ia].Substring(0, colon).TrimEnd(new char[] { });
											string value = args[2 * ia].Substring(colon + 1).TrimStart(new char[] { });
											if (!double.TryParse(value, NumberStyles.Float, Culture, out offset)) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "The track position offset " + value + " is invalid in " + t + Epilog);
												break;
											}
										} else {
											file = args[2 * ia];
											offset = 0.0;
										}
										files[ia] = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), file);
										offsets[ia] = offset;
										if (!System.IO.File.Exists(files[ia])) {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "The file " + file + " could not be found in " + t + Epilog);
											for (int ta = i; ta < Expressions.Length - 1; ta++)
											{
												Expressions[ta] = Expressions[ta + 1];
											}
											Array.Resize<Expression>(ref Expressions, Expressions.Length - 1);
											i--;
											break;
										}
										if (2 * ia + 1 < args.Length)
										{
											if (!NumberFormats.TryParseDoubleVb6(args[2 * ia + 1], out weights[ia])) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "A weight is invalid in " + t + Epilog);
												break;
											}
											if (weights[ia] <= 0.0) {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "A weight is not positive in " + t + Epilog);
												break;
											}
											weightsTotal += weights[ia];
										}
										else {
											weights[ia] = 1.0;
											weightsTotal += 1.0;
										}
									}
									if (count == 0) {
										continueWithNextExpression = true;
										Interface.AddMessage(MessageType.Error, false, "No file was specified in " + t + Epilog);
										break;
									}
									if (!continueWithNextExpression) {
										double number = Program.RandomNumberGenerator.NextDouble() * weightsTotal;
										double value = 0.0;
										int chosenIndex = 0;
										for (int ia = 0; ia < count; ia++) {
											value += weights[ia];
											if (value > number) {
												chosenIndex = ia;
												break;
											}
										}
										Expression[] expr;
										//Get the text encoding of our $Include file
										System.Text.Encoding includeEncoding = TextEncoding.GetSystemEncodingFromFile(files[chosenIndex]);
										if (!includeEncoding.Equals(Encoding) && includeEncoding.WindowsCodePage != Encoding.WindowsCodePage)
										{
											//If the encodings do not match, add a warning
											//This is not critical, but it's a bad idea to mix and match character encodings within a routefile, as the auto-detection may sometimes be wrong
											Interface.AddMessage(MessageType.Warning, false, "The text encoding of the $Include file " + files[chosenIndex] + " does not match that of the base routefile.");
										}
										string[] lines = System.IO.File.ReadAllLines(files[chosenIndex], includeEncoding);
										PreprocessSplitIntoExpressions(files[chosenIndex], IsRW, lines, out expr, false, offsets[chosenIndex] + Expressions[i].TrackPositionOffset);
										int length = Expressions.Length;
										if (expr.Length == 0) {
											for (int ia = i; ia < Expressions.Length - 1; ia++) {
												Expressions[ia] = Expressions[ia + 1];
											}
											Array.Resize<Expression>(ref Expressions, length - 1);
										} else {
											Array.Resize<Expression>(ref Expressions, length + expr.Length - 1);
											for (int ia = Expressions.Length - 1; ia >= i + expr.Length; ia--) {
												Expressions[ia] = Expressions[ia - expr.Length + 1];
											}
											for (int ia = 0; ia < expr.Length; ia++) {
												Expressions[i + ia] = expr[ia];
											}
										}
										i--;
										continueWithNextExpression = true;
									}
									break;
								case "$chr":
								case "$chruni":
									{
										int x;
										if (NumberFormats.TryParseIntVb6(s, out x)) {
											if (x < 0)
											{
												//Must be non-negative
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index must be a non-negative character in " + t + Epilog);
											}
											else
											{
												Expressions[i].Text = Expressions[i].Text.Substring(0, j) + char.ConvertFromUtf32(x) + Expressions[i].Text.Substring(h + 1);
											}
										}
										else {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
										}
									} break;
								case "$chrascii":
								{
									int x;
									if (NumberFormats.TryParseIntVb6(s, out x))
									{
										if (x < 0 || x > 128)
										{
											//Standard ASCII characters from 0-128
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Index does not correspond to a valid ASCII character in " + t + Epilog);
										}
										else
										{
											Expressions[i].Text = Expressions[i].Text.Substring(0, j) + char.ConvertFromUtf32(x) + Expressions[i].Text.Substring(h + 1);
										}
									}
									else
									{
										continueWithNextExpression = true;
										Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
									}
								}
									break;
								case "$rnd":
									{
										int m = s.IndexOf(";", StringComparison.Ordinal);
										if (m >= 0)
										{
											string s1 = s.Substring(0, m).TrimEnd(new char[] { });
											string s2 = s.Substring(m + 1).TrimStart(new char[] { });
											int x; if (NumberFormats.TryParseIntVb6(s1, out x)) {
												int y; if (NumberFormats.TryParseIntVb6(s2, out y)) {
													int z = x + (int)Math.Floor(Program.RandomNumberGenerator.NextDouble() * (double)(y - x + 1));
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + z.ToString(Culture) + Expressions[i].Text.Substring(h + 1);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index2 is invalid in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index1 is invalid in " + t + Epilog);
											}
										} else {
											continueWithNextExpression = true;
											Interface.AddMessage(MessageType.Error, false, "Two arguments are expected in " + t + Epilog);
										}
									} break;
								case "$sub":
									{
										l = 0;
										bool f = false;
										int m;
										for (m = h + 1; m < Expressions[i].Text.Length; m++) {
											switch (Expressions[i].Text[m]) {
													case '(': l++; break;
													case ')': l--; break;
													case '=': if (l == 0) {
														f = true;
													}
													break;
												default:
													if (!char.IsWhiteSpace(Expressions[i].Text[m])) l = -1;
													break;
											}
											if (f | l < 0) break;
										}
										if (f) {
											l = 0;
											int n;
											for (n = m + 1; n < Expressions[i].Text.Length; n++) {
												switch (Expressions[i].Text[n]) {
														case '(': l++; break;
														case ')': l--; break;
												}
												if (l < 0) break;
											}
											int x;
											if (NumberFormats.TryParseIntVb6(s, out x)) {
												if (x >= 0) {
													while (x >= Subs.Length) {
														Array.Resize<string>(ref Subs, Subs.Length << 1);
													}
													Subs[x] = Expressions[i].Text.Substring(m + 1, n - m - 1).Trim(new char[] { });
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Expressions[i].Text.Substring(n);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index is expected to be non-negative in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
											}
										} else {
											int x;
											if (NumberFormats.TryParseIntVb6(s, out x)) {
												if (x >= 0 & x < Subs.Length && Subs[x] != null) {
													Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Subs[x] + Expressions[i].Text.Substring(h + 1);
												} else {
													continueWithNextExpression = true;
													Interface.AddMessage(MessageType.Error, false, "Index is out of range in " + t + Epilog);
												}
											} else {
												continueWithNextExpression = true;
												Interface.AddMessage(MessageType.Error, false, "Index is invalid in " + t + Epilog);
											}
										}
										
									}
									break;
							}
						}
					}
					if (continueWithNextExpression) {
						break;
					}
				}
			}
			// handle comments introduced via chr, rnd, sub
			{
				int length = Expressions.Length;
				for (int i = 0; i < length; i++) {
					Expressions[i].Text = Expressions[i].Text.Trim(new char[] { });
					if (Expressions[i].Text.Length != 0) {
						if (Expressions[i].Text[0] == ';') {
							for (int j = i; j < length - 1; j++) {
								Expressions[j] = Expressions[j + 1];
							}
							length--;
							i--;
						}
					} else {
						for (int j = i; j < length - 1; j++) {
							Expressions[j] = Expressions[j + 1];
						}
						length--;
						i--;
					}
				}
				if (length != Expressions.Length) {
					Array.Resize<Expression>(ref Expressions, length);
				}
			}
		}

		private static void PreprocessSortByTrackPosition(bool IsRW, double[] UnitFactors, ref Expression[] Expressions) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			PositionedExpression[] p = new PositionedExpression[Expressions.Length];
			int n = 0;
			double a = -1.0;
			bool NumberCheck = !IsRW;
			for (int i = 0; i < Expressions.Length; i++) {
				if (IsRW) {
					// only check for track positions in the railway section for RW routes
					if (Expressions[i].Text.StartsWith("[", StringComparison.Ordinal) & Expressions[i].Text.EndsWith("]", StringComparison.Ordinal)) {
						string s = Expressions[i].Text.Substring(1, Expressions[i].Text.Length - 2).Trim(new char[] { });
						if (string.Compare(s, "Railway", StringComparison.OrdinalIgnoreCase) == 0) {
							NumberCheck = true;
						} else {
							NumberCheck = false;
						}
					}
				}
				double x;
				if (NumberCheck && NumberFormats.TryParseDouble(Expressions[i].Text, UnitFactors, out x)) {
					x += Expressions[i].TrackPositionOffset;
					if (x >= 0.0) {
						if (Interface.CurrentOptions.EnableBveTsHacks)
						{
							switch (System.IO.Path.GetFileName(Expressions[i].File.ToLowerInvariant()))
							{
								case "balloch - dumbarton central special nighttime run.csv":
								case "balloch - dumbarton central summer 2004 morning run.csv":
									if (x != 0 || a != 4125)
									{
										//Misplaced comma in the middle of the line causes this to be interpreted as a track position
										a = x;
									}
									break;
								default:
									a = x;
									break;
							}
						}
						else
						{
							a = x;
						}
						
					} else {
						Interface.AddMessage(MessageType.Error, false, "Negative track position encountered at line " + Expressions[i].Line.ToString(Culture) + ", column " + Expressions[i].Column.ToString(Culture) + " in file " + Expressions[i].File);
					}
				} else {
					p[n].TrackPosition = a;
					p[n].Expression = Expressions[i];
					int j = n;
					n++;
					while (j > 0) {
						if (p[j].TrackPosition < p[j - 1].TrackPosition) {
							PositionedExpression t = p[j];
							p[j] = p[j - 1];
							p[j - 1] = t;
							j--;
						} else {
							break;
						}
					}
				}
			}
			a = -1.0;
			Expression[] e = new Expression[Expressions.Length];
			int m = 0;
			for (int i = 0; i < n; i++) {
				if (p[i].TrackPosition != a) {
					a = p[i].TrackPosition;
					e[m] = new Expression
					{
						Text = (a / UnitFactors[UnitFactors.Length - 1]).ToString(Culture),
						Line = -1,
						Column = -1
					};
					m++;
				}
				e[m] = p[i].Expression;
				m++;
			}
			Array.Resize<Expression>(ref e, m);
			Expressions = e;
		}
	}
}
