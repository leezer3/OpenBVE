using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenBveApi.Interface {
	public static partial class Translations {

		/// <summary>Loads all available language files from the specificed folder</summary>
        public static void LoadLanguageFiles(string LanguageFolder) {
            try {
                string[] LanguageFiles = System.IO.Directory.GetFiles(LanguageFolder, "*.cfg");
                foreach (var File in LanguageFiles) {
					Language l = new Language(File);
					AvailableLanguages.Add(l);
                }
            } catch {
                MessageBox.Show(@"An error occured whilst attempting to load the default language files.");
            }
        }

		/// <summary>Populates a list of languages in a combobox</summary>
		/// <param name="LanguageFolder">The folder in which to search for language files</param>
		/// <param name="comboboxLanguages">The combobox to populate</param>
        public static void ListLanguages(string LanguageFolder, ComboBox comboboxLanguages) {
            if (System.IO.Directory.Exists(LanguageFolder)) {
                comboboxLanguages.Items.Clear();
                //Load all available languages
	            int idx = -1;
                for (int i = 0; i < AvailableLanguages.Count; i++)
                {
                    comboboxLanguages.Items.Add(AvailableLanguages[i]);
	                if (AvailableLanguages[i].LanguageCode == CurrentLanguageCode)
	                {
		                idx = i;
	                }
                }

	            if (idx != -1)
	            {
		            comboboxLanguages.SelectedIndex = idx;
	            }
            }
			else
			{
				comboboxLanguages.Items.Clear();
			}
		}

		/// <summary>Attempts to set the flag image for the selected language code</summary>
		/// <param name="FlagFolder">The folder containing flag images</param>
		/// <param name="CurrentLanguageCodeArgument">The language code we wish to get the flag for</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <param name="pictureboxLanguage">A reference to the picturebox in which to display the flag</param>
		/// <returns>True if we have found and successfully loaded the flag image</returns>
        public static bool SelectedLanguage(string FlagFolder, ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages, PictureBox pictureboxLanguage)
		{
			int i = comboboxLanguages.SelectedIndex;
			if (i != -1)
			{
				Language l = comboboxLanguages.Items[i] as Language;
				if (l == null)
				{
					return false;
				}
				CurrentLanguageCode = l.LanguageCode;
				CurrentLanguageCodeArgument = l.LanguageCode;
				string File = Path.CombineFile(FlagFolder, l.Flag);
				if (!System.IO.File.Exists(File)) {
					File = Path.CombineFile(FlagFolder, "unknown.png");
				}
				if (System.IO.File.Exists(File)) {
					using (var fs = new System.IO.FileStream(File, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
						pictureboxLanguage.Image = System.Drawing.Image.FromStream(fs);
					}
				} else {
					pictureboxLanguage.Image = null;
				}
				return true;
			}
            return false;
        }

		/// <summary>Selects a language</summary>
		/// <param name="CurrentLanguageCodeArgument">The language code to select</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if the language was found and selected successfully</returns>
        public static bool SelectedLanguage(ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages) {
            int i = comboboxLanguages.SelectedIndex;
			if (i != -1)
			{
				Language l = comboboxLanguages.Items[i] as Language;
				if (l == null)
				{
					return false;
				}
				CurrentLanguageCode = l.LanguageCode;
				CurrentLanguageCodeArgument = l.LanguageCode;
				return true;
			}
            return false;
        }

		private static readonly List<Language> AvailableLanguages = new List<Language>();

	    private class Language
	    {
            /// <summary>The interface strings for this language</summary>
	        internal InterfaceString[] InterfaceStrings;
            /// <summary>The command information strings for this language</summary>
	        internal CommandInfo[] CommandInfos;
			/// <summary>The key information strings for this language</summary>
			internal KeyInfo[] KeyInfos;
            /// <summary>The quick-reference strings for this language</summary>
	        internal InterfaceQuickReference QuickReferences;

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

		    public Language(string languageFile)
		    {
			    Name = "Unknown";
			    LanguageCode = System.IO.Path.GetFileNameWithoutExtension(languageFile);
			    FallbackCodes = new List<string> { "en-US" };
			    InterfaceStrings = new InterfaceString[16];
			    CommandInfos = new CommandInfo[Translations.CommandInfos.Length];
			    KeyInfos = new KeyInfo[TranslatedKeys.Length];
				QuickReferences = new InterfaceQuickReference();
			    Array.Copy(Translations.CommandInfos, CommandInfos, CommandInfos.Length);
			    Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);
			    try
			    {
				    string[] Lines = System.IO.File.ReadAllLines(languageFile, new System.Text.UTF8Encoding());
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
												    QuickReferences.HandleForward = b;
												    break;
											    case "neutral":
												    QuickReferences.HandleNeutral = b;
												    break;
											    case "backward":
												    QuickReferences.HandleBackward = b;
												    break;
											    case "power":
												    QuickReferences.HandlePower = b;
												    break;
											    case "powernull":
												    QuickReferences.HandlePowerNull = b;
												    break;
											    case "brake":
												    QuickReferences.HandleBrake = b;
												    break;
											    case "locobrake":
												    QuickReferences.HandleLocoBrake = b;
												    break;
											    case "brakenull":
												    QuickReferences.HandleBrakeNull = b;
												    break;
											    case "release":
												    QuickReferences.HandleRelease = b;
												    break;
											    case "lap":
												    QuickReferences.HandleLap = b;
												    break;
											    case "service":
												    QuickReferences.HandleService = b;
												    break;
											    case "emergency":
												    QuickReferences.HandleEmergency = b;
												    break;
											    case "holdbrake":
												    QuickReferences.HandleHoldBrake = b;
												    break;
										    }

										    break;
									    case "doors":
										    switch (a)
										    {
											    case "left":
												    QuickReferences.DoorsLeft = b;
												    break;
											    case "right":
												    QuickReferences.DoorsRight = b;
												    break;
										    }

										    break;
									    case "misc":
										    switch (a)
										    {
											    case "score":
												    QuickReferences.Score = b;
												    break;
										    }

										    break;
									    case "commands":
									    {
										    for (int k = 0; k < CommandInfos.Length; k++)
										    {
											    if (string.Compare(CommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
											    {
												    CommandInfos[k].Description = b;
												    break;
											    }
										    }
									    }
										    break;
									    case "keys":
									    {
										    for (int k = 0; k < KeyInfos.Length; k++)
										    {
											    if (string.Compare(KeyInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
											    {
												    KeyInfos[k].Description = b;
												    break;
											    }
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
			    catch (Exception ex)
			    {
				    //This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
				    MessageBox.Show(@"An error occurred whilst attempting to load the language file: \n \n" + languageFile);
				    Environment.Exit(0);
			    }
		    }

		    public override string ToString()
		    {
			    return Name;
		    }
	    }
    }
}
