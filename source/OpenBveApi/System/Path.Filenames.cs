﻿using System;
using System.IO;

namespace OpenBveApi
{
    public partial class Path
    {
		/// <summary>Returns whether the dat file is potentially a Mechanik DAT file</summary>
	    public static bool IsInvalidDatName(string fileName)
	    {
		    string fn = Path.GetFileName(fileName).ToLowerInvariant();
			/*
		     * Blacklist a bunch of invalid .dat files so they don't show up in the route browser
		     */
			switch (fn)
		    {
			    case "train.dat":       //BVE Train data
			    case "tekstury.dat":    //Mechanik texture list
			    case "dzweiki.dat":     //Mechanik sound list
			    case "moduly.dat":      //Mechnik route generator dat list
			    case "dzw_osob.dat":    //Mechanik route generator sound list
			    case "dzw_posp.dat":    //Mechanik route generator sound list
			    case "log.dat":         //Mechanik route generator logfile
			    case "s80_text.dat":    //S80 Mechanik routefile sounds
			    case "s80_snd.dat":     //S80 Mechanik routefile textures
			    case "gensc.dat":       //Mechanik route generator (?)
			    case "scenerio.dat":    //Mechanik route generator (?)
			    case "ntuser.dat":      //Windows user file
				    return true;
			    default:
				    return false;
		    }
	    }

		/// <summary>Returns whether the TXT file is potentially a BVE5 route</summary>
		public static bool IsInvalidTxtName(string fileName)
	    {
		    /*
		     * Unfortunately, BVE5 uses plain txt files for routes
		     *
		     * Blacklist the following (likely to require expansion):
		     * > Common route components
		     * > Legacy train.txt
		     * > Common readme names
		     * > Files over 20kb (a BVE5 scenario should be ~1kb, but let's be safe)
		     */
		    FileInfo f;
		    try
		    {
			    f = new FileInfo(fileName);
			}
		    catch
		    {
				// couldn't access the file, so unable to load either way
			    return false;
		    }
		    
		    if (f.Length > 20000)
		    {
			    return true;
		    }

		    string fn = Path.GetFileName(fileName).ToLowerInvariant();
		    if (string.IsNullOrEmpty(fn))
		    {
			    return true;
		    }

		    switch (fn)
		    {
			    case "train":
			    case "stations":
			    case "structures":
			    case "sounds":
			    case "sounds3d":
			    case "signals":
			    case "map":
				    return false;
		    }

		    if (fn.IndexOf("readme", StringComparison.InvariantCultureIgnoreCase) != -1)
		    {
			    return true;
		    }
		    if (fn.IndexOf("liesmich", StringComparison.InvariantCultureIgnoreCase) != -1)
		    {
			    return true;
		    }
		    return false;
	    }
	}
}
