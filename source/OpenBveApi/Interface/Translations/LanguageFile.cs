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
                    AddLanguage(File);
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


        /// <summary>Adds a language file to the available langauge list</summary>
        /// <param name="File">The absolute on-disk path to the language file we wish to load</param>
	    private static void AddLanguage(string File)
	    {
            //Create new language
            Language newLanguage = new Language
            {
                Name = "Unknown",
                LanguageCode = System.IO.Path.GetFileNameWithoutExtension(File),
                FallbackCodes = new List<string>()
            };
            try
            {
                string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
                string Section = "";
                InterfaceString[] LoadedStrings = new InterfaceString[16];
                CommandInfo[] LoadedCommands = new CommandInfo[CommandInfos.Length];
				KeyInfo[] LoadedKeys = new KeyInfo[TranslatedKeys.Length];
				InterfaceQuickReference QuickReference = new InterfaceQuickReference();
                Array.Copy(CommandInfos, LoadedCommands, CommandInfos.Length);
				Array.Copy(TranslatedKeys, LoadedKeys, TranslatedKeys.Length);
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
                                            case "forward": QuickReference.HandleForward = b; break;
                                            case "neutral": QuickReference.HandleNeutral = b; break;
                                            case "backward": QuickReference.HandleBackward = b; break;
                                            case "power": QuickReference.HandlePower = b; break;
                                            case "powernull": QuickReference.HandlePowerNull = b; break;
                                            case "brake": QuickReference.HandleBrake = b; break;
	                                        case "locobrake": QuickReference.HandleLocoBrake = b; break;
                                            case "brakenull": QuickReference.HandleBrakeNull = b; break;
                                            case "release": QuickReference.HandleRelease = b; break;
                                            case "lap": QuickReference.HandleLap = b; break;
                                            case "service": QuickReference.HandleService = b; break;
                                            case "emergency": QuickReference.HandleEmergency = b; break;
                                            case "holdbrake": QuickReference.HandleHoldBrake = b; break;
                                        } break;
                                    case "doors":
                                        switch (a)
                                        {
                                            case "left": QuickReference.DoorsLeft = b; break;
                                            case "right": QuickReference.DoorsRight = b; break;
                                        } break;
                                    case "misc":
                                        switch (a)
                                        {
                                            case "score": QuickReference.Score = b; break;
                                        } break;
                                    case "commands":
                                        {
                                            for (int k = 0; k < LoadedCommands.Length; k++)
                                            {
                                                if (string.Compare(LoadedCommands[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    LoadedCommands[k].Description = b;
                                                    break;
                                                }
                                            }
                                        } break;
                                    case "keys":
                                        {
                                            for (int k = 0; k < LoadedKeys.Length; k++)
                                            {
                                                if (string.Compare(LoadedKeys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    LoadedKeys[k].Description = b;
                                                    break;
                                                }
                                            }
                                        } break;
                                    case "fallback":
                                        switch (a)
                                        {
                                            case "language": newLanguage.FallbackCodes.Add(b); break;
                                        } break;
                                    case "language":
                                        switch (a)
                                        {
                                            case "name": newLanguage.Name = b; break;
                                            case "flag": newLanguage.Flag = b; break;
                                        } break;

                                    default:
                                        if (LoadedStringCount >= LoadedStrings.Length)
                                        {
                                            Array.Resize<InterfaceString>(ref LoadedStrings,
                                                LoadedStrings.Length << 1);
                                        }
                                        LoadedStrings[LoadedStringCount].Name = Section + "_" + a;
                                        LoadedStrings[LoadedStringCount].Text = b;
                                        LoadedStringCount++;
                                        break;
                                }
                            }
                        }
                    }
                }
                newLanguage.InterfaceStrings = LoadedStrings;
                newLanguage.CommandInfos = LoadedCommands;
	            newLanguage.KeyInfos = LoadedKeys;
                newLanguage.InterfaceStringCount = LoadedStringCount;
                newLanguage.QuickReferences = QuickReference;
                //We should always fall-back to en-US as the last-resort before failing to load a string
                newLanguage.FallbackCodes.Add("en-US");
                AvailableLanguages.Add(newLanguage);
                AvailableLanguages.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception)
            {
                //This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
                MessageBox.Show(@"An error occurred whilst attempting to load the language file: \n \n" + File);
                Environment.Exit(0);
            }
	    }

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

	        internal int InterfaceStringCount;
            /// <summary>The language name</summary>
            internal string Name;
            /// <summary>The language flag</summary>
            internal string Flag;
            /// <summary>The language code</summary>
	        internal string LanguageCode;
            /// <summary>The language codes on which to fall-back if a string is not found in this language(In order from best to worst)</summary>
            /// en-US should always be present in this list
	        internal List<String> FallbackCodes;

		    public override string ToString()
		    {
			    return Name;
		    }
	    }
    }
}
