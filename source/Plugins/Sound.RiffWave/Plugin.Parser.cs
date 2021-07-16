using System;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NLayer.NAudioSupport;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin
	{

		// --- structures and enumerations ---
		private abstract class WaveFormatEx
		{
			internal ushort wFormatTag;
			internal ushort nChannels;
			internal uint nSamplesPerSec;
			internal uint nAvgBytesPerSec;
			internal ushort nBlockAlign;
			internal ushort wBitsPerSample;
			internal ushort cbSize;
		}

		private class WaveFormatPcm : WaveFormatEx
		{
		}

		private class WaveFormatAdPcm : WaveFormatEx
		{
			internal struct CoefSet
			{
				internal short iCoef1;
				internal short iCoef2;
			}

			internal struct BlockData
			{
				internal readonly byte[] bPredictor;
				internal readonly short[] iDelta;
				internal readonly short[] iSamp1;
				internal readonly short[] iSamp2;
				internal readonly CoefSet[] CoefSet;

				internal BlockData(int channels)
				{
					bPredictor = new byte[channels];
					iDelta = new short[channels];
					iSamp1 = new short[channels];
					iSamp2 = new short[channels];
					CoefSet = new CoefSet[channels];
				}
			}

			internal ushort nSamplesPerBlock;
			internal ushort nNumCoef;
			internal CoefSet[] aCoeff;

			internal static readonly short[] AdaptionTable =
			{
				230, 230, 230, 230, 307, 409, 512, 614,
				768, 614, 512, 409, 307, 230, 230, 230
			};
		}

		private class WaveFormatMp3 : WaveFormatEx
		{
		}


		// --- functions ---

		/// <summary>Reads wave data from a WAVE file.</summary>
		/// <param name="fileName">The file name of the WAVE file.</param>
		/// <returns>The wave data.</returns>
		private static Sound LoadFromFile(string fileName)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					BinaryReaderExtensions.Endianness endianness;
					uint headerCkID = reader.ReadUInt32();

					// "RIFF"
					if (headerCkID == 0x46464952)
					{
						endianness = BinaryReaderExtensions.Endianness.Little;
					}
					// "RIFX"
					else if (headerCkID == 0x58464952)
					{
						endianness = BinaryReaderExtensions.Endianness.Big;
					}
					else
					{
						// unsupported format
						throw new InvalidDataException("Invalid header chunk ID");
					}

					uint headerCkSize = reader.ReadUInt32(endianness);

					uint formType = reader.ReadUInt32(endianness);

					// "WAVE"
					if (formType == 0x45564157)
					{
						return WaveLoadFromStream(reader, endianness, headerCkSize + 8);
					}

					// unsupported format
					throw new InvalidDataException("Unsupported format");
				}
			}
		}

		/// <summary>Reads sound data from a MP3 stream.</summary>
		/// <param name="stream">The stream of the MP3.</param>
		/// <returns>The raw sound data.</returns>
		private static Sound Mp3LoadFromStream(Stream stream)
		{
			using (Mp3FileReader reader = new Mp3FileReader(stream, wf => new Mp3FrameDecompressor(wf)))
			{
				byte[] dataBytes = new byte[reader.Length];

				// Convert MP3 to raw 32-bit float n channels PCM.
				int bytesRead = reader.Read(dataBytes, 0, (int)reader.Length);

				int sampleCount = bytesRead / (reader.WaveFormat.Channels * sizeof(float));
				byte[] newDataBytes = new byte[sampleCount * reader.WaveFormat.Channels * sizeof(short)];
				byte[][] buffers = new byte[reader.WaveFormat.Channels][];

				for (int i = 0; i < buffers.Length; i++)
				{
					buffers[i] = new byte[newDataBytes.Length / buffers.Length];
				}

				// Convert PCM bit depth from 32-bit float to 16-bit integer.
				using (MemoryStream writeStream = new MemoryStream(newDataBytes))
				using (BinaryWriter writer = new BinaryWriter(writeStream))
				{
					for (int i = 0; i < bytesRead; i += sizeof(float))
					{
						float sample = BitConverter.ToSingle(dataBytes, i);

						if (sample < -1.0f)
						{
							sample = -1.0f;
						}

						if (sample > 1.0f)
						{
							sample = 1.0f;
						}

						writer.Write((short)(sample * short.MaxValue));
					}
				}

				// Separated for each channel.
				for (int i = 0; i < sampleCount; i++)
				{
					for (int j = 0; j < buffers.Length; j++)
					{
						for (int k = 0; k < sizeof(short); k++)
						{
							buffers[j][i * sizeof(short) + k] = newDataBytes[i * sizeof(short) * buffers.Length + sizeof(short) * j + k];
						}
					}
				}

				return new Sound(reader.WaveFormat.SampleRate, sizeof(short) * 8, buffers);
			}
		}

		private static Sound WaveLoadFromStream(BinaryReader reader, BinaryReaderExtensions.Endianness endianness, uint fileSize)
		{
			long stopPosition = Math.Min(fileSize, reader.BaseStream.Length);
			WaveFormatEx format = null;
			byte[] dataBytes = null;

			while (reader.BaseStream.Position + 8 <= stopPosition)
			{
				uint ckID = reader.ReadUInt32(endianness);
				uint ckSize = reader.ReadUInt32(endianness);

				// "fmt "
				if (ckID == 0x20746D66)
				{
					if (ckSize < 16)
					{
						throw new InvalidDataException("Unsupported fmt chunk size");
					}

					ushort wFormatTag = reader.ReadUInt16(endianness);

					if (wFormatTag == 0x0001 || wFormatTag == 0x0003)
					{
						format = new WaveFormatPcm { wFormatTag = wFormatTag };
					}
					else if (wFormatTag == 0x0002)
					{
						format = new WaveFormatAdPcm { wFormatTag = wFormatTag };
					}
					else if (wFormatTag == 0x0050 || wFormatTag == 0x0055)
					{
						format = new WaveFormatMp3 { wFormatTag = wFormatTag };
					}
					else
					{
						// unsupported format
						throw new InvalidDataException("Unsupported wFormatTag");
					}

					format.nChannels = reader.ReadUInt16(endianness);
					format.nSamplesPerSec = reader.ReadUInt32(endianness);
					format.nAvgBytesPerSec = reader.ReadUInt32(endianness);
					format.nBlockAlign = reader.ReadUInt16(endianness);
					format.wBitsPerSample = reader.ReadUInt16(endianness);

					if (!(format is WaveFormatAdPcm))
					{
						if (ckSize > 16)
						{
							format.cbSize = reader.ReadUInt16(endianness);

							// This is a countermeasure to the cbSize is not correct file.
							reader.BaseStream.Position += Math.Min(format.cbSize, ckSize - 18);
						}

						if (format is WaveFormatPcm)
						{
							if (format.wBitsPerSample < 1)
							{
								throw new InvalidDataException("Unsupported wBitsPerSample");
							}

							if (format.nBlockAlign != format.nChannels * format.wBitsPerSample / 8)
							{
								throw new InvalidDataException("Unexpected wBlockAlign");
							}

							if (format.nAvgBytesPerSec != format.nChannels * format.nSamplesPerSec * format.wBitsPerSample / 8)
							{
								throw new InvalidDataException("Unexpected nAvgBytesPerSec");
							}
						}
					}
					else
					{
						WaveFormatAdPcm adPcmFormat = (WaveFormatAdPcm)format;

						if (ckSize <= 16)
						{
							throw new InvalidDataException("Unsupported fmt chunk size");
						}

						adPcmFormat.cbSize = reader.ReadUInt16(endianness);
						long readerPrevPos = reader.BaseStream.Position;

						if (adPcmFormat.cbSize < 32)
						{
							throw new InvalidDataException("Unsupported fmt chunk size");
						}

						adPcmFormat.nSamplesPerBlock = reader.ReadUInt16(endianness);
						adPcmFormat.nNumCoef = reader.ReadUInt16(endianness);
						adPcmFormat.aCoeff = new WaveFormatAdPcm.CoefSet[adPcmFormat.nNumCoef];

						for (int i = 0; i < adPcmFormat.nNumCoef; i++)
						{
							adPcmFormat.aCoeff[i].iCoef1 = reader.ReadInt16(endianness);
							adPcmFormat.aCoeff[i].iCoef2 = reader.ReadInt16(endianness);
						}

						reader.BaseStream.Position += adPcmFormat.cbSize - (reader.BaseStream.Position - readerPrevPos);

						if (adPcmFormat.wBitsPerSample != 4)
						{
							throw new InvalidDataException("Unsupported wBitsPerSample");
						}

						if (adPcmFormat.nSamplesPerBlock != (adPcmFormat.nBlockAlign - 7 * adPcmFormat.nChannels) * 8 / (adPcmFormat.wBitsPerSample * adPcmFormat.nChannels) + 2)
						{
							throw new InvalidDataException("Unexpected nSamplesPerBlock");
						}
					}
				}
				// "data"
				else if (ckID == 0x61746164)
				{
					if (format == null)
					{
						// invalid
						throw new InvalidDataException("No fmt chunk before the data chunk");
					}

					if (!(format is WaveFormatAdPcm))
					{
						dataBytes = reader.ReadBytes((int)ckSize);
						break;
					}

					{
						WaveFormatAdPcm adPcmFormat = (WaveFormatAdPcm)format;
						uint blocks = ckSize / adPcmFormat.nBlockAlign;
						dataBytes = new byte[adPcmFormat.nChannels * 2 * blocks * adPcmFormat.nSamplesPerBlock];

						using (MemoryStream stream = new MemoryStream(dataBytes))
						using (BinaryWriter writer = new BinaryWriter(stream))
						{
							for (int i = 0; i < blocks; i++)
							{
								WaveFormatAdPcm.BlockData blockData = new WaveFormatAdPcm.BlockData(adPcmFormat.nChannels);
								long readerPrevPos = reader.BaseStream.Position;

								// get first data from this block header
								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									blockData.bPredictor[j] = reader.ReadByte();
								}

								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									blockData.iDelta[j] = reader.ReadInt16(endianness);
								}

								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									blockData.iSamp1[j] = reader.ReadInt16(endianness);
								}

								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									blockData.iSamp2[j] = reader.ReadInt16(endianness);
								}

								// set first coefficients
								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									if (blockData.bPredictor[j] >= adPcmFormat.nNumCoef)
									{
										throw new InvalidDataException("Invalid bPredictor");
									}

									blockData.CoefSet[j] = adPcmFormat.aCoeff[blockData.bPredictor[j]];
								}

								// the first two samples are already decoded
								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									writer.Write(blockData.iSamp2[j]);
								}

								for (int j = 0; j < adPcmFormat.nChannels; j++)
								{
									writer.Write(blockData.iSamp1[j]);
								}

								byte nibbleByte = 0;
								bool nibbleFirst = true;

								for (int j = 0; j < adPcmFormat.nSamplesPerBlock - 2; j++)
								{
									for (int k = 0; k < adPcmFormat.nChannels; k++)
									{
										int lPredSamp = (blockData.iSamp1[k] * blockData.CoefSet[k].iCoef1 + blockData.iSamp2[k] * blockData.CoefSet[k].iCoef2) / 256;
										int iErrorDeltaUnsigned;

										if (nibbleFirst)
										{
											nibbleByte = reader.ReadByte();
											nibbleFirst = false;
											iErrorDeltaUnsigned = nibbleByte >> 4;
										}
										else
										{
											nibbleFirst = true;
											iErrorDeltaUnsigned = nibbleByte & 15;
										}

										int iErrorDeltaSigned = iErrorDeltaUnsigned >= 8 ? iErrorDeltaUnsigned - 16 : iErrorDeltaUnsigned;
										int lNewSampInt = lPredSamp + blockData.iDelta[k] * iErrorDeltaSigned;
										short lNewSamp;

										if (lNewSampInt > short.MaxValue)
										{
											lNewSamp = short.MaxValue;
										}
										else if (lNewSampInt < short.MinValue)
										{
											lNewSamp = short.MinValue;
										}
										else
										{
											lNewSamp = (short)lNewSampInt;
										}

										writer.Write(lNewSamp);

										blockData.iDelta[k] = (short)(blockData.iDelta[k] * WaveFormatAdPcm.AdaptionTable[iErrorDeltaUnsigned] / 256);

										if (blockData.iDelta[k] < 16)
										{
											blockData.iDelta[k] = 16;
										}

										blockData.iSamp2[k] = blockData.iSamp1[k];
										blockData.iSamp1[k] = lNewSamp;
									}
								}

								reader.BaseStream.Position += adPcmFormat.nBlockAlign - (reader.BaseStream.Position - readerPrevPos);
							}
						}

						break;
					}
				}
				// unsupported chunk
				else
				{
					reader.BaseStream.Position += ckSize;
				}

				// pad byte
				if ((ckSize & 1) == 1)
				{
					reader.BaseStream.Position++;
				}
			}

			// finalize
			if (dataBytes == null)
			{
				throw new InvalidDataException("No data chunk before the end of the file");
			}

			if (format is WaveFormatMp3)
			{
				using (MemoryStream dataStream = new MemoryStream(dataBytes))
				{
					return Mp3LoadFromStream(dataStream);
				}
			}

			int bytesPerSample;

			if (!(format is WaveFormatAdPcm))
			{
				bytesPerSample = format.wBitsPerSample / 8;
			}
			else
			{
				bytesPerSample = 2;
			}

			// The size of the data chunk may not be correct, so we use sampleCount as the standard thereafter.
			int sampleCount = dataBytes.Length / (format.nChannels * bytesPerSample);

			byte[][] buffers = new byte[format.nChannels][];

			for (int i = 0; i < format.nChannels; i++)
			{
				buffers[i] = new byte[sampleCount * bytesPerSample];
			}

			for (int i = 0; i < sampleCount; i++)
			{
				for (int j = 0; j < format.nChannels; j++)
				{
					for (int k = 0; k < bytesPerSample; k++)
					{
						buffers[j][i * bytesPerSample + k] = dataBytes[i * bytesPerSample * format.nChannels + bytesPerSample * j + k];
					}
				}
			}

			return ChangeBitDepth(format.wFormatTag, (int)format.nSamplesPerSec, bytesPerSample, sampleCount, buffers);
		}

		private static Sound ChangeBitDepth(ushort formatTag, int samplesPerSec, int bytesPerSample, int sampleCount, byte[][] buffers)
		{
			byte[][] newBuffers = new byte[buffers.Length][];

			for (int i = 0; i < buffers.Length; i++)
			{
				newBuffers[i] = new byte[sampleCount * sizeof(short)];
			}

			switch (formatTag)
			{
				case 0x0001:
					switch (bytesPerSample)
					{
						case 3:
							for (int i = 0; i < buffers.Length; i++)
							{
								using (var stream = new MemoryStream(newBuffers[i]))
								using (var writer = new BinaryWriter(stream))
								{
									for (int j = 0; j < buffers[i].Length; j += 3)
									{
										byte[] tmp = new byte[4];
										Array.Copy(buffers[i], j, tmp, 0, 3);
										tmp[3] = (byte)(tmp[2] > 0x7F ? 0xff : 0x00);
										float sample = BitConverter.ToInt32(tmp, 0) / 8388608.0f;
										writer.Write((short)(sample * short.MaxValue));
									}
								}
							}

							bytesPerSample = sizeof(short);
							break;
						case 4:
							for (int i = 0; i < buffers.Length; i++)
							{
								using (var stream = new MemoryStream(newBuffers[i]))
								using (var writer = new BinaryWriter(stream))
								{
									for (int j = 0; j < buffers[i].Length; j += 4)
									{
										float sample = BitConverter.ToInt32(buffers[i], j) / 2147483648.0f;
										writer.Write((short)(sample * short.MaxValue));
									}
								}
							}

							bytesPerSample = sizeof(short);
							break;
						default:
							newBuffers = buffers;
							break;
					}
					break;
				case 0x0003:
					for (int i = 0; i < buffers.Length; i++)
					{
						using (var stream = new MemoryStream(newBuffers[i]))
						using (var writer = new BinaryWriter(stream))
						{
							for (int j = 0; j < buffers[i].Length; j += 4)
							{
								float sample = BitConverter.ToSingle(buffers[i], j);

								if (sample < -1.0f)
								{
									sample = -1.0f;
								}

								if (sample > 1.0f)
								{
									sample = 1.0f;
								}

								writer.Write((short)(sample * short.MaxValue));
							}
						}
					}

					bytesPerSample = sizeof(short);
					break;
				default:
					newBuffers = buffers;
					break;
			}

			return new Sound(samplesPerSec, bytesPerSample * 8, newBuffers);
		}
	}

	internal static class BinaryReaderExtensions
	{
		/// <summary>Represents the endianness of an integer.</summary>
		internal enum Endianness
		{
			/// <summary>Represents little endian byte order, i.e. least-significant byte first.</summary>
			Little = 0,

			/// <summary>Represents big endian byte order, i.e. most-significant byte first.</summary>
			Big = 1
		}

		private static byte[] Reverse(this byte[] bytes, Endianness endianness)
		{
			if (BitConverter.IsLittleEndian ^ endianness == Endianness.Little)
			{
				return bytes.Reverse().ToArray();
			}

			return bytes;
		}

		internal static ushort ReadUInt16(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToUInt16(reader.ReadBytes(sizeof(ushort)).Reverse(endianness), 0);
		}

		internal static short ReadInt16(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToInt16(reader.ReadBytes(sizeof(short)).Reverse(endianness), 0);
		}

		internal static uint ReadUInt32(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToUInt32(reader.ReadBytes(sizeof(uint)).Reverse(endianness), 0);
		}

		internal static int ReadInt32(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToInt32(reader.ReadBytes(sizeof(int)).Reverse(endianness), 0);
		}
	}
}
