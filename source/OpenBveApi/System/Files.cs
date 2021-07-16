using System;
using System.IO;

namespace OpenBveApi
{
	/// <summary>Provides helper functions for working with specific file formats, which may not be uniquely identifiable</summary>
	public static class FileFormats
	{
		/// <summary>Whether the file is Nautilus</summary>
		public static bool IsNautilusFile(string path)
		{
			/*
			 * Encryption used by certain Chinese items.
			 * This is NOT supported or encouraged in any manner by the OpenBVE project,
			 * and this detection is provided as a courtesy only.
			 */
			try
			{
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					if (stream.Length < 32)
					{
						return false;
					}
					using (BinaryReader reader = new BinaryReader(stream))
					{
						byte[] readBytes = reader.ReadBytes(16);
						return (BitConverter.ToUInt32(readBytes, 0) == 0x554c544eu && BitConverter.ToUInt32(readBytes, 4) == 0x4d524453u
							&& BitConverter.ToUInt32(readBytes, 8) == 0x14131211u && BitConverter.ToUInt32(readBytes, 12) == 0x00811919u);
					}
				}
			}
			catch
			{
				return false;
			}
			
		}

	}
}
