using System;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Plugin
{
	public class Plugin : ObjectInterface
	{
		internal static HostInterface currentHost;

		internal static string LoksimPackageFolder;

		public override void Load(HostInterface host, FileSystem fileSystem)
		{
			currentHost = host;
			LoksimPackageFolder = fileSystem.LoksimPackageInstallationDirectory;
		}


		public override bool CanLoadObject(string path)
		{
			if (path.ToLowerInvariant().EndsWith(".l3dobj", StringComparison.InvariantCultureIgnoreCase) || path.ToLowerInvariant().EndsWith(".l3dgrp", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		public override bool LoadObject(string path, Encoding Encoding, out UnifiedObject unifiedObject)
		{
			try
			{
				if (path.ToLowerInvariant().EndsWith(".l3dobj", StringComparison.InvariantCultureIgnoreCase))
				{
					unifiedObject = Ls3DObjectParser.ReadObject(path, Vector3.Zero);
				}
				else
				{
					unifiedObject = Ls3DGrpParser.ReadObject(path, Encoding, Vector3.Zero);
				}
			}
			catch
			{
				unifiedObject = null;
				return false;
			}

			return true;
		}
	}
}
