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

			// Probe readability; retry transient locks once.
			// HResult is Windows-only, so filter by exception type instead.
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
				catch (IOException ex) when (attempt == 0 && IsTransientIoException(ex))
				{
					Thread.Sleep(100);
					continue;
				}
				catch
				{
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
			const int maxAttempts = 3;
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
				catch (IOException ex) when (IsTransientIoException(ex) && attempt + 1 < maxAttempts)
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

		private static bool IsTransientIoException(IOException ex)
		{
			// HResult is Win32-only (raw errno elsewhere), so retry by
			// exception type, any IOException except permanent path errors.
			// Parse errors and permission errors still fail fast.
			if (ex is FileNotFoundException || ex is DirectoryNotFoundException ||
			    ex is DriveNotFoundException || ex is PathTooLongException)
			{
				return false;
			}
			return true;
		}
	}
}
