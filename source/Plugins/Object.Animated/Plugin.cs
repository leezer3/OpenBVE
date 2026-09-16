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

		public override bool CanLoadObject(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}

			// Probe readability, retrying transient locks once.
			for (int attempt = 0; ; attempt++)
			{
				try
				{
					using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						// Probe only- catches no access exceptions etc.
					}
					break;
				}
				catch
				{
					if (attempt == 0)
					{
						Thread.Sleep(100);
						continue;
					}
					break;
				}
			}

			if (path.ToLowerInvariant().EndsWith(".animated", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		public override bool LoadObject(string path, Encoding textEncoding, out UnifiedObject unifiedObject)
		{
			// Retry transient file locks.
			const int maxAttempts = 5;
			for (int attempt = 0; ; attempt++)
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
				catch (IOException ex) when (IsFileLocked(ex) && attempt + 1 < maxAttempts)
				{
					Thread.Sleep(100);
				}
				catch
				{
					unifiedObject = null;
					return false;
				}
			}
		}

		private static bool IsFileLocked(IOException ex)
		{
			int code = ex.HResult & 0xFFFF;
			return code == 32 || code == 33; // ERROR_SHARING_VIOLATION / ERROR_LOCK_VIOLATION
		}
	}
}
