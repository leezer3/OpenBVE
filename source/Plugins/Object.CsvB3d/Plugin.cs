using System;
using System.IO;
using System.Threading;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Object.CsvB3d
{
    public class Plugin : ObjectInterface
    {
	    internal HostInterface currentHost;
	    internal CompatabilityHacks enabledHacks;
	    internal string CompatibilityFolder;

	    private int retryCounter = 0;

	    public override string[] SupportedStaticObjectExtensions => new[] { ".b3d", ".csv" };

	    public override void Load(HostInterface host, FileSystem fileSystem) {
		    currentHost = host;
		    CompatibilityFolder = fileSystem.GetDataFolder("Compatibility");
	    }

	    public override void SetCompatibilityHacks(CompatabilityHacks EnabledHacks)
	    {
		    enabledHacks = EnabledHacks;
	    }


	    public override bool CanLoadObject(string path)
	    {
		    try
		    {


			    if (string.IsNullOrEmpty(path) || !File.Exists(path))
			    {
				    return false;
			    }

			    if (path.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase) ||
			        path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
			    {
				    if (FileFormats.IsNautilusFile(path))
				    {
					    retryCounter = 0;
					    return false;
				    }

				    bool currentlyLoading = false;

				    if (currentHost.Application != HostApplication.ObjectViewer)
				    {
					    for (int i = 0; i < currentHost.Plugins.Length; i++)
					    {
						    if (currentHost.Plugins[i].Route != null && currentHost.Plugins[i].Route.IsLoading ||
						        currentHost.Plugins[i].Train != null && currentHost.Plugins[i].Train.IsLoading)
						    {
							    currentlyLoading = true;
							    break;
						    }
					    }
				    }

				    try
				    {
					    using (StreamReader fileReader = new StreamReader(path))
					    {
						    for (int i = 0; i < 100; i++)
						    {
							    try
							    {
								    string readLine = fileReader.ReadLine();
								    if (!string.IsNullOrEmpty(readLine) && readLine.IndexOf("meshbuilder",
									        StringComparison.InvariantCultureIgnoreCase) != -1)
								    {
										//We have found the MeshBuilder statement within the first 100 lines
										//This must be an object (we hope)
										//Use a slightly larger value than for routes, as we're hoping for a positive match
										retryCounter = 0;
										return true;
								    }
							    }
							    catch
							    {
								    //ignored
							    }

						    }
					    }
				    }
				    catch
				    {
					    retryCounter = 0;
						return false;
				    }

				    if (currentlyLoading)
				    {
						/*
					     * https://github.com/leezer3/OpenBVE/issues/666
					     * https://github.com/leezer3/OpenBVE/issues/661
					     *
					     * In BVE routes, a null (empty) object may be used
					     * in circumstances where we want a rail / wall / ground etc.
					     * to continue, but show nothing
					     *
					     * These have no set format, and likely are undetectable, especially
					     * if they're an empty file in the first place.....
					     *
					     * However, we *still* want to be able to detect that we can't load a file
					     * and pass it off to Object Viewer if it thinks it can handle it, so we need to
					     * know if a Route Plugin is loading (if so, it must be a null object) versus not-
					     * Don't do anything
					     *
					     * TODO: Add a way to have 'proper' empty railtypes without this kludge and add appropriate info message here
					     */
						retryCounter = 0;
						return true;
				    }
			    }
			    retryCounter = 0;
				return false;
		    }
		    catch
		    {
			    if (retryCounter == 0)
			    {
				    Thread.Sleep(100);
				    retryCounter++;
				    return CanLoadObject(path);
			    }
				return false;
		    }
	    }

    public override bool LoadObject(string path, System.Text.Encoding textEncoding, out UnifiedObject unifiedObject)
    {
	    HostInterface host = currentHost;
	    CompatabilityHacks hacks = enabledHacks;
	    string compatibilityFolder = CompatibilityFolder;
	    try
	    {
		    unifiedObject = new NewParser(host, hacks, compatibilityFolder).ReadObject(path, textEncoding);
		    return true;
	    }
	    catch
	    {
		    unifiedObject = null;
			host.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following object: " + path);
	    }
	    return false;
    }
    }
}
