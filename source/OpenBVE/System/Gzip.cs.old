using System;
using System.IO;
using System.IO.Compression;

namespace OpenBve {
	internal static class Gzip {
		
		/// <summary>Takes the argument and returns the gzip-compressed equivalent.</summary>
		/// <param name="data">The data to compress.</param>
		/// <returns>The compressed data.</returns>
		internal static byte[] Compress(byte[] data) {
			byte[] target;
			using (MemoryStream outputStream = new MemoryStream()) {
				using (GZipStream gZipStream = new GZipStream(outputStream, CompressionMode.Compress, true)) {
					gZipStream.Write(data, 0, data.Length);
				}
				target = new byte[outputStream.Length];
				outputStream.Position = 0;
				outputStream.Read(target, 0, target.Length);
			}
			return target;
		}

		/// <summary>Takes the gzip-compressed argument and returns the uncompressed equivalent.</summary>
		/// <param name="data">The data to decompress.</param>
		/// <returns>The uncompressed data.</returns>
		internal static byte[] Decompress(byte[] data) {
			byte[] target;
			using (MemoryStream inputStream = new MemoryStream(data)) {
				using (GZipStream gZipStream = new GZipStream(inputStream, CompressionMode.Decompress, true)) {
					using (MemoryStream outputStream = new MemoryStream()) {
						byte[] buffer = new byte[4096];
						while (true) {
							int count = gZipStream.Read(buffer, 0, buffer.Length);
							if (count != 0) {
								outputStream.Write(buffer, 0, count);
							}
							if (count != buffer.Length) {
								break;
							}
						}
						target = new byte[outputStream.Length];
						outputStream.Position = 0;
						outputStream.Read(target, 0, target.Length);
					}
				}
			}
			return target;
		}
		
	}
}