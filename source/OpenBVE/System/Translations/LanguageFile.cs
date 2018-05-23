using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenBve {
	internal static partial class Interface {

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
                                string b = Interface.Unescape(Lines[i].Substring(j + 1).TrimStart());
                                switch (Section)
                                {
                                    case "handles":
                                        switch (a)
                                        {
                                            case "forward": Interface.QuickReferences.HandleForward = b; break;
                                            case "neutral": Interface.QuickReferences.HandleNeutral = b; break;
                                            case "backward": Interface.QuickReferences.HandleBackward = b; break;
                                            case "power": Interface.QuickReferences.HandlePower = b; break;
                                            case "powernull": Interface.QuickReferences.HandlePowerNull = b; break;
                                            case "brake": Interface.QuickReferences.HandleBrake = b; break;
	                                        case "locobrake": Interface.QuickReferences.HandleLocoBrake = b; break;
                                            case "brakenull": Interface.QuickReferences.HandleBrakeNull = b; break;
                                            case "release": Interface.QuickReferences.HandleRelease = b; break;
                                            case "lap": Interface.QuickReferences.HandleLap = b; break;
                                            case "service": Interface.QuickReferences.HandleService = b; break;
                                            case "emergency": Interface.QuickReferences.HandleEmergency = b; break;
                                            case "holdbrake": Interface.QuickReferences.HandleHoldBrake = b; break;
                                        } break;
                                    case "doors":
                                        switch (a)
                                        {
                                            case "left": Interface.QuickReferences.DoorsLeft = b; break;
                                            case "right": Interface.QuickReferences.DoorsRight = b; break;
                                        } break;
                                    case "misc":
                                        switch (a)
                                        {
                                            case "score": Interface.QuickReferences.Score = b; break;
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
                CommandInfo[] LoadedCommands = new CommandInfo[Interface.CommandInfos.Length];
				KeyInfo[] LoadedKeys = new KeyInfo[Interface.TranslatedKeys.Length];
				InterfaceQuickReference QuickReference = new InterfaceQuickReference();
                Array.Copy(Interface.CommandInfos, LoadedCommands, Interface.CommandInfos.Length);
				Array.Copy(Interface.TranslatedKeys, LoadedKeys, Interface.TranslatedKeys.Length);
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
                                string b = Interface.Unescape(Lines[i].Substring(j + 1).TrimStart());
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