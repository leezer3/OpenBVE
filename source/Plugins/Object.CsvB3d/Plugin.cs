using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
    public partial class Plugin : ObjectInterface
    {
		private static HostInterface currentHost;
	    private static bool CylinderHack = false;
	    private static bool BveTsHacks = false;
	    private static string CompatibilityFolder;

	    public override void Load(HostInterface host, string compatibilityFolder) {
		    currentHost = host;
		    CompatibilityFolder = compatibilityFolder;
	    }

	    public override void SetCompatibilityHacks(bool EnableBveTsHacks, bool EnableCylinderHack)
	    {
		    BveTsHacks = EnableBveTsHacks;
		    CylinderHack = EnableCylinderHack;
	    }


	    public override bool CanLoadObject(string path)
	    {
		    path = path.ToLowerInvariant();
		    if (path.EndsWith(".b3d") || path.EndsWith(".csv"))
		    {
			    return true;
		    }

		    return false;
	    }

	    public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject unifiedObject)
	    {
		    try
		    {
			    unifiedObject = ReadObject(path, Encoding);
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
