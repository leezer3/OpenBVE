using System;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
    public class Plugin : ObjectInterface
    {
	    internal static HostInterface currentHost;
	    private static ObjParsers currentObjParser = ObjParsers.Original;

	    public override void Load(HostInterface host, FileSystem fileSystem) {
		    currentHost = host;
	    }
		
	    public override void SetObjectParser(object parserType)
	    {
		    if (parserType is ObjParsers)
		    {
			    currentObjParser = (ObjParsers) parserType;
		    }
	    }

	    public override bool CanLoadObject(string path)
	    {
		    if (path.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
		    {
			    return true;
		    }
		    return false;
	    }

	    public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject unifiedObject)
	    {

		    if (currentObjParser == ObjParsers.Assimp)
		    {
			    try
			    {
				    unifiedObject = AssimpObjParser.ReadObject(path);
				    return true;
			    }
			    catch (Exception ex)
			    {
				    currentHost.AddMessage(MessageType.Error, false, "The new Obj parser raised the following exception: " + ex);
			    }
		    }
		    try
		    {   
			    unifiedObject = WavefrontObjParser.ReadObject(path, Encoding);
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
