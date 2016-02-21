
using System;
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
                                            for (int k = 0; k < Keys.Length; k++)
                                            {
                                                if (string.Compare(Keys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    Keys[k].Description = b;
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
                MessageBox.Show("An error occurred whilst attempting to load the selected language file.");
                Environment.Exit(0);
            }
        }
    }
}
