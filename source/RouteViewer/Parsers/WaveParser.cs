#pragma warning disable 0660 // Defines == or != but does not override Object.Equals
#pragma warning disable 0661 // Defines == or != but does not override Object.GetHashCode

using System;
using System.IO;

namespace OpenBve {
	internal static class WaveParser {
		
		// --- structures and enumerations ---
		
		/// <summary>Represents the format of wave data.</summary>
		internal struct WaveFormat {
			// members
			/// <summary>The number of samples per second per channel.</summary>
			internal int SampleRate;
			/// <summary>The number of bits per sample.</summary>
			internal int BitsPerSample;
			/// <summary>The number of channels.</summary>
			internal int Channels;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="sampleRate">The number of samples per second per channel.</param>
			/// <param name="bitsPerSample">The number of bits per sample.</param>
			/// <param name="channels">The number of channels.</param>
			internal WaveFormat(int sampleRate, int bitsPerSample, int channels) {
				this.SampleRate = sampleRate;
				this.BitsPerSample = bitsPerSample;
				this.Channels = channels;
			}
			// operators
			public static bool operator ==(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return false;
				if (a.BitsPerSample != b.BitsPerSample) return false;
				if (a.Channels != b.Channels) return false;
				return true;
			}
			public static bool operator !=(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return true;
				if (a.BitsPerSample != b.BitsPerSample) return true;
				if (a.Channels != b.Channels) return true;
				return false;
			}
		}
		
		/// <summary>Represents wave data.</summary>
		internal class WaveData {
			// members
			/// <summary>The format of the wave data.</summary>
			internal WaveFormat Format;
			/// <summary>The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</summary>
			internal byte[] Bytes;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="format">The format of the wave data.</param>
			/// <param name="bytes">The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</param>
			internal WaveData(WaveFormat format, byte[] bytes) {
				this.Format = format;
				this.Bytes = bytes;
			}
		}
		
		/// <summary>Represents the endianness of an integer.</summary>
		private enum Endianness {
			/// <summary>Represents little endian byte order, i.e. least-significant byte first.</summary>
			Little = 0,
			/// <summary>Represents big endian byte order, i.e. most-significant byte first.</summary>
			Big = 1
		}
		
		
		// --- format-specific data ---
		
		/// <summary>Represents format-specific data.</summary>
		private abstract class FormatData {
			internal int BlockSize;
		}
		
		/// <summary>Represents PCM-specific data.</summary>
		private class PcmData : FormatData { }
		
		/// <summary>Represents Microsoft-ADPCM-specific data.</summary>
		private class MicrosoftAdPcmData : FormatData {
			// structures
			internal struct ChannelData {
				internal int bPredictor;
				internal short iDelta;
				internal short iSamp1;
				internal short iSamp2;
				internal int iCoef1;
				internal int iCoef2;
			}
			// members
			internal int SamplesPerBlock;
			internal short[][] Coefficients = null;
			// read-only fields
			internal static readonly short[] AdaptionTable = new short[] {
				230, 230, 230, 230, 307, 409, 512, 614,
				768, 614, 512, 409, 307, 230, 230, 230
			};
		}

		
		// --- functions ---
		
