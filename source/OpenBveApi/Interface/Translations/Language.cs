using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace OpenBveApi.Interface
{
	public static partial class Translations
	{
		private class Language
		{
			/// <summary>The interface strings for this language</summary>
			internal readonly InterfaceString[] InterfaceStrings;
			/// <summary>The command information strings for this language</summary>
			internal readonly CommandInfo[] myCommandInfos;
			/// <summary>The key information strings for this language</summary>
			internal readonly KeyInfo[] KeyInfos;
			/// <summary>The quick-reference strings for this language</summary>
			internal readonly InterfaceQuickReference myQuickReferences;
			/// <summary>Returns the number of translated strings contained in the language</summary>
			internal int InterfaceStringCount
			{
				get
				{
					return InterfaceStrings.Length;
				}
			}
			/// <summary>The language name</summary>
			internal readonly string Name;
			/// <summary>The language flag</summary>
			internal readonly string Flag;
			/// <summary>The language code</summary>
			internal readonly string LanguageCode;
			/// <summary>The language codes on which to fall-back if a string is not found in this language(In order from best to worst)</summary>
			/// en-US should always be present in this list
			internal readonly List<String> FallbackCodes;

			/// <summary>Creates a new language from an on-disk language file</summary>
			/// <param name="languageFile">The absolute on-disk path to the language file we wish to load</param>
			internal Language(string languageFile)
			{
				Name = "Unknown";
				LanguageCode = System.IO.Path.GetFileNameWithoutExtension(languageFile);
				FallbackCodes = new List<string> {"en-US"};
				InterfaceStrings = new InterfaceString[16];
				myCommandInfos = new CommandInfo[Translations.CommandInfos.Length];
				KeyInfos = new KeyInfo[TranslatedKeys.Length];
				myQuickReferences = new InterfaceQuickReference();
				Array.Copy(Translations.CommandInfos, myCommandInfos, myCommandInfos.Length);
				Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);
				try
				{
					string[] Lines = File.ReadAllLines(languageFile, new System.Text.UTF8Encoding());
					string Section = "";
					var LoadedStringCount = 0;
					for (int i = 0; i < Lines.Length; i++)
					{
						Lines[i] = Lines[i].Trim();
						if (!Lines[i].StartsWith(";"))
						{
							if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
							}
							else
							{
								int j = Lines[i].IndexOf('=');
								if (j >= 0)
								{
									string a = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
									string b = Lines[i].Substring(j + 1).TrimStart().Unescape();
									switch (Section)
									{
										case "handles":
											switch (a)
											{
												case "forward":
													myQuickReferences.HandleForward = b;
													break;
												case "neutral":
													myQuickReferences.HandleNeutral = b;
													break;
												case "backward":
													myQuickReferences.HandleBackward = b;
													break;
												case "power":
													myQuickReferences.HandlePower = b;
													break;
												case "powernull":
													myQuickReferences.HandlePowerNull = b;
													break;
												case "brake":
													myQuickReferences.HandleBrake = b;
													break;
												case "locobrake":
													myQuickReferences.HandleLocoBrake = b;
													break;
												case "brakenull":
													myQuickReferences.HandleBrakeNull = b;
													break;
												case "release":
													myQuickReferences.HandleRelease = b;
													break;
												case "lap":
													myQuickReferences.HandleLap = b;
													break;
												case "service":
													myQuickReferences.HandleService = b;
													break;
												case "emergency":
													myQuickReferences.HandleEmergency = b;
													break;
												case "holdbrake":
													myQuickReferences.HandleHoldBrake = b;
													break;
											}
											break;
										case "doors":
											switch (a)
											{
												case "left":
													myQuickReferences.DoorsLeft = b;
													break;
												case "right":
													myQuickReferences.DoorsRight = b;
													break;
											}
											break;
										case "misc":
											switch (a)
											{
												case "score":
													myQuickReferences.Score = b;
													break;
											}
											break;
										case "commands":
											for (int k = 0; k < myCommandInfos.Length; k++)
											{
												if (string.Compare(myCommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
												{
													myCommandInfos[k].Description = b;
													break;
												}
											}
											break;
										case "keys":
											for (int k = 0; k < KeyInfos.Length; k++)
											{
												if (string.Compare(KeyInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
												{
													KeyInfos[k].Description = b;
													break;
												}
											}
											break;
										case "fallback":
											switch (a)
											{
												case "language":
													FallbackCodes.Add(b);
													break;
											}
											break;
										case "language":
											switch (a)
											{
												case "name":
													Name = b;
													break;
												case "flag":
													Flag = b;
													break;
											}
											break;
										default:
											if (LoadedStringCount >= InterfaceStrings.Length)
											{
												Array.Resize<InterfaceString>(ref InterfaceStrings,
													InterfaceStrings.Length << 1);
											}

											InterfaceStrings[LoadedStringCount].Name = Section + "_" + a;
											InterfaceStrings[LoadedStringCount].Text = b;
											LoadedStringCount++;
											break;
									}
								}
							}
						}
					}

					Array.Resize(ref InterfaceStrings, LoadedStringCount);
				}
				catch
				{
					//This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
					MessageBox.Show(@"An error occurred whilst attempting to load the language file: \n \n" + languageFile);
					//Pass the exception down the line
					//TODO: Not currently handled specifically, but may be in future
					throw;
				}
			}

			/// <summary>Creates a new language from a file stream</summary>
			/// <param name="languageStream">The file stream</param>
			/// <param name="languageCode">The language code</param>
			internal Language(Stream languageStream, string languageCode)
			{
				Name = "Unknown";
				LanguageCode = languageCode;
				FallbackCodes = new List<string> {"en-US"};
				InterfaceStrings = new InterfaceString[16];
				myCommandInfos = new CommandInfo[Translations.CommandInfos.Length];
				KeyInfos = new KeyInfo[TranslatedKeys.Length];
				myQuickReferences = new InterfaceQuickReference();
				Array.Copy(Translations.CommandInfos, myCommandInfos, myCommandInfos.Length);
				Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);
				try
				{
					string[] Lines;
					using (StreamReader reader = new StreamReader(languageStream))
					{
						Lines = reader.ReadToEnd().Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
					}

					string Section = "";
					var LoadedStringCount = 0;
					for (int i = 0; i < Lines.Length; i++)
					{
						Lines[i] = Lines[i].Trim();
						if (!Lines[i].StartsWith(";"))
						{
							if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
							{
								Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
							}
							else
							{
								int j = Lines[i].IndexOf('=');
								if (j >= 0)
								{
									string a = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
									string b = Lines[i].Substring(j + 1).TrimStart().Unescape();
									switch (Section)
									{
										case "handles":
											switch (a)
											{
												case "forward":
													myQuickReferences.HandleForward = b;
													break;
												case "neutral":
													myQuickReferences.HandleNeutral = b;
													break;
												case "backward":
													myQuickReferences.HandleBackward = b;
													break;
												case "power":
													myQuickReferences.HandlePower = b;
													break;
												case "powernull":
													myQuickReferences.HandlePowerNull = b;
													break;
												case "brake":
													myQuickReferences.HandleBrake = b;
													break;
												case "locobrake":
													myQuickReferences.HandleLocoBrake = b;
													break;
												case "brakenull":
													myQuickReferences.HandleBrakeNull = b;
													break;
												case "release":
													myQuickReferences.HandleRelease = b;
													break;
												case "lap":
													myQuickReferences.HandleLap = b;
													break;
												case "service":
													myQuickReferences.HandleService = b;
													break;
												case "emergency":
													myQuickReferences.HandleEmergency = b;
													break;
												case "holdbrake":
													myQuickReferences.HandleHoldBrake = b;
													break;
											}
											break;
										case "doors":
											switch (a)
											{
												case "left":
													myQuickReferences.DoorsLeft = b;
													break;
												case "right":
													myQuickReferences.DoorsRight = b;
													break;
											}
											break;
										case "misc":
											switch (a)
											{
												case "score":
													myQuickReferences.Score = b;
													break;
											}
											break;
										case "commands":
											for (int k = 0; k < myCommandInfos.Length; k++)
											{
												if (string.Compare(myCommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
												{
													myCommandInfos[k].Description = b;
													break;
												}
											}
											break;
										case "keys":
											for (int k = 0; k < KeyInfos.Length; k++)
											{
												if (string.Compare(KeyInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
												{
													KeyInfos[k].Description = b;
													break;
												}
											}
										break;
										case "fallback":
											switch (a)
											{
												case "language":
													FallbackCodes.Add(b);
													break;
											}
											break;
										case "language":
											switch (a)
											{
												case "name":
													Name = b;
													break;
												case "flag":
													Flag = b;
													break;
											}
											break;
										default:
											if (LoadedStringCount >= InterfaceStrings.Length)
											{
												Array.Resize<InterfaceString>(ref InterfaceStrings,
													InterfaceStrings.Length << 1);
											}
											InterfaceStrings[LoadedStringCount].Name = Section + "_" + a;
											InterfaceStrings[LoadedStringCount].Text = b;
											LoadedStringCount++;
											break;
									}
								}
							}
						}
					}
					Array.Resize(ref InterfaceStrings, LoadedStringCount);
				}
				catch
				{
				}
			}

			/// <summary>Always returns the textual name of the language</summary>
			public override string ToString()
			{
				return Name;
			}
		}
	}
}
