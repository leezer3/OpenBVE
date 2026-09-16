using System;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;

namespace Plugin
{
	public partial class Plugin : ObjectInterface
	{
		private HostInterface currentHost;

		public override string[] SupportedAnimatedObjectExtensions => new[] { ".animated" };

		public override void Load(HostInterface host, FileSystem fileSystem)
		{
			currentHost = host;
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

		public override bool LoadObject(string path, Encoding textEncoding, out UnifiedObject unifiedObject)
		{
			try
			{
				unifiedObject = ReadObject(path, textEncoding);
				if (unifiedObject == null)
				{
					return false;
				}
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
