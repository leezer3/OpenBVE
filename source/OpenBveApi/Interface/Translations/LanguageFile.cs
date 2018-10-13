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
	                try
	                {
		                Language l = new Language(File);
		                AvailableLanguages.Add(l);
	                }
	                catch { }
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
		    public Language(string languageFile)
		    {
			    Name = "Unknown";
			    LanguageCode = System.IO.Path.GetFileNameWithoutExtension(languageFile);
			    FallbackCodes = new List<string> { "en-US" };
			    InterfaceStrings = new InterfaceString[16];
			    myCommandInfos = new CommandInfo[Translations.CommandInfos.Length];
			    KeyInfos = new KeyInfo[TranslatedKeys.Length];
				myQuickReferences = new InterfaceQuickReference();
			    Array.Copy(Translations.CommandInfos, myCommandInfos, myCommandInfos.Length);
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
									    {
										    for (int k = 0; k < myCommandInfos.Length; k++)
										    {
											    if (string.Compare(myCommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
											    {
												    myCommandInfos[k].Description = b;
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
			    catch
			    {
				    //This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
				    MessageBox.Show(@"An error occurred whilst attempting to load the language file: \n \n" + languageFile);
					//Pass the exception down the line
					//TODO: Not currently handled specifically, but may be in future
				    throw;
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
