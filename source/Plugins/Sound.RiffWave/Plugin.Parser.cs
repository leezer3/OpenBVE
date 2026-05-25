using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using NLayer.NAudioSupport;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin
	{
		


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
					Endianness endianness;
					uint headerCkID = reader.ReadUInt32();

					// "RIFF"
					if (headerCkID == 0x46464952)
					{
						endianness = Endianness.Little;
					}
					// "RIFX"
					else if (headerCkID == 0x58464952)
					{
						endianness = Endianness.Big;
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

		private static Sound WaveLoadFromStream(BinaryReader reader, Endianness endianness, uint fileSize)
		{
			long stopPosition = Math.Min(fileSize, reader.BaseStream.Length);
			WaveFormatEx format = null;
			byte[] dataBytes = null;
			CuePoint[] cuePoints = null;
			List<Chunk> chunks = new List<Chunk>();

			while (reader.BaseStream.Position + 8 <= stopPosition)
			{
				ChunkID ckID = (ChunkID)reader.ReadUInt32(endianness);
				uint ckSize = reader.ReadUInt32(endianness);

				// "fmt "
				switch (ckID)
				{
					case ChunkID.FMT:
						if (ckSize < 16)
						{
							throw new InvalidDataException("Unsupported fmt chunk size");
						}

						ushort wFormatTag = reader.ReadUInt16(endianness);

						switch (wFormatTag)
						{
							case 0x0001:
							case 0x0003:
								format = new WaveFormatPcm(wFormatTag);
								break;
							case 0x0002:
								format = new WaveFormatAdPcm(wFormatTag);
								break;
							case 0x0050:
							case 0x0055:
								format = new WaveFormatMp3(wFormatTag);
								break;
							case 0xFFFE:
								format = new WaveFormatExtensible(wFormatTag);
								break;
							default:
								// unsupported format
								throw new InvalidDataException("Unsupported wFormatTag");
						}

						format.Channels = reader.ReadUInt16(endianness);
						format.SamplesPerSec = reader.ReadUInt32(endianness);
						format.AvgBytesPerSec = reader.ReadUInt32(endianness);
						format.BlockAlign = reader.ReadUInt16(endianness);
						format.BitsPerSample = reader.ReadUInt16(endianness);

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
								if (format.BitsPerSample < 1)
								{
									throw new InvalidDataException("Unsupported wBitsPerSample");
								}

								if (format.BlockAlign != format.Channels * format.BitsPerSample / 8)
								{
									throw new InvalidDataException("Unexpected wBlockAlign");
								}

								if (format.AvgBytesPerSec != format.Channels * format.SamplesPerSec * format.BitsPerSample / 8)
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
							adPcmFormat.aCoeff = new Vector2[adPcmFormat.nNumCoef];

							for (int i = 0; i < adPcmFormat.nNumCoef; i++)
							{
								adPcmFormat.aCoeff[i].X = reader.ReadInt16(endianness);
								adPcmFormat.aCoeff[i].Y = reader.ReadInt16(endianness);
							}

							reader.BaseStream.Position += adPcmFormat.cbSize - (reader.BaseStream.Position - readerPrevPos);

							if (adPcmFormat.BitsPerSample != 4)
							{
								throw new InvalidDataException("Unsupported wBitsPerSample");
							}

							if (adPcmFormat.nSamplesPerBlock != (adPcmFormat.BlockAlign - 7 * adPcmFormat.Channels) * 8 / (adPcmFormat.BitsPerSample * adPcmFormat.Channels) + 2)
							{
								throw new InvalidDataException("Unexpected nSamplesPerBlock");
							}
						}
						break;
					// "data"
					case ChunkID.DATA:
						if (format == null)
						{
							// invalid
							throw new InvalidDataException("No fmt chunk before the data chunk");
						}

						if (!(format is WaveFormatAdPcm))
						{
							dataBytes = reader.ReadBytes((int)ckSize);
						}
						else
						{
							WaveFormatAdPcm adPcmFormat = (WaveFormatAdPcm)format;
							uint blocks = ckSize / adPcmFormat.BlockAlign;
							dataBytes = new byte[adPcmFormat.Channels * 2 * blocks * adPcmFormat.nSamplesPerBlock];

							using (MemoryStream stream = new MemoryStream(dataBytes))
							using (BinaryWriter writer = new BinaryWriter(stream))
							{
								for (int i = 0; i < blocks; i++)
								{
									WaveFormatAdPcm.BlockData blockData = new WaveFormatAdPcm.BlockData(adPcmFormat.Channels);
									long readerPrevPos = reader.BaseStream.Position;

									// get first data from this block header
									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										blockData.bPredictor[j] = reader.ReadByte();
									}

									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										blockData.iDelta[j] = reader.ReadInt16(endianness);
									}

									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										blockData.iSamp1[j] = reader.ReadInt16(endianness);
									}

									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										blockData.iSamp2[j] = reader.ReadInt16(endianness);
									}

									// set first coefficients
									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										if (blockData.bPredictor[j] >= adPcmFormat.nNumCoef)
										{
											throw new InvalidDataException("Invalid bPredictor");
										}

										blockData.CoefSet[j] = adPcmFormat.aCoeff[blockData.bPredictor[j]];
									}

									// the first two samples are already decoded
									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										writer.Write(blockData.iSamp2[j]);
									}

									for (int j = 0; j < adPcmFormat.Channels; j++)
									{
										writer.Write(blockData.iSamp1[j]);
									}

									byte nibbleByte = 0;
									bool nibbleFirst = true;

									for (int j = 0; j < adPcmFormat.nSamplesPerBlock - 2; j++)
									{
										for (int k = 0; k < adPcmFormat.Channels; k++)
										{
											int lPredSamp = (int)(blockData.iSamp1[k] * blockData.CoefSet[k].X + blockData.iSamp2[k] * blockData.CoefSet[k].Y) / 256;
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

									reader.BaseStream.Position += adPcmFormat.BlockAlign - (reader.BaseStream.Position - readerPrevPos);
								}
							}
						}
						chunks.Add(new DataChunk(dataBytes, format));
						break;
					case ChunkID.CUE:
						uint numCuePoints = reader.ReadUInt32(endianness);
						cuePoints = new CuePoint[numCuePoints];
						for (int i = 0; i < numCuePoints; i++)
						{
							cuePoints[i].ID = reader.ReadUInt32(endianness);
							cuePoints[i].Position = reader.ReadUInt32(endianness);
							cuePoints[i].ChunkID = (ChunkID)reader.ReadUInt32(endianness);
							cuePoints[i].ChunkStart = reader.ReadUInt32(endianness);
							cuePoints[i].BlockStart = reader.ReadUInt32(endianness);
							cuePoints[i].SampleStart = reader.ReadUInt32(endianness);
						}
						break;
					case ChunkID.SLNT:
						chunks.Add(new SilentChunk(reader.ReadUInt32(endianness), format));
						break;
					default:
						// unsupported and not interesting chunks
						reader.BaseStream.Position += ckSize;
						break;
				}

				// pad byte
				if ((ckSize & 1) == 1)
				{
					reader.BaseStream.Position++;
				}
			}

			// finalize
			if (chunks.Count == 0)
			{
				throw new InvalidDataException("File contains no DATA or SLNT chunks.");
			}

			if (format is WaveFormatMp3)
			{
				using (MemoryStream dataStream = new MemoryStream(dataBytes))
				{
					return Mp3LoadFromStream(dataStream);
				}
			}

			byte[][] buffers = new byte[format.Channels][];
			int startPosition = 0;

			int bytesPerSample = -1;

			for (int i = 0; i < chunks.Count; i++)
			{
				if (bytesPerSample == -1)
				{
					bytesPerSample = chunks[i].BytesPerSample;
				}
				else if (bytesPerSample != chunks[i].BytesPerSample)
				{
					throw new InvalidDataException("All chunks must have the same number of bytes per sample.");
				}
				for (int j = 0; j < buffers.Length; j++)
				{
					if (buffers[j] == null)
					{
						buffers[j] = new byte[chunks[i].Buffers[j].Length];
					}
					else
					{
						Array.Resize(ref buffers[j], buffers[j].Length + chunks[i].Buffers[j].Length);
					}
					Array.Copy(chunks[i].Buffers[j], 0, buffers[j], startPosition, chunks[i].Buffers[j].Length);
				}
				startPosition += buffers[0].Length;
			}

			return new Sound((int)format.SamplesPerSec, bytesPerSample * 8, buffers);
		}
	}
}
