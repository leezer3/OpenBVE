//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using Ionic.Zlib;
// ReSharper disable UnusedVariable

namespace Plugin
{
	// ReSharper disable once InconsistentNaming
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

#pragma warning disable CS0219
			// Read file size after decompression excluding header
			uint uncompressedFinalSize = BitConverter.ToUInt32(Data, p) - 16;
			p += 4;
#pragma warning restore CS0219

            // Preparing for decompression
            MemoryStream inputStream = new MemoryStream(Data);
            MemoryStream outputStream = new MemoryStream();
			int currentBlock = 0;
			byte[] previousBlockBytes = new byte[(int) MSZIP_BLOCK];
			while (p + 3 < end)
			{

				// Read compressed block size after decompression
#pragma warning disable CS0219
				ushort uncompressedBlockSize = BitConverter.ToUInt16(Data, p);
#pragma warning restore CS0219
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
				byte[] blockBytes = new byte[compressedBlockSize];
				int numBytesRead = inputStream.Read(blockBytes, 0, compressedBlockSize);

				// NOTE: Some MSZip objects have a truncated final block
				//		 To decode these correctly, the block byte array must be zero-padded to compressedBlockSize
				//		 The compiler warning is incorrect in this case (YUCK)- Don't message except in debug builds
				//		 https://github.com/leezer3/OpenBVE/issues/1150
#if DEBUG
				if (numBytesRead != compressedBlockSize)
				{
					Plugin.CurrentHost.AddMessage(OpenBveApi.Interface.MessageType.Warning, false, "MSZip: Potentially truncated final block. ");
				}
#endif

				byte[] decompressedBytes = ZlibDecompressWithDictionary(blockBytes, currentBlock == 0 ? null : previousBlockBytes);

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
					
		            codec.AssertOk("SetDictionary", codec.SetDictionaryUnconditionally(dictionary));
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

        internal static void AssertOk(this ZlibCodec codec, string errorLocation, int errorCode)
        {
            if (errorCode != 0) {
                throw new InvalidOperationException("Failed with " + errorCode + "; " + errorLocation + "; " + codec.Message);
            }
        }
    }
}
