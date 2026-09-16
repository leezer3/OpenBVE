using System;
using System.IO;
using System.Text;
using System.Threading;
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

		private int retryCounter = 0;

		public override bool CanLoadObject(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			try
			{
				using (FileStream fs = new FileStream(path, FileMode.Open))
				{
					// ignore- used to catch no access exceptions etc.
				}
			}
			catch
			{
				if (retryCounter == 0)
				{
					Thread.Sleep(100);
					retryCounter++;
					return CanLoadObject(path);
				}
			}

			retryCounter = 0;
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
