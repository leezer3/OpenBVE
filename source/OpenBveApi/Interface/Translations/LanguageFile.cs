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
		/// <param name="LanguageFiles">The list of found language files</param>
		/// <param name="comboboxLanguages">The combobox to populate</param>
        public static void ListLanguages(string LanguageFolder, out string[] LanguageFiles, ComboBox comboboxLanguages) {
            if (System.IO.Directory.Exists(LanguageFolder)) {
                string[] Files = System.IO.Directory.GetFiles(LanguageFolder);
                string[] LanguageNames = new string[Files.Length];
                LanguageFiles = new string[Files.Length];
                int n = 0;
                for (int i = 0; i < Files.Length; i++) {
                    string Title = System.IO.Path.GetFileName(Files[i]);
                    if (Title != null && Title.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase)) {
                        string Code = Title.Substring(0, Title.Length - 4);
                        string[] Lines = System.IO.File.ReadAllLines(Files[i], System.Text.Encoding.UTF8);
                        string Section = "";
                        string languageName = Code;
                        for (int j = 0; j < Lines.Length; j++) {
                            Lines[j] = Lines[j].Trim();
                            if (Lines[j].StartsWith("[", StringComparison.Ordinal) & Lines[j].EndsWith("]", StringComparison.Ordinal)) {
                                Section = Lines[j].Substring(1, Lines[j].Length - 2).Trim().ToLowerInvariant();
                            } else if (!Lines[j].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
                                int k = Lines[j].IndexOf('=');
                                if (k >= 0) {
                                    string Key = Lines[j].Substring(0, k).TrimEnd().ToLowerInvariant();
                                    string Value = Lines[j].Substring(k + 1).TrimStart();
                                    if (Section == "language" & Key == "name") {
                                        languageName = Value;
                                        break;
                                    }
                                }
                            }
                        }
                        LanguageFiles[n] = Files[i];
                        LanguageNames[n] = languageName;
                        n++;
                    }
                }
                Array.Resize<string>(ref LanguageFiles, n);
                Array.Resize<string>(ref LanguageNames, n);
                Array.Sort<string, string>(LanguageNames, LanguageFiles, StringComparer.InvariantCultureIgnoreCase);
                comboboxLanguages.Items.Clear();
                //Load all available languages
                for (int i = 0; i < AvailableLangauges.Count; i++) {
                    comboboxLanguages.Items.Add(AvailableLangauges[i].Name);
                }
            } else {
                LanguageFiles = new string[] { };
                comboboxLanguages.Items.Clear();
            }
        }

		/// <summary>Attempts to initialise a language</summary>
		/// <param name="LanguageFolder">The folder containing the language files</param>
		/// <param name="LanguageFiles">The list of language files</param>
		/// <param name="LanguageCodeOption">The language code to initialise</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if initialising the language succeeded</returns>
        public static bool InitLanguage(string LanguageFolder, string[] LanguageFiles, string LanguageCodeOption, ComboBox comboboxLanguages) {
            int j;
            for (j = 0; j < LanguageFiles.Length; j++) {
                string File = OpenBveApi.Path.CombineFile(LanguageFolder, LanguageCodeOption + ".cfg");
                if (string.Compare(File, LanguageFiles[j], StringComparison.OrdinalIgnoreCase) == 0) {
                    comboboxLanguages.SelectedIndex = j;
                    break;
                }
            }
            if (j == LanguageFiles.Length) {
#if !DEBUG
                try {
#endif
                    string File = OpenBveApi.Path.CombineFile(LanguageFolder, "en-US.cfg");
                    LoadLanguage(File);
                    return true;
#if !DEBUG
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
#endif
            }
            return false;
        }
		/// <summary>Attempts to set the flag image for the selected language code</summary>
		/// <param name="FlagFolder">The folder containing flag images</param>
		/// <param name="LanguageFiles">The list of language files</param>
		/// <param name="CurrentLanguageCodeArgument">The language code we wish to get the flag for</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <param name="pictureboxLanguage">A reference to the picturebox in which to display the flag</param>
		/// <returns>True if we have found and successfully loaded the flag image</returns>
        public static bool SelectedLanguage(string FlagFolder, string[] LanguageFiles, ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages, PictureBox pictureboxLanguage) {
            int i = comboboxLanguages.SelectedIndex;
            if (i >= 0 & i < LanguageFiles.Length) {
                string Code = System.IO.Path.GetFileNameWithoutExtension(LanguageFiles[i]);
#if !DEBUG
                try {
#endif
                    CurrentLanguageCode = AvailableLangauges[i].LanguageCode;
#if !DEBUG
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
#endif
#if !DEBUG
                try {
#endif
                    string File = OpenBveApi.Path.CombineFile(FlagFolder, AvailableLangauges[i].Flag);
                    if (!System.IO.File.Exists(File)) {
                        File = OpenBveApi.Path.CombineFile(FlagFolder, "unknown.png");
                    }
                    if (System.IO.File.Exists(File)) {
                        using (var fs = new System.IO.FileStream(File, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                            pictureboxLanguage.Image = System.Drawing.Image.FromStream(fs);
                        }
                    } else {
                        pictureboxLanguage.Image = null;
                    }
                    CurrentLanguageCodeArgument = Code;
#if !DEBUG
                } catch { }
#endif
                return true;
            }
            return false;
        }

		/// <summary>Selects a language</summary>
		/// <param name="LanguageFiles">The list of language files</param>
		/// <param name="CurrentLanguageCodeArgument">The language code to select</param>
		/// <param name="comboboxLanguages">A reference to the combobox used to select the UI language</param>
		/// <returns>True if the language was found and selected successfully</returns>
        public static bool SelectedLanguage(string[] LanguageFiles, ref string CurrentLanguageCodeArgument, ComboBox comboboxLanguages) {
            int i = comboboxLanguages.SelectedIndex;
            if (i >= 0 & i < LanguageFiles.Length) {
                string Code = System.IO.Path.GetFileNameWithoutExtension(LanguageFiles[i]);
#if !DEBUG
                try {
#endif
                    CurrentLanguageCode = AvailableLangauges[i].LanguageCode;
#if !DEBUG
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
#endif
#if !DEBUG
                try {
#endif
                    CurrentLanguageCodeArgument = Code;
#if !DEBUG
                } catch { }
#endif
                return true;
            }
            return false;
        }

        /// <summary>Loads a translation from a language file</summary>
        /// <param name="File">The absolute on-disk path to the language file we wish to load</param>
        internal static void LoadLanguage(string File)
        {
            try
            {
                string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
                string Section = "";
                InterfaceStrings = new InterfaceString[16];
                InterfaceStringCount = 0;
                QuickReferences.HandleForward = "F";
                QuickReferences.HandleNeutral = "N";
                QuickReferences.HandleBackward = "B";
                QuickReferences.HandlePower = "P";
                QuickReferences.HandlePowerNull = "N";
                QuickReferences.HandleBrake = "B";
                QuickReferences.HandleBrakeNull = "N";
                QuickReferences.HandleRelease = "RL";
                QuickReferences.HandleLap = "LP";
                QuickReferences.HandleService = "SV";
                QuickReferences.HandleEmergency = "EM";
                QuickReferences.HandleHoldBrake = "HB";
                QuickReferences.DoorsLeft = "L";
                QuickReferences.DoorsRight = "R";
                QuickReferences.Score = "Score: ";
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
                                            case "forward": QuickReferences.HandleForward = b; break;
                                            case "neutral": QuickReferences.HandleNeutral = b; break;
                                            case "backward": QuickReferences.HandleBackward = b; break;
                                            case "power": QuickReferences.HandlePower = b; break;
                                            case "powernull": QuickReferences.HandlePowerNull = b; break;
                                            case "brake": QuickReferences.HandleBrake = b; break;
	                                        case "locobrake": QuickReferences.HandleLocoBrake = b; break;
                                            case "brakenull": QuickReferences.HandleBrakeNull = b; break;
                                            case "release": QuickReferences.HandleRelease = b; break;
                                            case "lap": QuickReferences.HandleLap = b; break;
                                            case "service": QuickReferences.HandleService = b; break;
                                            case "emergency": QuickReferences.HandleEmergency = b; break;
                                            case "holdbrake": QuickReferences.HandleHoldBrake = b; break;
                                        } break;
                                    case "doors":
                                        switch (a)
                                        {
                                            case "left": QuickReferences.DoorsLeft = b; break;
                                            case "right": QuickReferences.DoorsRight = b; break;
                                        } break;
                                    case "misc":
                                        switch (a)
                                        {
                                            case "score": QuickReferences.Score = b; break;
                                        } break;
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
                                        } break;
                                    case "keys":
                                        {
                                            for (int k = 0; k < TranslatedKeys.Length; k++)
                                            {
                                                if (string.Compare(TranslatedKeys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    TranslatedKeys[k].Description = b;
                                                    break;
                                                }
                                            }

                                        } break;
                                    default:
                                        AddInterfaceString(Section + "_" + a, b);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
                MessageBox.Show(@"An error occurred whilst attempting to load the selected language file.");
                Environment.Exit(0);
            }
        }

	    internal static readonly List<Language> AvailableLangauges = new List<Language>();


        /// <summary>Adds a language file to the available langauge list</summary>
        /// <param name="File">The absolute on-disk path to the language file we wish to load</param>
	    internal static void AddLanguage(string File)
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
                AvailableLangauges.Add(newLanguage);
                AvailableLangauges.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception)
            {
                //This message is shown when loading a language fails, and must not be translated, as otherwise it could produce a blank error message
                MessageBox.Show(@"An error occurred whilst attempting to load the language file: \n \n" + File);
                Environment.Exit(0);
            }
	    }

	    internal class Language
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
	    }
    }
}