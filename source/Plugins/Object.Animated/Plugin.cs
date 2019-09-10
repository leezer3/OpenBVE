using System;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;

namespace Plugin
{
	public partial class Plugin : ObjectInterface
	{
		private static HostInterface currentHost;

		private static string currentSoundFolder;

		public override void Load(HostInterface host, FileSystem fileSystem)
		{
			currentHost = host;
		}

		public override void SetObjectParser(object parserType)
		{
			if (parserType is string)
			{
				//HACK: This avoids creating yet another method or override
				currentSoundFolder = (string) parserType;
			}
		}

		public override bool CanLoadObject(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			if (path.ToLowerInvariant().EndsWith(".animated", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		public override bool LoadObject(string path, Encoding Encoding, out UnifiedObject unifiedObject)
		{
			try
			{
				unifiedObject = ReadObject(path, Encoding);
				return true;
			}
			catch
			{
				unifiedObject = null;
				return false;
			}
		}
	}
}
