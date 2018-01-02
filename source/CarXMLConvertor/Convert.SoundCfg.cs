using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;

namespace CarXmlConvertor
{
    class ConvertSoundCfg
    {
	    private static MainForm mainForm;
	    internal static Vector3 DriverPosition = new Vector3(0, 1, 0);
	    //3D center of the car
	    internal static Vector3 center = new Vector3(0.0, 0.0, 0.0);
		//Positioned to the left of the car, but centered Y & Z
	    internal static Vector3 left = new Vector3(-1.3, 0.0, 0.0);
		//Positioned to the right of the car, but centered Y & Z
	    internal static Vector3 right = new Vector3(1.3, 0.0, 0.0);
		//Positioned at the front of the car, centered X and Y
	    internal static Vector3 front = new Vector3(0.0, 0.0, 0.5 * ConvertTrainDat.CarLength);
		//Positioned at the position of the panel / 3D cab (Remember that the panel is just an object in the world...)
	    internal static Vector3 panel;
		internal static string FileName;
        internal static void Process(MainForm form)
        {
	        if (!System.IO.File.Exists(FileName))
	        {
		        return;
	        }
	        mainForm = form;
            panel = new Vector3(DriverPosition.X, DriverPosition.Y, DriverPosition.Z + 1.0);
			string[] Lines = System.IO.File.ReadAllLines(FileName);
	        TabbedList newLines = new TabbedList();
            for (int i = 0; i < Lines.Length; i++)
            {
                int j = Lines[i].IndexOf(';');
                if (j >= 0)
                {
                    Lines[i] = Lines[i].Substring(0, j).Trim();
                }
                else
                {
                    Lines[i] = Lines[i].Trim();
                }
            }
            if (Lines.Length < 1 || string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
            {
	            mainForm.updateLogBoxText += "Invalid sound.cfg format string " + Lines[0] + Environment.NewLine;
                MessageBox.Show("Invalid sound.cfg format declaration.");
                return;
            }
            newLines.Add("<CarSounds>");
            for (int i = 0; i < Lines.Length; i++)
            {
                switch (Lines[i].ToLowerInvariant())
                {
                    case "[run]":
                        newLines.Add("<Run>");
                        i++;
                        while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                int k;
                                if (!int.TryParse(a, System.Globalization.NumberStyles.Integer,
                                    CultureInfo.InvariantCulture, out k))
                                {
                                    continue;
                                }
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                if (k >= 0)
                                {
                                    newLines.Add("<Sound>");
                                    newLines.Add("<Index>" + k + "</Index>");
                                    newLines.Add("<FileName>" + b + "</FileName>");
                                    newLines.Add("<Position>0,0,0</Position>");
                                    newLines.Add("<Radius>10.0</Radius>");
                                    newLines.Add("</Sound>");
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Run>");
                        break;
	                case "[motor]":
		                newLines.Add("<Motor>");
		                i++;
		                while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
		                {
			                int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
			                if (j >= 0)
			                {
				                string a = Lines[i].Substring(0, j).TrimEnd();
				                string b = Lines[i].Substring(j + 1).TrimStart();
				                int k;
				                if (!int.TryParse(a, System.Globalization.NumberStyles.Integer,
					                CultureInfo.InvariantCulture, out k))
				                {
					                continue;
				                }
				                if (b.Length == 0 || Path.ContainsInvalidChars(b))
				                {
					                continue;
				                }
				                if (k >= 0)
				                {
					                newLines.Add("<Sound>");
					                newLines.Add("<Index>" + k + "</Index>");
					                newLines.Add("<FileName>" + b + "</FileName>");
					                newLines.Add("<Position>0,0,0</Position>");
					                newLines.Add("<Radius>10.0</Radius>");
					                newLines.Add("</Sound>");
				                }
			                }
			                i++;
		                }
		                i--;
		                newLines.Add("</Motor>");
		                break;
					case "[ats]":
                        newLines.Add("<ATS>");
                        i++;
                        while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                int k;
                                if (!int.TryParse(a, System.Globalization.NumberStyles.Integer,
                                    CultureInfo.InvariantCulture, out k))
                                {
                                    continue;
                                }
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                if (k >= 0)
                                {
                                    newLines.Add("<Sound>");
                                    newLines.Add("<Index>" + k + "</Index>");
                                    newLines.Add("<FileName>" + b + "</FileName>");
                                    newLines.Add("<Position>0,0,0</Position>");
                                    newLines.Add("<Radius>10.0</Radius>");
                                    newLines.Add("</Sound>");
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</ATS>");
                        break;
                    case "[switch]":
                        newLines.Add("<Switch>");
                        i++;
                        while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                int k;
                                if (!int.TryParse(a, System.Globalization.NumberStyles.Integer,
                                    CultureInfo.InvariantCulture, out k))
                                {
                                    continue;
                                }
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                if (k >= 0)
                                {
                                    newLines.Add("<Sound>");
                                    newLines.Add("<Index>" + k + "</Index>");
                                    newLines.Add("<FileName>" + b + "</FileName>");
                                    newLines.Add("<Position>0,0,0</Position>");
                                    newLines.Add("<Radius>10.0</Radius>");
                                    newLines.Add("</Sound>");
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Switch>");
                        break;
                    case "[brake]":
                        newLines.Add("<Brake>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "bc release high":
                                        newLines.Add("<ReleaseHigh>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</ReleaseHigh>");
                                        break;
                                    case "bc release":
                                        newLines.Add("<Release>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</Release>");
                                        break;
                                    case "bc release full":
                                        newLines.Add("<ReleaseFull>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</ReleaseFull>");
                                        break;
                                    case "emergency":
                                        newLines.Add("<Emergency>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</Emergency>");
                                        break;
                                    case "bp decomp":
                                        newLines.Add("<Application>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</Application>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Brake>");
                        break;
                    case "[compressor]":
                        newLines.Add("<Compressor>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "attack":
                                        newLines.Add("<Start>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>10.0</Radius>");
                                        newLines.Add("</Start>");
                                        break;
                                    case "loop":
                                        newLines.Add("<Loop>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>10.0</Radius>");
                                        newLines.Add("</Loop>");
                                        break;
                                    case "release":
                                        newLines.Add("<End>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>10.0</Radius>");
                                        newLines.Add("</End>");
                                        break;
                                }


                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Compressor>");
                        break;
                    case "[suspension]":
                        newLines.Add("<Suspension>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "left":
                                        newLines.Add("<Left>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</Left>");
                                        break;
                                    case "right":
                                        newLines.Add("<Right>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>5.0</Radius>");
                                        newLines.Add("</Right>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Suspension>");
                        break;
                    case "[horn]":
                        newLines.Add("<Horn>");
                        i++;
                        List<string> primary = new List<string>();
                        List<string> secondary = new List<string>();
                        List<string> music = new List<string>();
                        while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }

                                switch (a.ToLowerInvariant())
                                {
                                    //PRIMARY HORN (Enter)
                                    case "primarystart":
                                        primary.Add("<Start>");
                                        primary.Add("<FileName>" + b + "</FileName>");
                                        primary.Add("<Position>" + front + "</Position>");
                                        primary.Add("<Radius>30.0</Radius>");
                                        primary.Add("</Start>");
                                        break;
                                    case "primaryend":
                                    case "primaryrelease":
                                        primary.Add("<End>");
                                        primary.Add("<FileName>" + b + "</FileName>");
                                        primary.Add("<Position>" + front + "</Position>");
                                        primary.Add("<Radius>30.0</Radius>");
                                        primary.Add("</End>");
                                        break;
                                    case "primaryloop":
                                    case "primary":
                                        primary.Add("<Loop>");
                                        primary.Add("<FileName>" + b + "</FileName>");
                                        primary.Add("<Position>" + front + "</Position>");
                                        primary.Add("<Radius>30.0</Radius>");
                                        primary.Add("</Loop>");
                                        break;
                                    //SECONDARY HORN (Numpad Enter)
                                    case "secondarystart":
                                        secondary.Add("<Start>");
                                        secondary.Add("<FileName>" + b + "</FileName>");
                                        secondary.Add("<Position>" + front + "</Position>");
                                        secondary.Add("<Radius>30.0</Radius>");
                                        secondary.Add("</Start>");
                                        break;
                                    case "secondaryend":
                                    case "secondaryrelease":
                                        secondary.Add("<End>");
                                        secondary.Add("<FileName>" + b + "</FileName>");
                                        secondary.Add("<Position>" + front + "</Position>");
                                        secondary.Add("<Radius>30.0</Radius>");
                                        secondary.Add("</End>");
                                        break;
                                    case "secondaryloop":
                                    case "secondary":
                                        secondary.Add("<Loop>");
                                        secondary.Add("<FileName>" + b + "</FileName>");
                                        secondary.Add("<Position>" + front + "</Position>");
                                        secondary.Add("<Radius>30.0</Radius>");
                                        secondary.Add("</Loop>");
                                        break;
                                    //MUSIC HORN
                                    case "musicstart":
                                        music.Add("<Start>");
                                        music.Add("<FileName>" + b + "</FileName>");
                                        music.Add("<Position>" + front + "</Position>");
                                        music.Add("<Radius>30.0</Radius>");
                                        music.Add("</Start>");
                                        break;
                                    case "musicend":
                                    case "musicrelease":
                                        music.Add("<End>");
                                        music.Add("<FileName>" + b + "</FileName>");
                                        music.Add("<Position>" + front + "</Position>");
                                        music.Add("<Radius>30.0</Radius>");
                                        music.Add("</End>");
                                        break;
                                    case "musicloop":
                                    case "music":
                                        music.Add("<Loop>");
                                        music.Add("<FileName>" + b + "</FileName>");
                                        music.Add("<Position>" + front + "</Position>");
                                        music.Add("<Radius>30.0</Radius>");
                                        music.Add("</Loop>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        if (primary.Count != 0)
                        {
                            newLines.Add("<Primary>");
                            newLines.AddRange(primary);
                            newLines.Add("</Primary>");
                        }
                        if (secondary.Count != 0)
                        {
                            newLines.Add("<Secondary>");
                            newLines.AddRange(secondary);
                            newLines.Add("</Secondary>");
                        }
                        if (music.Count != 0)
                        {
                            newLines.Add("<Music>");
                            newLines.AddRange(music);
							newLines.Add("<Toggle>true</Toggle>");
                            newLines.Add("</Music>");
                        }
                        newLines.Add("</Horn>");
                        break;
                    case "[door]":
                        newLines.Add("<Door>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                else
                                {
                                    switch (a.ToLowerInvariant())
                                    {
                                        case "open left":
                                            newLines.Add("<OpenLeft>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>-1.3,0,0</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</OpenLeft>");
                                            break;
                                        case "open right":
                                            newLines.Add("<OpenRight>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>1.3,0,0</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</OpenRight>");
                                            break;
                                        case "close left":
                                            newLines.Add("<CloseLeft>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>-1.3,0,0</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</CloseLeft>");
                                            break;
                                        case "close right":
                                            newLines.Add("<CloseRight>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>1.3,0,0</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</CloseRight>");
                                            break;
                                    }
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Door>");
                        break;
                    case "[buzzer]":
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "correct":
                                        newLines.Add("<Buzzer>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Buzzer>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--; break;
                    case "[pilot lamp]":
                        newLines.Add("<PilotLamp>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "on":
                                        newLines.Add("<On>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</On>");
                                        break;
                                    case "off":
                                        newLines.Add("<Off>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Off>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</PilotLamp>");
                        break;
                    case "[brake handle]":
                        newLines.Add("<BrakeHandle>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "apply":
                                        newLines.Add("<Apply>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Apply>");
                                        break;
                                    case "release":
                                        newLines.Add("<Release>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Release>");
                                        break;
                                    case "min":
                                        newLines.Add("<Minimum>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Minimum>");
                                        break;
                                    case "max":
                                        newLines.Add("<Maximum>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Maximum>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</BrakeHandle>");
                        break;
                    case "[master controller]":
                        newLines.Add("<PowerHandle>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                else
                                {
                                    switch (a.ToLowerInvariant())
                                    {
                                        case "up":
                                            newLines.Add("<Increase>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>2.0</Radius>");
                                            newLines.Add("</Increase>");
                                            break;
                                        case "down":
                                            newLines.Add("<Decrease>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>2.0</Radius>");
                                            newLines.Add("</Decrease>");
                                            break;
                                        case "min":
                                            newLines.Add("<Minimum>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>2.0</Radius>");
                                            newLines.Add("</Minimum>");
                                            break;
                                        case "max":
                                            newLines.Add("<Maximum>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>2.0</Radius>");
                                            newLines.Add("</Maximum>");
                                            break;

                                    }
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</PowerHandle>");
                        break;
                    case "[reverser]":
                        newLines.Add("<Reverser>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "on":
                                        newLines.Add("<On>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</On>");
                                        break;
                                    case "off":
                                        newLines.Add("<Off>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>" + panel + "</Position>");
                                        newLines.Add("<Radius>2.0</Radius>");
                                        newLines.Add("</Off>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Reverser>");
                        break;
                    case "[breaker]":
                        newLines.Add("<Breaker>");
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                else
                                {
                                    switch (a.ToLowerInvariant())
                                    {
                                        case "on":
                                            newLines.Add("<On>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</On>");
                                            break;
                                        case "off":
                                            newLines.Add("<Off>");
                                            newLines.Add("<FileName>" + b + "</FileName>");
                                            newLines.Add("<Position>" + panel + "</Position>");
                                            newLines.Add("<Radius>5.0</Radius>");
                                            newLines.Add("</Off>");
                                            break;
                                    }
                                }
                            }
                            i++;
                        }
                        i--;
                        newLines.Add("</Breaker>");
                        break;
                    case "[others]":
                        i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal))
                        {
                            int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                            if (j >= 0)
                            {
                                string a = Lines[i].Substring(0, j).TrimEnd();
                                string b = Lines[i].Substring(j + 1).TrimStart();
                                if (b.Length == 0 || Path.ContainsInvalidChars(b))
                                {
                                    continue;
                                }
                                switch (a.ToLowerInvariant())
                                {
                                    case "noise":
                                        newLines.Add("<Loop>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>10.0</Radius>");
                                        newLines.Add("</Loop>");
                                        break;
                                    case "shoe":
                                        newLines.Add("<Rub>");
                                        newLines.Add("<FileName>" + b + "</FileName>");
                                        newLines.Add("<Position>0,0,0</Position>");
                                        newLines.Add("<Radius>10.0</Radius>");
                                        newLines.Add("</Rub>");
                                        break;
                                }
                            }
                            i++;
                        }
                        i--;
                        break;
                }
            }
            newLines.Add("</CarSounds>");
            newLines.Add("</openBVE>");
	        string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "sound.xml");
			try
            {
                
                using (StreamWriter sw = new StreamWriter(fileOut))
                {
                    foreach (String s in newLines.Lines)
                        sw.WriteLine(s);
                }
            }
            catch
            {
	            mainForm.updateLogBoxText += "Error writing file " + fileOut + Environment.NewLine;
                MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
        }
    }
}
