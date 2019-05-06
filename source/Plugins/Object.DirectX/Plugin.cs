using System;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
    public class Plugin : ObjectInterface
    {
	    internal static HostInterface currentHost;
	    private static XParsers currentXParser = XParsers.Original;

	    public override void Load(HostInterface host, string compatibilityFolder) {
		    currentHost = host;
	    }
		
	    public override void SetObjectParser(object parserType)
	    {
		    if (parserType is XParsers)
		    {
			    currentXParser = (XParsers) parserType;
		    }
	    }

	    public override bool CanLoadObject(string path)
	    {
		    byte[] Data = System.IO.File.ReadAllBytes(path);
		    if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32)
		    {
			    // not an x object
			    return false;
		    }

		    if (Data[4] != 48 | Data[5] != 51 | Data[6] != 48 | Data[7] != 50 & Data[7] != 51)
		    {
			    // unrecognized version
			    return false;
		    }

		    // floating-point format
		    if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50)
		    {
				//32-bit FP
		    }
		    else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52)
		    {
				//64-bit FP
		    }
		    else
		    {
			    return false;
		    }
		    return true;
	    }

	    public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject unifiedObject)
	    {
		    try
		    {   
			    if (currentXParser != XParsers.Original)
			    {
				    try
				    {
					    if (currentXParser == XParsers.NewXParser)
					    {
						    unifiedObject = NewXParser.ReadObject(path, Encoding);
						    return true;
					    }
					    unifiedObject = AssimpXParser.ReadObject(path);
					    return true;
				    }
				    catch (Exception ex)
				    {
					    currentHost.AddMessage(MessageType.Error, false, "The new X parser raised the following exception: " + ex);
					    unifiedObject = XObjectParser.ReadObject(path, Encoding);
					    return true;
				    }
			    }
			    unifiedObject = XObjectParser.ReadObject(path, Encoding);
			    return true;
		    }
		    catch
		    {
			    unifiedObject = null;
			    currentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following object: " + path);
		    }
		    return false;
	    }
    }
}
