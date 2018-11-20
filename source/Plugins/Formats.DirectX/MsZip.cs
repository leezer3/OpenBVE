using System;
using System.IO;
using Ionic.Zlib;

namespace OpenBve.Formats.DirectX
{
    public static class MSZip
    {
		/// <summary>Decompresses a byte array compressed with MSZip compression</summary>
		/// <param name="Data">The byte array to decompress</param>
		/// <returns>The uncompressed data</returns>
		public static byte[] Decompress(byte[] Data)
		{
			uint MSZIP_SIGNATURE = 0x4B43;
	    	uint MSZIP_BLOCK = 32786;

			int p = 0;
            int end = Data.Length;

            // Skip Header
            p += 16;

            // Read file size after decompression excluding header
            uint uncompressedFinalSize = BitConverter.ToUInt32(Data, p) - 16;
            p += 4;

            // Preparing for decompression
            MemoryStream inputStream = new MemoryStream(Data);
            MemoryStream outputStream = new MemoryStream();
			int currentBlock = 0;
			byte[] previousBlockBytes = new byte[(int) MSZIP_BLOCK];
			byte[] blockBytes;
			while (p + 3 < end)
			{

				// Read compressed block size after decompression
				ushort uncompressedBlockSize = BitConverter.ToUInt16(Data, p);
				p += 2;

				// Read compressed block size
				ushort compressedBlockSize = BitConverter.ToUInt16(Data, p);
				p += 2;

				// Check compressed block size
				if (compressedBlockSize > MSZIP_BLOCK)
				{
					throw new Exception("Compressed block size is larger than MSZIP standard.");
				}

				// Check MSZIP signature of compressed block
				ushort signature = BitConverter.ToUInt16(Data, p);

				if (signature != MSZIP_SIGNATURE)
				{
					throw new Exception("The compressed block's signature is incorrect.");
				}

				// Skip MSZIP signature
				inputStream.Position = p + 2;

				// Decompress the compressed block
				blockBytes = new byte[compressedBlockSize];
				inputStream.Read(blockBytes, 0, compressedBlockSize);
				byte[] decompressedBytes;

				if (currentBlock == 0)
				{
					decompressedBytes = ZlibDecompressWithDictionary(blockBytes, null);
					//Works OK
				}
				else
				{

					decompressedBytes = ZlibDecompressWithDictionary(blockBytes, previousBlockBytes);
				}
				
				outputStream.Write(decompressedBytes, 0, decompressedBytes.Length);
				previousBlockBytes = decompressedBytes;
				
				
				// Preparing to move to the next data block
				p += compressedBlockSize;
				currentBlock++;
			}

			return outputStream.ToArray();
		}
		/// <summary>Decompressed (inflates) a compressed byte array using the Inflate algorithm.</summary>
        /// <param name="compressedData">The deflate-compressed data</param>
        /// <param name="dictionary">The dictionary originally used to compress the data, or null if no dictionary was used.</param>
        /// <returns>The uncompressed data</returns>
        private static byte[] ZlibDecompressWithDictionary(byte[] compressedData, byte[] dictionary)
        {
            using (var ms = new MemoryStream()) {
                const int bufferSize = 256;
                var buffer = new byte[bufferSize];

                var codec = new ZlibCodec {
                    InputBuffer = compressedData,
                    NextIn = 0,
                    AvailableBytesIn = compressedData.Length
                };

                codec.AssertOk("InitializeInflate", codec.InitializeInflate(false));
	            if (dictionary != null)
	            {
					
		            codec.AssertOk("SetDictionary", codec.SetDictionary(dictionary));
	            }
                codec.OutputBuffer = buffer;

                while (true) {
                    codec.NextOut = 0;
                    codec.AvailableBytesOut = bufferSize;
                    var inflateReturnCode = codec.Inflate(FlushType.None);
                    var bytesToWrite = bufferSize - codec.AvailableBytesOut;
                    ms.Write(buffer, 0, bytesToWrite);

                    if (inflateReturnCode == ZlibConstants.Z_STREAM_END) {
                        break;
                    } else if (inflateReturnCode == ZlibConstants.Z_NEED_DICT && dictionary != null) {
                        //implies bytesToWrite was 0
                        var dictionaryAdler32 = ((int)Adler.Adler32(1u, dictionary, 0, dictionary.Length));
                        if (codec.Adler32 != dictionaryAdler32) {
                            throw new InvalidOperationException("Compressed data is requesting a dictionary with adler32 " + codec.Adler32 + ", but the dictionary is actually " + dictionaryAdler32);
                        }

                        codec.AssertOk("SetDictionary", codec.SetDictionary(dictionary));
                    } else {
                        codec.AssertOk("Inflate", inflateReturnCode);
                    }
                }

                codec.AssertOk("EndInflate", codec.EndInflate());
                return ms.ToArray();
            }
        }

        internal static void AssertOk(this ZlibCodec codec, string Message, int errorCode)
        {
            if (errorCode != 0) {
                throw new InvalidOperationException("Failed with " + errorCode + "; " + Message + "; " + codec.Message);
            }
        }
    }
}