		/// <summary>Reads wave data from a RIFF/WAVE/PCM file.</summary>
		/// <param name="fileName">The file name of the RIFF/WAVE/PCM file.</param>
		/// <returns>The wave data.</returns>
		/// <remarks>Both RIFF and RIFX container formats are supported by this function.</remarks>
		internal static WaveData LoadFromFile(string fileName) {
			string fileTitle = Path.GetFileName(fileName);
			byte[] fileBytes = File.ReadAllBytes(fileName);
			using (MemoryStream stream = new MemoryStream(fileBytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// RIFF/RIFX chunk
					Endianness endianness;
					uint headerCkID = reader.ReadUInt32(); /* Chunk ID is character-based */
					if (headerCkID == 0x46464952) {
						endianness = Endianness.Little;
					} else if (headerCkID == 0x58464952) {
						endianness = Endianness.Big;
					} else {
						throw new InvalidDataException("Invalid chunk ID in " + fileTitle);
					}
					ReadUInt32(reader, endianness); //uint headerCkSize
					uint formType = ReadUInt32(reader, endianness);
					if (formType != 0x45564157) {
						throw new InvalidDataException("Unsupported format in " + fileTitle);
					}
					// data chunks
					WaveFormat format = new WaveFormat();
					FormatData data = null;
					byte[] dataBytes = null;
					while (stream.Position + 8 <= stream.Length) {
						uint ckID = reader.ReadUInt32(); /* Chunk ID is character-based */
						uint ckSize = ReadUInt32(reader, endianness);
						if (ckID == 0x20746d66) {
							// "fmt " chunk
							if (ckSize < 14) {
								throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
							}
							ushort wFormatTag = ReadUInt16(reader, endianness);
							ushort wChannels = ReadUInt16(reader, endianness);
							uint dwSamplesPerSec = ReadUInt32(reader, endianness);
							if (dwSamplesPerSec >= 0x80000000) {
								throw new InvalidDataException("Unsupported dwSamplesPerSec in " + fileTitle);
							}
							ReadUInt32(reader, endianness); // uint dwAvgBytesPerSec = 
							ushort wBlockAlign = ReadUInt16(reader, endianness);
							if (wFormatTag == 1) {
								// PCM
								if (ckSize < 16) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								stream.Position += ckSize - 16;
								if (wBitsPerSample < 1) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								if (wBlockAlign != ((wBitsPerSample + 7) / 8) * wChannels) {
									throw new InvalidDataException("Unexpected wBlockAlign in " + fileTitle);
								}
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = (int)wBitsPerSample;
								format.Channels = (int)wChannels;
								PcmData pcmData = new PcmData();
								pcmData.BlockSize = (int)wBlockAlign;
								data = pcmData;
							} else if (wFormatTag == 2) {
								// Microsoft ADPCM
								if (ckSize < 22) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								if (wBitsPerSample != 4) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								ReadUInt16(reader, endianness); // ushort cbSize = 
								MicrosoftAdPcmData adpcmData = new MicrosoftAdPcmData();
								adpcmData.SamplesPerBlock = ReadUInt16(reader, endianness);
								if (adpcmData.SamplesPerBlock == 0 | adpcmData.SamplesPerBlock > 2 * ((int)wBlockAlign - 6)) {
									throw new InvalidDataException("Unexpected nSamplesPerBlock in " + fileTitle);
								}
								ushort wNumCoef = ReadUInt16(reader, endianness);
								if (ckSize < 22 + 4 * wNumCoef) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								adpcmData.Coefficients = new short[wNumCoef][];
								for (int i = 0; i < wNumCoef; i++) {
									unchecked {
										adpcmData.Coefficients[i] = new short[] {
											(short)ReadUInt16(reader, endianness),
											(short)ReadUInt16(reader, endianness)
										};
									}
								}
								stream.Position += ckSize - (22 + 4 * wNumCoef);
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = 16;
								format.Channels = (int)wChannels;
								adpcmData.BlockSize = wBlockAlign;
								data = adpcmData;
							} else {
								// unsupported format
								throw new InvalidDataException("Unsupported wFormatTag in " + fileTitle);
							}
						} else if (ckID == 0x61746164) {
							// "data" chunk
							if (ckSize >= 0x80000000) {
								throw new InvalidDataException("Unsupported data chunk size in " + fileTitle);
							}
							if (data is PcmData) {
								// PCM
								int bytesPerSample = (format.BitsPerSample + 7) / 8;
								int samples = (int)ckSize / (format.Channels * bytesPerSample);
								int dataSize = samples * format.Channels * bytesPerSample;
								dataBytes = reader.ReadBytes(dataSize);
								stream.Position += ckSize - dataSize;
							} else if (data is MicrosoftAdPcmData) {
								// Microsoft ADPCM
								MicrosoftAdPcmData adpcmData = (MicrosoftAdPcmData)data;
								int blocks = (int)ckSize / adpcmData.BlockSize;
								dataBytes = new byte[2 * blocks * format.Channels * adpcmData.SamplesPerBlock];
								int position = 0;
								for (int i = 0; i < blocks; i++) {
									unchecked {
										MicrosoftAdPcmData.ChannelData[] channelData = new MicrosoftAdPcmData.ChannelData[format.Channels];
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].bPredictor = (int)reader.ReadByte();
											if (channelData[j].bPredictor >= adpcmData.Coefficients.Length) {
												throw new InvalidDataException("Invalid bPredictor in " + fileTitle);
											} else {
												channelData[j].iCoef1 = (int)adpcmData.Coefficients[channelData[j].bPredictor][0];
												channelData[j].iCoef2 = (int)adpcmData.Coefficients[channelData[j].bPredictor][1];
											}
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iDelta = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp1 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											channelData[j].iSamp2 = (short)ReadUInt16(reader, endianness);
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp2;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp2 >> 8);
											position += 2;
										}
										for (int j = 0; j < format.Channels; j++) {
											dataBytes[position] = (byte)(ushort)channelData[j].iSamp1;
											dataBytes[position + 1] = (byte)((ushort)channelData[j].iSamp1 >> 8);
											position += 2;
										}
										uint nibbleByte = 0;
										bool nibbleFirst = true;
										for (int j = 0; j < adpcmData.SamplesPerBlock - 2; j++) {
											for (int k = 0; k < format.Channels; k++) {
												int lPredSample =
													(int)channelData[k].iSamp1 * channelData[k].iCoef1 +
													(int)channelData[k].iSamp2 * channelData[k].iCoef2 >> 8;
												int iErrorDeltaUnsigned;
												if (nibbleFirst) {
													nibbleByte = (uint)reader.ReadByte();
													iErrorDeltaUnsigned = (int)(nibbleByte >> 4);
													nibbleFirst = false;
												} else {
													iErrorDeltaUnsigned = (int)(nibbleByte & 15);
													nibbleFirst = true;
												}
												int iErrorDeltaSigned =
													iErrorDeltaUnsigned >= 8 ? iErrorDeltaUnsigned - 16 : iErrorDeltaUnsigned;
												int lNewSampInt =
													lPredSample + (int)channelData[k].iDelta * iErrorDeltaSigned;
												short lNewSamp =
													lNewSampInt <= -32768 ? (short)-32768 :
													lNewSampInt >= 32767 ? (short)32767 :
													(short)lNewSampInt;
												channelData[k].iDelta = (short)(
													(int)channelData[k].iDelta *
													(int)MicrosoftAdPcmData.AdaptionTable[iErrorDeltaUnsigned] >> 8
												);
												if (channelData[k].iDelta < 16) {
													channelData[k].iDelta = 16;
												}
												channelData[k].iSamp2 = channelData[k].iSamp1;
												channelData[k].iSamp1 = lNewSamp;
												dataBytes[position] = (byte)(ushort)lNewSamp;
												dataBytes[position + 1] = (byte)((ushort)lNewSamp >> 8);
												position += 2;
											}
										}
									}
									stream.Position += adpcmData.BlockSize - (format.Channels * (adpcmData.SamplesPerBlock - 2) + 1 >> 1) - 7 * format.Channels;
								}
								stream.Position += (int)ckSize - blocks * adpcmData.BlockSize;
							} else {
								// invalid
								throw new InvalidDataException("No fmt chunk before the data chunk in " + fileTitle);
							}
						} else {
							// unsupported chunk
							stream.Position += (long)ckSize;
						}
						// pad byte
						if ((ckSize & 1) == 1) {
							stream.Position++;
						}
					}
					// finalize
					if (dataBytes == null) {
						throw new InvalidDataException("No data chunk before the end of the file in " + fileTitle);
					} else {
						return new WaveData(format, dataBytes);
					}
				}
			}
		}
		
		/// <summary>Reads a System.UInt32 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt32 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static uint ReadUInt32(BinaryReader reader, Endianness endianness) {
			uint value = reader.ReadUInt32();
			if (endianness == Endianness.Big) {
				unchecked {
					return (value << 24) | (value & ((uint)0xFF00 << 8)) | ((value & (uint)0xFF0000) >> 8) | (value >> 24);
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Reads a System.UInt16 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt16 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static ushort ReadUInt16(BinaryReader reader, Endianness endianness) {
			ushort value = reader.ReadUInt16();
			if (endianness == Endianness.Big) {
				unchecked {
					return (ushort)(((uint)value << 8) | ((uint)value >> 8));
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Converts the specified wave data to 8-bit or 16-bit mono.</summary>
		/// <param name="data">The original wave data.</param>
		/// <returns>The wave data converted to 8-bit or 16-bit mono.</returns>
		/// <remarks>If the bits per sample per channel are less than or equal to 8, the result will be 8-bit mono, otherwise 16-bit mono.</remarks>
		internal static WaveData ConvertToMono8Or16(WaveData data) {
			if ((data.Format.BitsPerSample == 8 | data.Format.BitsPerSample == 16) & data.Format.Channels == 1) {
				// already in target format
				return data;
			} else if (data.Format.Channels != 1) {
				// convert to mono first
				return ConvertToMono(data);
			} else if (data.Format.BitsPerSample < 8) {
				// less than 8 bits per sample
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 8, 1);
				return new WaveData(format, data.Bytes);
			} else if (data.Format.BitsPerSample < 16) {
				// between 9 and 15 bits per sample
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 16, 1);
				return new WaveData(format, data.Bytes);
			} else {
				// more than 16 bits per sample
				int bytesPerSample = data.Format.BitsPerSample + 7 >> 3;
				int samples = data.Bytes.Length / bytesPerSample;
				byte[] bytes = new byte[samples << 1];
				for (int i = 0; i < samples; i++) {
					int j = (i + 1) * bytesPerSample;
					bytes[2 * i] = data.Bytes[j - 2];
					bytes[2 * i + 1] = data.Bytes[j - 1];
				}
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 16, 1);
				return new WaveData(format, bytes);
			}
		}
		
		/// <summary>Converts the specified wave data to mono.</summary>
		/// <param name="data">The original wave data.</param>
		/// <returns>The wave data converted to mono.</returns>
		/// <remarks>This function will try to mix the channels, but will revert to a single channel if silence, constructive or destructive interference is detected, or if the number of bits per channel exceeds 48.</remarks>
		private static WaveData ConvertToMono(WaveData data) {
			if (data.Format.Channels == 1) {
				// is already mono
				return data;
			} else {
				// convert to mono
				int bytesPerSample = (data.Format.BitsPerSample + 7) / 8;
				int samples = data.Bytes.Length / (data.Format.Channels * bytesPerSample);
				/* 
				 * In order to detect for silence, constructive interference and
				 * destructive interference, compute the sums of the absolute values
				 * of the signed samples in each channel, considering the high byte only.
				 *  */
				long[] channelSum = new long[data.Format.Channels];
				int position = 0;
				for (int i = 0; i < samples; i++) {
					for (int j = 0; j < data.Format.Channels; j++) {
						int value;
						if (data.Format.BitsPerSample <= 8) {
							value = (int)data.Bytes[position + bytesPerSample - 1] - 128;
						} else {
							unchecked {
								value = (int)(sbyte)data.Bytes[position + bytesPerSample - 1];
							}
						}
						channelSum[j] += Math.Abs(value);
						position += bytesPerSample;
					}
				}
				/*
				 * Determine the highest of all channel sums.
				 * */
				long maximum = 0;
				for (int i = 0; i < data.Format.Channels; i++) {
					if (channelSum[i] > maximum) {
						maximum = channelSum[i];
					}
				}
				/*
				 * Isolate channels which are not silent. A channel is considered not silent
				 * if its sum is more than 0.39% of the highest sum found in all channels.
				 * */
				long silenceThreshold = maximum >> 8;
				int[] nonSilentChannels = new int[data.Format.Channels];
				int nonSilentChannelsCount = 0;
				for (int i = 0; i < data.Format.Channels; i++) {
					if (channelSum[i] >= silenceThreshold) {
						nonSilentChannels[nonSilentChannelsCount] = i;
						nonSilentChannelsCount++;
					}
				}
				/*
				 * If there is only one non-silent channel, use that channel.
				 * Otherwise, try to mix the non-silent channels.
				 * */
				if (nonSilentChannelsCount == 1 | bytesPerSample > 3) {
					/* Use the only non-silent channel. */
					byte[] bytes = new byte[samples * bytesPerSample];
					int channel = nonSilentChannels[0];
					int to = 0;
					int from = channel * bytesPerSample;
					for (int i = 0; i < samples; i++) {
						for (int j = 0; j < bytesPerSample; j++) {
							bytes[to] = data.Bytes[from + j];
							to++;
						}
						from += data.Format.Channels * bytesPerSample;
					}
					WaveFormat format = new WaveFormat(data.Format.SampleRate, data.Format.BitsPerSample, 1);
					return ConvertToMono8Or16(new WaveData(format, bytes));
				} else {
					/*
					 * Try mixing the non-silent channels. In order to detect for constructive
					 * or destructive interference, compute the sum of the absolute values
					 * of the signed samples in the mixed channel, considering the high
					 * byte only.
					 * */
					long mixedSum = 0;
					byte[] bytes = new byte[samples * bytesPerSample];
					int from = 0;
					int to = 0;
					long bytesFullRange = 1 << (8 * bytesPerSample);
					long bytesHalfRange = bytesFullRange >> 1;
					long bytesMinimum = -bytesHalfRange;
					long bytesMaximum = bytesHalfRange - 1;
					int mixedRightShift = 8 * (bytesPerSample - 1);
					for (int i = 0; i < samples; i++) {
						long mixed = 0;
						for (int j = 0; j < nonSilentChannelsCount; j++) {
							if (j == 0) {
								from += bytesPerSample * nonSilentChannels[0];
							} else {
								from += bytesPerSample * (nonSilentChannels[j] - nonSilentChannels[j - 1]);
							}
							if (bytesPerSample == 1) {
								long sample = (long)data.Bytes[from] - 0x80;
								mixed += sample;
							} else {
								ulong sampleUnsigned = 0;
								for (int k = 0; k < bytesPerSample; k++) {
									sampleUnsigned |= (ulong)data.Bytes[from + k] << (k << 3);
								}
								unchecked {
									long sampleSigned = (long)sampleUnsigned;
									if (sampleSigned >= bytesHalfRange) {
										sampleSigned -= bytesFullRange;
									}
									mixed += sampleSigned;
								}
							}
						}
						if (bytesPerSample == 1) {
							mixedSum += Math.Abs(mixed);
							unchecked {
								bytes[to] = (byte)(mixed + 0x80);
							}
						} else {
							mixedSum += Math.Abs(mixed >> mixedRightShift);
							if (mixed < bytesMinimum) {
								mixed = bytesMinimum;
							} else if (mixed > bytesMaximum) {
								mixed = bytesMaximum;
							}
							ulong mixedUnsigned;
							unchecked {
								if (mixed < 0) {
									mixed += bytesFullRange;
								}
								mixedUnsigned = (ulong)mixed;
								for (int k = 0; k < bytesPerSample; k++) {
									bytes[to + k] = (byte)mixedUnsigned;
									mixedUnsigned >>= 8;
								}
							}
						}
						from += bytesPerSample * (data.Format.Channels - nonSilentChannels[nonSilentChannelsCount - 1]);
						to += bytesPerSample;
					}
					/*
					 * Determine the lowest of all non-silent channel sums.
					 * */
					long minimum = long.MaxValue;
					for (int i = 0; i < nonSilentChannelsCount; i++) {
						if (channelSum[nonSilentChannels[i]] < minimum) {
							minimum = channelSum[nonSilentChannels[i]];
						}
					}
					/*
					 * Detect constructive and destructive interference. If an interference is
					 * detected, use the first non-silent channel, otherwise the mixed channel.
					 * */
					if (
						(double)mixedSum < 0.4 * (double)minimum ||
						(double)mixedSum > 1.1 * (double)maximum
					) {
						/* Interference detected. Use the first non-silent channel. */
						int channel = nonSilentChannels[0];
						from = channel * bytesPerSample;
						to = 0;
						for (int i = 0; i < samples; i++) {
							for (int j = 0; j < bytesPerSample; j++) {
								bytes[to] = data.Bytes[from + j];
								to++;
							}
							from += data.Format.Channels * bytesPerSample;
						}
						WaveFormat format = new WaveFormat(data.Format.SampleRate, data.Format.BitsPerSample, 1);
						return ConvertToMono8Or16(new WaveData(format, bytes));
					} else {
						/* No interference detected. Use the mixed channel. */
						WaveFormat format = new WaveFormat(data.Format.SampleRate, data.Format.BitsPerSample, 1);
						return ConvertToMono8Or16(new WaveData(format, bytes));
					}
				}
			}
		}
		
	}
}