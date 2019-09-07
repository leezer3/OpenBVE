using System;
using System.IO;
using System.Text;
using OpenBve.Formats.MsTs;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

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
			Stream fb = new FileStream(path, FileMode.Open, FileAccess.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);

			string headerString;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				headerString = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 2, 14);
				headerString = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			// SIMISA@F  means compressed
			// SIMISA@@  means uncompressed
			if (headerString.StartsWith("SIMISA@F"))
			{
				fb = new ZlibStream(fb, CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				return false;
			}

			string subHeader;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				subHeader = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 0, 16);
				subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
			}
			if (subHeader[7] == 't')
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					byte[] newBytes = reader.ReadBytes((int)(fb.Length - fb.Position));
					string s;
					if (unicode)
					{
						s = Encoding.Unicode.GetString(newBytes);
					}
					else
					{
						s = Encoding.ASCII.GetString(newBytes);
					}

					s = s.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim(new char[] { });
					if (s.StartsWith("shape", StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}
					
			}
			else if (subHeader[7] != 'b')
			{
				return false;
			}
			else
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					KujuTokenID currentToken = (KujuTokenID) reader.ReadUInt16();
					if (currentToken == KujuTokenID.shape)
					{
						return true; //Shape definition
					}
				}
			}
			return false;
		}
	

		public override bool LoadObject(string path, Encoding Encoding, out UnifiedObject unifiedObject)
		{
			try
			{
				unifiedObject = MsTsShapeParser.ReadObject(path);
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
