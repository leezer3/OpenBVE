// Copyright 2016-2018 EamonNerbonne, 2018 The openBVE Project
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using Ionic.Zlib;

namespace ZlibWithDictionary
{
	internal static class DeflateCompression
	{
		/// <summary>
		/// Decompressed (inflates) a compressed byte array using the Inflate algorithm.
		/// </summary>
		/// <param name="compressedData">The deflate-compressed data</param>
		/// <param name="dictionary">The dictionary originally used to compress the data, or null if no dictionary was used.</param>
		/// <returns>The uncompressed data</returns>
		internal static byte[] ZlibDecompressWithDictionary(byte[] compressedData, byte[] dictionary)
		{
			using (var ms = new MemoryStream())
			{
				const int bufferSize = 256;
				var buffer = new byte[bufferSize];

				var codec = new ZlibCodec
				{
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

				while (true)
				{
					codec.NextOut = 0;
					codec.AvailableBytesOut = bufferSize;
					var inflateReturnCode = codec.Inflate(FlushType.None);
					var bytesToWrite = bufferSize - codec.AvailableBytesOut;
					ms.Write(buffer, 0, bytesToWrite);

					if (inflateReturnCode == ZlibConstants.Z_STREAM_END)
					{
						break;
					}
					else if (inflateReturnCode == ZlibConstants.Z_NEED_DICT && dictionary != null)
					{
						//implies bytesToWrite was 0
						var dictionaryAdler32 = ((int)Adler.Adler32(1u, dictionary, 0, dictionary.Length));
						if (codec.Adler32 != dictionaryAdler32)
						{
							throw new InvalidOperationException("Compressed data is requesting a dictionary with adler32 " + codec.Adler32 + ", but the dictionary is actually " + dictionaryAdler32);
						}

						codec.AssertOk("SetDictionary", codec.SetDictionary(dictionary));
					}
					else
					{
						codec.AssertOk("Inflate", inflateReturnCode);
					}
				}

				codec.AssertOk("EndInflate", codec.EndInflate());
				return ms.ToArray();
			}
		}

		internal static void AssertOk(this ZlibCodec codec, string Message, int errorCode)
		{
			if (errorCode != 0)
			{
				throw new InvalidOperationException("Failed with " + errorCode + "; " + Message + "; " + codec.Message);
			}
		}
	}
}
